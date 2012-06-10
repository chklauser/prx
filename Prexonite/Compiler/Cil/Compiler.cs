// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Commands;
using Prexonite.Modular;
using Prexonite.Types;
using CilException = Prexonite.PrexoniteException;

#endregion

namespace Prexonite.Compiler.Cil
{
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public static class Compiler
    {
        #region Public interface and LCG Setup

        public static void Compile(Application app, Engine targetEngine)
        {
            Compile(app, targetEngine, FunctionLinking.FullyStatic);
        }

        public static void Compile(Application app, Engine targetEngine, FunctionLinking linking)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            Compile(app.Functions, targetEngine, linking);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, Application app)
        {
            Compile(sctx, app, FunctionLinking.FullyStatic);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, Application app, FunctionLinking linking)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            Compile(app, sctx.ParentEngine, linking);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, List<PValue> lst)
        {
            Compile(sctx, lst, FunctionLinking.FullyStatic);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, List<PValue> lst, bool fullyStatic)
        {
            Compile(sctx, lst,
                fullyStatic ? FunctionLinking.FullyStatic : FunctionLinking.FullyIsolated);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, List<PValue> lst, FunctionLinking linking)
        {
            if (lst == null)
                throw new ArgumentNullException("lst");
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            var functions = new List<PFunction>();
            foreach (var value in lst)
            {
                if (value == null)
                    continue;
                var T = value.Type.ToBuiltIn();
                PFunction func;
                switch (T)
                {
                    case PType.BuiltIn.String:
                        if (
                            !sctx.ParentApplication.Functions.TryGetValue((string) value.Value,
                                out func))
                            continue;
                        break;
                    case PType.BuiltIn.Object:
                        if (!value.TryConvertTo(sctx, false, out func))
                            continue;
                        break;
                    default:
                        continue;
                }
                functions.Add(func);
            }

            Compile(functions, sctx.ParentEngine, linking);
        }

        public static void Compile(IEnumerable<PFunction> functions, Engine targetEngine)
        {
            Compile(functions, targetEngine, FunctionLinking.FullyStatic);
        }

        public static void Compile(IEnumerable<PFunction> functions, Engine targetEngine,
            FunctionLinking linking)
        {
            _checkQualification(functions, targetEngine);

            var qfuncs = new List<PFunction>();

            //Get a list of qualifying functions
            foreach (var func in functions)
                if (!func.Meta.GetDefault(PFunction.VolatileKey, false))
                    qfuncs.Add(func);

            if (qfuncs.Count == 0)
                return; //No compilation to be done

            var pass = new CompilerPass(linking);

            //Generate method stubs
            foreach (var func in qfuncs)
                pass.DefineImplementationMethod(func.Id);

            //Emit IL
            foreach (var func in qfuncs)
            {
                _compile(func, CompilerPass.GetIlGenerator(pass.Implementations[func.Id]),
                    targetEngine, pass, linking);
            }

            //Enable by name linking and link meta data to CIL implementations
            foreach (var func in qfuncs)
            {
                func.Declaration.CilImplementation = pass.GetDelegate(func.Id);
                pass.LinkMetadata(func);
            }
        }

        public static bool TryCompile(PFunction func, Engine targetEngine)
        {
            return TryCompile(func, targetEngine, FunctionLinking.FullyStatic);
        }

        public static bool TryCompile(PFunction func, Engine targetEngine, FunctionLinking linking)
        {
            if (_checkQualification(func, targetEngine))
            {
                var pass = new CompilerPass(func.ParentApplication, linking);

                var m = pass.DefineImplementationMethod(func.Id);
                var il = CompilerPass.GetIlGenerator(m);

                _compile(func, il, targetEngine, pass, linking);

                func.Declaration.CilImplementation = pass.GetDelegate(m);
                pass.LinkMetadata(func);

                return true;
            }
            return false;
        }

        #endregion

        #region Store debug implementation

        public static void StoreDebugImplementation(StackContext sctx)
        {
            StoreDebugImplementation(sctx.ParentApplication, sctx.ParentEngine);
        }

        public static void StoreDebugImplementation(StackContext sctx, Application app)
        {
            StoreDebugImplementation(app, sctx.ParentEngine);
        }

        public static void StoreDebugImplementation(Application app, Engine targetEngine)
        {
            _checkQualification(app.Functions, targetEngine);

            const FunctionLinking linking = FunctionLinking.FullyStatic;
            var pass = new CompilerPass(linking);

            var qfuncs = new List<PFunction>();
            foreach (var func in app.Functions)
            {
                if (!func.Meta.GetDefault(PFunction.VolatileKey, false))
                {
                    qfuncs.Add(func);
                    pass.DefineImplementationMethod(func.Id);
                }
            }

            foreach (var func in qfuncs)
            {
                _compile(func, pass.GetIlGenerator(func.Id), targetEngine, pass, linking);
            }

            pass.TargetType.CreateType();

            pass.Assembly.Save(pass.Assembly.GetName().Name + ".dll");
        }

        public static void StoreDebugImplementation(PFunction func, Engine targetEngine)
        {
            const FunctionLinking linking = FunctionLinking.FullyStatic;
            var pass = new CompilerPass(linking);

            var m = pass.DefineImplementationMethod(func.Id);

            var il = CompilerPass.GetIlGenerator(m);

            _compile(func, il, targetEngine, pass, linking);

            pass.TargetType.CreateType();

            //var sm = tb.DefineMethod("whoop", MethodAttributes.Static | MethodAttributes.Public);

            //ab.SetEntryPoint(sm);
            pass.Assembly.Save(pass.Assembly.GetName().Name + ".dll");
        }

        public static void StoreDebugImplementation(StackContext sctx, PFunction func)
        {
            StoreDebugImplementation(func, sctx.ParentEngine);
        }

        #endregion

        #region Check Qualification

        private static bool _checkQualification(PFunction source, Engine targetEngine)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            lock (source)
            {
                string reason;
                var qualifies = _check(source, targetEngine, out reason);
                _registerCheckResults(source, qualifies, reason);
                return qualifies;
            }
        }

        private static void _registerCheckResults(IHasMetaTable source, bool qualifies,
            string reason)
        {
            if (!qualifies && source.Meta[PFunction.DeficiencyKey].Text == "" && reason != null)
            {
                source.Meta[PFunction.DeficiencyKey] = reason;
            } //else nothing
            if ((!qualifies) || source.Meta.ContainsKey(PFunction.VolatileKey))
            {
                source.Meta[PFunction.VolatileKey] = !qualifies;
            }
        }

        private static void _checkQualification(IEnumerable<PFunction> functions,
            Engine targetEngine)
        {
            //Handle dynamic functions (functions that operate on their caller)
            foreach (var func in functions)
            {
                if (func.Meta[PFunction.VolatileKey].Switch ||
                    func.Meta.ContainsKey(PFunction.DynamicKey))
                    continue;
            }

            //Check qualifications (whether a function can be compiled by the CIL compiler)
            foreach (var func in functions)
            {
                string reason;
                var qualifies = _check(func, targetEngine, out reason);
                _registerCheckResults(func, qualifies, reason);
            }
        }

        private static bool _rangeInSet(int offset, int count, HashSet<int> hashSet)
        {
            for (var i = offset; i < offset + count; i++)
                if (hashSet.Contains(i))
                    return true;
            return false;
        }

        private static bool _check(PFunction source, Engine targetEngine, out string reason)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (targetEngine == null)
                throw new ArgumentNullException("targetEngine");
            //Application does not allow cil compilation
            if ((!source.Meta.ContainsKey(PFunction.VolatileKey)) &&
                source.ParentApplication.Meta[PFunction.VolatileKey].Switch)
            {
                reason = "Application does not allow cil compilation";
                return false;
            }
            //Function does not allow cil compilation
            if (source.Meta[PFunction.VolatileKey].Switch)
            {
                reason = null; //don't add a message
                return false;
            }

            //Prepare for CIL extensions
            var cilExtensions = new List<int>();
            var localVariableMapping = new Dictionary<int, string>(source.LocalVariableMapping.Count);
            foreach (var kvp in source.LocalVariableMapping)
                localVariableMapping[kvp.Value] = kvp.Key;

            var jumpTargets = new HashSet<int>(from ins in source.Code
                                               where ins.OpCode == OpCode.jump
                                                   || ins.OpCode == OpCode.jump_t
                                                       || ins.OpCode == OpCode.jump_f
                                               select ins.Arguments);

            var seh = new StructuredExceptionHandling(source);

            //Check for not supported instructions and instructions used in a way
            //  that is not supported by the CIL compiler)
            for (var insOffset = 0; insOffset < source.Code.Count; insOffset++)
            {
                var address = insOffset;
                var ins = source.Code[address];
                switch (ins.OpCode)
                {
                    case OpCode.cmd:
                        //Check for commands that are not compatible.
                        ICommandInfo cmd;
                        if (!targetEngine.Commands.TryGetInfo(ins.Id, out cmd))
                        {
                            reason = "Cannot find information about command " + ins.Id;
                            return false;
                        }

                        ICilExtension extension;
                        ICilCompilerAware aware;
                        CompileTimeValue[] staticArgv;

                        //First allow CIL extensions to kick in, and only if they don't apply, check for CIL awareness.
                        if (cmd.TryGetCilExtension(out extension)
                            &&
                            !_rangeInSet(
                                insOffset -
                                    (staticArgv =
                                        CompileTimeValue.ParseSequenceReverse(source.Code,
                                            localVariableMapping, address - 1, source.ParentApplication.Module.Cache,source.ParentApplication.Module.Name)).Length + 1,
                                staticArgv.Length, jumpTargets)
                                    &&
                                    extension.ValidateArguments(staticArgv,
                                        ins.Arguments - staticArgv.Length))
                        {
                            cilExtensions.Add(address - staticArgv.Length);
                        }
                        else if (cmd.TryGetCilCompilerAware(out aware))
                        {
                            var flags = aware.CheckQualification(ins);
                            if (flags == CompilationFlags.IsIncompatible)
                                //Incompatible and no workaround
                            {
                                reason = "Incompatible command " + ins.Id;
                                return false;
                            }
                        }
                        break;
                    case OpCode.func:
                        //Check for functions that use dynamic features
                        PFunction func;
                        if (source.ParentApplication.Functions.TryGetValue(ins.Id, out func) &&
                            func.Meta[PFunction.DynamicKey].Switch)
                        {
                            reason = "Uses dynamic function " + ins.Id;
                            return false;
                        }
                        break;
                    case OpCode.tail:
                    case OpCode.invalid:
                        reason = "Unsupported instruction " + ins;
                        return false;
                    case OpCode.newclo:
                        //Function must already be available
                        if (!source.ParentApplication.Functions.Contains(ins.Id))
                        {
                            reason = "Enclosed function " + ins.Id +
                                " must already be compiled (closure creation)";
                            return false;
                        }
                        break;
                    case OpCode.@try:
                        //must be the first instruction of a try block
                        var isCorrect =
                            source.TryCatchFinallyBlocks.Any(block => block.BeginTry == address);
                        if (!isCorrect)
                        {
                            reason =
                                "try instruction is not the first instruction of a guarded block.";
                            return false;
                        }
                        break;
                    case OpCode.exc:
                        //must be the first instruction of a catch block
                        isCorrect =
                            source.TryCatchFinallyBlocks.Any(block => block.BeginCatch == address);
                        if (!isCorrect)
                        {
                            reason =
                                "exc instruction is not the first instruction of a catch clause.";
                            return false;
                        }
                        break;
                    case OpCode.jump:
                    case OpCode.jump_t:
                    case OpCode.jump_f:
                    case OpCode.leave:
                        if (seh.AssessJump(address, ins.Arguments) == BranchHandling.Invalid)
                        {
                            reason = "jumping instruction at " + address + " invalid for SEH";
                            return false;
                        }
                        break;
                    case OpCode.ret_break:
                    case OpCode.ret_continue:
                    case OpCode.ret_exit:
                    case OpCode.ret_value:
                        if (seh.AssessJump(address, source.Code.Count) == BranchHandling.Invalid)
                        {
                            reason = "return instruction at " + address + " invalid for SEH";
                            return false;
                        }
                        break;
                }
            }

            if (cilExtensions.Count > 0)
            {
                var cilExtensionHint = new CilExtensionHint(cilExtensions);
                SetCilHint(source, cilExtensionHint);
            }

            //Otherwise, qualification passed.
            reason = null;
            return true;
        }

        #endregion

        #region Compile Function

        private static void _compile
            (PFunction source, ILGenerator il, Engine targetEngine, CompilerPass pass,
                FunctionLinking linking)
        {
            var state = new CompilerState(source, targetEngine, il, pass, linking);

            //Every cil implementation needs to instantiate a CilFunctionContext and assign PValue.Null to the result.
            _emitCilImplementationHeader(state);

            //Reads the functions metadata about parameters, local variables and shared variables.
            //initializes shared variables.
            _buildSymbolTable(state);

            //CODE ANALYSIS
            //  - determine number of temporary variables
            //  - find variable references (alters the symbol table)
            //  - determine stack size at all offsets
            _analysisAndPreparation(state);

            //Create and initialize local variables for parameters
            _parseParameters(state);

            //Shared variables and parameters have already been initialized
            // this method initializes (PValue.Null) the rest.
            _createAndInitializeRemainingLocals(state);

            //Emits IL for the functions Prexonite byte code.
            _emitInstructions(state);
        }

        private static void _emitCilImplementationHeader(CompilerState state)
        {
            //Create local cil function stack context
            //  CilFunctionContext cfctx = CilFunctionContext.New(sctx, source);
            state.SctxLocal = state.Il.DeclareLocal(typeof (CilFunctionContext));
            state.EmitLoadArg(CompilerState.ParamSctxIndex);
            state.EmitLoadArg(CompilerState.ParamSourceIndex);
            state.Il.EmitCall(OpCodes.Call, CilFunctionContext.NewMethod, null);
            state.EmitStoreLocal(state.SctxLocal.LocalIndex);

            //Initialize result and assign default return mode
            //  Result = null;
            state._EmitAssignReturnMode(ReturnMode.Exit);
            state.EmitLoadArg(CompilerState.ParamResultIndex);
            state.EmitLoadNullAsPValue();
            state.Il.Emit(OpCodes.Stind_Ref);
        }

        private static void _buildSymbolTable(CompilerState state)
        {
            //Create local ref variables for shared names
            //  and populate them with the contents of the sharedVariables parameter
            if (state.Source.Meta.ContainsKey(PFunction.SharedNamesKey))
            {
                var sharedNames = state.Source.Meta[PFunction.SharedNamesKey].List;
                for (var i = 0; i < sharedNames.Length; i++)
                {
                    if (state.Source.Variables.Contains(sharedNames[i]))
                        continue; //Arguments are redeclarations.
                    var sym = new CilSymbol(SymbolKind.LocalRef)
                        {
                            Local = state.Il.DeclareLocal(typeof (PVariable))
                        };
                    var id = sharedNames[i].Text;

                    state.EmitLoadArg(CompilerState.ParamSharedVariablesIndex);
                    state.Il.Emit(OpCodes.Ldc_I4, i);
                    state.Il.Emit(OpCodes.Ldelem_Ref);
                    state.EmitStoreLocal(sym.Local.LocalIndex);

                    state.Symbols.Add(id, sym);
                }
            }

            //Create index -> id map
            foreach (var mapping in state.Source.LocalVariableMapping)
                state.IndexMap.Add(mapping.Value, mapping.Key);

            //Add entries for paramters
            foreach (var parameter in state.Source.Parameters)
                if (!state.Symbols.ContainsKey(parameter))
                    state.Symbols.Add(parameter, new CilSymbol(SymbolKind.Local));

            //Add entries for enumerator variables
            foreach (var hint in state._ForeachHints)
            {
                if (state.Symbols.ContainsKey(hint.EnumVar))
                    throw new PrexoniteException(
                        "Invalid foreach hint. Enumerator variable is shared.");
                state.Symbols.Add(hint.EnumVar, new CilSymbol(SymbolKind.LocalEnum));
            }

            //Add entries for non-shared local variables
            foreach (var variable in state.Source.Variables)
                if (!state.Symbols.ContainsKey(variable))
                    state.Symbols.Add(variable, new CilSymbol(SymbolKind.Local));
        }

        private static void _analysisAndPreparation(CompilerState state)
        {
            var tempMaxOrder = 1; // 
            var needsSharedVariables = false;

            foreach (var ins in state.Source.Code.InReverse())
            {
                string toConvert;
                switch (ins.OpCode)
                {
                    case OpCode.ldr_loci:
                        //see ldr_loc
                        toConvert = state.IndexMap[ins.Arguments];
                        goto Convert;
                    case OpCode.ldr_loc:
                        toConvert = ins.Id;
                        Convert:

                        //Normal local variables are implemented as CIL locals.
                        // If the function uses variable references, they must be converted to reference variables.
                        state.Symbols[toConvert].Kind = SymbolKind.LocalRef;
                        break;
                    case OpCode.rot:
                        //Determine the maximum number of temporary variables for the implementation of rot[ate]
                        var order = (int) ins.GenericArgument;
                        if (order > tempMaxOrder)
                            tempMaxOrder = order;
                        break;
                    case OpCode.newclo:
                        MetaEntry[] entries;
                        var func = state.Source.ParentApplication.Functions[ins.Id];
                        MetaEntry entry;
                        if (func.Meta.ContainsKey(PFunction.SharedNamesKey) &&
                            (entry = func.Meta[PFunction.SharedNamesKey]).IsList)
                            entries = entry.List;
                        else
                            entries = new MetaEntry[] {};
                        foreach (var t in entries)
                        {
                            var symbolName = t.Text;
                            if (!state.Symbols.ContainsKey(symbolName))
                                throw new PrexoniteException
                                    (func + " does not contain a mapping for the symbol " +
                                        symbolName);

                            //In order for variables to be shared, they too, need to be converted to reference locals.
                            state.Symbols[symbolName].Kind = SymbolKind.LocalRef;
                        }

                        //Notify the compiler of the presence of closures with shared variables
                        needsSharedVariables = needsSharedVariables || entries.Length > 0;
                        break;
                }
            }

            //Create temporary variables for rotation
            state.TempLocals = new LocalBuilder[tempMaxOrder];
            for (var i = 0; i < tempMaxOrder; i++)
            {
                var rotTemp = state.Il.DeclareLocal(typeof (PValue));
                state.TempLocals[i] = rotTemp;
            }

            //Create temporary variable for argv and sharedVariables
            state.ArgvLocal = state.Il.DeclareLocal(typeof (PValue[]));
            state.SharedLocal = needsSharedVariables
                ? state.Il.DeclareLocal(typeof (PVariable[]))
                : null;

            //Create argc local variable and initialize it, if needed
            if (state.Source.Parameters.Count > 0)
            {
                state.ArgcLocal = state.Il.DeclareLocal(typeof (Int32));
                state.EmitLoadArg(CompilerState.ParamArgsIndex);
                state.Il.Emit(OpCodes.Ldlen);
                state.Il.Emit(OpCodes.Conv_I4);
                state.EmitStoreLocal(state.ArgcLocal);
            }

            //Determine stack size at every instruction
            _determineStackSize(state);
        }

        private static void _determineStackSize(CompilerState state)
        {
            if (state.Source.Code.Count == 0)
                return;

            var stackSize = new int?[state.StackSize.Length];
            // stack for abstract interpretation: (index, size-before)
            var interpretationStack = new Stack<Tuple<int, int>>();
            interpretationStack.Push(Tuple.Create(0, 0));

            while (interpretationStack.Count > 0)
            {
                var t = interpretationStack.Pop();
                var i = t.Item1;
                var currentStackSize = t.Item2;
                Debug.Assert(0 <= i && i < state.Source.Code.Count,
                    "Instruction pointer out of range.",
                    "During abstract interpretation to determine stack size, the instruction" +
                        " pointer assumed an invalid value {0}. Acceptable values are between 0 and {1}. " +
                            "The length of the stackSize array is {2}.",
                    i, state.Source.Code.Count - 1, stackSize.Length);
                var ins = state.Source.Code[i];
                int newValue;
                if (ins.IsFunctionExit)
                {
                    if (ins.OpCode == OpCode.ret_value && currentStackSize < 1)
                        throw new PrexoniteInvalidStackException(
                            String.Format(
                                "Function {0}: Stack underflow at return instruction {1}.",
                                state.Source, i));
                    newValue = currentStackSize + ins.StackSizeDelta;
                }
                else
                {
                    newValue = currentStackSize + ins.StackSizeDelta;
                }

                var oldValue = stackSize[i];
                if (newValue < 0)
                    throw new PrexoniteInvalidStackException(
                        String.Format("Function {0}: Instruction {1}: {2} causes stack underflow.",
                            state.Source, i, ins));

                if (oldValue.HasValue)
                {
                    //Debug.Assert(currentStackSize + delta == oldValue.Value);
                    if (newValue != oldValue)
                        throw new PrexoniteInvalidStackException(String.Format(
                            "Function {3}: Instruction {0} reached with stack size {1} and {2}",
                            i, oldValue.Value, newValue, state.Source));
                }
                else
                {
                    stackSize[i] = newValue;

                    if ((ins.IsJump || ins.OpCode == OpCode.leave)
                        && 0 <= ins.Arguments && ins.Arguments < stackSize.Length)
                        interpretationStack.Push(Tuple.Create(ins.Arguments, newValue));

                    if (i + 1 < stackSize.Length && (!ins.IsUnconditionalJump) &&
                        ins.OpCode != OpCode.leave)
                        interpretationStack.Push(Tuple.Create(i + 1, newValue));
                }
            }

            for (var i = 0; i < stackSize.Length; i++)
                state.StackSize[i] = stackSize[i] ?? 0;
        }

        private static void _parseParameters(CompilerState state)
        {
            for (var i = 0; i < state.Source.Parameters.Count; i++)
            {
                var id = state.Source.Parameters[i];
                var sym = state.Symbols[id];
                LocalBuilder local;

                //Determine whether local variables for parameters have already been created and create them if necessary
                switch (sym.Kind)
                {
                    case SymbolKind.Local:
                        local = sym.Local ?? state.Il.DeclareLocal(typeof (PValue));
                        break;
                    case SymbolKind.LocalRef:
                        if (sym.Local == null)
                        {
                            local = state.Il.DeclareLocal(typeof (PVariable));
                            state.Il.Emit(OpCodes.Newobj, NewPVariableCtor);
                            state.EmitStoreLocal(local);
                            //PVariable objects already contain PValue.Null and need not be initialized if no
                            //  argument has been passed.
                        }
                        else
                        {
                            local = sym.Local;
                        }
                        break;
                    default:
                        throw new PrexoniteException("Cannot create variable to represent symbol");
                }

                sym.Local = local;

                var hasArg = state.Il.DefineLabel();
                var end = state.Il.DefineLabel();

                if (sym.Kind == SymbolKind.Local) // var = idx < len ? args[idx] : null;
                {
                    //The closure below is only accessed once. The capture is therefore transparent.
                    // ReSharper disable AccessToModifiedClosure
                    state.EmitStorePValue
                        (
                            sym,
                            delegate
                                {
                                    //(idx < argc) ? args[idx] : null; 
                                    state.EmitLdcI4(i);
                                    state.EmitLoadLocal(state.ArgcLocal);
                                    state.Il.Emit(OpCodes.Blt_S, hasArg);
                                    state.EmitLoadNullAsPValue();
                                    state.Il.Emit(OpCodes.Br_S, end);
                                    state.Il.MarkLabel(hasArg);
                                    state.EmitLoadArg(CompilerState.ParamArgsIndex);
                                    state.EmitLdcI4(i);
                                    state.Il.Emit(OpCodes.Ldelem_Ref);
                                    state.Il.MarkLabel(end);
                                }
                        );
                    // ReSharper restore AccessToModifiedClosure
                }
                else // if(idx < len) var = args[idx];
                {
                    state.EmitLdcI4(i);
                    state.EmitLoadLocal(state.ArgcLocal);
                    state.Il.Emit(OpCodes.Bge_S, end);

                    //The following closure is only accessed once. The capture is therefore transparent.
                    // ReSharper disable AccessToModifiedClosure
                    state.EmitStorePValue
                        (
                            sym,
                            delegate
                                {
                                    state.EmitLoadArg(CompilerState.ParamArgsIndex);
                                    state.EmitLdcI4(i);
                                    state.Il.Emit(OpCodes.Ldelem_Ref);
                                });
                    // ReSharper restore AccessToModifiedClosure
                    state.Il.MarkLabel(end);
                }
            }
        }

        private static void _createAndInitializeRemainingLocals(CompilerState state)
        {
            var nullLocals = new List<LocalBuilder>();

            //Create remaining local variables and initialize them
            foreach (var pair in state.Symbols)
            {
                var id = pair.Key;
                var sym = pair.Value;
                if (sym.Local != null)
                    continue;

                switch (sym.Kind)
                {
                    case SymbolKind.Local:
                        {
                            sym.Local = state.Il.DeclareLocal(typeof (PValue));
                            var initVal = _getVariableInitialization(state, id, false);
                            switch (initVal)
                            {
                                case VariableInitialization.ArgV:
                                    _emitLoadArgV(state);
                                    state.EmitStoreLocal(sym.Local);
                                    break;
                                case VariableInitialization.Null:
                                    nullLocals.Add(sym.Local); //defer assignment
                                    break;

                                    // ReSharper disable RedundantCaseLabel
                                case VariableInitialization.None:
                                    // ReSharper restore RedundantCaseLabel
                                default:
                                    break;
                            }
                        }
                        break;
                    case SymbolKind.LocalRef:
                        {
                            sym.Local = state.Il.DeclareLocal(typeof (PVariable));
                            var initVal = _getVariableInitialization(state, id, true);

                            var idx = sym.Local.LocalIndex;

                            state.Il.Emit(OpCodes.Newobj, NewPVariableCtor);

                            if (initVal != VariableInitialization.None)
                            {
                                state.Il.Emit(OpCodes.Dup);
                                state.EmitStoreLocal(idx);

                                switch (initVal)
                                {
                                    case VariableInitialization.ArgV:
                                        _emitLoadArgV(state);
                                        break;
                                    case VariableInitialization.Null:
                                        state.EmitLoadNullAsPValue();
                                        break;

                                    default:
                                        break;
                                }
                                state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                            }
                            else
                            {
                                state.EmitStoreLocal(idx);
                            }
                        }
                        break;
                    case SymbolKind.LocalEnum:
                        {
                            sym.Local = state.Il.DeclareLocal(typeof (IEnumerator<PValue>));
                            //No initialization needed.
                        }
                        break;
                    default:
                        throw new PrexoniteException("Cannot initialize unknown symbol kind.");
                }
            }

            //Initialize null locals
            var nullCount = nullLocals.Count;
            if (nullCount > 0)
            {
                state.EmitLoadNullAsPValue();
                for (var i = 0; i < nullCount; i++)
                {
                    var local = nullLocals[i];
                    if (i + 1 != nullCount)
                        state.Il.Emit(OpCodes.Dup);
                    state.EmitStoreLocal(local);
                }
            }
        }

        private static void _emitInstructions(CompilerState state)
        {
            //Tables of foreach call hint hooks
            var foreachCasts = new Dictionary<int, ForeachHint>();
            var foreachGetCurrents = new Dictionary<int, ForeachHint>();
            var foreachMoveNexts = new Dictionary<int, ForeachHint>();
            var foreachDisposes = new Dictionary<int, ForeachHint>();

            foreach (var hint in state._ForeachHints)
            {
                foreachCasts.Add(hint.CastAddress, hint);
                foreachGetCurrents.Add(hint.GetCurrentAddress, hint);
                foreachMoveNexts.Add(hint.MoveNextAddress, hint);
                foreachDisposes.Add(hint.DisposeAddress, hint);
            }

            var sourceCode = state.Source.Code;

            //CIL Extension
            var cilExtensionMode = false;
            List<CompileTimeValue> staticArgv = null;

            for (var instructionIndex = 0; instructionIndex < sourceCode.Count; instructionIndex++)
            {
                #region Handling for try-finally-catch blocks

                //Handle try-finally-catch blocks
                //Push new blocks
                foreach (var block in state.Seh.GetOpeningTryBlocks(instructionIndex))
                {
                    state.TryBlocks.Push(block);
                    if (block.HasFinally)
                        state.Il.BeginExceptionBlock();
                    if (block.HasCatch)
                        state.Il.BeginExceptionBlock();
                }

                //Handle active blocks
                if (state.TryBlocks.Count > 0)
                {
                    CompiledTryCatchFinallyBlock block;
                    do
                    {
                        block = state.TryBlocks.Peek();
                        if (instructionIndex == block.BeginFinally)
                        {
                            if (block.SkipTry == instructionIndex)
                            {
                                //state.Il.MarkLabel(block.SkipTryLabel);
                                state.Il.Emit(OpCodes.Nop);
                            }
                            state.Il.BeginFinallyBlock();
                        }
                        else if (instructionIndex == block.BeginCatch)
                        {
                            if (block.HasFinally)
                                state.Il.EndExceptionBlock(); //end finally here
                            state.Il.BeginCatchBlock(typeof (Exception));
                            //parse the exception
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, Runtime.ParseExceptionMethod, null);
                            //user code will store it in a local variable
                        }
                        else if (instructionIndex == block.EndTry)
                        {
                            if (block.HasFinally || block.HasCatch)
                                state.Il.EndExceptionBlock();
                            if (block.SkipTry == instructionIndex)
                            {
                                //state.Il.MarkLabel(block.SkipTryLabel);
                                state.Il.Emit(OpCodes.Nop);
                            }
                            state.TryBlocks.Pop();
                            block = null; //signal another loop iteration
                        }
                    } while (block == null && state.TryBlocks.Count > 0);
                }

                #endregion

                state.MarkInstruction(instructionIndex);

                var ins = sourceCode[instructionIndex];

                #region CIL hints

                // **** CIL hints ****
                //  * CIL Extension *
                {
                    if (state._CilExtensionOffsets.Count > 0 &&
                        state._CilExtensionOffsets.Peek() == instructionIndex)
                    {
                        state._CilExtensionOffsets.Dequeue();
                        if (staticArgv == null)
                            staticArgv = new List<CompileTimeValue>(8);
                        else
                            staticArgv.Clear();
                        cilExtensionMode = true;
                    }
                    if (cilExtensionMode)
                    {
                        CompileTimeValue compileTimeValue;
                        if (CompileTimeValue.TryParse(ins, state.IndexMap, state.Cache, state.Source.ParentApplication.Module.Name, out compileTimeValue))
                        {
                            staticArgv.Add(compileTimeValue);
                        }
                        else
                        {
                            //found the actual invocation of the CIL extension
                            cilExtensionMode = false;

                            switch (ins.OpCode)
                            {
                                case OpCode.cmd:
                                    PCommand command;
                                    ICilExtension extension;
                                    if (
                                        !state.TargetEngine.Commands.TryGetValue(ins.Id, out command) ||
                                            (extension = command as ICilExtension) == null)
                                        goto default;

                                    extension.Implement(state, ins, staticArgv.ToArray(),
                                        ins.Arguments - staticArgv.Count);
                                    break;
                                default:
                                    throw new PrexoniteException(
                                        "The CIL compiler does not support CIL extensions for this opcode: " +
                                            ins);
                            }
                        }
                        continue;
                    }
                }
                //  * Foreach *
                {
                    ForeachHint hint;
                    if (foreachCasts.TryGetValue(instructionIndex, out hint))
                    {
                        //result of (expr).GetEnumerator on the stack
                        //cast IEnumerator
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.ExtractEnumeratorMethod, null);
                        instructionIndex++;
                        //stloc enum
                        state.EmitStoreLocal(state.Symbols[hint.EnumVar].Local);
                        continue;
                    }
                    else if (foreachGetCurrents.TryGetValue(instructionIndex, out hint))
                    {
                        //ldloc enum
                        state.EmitLoadLocal(state.Symbols[hint.EnumVar].Local);
                        instructionIndex++;
                        //get.0 Current
                        state.Il.EmitCall(OpCodes.Callvirt, ForeachHint.GetCurrentMethod, null);
                        //result will be stored by user code
                        continue;
                    }
                    else if (foreachMoveNexts.TryGetValue(instructionIndex, out hint))
                    {
                        //ldloc enum
                        state.EmitLoadLocal(state.Symbols[hint.EnumVar].Local);
                        instructionIndex++;
                        //get.0 MoveNext
                        state.Il.EmitCall(OpCodes.Callvirt, ForeachHint.MoveNextMethod, null);
                        instructionIndex++;
                        //jump.t begin
                        var target = sourceCode[instructionIndex].Arguments; //read from user code
                        state.Il.Emit(OpCodes.Brtrue, state.InstructionLabels[target]);
                        continue;
                    }
                    else if (foreachDisposes.TryGetValue(instructionIndex, out hint))
                    {
                        //ldloc enum
                        state.EmitLoadLocal(state.Symbols[hint.EnumVar].Local);
                        instructionIndex++;
                        //@cmd.1 dispose
                        state.Il.EmitCall(OpCodes.Callvirt, ForeachHint.DisposeMethod, null);
                        continue;
                    }
                }

                #endregion

                //  * Normal code generation *
                //Decode instruction
                var argc = ins.Arguments;
                var justEffect = ins.JustEffect;
                var id = ins.Id;
                int idx;
                string methodId;
                string typeExpr;
                var moduleName = ins.ModuleName;

                //Emit code for the instruction
                switch (ins.OpCode)
                {
                        #region NOP

                        //NOP
                    case OpCode.nop:
                        //Do nothing
                        state.Il.Emit(OpCodes.Nop);
                        break;

                        #endregion

                        #region LOAD

                        #region LOAD CONSTANT

                        //LOAD CONSTANT
                    case OpCode.ldc_int:
                        state.EmitLoadIntAsPValue(argc);
                        break;
                    case OpCode.ldc_real:
                        state.EmitLoadRealAsPValue(ins);
                        break;
                    case OpCode.ldc_bool:
                        state.EmitLoadBoolAsPValue(argc != 0);
                        break;
                    case OpCode.ldc_string:
                        state.EmitLoadStringAsPValue(id);
                        break;

                    case OpCode.ldc_null:
                        state.EmitLoadNullAsPValue();
                        break;

                        #endregion LOAD CONSTANT

                        #region LOAD REFERENCE

                        //LOAD REFERENCE
                    case OpCode.ldr_loc:
                        state.EmitLoadLocalRefAsPValue(id);
                        break;
                    case OpCode.ldr_loci:
                        id = state.IndexMap[argc];
                        goto case OpCode.ldr_loc;
                    case OpCode.ldr_glob:
                        state.EmitLoadGlobalRefAsPValue(id, moduleName);
                        break;
                    case OpCode.ldr_func:
                        state.EmitLoadFuncRefAsPValue(id, moduleName);
                        break;
                    case OpCode.ldr_cmd:
                        state.EmitLoadCmdRefAsPValue(id);
                        break;
                    case OpCode.ldr_app:
                        CompilerState.EmitLoadAppRefAsPValue(state);
                        break;
                    case OpCode.ldr_eng:
                        state.EmitLoadEngRefAsPValue();
                        break;
                    case OpCode.ldr_type:
                        state.EmitPTypeAsPValue(id);
                        break;
                    case OpCode.ldr_mod:
                        state.EmitModuleNameAsPValue(moduleName);
                        break;

                        #endregion //LOAD REFERENCE

                        #endregion //LOAD

                        #region VARIABLES

                        #region LOCAL

                        //LOAD LOCAL VARIABLE
                    case OpCode.ldloc:
                        state.EmitLoadPValue(state.Symbols[id]);
                        break;
                    case OpCode.stloc:
                        //Don't use EmitStorePValue here, because this is a more efficient solution
                        var sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitStoreLocal(sym.Local.LocalIndex);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitStoreLocal(state.PrimaryTempLocal.LocalIndex);
                            state.EmitLoadLocal(sym.Local.LocalIndex);
                            state.EmitLoadLocal(state.PrimaryTempLocal.LocalIndex);
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                        break;

                    case OpCode.ldloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.ldloc;

                    case OpCode.stloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.stloc;

                        #endregion

                        #region GLOBAL

                        //LOAD GLOBAL VARIABLE
                    case OpCode.ldglob:
                        state.EmitLoadGlobalValue(id, moduleName);
                        break;
                    case OpCode.stglob:
                        state.EmitStoreLocal(state.PrimaryTempLocal);
                        state.EmitLoadGlobalReference(id,moduleName);
                        state.EmitLoadLocal(state.PrimaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        break;

                        #endregion

                        #endregion

                        #region CONSTRUCTION

                        //CONSTRUCTION
                    case OpCode.newobj:
                        state.EmitNewObj(id, argc);
                        break;
                    case OpCode.newtype:
                        state.FillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.ReadArgv(argc);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, Runtime.NewTypeMethod, null);
                        break;

                    case OpCode.newclo:
                        //Collect shared variables
                        MetaEntry[] entries;
                        var func = state.Source.ParentApplication.Functions[id];
                        if (func.Meta.ContainsKey(PFunction.SharedNamesKey))
                            entries = func.Meta[PFunction.SharedNamesKey].List;
                        else
                            entries = new MetaEntry[] {};
                        var hasSharedVariables = entries.Length > 0;
                        if (hasSharedVariables)
                        {
                            state.EmitLdcI4(entries.Length);
                            state.Il.Emit(OpCodes.Newarr, typeof (PVariable));
                            state.EmitStoreLocal(state.SharedLocal);
                            for (var i = 0; i < entries.Length; i++)
                            {
                                state.EmitLoadLocal(state.SharedLocal);
                                state.EmitLdcI4(i);
                                state.EmitLoadLocal(state.Symbols[entries[i].Text].Local);
                                state.Il.Emit(OpCodes.Stelem_Ref);
                            }
                        }
                        state.EmitLoadLocal(state.SctxLocal);
                        if (hasSharedVariables)
                            state.EmitLoadLocal(state.SharedLocal);
                        else
                            state.Il.Emit(OpCodes.Ldnull);

                        state.EmitNewClo(id,moduleName);
                        break;

                    case OpCode.newcor:
                        state.FillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.ReadArgv(argc);
                        state.Il.EmitCall(OpCodes.Call, Runtime.NewCoroutineMethod, null);
                        break;

                        #endregion

                        #region OPERATORS

                        #region UNARY

                        //UNARY OPERATORS
                    case OpCode.incloc:
                        sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVIncrementMethod, null);
                            state.EmitStoreLocal(sym.Local);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.Il.Emit(OpCodes.Dup);
                            state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVIncrementMethod, null);
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                        break;

                    case OpCode.incloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.incloc;

                    case OpCode.incglob:
                        state.EmitLoadGlobalReference(id,moduleName);
                        state.Il.Emit(OpCodes.Dup);
                        state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, PVIncrementMethod, null);
                        state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        break;

                    case OpCode.decloc:
                        sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVDecrementMethod, null);
                            state.EmitStoreLocal(sym.Local);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.Il.Emit(OpCodes.Dup);
                            state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVDecrementMethod, null);
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                        break;
                    case OpCode.decloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.decloc;

                    case OpCode.decglob:
                        state.EmitLoadGlobalReference(id,moduleName);
                        state.Il.Emit(OpCodes.Dup);
                        state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, PVDecrementMethod, null);
                        state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        break;

                        #endregion

                        #region BINARY

                        // all binary operators are implemented as CIL extensions in
                        //  Prexonite.Commands.Core.Operators

                        #endregion //OPERATORS

                        #endregion

                        #region TYPE OPERATIONS

                        #region TYPE CHECK

                        //TYPE CHECK
                    case OpCode.check_const:
                        //Stack:
                        //  Obj
                        state.EmitLoadType(id);
                        //Stack:
                        //  Obj
                        //  Type
                        state.EmitCall(Runtime.CheckTypeConstMethod);
                        break;
                    case OpCode.check_arg:
                        //Stack: 
                        //  Obj
                        //  Type
                        state.Il.EmitCall(OpCodes.Call, Runtime.CheckTypeMethod, null);
                        break;

                    case OpCode.check_null:
                        state.Il.EmitCall(OpCodes.Call, PVIsNullMethod, null);
                        state.Il.Emit(OpCodes.Box, typeof (bool));
                        state.Il.EmitCall(OpCodes.Call, GetBoolPType, null);
                        state.Il.Emit(OpCodes.Newobj, NewPValue);
                        break;

                        #endregion

                        #region TYPE CAST

                    case OpCode.cast_const:
                        //Stack:
                        //  Obj
                        state.EmitLoadType(id);
                        //Stack:
                        //  Obj
                        //  Type
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitCall(Runtime.CastConstMethod);

                        break;
                    case OpCode.cast_arg:
                        //Stack
                        //  Obj
                        //  Type
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.CastMethod, null);
                        break;

                        #endregion

                        #endregion

                        #region OBJECT CALLS

                        #region DYNAMIC

                    case OpCode.get:
                        state.FillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.ReadArgv(argc);
                        state.EmitLdcI4((int) PCall.Get);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, PVDynamicCallMethod, null);
                        if (justEffect)
                            state.Il.Emit(OpCodes.Pop);
                        break;

                    case OpCode.set:
                        state.FillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.ReadArgv(argc);
                        state.EmitLdcI4((int) PCall.Set);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, PVDynamicCallMethod, null);
                        state.Il.Emit(OpCodes.Pop);
                        break;

                        #endregion

                        #region STATIC

                    case OpCode.sget:
                        //Stack:
                        //  arg
                        //   .
                        //   .
                        //   .
                        state.FillArgv(argc);
                        idx = id.LastIndexOf("::");
                        if (idx < 0)
                            throw new PrexoniteException
                                (
                                "Invalid sget instruction. Does not specify a method.");
                        methodId = id.Substring(idx + 2);
                        typeExpr = id.Substring(0, idx);
                        state.EmitLoadType(typeExpr);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.ReadArgv(argc);
                        state.EmitLdcI4((int) PCall.Get);
                        state.Il.Emit(OpCodes.Ldstr, methodId);
                        state.EmitVirtualCall(Runtime.StaticCallMethod);
                        if (justEffect)
                            state.Il.Emit(OpCodes.Pop);
                        break;

                    case OpCode.sset:
                        state.FillArgv(argc);
                        idx = id.LastIndexOf("::");
                        if (idx < 0)
                            throw new PrexoniteException
                                (
                                "Invalid sset instruction. Does not specify a method.");
                        methodId = id.Substring(idx + 2);
                        typeExpr = id.Substring(0, idx);
                        state.EmitLoadType(typeExpr);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.ReadArgv(argc);
                        state.EmitLdcI4((int) PCall.Set);
                        state.Il.Emit(OpCodes.Ldstr, methodId);
                        state.EmitVirtualCall(Runtime.StaticCallMethod);
                        state.Il.Emit(OpCodes.Pop);
                        break;

                        #endregion

                        #endregion

                        #region INDIRECT CALLS

                    case OpCode.indloc:
                        sym = state.Symbols[id];
                        if (sym == null)
                            throw new PrexoniteException(
                                "Internal CIL compiler error. Information about local entity " + id +
                                    " missing.");
                        state.FillArgv(argc);
                        sym.EmitLoad(state);
                        state.EmitIndirectCall(argc, justEffect);
                        break;

                    case OpCode.indloci:
                        idx = argc & ushort.MaxValue;
                        argc = (argc & (ushort.MaxValue << 16)) >> 16;
                        id = state.IndexMap[idx];
                        goto case OpCode.indloc;

                    case OpCode.indglob:
                        state.FillArgv(argc);
                        state.EmitLoadGlobalValue(id,moduleName);
                        state.EmitIndirectCall(argc, justEffect);
                        break;

                    case OpCode.indarg:
                        //Stack
                        //  obj
                        //  args
                        state.FillArgv(argc);
                        state.EmitIndirectCall(argc, justEffect);
                        break;

                    case OpCode.tail:
                        throw new PrexoniteException(
                            "Cannot compile tail instruction to CIL. Qualification should have failed.");

                        #endregion

                        #region ENGINE CALLS

                    case OpCode.func:
                        state.EmitFuncCall(argc, id, moduleName, justEffect);
                        break;
                    case OpCode.cmd:
                        state.EmitCommandCall(ins);
                        break;

                        #endregion

                        #region FLOW CONTROL

                        //FLOW CONTROL

                        #region JUMPS

                    case OpCode.jump:
                    case OpCode.jump_t:
                    case OpCode.jump_f:
                    case OpCode.ret_break:
                    case OpCode.ret_continue:
                        state.Seh.EmitJump(instructionIndex, ins);
                        break;

                        #endregion

                        #region RETURNS

                    case OpCode.ret_exit:
                        goto case OpCode.jump;

                    case OpCode.ret_value:
                        //return value is assigned by SEH
                        goto case OpCode.jump;

                    case OpCode.ret_set:
                        state.EmitSetReturnValue();
                        break;

                        #endregion

                        #region THROW

                    case OpCode.@throw:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.ThrowExceptionMethod, null);
                        break;

                        #endregion

                        #region LEAVE

                    case OpCode.@try:
                        //Is done via analysis of TryCatchFinally objects associated with the function
                        Debug.Assert(state.StackSize[instructionIndex] == 0,
                            "The stack should be empty when entering a try-block.",
                            "The stack is not empty when entering the try-block at instruction {0} in function {1}.",
                            instructionIndex, state.Source);
                        break;

                    case OpCode.leave:
                        //is handled by the CLR

                        #endregion

                        #region EXCEPTION

                    case OpCode.exc:
                        //is not implemented via Emit
                        // The exception is stored when the exception block is entered.
                        break;

                        #endregion

                        #endregion

                        #region STACK MANIPULATION

                        //STACK MANIPULATION
                    case OpCode.pop:
                        for (var i = 0; i < argc; i++)
                            state.Il.Emit(OpCodes.Pop);
                        break;
                    case OpCode.dup:
                        for (var i = 0; i < argc; i++)
                            state.Il.Emit(OpCodes.Dup);
                        break;
                    case OpCode.rot:
                        var values = (int) ins.GenericArgument;
                        var rotations = argc;
                        for (var i = 0; i < values; i++)
                            state.EmitStoreLocal
                                (
                                    state.TempLocals[(i + rotations)%values].LocalIndex);
                        for (var i = values - 1; i >= 0; i--)
                            state.EmitLoadLocal(state.TempLocals[i].LocalIndex);
                        break;

                        #endregion
                } //end of switch over opcode

                //DON'T ADD ANY CODE HERE, A LOT OF CASES USE `CONTINUE`
            } // end of loop over instructions

            //Close all pending try blocks, since the next instruction will never come
            //  (other closing try blocks are handled by the emitting the instruction immediately following 
            //  the try block)
            foreach (var block in state.TryBlocks)
            {
                if (block.HasCatch || block.HasFinally)
                    state.Il.EndExceptionBlock();
            }

            //Implicit return
            //Often instructions refer to a virtual instruction after the last real one.
            state.MarkInstruction(sourceCode.Count);
            state.Il.MarkLabel(state.ReturnLabel);
            state.Il.Emit(OpCodes.Ret);
        }

        #region IL helper

        // ReSharper disable InconsistentNaming

        public static readonly MethodInfo CreateNativePValue =
            typeof (CilFunctionContext).GetMethod("CreateNativePValue", new[] {typeof (object)});

        private static readonly MethodInfo _GetBoolPType =
            typeof (PType).GetProperty("Bool").GetGetMethod();

        private static readonly MethodInfo _GetIntPType =
            typeof (PType).GetProperty("Int").GetGetMethod();

        private static readonly MethodInfo _GetPTypeListMethod =
            typeof (PType).GetProperty("List").GetGetMethod();

        private static readonly MethodInfo _getPTypeNull =
            typeof (PType).GetProperty("Null").GetGetMethod();

        private static readonly MethodInfo _GetRealPType =
            typeof (PType).GetProperty("Real").GetGetMethod();

        private static readonly MethodInfo _GetStringPType =
            typeof (PType).GetProperty("String").GetGetMethod();

        private static readonly MethodInfo _GetCharPType =
            typeof (PType).GetProperty("Char").GetGetMethod();

        internal static readonly MethodInfo GetNullPType =
            typeof (PType).GetProperty("Null").GetGetMethod();

        internal static readonly MethodInfo GetObjectProxy =
            typeof (PType).GetProperty("Object").GetGetMethod();

        private static readonly MethodInfo _getValue =
            typeof (PVariable).GetProperty("Value").GetGetMethod();

        private static readonly ConstructorInfo _NewPValue =
            typeof (PValue).GetConstructor(new[] {typeof (object), typeof (PType)});

        private static readonly ConstructorInfo _NewPValueListCtor =
            typeof (List<PValue>).GetConstructor(new[] {typeof (IEnumerable<PValue>)});

        private static readonly ConstructorInfo _newPVariableCtor =
            typeof (PVariable).GetConstructor(new Type[] {});

        private static readonly MethodInfo _nullCreatePValue =
            typeof (NullPType).GetMethod("CreatePValue", new Type[] {});

        private static readonly MethodInfo _PVAdditionMethod =
            typeof (PValue).GetMethod("Addition", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVBitwiseAndMethod =
            typeof (PValue).GetMethod
                (
                    "BitwiseAnd", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVBitwiseOrMethod =
            typeof (PValue).GetMethod("BitwiseOr", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVDecrementMethod =
            typeof (PValue).GetMethod("Decrement", new[] {typeof (StackContext)});

        private static readonly MethodInfo _PVDivisionMethod =
            typeof (PValue).GetMethod("Division", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVDynamicCallMethod =
            typeof (PValue).GetMethod("DynamicCall");

        private static readonly MethodInfo _PVEqualityMethod =
            typeof (PValue).GetMethod("Equality", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVExclusiveOrMethod =
            typeof (PValue).GetMethod
                (
                    "ExclusiveOr", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVGreaterThanMethod =
            typeof (PValue).GetMethod
                (
                    "GreaterThan", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVGreaterThanOrEqualMethod =
            typeof (PValue).GetMethod
                (
                    "GreaterThanOrEqual", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVIncrementMethod =
            typeof (PValue).GetMethod("Increment", new[] {typeof (StackContext)});

        private static readonly MethodInfo _PVIndirectCallMethod =
            typeof (PValue).GetMethod("IndirectCall");

        private static readonly MethodInfo _PVOnesComplementMethod =
            typeof (PValue).GetMethod("OnesComplement",
                new[]
                    {
                        typeof (
                            StackContext)
                    });

        private static readonly MethodInfo _PVInequalityMethod =
            typeof (PValue).GetMethod
                (
                    "Inequality", new[] {typeof (StackContext), typeof (PValue)});


        private static readonly MethodInfo _PVIsNullMethod =
            typeof (PValue).GetProperty("IsNull").GetGetMethod();

        private static readonly MethodInfo _PVLessThanMethod =
            typeof (PValue).GetMethod("LessThan", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVLessThanOrEqualMethod =
            typeof (PValue).GetMethod
                (
                    "LessThanOrEqual", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVLogicalNotMethod =
            typeof (PValue).GetMethod("LogicalNot", new[] {typeof (StackContext)});

        private static readonly MethodInfo _PVModulusMethod =
            typeof (PValue).GetMethod("Modulus", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVMultiplyMethod =
            typeof (PValue).GetMethod("Multiply", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVSubtractionMethod =
            typeof (PValue).GetMethod
                (
                    "Subtraction", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVUnaryNegationMethod =
            typeof (PValue).GetMethod("UnaryNegation", new[] {typeof (StackContext)});

        private static readonly MethodInfo _setValue =
            typeof (PVariable).GetProperty("Value").GetSetMethod();

        private static MethodInfo GetPTypeListMethod
        {
            get { return _GetPTypeListMethod; }
        }

        private static ConstructorInfo NewPValueListCtor
        {
            get { return _NewPValueListCtor; }
        }

        internal static MethodInfo getPTypeNull
        {
            get { return _getPTypeNull; }
        }

        internal static MethodInfo nullCreatePValue
        {
            get { return _nullCreatePValue; }
        }

        public static ConstructorInfo NewPVariableCtor
        {
            get { return _newPVariableCtor; }
        }

        public static MethodInfo GetValueMethod
        {
            get { return _getValue; }
        }

        public static MethodInfo SetValueMethod
        {
            get { return _setValue; }
        }

        internal static MethodInfo GetIntPType
        {
            get { return _GetIntPType; }
        }

        internal static MethodInfo GetRealPType
        {
            get { return _GetRealPType; }
        }

        internal static MethodInfo GetBoolPType
        {
            get { return _GetBoolPType; }
        }

        internal static MethodInfo GetStringPType
        {
            get { return _GetStringPType; }
        }

        internal static MethodInfo GetCharPType
        {
            get { return _GetCharPType; }
        }

        private static readonly MethodInfo _GetObjectPTypeSelector =
            typeof (PType).GetProperty("Object").GetGetMethod();

        public static MethodInfo GetObjectPTypeSelector
        {
            get { return _GetObjectPTypeSelector; }
        }

        private static readonly MethodInfo _CreatePValueAsObject = typeof (
            PType.PrexoniteObjectTypeProxy).GetMethod
            ("CreatePValue", new[] {typeof (object)});

        public static MethodInfo CreatePValueAsObject
        {
            get { return _CreatePValueAsObject; }
        }

        private static readonly ConstructorInfo _NewPValueKeyValuePair =
            typeof (PValueKeyValuePair).GetConstructor(new[] {typeof (PValue), typeof (PValue)});


        public static ConstructorInfo NewPValueKeyValuePair
        {
            get { return _NewPValueKeyValuePair; }
        }

        //private readonly MethodInfo _CreateNativePValue = typeof(StackContext).GetMethod("CreateNativePValue");
        //private MethodInfo CreateNativePValue
        //{
        //    get { return _CreateNativePValue; }
        //}

        internal static ConstructorInfo NewPValue
        {
            get { return _NewPValue; }
        }

        public static MethodInfo PVIncrementMethod
        {
            get { return _PVIncrementMethod; }
        }

        public static MethodInfo PVDecrementMethod
        {
            get { return _PVDecrementMethod; }
        }

        public static MethodInfo PVUnaryNegationMethod
        {
            get { return _PVUnaryNegationMethod; }
        }

        public static MethodInfo PVLogicalNotMethod
        {
            get { return _PVLogicalNotMethod; }
        }

        public static MethodInfo PVAdditionMethod
        {
            get { return _PVAdditionMethod; }
        }

        public static MethodInfo PVSubtractionMethod
        {
            get { return _PVSubtractionMethod; }
        }

        public static MethodInfo PVMultiplyMethod
        {
            get { return _PVMultiplyMethod; }
        }

        public static MethodInfo PVDivisionMethod
        {
            get { return _PVDivisionMethod; }
        }

        public static MethodInfo PVModulusMethod
        {
            get { return _PVModulusMethod; }
        }

        public static MethodInfo PVBitwiseAndMethod
        {
            get { return _PVBitwiseAndMethod; }
        }

        public static MethodInfo PVBitwiseOrMethod
        {
            get { return _PVBitwiseOrMethod; }
        }

        public static MethodInfo PVExclusiveOrMethod
        {
            get { return _PVExclusiveOrMethod; }
        }

        public static MethodInfo PVEqualityMethod
        {
            get { return _PVEqualityMethod; }
        }

        public static MethodInfo PVInequalityMethod
        {
            get { return _PVInequalityMethod; }
        }

        public static MethodInfo PVGreaterThanMethod
        {
            get { return _PVGreaterThanMethod; }
        }

        public static MethodInfo PVLessThanMethod
        {
            get { return _PVLessThanMethod; }
        }

        public static MethodInfo PVGreaterThanOrEqualMethod
        {
            get { return _PVGreaterThanOrEqualMethod; }
        }

        public static MethodInfo PVLessThanOrEqualMethod
        {
            get { return _PVLessThanOrEqualMethod; }
        }

        public static MethodInfo PVIsNullMethod
        {
            get { return _PVIsNullMethod; }
        }

        public static MethodInfo PVDynamicCallMethod
        {
            get { return _PVDynamicCallMethod; }
        }

        public static MethodInfo PVIndirectCallMethod
        {
            get { return _PVIndirectCallMethod; }
        }

        public static MethodInfo PVOnesComplementMethod
        {
            get { return _PVOnesComplementMethod; }
        }

        // ReSharper restore InconsistentNaming

        private enum VariableInitialization
        {
            None,
            Null,
            ArgV
        }

        private static VariableInitialization _getVariableInitialization(CompilerState state,
            string id, bool isRef)
        {
            if (Engine.StringsAreEqual(id, state.EffectiveArgumentsListId))
            {
                return VariableInitialization.ArgV;
            }
            else if (!isRef)
            {
                return VariableInitialization.Null;
            }
            else
            {
                return VariableInitialization.None;
            }
        }

        private static void _emitLoadArgV(CompilerState state)
        {
            state.EmitLoadArg(CompilerState.ParamArgsIndex);
            state.Il.Emit(OpCodes.Newobj, NewPValueListCtor);
            state.Il.EmitCall(OpCodes.Call, GetPTypeListMethod, null);
            state.Il.Emit(OpCodes.Newobj, NewPValue);
        }

        #endregion //IL Helper

        #endregion

        /// <summary>
        ///     Sets the supplied CIL hint on the meta table. Replaces previous CIL hints of the same type.
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "newHint"></param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Cil")]
        public static void SetCilHint(IHasMetaTable target, ICilHint newHint)
        {
            MetaEntry cilHints;
            if (target.Meta.TryGetValue(Loader.CilHintsKey, out cilHints))
            {
                var hints = cilHints.List;
                var replaced = false;
                var excessHints = 0;
                for (var i = 0; i < hints.Length; i++)
                {
                    var cilHint = hints[i].List;

                    //We're only interested in CIL hints that conflict with the new one.
                    if (!Engine.StringsAreEqual(cilHint[0].Text, newHint.CilKey))
                        continue;


                    if (replaced)
                    {
                        hints[i] = null;
                        excessHints++;
                    }
                    else
                    {
                        hints[i] = newHint.ToMetaEntry();
                        replaced = true;
                    }
                }

                if (excessHints == 0)
                {
                    if (!replaced)
                        target.Meta.AddTo(Loader.CilHintsKey, newHint.ToMetaEntry());
                    //otherwise the array has already been modified by ref.
                }
                else
                {
                    //need to resize array (and possibly add new CIL hint)
                    var newHints = new MetaEntry[hints.Length - excessHints + (replaced ? 0 : 1)];
                    int idxNew;
                    int idxOld;
                    for (idxNew = idxOld = 0; idxOld < hints.Length; idxOld++)
                    {
                        var oldHint = hints[idxOld];
                        if (oldHint == null)
                            continue;

                        newHints[idxNew++] = oldHint;
                    }
                    if (!replaced)
                        newHints[idxNew] = newHint.ToMetaEntry();

                    target.Meta[Loader.CilHintsKey] = (MetaEntry) newHints;
                }
            }
            else
            {
                target.Meta[Loader.CilHintsKey] = (MetaEntry) new[] {newHint.ToMetaEntry()};
            }
        }

        /// <summary>
        ///     Adds the supplied CIL hint to the meta table. Does not touch existing hints, even of the same type.
        /// </summary>
        /// <param name = "target">The meta table to add the CIL hint to</param>
        /// <param name = "hint">The CIL hint to add</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Cil")]
        public static void AddCilHint(IHasMetaTable target, ICilHint hint)
        {
            if (target.Meta.ContainsKey(Loader.CilHintsKey))
                target.Meta.AddTo(Loader.CilHintsKey, hint.ToMetaEntry());
            else
                target.Meta[Loader.CilHintsKey] = (MetaEntry) new[] {hint.ToMetaEntry()};
        }
    }
}