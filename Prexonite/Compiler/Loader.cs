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

#if Compress
using System.IO.Compression;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Commands;
using Prexonite.Commands.Concurrency;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;
using Debug = System.Diagnostics.Debug;

namespace Prexonite.Compiler
{
    public class Loader : StackContext
    {
        #region Static

        public const string SuppressPrimarySymbol = "\\sps";

        #endregion

        #region Construction

        [DebuggerStepThrough]
        public Loader(Engine parentEngine, Application targetApplication)
            : this(new LoaderOptions(parentEngine, targetApplication))
        {
        }

        [DebuggerStepThrough]
        public Loader(LoaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            _options = options;

            _functionTargets = new SymbolTable<CompilerTarget>();
            _functionTargetsIterator = new FunctionTargetsIterator(this);

            CreateFunctionTarget(
                ParentApplication._InitializationFunction);

            if (options.RegisterCommands)
                RegisterExistingCommands();

            _compilerHooksIterator = new CompilerHooksIterator(this);
            _customResolversProxy = new CustomResolversIterator(_customResolvers);

            //Build commands
            _initializeBuildCommands();

            //Macro commands
            _initializeMacroCommands();

            //Provide fallback command info for CIL compilation
            _imprintCommandInfo();
        }

        private void _imprintCommandInfo()
        {
            foreach (var buildCommand in _buildCommands)
                ParentEngine.Commands.ProvideFallbackInfo(buildCommand.Key,
                    buildCommand.Value.ToCommandInfo());
        }

        public void RegisterExistingCommands()
        {
            foreach (var kvp in ParentEngine.Commands)
                Symbols.Declare(kvp.Key, CallSymbol.Create(EntityRef.Command.Create(kvp.Key)));
        }

        #endregion

        #region Options

        private readonly LoaderOptions _options;

        public LoaderOptions Options
        {
            [DebuggerStepThrough]
            get { return _options; }
        }

        #endregion

        #region Global Symbol Table

        public SymbolStore Symbols
        {
            [DebuggerStepThrough]
            get { return _options.Symbols; }
        }

        #endregion

        #region Function Symbol Tables

        private readonly SymbolTable<CompilerTarget> _functionTargets;
        private readonly FunctionTargetsIterator _functionTargetsIterator;

        public FunctionTargetsIterator FunctionTargets
        {
            [DebuggerStepThrough]
            get { return _functionTargetsIterator; }
        }

        [DebuggerStepThrough]
        public sealed class FunctionTargetsIterator : IEnumerable<CompilerTarget>
        {
            private readonly Loader _outer;

            internal FunctionTargetsIterator(Loader outer)
            {
                _outer = outer;
            }

            public int Count
            {
                get { return _outer._functionTargets.Count; }
            }

            public CompilerTarget this[string key]
            {
                get { return _outer._functionTargets[key]; }
            }

            public CompilerTarget this[PFunction key]
            {
                get { return _outer._functionTargets[key.Id]; }
            }

            public void Remove(CompilerTarget target)
            {
                var funcId = target.Function.Id;
                if (_outer._functionTargets.ContainsKey(funcId))
                    _outer._functionTargets.Remove(funcId);
            }

            #region Implementation of IEnumerable

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            ///     A <see cref = "T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
            /// </returns>
            /// <filterpriority>1</filterpriority>
            public IEnumerator<CompilerTarget> GetEnumerator()
            {
                return _outer._functionTargets.Values.GetEnumerator();
            }

            /// <summary>
            ///     Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            ///     An <see cref = "T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        //[DebuggerStepThrough]
        public CompilerTarget CreateFunctionTarget(PFunction func, CompilerTarget parentTarget = null, ISourcePosition sourcePosition = null)
        {
            if (func == null)
                throw new ArgumentNullException("func");

            var target = new CompilerTarget(this, func,parentTarget,sourcePosition);
            if (_functionTargets.ContainsKey(func.Id) &&
                (!ParentApplication.Meta.GetDefault(Application.AllowOverridingKey, true).Switch))
                throw new PrexoniteException(
                    String.Format("The application {0} does not allow overriding of function {1}.",
                        ParentApplication.Id,
                        func.Id));

            _functionTargets[func.Id] = target;

            return target;
        }

        #endregion

        #region Compiler Hooks

        private readonly CompilerHooksIterator _compilerHooksIterator;

        public CompilerHooksIterator CompilerHooks
        {
            [DebuggerStepThrough]
            get { return _compilerHooksIterator; }
        }

        private readonly List<CompilerHook> _compilerHooks = new List<CompilerHook>();

        [DebuggerStepThrough]
        public class CompilerHooksIterator : ICollection<CompilerHook>
        {
            private readonly List<CompilerHook> _lst;

            internal CompilerHooksIterator(Loader outer)
            {
                _lst = outer._compilerHooks;
            }

            #region ICollection<CompilerHook> Members

            ///<summary>
            ///    Adds an item to the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
            ///</summary>
            ///<param name = "item">The object to add to the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.</param>
            ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
            public void Add(CompilerHook item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");
                _lst.Add(item);
            }

            /// <summary>
            ///     Adds a managed transformation to the collection.
            /// </summary>
            /// <param name = "transformation">A managed transformation.</param>
            public void Add(AstTransformation transformation)
            {
                _lst.Add(new CompilerHook(transformation));
            }

