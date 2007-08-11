using System;
using System.IO;
using System.Reflection;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of the LoadAssembly command which dynamically loads an assembly from a file.
    /// </summary>
    public class LoadAssembly : PCommand
    {
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
                eng.RegisterAssembly(Assembly.LoadFile(Path.GetFullPath(path)));
            }

            return PType.Null.CreatePValue();
        }
    }
}