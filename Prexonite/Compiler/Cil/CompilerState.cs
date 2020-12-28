// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Prexonite.Commands;
using Prexonite.Modular;
using Prexonite.Types;

#endregion

namespace Prexonite.Compiler.Cil
{
    public sealed class CompilerState : StackContext
    {
        public const int ParamArgsIndex = 2;
        public const int ParamResultIndex = 4;
        public const int ParamSctxIndex = 1;
        public const int ParamSharedVariablesIndex = 3;
        public const int ParamSourceIndex = 0;
        public const int ParamReturnModeIndex = 5;

        private string _effectiveArgumentsListId;

        /// <summary>
        ///     The name of the arguments list variable.
        /// </summary>
        public string EffectiveArgumentsListId =>
            _effectiveArgumentsListId ??= PFunction.ArgumentListId;

        public CompilerState
            (PFunction source, Engine targetEngine, ILGenerator il, CompilerPass pass,
                FunctionLinking linking)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Linking = linking;
            Pass = pass;
            TargetEngine = targetEngine ?? throw new ArgumentNullException(nameof(targetEngine));
            Il = il ?? throw new ArgumentNullException(nameof(il));
            IndexMap = new Dictionary<int, string>();
            InstructionLabels = new Label[Source.Code.Count + 1];
            for (var i = 0; i < InstructionLabels.Length; i++)
                InstructionLabels[i] = il.DefineLabel();
            ReturnLabel = il.DefineLabel();
            Symbols = new SymbolTable<CilSymbol>();
            TryBlocks = new Stack<CompiledTryCatchFinallyBlock>();

            _ForeachHints = new List<ForeachHint>();
            _CilExtensionOffsets = new Queue<int>();
            if (source.Meta.TryGetValue(Loader.CilHintsKey, out var cilHints))
            {
                SortedSet<int> cilExtensionOffsets = null;
                foreach (var entry in cilHints.List)
                {
                    var hint = entry.List;
                    if (hint.Length < 1)
                        continue;
                    switch (hint[0].Text)
                    {
                        case CilExtensionHint.Key:
                            if (cilExtensionOffsets == null)
                                cilExtensionOffsets = new SortedSet<int>();
                            var cilExt = CilExtensionHint.FromMetaEntry(hint);
                            foreach (var offset in cilExt.Offsets)
                                cilExtensionOffsets.Add(offset);
                            break;
                        case ForeachHint.Key:
                            _ForeachHints.Add(ForeachHint.FromMetaEntry(hint));
                            break;
                    }
                }
                if (cilExtensionOffsets != null)
                {
                    foreach (var offset in cilExtensionOffsets)
                        _CilExtensionOffsets.Enqueue(offset);
                }
            }

            Seh = new StructuredExceptionHandling(this);
            StackSize = new int[source.Code.Count];
        }

        #region Accessors

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Argc")]
        public LocalBuilder ArgcLocal { get; internal set; }