            /// <summary>
            ///     Adds an interpreted transformation to the collection.
            /// </summary>
            /// <param name = "transformation">An interpreted transformation.</param>
            public void Add(PValue transformation)
            {
                if (transformation.Type.ToBuiltIn() == PType.BuiltIn.Object &&
                    transformation.Value is AstTransformation)
                    _lst.Add(new CompilerHook((AstTransformation) transformation.Value));
                else
                    _lst.Add(new CompilerHook(transformation));
            }

            ///<summary>
            ///    Removes all items from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
            ///</summary>
            ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
            public void Clear()
            {
                _lst.Clear();
            }

            ///<summary>
            ///    Determines whether the <see cref = "T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
            ///</summary>
            ///<returns>
            ///    true if item is found in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
            ///</returns>
            ///<param name = "item">The object to locate in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.</param>
            public bool Contains(CompilerHook item)
            {
                if (item == null)
                    return false;
                return _lst.Contains(item);
            }

            ///<summary>
            ///    Copies the elements of the <see cref = "T:System.Collections.Generic.ICollection`1"></see> to an <see
            ///     cref = "T:System.Array"></see>, starting at a particular <see cref = "T:System.Array"></see> index.
            ///</summary>
            ///<param name = "array">The one-dimensional <see cref = "T:System.Array"></see> that is the destination of the elements copied from <see
            ///     cref = "T:System.Collections.Generic.ICollection`1"></see>. The <see cref = "T:System.Array"></see> must have zero-based indexing.</param>
            ///<param name = "arrayIndex">The zero-based index in array at which copying begins.</param>
            ///<exception cref = "T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
            ///<exception cref = "T:System.ArgumentNullException">array is null.</exception>
            ///<exception cref = "T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see
            ///     cref = "T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
            public void CopyTo(CompilerHook[] array, int arrayIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");
                _lst.CopyTo(array, arrayIndex);
            }

            ///<summary>
            ///    Removes the first occurrence of a specific object from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
            ///</summary>
            ///<returns>
            ///    true if item was successfully removed from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see
            ///     cref = "T:System.Collections.Generic.ICollection`1"></see>.
            ///</returns>
            ///<param name = "item">The object to remove from the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.</param>
            ///<exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
            public bool Remove(CompilerHook item)
            {
                if (item == null)
                    return false;
                else
                    return _lst.Remove(item);
            }

            ///<summary>
            ///    Gets the number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
            ///</summary>
            ///<returns>
            ///    The number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1"></see>.
            ///</returns>
            public int Count
            {
                get { return _lst.Count; }
            }

            ///<summary>
            ///    Gets a value indicating whether the <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.
            ///</summary>
            ///<returns>
            ///    true if the <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.
            ///</returns>
            public bool IsReadOnly
            {
                get { return ((ICollection<CompilerHook>) _lst).IsReadOnly; }
            }

            #endregion

            #region IEnumerable<CompilerHook> Members

            ///<summary>
            ///    Returns an enumerator that iterates through the collection.
            ///</summary>
            ///<returns>
            ///    A <see cref = "T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
            ///</returns>
            ///<filterpriority>1</filterpriority>
            IEnumerator<CompilerHook> IEnumerable<CompilerHook>.GetEnumerator()
            {
                return _lst.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            ///<summary>
            ///    Returns an enumerator that iterates through a collection.
            ///</summary>
            ///<returns>
            ///    An <see cref = "T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
            ///</returns>
            ///<filterpriority>2</filterpriority>
            public IEnumerator GetEnumerator()
            {
                return _lst.GetEnumerator();
            }

            #endregion
        }

        #endregion

        #region Custom symbol resolving

        private readonly List<CustomResolver> _customResolvers = new List<CustomResolver>();
        private readonly CustomResolversIterator _customResolversProxy;

        public CustomResolversIterator CustomResolvers
        {
            get { return _customResolversProxy; }
        }

        public class CustomResolversIterator : ICollection<CustomResolver>
        {
            private readonly List<CustomResolver> _resolvers;

            internal CustomResolversIterator(List<CustomResolver> outer)
            {
                _resolvers = outer;
            }

            public int Length
            {
                get { return _resolvers.Count; }
            }

            public CustomResolver this[int index]
            {
                get { return _resolvers[index]; }
            }

            public IEnumerator<CustomResolver> GetEnumerator()
            {
                return _resolvers.GetEnumerator();
            }

            #region Implementation of IEnumerable

            /// <summary>
            ///     Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            ///     An <see cref = "T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of ICollection<CustomResolver>

            /// <summary>
            ///     Adds an item to the <see cref = "T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name = "item">The object to add to the <see cref = "T:System.Collections.Generic.ICollection`1" />.</param>
            /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
            public void Add(CustomResolver item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");
                _resolvers.Add(item);
            }

            /// <summary>
            ///     Removes all items from the <see cref = "T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only. </exception>
            public void Clear()
            {
                _resolvers.Clear();
            }

            /// <summary>
            ///     Determines whether the <see cref = "T:System.Collections.Generic.ICollection`1" /> contains a specific value.
            /// </summary>
            /// <returns>
            ///     true if <paramref name = "item" /> is found in the <see cref = "T:System.Collections.Generic.ICollection`1" />; otherwise, false.
            /// </returns>
            /// <param name = "item">The object to locate in the <see cref = "T:System.Collections.Generic.ICollection`1" />.</param>
            public bool Contains(CustomResolver item)
            {
                return item != null && Count > 0 && _resolvers.Contains(item);
            }

