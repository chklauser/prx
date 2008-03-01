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
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    /// <summary>
    /// Implementation of the LoadAssembly command which dynamically loads an assembly from a file.
    /// </summary>
    public sealed class LoadAssembly : PCommand
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
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            Engine eng = sctx.ParentEngine;
            foreach (PValue arg in args)
            {
                string path = arg.CallToString(sctx);
                LoaderOptions ldrOptions = new LoaderOptions(sctx.ParentEngine, sctx.ParentApplication);
                ldrOptions.ReconstructSymbols = false;
                Loader ldr = sctx as Loader ?? new Loader(ldrOptions);
                eng.RegisterAssembly(Assembly.LoadFile(ldr.ApplyLoadPaths(path).FullName));
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
    }
}