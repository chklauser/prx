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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite
{
    /// <summary>
    ///     A function in the Prexonite Script VM.
    /// </summary>
    public class PFunction : IMetaFilter,
                             IHasMetaTable,
                             IIndirectCall,
                             IStackAware,
                             IDependent<string>
    {
        /// <summary>
        ///     The meta key under which the function's id is stored.
        /// </summary>
        public const string IdKey = "id";

        /// <summary>
        ///     The meta key under which the list of shared names is stored.
        /// </summary>
        public const string SharedNamesKey = @"\sharedNames";

        /// <summary>
        ///     The name of the variable that holds the list of arguments.
        /// </summary>
        public const string ArgumentListId = "args";

        public const string SymbolMappingKey = @"\symbol_mapping";

        /// <summary>
        ///     Signals that the function cannot be compiled to CIL.
        /// </summary>
        public const string VolatileKey = "volatile";

        /// <summary>
        ///     Signals that the function operates on its caller and thus causes the caller to be volatile (<see cref = "VolatileKey" />).
        /// </summary>
        public const string DynamicKey = "dynamic";

        /// <summary>
        ///     The reason why a function was marked volatile by the CIL compiler.
        /// </summary>
        public const string DeficiencyKey = "deficiency";

        /// <summary>
        ///     The id used in the source code. (As the nested function)
        /// </summary>
        public const string LogicalIdKey = "LogicalId";

        /// <summary>
        ///     The name of the functions logical parent.
        /// </summary>
        public const string ParentFunctionKey = "ParentFunction";

        /// <summary>
        ///     Indicates whether a function requires its arguments to be lazy.
        /// </summary>
        public const string LazyKey = "lazy";

        /// <summary>
        ///     The list of let-bound local names (local variables and shared variables)
        /// </summary>
        public const string LetKey = "let";

        #region Construction

        /// <summary>
        ///     Creates a new instance of PFunction.
        /// </summary>
        /// <param name = "parentApplication">The application of which the new function is part of.</param>
        /// <remarks>
        ///     The id is randomly generated using a GUID.
        /// </remarks>
        [DebuggerStepThrough]
        public PFunction(Application parentApplication)
            : this(parentApplication, "F\\" + Guid.NewGuid().ToString("N"))
        {
        }

        /// <summary>
        ///     Creates a new instance of PFunction.
        /// </summary>
        /// <param name = "parentApplication">The application of which the new function is part of.</param>
        /// <param name = "id">The function's id.</param>
        /// <remarks>
        ///     The id does not have to be a legal Prexonite Script identifier.
        /// </remarks>
        [DebuggerStepThrough]
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
            _meta._SetDirect(IdKey, id);

            Meta[Application.ImportKey] = parentApplication.Meta[Application.ImportKey];
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The functions id
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get { return _meta[IdKey]; }
        }

        public string LogicalId
        {
            [DebuggerStepThrough]
            get
            {
                MetaEntry logicalIdEntry;
                if (_meta.TryGetValue(LogicalIdKey, out logicalIdEntry))
                    return logicalIdEntry.Text;
                else
                    return Id;
            }
        }

        private readonly Application _parentApplication;

        /// <summary>
        ///     The application the function belongs to.
        /// </summary>
        public Application ParentApplication
        {
            [DebuggerStepThrough]
            get { return _parentApplication; }
        }

        private readonly SymbolCollection _importedNamesapces = new SymbolCollection();

        /// <summary>
        ///     The set of namespaces imported by this particular function.
        /// </summary>
        public SymbolCollection ImportedNamespaces
        {
            [DebuggerStepThrough]
            get { return _importedNamesapces; }
        }

        private readonly List<Instruction> _code = new List<Instruction>();

        /// <summary>
        ///     The bytecode for this function.
        /// </summary>
        public List<Instruction> Code
        {
            [DebuggerStepThrough]
            get { return _code; }
        }

        private readonly List<string> _parameters = new List<string>();

        /// <summary>
        ///     The list of formal parameters for this function.
        /// </summary>
        public List<string> Parameters
        {
            [DebuggerStepThrough]
            get { return _parameters; }
        }

        private readonly SymbolCollection _variables = new SymbolCollection();

        /// <summary>
        ///     The collection of variable names used by this function.
        /// </summary>
        public SymbolCollection Variables
        {
            [DebuggerStepThrough]
            get { return _variables; }
        }

        private SymbolTable<int> _localVariableMapping;

        /// <summary>
        ///     Updates the mapping of local names.
        /// </summary>
        internal void CreateLocalVariableMapping()
        {
            var idx = 0;
            if (_localVariableMapping == null)
                _localVariableMapping = new SymbolTable<int>();
            else
                _localVariableMapping.Clear();

            foreach (var p in _parameters)
                if (!_localVariableMapping.ContainsKey(p))
                    _localVariableMapping.Add(p, idx++);

            foreach (var v in _variables)
                if (!_localVariableMapping.ContainsKey(v))
                    _localVariableMapping.Add(v, idx++);

            if (_meta.ContainsKey(SharedNamesKey))
                foreach (var entry in _meta[SharedNamesKey].List)
                    if (!_localVariableMapping.ContainsKey(entry))
                        _localVariableMapping.Add(entry, idx++);
        }

        /// <summary>
        ///     The table that maps indices to local names.
        /// </summary>
        public SymbolTable<int> LocalVariableMapping
        {
            [DebuggerNonUserCode]
            get
            {
                if (_localVariableMapping == null)
                    CreateLocalVariableMapping();
                return _localVariableMapping;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Cil")]
        public CilFunction CilImplementation { [DebuggerStepThrough]
        get; [DebuggerStepThrough]
        internal set; }

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
            get { return _meta[CompilerTarget.MacroMetaKey].Switch; }
        }

        #endregion

        #region Storage

        /// <summary>
        ///     Returns a string describing the function.
        /// </summary>
        /// <returns>A string describing the function.</returns>
        /// <remarks>
        ///     If you need a complete string representation, use <see cref = "Store(StringBuilder)" />.
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
        ///     Creates a complete string representation of the function.
        /// </summary>
        /// <param name = "writer">The writer to which to write the string representation.</param>
        public void StoreCode(TextWriter writer)
        {
            var reverseLocalMapping = new string[LocalVariableMapping.Count];
            foreach (var kvp in LocalVariableMapping)
                reverseLocalMapping[kvp.Value] = kvp.Key;

            var buffer = new StringBuilder();
            if (Variables.Count > 0)
            {
                buffer.Append("var ");
                foreach (var variable in Variables)
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
                var digits = (int) Math.Ceiling(Math.Log10(Code.Count));

                appendAddress(buffer, idx, digits);

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
                    appendAddress(buffer, ++idx, digits);
#else
                    buffer.AppendLine();
                    appendAddress(buffer, ++idx, digits);

#endif
                    writer.Write(buffer.ToString());
                    buffer.Length = 0;
                }
            }
        }

        private static void appendAddress(StringBuilder buffer, int address, int digits)
        {
            buffer.Append("/* ");
            buffer.Append(address.ToString().PadLeft(digits, '0'));
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
        public void Store(TextWriter writer)
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
            meta.Remove(Application.InitializationGeneration); //must be set to default
            meta.Remove(Loader.SuppressPrimarySymbol);
            //stored functions always have their symbol declared separately
            meta.Store(writer);
            var lst = new List<MetaEntry>();
            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var ns in ImportedNamespaces)
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
            writer.Write(SymbolMappingKey);
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

        #endregion

        #region IHasMetaTable Members

        private readonly MetaTable _meta;

        /// <summary>
        ///     Returns a reference to the meta table associated with this function.
        /// </summary>
        public MetaTable Meta
        {
            [DebuggerNonUserCode]
            get { return _meta; }
        }

        #endregion

        #region IMetaFilter Members

        /// <summary>
        ///     Transforms requests to the meta table.
        /// </summary>
        /// <param name = "key">The key to transform.</param>
        /// <returns>The transformed key.</returns>
        [DebuggerNonUserCode]
        string IMetaFilter.GetTransform(string key)
        {
            if (Engine.DefaultStringComparer.Compare(key, "name") == 0)
                return IdKey;
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
            if (Engine.StringsAreEqual(item.Key, IdKey) ||
                Engine.StringsAreEqual(item.Key, "name"))
                return null;
            else if (Engine.StringsAreEqual(item.Key, Application.ImportKey) ||
                Engine.StringsAreEqual(item.Key, "imports"))
            {
                //
                _importedNamesapces.Clear();
                var entries = item.Value.List;
                foreach (var entry in entries)
                    _importedNamesapces.Add(entry.Text);
                return item;
            }
            else if (Engine.StringsAreEqual(item.Key, TryCatchFinallyBlock.MetaKey))
            {
                //Make sure the list of blocks is refreshed.
                InvalidateTryCatchFinallyBlocks();
                return item;
            }
            else if (Engine.StringsAreEqual(item.Key, SymbolMappingKey))
            {
                var lst = item.Value.List;
                _localVariableMapping = new SymbolTable<int>(lst.Length);
                for (var i = 0; i < lst.Length; i++)
                {
                    var symbol = lst[i];
                    _localVariableMapping.Add(symbol.Text, i);
                }
                return null;
            }
            else
                return item;
        }

        #endregion

        #region Invocation

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
        /// <param name = "suppressInitialization">A boolean indicating whether to suppress initialization of the parent application.</param>
        /// <returns>A function context for the execution of this function.</returns>
        internal FunctionContext CreateFunctionContext
            (
            Engine engine,
            PValue[] args,
            PVariable[] sharedVariables,
            bool suppressInitialization)
        {
            return
                new FunctionContext
                    (
                    engine, this, args, sharedVariables, suppressInitialization);
        }

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext
            (
            Engine engine, PValue[] args, PVariable[] sharedVariables)
        {
            return new FunctionContext(engine, this, args, sharedVariables);
        }

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "sctx">The stack context in which to create the new context.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(StackContext sctx, PValue[] args)
        {
            return CreateFunctionContext(sctx.ParentEngine, args);
        }


        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine for which to create the new context.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(Engine engine, PValue[] args)
        {
            return new FunctionContext(engine, this, args);
        }

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(Engine engine)
        {
            return new FunctionContext(engine, this);
        }

        /// <summary>
        ///     Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine, PValue[] args, PVariable[] sharedVariables)
        {
            if (HasCilImplementation)
            {
                //Fix #8
                ParentApplication.EnsureInitialization(engine, this);
                PValue result;
                ReturnMode returnMode;
                CilImplementation
                    (
                        this,
                        new NullContext(engine, ParentApplication, ImportedNamespaces),
                        args,
                        sharedVariables,
                        out result, out returnMode);
                return result;
            }
            else
            {
                var fctx = CreateFunctionContext(engine, args, sharedVariables);
                engine.Stack.AddLast(fctx);
                return engine.Process();
            }
        }

        /// <summary>
        ///     Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine, PValue[] args)
        {
            return Run(engine, args, null);
        }

        /// <summary>
        ///     Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine)
        {
            return Run(engine, null);
        }

        #endregion

        #region IIndirectCall Members

        /// <summary>
        ///     Executes the function and returns its result.
        /// </summary>
        /// <param name = "sctx">The stack context from which the function is called.</param>
        /// <param name = "args">The list of arguments to be passed to the function.</param>
        /// <returns>The value returned by the function or {null~Null}</returns>
        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
        {
            return Run(sctx.ParentEngine, args);
        }

        #endregion

        #region IStackAware Members

        /// <summary>
        ///     Creates a new stack context for the execution of this function.
        /// </summary>
        /// <param name = "sctx">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        [DebuggerNonUserCode]
        StackContext IStackAware.CreateStackContext(StackContext sctx, PValue[] args)
        {
            return CreateFunctionContext(sctx, args);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        ///     Causes the set of try-catch-finally blocks to be re-read on the next occurance of an exception.
        /// </summary>
        public void InvalidateTryCatchFinallyBlocks()
        {
            _tryCatchFinallyBlocks = null;
        }

        private List<TryCatchFinallyBlock> _tryCatchFinallyBlocks;

        /// <summary>
        ///     The cached set of try-catch-finally blocks.
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

                            _tryCatchFinallyBlocks.Add(block);
                        }
                    }
                }
                return _tryCatchFinallyBlocks.AsReadOnly();
            }
        }

        #endregion

        #region Implementation of INamed<string>/IDependent<string>

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
    }
}