        public PFunction Source { get; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Argv")]
        public LocalBuilder ArgvLocal { get; internal set; }

        public ILGenerator Il { get; }

        /// <summary>
        ///     Maps from local variable indices to local variable phyical ids
        /// </summary>
        public Dictionary<int, string> IndexMap { get; }

        /// <summary>
        ///     <para>Maps from instruction addresses to the corresponding logical labels</para>
        ///     <para>Use these labels to jump to Prexonite Instructions.</para>
        /// </summary>
        public Label[] InstructionLabels { get; }

        /// <summary>
        ///     <para>The label that marks the exit of the function. Jump here to return.</para>
        /// </summary>
        public Label ReturnLabel { get; }

        /// <summary>
        ///     The local variable that holds the CIL stack context
        /// </summary>
        public LocalBuilder SctxLocal { get; internal set; }

        /// <summary>
        ///     <para>The local variable that holds arrays of shared variables immediately before closure instantiation</para>
        /// </summary>
        public LocalBuilder SharedLocal { get; internal set; }

        /// <summary>
        ///     CilSymbol table for the CIL compiler. See <see cref = "CilSymbol" /> for details.
        /// </summary>
        public SymbolTable<CilSymbol> Symbols { get; }

        /// <summary>
        ///     <para>Array of temporary variables. They are not preserved across Prexonite instructions. You are free to use them within <see
        ///      cref = "ICilCompilerAware.ImplementInCil" /> or <see cref = "ICilExtension.Implement" /></para>.
        /// </summary>
        public LocalBuilder[] TempLocals { get; internal set; }

        /// <summary>
        ///     The stack of try blocks currently in effect. The innermost try block is on top.
        /// </summary>
        public Stack<CompiledTryCatchFinallyBlock> TryBlocks { get; }

        /// <summary>
        ///     The engine in which the function is compiled to CIL. It can be assumed that engine configuration (such as command aliases) will not change anymore.
        /// </summary>
        public Engine TargetEngine { get; }

        /// <summary>
        ///     List of foreach CIL hints associated with this function.
        /// </summary>
        internal List<ForeachHint> _ForeachHints { get; }

        /// <summary>
        ///     List of addresses where valid CIL extension code begins.
        /// </summary>
        internal Queue<int> _CilExtensionOffsets { [DebuggerStepThrough] get; }

        private LocalBuilder _partialApplicationMapping;

        /// <summary>
        ///     <para>Local <code>System.Int32[]</code> variable. Used for temporarily holding arguments for partial application constructors.</para>
        ///     <para>Is not guaranteed to retain its value across instructions</para>
        /// </summary>
        public LocalBuilder PartialApplicationMappingLocal =>
            _partialApplicationMapping ??= Il.DeclareLocal(typeof (int[]));

        /// <summary>
        ///     Represents the engine this context is part of.
        /// </summary>
        public override Engine ParentEngine => TargetEngine;

        /// <summary>
        ///     The parent application.
        /// </summary>
        public override Application ParentApplication => Source.ParentApplication;

        /// <summary>
        ///     Collection of imported namespaces. Serves the same function as <see cref = "StackContext.ImportedNamespaces" />.
        /// </summary>
        public override SymbolCollection ImportedNamespaces => Source.ImportedNamespaces;

        /// <summary>
        ///     Indicates whether the context still has code/work to do.
        /// </summary>
        /// <returns>True if the context has additional work to perform in the next cycle, False if it has finished it's work and can be removed from the stack</returns>
        protected override bool PerformNextCycle(StackContext lastContext)
        {
            return false;
        }

        /// <summary>
        ///     Tries to handle the supplied exception.
        /// </summary>
        /// <param name = "exc">The exception to be handled.</param>
        /// <returns>True if the exception has been handled, false otherwise.</returns>
        public override bool TryHandleException(Exception exc)
        {
            return false;
        }

        /// <summary>
        ///     Represents the return value of the context.
        ///     Just providing a value here does not mean that it gets consumed by the caller.
        ///     If the context does not provide a return value, this property should return null (not NullPType).
        /// </summary>
        public override PValue ReturnValue => PType.Null;

        /// <summary>
        ///     Returns a reference to the current compiler pass.
        /// </summary>
        public CompilerPass Pass { get; }

        public FunctionLinking Linking { get; }

        public LocalBuilder PrimaryTempLocal => TempLocals[0];

        public StructuredExceptionHandling Seh { get; }

        public int[] StackSize { [DebuggerStepThrough] get; }

        #endregion

        #region Emit-helper methods

        /// <summary>
        ///     <para>Emits the shortest possible ldc.i4 opcode.</para>
        /// </summary>
        /// <param name = "i"></param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Ldc")]
        public void EmitLdcI4(int i)
        {
            switch (i)
            {
                case -1:
                    Il.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    Il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    Il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    Il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    Il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    Il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    Il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    Il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    Il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    Il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (i >= sbyte.MinValue && i <= sbyte.MaxValue)
                        Il.Emit(OpCodes.Ldc_I4_S, (sbyte) i);
                    else
                        Il.Emit(OpCodes.Ldc_I4, i);
                    break;
            }
        }

        /// <summary>
        ///     Shove arguments from the stack into the argument array (`argv`). This way the arguments can later be
        ///     passed to methods. Use <see cref = "ReadArgv" /> to load that array onto the stack.
        /// </summary>
        /// <param name = "argc">The number of arguments to load from the stack.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Argv")]
        public void FillArgv(int argc)
        {
            if (argc == 0)
            {
                //Nothing, argv is read from CilRuntime.EmptyPValueArrayField
            }
            else
            {
                //Instantiate array -> argv
                EmitLdcI4(argc);
                Il.Emit(OpCodes.Newarr, typeof (PValue));
                EmitStoreLocal(ArgvLocal);

                for (var i = argc - 1; i >= 0; i--)
                {
                    //get argument
                    EmitStoreTemp(0);

                    //store argument
                    EmitLoadLocal(ArgvLocal);
                    EmitLdcI4(i);
                    EmitLoadTemp(0);
                    Il.Emit(OpCodes.Stelem_Ref);
                }
            }
        }

        /// <summary>
        ///     Load previously perpared argument array (<see cref = "FillArgv" />) onto the stack.
        /// </summary>
        /// <param name = "argc">The number of elements in that argument array.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Argv")]
        public void ReadArgv(int argc)
        {
            if (argc == 0)
            {
                Il.Emit(OpCodes.Ldsfld, Runtime.EmptyPValueArrayField);
            }
            else
            {
                EmitLoadLocal(ArgvLocal);
            }
        }

        /// <summary>
        ///     Emits the shortest possible ldarg instruction.
        /// </summary>
        /// <param name = "index">The index of the argument to load.</param>
        public void EmitLoadArg(int index)
        {
            switch (index)
            {
                case 0:
                    Il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index < byte.MaxValue)
                        Il.Emit(OpCodes.Ldarg_S, (byte) index);
                    else
                        Il.Emit(OpCodes.Ldarg, index);
                    break;
            }
        }

