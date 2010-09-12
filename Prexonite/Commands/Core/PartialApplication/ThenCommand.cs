using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class ThenCommand : PCommand, ICilCompilerAware
    {

        #region Singleton pattern

        private ThenCommand()
        {
        }

        private static readonly ThenCommand _instance = new ThenCommand();

        public static ThenCommand Instance
        {
            get { return _instance; }
        }

        #endregion 

        #region Overrides of PCommand

        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args.Length < 2)
                throw new PrexoniteException("then command requires two arguments.");

            return sctx.CreateNativePValue(new CallComposition(args[0], args[1]));
        }

        #endregion

        #region Implementation of ICilCompilerAware

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class CallComposition : IIndirectCall
    {
        private readonly PValue _innerExpression;
        private readonly PValue _outerExpression;

        public PValue InnerExpression
        {
            [DebuggerStepThrough]
            get { return _innerExpression; }
        }

        public PValue OuterExpression
        {
            [DebuggerStepThrough]
            get { return _outerExpression; }
        }

        public CallComposition(PValue innerExpression, PValue outerExpression)
        {
            if (innerExpression == null)
                throw new ArgumentNullException("innerExpression");
            if (outerExpression == null)
                throw new ArgumentNullException("outerExpression");
            _innerExpression = innerExpression;
            _outerExpression = outerExpression;
        }

        #region Implementation of IIndirectCall

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _outerExpression.IndirectCall(sctx, new[] {_innerExpression.IndirectCall(sctx, args)});
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} then ({1})", InnerExpression, OuterExpression);
        }
    }
}
