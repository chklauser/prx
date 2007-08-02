using System;
using System.Collections.Generic;
using System.Text;
using Prexonite;
using Prexonite.Types;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler
{
    /// <summary>
    /// A method that modifies the supplied 
    /// <see cref="CompilerTarget"/> when invoked priot to optimization and code generation.
    /// </summary>
    /// <param name="target">The <see cref="CompilerTarget"/> of the function to be modified.</param>
    public delegate void AstTransformation(CompilerTarget target);

    /// <summary>
    /// Union class for both managed as well as interpreted compiler hooks.
    /// </summary>
    [System.Diagnostics.DebuggerNonUserCode]
    public class CompilerHook
    {
        private AstTransformation _managed;
        private PValue _interpreted;

        /// <summary>
        /// Creates a new compiler hook, that executes a managed method.
        /// </summary>
        /// <param name="transformation">A managed transformation.</param>
        public CompilerHook(AstTransformation transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation"); 
            _managed = transformation;
        }

        /// <summary>
        /// Creates a new compiler hook, that indirectly calls a <see cref="PValue"/>.
        /// </summary>
        /// <param name="transformation">A value that supports indirect calls (such as a function reference).</param>
        public CompilerHook(PValue transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation"); 
            _interpreted = transformation;
        }

        /// <summary>
        /// Indicates whether the compiler hook is managed.
        /// </summary>
        public bool IsManaged
        {
            get
            {
                return _managed != null;
            }
        }

        /// <summary>
        /// Indicates whether the compiler hook is interpreted.
        /// </summary>
        public bool IsInterpreted
        {
            get
            {
                return _interpreted != null;
            }
        }

        /// <summary>
        /// Executes the compiler hook (either calls the managed 
        /// delegate or indirectly calls the <see cref="PValue"/> in the context of the <see cref="Loader"/>.)
        /// </summary>
        /// <param name="target">The compiler target to modify.</param>
        public void Execute(CompilerTarget target)
        {
            try
            {
                target.Loader.Options.TargetApplication._SuppressInitialization = true;
                if (IsManaged)
                    _managed(target);
                else
                    _interpreted.IndirectCall(
                        target.Loader, new PValue[] {target.Loader.CreateNativePValue(target)});
            }
            finally
            {
                target.Loader.Options.TargetApplication._SuppressInitialization = false;
            }
        }
    }
}
