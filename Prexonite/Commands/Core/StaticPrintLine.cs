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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class StaticPrintLine : PCommand, ICilCompilerAware, ICilExtension
    {
        #region Singleton

        private StaticPrintLine()
        {
        }

        private static readonly StaticPrintLine _instance = new StaticPrintLine();

        public static StaticPrintLine Instance
        {
            get { return _instance; }
        }

        #endregion  

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            var text = Concat.ConcatenateString(sctx, args);
            StaticPrint.Writer.WriteLine(text);

            return text;
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
            return CompilationFlags.PrefersRunStatically;
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

        #region Implementation of ICilExtension

        /// <summary>
        /// Checks whether the static arguments and number of dynamic arguments are valid for the CIL extension. 
        /// 
        /// <para>Returning false means that the CIL extension cannot provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler will fall back to  <see cref="ICilCompilerAware"/> and finally the built-in mechanisms.</para>
        /// <para>Returning true means that the CIL extension can provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler may subsequently call <see cref="ICilExtension.Implement"/> with the same set of arguments.</para>
        /// </summary>
        /// <param name="staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name="dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        /// <returns>true if the extension can provide a CIL implementation for the set of arguments; false otherwise</returns>
        public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            return dynamicArgc <= 0 && staticArgv.All(ctv => !ctv.IsReference);
        }

        public void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            var text = String.Concat(staticArgv.Select(StaticPrint._ToString));

            state.EmitCall(StaticPrint._StaticPrintTextWriterGetMethod);
            state.Il.Emit(OpCodes.Ldstr, text);
            if (!ins.JustEffect)
            {
                state.Il.Emit(OpCodes.Dup);
                state.EmitStoreTemp(0);
            }
            state.EmitVirtualCall(_textWriterWriteLineMethod);
            if (!ins.JustEffect)
            {
                state.EmitLoadTemp(0);
                state.EmitWrapString();
            }
        }

        private static readonly MethodInfo _textWriterWriteLineMethod = typeof (TextWriter).GetMethod(
            "WriteLine", new[] {typeof (String)});

        #endregion
    }
}