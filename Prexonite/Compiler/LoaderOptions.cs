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
using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;

#nullable enable

namespace Prexonite.Compiler
{
    [DebuggerStepThrough]
    public class LoaderOptions
    {
        #region Construction

        public LoaderOptions(Engine? parentEngine, Application? targetApplication)
        {
            ParentEngine = parentEngine;
            TargetApplication = targetApplication;
            ExternalSymbols = new EmptySymbolView<Symbol>();
        }

        public LoaderOptions([NotNull] Engine parentEngine, [NotNull] Application targetApplication, ISymbolView<Symbol>? externalSymbols)
        {
            ParentEngine = parentEngine ?? throw new ArgumentNullException(nameof(parentEngine));
            TargetApplication = targetApplication ?? throw new ArgumentNullException(nameof(targetApplication));
            ExternalSymbols = externalSymbols ?? throw new ArgumentNullException(nameof(externalSymbols));
        }

        #endregion

        #region Properties

        public Engine? ParentEngine { get; }

        public Application? TargetApplication { get; }

        [NotNull]
        public ISymbolView<Symbol> ExternalSymbols { get; }

        private bool? _registerCommands;
        public bool RegisterCommands
        {
            get => _registerCommands ?? true;
            set => _registerCommands = value;
        }

        private bool? _reconstructSymbols;
        public bool ReconstructSymbols
        {
            get => _reconstructSymbols ?? true;
            set => _reconstructSymbols = value;
        }

        private bool? _storeSymbols;
        public bool StoreSymbols
        {
            get => _storeSymbols ?? true;
            set => _storeSymbols = value;
        }

        private bool? _dumpExternalSymbols;

        /// <summary>
        /// Indicates whether the loader will include external symbols when storing a representation of the application.
        /// </summary>
        /// <para>
        /// This is only useful to diagnose the symbol environment that the loader is working with. While the resulting
        /// image can be loaded it should not be used in production code as it effectively re-exports all of the symbols
        /// of all of its dependencies (including conflicts).
        /// </para>
        public bool DumpExternalSymbols
        {
            get => _dumpExternalSymbols ?? false;
            set => _dumpExternalSymbols = value;
        }

        private bool? _useIndicesLocally;
        public bool UseIndicesLocally
        {
            get => _useIndicesLocally ?? true;
            set => _useIndicesLocally = value;
        }

        private bool? _storeSourceInformation;
        public bool StoreSourceInformation
        {
            get => _storeSourceInformation ?? false;
            set => _storeSourceInformation = value;
        }

        private bool? _preflightModeEnabled;

        /// <summary>
        /// Preflight mode causes the parser to abort at the 
        /// first non-meta construct, giving the user the opportunity 
        /// to inspect a file's "header" without fully compiling 
        /// that file.
        /// </summary>
        public bool PreflightModeEnabled
        {
            get => _preflightModeEnabled ?? false;
            set => _preflightModeEnabled = value;
        }

        private bool? _flagLiteralsEnabled;

        ///<summary>
        /// Determines whether flag literals (-f, --query, --option=value) are parsed globally. Not backwards compatible
        /// because of overlap with unary minus and pre-decrement.
        ///</summary>
        [PublicAPI]
        public bool FlagLiteralsEnabled
        {
            get => _flagLiteralsEnabled ?? false;
            set => _flagLiteralsEnabled = value;
        }

        [CanBeNull]
        private string? _storeNewLine;

        /// <summary>
        /// The line separator to use when storing a compiled Prexonite program. 
        /// </summary>
        /// <see cref="Loader.Store(System.Text.StringBuilder)"/>
        [PublicAPI]
        [NotNull]
        public string StoreNewLine
        {
            get => _storeNewLine ?? "\n";
            set => _storeNewLine = value;
        }

        #endregion

        public void InheritFrom([NotNull] LoaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _registerCommands ??= options._registerCommands;
            _reconstructSymbols ??= options._reconstructSymbols;
            _storeSymbols ??= options._storeSymbols;
            _dumpExternalSymbols ??= options._dumpExternalSymbols;
            _useIndicesLocally ??= options._useIndicesLocally;
            _storeSourceInformation ??= options._storeSourceInformation;
            _preflightModeEnabled ??= options._preflightModeEnabled;
            _flagLiteralsEnabled ??= options._flagLiteralsEnabled;
            _storeNewLine ??= options._storeNewLine;
        }
    }
}