        /// <summary>
        ///     Emits the shortest possible ldloc instruction for the supplied local variable.
        /// </summary>
        /// <param name = "local">The local variable to load.</param>
        public void EmitLoadLocal(LocalBuilder local)
        {
            EmitLoadLocal(local.LocalIndex);
        }

        /// <summary>
        ///     Emits the shortest possible ldloc instruction for the supplied local variable.
        /// </summary>
        /// <param name = "index">The index of the local variable to load.</param>
        public void EmitLoadLocal(int index)
        {
            switch (index)
            {
                case 0:
                    Il.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    Il.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    Il.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    Il.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    if (index < byte.MaxValue)
                        Il.Emit(OpCodes.Ldloc_S, (byte) index);
                    else
                        Il.Emit(OpCodes.Ldloc, index);
                    break;
            }
        }

        public void EmitStoreLocal(LocalBuilder local)
        {
            EmitStoreLocal(local.LocalIndex);
        }

        public void EmitStoreLocal(int index)
        {
            switch (index)
            {
                case 0:
                    Il.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    Il.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    Il.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    Il.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    if (index < byte.MaxValue)
                        Il.Emit(OpCodes.Stloc_S, (byte) index);
                    else
                        Il.Emit(OpCodes.Stloc, index);
                    break;
            }
        }

        public void EmitStoreTemp(int i)
        {
            if (i >= TempLocals.Length)
                throw new ArgumentOutOfRangeException
                    (
                    nameof(i), i,
                    "This particular cil implementation does not use that many temporary variables.");
            EmitStoreLocal(TempLocals[i]);
        }

        public void EmitLoadTemp(int i)
        {
            if (i >= TempLocals.Length)
                throw new ArgumentOutOfRangeException
                    (
                    nameof(i), i,
                    "This particular cil implementation does not use that many temporary variables.");
            EmitLoadLocal(TempLocals[i]);
        }

        /// <summary>
        /// Loads the value of a global variable from the current or any other module. Internal access is optimized.
        /// </summary>
        /// <param name="id">The name of the global variable to be loaded (an internal id)</param>
        /// <param name="moduleName">The name of the module that defines the global variable. May be null to indicate an internal variable.</param>
        public void EmitLoadGlobalValue(string id, ModuleName moduleName)
        {
            EmitLoadGlobalReference(id,moduleName);
            Il.EmitCall(OpCodes.Call, Compiler.GetValueMethod, null);
        }

