// /*
//  * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
//  *  Copyright (C) 2007  Christian "SealedSun" Klauser
//  *  E-mail  sealedsun a.t gmail d.ot com
//  *  Web     http://www.sealedsun.ch/
//  *
//  *  This program is free software; you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation; either version 2 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
//  * 
//  *  This program is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License along
//  *  with this program; if not, write to the Free Software Foundation, Inc.,
//  *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//  */
#if ((!(DEBUG || Verbose)) || forceIndex) && allowIndex
#define UseIndex
#endif

#region Namespace Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite.Compiler
{
    public class CompilerTarget : IHasMetaTable
    {
        private readonly LinkedList<AddressChangeHook> _addressChangeHooks = new LinkedList<AddressChangeHook>();

        public ICollection<AddressChangeHook> AddressChangeHooks
        {
            get { return _addressChangeHooks; }
        }

        #region IHasMetaTable Members

        /// <summary>
        /// Provides access to the <see cref="Function"/>'s metatable.
        /// </summary>
        public MetaTable Meta
        {
            [DebuggerStepThrough]
            get { return _function.Meta; }
        }

        #endregion

        /// <summary>
        /// Returns the <see cref="Function"/>'s string representation.
        /// </summary>
        /// <returns>The <see cref="Function"/>'s string representation.</returns>
        [DebuggerStepThrough]
        public override string ToString()
        {
            return String.Format("Target({0})", Function);
        }

        #region Fields

        private readonly PFunction _function;
        private readonly Loader _loader;
        private readonly SymbolTable<SymbolEntry> _symbols = new SymbolTable<SymbolEntry>();
        private CompilerTarget _parentTarget;

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

        public SymbolTable<SymbolEntry> LocalSymbols
        {
            [DebuggerStepThrough]
            get { return _symbols; }
        }

        public CompilerTarget ParentTarget
        {
            [DebuggerStepThrough]
            get { return _parentTarget; }
            [DebuggerStepThrough]
            set
            {
                _parentTarget = value;
                _combinedSymbolProxy = new CombinedSymbolProxy(this);
            }
        }

        private int _nestedIdCounter;

        #endregion

        #region Construction

        [DebuggerStepThrough]
        public CompilerTarget(Loader loader, PFunction function, AstBlock block)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");
            if (function == null)
                function = new PFunction(loader.Options.TargetApplication);

            _loader = loader;
            _function = function;

            _combinedSymbolProxy = new CombinedSymbolProxy(this);

            _ast = block;
        }

        #endregion

        #region Macro system

        /// <summary>
        /// Setup function as macro (symbol declarations etc.)
        /// </summary>
        public void SetupAsMacro()
        {
            if (!_function.Meta.ContainsKey(MacroMetaKey))
                _function.Meta[MacroMetaKey] = true;

            if (!_function.Meta.ContainsKey(CompilerMetakey))
                _function.Meta[CompilerMetakey] = true;

            //If you change something in this list, it must also be changed in
            // AstMacroInvocation.cs (method EmitCode).
            var providedLocalRefs = new List<string>
            {
                MacroAliases.LoaderAlias,
                MacroAliases.TargetAlias,
                MacroAliases.LocalsAlias,
                MacroAliases.NewLocalVariableAlias,
                MacroAliases.CallTypeAlias,
                MacroAliases.JustEffectAlias,
                MacroAliases.MacroInvocationAlias
            };

            foreach (var localRefId in providedLocalRefs)
            {
                Declare(SymbolInterpretations.LocalReferenceVariable, localRefId);
                _outerVariables.Add(localRefId); //remember: outer variables are not added as local variables
            }
        }

        /// <summary>
        /// The boolean macro meta key indicates that a function is a macro and to be executed at compile time. 
        /// </summary>
        public const string MacroMetaKey = "macro";

        public bool IsMacro
        {
            get { return Meta.GetDefault(MacroMetaKey, false).Switch; }
            set { Meta[MacroMetaKey] = value; }
        }

        /// <summary>
        /// The boolean compiler meta key indicates that a function is part of the compiler and might not work outside of the original loader environment.
        /// </summary>
        public const string CompilerMetakey = "compiler";

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

        private static PVariable _value(CompilerTarget target, Object value)
        {
            return new PVariable {Value = PType.Object.CreatePValue(new ProvidedValue(target.Loader.CreateNativePValue(value)))};
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

        private static PVariable _func(Func<StackContext, PValue[], PValue> func)
        {
            return new PVariable {Value = PType.Object.CreatePValue(new ProvidedFunction(func))};
        }

        public static SymbolTable<PVariable> CreateEnvironment(CompilerTarget target, AstMacroInvocation invocation, bool justEffect)
        {
            return new SymbolTable<PVariable>
            {
                {MacroAliases.LoaderAlias, _value(target, target.Loader)},
                {MacroAliases.TargetAlias, _value(target, target)},
                {MacroAliases.LocalsAlias, _value(target, target.Function.Variables)},
                {MacroAliases.NewLocalVariableAlias, _func(_makeNewLocalVariableFunction(target))},
                {MacroAliases.CallTypeAlias, _value(target, invocation.Call)},
                {MacroAliases.JustEffectAlias, _value(target, justEffect)},
                {MacroAliases.MacroInvocationAlias, _value(target, invocation)}
            };
        }

        private static Func<StackContext, PValue[], PValue> _makeNewLocalVariableFunction(CompilerTarget target)
        {
            return (sctx, args) =>
            {
                string varId;
                IAstExpression init;

                if (args.Length == 0)
                {
                    varId = null;
                    init = null;
                }
                else
                {
                    var idArg = args[0];
                    var initArg = args.Length >= 2 ? args[1] : null;
                    //Check for string arg
                    varId = idArg.Value as string;
                    if (varId == null)
                        initArg = idArg;

                    init = initArg != null ? initArg.Value as IAstExpression : null;
                }

                varId = varId ?? Engine.GenerateName();

                if (!target.Function.Variables.Contains(varId))
                    target.Function.Variables.Add(varId);

                if (init == null)
                {
                    return varId;
                }
                else
                {
                    var varAssign = new AstGetSetSymbol(
                        "MacroInvocation", -1, -1, PCall.Set, varId, SymbolInterpretations.LocalObjectVariable);
                    varAssign.Arguments.Add(init);
                    return target.Loader.CreateNativePValue(init);
                }
            };
        }

        #endregion

        #region Temporary variables

        private readonly Stack<string> _freeTemporaryVariables = new Stack<string>(5);
        private readonly SymbolCollection _usedTemporaryVariables = new SymbolCollection(5);

        public string RequestTemporaryVariable()
        {
            if (_freeTemporaryVariables.Count == 0)
            {
                //Allocate temporary variable
                var tempName = Engine.GenerateName("tmp" + _usedTemporaryVariables.Count);
                _function.Variables.Add(tempName);
                _freeTemporaryVariables.Push(tempName);
            }

            var temp = _freeTemporaryVariables.Pop();
            _usedTemporaryVariables.Add(temp);

            return temp;
        }

        public void ReleaseTemporaryVariable(string temporaryVariableId)
        {
            if (!_usedTemporaryVariables.Contains(temporaryVariableId))
                throw new PrexoniteException("The variable " + temporaryVariableId + " is not a temporary variable managed by " + this);

            _usedTemporaryVariables.Remove(temporaryVariableId);
            _freeTemporaryVariables.Push(temporaryVariableId);
        }

        #endregion

        #region Symbol Lookup / Combined Symbol Proxy

        private CombinedSymbolProxy _combinedSymbolProxy;

        public CombinedSymbolProxy Symbols
        {
            [DebuggerStepThrough]
            get { return _combinedSymbolProxy; }
        }

        //[DebuggerStepThrough]
        public sealed class CombinedSymbolProxy : IDictionary<string, SymbolEntry>
        {
            private readonly SymbolTable<SymbolEntry> _loaderSymbols;
            private readonly SymbolTable<SymbolEntry> _symbols;
            private readonly CompilerTarget _outer;

            internal CombinedSymbolProxy(CompilerTarget outer)
            {
                _symbols = outer._symbols;
                _outer = outer;
                _loaderSymbols = outer._loader.Symbols;
            }

            private CombinedSymbolProxy _parent
            {
                get { return _outer._parentTarget != null ? _outer._parentTarget.Symbols : null; }
            }

            #region IDictionary<string,SymbolEntry> Members

            public SymbolEntry this[string key]
            {
                get
                {
                    SymbolEntry entry;
                    if (_symbols.TryGetValue(key, out entry))
                        return entry;
                    else if (_parent != null)
                        return _parent[key];
                    else
                        return _loaderSymbols[key];
                }
                set { _symbols[key] = value; }
            }

            public void Add(string key, SymbolEntry value)
            {
                _symbols.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                if (_symbols.ContainsKey(key))
                    return true;
                else if (_parent != null)
                    return _parent.ContainsKey(key);
                else
                    return _loaderSymbols.ContainsKey(key);
            }

            public ICollection<string> Keys
            {
                get
                {
                    var localKeys = _symbols.Keys;
                    var keys = new SymbolCollection(localKeys);
                    if (_parent != null)
                    {
                        foreach (var key in _parent.Keys)
                            if (!localKeys.Contains(key))
                                keys.Add(key);
                    }
                    else
                    {
                        foreach (var key in _loaderSymbols.Keys)
                            if (!localKeys.Contains(key))
                                keys.Add(key);
                    }
                    return keys;
                }
            }

            public bool Remove(string key)
            {
                return _symbols.Remove(key);
            }

            public bool TryGetValue(string key, out SymbolEntry value)
            {
                if (_symbols.TryGetValue(key, out value))
                    return true;
                else if (_parent != null)
                    return _parent.TryGetValue(key, out value);
                else
                    return _loaderSymbols.TryGetValue(key, out value);
            }

            public ICollection<SymbolEntry> Values
            {
                get
                {
                    var localValues = _symbols.Values;
                    var values = new List<SymbolEntry>(localValues);
                    if (_parent != null)
                    {
                        foreach (var kvp in _parent)
                            if (!localValues.Contains(kvp.Value))
                                values.Add(kvp.Value);
                    }
                    else
                    {
                        foreach (var kvp in _loaderSymbols)
                            if (!localValues.Contains(kvp.Value))
                                values.Add(kvp.Value);
                    }
                    return values;
                }
            }

            public void Add(KeyValuePair<string, SymbolEntry> item)
            {
                _symbols.Add(item);
            }

            public void Clear()
            {
                _symbols.Clear();
            }

            public bool Contains(KeyValuePair<string, SymbolEntry> item)
            {
                if (_symbols.Contains(item))
                    return true;
                else if (_parent != null)
                    return _parent.Contains(item);
                else
                    return _loaderSymbols.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, SymbolEntry>[] array, int arrayIndex)
            {
                var lst =
                    new List<KeyValuePair<string, SymbolEntry>>(_symbols);
                if (_parent != null)
                {
                    foreach (var kvp in _parent)
                        if (!_symbols.ContainsKey(kvp.Key))
                            lst.Add(kvp);
                }
                else
                {
                    foreach (var kvp in _loaderSymbols)
                        if (!_symbols.ContainsKey(kvp.Key))
                            lst.Add(kvp);
                }
                lst.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return Keys.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<string, SymbolEntry> item)
            {
                return _symbols.Remove(item);
            }

            IEnumerator<KeyValuePair<string, SymbolEntry>>
                IEnumerable<KeyValuePair<string, SymbolEntry>>.GetEnumerator()
            {
                foreach (var kvp in _symbols)
                    yield return kvp;
                if (_parent != null)
                {
                    foreach (var kvp in _parent)
                        if (!_symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
                else
                {
                    foreach (var kvp in _loaderSymbols)
                        if (!_symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (var kvp in _symbols)
                    yield return kvp;
                if (_parent != null)
                {
                    foreach (var kvp in _parent)
                        if (!_symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
                else
                {
                    foreach (var kvp in _loaderSymbols)
                        if (!_symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
            }

            #endregion

            public bool IsKeyDefinedLocally(string key)
            {
                return _symbols.ContainsKey(key);
            }

            public bool IsKeyDefinedInParent(string key)
            {
                if (_parent == null)
                    return false;
                else if (_parent._symbols.ContainsKey(key)) //Direct lookup in parent
                    return true;
                else //Forward question
                    return _parent.IsKeyDefinedInParent(key);
            }
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

        #region Symbols

        /// <summary>
        /// (Re)declares a local symbol.
        /// </summary>
        /// <param name="kind">The new interpretation for this symbol.</param>
        /// <param name="id">The symbols id.</param>
        [DebuggerStepThrough]
        public void Declare(SymbolInterpretations kind, string id)
        {
            Declare(kind, id, id);
        }

        /// <summary>
        /// (Re)declares a local symbol.
        /// </summary>
        /// <param name="kind">The new interpretation for this symbol.</param>
        /// <param name="id">The id entered into the symbol table.</param>
        /// <param name="translatedId">The (physical) id used when translating the program. (Use for aliases or set to <paramref name="id"/>)</param>
        [DebuggerStepThrough]
        public void Declare(SymbolInterpretations kind, string id, string translatedId)
        {
            if (Symbols.IsKeyDefinedLocally(id))
            {
                var entry = Symbols[id];
                entry.Interpretation = kind;
                entry.Id = translatedId;
            }
            else
            {
                Symbols[id] = new SymbolEntry(kind, translatedId);
            }
        }

        /// <summary>
        /// Creates local variables or declares global variables locally.
        /// </summary>
        /// <param name="kind">The (new) interpretation for the local variable.</param>
        /// <param name="id">The id for the local variable.</param>
        /// <remarks>Local object and reference variables are created in addition to being registered in the symbol table. Global object and reference variables are only declared, not created.</remarks>
        [DebuggerStepThrough]
        public void Define(SymbolInterpretations kind, string id)
        {
            Define(kind, id, id);
        }

        /// <summary>
        /// Creates local variables or declares global variables locally.
        /// </summary>
        /// <param name="kind">The (new) interpretation for the local variable.</param>
        /// <param name="id">The id for the local variable.</param>
        /// <param name="translatedId">The (physical) id used when translating the program. (Use for aliases or set to <paramref name="id"/>). This is the name used for the variable created.</param>
        /// <remarks>Local object and reference variables are created in addition to being registered in the symbol table. Global object and reference variables are only declared, not created.</remarks>
        [DebuggerStepThrough]
        public void Define(SymbolInterpretations kind, string id, string translatedId)
        {
            switch (kind)
            {
                    //Declare global variables
                case SymbolInterpretations.GlobalObjectVariable:
                case SymbolInterpretations.GlobalReferenceVariable:
                    if (Symbols.IsKeyDefinedLocally(id))
                        Symbols[id].Interpretation = kind;
                    else
                        Symbols[id] = new SymbolEntry(kind, translatedId);
                    break;
                    //Define local variables
                case SymbolInterpretations.LocalObjectVariable:
                case SymbolInterpretations.LocalReferenceVariable:
                    if (Symbols.IsKeyDefinedLocally(id))
                        Symbols[id].Interpretation = kind;
                    else
                        Symbols[id] = new SymbolEntry(kind, translatedId);

                    if (!Function.Variables.Contains(translatedId))
                        Function.Variables.Add(translatedId);
                    break;
            }
        }

        #endregion //Symbols

        #region Scope Block Stack

        private readonly Stack<AstBlock> _scopeBlocks = new Stack<AstBlock>();

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
        public void BeginBlock(AstBlock bl)
        {
            if (bl == null)
                throw new ArgumentNullException("bl");
            _scopeBlocks.Push(bl);
        }

        [DebuggerStepThrough]
        public AstBlock BeginBlock(string prefix)
        {
            var prototype = _scopeBlocks.Count > 0 ? _scopeBlocks.Peek() : _ast;
            var bl = new AstBlock(prototype.File, prototype.Line, prototype.Column, GenerateLocalId(), prefix);
            _scopeBlocks.Push(bl);
            return bl;
        }

        [DebuggerStepThrough]
        public AstBlock BeginBlock()
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
        /// Safely removes an instruction without invalidating jumps or try-blocks. Notifies <see cref="AddressChangeHooks"/>.
        /// </summary>
        /// <param name="index">The address of the instruction to remove.</param>
        [DebuggerStepThrough]
        public void RemoveInstructionAt(int index)
        {
            RemoveInstructionRange(index, 1);
        }

        /// <summary>
        /// Safely remoes a range of instructions without invalidating jumps or try-blocks. Notifies <see cref="AddressChangeHooks"/>.
        /// </summary>
        /// <param name="index">The address of the first instruction to remove.</param>
        /// <param name="count">The number of instructions to remove.</param>
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

            //Correct jump targets by...
            foreach (var ins in code)
            {
                if ((ins.IsJump || ins.OpCode == OpCode.leave)
                    && ins.Arguments > index) //decrementing target addresses pointing 
                    //behind the removed instruction
                    ins.Arguments -= count;
            }

            //Correct try-catch-finally blocks
            var modifiedBlocks = new MetaEntry[_function.TryCatchFinallyBlocks.Count];
            var i = 0;
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
        /// Requests an outer function to share a variable with this inner function.
        /// </summary>
        /// <param name="id">The (physical) id of the variable or parameter to require from the outer function.</param>
        /// <exception cref="PrexoniteException">Outer function(s) don't contain a variable or parameter named <paramref name="id"/>.</exception>
        [DebuggerStepThrough]
        public void RequireOuterVariable(string id)
        {
            if (_parentTarget == null)
                throw new PrexoniteException("Cannot require outer variable from top-level function.");

            _outerVariables.Add(id);
            //Make parent functions hand down the variable, even if they don't use them themselves.

            //for (var T = _parentTarget; T != null; T = T._parentTarget)
            var T = _parentTarget;

            {
                var func = T.Function;
                if (func.Variables.Contains(id) || func.Parameters.Contains(id) || T.OuterVariables.Contains(id))
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
        /// Promotes captured variables to function parameters.
        /// </summary>
        /// <returns>A list of expressions (get symbol) that should be added to the arguments list of any call to the lifted function.</returns>
        internal Func<Parser, IList<IAstExpression>> ToCaptureByValue()
        {
            return ToCaptureByValue(new string[0]);
        }

        /// <summary>
        /// Promotes captured variables to function parameters.
        /// </summary>
        /// <param name="keepByRef">The set of captured variables that should be kept captured by reference (i.e. not promoted to parameters)</param>
        /// <returns>A list of expressions (get symbol) that should be added to the arguments list of any call to the lifted function.</returns>
        internal Func<Parser, IList<IAstExpression>> ToCaptureByValue(IEnumerable<string> keepByRef)
        {
            var toPromote =
                _outerVariables.Where(outer => !keepByRef.Contains(outer)).ToList();

            //Declare locally, remove from outer variables and add as parameter to the end
            var exprs = new List<Func<Parser, IAstExpression>>();
            foreach (var outer in toPromote)
            {
                SymbolEntry sym;
                if (Symbols.TryGetValue(outer, out sym))
                    Symbols.Add(outer, sym);
                _outerVariables.Remove(outer);
                Function.Parameters.Add(outer);
                {
                    //Copy the value for capture by value in closure
                    var byValOuter = outer;
                    exprs.Add(p => new AstGetSetSymbol(p, byValOuter, SymbolInterpretations.LocalObjectVariable));
                }
            }

            return p => exprs.Select(e => e(p)).ToList();
        }

        #endregion

        #endregion

        #region Emitting Instructions

        #region Low Level

        [DebuggerStepThrough]
        public void Emit(Instruction ins)
        {
            _function.Code.Add(ins);
        }

        [DebuggerStepThrough]
        public void Emit(OpCode code)
        {
            Emit(new Instruction(code));
        }

        [DebuggerStepThrough]
        public void Emit(OpCode code, string id)
        {
            Emit(new Instruction(code, id));
        }

        [DebuggerStepThrough]
        public void Emit(OpCode code, int arguments)
        {
            Emit(new Instruction(code, arguments));
        }

        [DebuggerStepThrough]
        public void Emit(OpCode code, int arguments, string id)
        {
            Emit(new Instruction(code, arguments, id));
        }

        #endregion //Low Level

        #region High Level

        #region Constants

        [DebuggerStepThrough]
        public void EmitConstant(string value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitConstant(bool value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitConstant(double value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitConstant(int value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [DebuggerStepThrough]
        public void EmitNull()
        {
            Emit(Instruction.CreateNull());
        }

        #endregion

        #region Operators

        #endregion

        #region Variables

        [DebuggerStepThrough]
        public void EmitLoadLocal(string id)
        {
            Emit(Instruction.CreateLoadLocal(id));
        }

        public void EmitStoreLocal(string id)
        {
            Emit(Instruction.CreateStoreLocal(id));
        }

        public void EmitLoadGlobal(string id)
        {
            Emit(Instruction.CreateLoadGlobal(id));
        }

        public void EmitStoreGlobal(string id)
        {
            Emit(Instruction.CreateStoreGlobal(id));
        }

        #endregion

        #region Get/Set

        [DebuggerStepThrough]
        public void EmitGetCall(int args, string id, bool justEffect)
        {
            Emit(Instruction.CreateGetCall(args, id, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitGetCall(int args, string id)
        {
            EmitGetCall(args, id, false);
        }

        [DebuggerStepThrough]
        public void EmitSetCall(int args, string id)
        {
            Emit(Instruction.CreateSetCall(args, id));
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(int args, string callExpr, bool justEffect)
        {
            Emit(Instruction.CreateStaticGetCall(args, callExpr, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(int args, string callExpr)
        {
            EmitStaticGetCall(args, callExpr, false);
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(int args, string typeId, string memberId, bool justEffect)
        {
            Emit(Instruction.CreateStaticGetCall(args, typeId, memberId, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitStaticGetCall(int args, string typeId, string memberId)
        {
            EmitStaticGetCall(args, typeId, memberId, false);
        }

        [DebuggerStepThrough]
        public void EmitStaticSetCall(int args, string callExpr)
        {
            Emit(Instruction.CreateStaticSetCall(args, callExpr));
        }

        [DebuggerStepThrough]
        public void EmitStaticSet(int args, string typeId, string memberId)
        {
            Emit(Instruction.CreateStaticSetCall(args, typeId, memberId));
        }

        [DebuggerStepThrough]
        public void EmitIndirectCall(int args, bool justEffect)
        {
            Emit(Instruction.CreateIndirectCall(args, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitIndirectCall(int args)
        {
            Emit(Instruction.CreateIndirectCall(args));
        }

        #endregion //Get/Set

        #region Functions/Commands

        [DebuggerStepThrough]
        public void EmitFunctionCall(int args, string id)
        {
            EmitFunctionCall(args, id, false);
        }

        [DebuggerStepThrough]
        public void EmitFunctionCall(int args, string id, bool justEffect)
        {
            Emit(Instruction.CreateFunctionCall(args, id, justEffect));
        }

        [DebuggerStepThrough]
        public void EmitCommandCall(int args, string id)
        {
            EmitCommandCall(args, id, false);
        }

        [DebuggerStepThrough]
        public void EmitCommandCall(int args, string id, bool justEffect)
        {
            Emit(Instruction.CreateCommandCall(args, id, justEffect));
        }

        #endregion //Functions/Commands

        #region Stack manipulation

        public void EmitExchange()
        {
            Emit(Instruction.CreateExchange());
        }

        public void EmitRotate(int rotations)
        {
            Emit(Instruction.CreateRotate(rotations));
        }

        public void EmitRotate(int rotations, int instructions)
        {
            Emit(Instruction.CreateRotate(rotations, instructions));
        }

        public void EmitPop(int values)
        {
            Emit(Instruction.CreatePop(values));
        }

        public void EmitPop()
        {
            EmitPop(1);
        }

        public void EmitDuplicate(int copies)
        {
            Emit(Instruction.CreateDuplicate(copies));
        }

        public void EmitDuplicate()
        {
            Emit(Instruction.CreateDuplicate());
        }

        #endregion

        #region Jumps and Labels

        public const string LabelSymbolPostfix = @"\label\assembler";
        private readonly List<Instruction> _unresolvedInstructions = new List<Instruction>();

        public void EmitLeave(int address)
        {
            var ins = new Instruction(OpCode.leave, address);
            Emit(ins);
        }

        public void EmitJump(int address)
        {
            var ins = Instruction.CreateJump(address);
            Emit(ins);
        }

        public void EmitLeave(int address, string label)
        {
            var ins = new Instruction(OpCode.leave, address, label);
            Emit(ins);
        }

        public void EmitJump(int address, string label)
        {
            var ins = Instruction.CreateJump(address, label);
            Emit(ins);
        }

        public void EmitJumpIfTrue(int address)
        {
            Emit(Instruction.CreateJumpIfTrue(address));
        }

        public void EmitJumpIfTrue(int address, string label)
        {
            Emit(Instruction.CreateJumpIfTrue(address, label));
        }

        public void EmitJumpIfFalse(int address)
        {
            Emit(Instruction.CreateJumpIfFalse(address));
        }

        public void EmitJumpIfFalse(int address, string label)
        {
            Emit(Instruction.CreateJumpIfFalse(address, label));
        }

        public void EmitLeave(string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitLeave(address, label);
            }
            else
            {
                var ins = new Instruction(OpCode.leave, label);
                _unresolvedInstructions.Add(ins);
                Emit(ins);
            }
        }

        public void EmitJump(string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitJump(address, label);
            }
            else
            {
                var ins = Instruction.CreateJump(label);
                _unresolvedInstructions.Add(ins);
                Emit(ins);
            }
        }

        public void EmitJumpIfTrue(string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitJumpIfTrue(address, label);
            }
            else
            {
                var ins = Instruction.CreateJumpIfTrue(label);
                _unresolvedInstructions.Add(ins);
                Emit(ins);
            }
        }

        public void EmitJumpIfFalse(string label)
        {
            int address;
            if (TryResolveLabel(label, out address))
            {
                EmitJumpIfFalse(address, label);
            }
            else
            {
                var ins = Instruction.CreateJumpIfFalse(label);
                _unresolvedInstructions.Add(ins);
                Emit(ins);
            }
        }

        public void AddUnresolvedInstruction(Instruction jump)
        {
            _unresolvedInstructions.Add(jump);
        }

        public bool TryResolveLabel(string label, out int address)
        {
            address = -1;
            var labelNs = label + LabelSymbolPostfix;
            if (!LocalSymbols.ContainsKey(labelNs))
                return false;

            address = LocalSymbols[labelNs].Argument.Value;
            return true;
        }

        [DebuggerStepThrough]
        public string EmitLabel(int address)
        {
            var label = "L\\" + Guid.NewGuid().ToString("N");
            EmitLabel(label, address);
            return label;
        }

        /// <summary>
        /// <para>Adds a new label entry to the symbol table and resolves any symbolic jumps to this label.</para>
        /// <para>If the destination is an unconditional jump, it's destination address will 
        /// used instead of the supplied address.</para>
        /// <para>If the last instruction was a jump (conditional or unconditional) to this label, it 
        /// is considered redundant and will be removed.</para>
        /// </summary>
        /// <param name="label">The label's symbolic name.</param>
        /// <param name="address">The label's address.</param>
        //[DebuggerStepThrough]
        public void EmitLabel(string label, int address)
        {
            string partialResolve = null;

            //Check if the label points to an unconditional jump instruction
            Instruction jump = null;
            if (Code.Count > 0 && address < Code.Count && (jump = Code[address]).IsUnconditionalJump)
                if (jump.Arguments != -1)
                    //Forward destination address
                    address = jump.Arguments;
                else
                    //Forward destination label
                    partialResolve = jump.Id;

            //resolve any unresolved jumps
            foreach (var ins in _unresolvedInstructions.ToArray())
            {
                if (Engine.StringsAreEqual(ins.Id, label))
                {
                    //Found a matching unresolved 

                    if (partialResolve != null)
                    {
                        ins.Id = jump.Id;
                        //keep the instruction unresolved
                    }
                    else
                    {
                        ins.Arguments = address;
                        _unresolvedInstructions.Remove(ins);
                    }
                }
            }

            //Check if there is a redundant jump
            Instruction redundant;
            //if...
            if (
                //...there already are instructions, ...
                Code.Count > 0 &&
                //...this label points to the next instruction to write, ...
                address == Code.Count &&
                //...the last instruction is a jump (conditional or unconditional) and ...
                ((redundant = Code[address - 1]).IsJump) &&
                //...that last jump points to the next instruction) ...
                redundant.Arguments == address)
            {
                //...then ...
                //...remove that last jump ...
                Code.RemoveAt(Code.Count - 1);
                if (redundant.IsConditionalJump)
                    EmitPop(); //Make sure the stack keeps its integrity
                //..., adjust this labels target address
                address--;
                //...and all other instructions targeting this address
                foreach (var ins in Code)
                    if (ins.IsJump)
                        if (ins.Arguments == Code.Count + 1) // +1 since one instruction has been removed
                            ins.Arguments -= 1;
            }

            //Add the label to the symbol table
            Symbols[label + LabelSymbolPostfix] =
                new SymbolEntry(SymbolInterpretations.JumpLabel, address);
        }

        [DebuggerStepThrough]
        public void EmitLabel(string label)
        {
            EmitLabel(label, Code.Count);
        }

        [DebuggerStepThrough]
        public string EmitLabel()
        {
            return EmitLabel(Code.Count);
        }

        /// <summary>
        /// Deletes all information about a symbolic label.
        /// </summary>
        /// <param name="label">The name of the label to delete.</param>
        /// <remarks>This method just deletes the symbol table entry for the specified label and does not alter code in any way.</remarks>
        public void FreeLabel(string label)
        {
            LocalSymbols.Remove(label + LabelSymbolPostfix);
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
        /// Performs checks and block level optimizations on the target.
        /// </summary>
        /// <remarks>
        ///     Calling FinishTarget is <strong>>not</strong> optional, especially
        ///     for nested functions since they require additional processing.
        /// </remarks>
        public void FinishTarget()
        {
            _DetermineSharedNames();

            _checkUnresolvedInstructions();

            _unconditionalJumpTargetPropagation();

            _JumpReInversion();

            _removeJumpsToNextInstruction();

            _removeUnconditionalJumpSequences();

#if !(DEBUG || Verbose)
            _removeNop();
#endif

#if UseIndex
            if (Loader.Options.UseIndicesLocally)
                _by_index();
#endif
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
                    "The instruction [ " + _unresolvedInstructions[0] +
                    " ] has not been resolved.");
        }

        #endregion

        #region Unconditional jump target propagation

        /// <summary>
        /// Searches for jumps targeting unconditional jumps and propagates the final target back to the initial jump.
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
                    current.Arguments = target.Arguments;
                    current.Id = target.Id;

                    //Prepare next step
                    if (_targetIsInRange(target, count))
                        target = code[target.Arguments];
                    else
                        break;
                }
            }
            return;
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
        /// Detects and removes consecutive unconditional jumps.
        /// </summary>
        /// <remarks>
        /// <para>Since all jumps targeting unconditional jumps have been redirected by 
        /// <see cref="_unconditionalJumpTargetPropagation"/>, unconditional jumps that are preceded by an unconditional jump can no longer be reached directly.</para>
        /// <code>
        /// jump.f b
        /// ...
        /// jump a
        /// jump b
        /// jump c
        /// ...
        /// label a
        /// ...
        /// </code>
        /// <para>The above can be shortened to:</para>
        /// <code>
        /// jump.f b
        /// ...
        /// jump a
        /// ...
        /// label a
        /// ...
        /// </code>
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
        /// Detects and removes unconditional jumps to the following instruction.
        /// </summary>
        /// <remarks>
        /// <code>
        /// jump b
        /// label b
        /// ...
        /// </code> is shortened to <code>
        /// ...
        /// </code></remarks>
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
            return;
        }

        #endregion

        #region Jump re-inversion

        /// <summary>
        /// Detects conditional jumps skipping unconditional jumps and combines them into an inverted conditional jump.
        /// </summary>
        /// <remarks>
        /// <code>
        /// jump.f  after
        /// jump    somewhere
        /// label   after
        /// </code><para>is equal to</para>
        /// <code>
        /// jump.t  somewhere
        /// </code></remarks>
        private void _JumpReInversion()
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
                    condJ.OpCode = Instruction.InvertJumpCondition(condJ.OpCode);
                    condJ.Arguments = uncondJ.Arguments;
                    condJ.Id = uncondJ.Id;
                    RemoveInstructionAt(i + 1);
                }
                else
                {
                    RemoveInstructionRange(i, 2);
                }
            }

            return;
        }

        #endregion

        #region Removal of nop's (only RELEASE)

#if !(DEBUG || Verbose)
        private void _removeNop()
        {
            var code = Code;
            for (var i = 0; i < code.Count; i++)
            {
                var instruction = code[i];
                if (instruction.OpCode == OpCode.nop)
                    RemoveInstructionAt(i--);
            }
        }
#endif

        #endregion

        #region Replacement of 'by name' instructions for local variables

#if UseIndex

        /// <summary>
        /// Replaces by-name opcodes with by-index ones. Ignores variables with no mapping.
        /// </summary>
        private void _by_index()
        {
            //Exclude the initialization function from this optimization
            // as its symbol table keeps changing as more code files
            // are loaded into the VM.
            if (Engine.StringsAreEqual(Function.Id, Application.InitializationId))
                return;

            var code = Function.Code;
            Function.CreateLocalVariableMapping(); //Force (re)creation of the mapping
            var map = Function.LocalVariableMapping;
            if(map == null)
                throw new PrexoniteException("Local variable mapping of function " + Function.Id + " does not exist.");

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
                            throw new PrexoniteException(string.Format("Invalid instruction ({1}) in function {0}. Id missing.", Function.Id, ins));
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
            return;
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
    }
}