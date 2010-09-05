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
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Math
{
    public class Round : PCommand, ICilCompilerAware
    {
        #region Singleton

        private Round()
        {
        }

        private static readonly Round _instance = new Round();

        public static Round Instance
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
                throw new PrexoniteException("Round requires at least one argument.");

            var arg0 = args[0];
            var arg1 = args.Length > 1 ? args[1] : null;

            return RunStatically(arg0, arg1, sctx);
        }

        public static PValue RunStatically(PValue arg0, PValue arg1, StackContext sctx)
        {
            var x = (double) arg0.ConvertTo(sctx, PType.Real, true).Value;

            int d;
            PValue pd;

            if (arg1 != null && arg1.TryConvertTo(sctx, PType.Int, true, out pd))
                d = System.Math.Abs((int) pd.Value);
            else
                d = 0;

            return System.Math.Round(x, d, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
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
            switch (ins.Arguments)
            {
                case 0:
                case 1:
                case 2:
                    return CompilationFlags.PrefersCustomImplementation;
                default:
                    return CompilationFlags.PrefersRunStatically;
            }
        }

        private static readonly MethodInfo RunStaticallyMethod =
            typeof (Round).GetMethod("RunStatically", new[] {typeof (PValue), typeof (PValue), typeof (StackContext)});

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            if (ins.JustEffect)
            {
                for (var i = 0; i < ins.Arguments; i++)
                    state.Il.Emit(OpCodes.Pop);
            }
            else
            {
                switch (ins.Arguments)
                {
                    case 0:
                        state.EmitLoadNullAsPValue();
                        state.Il.Emit(OpCodes.Ldnull);
                        break;
                    case 1:
                        state.Il.Emit(OpCodes.Ldnull);
                        break;
                    case 2:
                        break;
                    default:
                        throw new NotSupportedException();
                }

                state.EmitLoadLocal(state.SctxLocal);
                state.EmitCall(RunStaticallyMethod);
            }
        }

        #endregion
    }
}