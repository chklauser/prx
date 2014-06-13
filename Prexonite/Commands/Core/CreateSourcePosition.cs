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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class CreateSourcePosition : PCommand, ICilCompilerAware
    {
        #region Singleton

        private static readonly CreateSourcePosition _instance = new CreateSourcePosition();

        public static CreateSourcePosition Instance { get { return _instance; } }

        private CreateSourcePosition()
        {
            
        }

        public const string Alias = "create_source_position";

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args.Length == 0)
            {
                return sctx.CreateNativePValue(NoSourcePosition.Instance);
            }

            string file = args[0].CallToString(sctx);
            int? line, column;

            PValue box;
            if (args.Length >= 2 && args[1].TryConvertTo(sctx, IntPType.Instance,true,out box))
            {
                line = (int) box.Value;
            }
            else
            {
                line = null;
            }

            if (args.Length >= 3 && args[2].TryConvertTo(sctx, IntPType.Instance, true, out box))
            {
                column = (int) box.Value;
            }
            else
            {
                column = null;
            }

            return sctx.CreateNativePValue(new SourcePosition(file, line ?? -1, column ?? -1));
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
           return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException("The command " + Alias + " does not provide a custom CIL implementation.");
        }
    }
}
