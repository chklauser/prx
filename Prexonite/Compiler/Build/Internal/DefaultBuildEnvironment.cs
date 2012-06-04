// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    class DefaultBuildEnvironment : IBuildEnvironment
    {
        private readonly CancellationToken _token;
        private readonly Dictionary<ModuleName, ITarget> _dependencies;
        private readonly SymbolStore _externalSymbols;
        private readonly ITargetDescription _description;
        private readonly Engine _compilationEngine;
        private readonly Module _module;

        public bool TryGetModule(ModuleName moduleName, out Module module)
        {
            ITarget target;
            if(_dependencies.TryGetValue(moduleName, out target))
            {
                module = target.Module;
                return true;
            }
            else
            {
                module = null;
                return false;
            }
        }

        public DefaultBuildEnvironment(IEnumerable<ITarget> dependencies, ITargetDescription description, CancellationToken token)
        {
            _token = token;
            _description = description;
            _dependencies = new Dictionary<ModuleName, ITarget>();
            var externals = new List<SymbolInfo>();
            foreach (var d in dependencies)
            {
                _dependencies.Add(d.Name, d);
                if(d.Symbols != null)
                {
                    var origin = new SymbolOrigin.ModuleTopLevel(_description.Name, NoSourcePosition.Instance);
                    externals.AddRange(from decl in d.Symbols
                                       select new SymbolInfo(decl.Value, origin, decl.Key));
                }
            }
            _externalSymbols = SymbolStore.Create(conflictUnionSource: externals);
            _compilationEngine = new Engine();
            _module = Module.Create(description.Name);
        }

        public SymbolStore ExternalSymbols
        {
            get { return _externalSymbols; }
        }

        public Loader CreateLoader(LoaderOptions defaults)
        {
            var lowPrioritySymbols = defaults.Symbols;
            SymbolStore predef;
            if(lowPrioritySymbols.Count == 0)
            {
                predef = SymbolStore.Create(ExternalSymbols);
            }
            else
            {
                // First create an intermediate symbol store as a copy
                //  of ExternalSymbols, inheriting from lowPrioritySymbols
                predef = SymbolStore.Create(lowPrioritySymbols);
                foreach (var externalSymbol in ExternalSymbols)
                    predef.Declare(externalSymbol.Key, externalSymbol.Value);
                // Then create the final SymbolStore inheriting from the intermediate one.
                //  that way we can later extract the declarations made by this module.
                predef = SymbolStore.Create(predef); 
            }
            var finalOptions = new LoaderOptions(_compilationEngine, new Application(_module), predef)
                {ReconstructSymbols = false, RegisterCommands = false};
            return new Loader(finalOptions);
        }
    }
}