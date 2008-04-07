using System;
using System.Collections.Generic;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List
{
    public class Except : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private static readonly Except _instance = new Except();

        public static Except Instance
        {
            get { return _instance; }
        }

        private Except()
        {
        }

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            List<IEnumerable<PValue>> xss = new List<IEnumerable<PValue>>();
            foreach (PValue arg in args)
            {
                IEnumerable<PValue> xs = Map._ToEnumerable(sctx, arg);
                if (xs != null)
                    xss.Add(xs);
            }

            int n = xss.Count;
            if (n < 2)
                throw new PrexoniteException("Except requires at least two sources.");

            Dictionary<PValue, bool> t = new Dictionary<PValue, bool>();
            //All elements of the first source are considered candidates
            foreach (PValue x in xss[0])
                if (!t.ContainsKey(x))
                    t.Add(x, true);

            for (int i = 1; i < n; i++)
                foreach (PValue x in xss[i])
                    if (t.ContainsKey(x))
                        t.Remove(x);

            return sctx.CreateNativePValue(t.Keys);
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