        public void EmitLoadGlobalRefAsPValue(EntityRef.Variable.Global globalVariable)
        {
            EmitLoadGlobalRefAsPValue(globalVariable.Id,globalVariable.ModuleName);
        }

        /// <summary>
        /// Loads the <see cref="PVariable"/> object for the specified global variable onto the managed stack.
        /// </summary>
        /// <param name="id">The internal id of the global variable.</param>
        /// <param name="moduleName">The module name containing the global variable definition. May be null to indicate an internal global variable.</param>
        public void EmitLoadGlobalReference(string id, ModuleName moduleName)
        {
            EmitLoadLocal(SctxLocal.LocalIndex);
            Il.Emit(OpCodes.Ldstr, id);
            if (moduleName == null || Equals(moduleName, Source.ParentApplication.Module.Name))
            {
                EmitCall(Runtime.LoadGlobalVariableReferenceInternalMethod);
            }
            else
            {
                EmitModuleName(moduleName);
                EmitCall(Runtime.LoadGlobalVariableReferenceMethod);
            }
        }

        public void EmitIndirectCall(int argc, bool justEffect)
        {
            EmitLoadLocal(SctxLocal);
            ReadArgv(argc);
            Il.EmitCall(OpCodes.Call, Compiler.PVIndirectCallMethod, null);
            if (justEffect)
                Il.Emit(OpCodes.Pop);
        }

        /// <summary>
        ///     <para>Write the value produced by <paramref name = "action" /> into the local variable behind <paramref name = "sym" />.</para>
        ///     <para>Warning: Action must work with an empty stack</para>
        /// </summary>
        /// <param name = "sym">The local variable to write to.</param>
        /// <param name = "action">The action that produces the value.</param>
        public void EmitStorePValue(CilSymbol sym, Action action)
        {
            if (sym.Kind == SymbolKind.Local)
            {
                action();
                EmitStoreLocal(sym.Local);
            }
            else if (sym.Kind == SymbolKind.LocalRef)
            {
                EmitLoadLocal(sym.Local);
                action();
                Il.EmitCall(OpCodes.Call, Compiler.SetValueMethod, null);
            }
            else
            {
                throw new PrexoniteException("Cannot emit code for CilSymbol");
            }
        }

        /// <summary>
        ///     <para>Load a value from the specified local variable.</para>
        /// </summary>
        /// <param name = "sym">The variable to load.</param>
        public void EmitLoadPValue(CilSymbol sym)
        {
            if (sym.Kind == SymbolKind.Local)
            {
                EmitLoadLocal(sym.Local);
            }
            else if (sym.Kind == SymbolKind.LocalRef)
            {
                EmitLoadLocal(sym.Local);
                Il.EmitCall(OpCodes.Call, Compiler.GetValueMethod, null);
            }
            else
            {
                throw new PrexoniteException("Cannot emit code for CilSymbol");
            }
        }

        /// <summary>
        ///     Creates a PValue null value.
        /// </summary>
        public void EmitLoadNullAsPValue()
        {
            Il.EmitCall(OpCodes.Call, Compiler.getPTypeNull, null);
            Il.EmitCall(OpCodes.Call, Compiler.nullCreatePValue, null);
        }

        public void MarkInstruction(int instructionAddress)
        {
            Il.MarkLabel(InstructionLabels[instructionAddress]);
        }

        public void EmitCall(MethodInfo method)
        {
            Il.EmitCall(OpCodes.Call, method, null);
        }

        public void EmitVirtualCall(MethodInfo method)
        {
            Il.EmitCall(OpCodes.Callvirt, method, null);
        }

        public void EmitWrapString()
        {
            Il.EmitCall(OpCodes.Call, Compiler.GetStringPType, null);
            Il.Emit(OpCodes.Newobj, Compiler.NewPValue);
        }

