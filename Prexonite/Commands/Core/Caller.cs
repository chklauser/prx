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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of the caller command. Returns the stack context of the caller.
    /// </summary>
    public sealed class Caller : PCommand, ICilCompilerAware
    {
        private Caller()
        {
        }

        private static readonly Caller _instance = new Caller();

        public static Caller Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Returns the caller of the supplied stack context.
        /// </summary>
        /// <param name="sctx">The stack contetx that wishes to find out, who called him.</param>
        /// <param name="args">Ignored</param>
        /// <returns>Either the stack context of the caller or null encapsulated in a PValue.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return sctx.CreateNativePValue(GetCaller(sctx));
        }

        /// <summary>
        /// Returns the caller of the supplied stack context.
        /// </summary>
        /// <param name="sctx">The stack context that wishes tp find out, who called him.</param>
        /// <returns>Either the stack context of the caller or null.</returns>
        public static StackContext GetCaller(StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            var stack = sctx.ParentEngine.Stack;
            if (!stack.Contains(sctx))
                return null;
            else
            {
                var callee = stack.FindLast(sctx);
                if (callee == null || callee.Previous == null)
                    return null;
                else
                    return callee.Previous.Value;
            }
        }

        public static PValue GetCallerFromCilFunction(StackContext sctx)
        {
            var stack = sctx.ParentEngine.Stack;
            if (stack.Count == 0)
                return PType.Null;
            else
                return sctx.CreateNativePValue(stack.Last.Value);
        }

        private static readonly MethodInfo GetCallerFromCilFunctionMethod =
            typeof (Caller).GetMethod("GetCallerFromCilFunction", new[] {typeof (StackContext)});

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.OperatesOnCaller | CompilationFlags.RequiresCustomImplementation;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            for (var i = 0; i < ins.Arguments; i++)
                state.Il.Emit(OpCodes.Pop);
            if (!ins.JustEffect)
            {
                state.EmitLoadLocal(state.SctxLocal);
                state.Il.EmitCall(OpCodes.Call, GetCallerFromCilFunctionMethod, null);
            }
        }

        #endregion
    }
}