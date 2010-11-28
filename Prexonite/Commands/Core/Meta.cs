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
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core
{
    public class Meta : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Meta()
        {
        }

        private static readonly Meta _instance = new Meta();

        public static Meta Instance
        {
            get { return _instance; }
        }

        #endregion

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.RequiresCustomImplementation;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            if (ins.Arguments > 0)
                throw new PrexoniteException("The meta command no longer accepts arguments.");

            state.EmitLoadLocal(state.SctxLocal);
            state.EmitLoadArg(CompilerState.ParamSourceIndex);
            var getMeta = typeof (PFunction).GetProperty("Meta").GetGetMethod();
            state.Il.EmitCall(OpCodes.Callvirt, getMeta, null);
            state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreateNativePValue, null);
        }

        #endregion

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public virtual bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args != null && args.Length > 0)
                throw new PrexoniteException("The meta command no longer accepts arguments.");

            var fctx = sctx as FunctionContext;

            if (fctx == null)
                throw new PrexoniteException(
                    "The meta command uses dynamic features and can therefor only be called from a Prexonite function.");

            return fctx.CreateNativePValue(fctx.Implementation.Meta);
        }
    }
}