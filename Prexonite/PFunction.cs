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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// A function in the Prexonite Script VM.
    /// </summary>
    public class PFunction : IMetaFilter,
                             IHasMetaTable,
                             IIndirectCall,
                             IStackAware
    {
        /// <summary>
        /// The meta key under which the function's id is stored.
        /// </summary>
        public const string IdKey = "id";

        /// <summary>
        /// The meta key under which the list of shared names is stored.
        /// </summary>
        public const string SharedNamesKey = @"\sharedNames";

        /// <summary>
        /// The name of the variable that holds the list of arguments.
        /// </summary>
        public const string ArgumentListId = "args";

        #region Construction

        /// <summary>
        /// Creates a new instance of PFunction.
        /// </summary>
        /// <param name="parentApplication">The application of which the new function is part of.</param>
        /// <remarks>The id is randomly generated using a GUID.</remarks>
        [NoDebug()]
        public PFunction(Application parentApplication)
            : this(parentApplication, "F\\" + Guid.NewGuid().ToString("N"))
        {
        }

        /// <summary>
        /// Creates a new instance of PFunction.
        /// </summary>
        /// <param name="parentApplication">The application of which the new function is part of.</param>
        /// <param name="id">The functions id.</param>
        /// <remarks>The id does not have to be a legal Prexonite Script identifier.</remarks>
        [NoDebug]
        internal PFunction(Application parentApplication, string id)
        {
            if (parentApplication == null)
                throw new ArgumentNullException("parentApplication");

            _parentApplication = parentApplication;

            if (id == null)
                throw new ArgumentNullException("id");
            else if (id.Length == 0)
                throw new ArgumentException("Id cannot be empty");

            //Note that function names do not have to be identifiers

            _meta = new MetaTable(this);
            _meta.SetDirect(IdKey, id);

            Meta[Application.ImportKey] = parentApplication.Meta[Application.ImportKey];
        }

        #endregion

        #region Properties

        /// <summary>
        /// The functions id
        /// </summary>
        public string Id
        {
            [NoDebug()]
            get { return _meta[IdKey]; }
        }

        private Application _parentApplication;

        /// <summary>
        /// The application the function belongs to.
        /// </summary>
        public Application ParentApplication
        {
            [NoDebug()]
            get { return _parentApplication; }
        }

        private SymbolCollection _importedNamesapces = new SymbolCollection();

        /// <summary>
        /// The set of namespaces imported by this particular function.
        /// </summary>
        public SymbolCollection ImportedNamespaces
        {
            [NoDebug()]
            get { return _importedNamesapces; }
        }

        private List<Instruction> _code = new List<Instruction>();

        /// <summary>
        /// The bytecode for this function.
        /// </summary>
        public List<Instruction> Code
        {
            [NoDebug()]
            get { return _code; }
        }

        private List<string> _parameters = new List<string>();

        /// <summary>
        /// The list of formal parameters for this function.
        /// </summary>
        public List<string> Parameters
        {
            [NoDebug()]
            get { return _parameters; }
        }

        private SymbolCollection _variables = new SymbolCollection();

        /// <summary>
        /// The collection of variable names used by this function.
        /// </summary>
        public SymbolCollection Variables
        {
            [NoDebug()]
            get { return _variables; }
        }

        private SymbolTable<int> _localVariableMapping;

        /// <summary>
        /// Updates the mapping of local names.
        /// </summary>
        internal void CreateLocalVariableMapping()
        {
            int idx = 0;
            if(_localVariableMapping == null)
                _localVariableMapping = new SymbolTable<int>();
            else
                _localVariableMapping.Clear();

            foreach (string p in _parameters)
                if(!_localVariableMapping.ContainsKey(p))
                    _localVariableMapping.Add(p, idx++);

            foreach (string v in _variables)
                if (!_localVariableMapping.ContainsKey(v))
                    _localVariableMapping.Add(v, idx++);

            if(_meta.ContainsKey(SharedNamesKey))
                foreach (MetaEntry entry in _meta[SharedNamesKey].List)
                    if (!_localVariableMapping.ContainsKey(entry))
                        _localVariableMapping.Add(entry, idx++);
        }

        /// <summary>
        /// The table that maps indices to local names.
        /// </summary>
        public SymbolTable<int> LocalVariableMapping
        {
            [NoDebug]
            get
            {
                if(_localVariableMapping == null)
                    CreateLocalVariableMapping();
                return _localVariableMapping;
            }
        }

        #endregion

        #region Storage

        /// <summary>
        /// Returns a string describing the function.
        /// </summary>
        /// <returns>A string describing the function.</returns>
        /// <remarks>If you need a complete string representation, use <see cref="Store(StringBuilder)"/>.</remarks>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("function ");
            buffer.Append(Id);
            if (Parameters.Count > 0)
            {
                buffer.Append("( ");
                foreach (string param in Parameters)
                    buffer.AppendFormat("{0}, ", param);
                buffer.Remove(buffer.Length - 2, 2);
                buffer.Append(")");
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Creates a complete string representation of the function.
        /// </summary>
        /// <param name="buffer">The buffer to which to write the string representation.</param>
        public void Store(StringBuilder buffer)
        {
            Store(new StringWriter(buffer));
        }

        /// <summary>
        /// Creates a complete string representation of the function.
        /// </summary>
        /// <param name="writer">The writer to which to write the string representation.</param>
        public void StoreCode(TextWriter writer)
        {
            StringBuilder buffer = new StringBuilder();
            if (Variables.Count > 0)
            {
                buffer.Append("var ");
                foreach (string variable in Variables)
                {
                    buffer.Append(variable);
                    buffer.Append(',');
                }
                buffer.Length -= 1;
                writer.WriteLine(buffer);
                buffer.Length = 0;
            }

#if DEBUG || Verbose
            int idx = 0;
#endif

            foreach (Instruction ins in Code)
            {
#if DEBUG || Verbose
                buffer.Append("/* " + (idx++).ToString().PadLeft(4, '0') + " */ ");
#endif
                int idxBeginning = buffer.Length;
                ins.ToString(buffer);
                if (buffer[idxBeginning] != '@')
                    buffer.Insert(idxBeginning, ' ');
                buffer.AppendLine();
            }
            writer.Write(buffer.ToString());
        }

        /// <summary>
        /// Creates a string representation of the functions byte code in Prexonite Assembler
        /// </summary>
        /// <param name="buffer">The buffer to which to write the string representation to.</param>
        public void StoreCode(StringBuilder buffer)
        {
            StoreCode(new StringWriter(buffer));
        }

        /// <summary>
        /// Creates a string representation of the functions byte code in Prexonite Assembler
        /// </summary>
        /// <param name="writer">The writer to which to write the string representation to.</param>
        public void Store(TextWriter writer)
        {
            writer.Write("function {0}", Id);
            StringBuilder buffer;
            if (Parameters.Count > 0)
            {
                writer.Write("(");
                buffer = new StringBuilder();
                foreach (string param in Parameters)
                    buffer.AppendFormat("{0},", param);
                buffer.Remove(buffer.Length - 1, 1);
                writer.Write(buffer.ToString());
                writer.Write(")");
            }
            writer.WriteLine();

            //Metainformation
            writer.WriteLine("[");
            Meta.Remove(Application.ImportKey);
            Meta.Store(writer);
            List<MetaEntry> lst = new List<MetaEntry>();
            foreach (string ns in ImportedNamespaces)
                lst.Add(ns);
            if (lst.Count > 0)
            {
                MetaEntry imports = new MetaEntry(lst.ToArray());
                writer.Write(Application.ImportKey);
                writer.Write(" ");
                buffer = new StringBuilder();
                imports.ToString(buffer);
                writer.Write(buffer.ToString());
                writer.WriteLine(";");
            }
            writer.Write("]");

            writer.WriteLine("{asm{");
            StoreCode(writer);
            writer.WriteLine("}}\n");
        }

        #endregion

        #region IHasMetaTable Members

        private MetaTable _meta;

        /// <summary>
        /// Returns a reference to the meta table associated with this function.
        /// </summary>
        public MetaTable Meta
        {
            [NoDebug()]
            get { return _meta; }
        }

        #endregion

        #region IMetaFilter Members

        /// <summary>
        /// Transforms requests to the meta table.
        /// </summary>
        /// <param name="key">The key to transform.</param>
        /// <returns>The transformed key.</returns>
        [NoDebug]
        string IMetaFilter.GetTransform(string key)
        {
            if (Engine.DefaultStringComparer.Compare(key, "name") == 0)
                return IdKey;
            else
                return key;
        }

        /// <summary>
        /// Transforms storage requests to the meta table.
        /// </summary>
        /// <param name="item">The item to update/store.</param>
        /// <returns>The transformed item or null if nothing is to be stored.</returns>
        [NoDebug]
        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            //Prevent changing the name of the function;
            if (Engine.StringsAreEqual(item.Key, IdKey) ||
                Engine.StringsAreEqual(item.Key, "name"))
                return null;
            else if (Engine.StringsAreEqual(item.Key, Application.ImportKey) ||
                     Engine.StringsAreEqual(item.Key, "imports"))
            {
                //
                _importedNamesapces.Clear();
                MetaEntry[] entries = item.Value.List;
                foreach (MetaEntry entry in entries)
                    _importedNamesapces.Add(entry.Text);
                return item;
            }
            else if (Engine.StringsAreEqual(item.Key, TryCatchFinallyBlock.MetaKey))
            {
                //Make sure the list of blocks is refreshed.
                InvalidateTryCatchFinallyBlocks();
                return item;
            }
            else
                return item;
        }

        #endregion

        #region Invocation

        /// <summary>
        /// Creates a new function context for execution.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <param name="sharedVariables">The list of variables shared with the caller.</param>
        /// <param name="suppressInitialization">A boolean indicating whether to suppress initialization of the parent application.</param>
        /// <returns>A function context for the execution of this function.</returns>
        internal FunctionContext CreateFunctionContext(
            Engine engine,
            PValue[] args,
            PVariable[] sharedVariables,
            bool suppressInitialization)
        {
            return
                new FunctionContext(
                    engine, this, args, sharedVariables, suppressInitialization);
        }

        /// <summary>
        /// Creates a new function context for execution.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <param name="sharedVariables">The list of variables shared with the caller.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(
            Engine engine, PValue[] args, PVariable[] sharedVariables)
        {
            return new FunctionContext(engine, this, args, sharedVariables);
        }

        /// <summary>
        /// Creates a new function context for execution.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(Engine engine, PValue[] args)
        {
            return new FunctionContext(engine, this, args);
        }

        /// <summary>
        /// Creates a new function context for execution.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(Engine engine)
        {
            return new FunctionContext(engine, this);
        }

        /// <summary>
        /// Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <param name="sharedVariables">The list of variables shared with the caller.</param>
        /// <returns>A function context for the execution of this function.</returns>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine, PValue[] args, PVariable[] sharedVariables)
        {
            FunctionContext fctx = CreateFunctionContext(engine, args, sharedVariables);
            engine.Stack.AddLast(fctx);
            engine.Process();
            return fctx.ReturnValue ?? PType.Null.CreatePValue();
        }

        /// <summary>
        /// Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine, PValue[] args)
        {
            return Run(engine, args, null);
        }

        /// <summary>
        /// Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine)
        {
            return Run(engine, null);
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        /// Executes the function and returns its result.
        /// </summary>
        /// <param name="sctx">The stack context from which the function is called.</param>
        /// <param name="args">The list of arguments to be passed to the function.</param>
        /// <returns>The value returned by the function or {null~Null}</returns>
        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
        {
            return Run(sctx.ParentEngine, args);
        }

        #endregion

        #region IStackAware Members

        /// <summary>
        /// Creates a new stack context for the execution of this function.
        /// </summary>
        /// <param name="engine">The engine in which to execute the function.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        [NoDebug]
        StackContext IStackAware.CreateStackContext(Engine engine, PValue[] args)
        {
            return CreateFunctionContext(engine, args);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Causes the set of try-catch-finally blocks to be re-read on the next occurance of an exception.
        /// </summary>
        public void InvalidateTryCatchFinallyBlocks()
        {
            _tryCatchFinallyBlocks = null;
        }

        private List<TryCatchFinallyBlock> _tryCatchFinallyBlocks = null;

        /// <summary>
        /// The cached set of try-catch-finally blocks.
        /// </summary>
        public ReadOnlyCollection<TryCatchFinallyBlock> TryCatchFinallyBlocks
        {
            get
            {
                //Create the collection if it does not exist.
                if (_tryCatchFinallyBlocks == null)
                {
                    _tryCatchFinallyBlocks = new List<TryCatchFinallyBlock>();
                    MetaEntry tcfe;
                    if (Meta.TryGetValue(TryCatchFinallyBlock.MetaKey, out tcfe))
                    {
                        foreach (MetaEntry blockEntry in tcfe.List)
                        {
                            int beginTry,
                                beginFinally,
                                beginCatch,
                                endTry;

                            MetaEntry[] blockLst = blockEntry.List;
                            if (blockLst.Length != 5)
                                continue;

                            if (!int.TryParse(blockLst[0], out beginTry))       //beginTry, required
                                continue;
                            if (!int.TryParse(blockLst[1], out beginFinally))   //beginFinally, default: -1
                                beginFinally = -1;
                            if (!int.TryParse(blockLst[2], out beginCatch))     //beginCatch, default: -1
                                beginCatch = -1;
                            if (!int.TryParse(blockLst[3], out endTry))         //endTry, required
                                continue;

                            TryCatchFinallyBlock block = new TryCatchFinallyBlock(beginTry, endTry);
                            block.BeginFinally = beginFinally;
                            block.BeginCatch = beginCatch;
                            block.UsesException = blockLst[4].Switch;

                            _tryCatchFinallyBlocks.Add(block);
                        }
                    }
                }
                return _tryCatchFinallyBlocks.AsReadOnly();
            }
        }

        #endregion
    }
}