        public void EmitWrapBool()
        {
            Il.Emit(OpCodes.Box, typeof (bool));
            Il.EmitCall(OpCodes.Call, Compiler.GetBoolPType, null);
            Il.Emit(OpCodes.Newobj, Compiler.NewPValue);
        }

        public void EmitWrapReal()
        {
            Il.Emit(OpCodes.Box, typeof (double));
            Il.EmitCall(OpCodes.Call, Compiler.GetRealPType, null);
            Il.Emit(OpCodes.Newobj, Compiler.NewPValue);
        }

        public void EmitWrapInt()
        {
            Il.Emit(OpCodes.Box, typeof (int));
            Il.EmitCall(OpCodes.Call, Compiler.GetIntPType, null);
            Il.Emit(OpCodes.Newobj, Compiler.NewPValue);
        }

        public void EmitWrapChar()
        {
            Il.Emit(OpCodes.Box, typeof (char));
            Il.EmitCall(OpCodes.Call, Compiler.GetCharPType, null);
            Il.Emit(OpCodes.Newobj, Compiler.NewPValue);
        }

        public void EmitLoadType(string typeExpr)
        {
            var T = ConstructPType(typeExpr);
            var cilT = T as ICilCompilerAware;

            var virtualInstruction = new Instruction(OpCode.cast_const, typeExpr);
            var cf = cilT?.CheckQualification(virtualInstruction) ?? CompilationFlags.IsCompatible;

            if ((cf & CompilationFlags.HasCustomImplementation) ==
                CompilationFlags.HasCustomImplementation &&
                    cilT != null)
            {
                cilT.ImplementInCil(this, virtualInstruction);
            }
            else
            {
                EmitLoadLocal(SctxLocal);
                Il.Emit(OpCodes.Ldstr, typeExpr);
                EmitCall(Runtime.ConstructPTypeMethod);
            }
        }

        public void EmitNewObj(string typeExpr, int argc)
        {
            FillArgv(argc);
            EmitLoadType(typeExpr);
            EmitLoadLocal(SctxLocal);
            ReadArgv(argc);
            EmitVirtualCall(_pTypeConstructMethod);
        }

        private static readonly MethodInfo _pTypeConstructMethod =
            typeof (PType).GetMethod("Construct", new[] {typeof (StackContext), typeof (PValue[])});

        public void EmitLoadClrType(Type T)
        {
            Il.Emit(OpCodes.Ldtoken, T);
            EmitCall(_typeGetTypeFromHandle);
        }

        private static readonly MethodInfo _typeGetTypeFromHandle =
            typeof (Type).GetMethod("GetTypeFromHandle", new[] {typeof (RuntimeTypeHandle)});

        #region Early bound command call

        /// <summary>
        ///     Emits a call to the static method "RunStatically(StackContext sctx, PValue[] args)" of the supplied type.
        /// </summary>
        /// <param name = "target">The type, that declares the RunStatically to call.</param>
        /// <param name = "ins">The call to the command for which code is to be emitted.</param>
        public void EmitEarlyBoundCommandCall(Type target, Instruction ins)
        {
            EmitEarlyBoundCommandCall(target, ins.Arguments, ins.JustEffect);
        }

        /// <summary>
        ///     Emits a call to the static method "RunStatically(StackContext sctx, PValue[] args)" of the supplied type.
        /// </summary>
        /// <param name = "target">The type, that declares the RunStatically to call.</param>
        /// <param name = "argc">The number of arguments to pass to the command.</param>
        public void EmitEarlyBoundCommandCall(Type target, int argc)
        {
            EmitEarlyBoundCommandCall(target, argc, false);
        }

