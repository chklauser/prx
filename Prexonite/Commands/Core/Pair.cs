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

using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Turns to arguments into a key-value pair
    /// </summary>
    /// <remarks>
    /// Equivalent to:
    /// <code>function pair(key, value) = key: value;</code>
    /// </remarks>
    public sealed class Pair : PCommand, ICilCompilerAware
    {
        private Pair()
        {
        }

        private static readonly Pair _instance = new Pair();

        public static Pair Instance
        {
            get { return _instance; }
        }   

        /// <summary>
        /// Turns to arguments into a key-value pair
        /// </summary>
        /// <param name="args">The arguments to pass to this command. Array must contain 2 elements.</param>
        /// <param name="sctx">Unused.</param>
        /// <remarks>
        /// Equivalent to:
        /// <code>function pair(key, value) = key: value;</code>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                args = new PValue[] {};

            if (args.Length < 2)
                return PType.Null.CreatePValue();
            else
                return PType.Object.CreatePValue(
                    new PValueKeyValuePair(
                        args[0] ?? PType.Null.CreatePValue(),
                        args[1] ?? PType.Null.CreatePValue()
                        ));
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return true; }
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferCustomImplementation;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            int argc = ins.Arguments;

            if(argc < 2)
            {
                state.EmitLoadPValueNull();
            }
            else
            {
                //pop excessive arguments
                for(int i = 2; i < argc; i++)
                    state.Il.Emit(OpCodes.Pop);

                //make pvkvp
                state.Il.Emit(OpCodes.Newobj,Compiler.Cil.Compiler.NewPValueKeyValuePair);

                //save pvkvp in temporary variable
                state.EmitStoreTemp(0);

                //PType.Object.CreatePValue(temp)
                state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.GetObjectPTypeSelector, null);
                state.EmitLoadTemp(0);
                state.Il.EmitCall(OpCodes.Call, Compiler.Cil.Compiler.CreatePValueAsObject, null);
            }
        }

        #endregion
    }
}