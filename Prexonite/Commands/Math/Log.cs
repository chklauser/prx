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
using System.Reflection;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Log : PCommand, ICilCompilerAware
    {
        #region Singleton

        private Log()
        {
        }

        private static readonly Log _instance = new Log();

        public static Log Instance
        {
            get { return _instance; }
        }

        #endregion

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return true; }
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
                throw new PrexoniteException("Log requires at least one argument.");

            if (args.Length > 1)
            {
                var arg0 = args[0];
                var arg1 = args[1];

                return RunStatically(arg0, arg1, sctx);
            }
            else
            {
                var arg0 = args[0];

                return RunStatically(arg0, sctx);
            }
        }

        public static PValue RunStatically(PValue arg0, PValue arg1, StackContext sctx)
        {
            var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value;
            var b = (double) arg1.ConvertTo(sctx, PType.Real, true).Value;
            return System.Math.Log(x, b);
        }

        public static PValue RunStatically(PValue arg0, StackContext sctx)
        {
            var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value;
            return System.Math.Log(x);
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
            return CompilationFlags.PreferCustomImplementation;
        }

        private static readonly MethodInfo RunStaticallyNaturalMethod =
            typeof (Log).GetMethod("RunStatically", new[] {typeof (PValue), typeof (StackContext)});

        private static readonly MethodInfo RunStaticallyAnyMethod =
            typeof (Log).GetMethod("RunStatically", new[] {typeof (PValue), typeof (PValue), typeof (StackContext)});

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            var argc = ins.Arguments;
            if (ins.JustEffect)
            {
                state.EmitIgnoreArguments(argc);
            }
            else
            {
                if (argc > 2)
                {
                    state.EmitIgnoreArguments(argc - 2);
                    argc = 2;
                }
                switch (argc)
                {
                    case 0:
                        state.EmitLdcI4(0);
                        state.EmitWrapInt();
                        goto case 1;
                    case 1:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitCall(RunStaticallyNaturalMethod);
                        break;
                    case 2:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitCall(RunStaticallyAnyMethod);
                        break;
                }
            }
        }

        #endregion
    }
}