            /// <summary>
            ///     Copies the elements of the <see cref = "T:System.Collections.Generic.ICollection`1" /> to an <see
            ///      cref = "T:System.Array" />, starting at a particular <see cref = "T:System.Array" /> index.
            /// </summary>
            /// <param name = "array">The one-dimensional <see cref = "T:System.Array" /> that is the destination of the elements copied from <see
            ///      cref = "T:System.Collections.Generic.ICollection`1" />. The <see cref = "T:System.Array" /> must have zero-based indexing.</param>
            /// <param name = "arrayIndex">The zero-based index in <paramref name = "array" /> at which copying begins.</param>
            /// <exception cref = "T:System.ArgumentNullException"><paramref name = "array" /> is null.</exception>
            /// <exception cref = "T:System.ArgumentOutOfRangeException"><paramref name = "arrayIndex" /> is less than 0.</exception>
            /// <exception cref = "T:System.ArgumentException"><paramref name = "array" /> is multidimensional.-or-<paramref
            ///     name = "arrayIndex" /> is equal to or greater than the length of <paramref name = "array" />.-or-The number of elements in the source <see
            ///      cref = "T:System.Collections.Generic.ICollection`1" /> is greater than the available space from <paramref
            ///      name = "arrayIndex" /> to the end of the destination <paramref name = "array" />.-or-Type cannot be cast automatically to the type of the destination <paramref
            ///      name = "array" />.</exception>
            public void CopyTo(CustomResolver[] array, int arrayIndex)
            {
                _resolvers.CopyTo(array, arrayIndex);
            }

            /// <summary>
            ///     Removes the first occurrence of a specific object from the <see cref = "T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <returns>
            ///     true if <paramref name = "item" /> was successfully removed from the <see
            ///      cref = "T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref
            ///      name = "item" /> is not found in the original <see cref = "T:System.Collections.Generic.ICollection`1" />.
            /// </returns>
            /// <param name = "item">The object to remove from the <see cref = "T:System.Collections.Generic.ICollection`1" />.</param>
            /// <exception cref = "T:System.NotSupportedException">The <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
            public bool Remove(CustomResolver item)
            {
                return item != null && _resolvers.Remove(item);
            }

            /// <summary>
            ///     Gets the number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <returns>
            ///     The number of elements contained in the <see cref = "T:System.Collections.Generic.ICollection`1" />.
            /// </returns>
            public int Count
            {
                get { return _resolvers.Count; }
            }

            /// <summary>
            ///     Gets a value indicating whether the <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.
            /// </summary>
            /// <returns>
            ///     true if the <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
            /// </returns>
            public bool IsReadOnly
            {
                get { return false; }
            }

            #endregion
        }

        #endregion

        #region Macro commands

        private readonly MacroCommandTable _macroCommands = new MacroCommandTable();

        /// <summary>
        ///     Table of macro commands supported by this loader. Macro commands are referenced 
        ///     by <see cref = "SymbolInterpretations.MacroCommand" /> symbols and applied at compile time.
        /// </summary>
        public MacroCommandTable MacroCommands
        {
            [DebuggerStepThrough]
            get { return _macroCommands; }
        }

        private void _initializeMacroCommands()
        {
            _addMacroCommand(CallSub.Instance);
            _addMacroCommand(CallSubInterpret.Instance);
            _addMacroCommand(Pack.Instance);
            _addMacroCommand(Unpack.Instance);
            _addHelperCommands(Unpack.GetHelperCommands());
            _addMacroCommand(Reference.Instance);
            _addHelperCommands(Reference.GetHelperCommands(this));
            _addMacroCommand(CallStar.Instance);

            // Call/* macros
            _addMacroCommand(Call.Instance.Partial);
            _addMacroCommand(Call_Member.Instance.Partial);
            _addMacroCommand(Call_Tail.Instance.Partial);
            _addMacroCommand(CallAsync.Instance.Partial);
            _addCallMacro();
        }

        private void _addCallMacro()
        {
            _addMacroCommand(CallMacro.Instance);
            var callMacroHelper = CallMacro.GetHelperCommands(this);
            _buildCommands.AddCompilerCommand(callMacroHelper.Key, callMacroHelper.Value);
            _addMacroCommand(callMacroHelper.Value.Partial);
        }

        private void _addHelperCommands(IEnumerable<KeyValuePair<string, PCommand>> helpers)
        {
            foreach (var helperCommand in helpers)
                _buildCommands.AddCompilerCommand(helperCommand.Key, helperCommand.Value);
        }

        private void _addMacroCommand(MacroCommand macroCommand)
        {
            MacroCommands.Add(macroCommand);
            if (Options.RegisterCommands)
                Symbols.Declare(macroCommand.Id,
                                ExpandSymbol.Create(EntityRef.MacroCommand.Create(macroCommand.Id)));
        }

        #endregion

        #region Compilation

        [DebuggerStepThrough]
        private void _loadFromStream(Stream str, string filePath)
        {
#if Compression
            if(!str.CanSeek)
                goto noCompression;
            
            byte[] buffer = new byte[6];
            if(str.Read(buffer,0,6) != 6)
                goto noCompression;

            string header = System.Text.Encoding.ASCII.GetString(buffer);
            if(!header.Substring(0,5).Equals("//PXS",StringComparison.InvariantCultureIgnoreCase))
                goto noCompression;

            if(Char.ToUpperInvariant(header[5]) != 'C')
                goto noCompression;

            //Compressed
            GZipStream zip = new GZipStream(str, CompressionMode.Decompress, true);
            str = zip;

            goto compile;

            noCompression:
            str.Position -= 6;

            compile:
#endif
            var reader = new StreamReader(str, Encoding.UTF8);
            _loadFromReader(reader, filePath);
        }

