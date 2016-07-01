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

namespace Prexonite.Compiler
{
    [DebuggerStepThrough]
    public class LoaderOptions
    {
        #region Construction

        public LoaderOptions([CanBeNull] Engine parentEngine, [CanBeNull] Application targetApplication)
        {
            ParentEngine = parentEngine;
            TargetApplication = targetApplication;
            Symbols = SymbolStore.Create();
        }

        public LoaderOptions([NotNull] Engine parentEngine, [NotNull] Application targetApplication, [NotNull] ISymbolView<Symbol> externalSymbols)
        {
            if (parentEngine == null)
                throw new ArgumentNullException(nameof(parentEngine));
            if (targetApplication == null)
                throw new ArgumentNullException(nameof(targetApplication));
            if (externalSymbols == null)
                throw new ArgumentNullException(nameof(externalSymbols));
            
            ParentEngine = parentEngine;
            TargetApplication = targetApplication;
            Symbols = SymbolStore.Create(externalSymbols);
        }

        #endregion

        #region Properties

        [CanBeNull]
        public Engine ParentEngine { get; }

        [CanBeNull]
        public Application TargetApplication { get; }

        [NotNull]
        public SymbolStore Symbols { get; }

        private bool? _registerCommands;
        public bool RegisterCommands
        {
            get { return _registerCommands ?? true; }
            set { _registerCommands = value; }
        }

        private bool? _reconstructSymbols;
        public bool ReconstructSymbols
        {
            get { return _reconstructSymbols ?? true; }
            set { _reconstructSymbols = value; }
        }

        private bool? _storeSymbols;
        public bool StoreSymbols
        {
            get { return _storeSymbols ?? true; }
            set { _storeSymbols = value; }
        }

        private bool? _useIndicesLocally;
        public bool UseIndicesLocally
        {
            get { return _useIndicesLocally ?? true; }
            set { _useIndicesLocally = value; }
        }

        private bool? _storeSourceInformation;
        public bool StoreSourceInformation
        {
            get { return _storeSourceInformation ?? false; }
            set { _storeSourceInformation = value; }
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
            get { return _preflightModeEnabled ?? false; }
            set { _preflightModeEnabled = value; }
        }

        private bool? _flagLiteralsEnabled;

        ///<summary>
        /// Determines whether flag literals (-f, --query, --option=value) are parsed globally. Not backwards compatible
        /// because of overlap with unary minus and pre-decrement.
        ///</summary>
        [PublicAPI]
        public bool FlagLiteralsEnabled
        {
            get { return _flagLiteralsEnabled ?? false; }
            set { _flagLiteralsEnabled = value; }
        }

        #endregion

        public void InheritFrom([NotNull] LoaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _registerCommands = _registerCommands ?? options._registerCommands;
            _reconstructSymbols = _reconstructSymbols ?? options._reconstructSymbols;
            _storeSymbols = _storeSymbols ?? options._storeSymbols;
            _useIndicesLocally = _useIndicesLocally ?? options._useIndicesLocally;
            _storeSourceInformation = _storeSourceInformation ?? options._storeSourceInformation;
            _preflightModeEnabled = _preflightModeEnabled ?? options._preflightModeEnabled;
            _flagLiteralsEnabled = _flagLiteralsEnabled ?? options._flagLiteralsEnabled;
        }
    }
}