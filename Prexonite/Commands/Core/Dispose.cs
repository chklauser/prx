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
using System.ComponentModel;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Command that calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
    /// </summary>
    /// <remarks>
    /// Note that only wrapped .NET objects are disposed. Custom types that respond to "Dispose" are ignored.
    /// </remarks>
    public sealed class Dispose : PCommand, ICilCompilerAware
    {
        private Dispose()
        {
        }

        private static readonly Dispose _instance = new Dispose();

        public static Dispose Instance
        {
            get { return _instance; }
        }

        public const string DisposeMemberId = "Dispose";

        /// <summary>
        /// Executes the dispose function.<br />
        /// Calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
        /// </summary>
        /// <param name="sctx">The stack context. Ignored by this command.</param>
        /// <param name="args">The list of values to dispose.</param>
        /// <returns>Always null.</returns>
        /// <remarks><para>
        /// Dispose tries to call the implementation of the IDisposable interface first before issuing dynamic calls.</para>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        /// <summary>
        /// Executes the dispose function.<br />
        /// Calls <see cref="IDisposable.Dispose"/> on object values that support the interface.
        /// </summary>
        /// <param name="sctx">The stack context. Ignored by this command.</param>
        /// <param name="args">The list of values to dispose.</param>
        /// <returns>Always null.</returns>
        /// <remarks><para>
        /// Dispose tries to call the implementation of the IDisposable interface first before issuing dynamic calls.</para>
        /// </remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            foreach (var arg in args)
                if (arg != null)
                {
                    RunStatically(arg, sctx);
                }
            return PType.Null.CreatePValue();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RunStatically(PValue arg, StackContext sctx)
        {
            PValue dummy;
            if (arg.Type is ObjectPType)
            {
                var toDispose = arg.Value as IDisposable;
                if (toDispose != null)
                    toDispose.Dispose();
                else
                {
                    var isObj = arg.Value as IObject;
                    if (isObj != null)
                    {
                        isObj.TryDynamicCall(
                            sctx, new PValue[0], PCall.Get, DisposeMemberId, out dummy);
                    }
                }
            }
            else
            {
                arg.TryDynamicCall(sctx, new PValue[0], PCall.Get, DisposeMemberId, out dummy);
            }
        }


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
            return CompilationFlags.PrefersCustomImplementation;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            switch (ins.Arguments)
            {
                case 0:
                    if (!ins.JustEffect)
                        state.EmitLoadPValueNull();
                    break;
                case 1:
                    //Emit call to RunStatically(PValue, StackContext)
                    state.EmitLoadLocal(state.SctxLocal);
                    var run =
                        typeof (Dispose).GetMethod("RunStatically", new[] {typeof (PValue), typeof (StackContext)});
                    state.Il.EmitCall(OpCodes.Call, run, null);
                    if (!ins.JustEffect)
                        state.EmitLoadPValueNull();
                    break;
                default:
                    //Emit call to RunStatically(StackContext, PValue[])
                    state.EmitEarlyBoundCommandCall(typeof (Dispose), ins);
                    break;
            }
        }

        #endregion
    }
}