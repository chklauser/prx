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
using System.IO;
using System.Text;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    public class PFunction : IMetaFilter,
                             IHasMetaTable,
                             IIndirectCall,
                             IStackAware
    {
        public const string IdKey = "id";
        public const string SharedNamesKey = @"\sharedNames";

        #region Construction

        [NoDebug()]
        public PFunction(Application parentApplication)
            : this(parentApplication, "F\\" + Guid.NewGuid().ToString("N"))
        {
        }

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

        public string Id
        {
            [NoDebug()]
            get { return _meta[IdKey]; }
        }

        private Application _parentApplication;
        public Application ParentApplication
        {
            [NoDebug()]
            get { return _parentApplication; }
        }

        private SymbolCollection _importedNamesapces = new SymbolCollection();
        public SymbolCollection ImportedNamespaces
        {
            [NoDebug()]
            get { return _importedNamesapces; }
        }

        private List<Instruction> _code = new List<Instruction>();
        public List<Instruction> Code
        {
            [NoDebug()]
            get { return _code; }
        }

        private List<string> _parameters = new List<string>();
        public List<string> Parameters
        {
            [NoDebug()]
            get { return _parameters; }
        }

        private SymbolCollection _variables = new SymbolCollection();
        public SymbolCollection Variables
        {
            [NoDebug()]
            get { return _variables; }
        }

        #endregion

        #region Storage

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

        public void Store(StringBuilder buffer)
        {
            Store(new StringWriter(buffer));
        }

        public void StoreCode(TextWriter writer)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (string variable in Variables)
                buffer.AppendLine("var " + variable + ";");

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

        public void StoreCode(StringBuilder buffer)
        {
            StoreCode(new StringWriter(buffer));
        }

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

        public MetaTable Meta
        {
            [NoDebug()]
            get { return _meta; }
        }

        #endregion

        #region IMetaFilter Members

        [NoDebug]
        public string GetTransform(string key)
        {
            if (Engine.DefaultStringComparer.Compare(key, "name") == 0)
                return IdKey;
            else
                return key;
        }

        [NoDebug]
        public KeyValuePair<string, MetaEntry>? SetTransform(KeyValuePair<string, MetaEntry> item)
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
            else if(Engine.StringsAreEqual(item.Key, TryCatchFinallyBlock.MetaKey))
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

        internal FunctionContext CreateFunctionContext(Engine parentEngine, PValue[] args, PVariable[] sharedVariable,
                                                       bool suppressInitialization)
        {
            return new FunctionContext(parentEngine, this, args, sharedVariable, suppressInitialization);
        }

        public FunctionContext CreateFunctionContext(Engine parentEngine, PValue[] args, PVariable[] sharedVariables)
        {
            return new FunctionContext(parentEngine, this, args, sharedVariables);
        }

        public FunctionContext CreateFunctionContext(Engine parentEngine, PValue[] args)
        {
            return new FunctionContext(parentEngine, this, args);
        }

        public FunctionContext CreateFunctionContext(Engine parentEngine)
        {
            return new FunctionContext(parentEngine, this);
        }

        public PValue Run(Engine parentEngine, PValue[] args, PVariable[] sharedVariables)
        {
            FunctionContext fctx = CreateFunctionContext(parentEngine, args, sharedVariables);
            parentEngine.Stack.AddLast(fctx);
            parentEngine.Process();
            return fctx.ReturnValue ?? PType.Null.CreatePValue();
        }

        public PValue Run(Engine parentEngine, PValue[] args)
        {
            return Run(parentEngine, args, null);
        }

        public PValue Run(Engine parentEngine)
        {
            return Run(parentEngine, null);
        }

        #endregion

        #region IIndirectCall Members

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            FunctionContext fctx = CreateFunctionContext(sctx.ParentEngine, args);
            sctx.ParentEngine.Process(fctx);
            return fctx.ReturnValue;
        }

        #endregion

        #region IStackAware Members

        [NoDebug]
        public StackContext CreateStackContext(Engine eng, PValue[] args)
        {
            return CreateFunctionContext(eng, args);
        }

        #endregion

        #region Exception Handling

        public void InvalidateTryCatchFinallyBlocks()
        {
            _tryCatchFinallyBlocks = null;
        }

        private List<TryCatchFinallyBlock> _tryCatchFinallyBlocks = null;
        public System.Collections.ObjectModel.ReadOnlyCollection<TryCatchFinallyBlock> TryCatchFinallyBlocks
        {
            get
            {
                if(_tryCatchFinallyBlocks == null)
                {
                    _tryCatchFinallyBlocks = new List<TryCatchFinallyBlock>();
                    MetaEntry tcfe;
                    if(Meta.TryGetValue(TryCatchFinallyBlock.MetaKey,out tcfe))
                    {
                        foreach (MetaEntry blockEntry in tcfe.List)
                        {
                            int beginTry,
                                beginFinally,
                                beginCatch,
                                endTry;

                            MetaEntry[] blockLst = blockEntry.List;
                            if(blockLst.Length != 5)
                                continue;

                            if(!int.TryParse(blockLst[0], out beginTry))
                                continue;
                            if (!int.TryParse(blockLst[1], out beginFinally))
                                beginFinally = -1;
                            if (!int.TryParse(blockLst[2], out beginCatch))
                                beginCatch = -1;
                            if (!int.TryParse(blockLst[3],out endTry))
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