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
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class CreateModuleName : PCommand, ICilCompilerAware
    {

        #region Singleton pattern

        private static readonly CreateModuleName _instance = new CreateModuleName();

        public static CreateModuleName Instance
        {
            get { return _instance; }
        }

        private CreateModuleName()
        {
        }

        #endregion

        public const string Alias = "create_module_name";

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if(args.Length < 1)
                throw new PrexoniteException(Alias + "(...) requires at least one argument.");

            PValue rawVersion;
            
            if(args.Length == 1)
            {
                if(args[0].Type == PType.Object[typeof(MetaEntry)])
                {
                    var entry = (MetaEntry) args[0].Value;
                    ModuleName moduleName;
                    if (ModuleName.TryParse(entry, out moduleName))
                        return sctx.CreateNativePValue(sctx.Cache[moduleName]);
                    else
                        return PType.Null;
                }
                else
                {
                    var raw = args[0].CallToString(sctx);

                    ModuleName moduleName;
                    if (ModuleName.TryParse(raw, out moduleName))
                        return sctx.CreateNativePValue(sctx.Cache[moduleName]);
                    else
                        return PType.Null;
                }
            }
            else if((rawVersion = args[1]).Type.Equals(PType.Object[typeof(Version)]))
            {
                var raw = args[0].CallToString(sctx);

                return
                    sctx.CreateNativePValue(sctx.Cache[new ModuleName(raw,
                        (Version) rawVersion.Value)]);
            }
            else
            {
                var raw = args[0].CallToString(sctx);

                Version version;
                if (Version.TryParse(rawVersion.CallToString(sctx), out version))
                    return sctx.CreateNativePValue(sctx.Cache[new ModuleName(raw, version)]);
                else
                    return PType.Null;
            }
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException("The command " + Alias +
                " does provide a custom cil implementation. ");
        }
    }
}