using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Modular
{
    public class FunctionIdChangingEventArgs : EventArgs
    {
        private readonly string _newId;

        public FunctionIdChangingEventArgs(string newId)
        {
            if(string.IsNullOrEmpty(newId))
                throw new ArgumentException("new id cannot be null or empty.","newId");
            _newId = newId;
        }

        public string NewId
        {
            get { return _newId; }
        }
    }

    public abstract class FunctionDeclaration : IHasMetaTable, IMetaFilter, IDependent<string>
    {
        protected FunctionDeclaration()
        {
        }

        public abstract event EventHandler<FunctionIdChangingEventArgs> IdChanging;

        /// <summary>
        /// The id of the global variable. Not null and not empty.
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// The meta table for this global variable.
        /// </summary>
        public abstract MetaTable Meta { get; }

        /// <summary>
        /// The collection of CLR namespaces imported by this function.
        /// </summary>
        public abstract SymbolCollection ImportedClrNamespaces { get; }

        /// <summary>
        /// The list of the function's parameter names.
        /// </summary>
        public abstract List<string> Parameters { get; }

        /// <summary>
        /// The set of (physical) local variables
        /// </summary>
        public abstract SymbolCollection LocalVariables { get; }

        /// <summary>
        /// The function's byte code, must always return the same reference.
        /// </summary>
        public abstract List<Instruction> Code { get; } 

        /// <summary>
        /// A table that maps from local variables to the indices of the variable slots used by the VM.
        /// </summary>
        public abstract SymbolTable<int> LocalVariableMapping { get; protected set; }

        /// <summary>
        ///     Writes a representation of this function delaration to the supplied writer.
        /// </summary>
        /// <param name="writer">The writer to write to. Assumed to be currently at valid position for a top-level definition.</param>
        public abstract void Store(TextWriter writer);

        /// <summary>
        ///     Creates a complete string representation of the function.
        /// </summary>
        /// <param name = "buffer">The buffer to which to write the string representation.</param>
        public void Store(StringBuilder buffer)
        {
            Store(new StringWriter(buffer));
        }

        /// <summary>
        ///     Creates a complete string representation of the function.
        /// </summary>
        /// <returns>A string containing the complete representation of the function.</returns>
        /// <remarks>
        ///     Use buffer or stream based overloads where possible.
        /// </remarks>
        public string Store()
        {
            var sb = new StringBuilder();
            Store(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Updates the mapping from local variable names to local variable slots (used by the VM).
        /// </summary>
        protected internal void CreateLocalVariableMapping()
        {
            var idx = 0;
            if (LocalVariableMapping == null)
                LocalVariableMapping = new SymbolTable<int>();
            else
                LocalVariableMapping.Clear();

            foreach (var p in Parameters)
                if (!LocalVariableMapping.ContainsKey(p))
                    LocalVariableMapping.Add(p, idx++);

            foreach (var v in LocalVariables)
                if (!LocalVariableMapping.ContainsKey(v))
                    LocalVariableMapping.Add(v, idx++);

            if (Meta.ContainsKey(PFunction.SharedNamesKey))
                foreach (var entry in Meta[PFunction.SharedNamesKey].List)
                    if (!LocalVariableMapping.ContainsKey(entry))
                        LocalVariableMapping.Add(entry, idx++);
        }

        /// <summary>
        /// An implementation of this function in CIL, if it is available; null otherwise.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Cil")]
        public abstract CilFunction CilImplementation { get; protected internal set; }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Cil")]
        public bool HasCilImplementation
        {
            [DebuggerStepThrough]
            get { return CilImplementation != null; }
        }

        public bool IsMacro
        {
            [DebuggerStepThrough]
            get { return Meta[CompilerTarget.MacroMetaKey].Switch; }
        }

        /// <summary>
        /// The name this function was originally declared under. 
        /// Primarily a debugging help, has no meaning in the Prexonite VM.
        /// </summary>
        public string LogicalId
        {
            [DebuggerStepThrough]
            get
            {
                MetaEntry logicalIdEntry;
                if (Meta.TryGetValue(PFunction.LogicalIdKey, out logicalIdEntry))
                    return logicalIdEntry.Text;
                else
                    return Id;
            }
        }

        /// <summary>
        ///     Transforms requests to the meta table.
        /// </summary>
        /// <param name = "key">The key to transform.</param>
        /// <returns>The transformed key.</returns>
        [DebuggerNonUserCode]
        string IMetaFilter.GetTransform(string key)
        {
            if (Engine.StringsAreEqual(key, Application.NameKey))
                return PFunction.IdKey;
            else
                return key;
        }

        /// <summary>
        ///     Transforms storage requests to the meta table.
        /// </summary>
        /// <param name = "item">The item to update/store.</param>
        /// <returns>The transformed item or null if nothing is to be stored.</returns>
        [DebuggerNonUserCode]
        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(
            KeyValuePair<string, MetaEntry> item)
        {
            //Prevent changing the name of the function;
            if ((Engine.StringsAreEqual(item.Key, PFunction.IdKey) ||
                Engine.StringsAreEqual(item.Key, Application.NameKey))
                && Meta != null) //this clauses causes the filter to skip this check 
                // while the Function is still being constructed
                return null;
            else if (Engine.StringsAreEqual(item.Key, Application.ImportKey) ||
                Engine.StringsAreEqual(item.Key, "imports"))
            {
                //
                ImportedClrNamespaces.Clear();
                var entries = item.Value.List;
                foreach (var entry in entries)
                    ImportedClrNamespaces.Add(entry.Text);
                return item;
            }
            else if (Engine.StringsAreEqual(item.Key, TryCatchFinallyBlock.MetaKey))
            {
                //Make sure the list of blocks is refreshed.
                InvalidateTryCatchFinallyBlocks();
                return item;
            }
            else if (Engine.StringsAreEqual(item.Key, PFunction.SymbolMappingKey))
            {
                var lst = item.Value.List;
                LocalVariableMapping = new SymbolTable<int>(lst.Length);
                for (var i = 0; i < lst.Length; i++)
                {
                    var symbol = lst[i];
                    LocalVariableMapping.Add(symbol.Text, i);
                }
                return null;
            }
            else
                return item;
        }

        public abstract ReadOnlyCollection<TryCatchFinallyBlock> TryCatchFinallyBlocks { get; }

        public abstract void InvalidateTryCatchFinallyBlocks();

        #region Implementation of INamed<string>/IDependent<string>

        // TODO: Consider implement cross-module dependency tracking

        string INamed<string>.Name
        {
            get { return Id; }
        }

        public IEnumerable<string> GetDependencies()
        {
            foreach (var ins in Code)
            {
                var argc = ins.Arguments;
                var id = ins.Id;
                var justEffect = ins.JustEffect;
                var genericArgument = ins.GenericArgument;
                var opCode = ins.OpCode;

                switch (opCode)
                {
                    case OpCode.invalid:
                        break;
                    case OpCode.nop:
                        break;
                    case OpCode.ldc_int:
                        break;
                    case OpCode.ldc_real:
                        break;
                    case OpCode.ldc_bool:
                        break;
                    case OpCode.ldc_string:
                        break;
                    case OpCode.ldc_null:
                        break;
                    case OpCode.ldr_loc:
                        break;
                    case OpCode.ldr_loci:
                        break;
                    case OpCode.ldr_glob:
                        break;
                    case OpCode.ldr_func:
                        yield return id;
                        break;
                    case OpCode.ldr_cmd:
                        break;
                    case OpCode.ldr_app:
                        break;
                    case OpCode.ldr_eng:
                        break;
                    case OpCode.ldr_type:
                        break;
                    case OpCode.ldloc:
                        break;
                    case OpCode.stloc:
                        break;
                    case OpCode.ldloci:
                        break;
                    case OpCode.stloci:
                        break;
                    case OpCode.ldglob:
                        break;
                    case OpCode.stglob:
                        break;
                    case OpCode.newobj:
                        break;
                    case OpCode.newtype:
                        break;
                    case OpCode.newclo:
                        yield return id;
                        break;
                    case OpCode.newcor:
                        break;
                    case OpCode.incloc:
                        break;
                    case OpCode.incglob:
                        break;
                    case OpCode.decloc:
                        break;
                    case OpCode.decglob:
                        break;
                    case OpCode.incloci:
                        break;
                    case OpCode.decloci:
                        break;
                    case OpCode.check_const:
                        break;
                    case OpCode.check_arg:
                        break;
                    case OpCode.check_null:
                        break;
                    case OpCode.cast_const:
                        break;
                    case OpCode.cast_arg:
                        break;
                    case OpCode.get:
                        break;
                    case OpCode.set:
                        break;
                    case OpCode.sget:
                        break;
                    case OpCode.sset:
                        break;
                    case OpCode.func:
                        yield return id;
                        break;
                    case OpCode.cmd:
                        break;
                    case OpCode.indarg:
                        break;
                    case OpCode.tail:
                        break;
                    case OpCode.indloc:
                        break;
                    case OpCode.indloci:
                        break;
                    case OpCode.indglob:
                        break;
                    case OpCode.jump:
                        break;
                    case OpCode.jump_t:
                        break;
                    case OpCode.jump_f:
                        break;
                    case OpCode.ret_exit:
                        break;
                    case OpCode.ret_value:
                        break;
                    case OpCode.ret_break:
                        break;
                    case OpCode.ret_continue:
                        break;
                    case OpCode.ret_set:
                        break;
                    case OpCode.@throw:
                        break;
                    case OpCode.@try:
                        break;
                    case OpCode.leave:
                        break;
                    case OpCode.exc:
                        break;
                    case OpCode.pop:
                        break;
                    case OpCode.dup:
                        break;
                    case OpCode.rot:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Creates a new function declaration using the default implementation.
        /// </summary>
        /// <param name="id">The physical id of the function.</param>
        /// <returns>A new function declaration.</returns>
        internal static FunctionDeclaration _Create(string id, Module module)
        {
            return new Impl(id,module);
        }

        private class Impl : FunctionDeclaration
        {
            public Impl(string id, Module module)
            {
                if(String.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("Function id cannot be null, empty or just whitespace.","id");
                var meta = MetaTable.Create(this);
                meta[PFunction.IdKey] = id;
                meta[Application.ImportKey] = module.Meta[Application.ImportKey];
                _meta = meta;

                LocalVariableMapping = new SymbolTable<int>();
            }

            private readonly MetaTable _meta; //initialized in constructor

            private readonly SymbolCollection _importedClrNamespaces = new SymbolCollection();

            private readonly SymbolCollection _localVariables = new SymbolCollection();

            private readonly List<Instruction> _code = new List<Instruction>();

            private List<TryCatchFinallyBlock> _tryCatchFinallyBlocks;
            private readonly List<string> _parameters = new List<string>();

            public override event EventHandler<FunctionIdChangingEventArgs> IdChanging;

            protected virtual void OnIdChanging(string newId)
            {
                var idChangingHandler = IdChanging;
                if (idChangingHandler != null)
                {
                    idChangingHandler(this, new FunctionIdChangingEventArgs(newId));
                }
            }

            public override string Id
            {
                get { return Meta[PFunction.IdKey].Text; }
            }

            public override MetaTable Meta
            {
                get { return _meta; }
            }

            public override SymbolCollection ImportedClrNamespaces
            {
                get { return _importedClrNamespaces; }
            }

            public override List<string> Parameters
            {
                get { return _parameters; }
            }

            public override SymbolCollection LocalVariables
            {
                get { return _localVariables; }
            }

            public override List<Instruction> Code
            {
                get { return _code; }
            }

            public sealed override SymbolTable<int> LocalVariableMapping { get; protected set; }

            public override CilFunction CilImplementation { get; protected internal set; }

            public override ReadOnlyCollection<TryCatchFinallyBlock> TryCatchFinallyBlocks
            {
                get
                { //Create the collection if it does not exist.
                    if (_tryCatchFinallyBlocks == null)
                        _tryCatchFinallyBlocks = _parseTryCatchFinallyBlocks();
                    return _tryCatchFinallyBlocks.AsReadOnly();
                }
            }

            private List<TryCatchFinallyBlock> _parseTryCatchFinallyBlocks()
            {
                var tryCatchFinallyBlocks = new List<TryCatchFinallyBlock>();
                MetaEntry tcfe;
                if (Meta.TryGetValue(TryCatchFinallyBlock.MetaKey, out tcfe))
                {
                    foreach (var blockEntry in tcfe.List)
                    {
                        int beginTry,
                            beginFinally,
                            beginCatch,
                            endTry;

                        var blockLst = blockEntry.List;
                        if (blockLst.Length != 5)
                            continue;

                        if (!int.TryParse(blockLst[0], out beginTry)) //beginTry, required
                            continue;
                        if (!int.TryParse(blockLst[1], out beginFinally))
                            //beginFinally, default: -1
                            beginFinally = -1;
                        if (!int.TryParse(blockLst[2], out beginCatch))
                            //beginCatch, default: -1
                            beginCatch = -1;
                        if (!int.TryParse(blockLst[3], out endTry)) //endTry, required
                            continue;

                        var block = new TryCatchFinallyBlock(beginTry, endTry)
                            {
                                BeginFinally = beginFinally,
                                BeginCatch = beginCatch,
                                UsesException = blockLst[4].Switch
                            };

                        tryCatchFinallyBlocks.Add(block);
                    }
                }
                return tryCatchFinallyBlocks;
            }

            public override void InvalidateTryCatchFinallyBlocks()
            {
                _tryCatchFinallyBlocks = null;
            }

            /// <summary>
            ///     Returns a string describing the function.
            /// </summary>
            /// <returns>A string describing the function.</returns>
            /// <remarks>
            ///     If you need a complete string representation, use <see cref = "Store" />.
            /// </remarks>
            public override string ToString()
            {
                var buffer = new StringBuilder();
                buffer.Append("function ");
                buffer.Append(Id);
                if (Parameters.Count > 0)
                {
                    buffer.Append("(");
                    foreach (var param in Parameters)
                        buffer.AppendFormat("{0}, ", param);
                    buffer.Remove(buffer.Length - 2, 2);
                    buffer.Append(")");
                }
                return buffer.ToString();
            }

            /// <summary>
            ///     Creates a complete string representation of the function.
            /// </summary>
            /// <param name = "writer">The writer to which to write the string representation.</param>
            public void StoreCode(TextWriter writer)
            {
                var reverseLocalMapping = new string[LocalVariableMapping.Count];
                foreach (var kvp in LocalVariableMapping)
                    reverseLocalMapping[kvp.Value] = kvp.Key;

                var buffer = new StringBuilder();
                if (LocalVariables.Count > 0)
                {
                    buffer.Append("var ");
                    foreach (var variable in LocalVariables)
                    {
                        buffer.Append(StringPType.ToIdLiteral(variable));
                        buffer.Append(',');
                    }
                    buffer.Length -= 1;
                    buffer.Append(' ');
                    writer.Write(buffer);
#if DEBUG || Verbose || true
                    writer.WriteLine();
#endif
                    buffer.Length = 0;
                }

                //#if DEBUG || Verbose
                var idx = 0;
                //#endif
                if (Code.Count > 0)
                {
                    var digits = (int)Math.Ceiling(Math.Log10(Code.Count));

                    _appendAddress(buffer, idx, digits);

                    foreach (var rawIns in Code)
                    {
#if DEBUG || Verbose
                        int idxBeginning = buffer.Length;
#endif

                        //Rewrite index-based op-codes back
                        //  to names
                        Instruction ins;
                        switch (rawIns.OpCode)
                        {
                            case OpCode.ldloci:
                                ins = new Instruction(OpCode.ldloc,
                                    reverseLocalMapping[rawIns.Arguments]);
                                break;
                            case OpCode.stloci:
                                ins = new Instruction(OpCode.stloc,
                                    reverseLocalMapping[rawIns.Arguments]);
                                break;
                            case OpCode.incloci:
                                ins = new Instruction(OpCode.incloc,
                                    reverseLocalMapping[rawIns.Arguments]);
                                break;
                            case OpCode.decloci:
                                ins = new Instruction(OpCode.decloc,
                                    reverseLocalMapping[rawIns.Arguments]);
                                break;
                            case OpCode.indloci:
                                int index;
                                int argc;
                                rawIns.DecodeIndLocIndex(out index, out argc);
                                ins = new Instruction(OpCode.indloc, argc, reverseLocalMapping[index],
                                    rawIns.JustEffect);
                                break;
                            default:
                                ins = rawIns;
                                break;
                        }

                        ins.ToString(buffer);
#if DEBUG || Verbose
                        if (buffer[idxBeginning] != '@')
                            buffer.Insert(idxBeginning, ' ');
                        buffer.AppendLine();
                        _appendAddress(buffer, ++idx, digits);
#else
                    buffer.AppendLine();
                    _appendAddress(buffer, ++idx, digits);

#endif
                        writer.Write(buffer.ToString());
                        buffer.Length = 0;
                    }
                }
            }

            private static void _appendAddress(StringBuilder buffer, int address, int digits)
            {
                buffer.Append("/* ");
                buffer.Append(address.ToString(CultureInfo.InvariantCulture).PadLeft(digits, '0'));
                buffer.Append(" */ ");
            }

            /// <summary>
            ///     Creates a string representation of the functions byte code in Prexonite Assembler
            /// </summary>
            /// <param name = "buffer">The buffer to which to write the string representation to.</param>
            public void StoreCode(StringBuilder buffer)
            {
                StoreCode(new StringWriter(buffer));
            }

            /// <summary>
            ///     Creates a string representation of the functions byte code in Prexonite Assembler
            /// </summary>
            /// <param name = "writer">The writer to which to write the string representation to.</param>
            public override void Store(TextWriter writer)
            {
                #region Head

                writer.Write("function ");
                writer.Write(StringPType.ToIdLiteral(Id));
                StringBuilder buffer;
                if (Parameters.Count > 0)
                {
                    writer.Write("(");
                    buffer = new StringBuilder();
                    foreach (var param in Parameters)
                    {
                        buffer.Append(StringPType.ToIdLiteral(param));
                        buffer.Append(",");
                    }
                    buffer.Remove(buffer.Length - 1, 1);
                    writer.Write(buffer.ToString());
                    writer.Write(")");
                }
#if DEBUG || Verbose || true
                writer.WriteLine();
#endif

                #endregion

                #region Metainformation

                //Metainformation
                writer.Write(@"[");
                writer.Write(Loader.SuppressPrimarySymbol);
                writer.Write(";");
#if DEBUG || Verbose
                writer.WriteLine();
#endif
                var meta = Meta.Clone();
                meta.Remove(Application.ImportKey); //to be added separately
                meta.Remove(Application.IdKey); //implied
#pragma warning disable 612,618
                meta.Remove(Application.InitializationGeneration); //must be set to default
#pragma warning restore 612,618
                meta.Remove(Loader.SuppressPrimarySymbol);
                //stored functions always have their symbol declared separately
                meta.Store(writer);
                var lst = new List<MetaEntry>();
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach (var ns in ImportedClrNamespaces)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    lst.Add(ns);
                if (lst.Count > 0)
                {
                    var imports = new MetaEntry(lst.ToArray());
                    writer.Write(Application.ImportKey);
                    writer.Write(" ");
                    buffer = new StringBuilder();
                    imports.ToString(buffer);
                    writer.Write(buffer.ToString());
                    writer.Write(";");
#if DEBUG || Verbose
                    writer.WriteLine();
#endif
                }
                //write symbol mapping information
                writer.Write(PFunction.SymbolMappingKey);
                writer.Write(" {");
                var map = new string[LocalVariableMapping.Count];
                foreach (var mapping in LocalVariableMapping)
                    map[mapping.Value] = mapping.Key;
                for (var i = 0; i < map.Length; i++)
                {
                    writer.Write(StringPType.ToIdOrLiteral(map[i]));
                    if (i < map.Length - 1)
                        writer.Write(',');
                }
                writer.Write("};");
#if DEBUG || Verbose
                writer.WriteLine();
#endif
                writer.Write("]");
#if DEBUG || Verbose
                writer.WriteLine();
#endif
                //End of Metadata

                #endregion

                #region Code

                //write code
                writer.Write("{asm{");
#if DEBUG || Verbose || true
                writer.WriteLine();
#endif
                StoreCode(writer);

#if DEBUG || Verbose || true
                writer.WriteLine("}}");
                writer.WriteLine();
#else
            writer.Write("}}");
#endif

                #endregion
            }
        }

        

        #endregion
    }
}