        /// <summary>
        ///     Emits a call to the static method "RunStatically(StackContext sctx, PValue[] args)" of the supplied type.
        /// </summary>
        /// <param name = "target">The type, that declares the RunStatically to call.</param>
        /// <param name = "argc">The number of arguments to pass to the command.</param>
        /// <param name = "justEffect">Indicates whether or not to ignore the return value.</param>
        public void EmitEarlyBoundCommandCall(Type target, int argc, bool justEffect)
        {
            var run =
                target.GetMethod("RunStatically", new[] {typeof (StackContext), typeof (PValue[])});

            if (run == null)
                throw new PrexoniteException
                    (
                    $"{target} does not provide a static method RunStatically(StackContext, PValue[])");
            if (run.ReturnType != typeof (PValue))
                throw new PrexoniteException
                    (
                    $"{target}'s RunStatically method does not return PValue but {run.ReturnType}.");
            FillArgv(argc);

            EmitLoadLocal(SctxLocal);
            ReadArgv(argc);
            Il.EmitCall(OpCodes.Call, run, null);
            if (justEffect)
                Il.Emit(OpCodes.Pop);
        }

        #endregion

        #endregion

        /// <summary>
        ///     Pops the specified number of arguments off the stack.
        /// </summary>
        /// <param name = "argc">The number of arguments to pop off the stack.</param>
        public void EmitIgnoreArguments(int argc)
        {
            for (var i = 0; i < argc; i++)
                Il.Emit(OpCodes.Pop);
        }

        public void EmitPTypeAsPValue(string expr)
        {
            EmitLoadLocal(SctxLocal);
            Il.Emit(OpCodes.Ldstr, expr);
            Il.EmitCall(OpCodes.Call, Runtime.ConstructPTypeAsPValueMethod, null);
        }

        public bool TryGetStaticallyLinkedFunction(string id, out MethodInfo targetMethod)
        {
            targetMethod = null;
            return (Linking & FunctionLinking.Static) == FunctionLinking.Static &&
                Pass.Implementations.TryGetValue(id, out targetMethod);
        }

        public void EmitCommandCall(Instruction ins)
        {
            var argc = ins.Arguments;
            var id = ins.Id;
            var justEffect = ins.JustEffect;
            ICilCompilerAware aware = null;
            CompilationFlags flags;
            if (
                TargetEngine.Commands.TryGetValue(id, out var cmd) &&
                    (aware = cmd as ICilCompilerAware) != null)
                flags = aware.CheckQualification(ins);
            else
                flags = CompilationFlags.IsCompatible;

            if (
                (
                    (flags & CompilationFlags.PrefersCustomImplementation) ==
                        CompilationFlags.PrefersCustomImplementation ||
                            (flags & CompilationFlags.RequiresCustomImplementation)
                                == CompilationFlags.RequiresCustomImplementation
                    ) && aware != null)
            {
                //Let the command handle the call
                aware.ImplementInCil(this, ins);
            }
            else if ((flags & CompilationFlags.PrefersRunStatically)
                == CompilationFlags.PrefersRunStatically)
            {
                //Emit a static call to $commandType$.RunStatically
                EmitEarlyBoundCommandCall(cmd.GetType(), ins);
            }
            else
            {
                //Implement via Runtime.CallCommand (call by name)
                FillArgv(argc);
                EmitLoadLocal(SctxLocal);
                ReadArgv(argc);
                Il.Emit(OpCodes.Ldstr, id);
                Il.EmitCall(OpCodes.Call, Runtime.CallCommandMethod, null);
                if (justEffect)
                    Il.Emit(OpCodes.Pop);
            }
        }

        public void EmitFuncCall(int argc, string internalId, ModuleName moduleName, bool justEffect)
        {
            if (internalId == null)
                throw new ArgumentNullException(nameof(internalId));

            var isInternal = moduleName == null ||
                             Equals(moduleName, Source.ParentApplication.Module.Name);

            if (isInternal && TryGetStaticallyLinkedFunction(internalId, out var staticTargetMethod))
            {
                //Link function statically
                FillArgv(argc);
                Il.Emit(OpCodes.Ldsfld, Pass.FunctionFields[internalId]);
                EmitLoadLocal(SctxLocal);
                ReadArgv(argc);
                Il.Emit(OpCodes.Ldnull);
                Il.Emit(OpCodes.Ldloca_S, TempLocals[0]);
                EmitLoadArg(ParamReturnModeIndex);
                EmitCall(staticTargetMethod);
                if (!justEffect)
                    EmitLoadTemp(0);
            }
            else if (isInternal)
            {
                //Link function dynamically
                FillArgv(argc);
                EmitLoadLocal(SctxLocal);
                ReadArgv(argc);
                Il.Emit(OpCodes.Ldstr, internalId);
                Il.EmitCall(OpCodes.Call, Runtime.CallInternalFunctionMethod, null);
                if (justEffect)
                    Il.Emit(OpCodes.Pop);
            }
            //TODO (Ticket #107) bind cross-module calls statically
            else
            {
                //Cross-Module-Link function dynamically
                FillArgv(argc);
                EmitLoadLocal(SctxLocal);
                ReadArgv(argc);
                Il.Emit(OpCodes.Ldstr, internalId);
                EmitModuleName(moduleName);
                Il.EmitCall(OpCodes.Call, Runtime.CallFunctionMethod, null);
                if (justEffect)
                    Il.Emit(OpCodes.Pop);
            }
        }

