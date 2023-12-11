﻿// Prexonite
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
#if ((!(DEBUG || Verbose)) || forceIndex) && allowIndex
#define UseIndex
#endif

#region Namespace Imports

using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;

#endregion

namespace Prexonite.Compiler;

public class CompilerTarget : IHasMetaTable
{
    readonly LinkedList<AddressChangeHook> _addressChangeHooks =
        new();

    public ICollection<AddressChangeHook> AddressChangeHooks => _addressChangeHooks;

    #region IHasMetaTable Members

    /// <summary>
    ///     Provides access to the <see cref = "Function" />'s metatable.
    /// </summary>
    public MetaTable Meta
    {
        [DebuggerStepThrough]
        get => Function.Meta;
    }

    #endregion

    /// <summary>
    ///     Returns the <see cref = "Function" />'s string representation.
    /// </summary>
    /// <returns>The <see cref = "Function" />'s string representation.</returns>
    [DebuggerStepThrough]
    public override string ToString()
    {
        return $"ITarget({Function})";
    }

    #region Fields

    MacroSession? _macroSession;
    int _macroSessionReferenceCounter;

    /// <summary>
    ///     Returns the current macro session, or creates one if necessary. Must always be paired with a call to <see
    ///      cref = "ReleaseMacroSession" />. Do not call <see cref = "MacroSession.Dispose" />.
    /// </summary>
    /// <returns>The current macro session.</returns>
    public MacroSession AcquireMacroSession()
    {
        _macroSessionReferenceCounter++;
        Debug.Assert(_macroSessionReferenceCounter > 0);
        return _macroSession ??= new(this);
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

    public Loader Loader { [DebuggerStepThrough] get; }

    public PFunction Function { [DebuggerStepThrough] get; }

    public CompilerTarget? ParentTarget { [DebuggerStepThrough] get; }

    int _nestedIdCounter;

    #endregion

    #region Construction

    [PublicAPI]
    [DebuggerStepThrough]
    public CompilerTarget(Loader loader, PFunction? function, CompilerTarget? parentTarget = null, ISourcePosition? position = null)
    {
        if (loader == null)
            throw new ArgumentNullException(nameof(loader));
        function ??= loader.ParentApplication.CreateFunction();
        if (!ReferenceEquals(function.ParentApplication, loader.ParentApplication))
            throw new ArgumentException(
                Resources.CompilerTarget_Cannot_create_for_foreign_function,
                nameof(function));

        Loader = loader;
        Function = function;
        ParentTarget = parentTarget;
        ImportScope = SymbolStore.Create(parentTarget?.Symbols ?? loader.Symbols);
        Ast = AstBlock.CreateRootBlock(position ?? new SourcePosition("",-1,-1),  
            SymbolStore.Create(ImportScope),
            AstBlock.RootBlockName, Guid.NewGuid().ToString("N"));
    }

    #endregion

    #region Macro system

    /// <summary>
    ///     Setup function as macro (self declarations etc.)
    /// </summary>
    public void SetupAsMacro()
    {
        if (!Function.Meta.ContainsKey(MacroMetaKey))
            Function.Meta[MacroMetaKey] = true;

        if (!Function.Meta.ContainsKey(CompilerMetakey))
            Function.Meta[CompilerMetakey] = true;

        //If you change something in this list, it must also be changed in
        // AstMacroInvocation.cs (method EmitCode).

        foreach (var localRefId in MacroAliases.Aliases())
        {
            Ast.Symbols.Declare(localRefId,
                Symbol.CreateDereference(
                    Symbol.CreateCall(EntityRef.Variable.Local.Create(localRefId), NoSourcePosition.Instance)));
            OuterVariables.Add(localRefId);
            //remember: outer variables are not added as local variables
        }
    }

    /// <summary>
    ///     The boolean macro meta key indicates that a function is a macro and to be executed at compile time.
    /// </summary>
    public const string MacroMetaKey = @"\macro";

    [PublicAPI]
    public bool IsMacro
    {
        get => Meta.GetDefault(MacroMetaKey, false).Switch;
        set => Meta[MacroMetaKey] = value;
    }

    /// <summary>
    ///     The boolean compiler meta key indicates that a function is part of the compiler and might not work outside of the original loader environment.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Metakey")] public const string CompilerMetakey = "compiler";

    class ProvidedValue : IIndirectCall
    {
        readonly PValue _value;

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

    class ProvidedFunction : IIndirectCall
    {
        readonly Func<StackContext, PValue[], PValue> _func;

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
        return new() {Value = PType.Object.CreatePValue(new ProvidedValue(value))};
    }

    public static PValue CreateFunctionValue(Func<StackContext, PValue[], PValue> implementation)
    {
        return new(new ProvidedFunction(implementation),
            PType.Object[typeof (IIndirectCall)]);
    }

    #region Temporary variables

    readonly Stack<string> _freeTemporaryVariables = new(5);
    readonly SymbolCollection _usedTemporaryVariables = new(5);

    public string RequestTemporaryVariable()
    {
        if (_freeTemporaryVariables.Count == 0)
        {
            //Allocate temporary variable
            var tempName = "tmpπ" + _usedTemporaryVariables.Count;
            while (Function.Variables.Contains(tempName))
                tempName += "'";
            Function.Variables.Add(tempName);
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
        get => CurrentBlock.Symbols;
    }

    #endregion

    #region Function.Code forwarding

    public List<Instruction> Code
    {
        [DebuggerStepThrough]
        get => Function.Code;
    }

    #endregion

    #region AST

    public AstBlock Ast { [DebuggerStepThrough] get; }

    CompilerTargetAstFactory? _factory;

    public IAstFactory Factory
    {
        get
        {
            if (_factory == null)
            {
                lock (this)
                {
                    _factory ??= new(this);
                }
            }
            return _factory;
        }
    }

    class CompilerTargetAstFactory : AstFactoryBase
    {
        readonly CompilerTarget _target;

        public CompilerTargetAstFactory(CompilerTarget target)
        {
            _target = target;
        }

        protected override AstBlock CurrentBlock => _target.CurrentBlock;

        protected override AstGetSet CreateNullNode(ISourcePosition position)
        {
            return IndirectCall(position, Null(position));
        }

        protected override bool IsOuterVariable(string id)
        {
            return _target._IsOuterVariable(id);
        }

        protected override void RequireOuterVariable(string id)
        {
            _target.RequireOuterVariable(id);
        }

        public override void ReportMessage(Message message)
        {
            _target.Loader.ReportMessage(message);
        }

        protected override CompilerTarget CompileTimeExecutionContext => _target;
    }

    #endregion

    #region Compiler Hooks

    public void ExecuteCompilerHooks()
    {
        foreach (CompilerHook hook in Loader.CompilerHooks)
            hook.Execute(this);
    }

    #endregion

    #region Manipulation

    #region Scope Block Stack

    readonly Stack<AstScopedBlock> _scopeBlocks = new();

    public IEnumerable<AstBlock> ScopeBlocks => _scopeBlocks;

    public AstBlock CurrentBlock
    {
        [DebuggerStepThrough]
        get
        {
            if (_scopeBlocks.Count == 0)
                return Ast;
            else
                return _scopeBlocks.Peek();
        }
    }

    public AstLoopBlock? CurrentLoopBlock
    {
        get
        {
            foreach (var block in _scopeBlocks)
            {
                if (block is AstLoopBlock loop)
                    return loop;
            }
            return Ast as AstLoopBlock;
        }
    }

    [PublicAPI]
    [DebuggerStepThrough]
    public void BeginBlock(AstScopedBlock bl)
    {
        if (bl == null)
            throw new ArgumentNullException(nameof(bl));
        _scopeBlocks.Push(bl);
    }

    [PublicAPI]
    [DebuggerStepThrough]
    public AstScopedBlock BeginBlock(string? prefix)
    {
        var currentBlock = CurrentBlock;
        var bl = new AstScopedBlock(currentBlock.Position, currentBlock, GenerateLocalId(), prefix);
        _scopeBlocks.Push(bl);
        return bl;
    }

    [PublicAPI]
    [DebuggerStepThrough]
    public AstScopedBlock BeginBlock()
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        return BeginBlock((string?) null);
    }

    [DebuggerStepThrough]
    public AstScopedBlock EndBlock()
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
    ///     Safely removes a range of instructions without invalidating jumps or try-blocks. Notifies <see
    ///      cref = "AddressChangeHooks" />.
    /// </summary>
    /// <param name = "index">The address of the first instruction to remove.</param>
    /// <param name = "count">The number of instructions to remove.</param>
    public void RemoveInstructionRange(int index, int count)
    {
        var code = Code;
        if (index < 0 || index >= code.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (count < 0 || index + count > code.Count)
            throw new ArgumentOutOfRangeException(nameof(count));
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
        var modifiedBlocks = new MetaEntry[Function.TryCatchFinallyBlocks.Count];
        i = 0;
        foreach (var block in Function.TryCatchFinallyBlocks)
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
        Function.Meta[TryCatchFinallyBlock.MetaKey] = (MetaEntry) modifiedBlocks;

        //Change custom addresses into this code (e.g., cil compiler hints)
        foreach (var hook in _addressChangeHooks)
            if (hook.InstructionIndex > index)
                hook.React(hook.InstructionIndex - count);
    }

    #endregion

    #region Nested function transparency

    public SymbolCollection OuterVariables { [DebuggerStepThrough] get; } = new();

    /// <summary>
    ///     Requests an outer function to share a variable with this inner function.
    /// </summary>
    /// <param name = "id">The (physical) id of the variable or parameter to require from the outer function.</param>
    /// <exception cref = "PrexoniteException">Outer function(s) don't contain a variable or parameter named <paramref
    ///      name = "id" />.</exception>
    [DebuggerStepThrough]
    public void RequireOuterVariable(string id)
    {
        if (ParentTarget == null)
            throw new PrexoniteException(
                "Cannot require outer variable from top-level function.");

        OuterVariables.Add(id);
        //Make parent functions hand down the variable, even if they don't use them themselves.

        //for (var T = _parentTarget; T != null; T = T._parentTarget)
        var T = ParentTarget;

        {
            var func = T.Function;
            // ReSharper disable RedundantJumpStatement
            if (func.Variables.Contains(id) || func.Parameters.Contains(id) ||
                T.OuterVariables.Contains(id))
                return; //Parent can supply the variable/parameter. Stop search here.
            else if (T.ParentTarget != null)
                T.RequireOuterVariable(id); //Order parent function to request outer variable
            else
                throw new PrexoniteException
                (
                    $"{Function} references outer variable {id} which cannot be supplied by top-level function {func}");
            // ReSharper restore RedundantJumpStatement
        }
    }

    internal bool _IsOuterVariable(string id)
    {
        //Check local function
        var func = Function;
        if (func.Variables.Contains(id) || func.Parameters.Contains(id))
            return false;

        //Check parents
        for (var parent = ParentTarget;
             parent != null;
             parent = parent.ParentTarget)
        {
            func = parent.Function;
            if (func.Variables.Contains(id) || func.Parameters.Contains(id) ||
                parent.OuterVariables.Contains(id))
                return true;
        }
        return false;
    }

    #endregion

    #region Lambda lifting (capture by value)

    /// <summary>
    ///     Promotes captured variables to function parameters.
    /// </summary>
    /// <param name = "keepByRef">The set of captured variables that should be kept captured by reference (i.e. not promoted to parameters)</param>
    /// <returns>A list of expressions (get self) that should be added to the arguments list of any call to the lifted function.</returns>
    internal Func<Parser, IList<AstExpr>> _ToCaptureByValue(IEnumerable<string> keepByRef)
    {
        keepByRef = new HashSet<string>(keepByRef);
        var toPromote =
            OuterVariables.Except(keepByRef).ToList();

        //Declare locally, remove from outer variables and add as parameter to the end
        var exprs = new List<Func<Parser, AstExpr>>();
        foreach (var outer in toPromote)
        {
            if(Symbols.TryGet(outer, out var sym))
                Symbols.Declare(outer, sym);
            OuterVariables.Remove(outer);
            Function.Parameters.Add(outer);
            {
                //Copy the value for capture by value in closure
                var byValOuter = outer;
                exprs.Add(
                    p =>
                    {
                        var pos = p.GetPosition();
                        return Factory.IndirectCall(pos,
                            Factory.Reference(pos,
                                Loader.Cache.EntityRefs.GetCached(
                                    EntityRef.Variable.Local.Create(
                                        byValOuter))));
                    }
                );
            }
        }

        return p => exprs.Select(e => e(p)).ToList();
    }

    #endregion

    #endregion

    #region Source Mapping

    public SourceMapping SourceMapping { [DebuggerStepThrough] get; } = new();

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
    public void Emit(ISourcePosition position, OpCode code, string id, ModuleName? moduleName)
    {
        Emit(position, code, 0, id, moduleName);
    }

    [DebuggerStepThrough]
    public void Emit(ISourcePosition position, OpCode code, int arguments, string id, ModuleName? moduleName)
    {
        Emit(position, new Instruction(code,arguments,id,moduleName));
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

    public void EmitLoadGlobal(ISourcePosition position, string id, ModuleName? moduleName)
    {
        if (moduleName == Loader.ParentApplication.Module.Name)
            moduleName = null;
        Emit(position, Instruction.CreateLoadGlobal(id, moduleName));
    }

    public void EmitStoreGlobal(ISourcePosition position, string id, ModuleName? moduleName)
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
    public void EmitFunctionCall(ISourcePosition position, int args,string id, ModuleName? moduleName, bool justEffect = false)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

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

    readonly HashSet<int> _unresolvedInstructions = new();

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
        if (TryResolveLabel(label, out var address))
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

    protected int NextAddress => Function.Code.Count;

    /// <summary>
    /// The scope that import directives transfer their symbols into. Lies between the function's local scope and the surrounding scope.
    /// </summary>
    /// <remarks>
    /// This means that imported symbols shadow the surrounding context but 
    /// any local definitions (even when provided by the compiler) precede imported symbols.
    /// </remarks>
    public SymbolStore ImportScope { get; }

    public void EmitJump(ISourcePosition position, string label)
    {
        if (TryResolveLabel(label, out var address))
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
        if (TryResolveLabel(label, out var address))
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
        if (TryResolveLabel(label, out var address))
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

    readonly SymbolTable<int> _labels = new(); 

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
    ///     <para>Adds a new label entry to the self table and resolves any symbolic jumps to this label.</para>
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
            $"Error, label {label} defined multiple times in {Function}, {position.File}");

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

        //Add the label to the self table
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
    ///     This method just deletes the self table entry for the specified label and does not alter code in any way.
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
        if (Function.Parameters.Contains(PFunction.ArgumentListId))
        {
            ISourcePosition pos;
            if (Ast.Count == 0)
                pos = new SourcePosition("-unknown-", -1, -1);
            else
                pos = Ast[0].Position;
            Loader.ReportMessage(Message.Create(MessageSeverity.Error,
                string.Format(
                    Resources.CompilerTarget_ParameterNameReserved,
                    Function.LogicalId, PFunction.ArgumentListId,
                    Function.Parameters.IndexOf(PFunction.ArgumentListId)), pos,MessageClasses.ParameterNameReserved));
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

        _useInternalIdsWherePossible();

        if (Loader.Options.StoreSourceInformation)
            SourceMapping.Store(Function);
    }

    void _useInternalIdsWherePossible()
    {
        // This method looks at instructions that refer to entities using a module-qualified name.
        //  If the module the target entity resides in is the same module as the one that contains
        //  this function, we have an internal reference. 
        // Internal references are much easier to handle for both the CIL compiler and the interpreter.
        // We thus replace module-aware references with internal references.

        var code = Function.Code;
        for (var i = 0; i < code.Count; i++)
        {
            var inst = code[i];
            switch (inst.OpCode)
            {
                case OpCode.ldr_glob:
                case OpCode.ldr_func:
                case OpCode.ldglob:
                case OpCode.stglob:
                case OpCode.newclo:
                case OpCode.incglob:
                case OpCode.decglob:
                case OpCode.func:
                case OpCode.indglob:
                    if (inst.ModuleName != null && inst.ModuleName.Equals(Loader.ParentApplication.Module.Name))
                        code[i] = inst.WithModuleName(null);
                    break;
            }
        }
    }

    internal void _DetermineSharedNames()
    {
        var outerVars = new MetaEntry[OuterVariables.Count];
        var i = 0;
        foreach (var outerVar in OuterVariables)
            outerVars[i++] = outerVar;
        if (i > 0)
            Function.Meta[PFunction.SharedNamesKey] = (MetaEntry) outerVars;
    }

    #region Check unresolved Instructions

    void _checkUnresolvedInstructions()
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
    void _unconditionalJumpTargetPropagation()
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

    static void _reset(bool[] addresses)
    {
        for (var i = 0; i < addresses.Length; i++)
            addresses[i] = false;
    }

    static bool _targetIsInRange(Instruction jump, int count)
    {
        return jump.Arguments >= 0
            && jump.Arguments < count;
    }

    static bool _isValidJump(Instruction jump, int count)
    {
        return
            (jump.OpCode == OpCode.jump ||
                jump.OpCode == OpCode.jump_f ||
                jump.OpCode == OpCode.jump_t ||
                jump.OpCode == OpCode.leave)
            && _targetIsInRange(jump, count);
    }

    static bool _isValidUnconditionalJump(Instruction jump, int count)
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
    void _removeUnconditionalJumpSequences()
    {
        var code = Code;
        for (var i = 0; i < code.Count; i++)
        {
            var current = code[i];
            if (!current.IsUnconditionalJump)
                continue;

            var count = 0;
            while (i + count + 1 < code.Count && code[i + count + 1].IsUnconditionalJump)
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
    void _removeJumpsToNextInstruction()
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
    void _jumpReInversion()
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
            if (!uncondJ.IsUnconditionalJump)
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
                code[i] = new(Instruction.InvertJumpCondition(condJ.OpCode), uncondJ.Arguments, uncondJ.Id);
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
            // as its table keeps changing as more code files are loaded into the VM.
            if (Engine.StringsAreEqual(Function.Id, Application.InitializationId))
                return;

            var code = Function.Code;

            Function.Declaration.CreateLocalVariableMapping();
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
                                String.Format(
                                    "Invalid instruction ({1}) in function {0}. Id missing.",
                                    Function.Id, ins));
                        if (!map.TryGetValue(ins.Id, out idx))
                            continue;
                        code[i] = new Instruction(nopc, idx);
                        break;
                    case OpCode.indloc:
                        if (!map.TryGetValue(ins.Id!, out idx))
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

    public string GenerateLocalId(string? prefix)
    {
        prefix ??= "";
        return
            Function.Id + "\\" + prefix +
            _nestedIdCounter++;
    }

    public ModuleName? ToInternalModule(ModuleName moduleName)
    {
        return moduleName == Function.ParentApplication.Module.Name ? null : moduleName;
    }

}