        private void _loadFromReader(TextReader reader, string filePath)
        {
            var lex = new Lexer(reader);
            if (filePath != null)
            {
                lex.File = filePath;
                _loadedFiles.Add(Path.GetFullPath(filePath));
            }

            _load(lex);
        }

        [DebuggerStepThrough]
        public void LoadFromStream(Stream str)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            _loadFromStream(str, null);
        }

        public void LoadFromReader(TextReader reader)
        {
            LoadFromReader(reader, null);
        }

        public void LoadFromReader(TextReader reader, string fileName)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            _loadFromReader(reader, fileName);
        }

#if DEBUG
        private int _loadIndent;
#endif

        [DebuggerStepThrough]
        public void LoadFromFile(string path)
        {
            var file = ApplyLoadPaths(path);
            if (file == null)
            {
                _throwCannotFindScriptFile(path);
                return;
            }
            _loadFromFile(file);
        }

        private void _loadFromFile(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            _loadedFiles.Add(file.FullName);
            _loadPaths.Push(file.DirectoryName);
            using (Stream str = new FileStream(
                file.FullName,
                FileMode.Open,
                FileSystemRights.ReadData,
                FileShare.Read,
                4*1024,
                FileOptions.SequentialScan))
            {
#if DEBUG
                var indent = new StringBuilder(_loadIndent);
                indent.Append(' ', 2*(_loadIndent++));
                Console.WriteLine(Resources.Loader__begin_compiling, file.Name, indent, file.FullName);
#endif
                _loadFromStream(str, file.Name);
#if DEBUG
                Console.WriteLine(Resources.Loader__end_compiling, file.Name, indent);
                _loadIndent--;
#endif
            }

            _loadPaths.Pop();
        }

        [PublicAPI]
        public void RequireFromFile(string path)
        {
            var file = ApplyLoadPaths(path);
            if (file == null)
            {
                _throwCannotFindScriptFile(path);
                return;
            }

            if (_loadedFiles.Contains(file.FullName))
                return;

            _loadFromFile(file);
        }

        private static void _throwCannotFindScriptFile(string path)
        {
            throw new FileNotFoundException(
                "Cannot find script file \"" + path + "\".", path);
        }

        [DebuggerStepThrough]
        public void LoadFromString(string code)
        {
            _load(new Lexer(new StringReader(code)));
        }

        private Action<int, int, string> _reportSemError;

        /// <summary>
        ///     Reports a semantic error to the current parsers error stream. 
        ///     Can only be used while Loader is actively parsing.
        /// </summary>
        /// <param name = "line">The line on which the error occurred.</param>
        /// <param name = "column">The column in which the error occurred.</param>
        /// <param name = "message">The error message.</param>
        /// <exception cref = "InvalidOperationException">when the Loader is not actively parsing.</exception>
        [Obsolete("Use Loader.ReportMessage instead.")]
        public void ReportSemanticError(int line, int column, string message)
        {
            if (_reportSemError == null)
                throw new InvalidOperationException(
                    "The Loader must be parsing when this method is called.");

            _reportSemError(line, column, message);
        }

        private void _messageHook(Object sender, MessageEventArgs e)
        {
            ReportMessage(e.Message);
        }

