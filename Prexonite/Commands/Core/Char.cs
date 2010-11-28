/*
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
using System.Collections.Generic;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class Char : PCommand, ICilCompilerAware, ICilExtension
    {
        private Char()
        {
        }

        private static readonly Char _instance = new Char();

        public static Char Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("Char requires at least one argument.");

            PValue v;
            var arg = args[0];
            if (arg.Type == PType.String)
            {
                var s = (string) arg.Value;
                if (s.Length == 0)
                    throw new PrexoniteException("Cannot create char from empty string.");
                else
                    return s[0];
            }
            else if (arg.TryConvertTo(sctx, PType.Char, true, out v))
            {
                return v;
            }
            else if (arg.TryConvertTo(sctx, PType.Int, true, out v))
            {
                return (char) (int) v.Value;
            }
            else
            {
                throw new PrexoniteException("Cannot create char from " + arg);
            }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        #region Implementation of ICilCompilerAware

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of ICilExtension

        bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            string literal;
            int code;
            return dynamicArgc == 0 && staticArgv.Length == 1 && (staticArgv[0].TryGetString(out literal) && literal.Length > 0 || staticArgv[0].TryGetInt(out code) && code >= 0);
        }

        void ICilExtension.Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            if (ins.JustEffect)
                return; //ValidateArguments proved that there are no arguments on the stack.
            string literal;
            int code;
            if (staticArgv[0].TryGetString(out literal))
                code = literal[0];
            else if (!staticArgv[0].TryGetInt(out code))
                throw new ArgumentException(
                    "char command requires one argument that is either a string or a 32-bit integer with the most significant bit cleared.");

            state.EmitLdcI4(code);
            state.EmitWrapChar();
        }

        #endregion
    }
}