/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Prexonite.Compiler.Ast;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    public class CompilerTarget : IHasMetaTable
    {
        [NoDebug]
        public static string GenerateName(string prefix)
        {
            return prefix + "\\" + Engine.GenerateName();
        }

        #region Fields

        private Loader _loader;

        public Loader Loader
        {
            [NoDebug()]
            get { return _loader; }
        }

        private PFunction _function;

        public PFunction Function
        {
            [NoDebug()]
            get { return _function; }
        }

        private SymbolTable<SymbolEntry> _symbols = new SymbolTable<SymbolEntry>();

        public SymbolTable<SymbolEntry> LocalSymbols
        {
            [NoDebug()]
            get { return _symbols; }
        }

        private CompilerTarget _parentTarget;

        public CompilerTarget ParentTarget
        {
            [NoDebug]
            get { return _parentTarget; }
            [NoDebug]
            set
            {
                _parentTarget = value;
                _combinedSymbolProxy = new CombinedSymbolProxy(this);
            }
        }

        private int _nestedFunctionCounter = 0;

        public int NestedFunctionCounter
        {
            [NoDebug]
            get { return _nestedFunctionCounter; }
            [NoDebug]
            set { _nestedFunctionCounter = value; }
        }

        #endregion

        #region Construction

        [NoDebug()]
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

        #region Symbol Lookup / Combined Symbol Proxy

        private CombinedSymbolProxy _combinedSymbolProxy;

        public CombinedSymbolProxy Symbols
        {
            [NoDebug()]
            get { return _combinedSymbolProxy; }
        }

        [NoDebug()]
        public sealed class CombinedSymbolProxy : IDictionary<string, SymbolEntry>
        {
            private readonly SymbolTable<SymbolEntry> symbols;
            private readonly CombinedSymbolProxy parent;
            private readonly SymbolTable<SymbolEntry> loaderSymbols;

            internal CombinedSymbolProxy(CompilerTarget outer)
            {
                symbols = outer._symbols;
                parent = outer._parentTarget != null ? outer._parentTarget.Symbols : null;
                loaderSymbols = outer._loader.Symbols;
            }

            public SymbolEntry this[string key]
            {
                get
                {
                    SymbolEntry entry;
                    if (symbols.TryGetValue(key, out entry))
                        return entry;
                    else if (parent != null)
                        return parent[key];
                    else
                        return loaderSymbols[key];
                }
                set { symbols[key] = value; }
            }

            #region IDictionary<string,SymbolEntry> Members

            public void Add(string key, SymbolEntry value)
            {
                symbols.Add(key, value);
            }

            public bool ContainsKey(string key)
            {
                if (symbols.ContainsKey(key))
                    return true;
                else if (parent != null)
                    return parent.ContainsKey(key);
                else
                    return loaderSymbols.ContainsKey(key);
            }

            public bool IsKeyDefinedLocally(string key)
            {
                return symbols.ContainsKey(key);
            }

            public bool IsKeyDefinedInParent(string key)
            {
                if (parent == null)
                    return false;
                else if (parent.symbols.ContainsKey(key)) //Direct lookup in parent
                    return true;
                else //Forward question
                    return parent.IsKeyDefinedInParent(key);
            }

            public ICollection<string> Keys
            {
                get
                {
                    ICollection<string> localKeys = symbols.Keys;
                    SymbolCollection keys = new SymbolCollection(localKeys);
                    if (parent != null)
                    {
                        foreach (string key in parent.Keys)
                            if (!localKeys.Contains(key))
                                keys.Add(key);
                    }
                    else
                    {
                        foreach (string key in loaderSymbols.Keys)
                            if (!localKeys.Contains(key))
                                keys.Add(key);
                    }
                    return keys;
                }
            }

            public bool Remove(string key)
            {
                return symbols.Remove(key);
            }

            public bool TryGetValue(string key, out SymbolEntry value)
            {
                if (symbols.TryGetValue(key, out value))
                    return true;
                else if (parent != null)
                    return parent.TryGetValue(key, out value);
                else
                    return loaderSymbols.TryGetValue(key, out value);
            }

            public ICollection<SymbolEntry> Values
            {
                get
                {
                    ICollection<SymbolEntry> localValues = symbols.Values;
                    List<SymbolEntry> values = new List<SymbolEntry>(localValues);
                    if (parent != null)
                    {
                        foreach (KeyValuePair<string, SymbolEntry> kvp in parent)
                            if (!localValues.Contains(kvp.Value))
                                values.Add(kvp.Value);
                    }
                    else
                    {
                        foreach (KeyValuePair<string, SymbolEntry> kvp in loaderSymbols)
                            if (!localValues.Contains(kvp.Value))
                                values.Add(kvp.Value);
                    }
                    return values;
                }
            }

            #endregion

            #region ICollection<KeyValuePair<string,SymbolEntry>> Members

            public void Add(KeyValuePair<string, SymbolEntry> item)
            {
                symbols.Add(item);
            }

            public void Clear()
            {
                symbols.Clear();
            }

            public bool Contains(KeyValuePair<string, SymbolEntry> item)
            {
                if (symbols.Contains(item))
                    return true;
                else if (parent != null)
                    return parent.Contains(item);
                else
                    return loaderSymbols.Contains(item);
            }

            public void CopyTo(KeyValuePair<string, SymbolEntry>[] array, int arrayIndex)
            {
                List<KeyValuePair<string, SymbolEntry>> lst =
                    new List<KeyValuePair<string, SymbolEntry>>(symbols);
                if (parent != null)
                {
                    foreach (KeyValuePair<string, SymbolEntry> kvp in parent)
                        if (!symbols.ContainsKey(kvp.Key))
                            lst.Add(kvp);
                }
                else
                {
                    foreach (KeyValuePair<string, SymbolEntry> kvp in loaderSymbols)
                        if (!symbols.ContainsKey(kvp.Key))
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
                return symbols.Remove(item);
            }

            #endregion

            #region IEnumerable<KeyValuePair<string,SymbolEntry>> Members

            IEnumerator<KeyValuePair<string, SymbolEntry>>
                IEnumerable<KeyValuePair<string, SymbolEntry>>.GetEnumerator()
            {
                foreach (KeyValuePair<string, SymbolEntry> kvp in symbols)
                    yield return kvp;
                if (parent != null)
                {
                    foreach (KeyValuePair<string, SymbolEntry> kvp in parent)
                        if (!symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
                else
                {
                    foreach (KeyValuePair<string, SymbolEntry> kvp in loaderSymbols)
                        if (!symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (KeyValuePair<string, SymbolEntry> kvp in symbols)
                    yield return kvp;
                if (parent != null)
                {
                    foreach (KeyValuePair<string, SymbolEntry> kvp in parent)
                        if (!symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
                else
                {
                    foreach (KeyValuePair<string, SymbolEntry> kvp in loaderSymbols)
                        if (!symbols.ContainsKey(kvp.Key))
                            yield return kvp;
                }
            }

            #endregion
        }

        #endregion

        #region Function.Code forwarding

        public List<Instruction> Code
        {
            [NoDebug()]
            get { return _function.Code; }
        }

        #endregion

        #region AST

        private AstBlock _ast;

        public AstBlock Ast
        {
            [NoDebug]
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

        [NoDebug]
        public void Declare(SymbolInterpretations kind, string id)
        {
            Declare(kind, id, id);
        }

        [NoDebug]
        public void Declare(SymbolInterpretations kind, string id, string translatedId)
        {
            if (Symbols.IsKeyDefinedLocally(id))
            {
                SymbolEntry entry = Symbols[id];
                entry.Interpretation = kind;
                entry.Id = translatedId;
            }
            else
            {
                Symbols[id] = new SymbolEntry(kind, translatedId);
            }
        }

        [NoDebug]
        public void Define(SymbolInterpretations kind, string id)
        {
            Define(kind, id, id);
        }

        [NoDebug]
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

                    if (!Function.Variables.Contains(id))
                        Function.Variables.Add(id);
                    break;
            }
        }

        #endregion //Symbols

        #region Block Jump Stack

        private Stack<BlockLabels> _blockLabelStack = new Stack<BlockLabels>();

        public Stack<BlockLabels> BlockLabelStack
        {
            [NoDebug]
            get { return _blockLabelStack; }
        }

        [NoDebug]
        public void BeginBlock(BlockLabels bl)
        {
            if (bl == null)
                throw new ArgumentNullException("bl");
            _blockLabelStack.Push(bl);
        }

        [NoDebug]
        public BlockLabels BeginBlock(string prefix)
        {
            BlockLabels bl = new BlockLabels(prefix);
            _blockLabelStack.Push(bl);
            return bl;
        }

        [NoDebug]
        public BlockLabels BeginBlock()
        {
            return BeginBlock((string) null);
        }

        [NoDebug]
        public BlockLabels EndBlock()
        {
            if (_blockLabelStack.Count > 0)
                return _blockLabelStack.Pop();
            else
                throw new PrexoniteException("There is no open block.");
        }

        public BlockLabels CurrentBlock
        {
            [NoDebug]
            get { return _blockLabelStack.Count > 0 ? _blockLabelStack.Peek() : null; }
        }

        #endregion //Block Jump Stack

        #region Code

        [NoDebug]
        public void RemoveInstructionAt(int index)
        {
            RemoveInstructionRange(index, 1);
        }

        public void RemoveInstructionRange(int index, int count)
        {
            List<Instruction> code = Code;
            if (index < 0 || index >= code.Count)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0 || index + count > code.Count)
                throw new ArgumentOutOfRangeException("count");
            if (count == 0)
                return;

            //Remove the instruction
            code.RemoveRange(index, count);

            //Correct jump targets by...
            foreach (Instruction ins in code)
            {
                if ((ins.IsJump || ins.OpCode == OpCode.leave)
                    && ins.Arguments > index) //decrementing target addresses pointing 
                    //behind the removed instruction
                    ins.Arguments -= count;
            }

            //Correct try-catch-finally blocks
            MetaEntry[] modifiedBlocks = new MetaEntry[_function.TryCatchFinallyBlocks.Count];
            int i = 0;
            foreach (TryCatchFinallyBlock block in _function.TryCatchFinallyBlocks)
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
                    throw new PrexoniteException(
                        "The try-catch-finally block (" + block +
                        ") is not valid after optimization.");

                modifiedBlocks[i++] = block;
            }

            _function.Meta[TryCatchFinallyBlock.MetaKey] = (MetaEntry) modifiedBlocks;
        }

        #endregion

        #region Nested function transparency

        private SymbolCollection _outerVariables = new SymbolCollection();

        [NoDebug]
        public void RequireOuterVariable(string id)
        {
            _outerVariables.Add(id);
            //Make parent function hand down the variable, even if they don't use them.
            for (CompilerTarget T = _parentTarget; T != null; T = T._parentTarget)
            {
                if (T._parentTarget != null)
                {
                    PFunction func = T.Function;
                    if (!(func.Parameters.Contains(id) || func.Variables.Contains(id)))
                        T.RequireOuterVariable(id);
                }
            }
        }

        #endregion

        #endregion

        #region Emitting Instructions

        #region Low Level

        [NoDebug()]
        public void Emit(Instruction ins)
        {
            _function.Code.Add(ins);
        }

        [NoDebug()]
        public void Emit(OpCode code)
        {
            Emit(new Instruction(code));
        }

        [NoDebug()]
        public void Emit(OpCode code, string id)
        {
            Emit(new Instruction(code, id));
        }

        [NoDebug()]
        public void Emit(OpCode code, int arguments)
        {
            Emit(new Instruction(code, arguments));
        }

        [NoDebug()]
        public void Emit(OpCode code, int arguments, string id)
        {
            Emit(new Instruction(code, arguments, id));
        }

        #endregion //Low Level

        #region High Level

        #region Constants

        [NoDebug()]
        public void EmitConstant(string value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [NoDebug()]
        public void EmitConstant(bool value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [NoDebug()]
        public void EmitConstant(double value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [NoDebug()]
        public void EmitConstant(int value)
        {
            Emit(Instruction.CreateConstant(value));
        }

        [NoDebug()]
        public void EmitNull()
        {
            Emit(Instruction.CreateNull());
        }

        #endregion

        #region Operators

        #endregion

        #region Variables

        [NoDebug()]
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

        [NoDebug()]
        public void EmitGetCall(int args, string id, bool justEffect)
        {
            Emit(Instruction.CreateGetCall(args, id, justEffect));
        }

        [NoDebug]
        public void EmitGetCall(int args, string id)
        {
            EmitGetCall(args, id, false);
        }

        [NoDebug]
        public void EmitSetCall(int args, string id)
        {
            Emit(Instruction.CreateSetCall(args, id));
        }

        [NoDebug]
        public void EmitStaticGetCall(int args, string callExpr, bool justEffect)
        {
            Emit(Instruction.CreateStaticGetCall(args, callExpr, justEffect));
        }

        [NoDebug]
        public void EmitStaticGetCall(int args, string callExpr)
        {
            EmitStaticGetCall(args, callExpr, false);
        }

        [NoDebug]
        public void EmitStaticGetCall(int args, string typeId, string memberId, bool justEffect)
        {
            Emit(Instruction.CreateStaticGetCall(args, typeId, memberId, justEffect));
        }

        [NoDebug]
        public void EmitStaticGetCall(int args, string typeId, string memberId)
        {
            EmitStaticGetCall(args, typeId, memberId, false);
        }

        [NoDebug]
        public void EmitStaticSetCall(int args, string callExpr)
        {
            Emit(Instruction.CreateStaticSetCall(args, callExpr));
        }

        [NoDebug]
        public void EmitStaticSet(int args, string typeId, string memberId)
        {
            Emit(Instruction.CreateStaticSetCall(args, typeId, memberId));
        }

        [NoDebug]
        public void EmitIndirectCall(int args, bool justEffect)
        {
            Emit(Instruction.CreateIndirectCall(args, justEffect));
        }

        [NoDebug]
        public void EmitIndirectCall(int args)
        {
            Emit(Instruction.CreateIndirectCall(args));
        }

        #endregion //Get/Set

        #region Functions/Commands

        [NoDebug]
        public void EmitFunctionCall(int args, string id)
        {
            EmitFunctionCall(args, id, false);
        }

        [NoDebug]
        public void EmitFunctionCall(int args, string id, bool justEffect)
        {
            Emit(Instruction.CreateFunctionCall(args, id, justEffect));
        }

        [NoDebug]
        public void EmitCommandCall(int args, string id)
        {
            EmitCommandCall(args, id, false);
        }

        [NoDebug]
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

        public void EmitLeave(int address)
        {
            Instruction ins = new Instruction(OpCode.leave, address);
            Emit(ins);
        }

        public void EmitJump(int address)
        {
            Instruction ins = Instruction.CreateJump(address);
            Emit(ins);
        }

        public void EmitLeave(int address, string label)
        {
            Instruction ins = new Instruction(OpCode.leave, address, label);
            Emit(ins);
        }

        public void EmitJump(int address, string label)
        {
            Instruction ins = Instruction.CreateJump(address, label);
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
                Instruction ins = new Instruction(OpCode.leave, label);
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
                Instruction ins = Instruction.CreateJump(label);
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
                Instruction ins = Instruction.CreateJumpIfTrue(label);
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
                Instruction ins = Instruction.CreateJumpIfFalse(label);
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
            string labelNs = label + LabelSymbolPostfix;
            if (!LocalSymbols.ContainsKey(labelNs))
                return false;

            address = LocalSymbols[labelNs].Argument.Value;
            return true;
        }

        private List<Instruction> _unresolvedInstructions = new List<Instruction>();

        [NoDebug()]
        public string EmitLabel(int address)
        {
            string label = "L\\" + Guid.NewGuid().ToString("N");
            EmitLabel(label, address);
            return label;
        }

        public const string LabelSymbolPostfix = @"\label\assembler";

        /// <summary>
        /// <para>Adds a new label entry to the symbol table and resolves any symbolic jumps to this label.</para>
        /// <para>If the destination is an unconditional jump, it's destination address will 
        /// used instead of the supplied address.</para>
        /// <para>If the last instruction was a jump (conditional or unconditional) to this label, it 
        /// is considered redundant and will be removed.</para>
        /// </summary>
        /// <param name="label">The label's symbolic name.</param>
        /// <param name="address">The label's address.</param>
        //[NoDebug()]
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
            foreach (Instruction ins in _unresolvedInstructions.ToArray())
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
                foreach (Instruction ins in Code)
                    if (ins.IsJump)
                        if (ins.Arguments == Code.Count + 1) // +1 since one instruction has been removed
                            ins.Arguments -= 1;
            }

            //Add the label to the symbol table
            Symbols[label + LabelSymbolPostfix] =
                new SymbolEntry(SymbolInterpretations.JumpLabel, address);
        }

        [NoDebug()]
        public void EmitLabel(string label)
        {
            EmitLabel(label, Code.Count);
        }

        [NoDebug()]
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

        /// <summary>
        /// Performs checks and block level optimizations on the target.
        /// </summary>
        /// <remarks>
        ///     Calling FinishTarget is <strong>>not</strong> optional, especially
        ///     for nested functions since they require additional processing.
        /// </remarks>
        public void FinishTarget()
        {
            MetaEntry[] outerVars = new MetaEntry[_outerVariables.Count];
            int i = 0;
            foreach (string outerVar in _outerVariables)
                outerVars[i++] = outerVar;
            if (i > 0)
                Function.Meta[PFunction.SharedNamesKey] = (MetaEntry) outerVars;

            _checkUnresolvedInstructions();

            _unconditionalJumpTargetPropagation();

            _JumpReInversion();

            _removeJumpsToNextInstruction();

            _removeUnconditionalJumpSequences();

#if !DEBUG
            _removeNop();

            if (Loader.Options.UseIndicesLocally)
                _by_index();
#endif
        }

        #region Check unresolved Instructions

        private void _checkUnresolvedInstructions()
        {
            //Check for unresolved instructions
            if (_unresolvedInstructions.Count > 0)
                throw new PrexoniteException(
                    "The instruction [ " + _unresolvedInstructions[0] +
                    " ] has not been resolved.");
        }

        #endregion

        #region Unconditional jump target propagation

        private bool _unconditionalJumpTargetPropagation()
        {
            //Unconditional jump target propagation
            List<Instruction> code = Code;
            int count = code.Count;
            bool[] addresses = new bool[count];
            bool optimized = false;
            for (int i = 0; i < count; i++)
            {
                Instruction current,
                            target;

                current = code[i];
                //Only valid jumps...
                if (!_isValidJump(current, count))
                    continue;

                target = code[current.Arguments];
                _reset(addresses);
                //...targetting valid unconditional jumps
                while (_isValidUnconditionalJump(target, count))
                {
                    //Mark the address of the unconditional jump for loop detection
                    addresses[current.Arguments] = true;
                    //Check for loop (uncond. jump targets another unconditional jump already visited
                    if (addresses[target.Arguments])
                        throw new PrexoniteException(
                            "Infinite loop in unconditional jump sequence detected.");
                    //Propagate address
                    current.Arguments = target.Arguments;
                    current.Id = target.Id;
                    optimized = true;

                    //Prepare next step
                    if (_targetIsInRange(target, count))
                        target = code[target.Arguments];
                    else
                        break;
                }
            }
            return optimized;
        }

        private static void _reset(bool[] addresses)
        {
            for (int i = 0; i < addresses.Length; i++)
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

        #region RemoveUnconditionalJumpSequences

        private void _removeUnconditionalJumpSequences()
        {
            List<Instruction> code = Code;
            for (int i = 0; i < code.Count; i++)
            {
                Instruction current = code[i];
                if (!current.IsUnconditionalJump)
                    continue;

                int count = 0;
                while ((i + count + 1) < code.Count && code[i + count + 1].IsUnconditionalJump)
                    count++;

                if (count > 0)
                    RemoveInstructionRange(i + 1, count);
                i -= count;
            }
        }

        #endregion

        #region RemoveJumpsToNextInstruction

        private bool _removeJumpsToNextInstruction()
        {
            bool optimized = false;
            List<Instruction> code = Code;
            for (int i = 0; i < code.Count; i++)
            {
                Instruction ins = code[i];
                if (ins.Arguments == i + 1)
                {
                    if (ins.IsUnconditionalJump)
                    {
                        RemoveInstructionAt(i--);
                        optimized = true;
                    }
                    else if (ins.IsConditionalJump)
                    {
                        throw new PrexoniteException(
                            "Redundant conditional jump to following instruction at address " +
                            i);
                    }
                }
            }
            return optimized;
        }

        #endregion

        #region Jump re-inversion

        private bool _JumpReInversion()
        {
            List<Instruction> code = Code;
            bool optimized = false;
            for (int i = 0; i < code.Count - 1; i++)
            {
                Instruction condJ = code[i];
                //jump, skipping the next instruction
                if (!(condJ.IsJump && condJ.Arguments == i + 2))
                    continue;
                Instruction uncondJ = code[i + 1];
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

                optimized = true;
            }

            return optimized;
        }

        #endregion

        #region Removal of nop's (only RELEASE)

#if !DEBUG
        private void _removeNop()
        {
            List<Instruction> code = Code;
            for (int i = 0; i < code.Count; i++)
            {
                Instruction instruction = code[i];
                if (instruction.OpCode == OpCode.nop)
                    RemoveInstructionAt(i--);
            }
        }
#endif

        #endregion

        #region Replacement of 'by name' instructions for local variables

        private bool _by_index()
        {
            bool optimized = false;
            if(Engine.StringsAreEqual(Function.Id,Application.InitializationId))
                return false; //Do not optimize \init function due to distributed symbol tables.
            List<Instruction> code = Function.Code;
            Function.CreateLocalVariableMapping(); //Force (re)creation of the mapping
            SymbolTable<int> map = Function.LocalVariableMapping;

            for (int i = 0; i < code.Count; i++)
            {
                Instruction ins = code[i];
                OpCode nopc;
                switch(ins.OpCode)
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
                        int idx = map[ins.Id];
                        code[i] = new Instruction(nopc, idx);
                        break;
                    case OpCode.indloc:
                        if(!map.ContainsKey(ins.Id))
                            continue;
                        idx = map[ins.Id];
                        int argc = ins.Arguments;
                        code[i] = Instruction.CreateIndLocI(idx, argc);
                        break;
                }
            }
            return optimized;
        }

        #endregion

        #endregion

        #region IHasMetaTable Members

        /// <summary>
        /// Provides access to the <see cref="Function"/>'s metatable.
        /// </summary>
        public MetaTable Meta
        {
            [NoDebug()]
            get { return _function.Meta; }
        }

        #endregion

        /// <summary>
        /// Returns the string <see cref="Function"/>'s string representation.
        /// </summary>
        /// <returns>The string <see cref="Function"/>'s string representation.</returns>
        [NoDebug]
        public override string ToString()
        {
            return Function.ToString();
        }
    }
}