/*
 * Prx, a standalone command line interface to the Prexonite scripting engine.
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Text
{
    public class SetLeftCommand : PCommand, ICilCompilerAware
    {
        #region Singleton

        private SetLeftCommand()
        {
        }

        private static readonly SetLeftCommand _instance = new SetLeftCommand();

        public static SetLeftCommand Instance
        {
            get { return _instance; }
        }

        #endregion 

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
            int w;
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
            w = (int) args[0].ConvertTo(sctx, PType.Int).Value;
            if (args.Length > 2)
                f = args[2].CallToString(sctx);
            else
                f = " ";

            return SetLeft(w, s, f);
        }

        public static string SetLeft(int w, string s, string f)
        {
            int fl = f.Length;
            int l = s.Length;
            if (l >= w)
                return s;

            StringBuilder sb = new StringBuilder(w, w);
            sb.Append(s);

            for (; l < w; l += fl)
                sb.Append(f);
            sb.Length = w;
            return sb.ToString();
        }

        public static string SetLeft(int w, string s)
        {
            return SetLeft(w, s, " ");
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        #region ICilCompilerAware Members

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}