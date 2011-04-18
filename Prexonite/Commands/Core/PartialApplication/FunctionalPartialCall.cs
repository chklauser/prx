using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class FunctionalPartialCall : IMaybeStackAware
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
            return _subject.IndirectCall(sctx, _getEffectiveArgs(args));
        }

        public bool TryDefer(StackContext sctx, PValue[] args, out StackContext partialApplicationContext, out PValue result)
        {
            var effectiveArgs = _getEffectiveArgs(args);

            partialApplicationContext = null;
            result = null;

            //The following code exists in a very similar form in PartialCall.cs, FlippedFunctionalPartialCall.cs
            if ((_subject.Type is Types.ObjectPType))
            {
                var raw = _subject.Value;
                var stackAware = raw as IStackAware;
                if (stackAware != null)
                {
                    partialApplicationContext = stackAware.CreateStackContext(sctx, effectiveArgs);
                    return true;
                }

                var partialApplication = raw as IMaybeStackAware;
                if (partialApplication != null)
                    return partialApplication.TryDefer(sctx, effectiveArgs,
                                                       out partialApplicationContext,
                                                       out result);
            }

            result = _subject.IndirectCall(sctx, effectiveArgs);
            return false;
        }

        private PValue[] _getEffectiveArgs(PValue[] args)
        {
            var effectiveArgs = new PValue[args.Length + _closedArguments.Length];
            Array.Copy(_closedArguments, effectiveArgs, _closedArguments.Length);
            Array.Copy(args, 0, effectiveArgs, _closedArguments.Length, args.Length);
            return effectiveArgs;
        }
    }
}
