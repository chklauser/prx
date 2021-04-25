// Prexonite
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
#if Compression
using System.IO.Compression;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NN = JetBrains.Annotations.NotNullAttribute;
using Prexonite.Commands;
using Prexonite.Commands.Concurrency;
using Prexonite.Commands.Core;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Internal;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Compiler.Symbolic;
using Prexonite.Compiler.Symbolic.Internal;
using Prexonite.Internal;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;
using Debug = System.Diagnostics.Debug;

namespace Prexonite.Compiler
{
    public class Loader : StackContext, IMessageSink
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

        public Loader(LoaderOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            
            // See comment at the declaration of these fields
            _commandSymbols = SymbolStore.Create(Options.ExternalSymbols);
            _topLevelImports = SymbolStore.Create(_commandSymbols);
            _topLevelView = ModuleLevelView.Create(SymbolStore.Create(_topLevelImports));

            _functionTargets = new SymbolTable<CompilerTarget>();
            FunctionTargets = new FunctionTargetsIterator(this);

            CreateFunctionTarget(ParentApplication._InitializationFunction);

            if (options.RegisterCommands)
                RegisterExistingCommands();

            CompilerHooks = new CompilerHooksIterator(this);
            CustomResolvers = new CustomResolversIterator(_customResolvers);

            //Build commands
            _initializeBuildCommands();

            //Macro commands
            _initializeMacroCommands();

            //Provide fallback command info for CIL compilation
            _imprintCommandInfo();
        }

        private void _imprintCommandInfo()
        {
            foreach (var buildCommand in BuildCommands)
                ParentEngine.Commands.ProvideFallbackInfo(buildCommand.Key,
                    buildCommand.Value.ToCommandInfo());
        }

        public void RegisterExistingCommands()
        {
            foreach (var kvp in ParentEngine.Commands)
                _commandSymbols.Declare(kvp.Key, Symbol.CreateCall(EntityRef.Command.Create(kvp.Key),NoSourcePosition.Instance));
        }

        #endregion

        #region Options

        public LoaderOptions Options { [DebuggerStepThrough] get; }

        #endregion

        #region Global Symbol Table

        private readonly Stack<DeclarationScope> _declarationScopes = new();
        
        // For the top-level (outside of all namespaces and functions), we use a stack of symbol stores. 
        // From "outside" to "inside", the stack looks as follows:
        //  * ExternalSymbols   (contains symbols exported by other modules)
        //  * _commandSymbols   (contains symbols generated by commands explicitly added to this loader)  
        //  * _topLevelImports  (contains symbols brought into scope by top-level `namespace import` statements) 
        //  * "topLevelSymbols" (contains symbols defined - and exported - on the top level)
        //  * _topLevelView     (a ModuleLevelView adapter through which symbols are accessed; ensures our contributions
        //                       to namespaces remain separated from imported/linked namespace contents)
        // 
        // NOTE: "topLevelSymbols" refers to the symbol store within the _topLevelView. 

        private readonly SymbolStore _commandSymbols;
        private SymbolStore _topLevelImports;
        private readonly ModuleLevelView _topLevelView;

        [CanBeNull]
        public DeclarationScope CurrentScope => _declarationScopes.Count > 0 ? _declarationScopes.Peek() : null;

        public void PushScope([NN] DeclarationScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            _declarationScopes.Push(scope);
        }

        [NN]
        public DeclarationScope PopScope()
        {
            return _declarationScopes.Pop();
        }

        /// <summary>
        /// The top-level symbols for this module. Includes external symbols (from other modules)
        /// and top-level imports.
        /// </summary>
        /// <para>If you simply want to reference the declaration level (outside of all functions, but inside the
        /// "current" namespace), use <see cref="Symbols"/> instead.</para>
        public SymbolStore TopLevelSymbols => _topLevelView;

        /// <summary>
        /// The symbols for the current declaration scope (outside of all functions). 
        /// </summary>
        /// <para>This is either the "current" namespace or the top-level of the module.</para>
        /// <para>Code that declares "global" variables and functions should prefer this property over
        /// <see cref="TopLevelSymbols"/> so that the declared functions and variables end up in the "current"
        /// namespace.</para>
        public SymbolStore Symbols => CurrentScope?.Store ?? TopLevelSymbols;