        [PublicAPI]
        public void ReportMessage(Message message)
        {
            switch (message.Severity)
            {
                case MessageSeverity.Error:
                    _errors.Add(message);
                    break;
                case MessageSeverity.Warning:
                    _warnings.Add(message);
                    break;
                case MessageSeverity.Info:
                    _infos.Add(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private EventHandler<MessageEventArgs> _messageHandler;

        private EventHandler<MessageEventArgs> _getMessageHandler()
        {
            return _messageHandler ?? (_messageHandler = _messageHook);
        }

        private void _load(IScanner lexer)
        {
            var parser = new Parser(lexer, this);

            var oldReportSemError = _reportSemError;
            // TODO: Retire the SemErr API once and for all
#pragma warning disable 612,618
            _reportSemError = parser.SemErr;
#pragma warning restore 612,618
            parser.errors.MessageReceived += _getMessageHandler();
            parser.Parse();

            //Compile initialization function
            var target = FunctionTargets[Application.InitializationId];
            _EmitPartialInitializationCode();
            target.FinishTarget();

            parser.errors.MessageReceived -= _getMessageHandler();
            _reportSemError = oldReportSemError;
        }

        [DebuggerStepThrough]
        internal void _EmitPartialInitializationCode()
        {
            var target = FunctionTargets[Application.InitializationId];
            target.ExecuteCompilerHooks();
            target.Ast.EmitCode(target, false, StackSemantics.Effect);
            //do not treat initialization blocks as top-level ones.
            target.Ast.Clear();
        }

        [PublicAPI]
        public int ErrorCount
        {
            [DebuggerStepThrough]
            get { return _errors.Count; }
        }

        [PublicAPI]
        public List<Message> Errors
        {
            get { return _errors; }
        }

        [PublicAPI]
        public List<Message> Warnings
        {
            get { return _warnings; }
        }

        [PublicAPI]
        public List<Message> Infos
        {
            get { return _infos; }
        }

        private readonly List<Message> _errors = new List<Message>();
        private readonly List<Message> _warnings = new List<Message>();
        private readonly List<Message> _infos = new List<Message>();

        #endregion

        #region Load Path and file table

        private readonly Stack<string> _loadPaths = new Stack<string>();

        public Stack<string> LoadPaths
        {
            get { return _loadPaths; }
        }

        private static readonly string _imageLocation =
            (new FileInfo(Assembly.GetExecutingAssembly().Location)).DirectoryName;

        public FileInfo ApplyLoadPaths(string pathPostfix)
        {
            if (pathPostfix == null)
                throw new ArgumentNullException("pathPostfix");
            var path = pathPostfix;

            //Try to find in process environment
            if (File.Exists(path))
                return new FileInfo(path);

            //Try to find in load paths
            foreach (var pathPrefix in _loadPaths)
                if (File.Exists((path = Path.Combine(pathPrefix, pathPostfix))))
                    return new FileInfo(path);

            //Try to find in engine paths
            foreach (var pathPrefix in ParentEngine.Paths)
                if (File.Exists((path = Path.Combine(pathPrefix, pathPostfix))))
                    return new FileInfo(path);

            //Try to find in current directory
            if (File.Exists((path = Path.Combine(Environment.CurrentDirectory, pathPostfix))))
                return new FileInfo(path);

            //Try to find next to image
            if (File.Exists((path = Path.Combine(_imageLocation, pathPostfix))))
                return new FileInfo(path);

            //Not found
            return null;
        }

        private readonly SymbolCollection _loadedFiles = new SymbolCollection();

        public SymbolCollection LoadedFiles
        {
            get { return _loadedFiles; }
        }

        #endregion

        #region Build Block Commands

        private readonly Stack<object> _buildCommandsRequests = new Stack<object>();

        public CommandTable BuildCommands
        {
            get { return _buildCommands; }
        }

        private readonly CommandTable _buildCommands = new CommandTable();

        /// <summary>
        ///     Request that build commands be added to the <see cref = "ParentEngine" />. Must be matched with a call to <see
        ///      cref = "ReleaseBuildCommands" />.
        /// </summary>
        /// <returns>A unique token that must be passed to <see cref = "ReleaseBuildCommands" /></returns>
        public object RequestBuildCommands()
        {
            if (_buildCommandsRequests.Count == 0)
                _enableBuildCommands();
            var token = new object();
            _buildCommandsRequests.Push(token);
            Debug.Assert(_buildCommandsRequests.Count > 0);
            return token;
        }

        /// <summary>
        ///     Notifies the loader that a caller of <see cref = "RequestBuildCommands" /> no longer requires build commands.
        /// </summary>
        /// <param name = "token">The token returned from the corresponding call to <see cref = "RequestBuildCommands" /></param>
// ReSharper disable UnusedParameter.Global // token is only used for ensuring correct usage of the API
        public void ReleaseBuildCommands(object token)
// ReSharper restore UnusedParameter.Global
        {
            if (_buildCommandsRequests.Count <= 0 ||
                !ReferenceEquals(_buildCommandsRequests.Peek(), token))
                throw new InvalidOperationException(
                    "Cannot release build commands more often than they were requested.");
            _buildCommandsRequests.Pop();

            if (_buildCommandsRequests.Count == 0)
                _disableBuildBlockCommands();
        }

        /// <summary>
        ///     The name of the add command in build blocks.
        /// </summary>
        public const string BuildAddCommand = @"Add";

        /// <summary>
        ///     The name of the require command in build blocks
        /// </summary>
        public const string BuildRequireCommand = @"Require";

        /// <summary>
        ///     The name of the default command in build blocks
        /// </summary>
        public const string BuildDefaultCommand = @"Default";

        /// <summary>
        ///     The name of the hook command for build blocks.
        /// </summary>
        public const string BuildHookCommand = @"Hook";

        /// <summary>
        ///     The name of the resolver command for build blocks.
        /// </summary>
        public const string BuildResolveCommand = "ResolveOperator";

        /// <summary>
        ///     The name of the getloader command for build blocks
        /// </summary>
        public const string BuildGetLoaderCommand = @"GetLoader";

        /// <summary>
        ///     The name of the default script file
        /// </summary>
        public const string DefaultScriptName = "_default.pxs";

        private void _initializeBuildCommands()
        {
            _buildCommands.Clear();
            _buildCommands.AddCompilerCommand(
                BuildAddCommand,
                delegate(StackContext sctx, PValue[] args)
                    {
                        foreach (var arg in args)
                        {
                            var path = arg.CallToString(sctx);
                            LoadFromFile(path);
                        }
                        return null;
                    });

            _buildCommands.AddCompilerCommand(
                BuildRequireCommand,
                delegate(StackContext sctx, PValue[] args)
                    {
                        var allLoaded = true;
                        foreach (var arg in args)
                        {
                            var path = arg.CallToString(sctx);
                            var file = ApplyLoadPaths(path);
                            if (file == null)
                            {
                                _throwCannotFindScriptFile(path);
                                return PType.Null;
                            }
                            if (_loadedFiles.Contains(file.FullName))
                                allLoaded = false;
                            else
                                _loadFromFile(file);
                        }
                        return
                            PType.Bool.CreatePValue(allLoaded);
                    });

            _buildCommands.AddCompilerCommand(
                BuildDefaultCommand,
                delegate
                    {
                        var defaultFile = ApplyLoadPaths(DefaultScriptName);
                        if (defaultFile == null)
                            return DefaultScriptName;
                        else
                            return defaultFile.FullName;
                    });

            _buildCommands.AddCompilerCommand(
                BuildHookCommand,
                delegate(StackContext sctx, PValue[] args)
                    {
                        foreach (var arg in args)
                        {
                            if (arg != null && !arg.IsNull)
                            {
                                if (arg.Type == PType.Object[typeof (AstTransformation)])
                                    CompilerHooks.Add((AstTransformation) arg.Value);
                                else
                                    CompilerHooks.Add(arg);
                            }
                        }
                        return PType.Null.CreatePValue();
                    });

            _buildCommands.AddCompilerCommand(
                BuildResolveCommand,
                delegate(StackContext sctx, PValue[] args)
                    {
                        foreach (var arg in args)
                        {
                            if (arg.Type == PType.Object[typeof (ResolveSymbol)])
                                CustomResolvers.Add(new CustomResolver((ResolveSymbol) arg.Value));
                            else
                                CustomResolvers.Add(new CustomResolver(arg));
                        }
                        return PType.Null.CreatePValue();
                    });

            _buildCommands.AddCompilerCommand(
                BuildGetLoaderCommand,
                (sctx, args) => sctx.CreateNativePValue(this));
        }


        private void _enableBuildCommands()
        {
            foreach (var pair in _buildCommands)
                if (pair.Value.IsInGroup(PCommandGroups.Compiler) &&
                    ! ParentEngine.Commands.ContainsKey(pair.Key))
                    ParentEngine.Commands.AddCompilerCommand(pair.Key, pair.Value);
        }

        private void _disableBuildBlockCommands()
        {
            ParentEngine.Commands.RemoveCompilerCommands();
        }

        public void DeclareBuildBlockCommands(CompilerTarget target)
        {
            foreach (var cmdEntry in _buildCommands)
                target.Symbols.Declare(cmdEntry.Key,CallSymbol.Create(EntityRef.Command.Create(cmdEntry.Key)));
        }

        #endregion

        #region Store

        public void StoreInFile(string path)
        {
#if Compress
            if(Options.Compress)
                using(FileStream fstr = new FileStream(path,FileMode.Create,FileAccess.Write,FileShare.None))
                    StoreCompressed(fstr);
            else
#endif
            using (var writer = new StreamWriter(path, false))
                Store(writer);
        }

        public string StoreInString()
        {
            var writer = new StringWriter();
            Store(writer);
            return writer.ToString();
        }

        public void Store(StringBuilder builder)
        {
            using (var writer = new StringWriter(builder))
                Store(writer);
        }

#if Compress

        public void StoreCompressed(Stream str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            using (GZipStream zip = new GZipStream(str, CompressionMode.Compress, true))
            using (StreamWriter writer = new StreamWriter(zip, Encoding.UTF8))
            {
                writer.WriteLine("//PXSC");
                Store(writer);
            }
        }

#endif

        public void Store(TextWriter writer)
        {
            var app = ParentApplication;

            //Header
            writer.WriteLine("//PXS_");
            writer.WriteLine("//--GENERATED");

            //Meta information
            StoreMetaInformation(writer);

            //Global variables
            writer.WriteLine("\n//--GLOBAL VARIABLES");
            foreach (var kvp in app.Variables)
            {
                writer.Write("var ");
                writer.Write(StringPType.ToIdLiteral(kvp.Key));
                var metaTable = kvp.Value.Meta.Clone();
                metaTable.Remove(Application.IdKey);
#pragma warning disable 612,618
                metaTable.Remove(Application.InitializationGeneration);
#pragma warning restore 612,618
                metaTable.Remove(SuppressPrimarySymbol);
#if DEBUG || Verbose
                    writer.WriteLine();
#endif
                writer.Write(@"[");
                writer.Write(SuppressPrimarySymbol);
                writer.Write(";");
#if DEBUG || Verbose
                    writer.WriteLine();
#endif
                metaTable.Store(writer);
                writer.Write("]");
#if DEBUG || Verbose
                    writer.WriteLine();
#endif
                writer.Write(";");
#if DEBUG || Verbose
                writer.WriteLine();
#endif
            }

            //Functions
            writer.WriteLine("\n//--FUNCTIONS");
            app.Functions.Store(writer);

            //add the initialization function only 
            if (app._InitializationFunction.Code.Count > 0)
                app._InitializationFunction.Store(writer);

            //Symbols
            if (Options.StoreSymbols)
                StoreSymbols(writer);
        }

        private class SymbolKinds
        {
            public readonly List<Tuple<string, int, EntityRef.Function>> Functions =
                new List<Tuple<string, int, EntityRef.Function>>();
            public readonly List<Tuple<string, int, EntityRef.Function>> MacroFunctions =
                new List<Tuple<string, int, EntityRef.Function>>();
            public readonly List<Tuple<string,int,EntityRef.Command>> Commands =
                new List<Tuple<string,int,EntityRef.Command>>();
            public readonly List<Tuple<string,int,EntityRef.Variable.Global>> Variables =
                new List<Tuple<string,int,EntityRef.Variable.Global>>();
            public readonly List<Tuple<string, int, EntityRef.Variable.Global>> MacroVariables =
                new List<Tuple<string, int, EntityRef.Variable.Global>>();
            public readonly List<Tuple<string,int,EntityRef.MacroCommand>> MacroCommands =
                new List<Tuple<string,int,EntityRef.MacroCommand>>();

            public string Key;
            public int CurrentDereferenceCount;
            public bool MacroMode;
        }

// ReSharper disable InconsistentNaming
        private static readonly EntitySplitMatcher EntitySplit = new EntitySplitMatcher();
// ReSharper restore InconsistentNaming
        private class EntitySplitMatcher : EntityRefMatcher<SymbolKinds, object>
        {
            [ContractAnnotation("=> halt")]
            protected override object OnNotMatched(EntityRef entity, SymbolKinds argument)
            {
                throw new PrexoniteException(String.Format("Cannot split up entity {0} into entity types.", entity));
            }

            protected override object OnCommand(EntityRef.Command command, SymbolKinds argument)
            {
                if(argument.MacroMode)
                    throw new PrexoniteException("Cannot create symbol that represents compile-time expansion of runtime command.");
                argument.Commands.Add(Tuple.Create(argument.Key,argument.CurrentDereferenceCount,command));
                return null;
            }

            protected override object OnMacroCommand(EntityRef.MacroCommand macroCommand, SymbolKinds argument)
            {
                if (!argument.MacroMode)
                    throw new PrexoniteException(string.Format("Cannot create symbol that represents runtime call to macro command {0}.", macroCommand.Id));
                argument.MacroCommands.Add(Tuple.Create(argument.Key,argument.CurrentDereferenceCount,macroCommand));
                return null;
            }

            public override object OnFunction(EntityRef.Function function, SymbolKinds argument)
            {
                var entry = Tuple.Create(argument.Key, argument.CurrentDereferenceCount, function);
                if (argument.MacroMode)
                    argument.MacroFunctions.Add(entry);
                else
                    argument.Functions.Add(entry);
                return null;
            }

            protected override object OnGlobalVariable(EntityRef.Variable.Global variable, SymbolKinds argument)
            {
                var entry = Tuple.Create(argument.Key, argument.CurrentDereferenceCount, variable);
                if (argument.MacroMode)
                    argument.MacroVariables.Add(entry);
                else
                    argument.Variables.Add(entry);
                return null;
            }
        }

// ReSharper disable InconsistentNaming
        private static readonly SymbolKindSplitHandler SymbolKindSplit = new SymbolKindSplitHandler();
// ReSharper restore InconsistentNaming
        private class SymbolKindSplitHandler : ISymbolHandler<SymbolKinds, object>
        {
            public object HandleCall(CallSymbol self, SymbolKinds argument)
            {
                return self.Entity.Match(EntitySplit, argument);
            }

            public object HandleExpand(ExpandSymbol self, SymbolKinds argument)
            {
                var oldMode = argument.MacroMode;
                argument.MacroMode = true;
                try
                {
                    return self.Entity.Match(EntitySplit, argument);
                } 
                finally
                {
                    argument.MacroMode = oldMode;
                }
            }

            public object HandleMessage(MessageSymbol self, SymbolKinds argument)
            {
                // the second condition is included in the first, but helps ReSharper understand
                //  that the else-branch cannot result in a null reference exception.
                if(self.Message.Severity == MessageSeverity.Error || self.Symbol == null)
                {
                    return null; //don't emit error symbols
                }
                else
                {
                    return self.Symbol.HandleWith(this, argument);
                }
            }

            public object HandleDereference(DereferenceSymbol self, SymbolKinds argument)
            {
                argument.CurrentDereferenceCount++;
                return self.Symbol.HandleWith(this, argument);
            }

            public object HandleReferenceTo(ReferenceToSymbol self, SymbolKinds argument)
            {
                argument.CurrentDereferenceCount--;
                return self.Symbol.HandleWith(this, argument);
            }

            public object HandleMacroInstance(MacroInstanceSymbol self, SymbolKinds argument)
            {
                return null;
            }
        }

        /// <summary>
        ///     Writes only the symbol declarations to the text writer (regardless of the <see cref = "LoaderOptions.StoreSymbols" /> property.)
        /// </summary>
        /// <param name = "writer">The writer to write the declarations to.</param>
        public void StoreSymbols(TextWriter writer)
        {
            writer.WriteLine("\n//--SYMBOLS");
            var split = new SymbolKinds();

            foreach (var kvp in Symbols)
            {
                split.Key = kvp.Key;
                kvp.Value.HandleWith(SymbolKindSplit, split);
            }

            _writeSymbolKind(writer, "command", split.Commands);
            _writeSymbolKind(writer, "function", split.Functions);
            _writeSymbolKind(writer, "var", split.Variables);
            _writeSymbolKind(writer, "macro command", split.MacroCommands);
            _writeSymbolKind(writer, "macro function", split.MacroFunctions);
            _writeSymbolKind(writer, "macro var", split.MacroVariables);
        }

        /// <summary>
        ///     Writes only meta information to the specified stream.
        /// </summary>
        /// <param name = "writer">The writer to write meta information to.</param>
        public void StoreMetaInformation(TextWriter writer)
        {
            writer.WriteLine("\n//--META INFORMATION");
            ParentApplication.Meta.Store(writer);
        }

        private static void _writeSymbolKind<T>(
            TextWriter writer,
            string kind,
            ICollection<Tuple<string, int, T>> entries) where T : EntityRef
        {
            if (entries.Count <= 0)
                return;

            foreach (var entry in entries.GroupBy(x => x.Item2))
            {
                writer.Write("declare ");
                var dereferenceCount = entry.Key;
                // =0 <=> ordinary symbol
                // <0 <=> referenceTo
                // >0 <=> dereference
                if(dereferenceCount < 0)
                    writer.Write(String.Concat(Enumerable.Repeat("-> ",-dereferenceCount)));
                else if(dereferenceCount > 0)
                    writer.Write(String.Concat(Enumerable.Repeat("ref ",dereferenceCount)));

                writer.Write(kind);
                writer.Write(' ');
                var isFirst = true;
                foreach (var kvp in entry)
                {
                    if (!isFirst)
                        writer.Write(",");
                    else
                        isFirst = false;

                    var sym = kvp.Item3.ToSymbolEntry();
                    writer.Write(StringPType.ToIdLiteral(sym.InternalId));

                    if (sym.Module != null)
                    {
                        writer.Write("/{0}/{1}", StringPType.ToIdOrLiteral(sym.Module.Id),
                            sym.Module.Version);
                    }

                    if (!Engine.StringsAreEqual(sym.InternalId, kvp.Item1))
                    {
                        writer.Write(" as ");
                        writer.Write(StringPType.ToIdLiteral(kvp.Item1));
                    }
                }
                writer.WriteLine(";");
            }
        }

        #endregion

        #region Stack Context

        public override sealed Engine ParentEngine
        {
            [DebuggerStepThrough]
            get { return _options.ParentEngine; }
        }

        public PFunction Implementation
        {
            [DebuggerStepThrough]
            get { return Options.TargetApplication._InitializationFunction; }
        }

        public override sealed Application ParentApplication
        {
            get { return Options.TargetApplication; }
        }

        public override sealed SymbolCollection ImportedNamespaces
        {
            get { return Options.TargetApplication._InitializationFunction.ImportedNamespaces; }
        }

        [DebuggerStepThrough]
        protected override bool PerformNextCycle(StackContext lastContext)
        {
            return false;
        }

        public override PValue ReturnValue
        {
            [DebuggerStepThrough]
            get { return Options.ParentEngine.CreateNativePValue(Options.TargetApplication); }
        }

        public override bool TryHandleException(Exception exc)
        {
            //Cannot handle exceptions.
            return false;
        }

        #endregion

        #region String Caching

        private readonly Dictionary<string, string> _stringCache = new Dictionary<string, string>();

        /// <summary>
        ///     Caches strings encountered while loading code.
        /// </summary>
        /// <param name = "toCache">The string to cache.</param>
        /// <returns>The cached instance of the supplied string. </returns>
        [DebuggerStepThrough]
        public string CacheString(string toCache)
        {
            string cached;
            if (_stringCache.TryGetValue(toCache, out cached))
            {
                return cached;
            }
            else
            {
                cached = toCache;
                _stringCache.Add(toCache, cached);
                return cached;
            }
        }

        #endregion

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Cil")] public const string CilHintsKey = "cilhints";

        public const string ObjectCreationFallbackPrefix = "create_";
        private static readonly ISymbolHandler<List<Message>, Symbol> _listMessages = new ListMessagesHandler();

        private class ListMessagesHandler : ISymbolHandler<List<Message>, Symbol>
        {
            #region Implementation of ISymbolHandler<in List<Message>,out Symbol>

            public Symbol HandleCall(CallSymbol self, List<Message> argument)
            {
                return self;
            }

            public Symbol HandleExpand(ExpandSymbol self, List<Message> argument)
            {
                return self;
            }

            public Symbol HandleMessage(MessageSymbol self, List<Message> argument)
            {
                // Add the symbol to the list and unwrap contents
                argument.Add(self.Message);
                if (self.Symbol == null)
                    return null;
                else
                    return self.Symbol.HandleWith(this, argument);
            }

            public Symbol HandleDereference(DereferenceSymbol self, List<Message> argument)
            {
                var s = self.Symbol.HandleWith(this,argument);
                if (s == null)
                    return null;
                else if (ReferenceEquals(s, self.Symbol))
                    return self;
                else
                    return DereferenceSymbol.Create(self);
            }

            public Symbol HandleReferenceTo(ReferenceToSymbol self, List<Message> argument)
            {
                var s = self.Symbol.HandleWith(this, argument);
                if (s == null)
                    return null;
                else if (ReferenceEquals(s, self.Symbol))
                    return self;
                else
                    return ReferenceToSymbol.Create(self);
            }

            public Symbol HandleMacroInstance(MacroInstanceSymbol self, List<Message> argument)
            {
                return self;
            }

            #endregion
        }

        [ContractAnnotation("=>true,symbol:notnull; =>false,symbol:null")]
        internal bool _TryUseSymbol(ref Symbol symbol, [NotNull] ISourcePosition position)
        {
            var msgs = new List<Message>(1);
            // symbol could be null.
            symbol = symbol.HandleWith(_listMessages, msgs);
            if (msgs.Count > 0)
            {
                var seen = new HashSet<String>();
                foreach (var message in msgs)
                {
                    var c = message.MessageClass;
                    if (c != null)
                        if (!seen.Add(c))
                            continue;

                    ReportMessage(message.Repositioned(position));
                    if (message.Severity == MessageSeverity.Error)
                    {
                        symbol = null;
                        return false;
                    }
                }
            }
            return true;
        }
    }
}