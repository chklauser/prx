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
using System.IO;
using System.Reflection;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of the LoadAssembly command which dynamically loads an assembly from a file.
    /// </summary>
    public sealed class LoadAssembly : PCommand, ICilCompilerAware
    {
        private LoadAssembly()
        {
        }

        private static readonly LoadAssembly _instance = new LoadAssembly();

        public static LoadAssembly Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Implementation of the LoadAssembly command which dynamically loads an assembly from a file.
        /// </summary>
        /// <param name="sctx">The stack context in which to load the assembly</param>
        /// <param name="args">A list of file paths to assemblies.</param>
        /// <returns>Null</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] { };

            var eng = sctx.ParentEngine;
            foreach (var arg in args)
            {
                var path = arg.CallToString(sctx);
                var ldrOptions = new LoaderOptions(sctx.ParentEngine, sctx.ParentApplication);
                ldrOptions.ReconstructSymbols = false;
                var ldr = sctx as Loader ?? new Loader(ldrOptions);
                var asmFile = ldr.ApplyLoadPaths(path);
                if (asmFile == null)
                    throw new FileNotFoundException("Prexonite can't load assembly located in " + path);

                eng.RegisterAssembly(Assembly.LoadFile(asmFile.FullName));
            }

            return PType.Null.CreatePValue();
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        #region Implementation of ICilCompilerAware

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}