        /// <summary>
        /// Replace the symbols imported at the top level with the supplied set of symbols.
        /// </summary>
        /// <para>
        /// Imported symbols are not part of the set of exported symbols by the module being compiled. Replacing
        /// top-level imports is a relatively expensive operation and should not occur more than once per module.
        /// </para>
        /// <param name="importedSymbols">The new set of symbols to import at the top level.</param>
        public void ReplaceTopLevelImports([NN] SymbolStoreBuilder importedSymbols)
        {
            importedSymbols.ExistingNamespace = _commandSymbols;
            _topLevelImports = importedSymbols.ToSymbolStore();
            _topLevelView.ExternalScope = _topLevelImports;
        }

        #endregion

        #region Function Symbol Tables

        private readonly SymbolTable<CompilerTarget> _functionTargets;

        public FunctionTargetsIterator FunctionTargets { [DebuggerStepThrough] get; }

        [DebuggerStepThrough]
        public sealed class FunctionTargetsIterator : IEnumerable<CompilerTarget>
        {
            private readonly Loader _outer;

            internal FunctionTargetsIterator(Loader outer)
            {
                _outer = outer;
            }

            public int Count => _outer._functionTargets.Count;

            public CompilerTarget this[string key] => _outer._functionTargets[key];

            public CompilerTarget this[PFunction key] => _outer._functionTargets[key.Id];

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
                throw new ArgumentNullException(nameof(func));

            var target = new CompilerTarget(this, func,parentTarget,sourcePosition);
            if (_functionTargets.ContainsKey(func.Id) &&
                !ParentApplication.Meta.GetDefault(Application.AllowOverridingKey, true).Switch)
                throw new PrexoniteException(
                    $"The application {ParentApplication.Id} does not allow overriding of function {func.Id}.");

            _functionTargets[func.Id] = target;

            return target;
        }

        #endregion

        #region Compiler Hooks

        public CompilerHooksIterator CompilerHooks { [DebuggerStepThrough] get; }

        private readonly List<CompilerHook> _compilerHooks = new();

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
                    throw new ArgumentNullException(nameof(item));
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
                    transformation.Value is AstTransformation transformationNode)
                    _lst.Add(new CompilerHook(transformationNode));
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
                    throw new ArgumentNullException(nameof(array));
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
            public int Count => _lst.Count;

            ///<summary>
            ///    Gets a value indicating whether the <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only.
            ///</summary>
            ///<returns>
            ///    true if the <see cref = "T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.
            ///</returns>
            public bool IsReadOnly => ((ICollection<CompilerHook>) _lst).IsReadOnly;

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

        private readonly List<CustomResolver> _customResolvers = new();

        public CustomResolversIterator CustomResolvers { get; }

        public class CustomResolversIterator : ICollection<CustomResolver>
        {
            private readonly List<CustomResolver> _resolvers;

            internal CustomResolversIterator(List<CustomResolver> outer)
            {
                _resolvers = outer;
            }

            public int Length => _resolvers.Count;

            public CustomResolver this[int index] => _resolvers[index];

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
                    throw new ArgumentNullException(nameof(item));
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
            public int Count => _resolvers.Count;

            /// <summary>
            ///     Gets a value indicating whether the <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only.
            /// </summary>
            /// <returns>
            ///     true if the <see cref = "T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
            /// </returns>
            public bool IsReadOnly => false;

            #endregion
        }

        #endregion

        #region Macro commands

        /// <summary>
        ///     Table of macro commands supported by this loader. Macro commands are referenced 
        ///     by <see cref = "SymbolInterpretations.MacroCommand" /> symbols and applied at compile time.
        /// </summary>
        public MacroCommandTable MacroCommands { [DebuggerStepThrough] get; } = new();

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
            _addMacroCommand(EntityRefTo.Instance);

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
            BuildCommands.AddCompilerCommand(callMacroHelper.Key, callMacroHelper.Value);
            _addMacroCommand(callMacroHelper.Value.Partial);
        }

        private void _addHelperCommands(IEnumerable<KeyValuePair<string, PCommand>> helpers)
        {
            foreach (var helperCommand in helpers)
                BuildCommands.AddCompilerCommand(helperCommand.Key, helperCommand.Value);
        }

        private void _addMacroCommand(MacroCommand macroCommand)
        {
            MacroCommands.Add(macroCommand);
            if (Options.RegisterCommands)
            {
                var commandReference =
                    Cache.EntityRefs.GetCached(EntityRef.MacroCommand.Create(macroCommand.Id));
                _commandSymbols.Declare(
                    macroCommand.Id, Symbol.CreateExpand(Symbol.CreateReference(commandReference, NoSourcePosition.Instance)));
            }
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
                LoadedFiles.Add(Path.GetFullPath(filePath));
            }

            _load(lex);
        }

        [PublicAPI]
        [DebuggerStepThrough]
        public void LoadFromStream(Stream str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            _loadFromStream(str, null);
        }

        [PublicAPI]
        public void LoadFromReader(TextReader reader)
        {
            LoadFromReader(reader, null);
        }

        [PublicAPI]
        public void LoadFromReader(TextReader reader, string fileName)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            _loadFromReader(reader, fileName);
        }