        /// <summary>
        /// Creates a new closure of the specified function. Needs to have the StackContext and the array of shared variables on the managed stack.
        /// </summary>
        /// <param name="internalId">The internal id of the function to create a closure for.</param>
        /// <param name="moduleName">If the function comes from another module, the module name is passed here.</param>
        public void EmitNewClo(string internalId, ModuleName moduleName)
        {
            if (internalId == null)
                throw new ArgumentNullException(nameof(internalId));

            var isInternal = moduleName == null ||
                             Equals(moduleName, Source.ParentApplication.Module.Name);

            MethodInfo runtimeMethod;
            if(isInternal && TryGetStaticallyLinkedFunction(internalId, out _))
            {
                Il.Emit(OpCodes.Ldsfld, Pass.FunctionFields[internalId]);
                runtimeMethod = Runtime.NewClosureMethodStaticallyBound;
            }
            else if(isInternal)
            {
                Il.Emit(OpCodes.Ldstr,internalId);
                runtimeMethod = Runtime.NewClosureMethodLateBound;
            }
            //TODO (Ticket #107) bind cross-module calls statically
            else
            {
                Il.Emit(OpCodes.Ldstr,internalId);
                EmitModuleName(moduleName);
                runtimeMethod = Runtime.NewClosureMethodCrossModule;
            }

            EmitCall(runtimeMethod);
        }

        public void EmitLoadEngRefAsPValue()
        {
            EmitLoadLocal(SctxLocal);
            Il.EmitCall(OpCodes.Call, Runtime.LoadEngineReferenceMethod, null);
        }

