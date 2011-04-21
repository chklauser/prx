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

            var iargs = Call.FlattenArguments(sctx, args, 1).ToArray();

            return RunStatically(sctx, fpv, iargs);
        }

        public static PValue RunStatically(StackContext sctx, PValue fpv, PValue[] iargs)
        {
            return RunStatically(sctx, fpv, iargs, false);
        }

        public static PValue RunStatically(StackContext sctx, PValue fpv, PValue[] iargs, bool useIndirectCallAsFallback)
        {
            IStackAware f;
            CilClosure cilClosure;
            PFunction func = null;
            PVariable[] sharedVars = null;

            PValue result;
            ReturnMode returnMode;

            if ((cilClosure = fpv.Value as CilClosure) != null)
            {
                func = cilClosure.Function;
                sharedVars = cilClosure.SharedVariables;
            }

            if((func = func ?? fpv.Value as PFunction) != null && func.HasCilImplementation)
            {
                func.CilImplementation.Invoke(
                    func, CilFunctionContext.New(sctx, func), iargs, sharedVars ?? new PVariable[0], out result, out returnMode);    
            }
            else if((f = fpv.Value as IStackAware) != null)
            {
                //Create stack context, let the engine execute it
                var subCtx = f.CreateStackContext(sctx, iargs);
                sctx.ParentEngine.Process(subCtx);
                result = subCtx.ReturnValue;
                returnMode = subCtx.ReturnMode;
            }
            else if(useIndirectCallAsFallback)
            {
                result = fpv.IndirectCall(sctx, iargs);
                returnMode = ReturnMode.Exit;
            }
            else
            {
                throw new PrexoniteException("call\\sub\\perform requires its argument to be stack aware.");
            }

            return new PValueKeyValuePair(sctx.CreateNativePValue(returnMode), result);
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
            return CompilationFlags.PrefersRunStatically;
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