#if DEBUG
        private int _loadIndent;
#endif

        [PublicAPI]
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

        private void _loadFromFile(ISourceSpec file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            LoadedFiles.Add(file.FullName);
            if (file.LoadPath is { } path)
            {
                LoadPaths.Push(path);
            }

            try
            {
                using var str = file.OpenStream();
#if DEBUG
                    var indent = new StringBuilder(_loadIndent);
                    indent.Append(' ', 2 * _loadIndent++);
                    Console.WriteLine(Resources.Loader__begin_compiling, file.FullName, indent, file.FullName);
#endif
                _loadFromStream(str, file.FullName);
#if DEBUG
                    Console.WriteLine(Resources.Loader__end_compiling, file.FullName, indent);
                    _loadIndent--;
#endif
            }
            finally
            {
                if (file.LoadPath != null)
                {
                    LoadPaths.Pop();
                }
            }
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

            if (LoadedFiles.Contains(file.FullName))
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

        private void _messageHook(object sender, MessageEventArgs e)
        {
            ReportMessage(e.Message);
        }

        [PublicAPI]
        public void ReportMessage(Message message)
        {
            switch (message.Severity)
            {
                case MessageSeverity.Error:
                    Errors.Add(message);
                    break;
                case MessageSeverity.Warning:
                    Warnings.Add(message);
                    break;
                case MessageSeverity.Info:
                    Infos.Add(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private EventHandler<MessageEventArgs> _messageHandler;

        private EventHandler<MessageEventArgs> _getMessageHandler()
        {
            return _messageHandler ??= _messageHook;
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
            get => Errors.Count;
        }

        [PublicAPI]
        public List<Message> Errors { get; } = new();

        [PublicAPI]
        public List<Message> Warnings { get; } = new();

        [PublicAPI]
        public List<Message> Infos { get; } = new();

        #endregion

        #region Load Path and file table

        public Stack<string> LoadPaths { get; } = new();

        /// <summary>
        /// The platform-independent directory separator used by this application. Can be controlled via the
        /// <see cref="DirectorySeparatorKey"/> meta entry. Defaults to <c>/</c> (forward slash).
        /// </summary>
        [PublicAPI]
        public char DirectorySeparator {
            get
            {
                if (ParentApplication.Meta.TryGetValue(DirectorySeparatorKey, out var value) 
                    && value.IsText && value.Text.Length == 1)
                {
                    return value.Text[0];
                }

                return '/';
            }
        }
        
        private static readonly string _imageLocation = AppContext.BaseDirectory;

        [CanBeNull]
        public ISourceSpec ApplyLoadPaths([System.Diagnostics.CodeAnalysis.NotNull] string pathSuffix)
        {
            if (pathSuffix == null)
                throw new ArgumentNullException(nameof(pathSuffix));

            FileInfo applyFilePaths(string path)
            {
                path = path.Replace(DirectorySeparator, Path.DirectorySeparatorChar);

                //Try to find in process environment
                if (File.Exists(path))
                    return new FileInfo(path);

                //Try to find in load paths
                foreach (var pathPrefix in LoadPaths)
                    if (File.Exists(path = Path.Combine(pathPrefix, pathSuffix)))
                        return new FileInfo(path);

                //Try to find in engine paths
                foreach (var pathPrefix in ParentEngine.Paths)
                    if (File.Exists(path = Path.Combine(pathPrefix, pathSuffix)))
                        return new FileInfo(path);

                //Try to find in current directory
                if (File.Exists(path = Path.Combine(Environment.CurrentDirectory, pathSuffix)))
                    return new FileInfo(path);

                //Try to find next to image
                if (File.Exists(path = Path.Combine(_imageLocation, pathSuffix)))
                    return new FileInfo(path);

                //Not found
                return null;
            }

            [CanBeNull]
            static ISourceSpec resolveResourcePath(string path)
            {
                var parts = path.Split('/', 2);
                if (parts.Length < 2)
                {
                    throw new ArgumentException($"Embedded resource path needs to have the format: "
                        + $"'resource:$AssemblyName/$ResourceName'. Got '{path}' instead.", nameof(path));
                }

                var rawAssemblyName = parts[0][9..];
                var resourceName = parts[1];
                var assemblyCandidates = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x =>
                    {
                        var candidateName = x.GetName();
                        return rawAssemblyName == candidateName.Name || rawAssemblyName == candidateName.ToString();
                    })
                    .Take(2)
                    .ToImmutableArray();
                return assemblyCandidates.Length switch
                {
                    <= 0 => null,
                    > 1 => throw new ArgumentException(
                        $"The assembly name '{rawAssemblyName}' matched multiple assemblies. " +
                        $"Examples: '{assemblyCandidates[0].GetName()}' and '{assemblyCandidates[1].GetName()}'",
                        nameof(path)),
                    1 => new ResourceSpec(assemblyCandidates[0], resourceName)
                };
            }

            return pathSuffix.StartsWith("resource:") 
                ? resolveResourcePath(pathSuffix) 
                : new FileSpec(applyFilePaths(pathSuffix));
        }

        public SymbolCollection LoadedFiles { get; } = new();

        #endregion

        #region Build Block Commands

        private readonly Stack<object> _buildCommandsRequests = new();

        public CommandTable BuildCommands { get; } = new();

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
            BuildCommands.Clear();
            BuildCommands.AddCompilerCommand(
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

            BuildCommands.AddCompilerCommand(
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
                            if (LoadedFiles.Contains(file.FullName))
                                allLoaded = false;
                            else
                                _loadFromFile(file);
                        }
                        return
                            PType.Bool.CreatePValue(allLoaded);
                    });

            BuildCommands.AddCompilerCommand(
                BuildDefaultCommand,
                (_, _) => ApplyLoadPaths(DefaultScriptName)?.FullName ?? DefaultScriptName);

            BuildCommands.AddCompilerCommand(
                BuildHookCommand,
                (_, args) =>
                {
                    foreach (var arg in args)
                    {
                        if (arg is {IsNull: false})
                        {
                            if (arg.Type == PType.Object[typeof(AstTransformation)])
                                CompilerHooks.Add((AstTransformation) arg.Value);
                            else
                                CompilerHooks.Add(arg);
                        }
                    }

                    return PType.Null.CreatePValue();
                });

            BuildCommands.AddCompilerCommand(
                BuildResolveCommand,
                delegate(StackContext _, PValue[] args)
                    {
                        foreach (var arg in args)
                        {
                            CustomResolvers.Add(arg.Type == PType.Object[typeof (ResolveSymbol)]
                                ? new CustomResolver((ResolveSymbol) arg.Value)
                                : new CustomResolver(arg));
                        }
                        return PType.Null.CreatePValue();
                    });

            BuildCommands.AddCompilerCommand(
                BuildGetLoaderCommand,
                (sctx, _) => sctx.CreateNativePValue(this));
        }


        private void _enableBuildCommands()
        {
            foreach (var pair in BuildCommands)
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
            foreach (var cmdEntry in BuildCommands)
                target.Symbols.Declare(cmdEntry.Key,Symbol.CreateCall(EntityRef.Command.Create(cmdEntry.Key), NoSourcePosition.Instance));
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
            using var writer = new StreamWriter(path, false);
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
            using var writer = new StringWriter(builder);
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

        private class SymbolSerializationPartitioner : SymbolHandler<string, object>
        {
            [NN]
            private readonly List<KeyValuePair<string, NamespaceSymbol>> _namespaceSymbols = new();

            [NN]
            private readonly IDictionary<Symbol,QualifiedId> _previousSymbols;

            public SymbolSerializationPartitioner([NN] IDictionary<Symbol, QualifiedId> previousSymbols)
            {
                _previousSymbols = previousSymbols;
            }

            [NN]
            public IEnumerable<KeyValuePair<string, NamespaceSymbol>> NamespaceSymbols => _namespaceSymbols;

            public override object HandleNamespace(NamespaceSymbol self, string argument)
            {
                if (_previousSymbols.ContainsKey(self))
                    return null;
                _namespaceSymbols.Add(new KeyValuePair<string,NamespaceSymbol>(argument,self));
                return null;
            }

            protected override object HandleWrappingSymbol(WrappingSymbol self, string argument)
            {
                if (_previousSymbols.ContainsKey(self))
                    return null;
                return self.InnerSymbol.HandleWith(this, argument);
            }

            protected override object HandleSymbolDefault(Symbol self, string argument)
            {
                // just ignore others
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

            var previousSymbols = new Dictionary<Symbol, QualifiedId>();
            var currentPrefix = new QualifiedId(null);
            var scope = Options.DumpExternalSymbols 
                ? (TopLevelSymbols as ModuleLevelView)?.BackingStore ?? TopLevelSymbols 
                : TopLevelSymbols;

            _storeScope(writer, scope, previousSymbols, currentPrefix);
        }

        private void _storeScope(TextWriter writer, IEnumerable<KeyValuePair<string,Symbol>> scope, IDictionary<Symbol, QualifiedId> previousSymbols,
            QualifiedId currentPrefix)
        {
            var partition = new SymbolSerializationPartitioner(previousSymbols);
            var cachedScope = scope.ToArray();
            foreach (var symbol in cachedScope)
                symbol.Value.HandleWith(partition, symbol.Key);
            foreach (var symbol in partition.NamespaceSymbols)
            {
                var namespaceContents = 
                    Options.DumpExternalSymbols ? symbol.Value.Namespace 
                    : symbol.Value.Namespace is LocalNamespace localNamespace ? localNamespace.Exports 
                    : throw new PrexoniteException("Cannot represent external namespace in symbols.");
                writer.Write("namespace ");
                writer.Write(StringPType.ToIdLiteral(symbol.Key));
                writer.WriteLine(" {");
                var nestedPrefix = _recordSymbol(previousSymbols, currentPrefix, symbol.Key,symbol.Value);
                _storeScope(writer, namespaceContents, previousSymbols, nestedPrefix);
                writer.WriteLine("};");
                
            }
            writer.WriteLine("declare(");
            foreach (var symbol in cachedScope)
            {
                writer.Write("  ");
                writer.Write(StringPType.ToIdLiteral(symbol.Key));
                writer.Write(" = ");
                var mexpr = symbol.Value.HandleWith(SymbolMExprSerializer.Instance, previousSymbols);
                mexpr.ToString(writer);
                _recordSymbol(previousSymbols, currentPrefix, symbol.Key,symbol.Value);
                writer.WriteLine(",");
            }
            writer.WriteLine(");");
        }

        private static QualifiedId _recordSymbol(IDictionary<Symbol, QualifiedId> previousSymbols, QualifiedId currentPrefix, string name, Symbol symbol)
        {
            var nestedPrefix = currentPrefix.ExtendedWith(name);
            if(!previousSymbols.ContainsKey(symbol))
                previousSymbols.Add(symbol, nestedPrefix);
            return nestedPrefix;
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

        #endregion

        #region Stack Context

        public sealed override Engine ParentEngine
        {
            [DebuggerStepThrough]
            get => Options.ParentEngine;
        }

        public sealed override Application ParentApplication => Options.TargetApplication;

        public sealed override SymbolCollection ImportedNamespaces =>
            // ReSharper disable PossibleNullReferenceException
            Options.TargetApplication._InitializationFunction.ImportedNamespaces;

        // ReSharper enable PossibleNullReferenceException
        [DebuggerStepThrough]
        protected override bool PerformNextCycle(StackContext lastContext)
        {
            return false;
        }

        public override PValue ReturnValue
        {
            [DebuggerStepThrough]
            get => Options.ParentEngine.CreateNativePValue(Options.TargetApplication);
        }

        public override bool TryHandleException(Exception exc)
        {
            //Cannot handle exceptions.
            return false;
        }

        #endregion

        #region String Caching

        private readonly Dictionary<string, string> _stringCache = new();

        /// <summary>
        ///     Caches strings encountered while loading code.
        /// </summary>
        /// <param name = "toCache">The string to cache.</param>
        /// <returns>The cached instance of the supplied string. </returns>
        [DebuggerStepThrough]
        public string CacheString(string toCache)
        {
            if (_stringCache.TryGetValue(toCache, out var cached))
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
        private const string DirectorySeparatorKey = @"\directory_separator";
    }
}