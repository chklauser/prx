using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List
{
    /// <summary>
    /// Implementation of the all command.
    /// </summary>
    public class All : PCommand, ICilCompilerAware
    {
        #region Singleton

        private All()
        {
        }

        private static readonly All _instance = new All();

        public static All Instance
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
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args"); 
            List<PValue> lst = new List<PValue>();
            foreach (PValue arg in args)
            {
                IEnumerable<PValue> set = Map._ToEnumerable(sctx, arg);
                if(set == null)
                    continue;
                else
                    lst.AddRange(set);
            }

            return (PValue) lst;
        }

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
            return CompilationFlags.PreferRunStatically;
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
    }
}