        public static void EmitLoadAppRefAsPValue(CompilerState state)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.EmitCall
                (
                    OpCodes.Call, Runtime.LoadApplicationReferenceMethod, null);
        }

        public void EmitLoadCmdRefAsPValue(string id)
        {
            EmitLoadLocal(SctxLocal);
            Il.Emit(OpCodes.Ldstr, id);
            Il.EmitCall(OpCodes.Call, Runtime.LoadCommandReferenceMethod, null);
        }

        public void EmitLoadFuncRefAsPValue(string internalId, ModuleName moduleName)
        {
            EmitLoadLocal(SctxLocal);
            var isInternal = moduleName == null ||
                Equals(moduleName, Source.ParentApplication.Module.Name);

            if (!isInternal && TryGetStaticallyLinkedFunction(internalId, out var dummyMethodInfo))
            {
                Il.Emit(OpCodes.Ldsfld, Pass.FunctionFields[internalId]);
                EmitVirtualCall(Compiler.CreateNativePValue);
            }
            //TODO (Ticket #107) Statically linked Cross-Module ldr.func
            else  if(isInternal)
            {
                Il.Emit(OpCodes.Ldstr, internalId);
                EmitCall(Runtime.LoadFunctionReferenceInternalMethod);
            }
            else
            {
                //Cross-module reference, dynamically linked
                Il.Emit(OpCodes.Ldstr,internalId);
                EmitModuleName(moduleName);
                EmitCall(Runtime.LoadFunctionReferenceMethod);
            }
        }

        public void EmitLoadGlobalRefAsPValue(string id, ModuleName moduleName)
        {
            EmitLoadLocal(SctxLocal);
            Il.Emit(OpCodes.Ldstr, id);
            if(moduleName == null || Equals(moduleName,Source.ParentApplication.Module.Name))
            {
                EmitCall(Runtime.LoadGlobalReferenceAsPValueInternalMethod);
            }
            else
            {
                EmitModuleName(moduleName);
                EmitCall(Runtime.LoadGlobalVariableReferenceAsPValueMethod);
            }
        }

        public void EmitLoadLocalRefAsPValue(string id)
        {
            EmitLoadLocal(Symbols[id].Local);
            Il.EmitCall(OpCodes.Call, Runtime.WrapPVariableMethod, null);
        }

        public void EmitLoadLocalRefAsPValue(EntityRef.Variable.Local localVariable)
        {
            EmitLoadLocalRefAsPValue(localVariable.Id);
        }

        public void EmitLoadStringAsPValue(string id)
        {
            Il.Emit(OpCodes.Ldstr, id);
            EmitWrapString();
        }

        public void EmitLoadBoolAsPValue(bool value)
        {
            EmitLdcI4(value ? 1 : 0);
            EmitWrapBool();
        }

        public void EmitLoadRealAsPValue(double value)
        {
            Il.Emit(OpCodes.Ldc_R8, value);
            EmitWrapReal();
        }

        public void EmitLoadRealAsPValue(Instruction ins)
        {
            EmitLoadRealAsPValue((double) ins.GenericArgument);
        }

        public void EmitLoadIntAsPValue(int argc)
        {
            EmitLdcI4(argc);
            EmitWrapInt();
        }

        internal void _EmitAssignReturnMode(ReturnMode returnMode)
        {
            EmitLoadArg(ParamReturnModeIndex);
            EmitLdcI4((int) returnMode);
            Il.Emit(OpCodes.Stind_I4);
        }

        public void EmitSetReturnValue()
        {
            EmitStoreLocal(PrimaryTempLocal);
            EmitLoadArg(ParamResultIndex);
            EmitLoadLocal(PrimaryTempLocal);
            Il.Emit(OpCodes.Stind_Ref);
        }

        private static readonly Lazy<ConstructorInfo[]> _versionCtors = new(() =>
            {
                var cs = new ConstructorInfo[3];
                cs[0] = 
                    typeof (Version).GetConstructor(new[] 
                        {typeof (int), typeof (int)});
                cs[1] =
                    typeof (Version).GetConstructor(new[] 
                        {typeof (int), typeof (int), typeof (int)});
                cs[2] =
                    typeof (Version).GetConstructor(new[]
                        {typeof (int), typeof (int), typeof (int), typeof (int)});
                return cs;
            },LazyThreadSafetyMode.None);

        public void EmitVersion(Version version)
        {
            EmitLdcI4(version.Major);
            EmitLdcI4(version.Minor);
            //major.minor.build.revision
            var offset =
                version.Revision >= 0
                    ? 2
                    : version.Build >= 0
                        ? 1
                        : 0;
            Il.Emit(OpCodes.Newobj, _versionCtors.Value[offset]);
        }

        public void EmitModuleNameAsPValue(ModuleName moduleName)
        {
            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));
            EmitLoadLocal(SctxLocal);
            Il.Emit(OpCodes.Ldstr, moduleName.Id);
            EmitVersion(moduleName.Version);
            EmitCall(Runtime.LoadModuleNameAsPValueMethod);
        }

        public void EmitModuleName(ModuleName moduleName)
        {
            if (moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));
            EmitLoadLocal(SctxLocal);
            Il.Emit(OpCodes.Ldstr, moduleName.Id);
            EmitVersion(moduleName.Version);
            EmitCall(Runtime.LoadModuleNameMethod); 
        }

        public void EmitLoadFuncRefAsPValue(EntityRef.Function function)
        {
            EmitLoadFuncRefAsPValue(function.Id,function.ModuleName);
        }

        public void EmitLoadCmdRefAsPValue(EntityRef.Command command)
        {
            EmitLoadCmdRefAsPValue(command.Id);
        }
    }
}