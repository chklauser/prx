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
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Text
{
    public class SetRightCommand : PCommand, ICilCompilerAware
    {
        #region Singleton

        private SetRightCommand()
        {
        }

        private static readonly SetRightCommand _instance = new SetRightCommand();

        public static SetRightCommand Instance
        {
            get { return _instance; }
        }

        #endregion

        [Obsolete]
        public override bool IsPure
        {
            get { return true; }
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            // function setright(w,s,f)
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            string s;
            string f;

            switch (args.Length)
            {
                case 0:
                    return "";
                case 1:
                    s = "";
                    goto parseW;
            }
            s = args[1].CallToString(sctx);
            parseW:
            var w = (int) args[0].ConvertTo(sctx, PType.Int).Value;
            if (args.Length > 2)
                f = args[2].CallToString(sctx);
            else
                f = " ";

            return SetRight(w, s, f);
        }

        public static string SetRight(int w, string s, string f)
        {
            var fl = f.Length;
            var l = s.Length;
            if (l >= w)
                return s;

            var sb = new StringBuilder(w, w);

            for (var i = l; i < w; i += fl)
                sb.Append(f);
            sb.Length = w - l;
            sb.Append(s);
            return sb.ToString();
        }

        public static string SetRight(int w, string s)
        {
            return SetRight(w, s, " ");
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        #region ICilCompilerAware Members

        /// <summary>
        ///     Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name = "ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}