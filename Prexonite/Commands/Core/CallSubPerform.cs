using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class CallSubPerform : PCommand, ICilCompilerAware
    {

        #region singleton pattern

        private static readonly CallSubPerform _instance = new CallSubPerform();

        public static CallSubPerform Instance
        {
            get { return _instance; }
        }

        private CallSubPerform(){}

        #endregion

        #region Overrides of PCommand

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
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args.Length < 1)
                throw new PrexoniteException("call\\sub\\perform needs at least one argument, the function to call.");
            var fpv = args[0];

            args = args.Skip(1).ToArray();

            IStackAware f;
            if((f = fpv.Value as IStackAware) != null)
            {
                //Create stack context, let the engine execute it
                var subCtx = f.CreateStackContext(sctx, args);
                var ret = sctx.ParentEngine.Process(subCtx);
                var retVar = subCtx.ReturnMode;

                //return (returnMode: returnValue)
                return new PValueKeyValuePair(sctx.CreateNativePValue(retVar), ret);
            }
            else
            {
                throw new PrexoniteException("call\\sub\\perform requires its argument to be stack aware.");
            }
        }

        #endregion

        #region Implementation of ICilCompilerAware

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
            throw new NotImplementedException();
        }

        #endregion
    }
}
