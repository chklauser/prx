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

#if ((!(DEBUG || Verbose)) || forceIndex) && allowIndex
#define UseIndex
#endif

#region Namespace Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Compatibility;
using Prexonite.Internal;
using Prexonite.Modular;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite.Compiler
{
    public class CompilerTarget : IHasMetaTable
    {
        private readonly LinkedList<AddressChangeHook> _addressChangeHooks =
            new LinkedList<AddressChangeHook>();

        public ICollection<AddressChangeHook> AddressChangeHooks
        {
            get { return _addressChangeHooks; }
        }

        #region IHasMetaTable Members

        /// <summary>
        ///     Provides access to the <see cref = "Function" />'s metatable.
        /// </summary>
        public MetaTable Meta
        {
            [DebuggerStepThrough]
            get { return _function.Meta; }
        }

        #endregion

        /// <summary>
        ///     Returns the <see cref = "Function" />'s string representation.
        /// </summary>
        /// <returns>The <see cref = "Function" />'s string representation.</returns>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return String.Format("ITarget({0})", Function);
        }

        #region Fields

        private readonly PFunction _function;
        private readonly Loader _loader;
        private readonly CompilerTarget _parentTarget;

        private MacroSession _macroSession;
        private int _macroSessionReferenceCounter;

        /// <summary>
        ///     Returns the current macro session, or creates one if necessary. Must always be paired with a call to <see
        ///      cref = "ReleaseMacroSession" />. Do not call <see cref = "MacroSession.Dispose" />.
        /// </summary>
        /// <returns>The current macro session.</returns>
        public MacroSession AcquireMacroSession()
        {
            _macroSessionReferenceCounter++;
            Debug.Assert(_macroSessionReferenceCounter > 0);
            return _macroSession ?? (_macroSession = new MacroSession(this));
        }

        /// <summary>
        ///     Releases the macro session acquired via <see cref = "AcquireMacroSession" />. Will dispose of the session, if no other release is pending.
        /// </summary>
        /// <param name = "acquiredSession">A session previously acquired through <see cref = "AcquireMacroSession" />.</param>
// ReSharper disable UnusedParameter.Global
        public void ReleaseMacroSession(MacroSession acquiredSession)
// ReSharper restore UnusedParameter.Global
        {
            if (_macroSession != acquiredSession)
                throw new InvalidOperationException(
                    "Invalid call to CompilerTarget.ReleaseMacroSession. Trying to release macro session that doesn't match.");
            _macroSessionReferenceCounter--;
            if (_macroSessionReferenceCounter <= 0)
            {
                _macroSession.Dispose();
                _macroSession = null;
            }

            Debug.Assert(_macroSessionReferenceCounter >= 0);
            Debug.Assert(_macroSessionReferenceCounter == 0 || _macroSession != null);
        }

        public Loader Loader
        {
            [DebuggerStepThrough]
            get { return _loader; }
        }

        public PFunction Function
        {
            [DebuggerStepThrough]
            get { return _function; }
        }

        public CompilerTarget ParentTarget
        {
            [DebuggerStepThrough]
            get { return _parentTarget; }
        }

        private int _nestedIdCounter;

        #endregion

        #region Construction

        [DebuggerStepThrough]
        public CompilerTarget(Loader loader, PFunction function, CompilerTarget parentTarget = null, ISourcePosition position = null)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");
            if (function == null)
                function = loader.ParentApplication.CreateFunction();
            if (!ReferenceEquals(function.ParentApplication, loader.ParentApplication))
                throw new ArgumentException(
                    "When creating a compiler target, the supplied function must match the application targetted by the loader.",
                    "function");

            _loader = loader;
            _function = function;
            _parentTarget = parentTarget;

            _ast = AstBlock.CreateRootBlock(position ?? new SourcePosition("",-1,-1),  SymbolStore.Create(parentTarget == null ? loader.Symbols : parentTarget.Symbols),
                                            AstBlock.RootBlockName, Guid.NewGuid().ToString("N"));
        }

        #endregion

        #region Macro system

        /// <summary>
        ///     Setup function as macro (symbol declarations etc.)
        /// </summary>
        public void SetupAsMacro()
        {
            if (!_function.Meta.ContainsKey(MacroMetaKey))
                _function.Meta[MacroMetaKey] = true;

            if (!_function.Meta.ContainsKey(CompilerMetakey))
                _function.Meta[CompilerMetakey] = true;

            //If you change something in this list, it must also be changed in
            // AstMacroInvocation.cs (method EmitCode).

            foreach (var localRefId in MacroAliases.Aliases())
            {
                Ast.Symbols.Declare(localRefId, new EntitySymbol(EntityRef.Variable.Local.Create(localRefId), true));
                _outerVariables.Add(localRefId);
                //remember: outer variables are not added as local variables
            }
        }

        /// <summary>
        ///     The boolean macro meta key indicates that a function is a macro and to be executed at compile time.
        /// </summary>
        public const string MacroMetaKey = @"\macro";

        public bool IsMacro
        {
            get { return Meta.GetDefault(MacroMetaKey, false).Switch; }
            set { Meta[MacroMetaKey] = value; }
        }

        /// <summary>
        ///     The boolean compiler meta key indicates that a function is part of the compiler and might not work outside of the original loader environment.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Metakey")] public const string CompilerMetakey = "compiler";

        private class ProvidedValue : IIndirectCall
        {
            private readonly PValue _value;

            public ProvidedValue(PValue value)
            {
                _value = value;
            }

            #region Implementation of IIndirectCall

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                return _value;
            }

            #endregion
        }

        private class ProvidedFunction : IIndirectCall
        {
            private readonly Func<StackContext, PValue[], PValue> _func;

            public ProvidedFunction(Func<StackContext, PValue[], PValue> func)
            {
                _func = func;
            }

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                return _func(sctx, args);
            }
        }

        /// <summary>
        ///     Creates a PVariable object that contains a reference to the supplied value.
        /// </summary>
        /// <param name = "value">The value to reference.</param>
        /// <returns>A PVariable object that contains a reference to the supplied value (needs to be de-referenced)</returns>
        public static PVariable CreateReadonlyVariable(PValue value)
        {
            return new PVariable {Value = PType.Object.CreatePValue(new ProvidedValue(value))};
        }

        public static PValue CreateFunctionValue(Func<StackContext, PValue[], PValue> implementation)
        {
            return new PValue(new ProvidedFunction(implementation),
                PType.Object[typeof (IIndirectCall)]);
        }

        #region Temporary variables

        private readonly Stack<string> _freeTemporaryVariables = new Stack<string>(5);
        private readonly SymbolCollection _usedTemporaryVariables = new SymbolCollection(5);

        public string RequestTemporaryVariable()
        {
            if (_freeTemporaryVariables.Count == 0)
            {
                //Allocate temporary variable
                var tempName = "tmpπ" + _usedTemporaryVariables.Count;
                while (_function.Variables.Contains(tempName))
                    tempName = tempName + "'";
                _function.Variables.Add(tempName);
                _freeTemporaryVariables.Push(tempName);
            }

            var temp = _freeTemporaryVariables.Pop();
            _usedTemporaryVariables.Add(temp);

            return temp;
        }

        public void FreeTemporaryVariable(string temporaryVariableId)
        {
            if (!_usedTemporaryVariables.Contains(temporaryVariableId))
                throw new PrexoniteException("The variable " + temporaryVariableId +
                    " is not a temporary variable managed by " + this);

            _usedTemporaryVariables.Remove(temporaryVariableId);
            _freeTemporaryVariables.Push(temporaryVariableId);
        }

        public void PromoteTemporaryVariable(string temporaryVariableId)
        {
            if (!_usedTemporaryVariables.Contains(temporaryVariableId))
                throw new PrexoniteException("The variable " + temporaryVariableId +
                    " is not a temporary variable managed by " + this);

            _usedTemporaryVariables.Remove(temporaryVariableId);
        }

        #endregion

        #endregion

        #region Symbol Lookup / Combined Symbol Proxy

        public SymbolStore Symbols
        {
            [DebuggerStepThrough]
            get { return CurrentBlock.Symbols; }
        }

        [Obsolete("Use the SymbolStore API to declare module-local symbols.")]
        public void DeclareModuleLocal(SymbolInterpretations interpretation, string physicalName)
        {
            DeclareModuleLocal(interpretation, physicalName, physicalName);
        }

        [Obsolete("Use the SymbolStore API to declare module-local symbols.")]
        public void DeclareModuleLocal(SymbolInterpretations interpretation, string logicalId, string physicalId)
        {
            var symbol =
                new SymbolEntry(interpretation, physicalId, Loader.ParentApplication.Module.Name).ToSymbol();
            Symbols.Declare(logicalId, symbol);
        }

        #endregion

        #region Function.Code forwarding

        public List<Instruction> Code
        {
            [DebuggerStepThrough]
            get { return _function.Code; }
        }

        #endregion

        #region AST

        private readonly AstBlock _ast;

        public AstBlock Ast
        {
            [DebuggerStepThrough]
            get { return _ast; }
        }

        #endregion

        #region Compiler Hooks

        public void ExecuteCompilerHooks()
        {
            foreach (CompilerHook hook in _loader.CompilerHooks)
                hook.Execute(this);
        }

        #endregion

        #region Manipulation

        #region Scope Block Stack

        private readonly Stack<AstSubBlock> _scopeBlocks = new Stack<AstSubBlock>();

        public IEnumerable<AstBlock> ScopeBlocks
        {
            get { return _scopeBlocks; }
        }

        public AstBlock CurrentBlock
        {
            [DebuggerStepThrough]
            get
            {
                if (_scopeBlocks.Count == 0)
                    return _ast;
                else
                    return _scopeBlocks.Peek();
            }
        }

        public AstLoopBlock CurrentLoopBlock
        {
            get
            {
                foreach (var block in _scopeBlocks)
                {
                    var loop = block as AstLoopBlock;
                    if (loop != null)
                        return loop;
                }
                return _ast as AstLoopBlock;
            }
        }

        [DebuggerStepThrough]
        public void BeginBlock(AstSubBlock bl)
        {
            if (bl == null)
                throw new ArgumentNullException("bl");
            _scopeBlocks.Push(bl);
        }

        [DebuggerStepThrough]
        public AstSubBlock BeginBlock(string prefix)
        {
            var currentBlock = CurrentBlock;
            var bl = new AstSubBlock(currentBlock, currentBlock, GenerateLocalId(), prefix);
            _scopeBlocks.Push(bl);
            return bl;
        }

        [DebuggerStepThrough]
        public AstSubBlock BeginBlock()
        {
            return BeginBlock((string) null);
        }

        [DebuggerStepThrough]
        public AstBlock EndBlock()
        {
            if (_scopeBlocks.Count > 0)
                return _scopeBlocks.Pop();
            else
                throw new PrexoniteException("Cannot end root block.");
        }

        #endregion //Scope Block Stack

        #region Code

        /// <summary>
        ///     Safely removes an instruction without invalidating jumps or try-blocks. Notifies <see cref = "AddressChangeHooks" />.
        /// </summary>
        /// <param name = "index">The address of the instruction to remove.</param>
        [DebuggerStepThrough]
        public void RemoveInstructionAt(int index)
        {
            RemoveInstructionRange(index, 1);
        }

        /// <summary>
        ///     Safely remoes a range of instructions without invalidating jumps or try-blocks. Notifies <see
        ///      cref = "AddressChangeHooks" />.
        /// </summary>
        /// <param name = "index">The address of the first instruction to remove.</param>
        /// <param name = "count">The number of instructions to remove.</param>
        public void RemoveInstructionRange(int index, int count)
        {
            var code = Code;
            if (index < 0 || index >= code.Count)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0 || index + count > code.Count)
                throw new ArgumentOutOfRangeException("count");
            if (count == 0)
                return;

            //Remove the instruction
            code.RemoveRange(index, count);

            //Adapt source mapping
            SourceMapping.RemoveRange(index, count);

            //Correct jump targets by...);
            int i;
            for (i = 0; i < code.Count; i++)
            {
                var ins = code[i];
                if ((ins.IsJump || ins.OpCode == OpCode.leave)
                    && ins.Arguments > index) //decrementing target addresses pointing 
                    //behind the removed instruction
                    code[i] = ins.With(arguments: ins.Arguments - count);
            }

            //Correct try-catch-finally blocks
            var modifiedBlocks = new MetaEntry[_function.TryCatchFinallyBlocks.Count];
            i = 0;
            foreach (var block in _function.TryCatchFinallyBlocks)
            {
                if (block.BeginTry > index)
                    block.BeginTry -= count;
                if (block.BeginFinally > index)
                    block.BeginFinally -= count;
                if (block.BeginCatch > index)
                    block.BeginCatch -= count;
                if (block.EndTry > index)
                    block.EndTry -= count;

                if (!block.IsValid)
                    throw new PrexoniteException
                        (
                        "The try-catch-finally block (" + block +
                            ") is not valid after optimization.");

                modifiedBlocks[i++] = block;
            }
            _function.Meta[TryCatchFinallyBlock.MetaKey] = (MetaEntry) modifiedBlocks;

            //Change custom addresses into this code (e.g., cil compiler hints)
            foreach (var hook in _addressChangeHooks)
                if (hook.InstructionIndex > index)
                    hook.React(hook.InstructionIndex - count);
        }

        #endregion

        #region Nested function transparency

        private readonly SymbolCollection _outerVariables = new SymbolCollection();

        public SymbolCollection OuterVariables
        {
            [DebuggerStepThrough]
            get { return _outerVariables; }
        }

        /// <summary>
        ///     Requests an outer function to share a variable with this inner function.
        /// </summary>
        /// <param name = "id">The (physical) id of the variable or parameter to require from the outer function.</param>
        /// <exception cref = "PrexoniteException">Outer function(s) don't contain a variable or parameter named <paramref
        ///      name = "id" />.</exception>
        [DebuggerStepThrough]
        public void RequireOuterVariable(string id)
        {
            if (_parentTarget == null)
                throw new PrexoniteException(
                    "Cannot require outer variable from top-level function.");

            _outerVariables.Add(id);
            //Make parent functions hand down the variable, even if they don't use them themselves.

            //for (var T = _parentTarget; T != null; T = T._parentTarget)
            var T = _parentTarget;

            {
                var func = T.Function;
                if (func.Variables.Contains(id) || func.Parameters.Contains(id) ||
                    T.OuterVariables.Contains(id))
                    return; //Parent can supply the variable/parameter. Stop search here.
                else if (T._parentTarget != null)
                    T.RequireOuterVariable(id); //Order parent function to request outer variable
                else
                    throw new PrexoniteException
                        (
                        String.Format
                            (
                                "{0} references outer variable {1} which cannot be supplied by top-level function {2}",
                                Function,
                                id,
                                func));
            }
        }

        #endregion

        #region Lambda lifting (capture by value)

        /// <summary>
        ///     Promotes captured variables to function parameters.
        /// </summary>
        /// <returns>A list of expressions (get symbol) that should be added to the arguments list of any call to the lifted function.</returns>
        internal Func<Parser, IList<AstExpr>> ToCaptureByValue()
        {
            return ToCaptureByValue(new string[0]);
        }

        /// <summary>
        ///     Promotes captured variables to function parameters.
        /// </summary>
        /// <param name = "keepByRef">The set of captured variables that should be kept captured by reference (i.e. not promoted to parameters)</param>
        /// <returns>A list of expressions (get symbol) that should be added to the arguments list of any call to the lifted function.</returns>
        internal Func<Parser, IList<AstExpr>> ToCaptureByValue(IEnumerable<string> keepByRef)
        {
            keepByRef = new HashSet<string>(keepByRef);
            var toPromote =
                _outerVariables.Except(keepByRef).ToList();

            //Declare locally, remove from outer variables and add as parameter to the end
            var exprs = new List<Func<Parser, AstExpr>>();
            foreach (var outer in toPromote)
            {
                Symbol sym;
                if(Symbols.TryGet(outer, out sym))
                    Symbols.Declare(outer, sym);
                _outerVariables.Remove(outer);
                Function.Parameters.Add(outer);
                {
                    //Copy the value for capture by value in closure
                    var byValOuter = outer;
                    exprs.Add(
                        p =>
                            new AstGetSetSymbol(p, SymbolEntry.LocalObjectVariable(byValOuter)));
                }
            }

            return p => exprs.Select(e => e(p)).ToList();
        }

        #endregion

        #endregion

        #region Source Mapping

        private readonly SourceMapping _sourceMapping = new SourceMapping();

        public SourceMapping SourceMapping
        {
            [DebuggerStepThrough]
            get { return _sourceMapping; }
        }

        #endregion

        #region Emitting Instructions

        #region Low Level

        public void Emit(ISourcePosition position, Instruction ins)
        {
            var index = Function.Code.Count;
            if (ins.Id != null)
                ins = ins.With(id: Loader.CacheString(ins.Id));

            Function.Code.Add(ins);
            SourceMapping.Add(index, position);
        }

        [DebuggerStepThrough]
        public void Emit(ISourcePosition position, OpCode code)
        {
            Emit(position, new Instruction(code));
        }

        [DebuggerStepThrough]
        public void Emit(ISourcePosition position, OpCode code, string id)
        {
            Emit(position, new Instruction(code, id));
        }

        [DebuggerStepThrough]
        public void Emit(ISourcePosition position, OpCode code, int arguments)
        {
            Emit(position, new Instruction(code, arguments));
        }

        [DebuggerStepThrough]
        public void Emit(ISourcePosition position, OpCode code, int arguments, string id)
        {
            Emit(position, new Instruction(code, arguments, id));
        }

        [DebuggerStepThrough]
        public void Emit(ISourcePosition position, OpCode code, string id, ModuleName modueName)
        {
            Emit(position, code, 0, id, modueName);
        }

        [DebuggerStepThrough]
        public void Emit(ISourcePosition position, OpCode code, int arguments, string id, ModuleName modueName)
        {
            Emit(position, new Instruction(code,arguments,id,modueName));
        }

        #endregion //Low Level

        #region High Level

        #region Constants

        public void EmitConstant(ISourcePosition position, ModuleName moduleName)
        {
            Emit(position, Instruction.CreateConstant(moduleName));
        }

        public void EmitConstant(ISourcePosition position, string value)
        {
            Emit(position, Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitConstant(ISourcePosition position, bool value)
        {
            Emit(position, Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitConstant(ISourcePosition position, double value)
        {
            Emit(position, Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitConstant(ISourcePosition position, int value)
        {
            Emit(position, Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitNull(ISourcePosition position)
        {
            Emit(position, Instruction.CreateNull());
        }

        #endregion

        #region Operators

        #endregion

        #region Variables

        [DebuggerStepThrough]
        public void EmitLoadLocal(ISourcePosition position, string id)
        {
            Emit(position, Instruction.CreateLoadLocal(id));
        }

        public void EmitStoreLocal(ISourcePosition position, string id)
        {
            Emit(position, Instruction.CreateStoreLocal(id));
        }

        public void EmitLoadGlobal(ISourcePosition position, string id, ModuleName moduleName)
        {
            if (moduleName == Loader.ParentApplication.Module.Name)
                moduleName = null;
            Emit(position, Instruction.CreateLoadGlobal(id, moduleName));
        }

        public void EmitStoreGlobal(ISourcePosition position, string id, ModuleName moduleName)
        {
            if (moduleName == Loader.ParentApplication.Module.Name)
                moduleName = null;
            Emit(position, Instruction.CreateStoreGlobal(id, moduleName));
        }

        #endregion

        #region Get/Set

        [DebuggerStepThrough]
        public void EmitGetCall(ISourcePosition position, int args, string id, bool justEffect)
        {
            Emit(position, Instruction.CreateGetCall(args, id, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitGetCall(ISourcePosition position, int args, string id)
        {
            EmitGetCall(position, args, id, false);
        }

        [DebuggerStepThrough]
        public void EmitSetCall(ISourcePosition position, int args, string id)
        {
            Emit(position, Instruction.CreateSetCall(args, id));
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(ISourcePosition position, int args, string callExpr,
            bool justEffect)
        {
            Emit(position, Instruction.CreateStaticGetCall(args, callExpr, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(ISourcePosition position, int args, string callExpr)
        {
            EmitStaticGetCall(position, args, callExpr, false);
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(ISourcePosition position, int args, string typeId,
            string memberId, bool justEffect)
        {
            Emit(position, Instruction.CreateStaticGetCall(args, typeId, memberId, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(ISourcePosition position, int args, string typeId,
            string memberId)
        {
            EmitStaticGetCall(position, args, typeId, memberId, false);
        }

        [DebuggerStepThrough]
        public void EmitStaticSetCall(ISourcePosition position, int args, string callExpr)
        {
            Emit(position, Instruction.CreateStaticSetCall(args, callExpr));
        }

        [DebuggerStepThrough]
        public void EmitStaticSet(ISourcePosition position, int args, string typeId, string memberId)
        {
            Emit(position, Instruction.CreateStaticSetCall(args, typeId, memberId));
        }

        [DebuggerStepThrough]
        public void EmitIndirectCall(ISourcePosition position, int args, bool justEffect)
        {
            Emit(position, Instruction.CreateIndirectCall(args, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitIndirectCall(ISourcePosition position, int args)
        {
            Emit(position, Instruction.CreateIndirectCall(args));
        }

        #endregion //Get/Set

        #region Functions/Commands

        [DebuggerStepThrough]
        public void EmitFunctionCall(ISourcePosition position, int args, string id, ModuleName moduleName)
        {
            EmitFunctionCall(position, args, id, moduleName, false);
        }

        [DebuggerStepThrough]
        public void EmitFunctionCall(ISourcePosition position, int args, string id, ModuleName moduleName, bool justEffect)
        {
            if(moduleName == Loader.ParentApplication.Module.Name)
                moduleName = null;
            Emit(position, Instruction.CreateFunctionCall(args, id, justEffect, moduleName));
        }

        [DebuggerStepThrough]
        public void EmitCommandCall(ISourcePosition position, int args, string id)
        {
            EmitCommandCall(position, args, id, false);
        }

        [DebuggerStepThrough]
        public void EmitCommandCall(ISourcePosition position, int args, string id, bool justEffect)
        {
            Emit(position, Instruction.CreateCommandCall(args, id, justEffect));
        }

        #endregion //Functions/Commands

        #region Stack manipulation

        public void EmitExchange(ISourcePosition position)
        {
            Emit(position, Instruction.CreateExchange());
        }

        public void EmitRotate(ISourcePosition position, int rotations)
        {
            Emit(position, Instruction.CreateRotate(rotations));
        }

        public void EmitRotate(ISourcePosition position, int rotations, int instructions)
        {
            Emit(position, Instruction.CreateRotate(rotations, instructions));
        }

        public void EmitPop(ISourcePosition position, int values)
        {
            Emit(position, Instruction.CreatePop(values));
        }

        public void EmitPop(ISourcePosition position)
        {
            EmitPop(position, 1);
        }

        public void EmitDuplicate(ISourcePosition position, int copies)
        {
            Emit(position, Instruction.CreateDuplicate(copies));
        }

        public void EmitDuplicate(ISourcePosition position)
        {
            Emit(position, Instruction.CreateDuplicate());
        }

        #endregion

        #region Jumps and Labels

        private readonly HashSet<int> _unresolvedInstructions = new HashSet<int>();

        public void EmitLeave(ISourcePosition position, int address)
        {
            var ins = new Instruction(OpCode.leave, address);
            Emit(position, ins);
        }

        public void EmitJump(ISourcePosition position, int address)
        {
            var ins = Instruction.CreateJump(address);
            Emit(position, ins);
        }

        public void EmitLeave(ISourcePosition position, int address, string label)
        {
            var ins = new Instruction(OpCode.leave, address, label);
            Emit(position, ins);
        }

        public void EmitJump(ISourcePosition position, int address, string label)
        {
            var ins = Instruction.CreateJump(address, label);
            Emit(position, ins);
        }

        public void EmitJumpIfTrue(ISourcePosition position, int address)
        {
            Emit(position, Instruction.CreateJumpIfTrue(address));
        }

        public void EmitJumpIfTrue(ISourcePosition position, int address, string label)
        {
            Emit(position, Instruction.CreateJumpIfTrue(address, label));
        }

        public void EmitJumpIfFalse(ISourcePosition position, int address)
        {
            Emit(position, Instruction.CreateJumpIfFalse(address));
        }

        public void EmitJumpIfFalse(ISourcePosition position, int address, string label)
        {
            Emit(position, Instruction.CreateJumpIfFalse(address, label));
        }

        public void EmitLeave(ISourcePosition position, string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitLeave(position, address, label);
            }
            else
            {
                var ins = new Instruction(OpCode.leave, label);
                _unresolvedInstructions.Add(NextAddress);
                Emit(position, ins);
            }
        }

        protected int NextAddress
        {
            get { return Function.Code.Count; }
        }

        public void EmitJump(ISourcePosition position, string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitJump(position, address, label);
            }
            else
            {
                var ins = Instruction.CreateJump(label);
                _unresolvedInstructions.Add(NextAddress);
                Emit(position, ins);
            }
        }

        public void EmitJumpIfTrue(ISourcePosition position, string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitJumpIfTrue(position, address, label);
            }
            else
            {
                var ins = Instruction.CreateJumpIfTrue(label);
                _unresolvedInstructions.Add(NextAddress);
                Emit(position, ins);
            }
        }

        public void EmitJumpIfFalse(ISourcePosition position, string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitJumpIfFalse(position, address, label);
            }
            else
            {
                var ins = Instruction.CreateJumpIfFalse(label);
                _unresolvedInstructions.Add(NextAddress);
                Emit(position, ins);
            }
        }

        private readonly SymbolTable<int> _labels = new SymbolTable<int>(); 

        public bool TryResolveLabel(string label, out int address)
        {
            return _labels.TryGetValue(label, out address);
        }

        [DebuggerStepThrough]
        public string EmitLabel(ISourcePosition position, int address)
        {
            var label = "L\\" + Guid.NewGuid().ToString("N");
            EmitLabel(position, label, address);
            return label;
        }

        /// <summary>
        ///     <para>Adds a new label entry to the symbol table and resolves any symbolic jumps to this label.</para>
        ///     <para>If the destination is an unconditional jump, it's destination address will 
        ///         used instead of the supplied address.</para>
        ///     <para>If the last instruction was a jump (conditional or unconditional) to this label, it 
        ///         is considered redundant and will be removed.</para>
        /// </summary>
        /// <param name = "position">The position in source code where this label originated.</param>
        /// <param name = "label">The label's symbolic name.</param>
        /// <param name = "address">The label's address.</param>
        //[DebuggerStepThrough]
        public void EmitLabel(ISourcePosition position, string label, int address)
        {
            //Safety check
            Debug.Assert(!_labels.ContainsKey(label),
                string.Format("Error, label {0} defined multiple times in {1}, {2}", label, Function,
                    position.File));

            //resolve any unresolved jumps);
            var resolved = new List<int>();
            foreach (var idx in _unresolvedInstructions)
            {
                var ins = Code[idx];
                if (Engine.StringsAreEqual(ins.Id, label))
                {
                    //Found a matching unresolved 

                    //else
                    {
                        Code[idx] = ins.With(arguments: address);
                        resolved.Add(idx);
                    }
                }
            }

            _unresolvedInstructions.ExceptWith(resolved);

            //Add the label to the symbol table
            _labels[label] = address;
        }

        [DebuggerStepThrough]
        public void EmitLabel(ISourcePosition position, string label)
        {
            EmitLabel(position, label, Code.Count);
        }

        [DebuggerStepThrough]
        public string EmitLabel(ISourcePosition position)
        {
            return EmitLabel(position, Code.Count);
        }

        /// <summary>
        ///     Deletes all information about a symbolic label.
        /// </summary>
        /// <param name = "label">The name of the label to delete.</param>
        /// <remarks>
        ///     This method just deletes the symbol table entry for the specified label and does not alter code in any way.
        /// </remarks>
        public void FreeLabel(string label)
        {
            _labels.Remove(label);
        }

        #endregion

        #endregion //Emit High Level

        #endregion //Emitting Instructions

        #region Finishing

        //
        //  This region contains code that is invoked after a first version
        //    of bytecode has been emitted by the AST.
        //

        /// <summary>
        ///     Performs checks and block level optimizations on the target.
        /// </summary>
        /// <remarks>
        ///     Calling FinishTarget is <strong>>not</strong> optional, especially
        ///     for nested functions since they require additional processing.
        /// </remarks>
        public void FinishTarget()
        {
            if (_function.Parameters.Contains(PFunction.ArgumentListId))
            {
                ISourcePosition pos;
                if (Ast.Count == 0)
                    pos = new SourcePosition("-unknown-", -1, -1);
                else
                    pos = Ast[0];
                _loader.ReportMessage(new Message(MessageSeverity.Error,
                    string.Format(
                        "Parameter list of function {0} contains {1} at position {2}. The name {1} is reserved for the local variable holding the argument list.",
                        _function.LogicalId, PFunction.ArgumentListId,
                        _function.Parameters.IndexOf(PFunction.ArgumentListId)), pos));
            }

            _DetermineSharedNames();

            _checkUnresolvedInstructions();

            _unconditionalJumpTargetPropagation();

            _removeJumpsToNextInstruction();

            _jumpReInversion();

            _removeUnconditionalJumpSequences();

            //Do not remove nop instructions!
            //nops used by try-catch-finally with degenerate finally clauses

#if UseIndex
            if (Loader.Options.UseIndicesLocally)
                _byIndex();
#endif

            if (Loader.Options.StoreSourceInformation)
                SourceMapping.Store(Function);
        }

        internal void _DetermineSharedNames()
        {
            var outerVars = new MetaEntry[_outerVariables.Count];
            var i = 0;
            foreach (var outerVar in _outerVariables)
                outerVars[i++] = outerVar;
            if (i > 0)
                Function.Meta[PFunction.SharedNamesKey] = (MetaEntry) outerVars;
        }

        #region Check unresolved Instructions

        private void _checkUnresolvedInstructions()
        {
            //Check for unresolved instructions
            if (_unresolvedInstructions.Count > 0)
                throw new PrexoniteException
                    (
                    "The instruction [ " + Function.Code[_unresolvedInstructions.First()] +
                        " ] has not been resolved.");
        }

        #endregion

        #region Unconditional jump target propagation

        /// <summary>
        ///     Searches for jumps targeting unconditional jumps and propagates the final target back to the initial jump.
        /// </summary>
        private void _unconditionalJumpTargetPropagation()
        {
            //Unconditional jump target propagation
            var code = Code;
            var count = code.Count;
            var addresses = new bool[count];
            for (var i = 0; i < count; i++)
            {
                var current = code[i];
                //Only valid jumps...
                if (!_isValidJump(current, count))
                    continue;

                var target = code[current.Arguments];
                _reset(addresses);
                //...targetting valid unconditional jumps
                while (_isValidUnconditionalJump(target, count))
                {
                    //Mark the address of the unconditional jump for loop detection
                    addresses[current.Arguments] = true;
                    //Check for loop (uncond. jump targets another unconditional jump already visited
                    if (addresses[target.Arguments])
                        throw new PrexoniteException
                            (
                            "Infinite loop in unconditional jump sequence detected.");
                    //Propagate address
                    current = current.With(arguments: target.Arguments, id: target.Id);

                    //Prepare next step
                    if (_targetIsInRange(target, count))
                        target = code[target.Arguments];
                    else
                        break;
                }

                code[i] = current;
            }
        }

        private static void _reset(bool[] addresses)
        {
            for (var i = 0; i < addresses.Length; i++)
                addresses[i] = false;
        }

        private static bool _targetIsInRange(Instruction jump, int count)
        {
            return jump.Arguments >= 0
                && jump.Arguments < count;
        }

        private static bool _isValidJump(Instruction jump, int count)
        {
            return
                (jump.OpCode == OpCode.jump ||
                    jump.OpCode == OpCode.jump_f ||
                        jump.OpCode == OpCode.jump_t ||
                            jump.OpCode == OpCode.leave)
                                && _targetIsInRange(jump, count);
        }

        private static bool _isValidUnconditionalJump(Instruction jump, int count)
        {
            return
                jump.OpCode == OpCode.jump
                    && _targetIsInRange(jump, count);
        }

        #endregion

        #region Remove unconditional jump sequences

        /// <summary>
        ///     Detects and removes consecutive unconditional jumps.
        /// </summary>
        /// <remarks>
        ///     <para>Since all jumps targeting unconditional jumps have been redirected by 
        ///         <see cref = "_unconditionalJumpTargetPropagation" />, unconditional jumps that are preceded by an unconditional jump can no longer be reached directly.</para>
        ///     <code>
        ///         jump.f b
        ///         ...
        ///         jump a
        ///         jump b
        ///         jump c
        ///         ...
        ///         label a
        ///         ...
        ///     </code>
        ///     <para>The above can be shortened to:</para>
        ///     <code>
        ///         jump.f b
        ///         ...
        ///         jump a
        ///         ...
        ///         label a
        ///         ...
        ///     </code>
        /// </remarks>
        private void _removeUnconditionalJumpSequences()
        {
            var code = Code;
            for (var i = 0; i < code.Count; i++)
            {
                var current = code[i];
                if (!current.IsUnconditionalJump)
                    continue;

                var count = 0;
                while ((i + count + 1) < code.Count && code[i + count + 1].IsUnconditionalJump)
                    count++;

                if (count > 0)
                    RemoveInstructionRange(i + 1, count);
                i -= count;
            }
        }

        #endregion

        #region RemoveJumpsToNextInstruction

        /// <summary>
        ///     Detects and removes unconditional jumps to the following instruction.
        /// </summary>
        /// <remarks>
        ///     <code>
        ///         jump b
        ///         label b
        ///         ...
        ///     </code> is shortened to <code>
        ///                                 ...
        ///                             </code>
        /// </remarks>
        private void _removeJumpsToNextInstruction()
        {
            var code = Code;
            for (var i = 0; i < code.Count; i++)
            {
                var ins = code[i];
                if (ins.Arguments == i + 1)
                {
                    if (ins.IsUnconditionalJump)
                    {
                        RemoveInstructionAt(i--);
                    }
                    else if (ins.IsConditionalJump)
                    {
                        //This is something of the form
                        //  <bool expr>
                        //  jump.t n
                        //  label n
                        //  ...
                        throw new PrexoniteException
                            (
                            "Redundant conditional jump to following instruction at address " +
                                i);
                    }
                }
            }
        }

        #endregion

        #region Jump re-inversion

        /// <summary>
        ///     Detects conditional jumps skipping unconditional jumps and combines them into an inverted conditional jump.
        /// </summary>
        /// <remarks>
        ///     <code>
        ///         jump.f  after
        ///         jump    somewhere
        ///         label   after
        ///     </code><para>is equal to</para>
        ///     <code>
        ///         jump.t  somewhere
        ///     </code>
        /// </remarks>
        private void _jumpReInversion()
        {
            var code = Code;
            for (var i = 0; i < code.Count - 1; i++)
            {
                var condJ = code[i];
                //jump, skipping the next instruction
                if (!(condJ.IsJump && condJ.Arguments == i + 2))
                    continue;
                var uncondJ = code[i + 1];
                //Unconditional jump
                if (!(uncondJ.IsUnconditionalJump))
                    continue;
                /*  jump.f  after
                 *  jump    somewhere
                 *  label   after
                 * 
                 * is equal to
                 * 
                 *  jump.t  somewhere
                 */
                if (condJ.IsConditionalJump)
                {
                    code[i] = new Instruction(Instruction.InvertJumpCondition(condJ.OpCode), uncondJ.Arguments, uncondJ.Id);
                    RemoveInstructionAt(i + 1);
                }
                else
                {
                    RemoveInstructionRange(i, 2);
                }
            }
        }

        #endregion

        #region Removal of nop's (only RELEASE) *not anymore*

        //Do not remove nop instructions!
        //nops used by try-catch-finally with degenerate finally clauses

        #endregion

        #region Replacement of 'by name' instructions for local variables

#if UseIndex

        /// <summary>
        ///     Replaces by-name opcodes with by-index ones. Ignores variables with no mapping.
        /// </summary>
        private void _byIndex()
        {
            //Exclude the initialization function from this optimization
            // as its symbol table keeps changing as more code files
            // are loaded into the VM.
            if (Engine.StringsAreEqual(Function.Id, Application.InitializationId))
                return;

            var code = Function.Code;

            var map = Function.LocalVariableMapping;
            if (map == null)
                throw new PrexoniteException("Local variable mapping of function " + Function.Id +
                    " does not exist.");

            for (var i = 0; i < code.Count; i++)
            {
                var ins = code[i];
                OpCode nopc;
                int idx;
                switch (ins.OpCode)
                {
                    case OpCode.ldloc:
                        nopc = OpCode.ldloci;
                        goto replaceInt;
                    case OpCode.stloc:
                        nopc = OpCode.stloci;
                        goto replaceInt;
                    case OpCode.incloc:
                        nopc = OpCode.incloci;
                        goto replaceInt;
                    case OpCode.decloc:
                        nopc = OpCode.decloci;
                        goto replaceInt;
                    case OpCode.ldr_loc:
                        nopc = OpCode.ldr_loci;
                        replaceInt:
                        if (ins.Id == null)
                            throw new PrexoniteException(
                                string.Format(
                                    "Invalid instruction ({1}) in function {0}. Id missing.",
                                    Function.Id, ins));
                        if (!map.TryGetValue(ins.Id, out idx))
                            continue;
                        code[i] = new Instruction(nopc, idx);
                        break;
                    case OpCode.indloc:
                        if (!map.TryGetValue(ins.Id, out idx))
                            continue;
                        var argc = ins.Arguments;
                        code[i] = Instruction.CreateIndLocI(idx, argc, ins.JustEffect);
                        break;
                }
            }
        }

#endif

        #endregion

        #endregion

        public string GenerateLocalId()
        {
            return GenerateLocalId("");
        }

        public string GenerateLocalId(string prefix)
        {
            if (prefix == null)
                prefix = "";
            return
                Function.Id + "\\" + prefix +
                    (_nestedIdCounter++);
        }

        public ModuleName ToInternalModule(ModuleName moduleName)
        {
            if (moduleName == Function.ParentApplication.Module.Name)
                return null;
            else
                return moduleName;
        }
    }
}