using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class FunctionalPartialCall : IIndirectCall
    {
        private readonly PValue _subject;
        private readonly PValue[] _closedArguments;

        public FunctionalPartialCall(PValue subject, PValue[] closedArguments)
        {
            _subject = subject;
            _closedArguments = closedArguments;
        }

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            var effectiveArgs = new PValue[args.Length + _closedArguments.Length];
            Array.Copy(_closedArguments, effectiveArgs, _closedArguments.Length);
            Array.Copy(args, 0, effectiveArgs, _closedArguments.Length, args.Length);
            return _subject.IndirectCall(sctx, effectiveArgs);
        }
    }
}
