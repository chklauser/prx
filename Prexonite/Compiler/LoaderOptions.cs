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

using System;
using System.Diagnostics;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler
{
    [DebuggerStepThrough]
    public class LoaderOptions
    {
        #region Construction

        public LoaderOptions(Engine parentEngine, Application targetApplication)
        {
            _parentEngine = parentEngine;
            _targetApplication = targetApplication;
        }

        #endregion

        #region Properties

        private readonly Engine _parentEngine;

        public Engine ParentEngine
        {
            get { return _parentEngine; }
        }

        private readonly Application _targetApplication;

        public Application TargetApplication
        {
            get { return _targetApplication; }
        }

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

        #endregion

        public void InheritFrom(LoaderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            RegisterCommands = _registerCommands ?? options.RegisterCommands;
            ReconstructSymbols = _reconstructSymbols ?? options.ReconstructSymbols;
            StoreSymbols = _storeSymbols ?? options.StoreSymbols;
            UseIndicesLocally = _useIndicesLocally ?? options.UseIndicesLocally;
            StoreSourceInformation = _storeSourceInformation ?? options.StoreSourceInformation;
        }
    }
}