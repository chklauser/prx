using System;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    /// <para>A more efficient implementation of the partial application pattern <code>obj.(?,c_1,c_2,c_3,...,c_n)</code>.</para>
    /// <para>For a more general implementation of partial application of indirect calls, see <see cref="PartialCall"/>.</para>
    /// </summary>
    public class FlippedFunctionalPartialCall : IIndirectCall
    {
        private readonly PValue _subject;
        private readonly PValue[] _closedArguments;

        /// <summary>
        /// Creates a new flipped, functional partial call, implementing a partial call to <code><paramref name="subject"/>.(?,<paramref name="closedArguments"/>)</code>.
        /// </summary>
        /// <param name="subject">The subject of the indirect call.</param>
        /// <param name="closedArguments">The closed arguments. Will be inserted starting at parameter index 1.</param>
        public FlippedFunctionalPartialCall(PValue subject, PValue[] closedArguments)
        {
            _subject = subject;
            _closedArguments = closedArguments;
        }

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            var effectiveArgs = new PValue[System.Math.Max(args.Length,1) + _closedArguments.Length];
            if (args.Length > 0 && args[0] != null)
                effectiveArgs[0] = args[0];
            else
                effectiveArgs[0] = PType.Null;
            Array.Copy(_closedArguments, 0, effectiveArgs,1, _closedArguments.Length);
            Array.Copy(args, System.Math.Min(1,args.Length), effectiveArgs, _closedArguments.Length+1, System.Math.Max(args.Length-1,0));
            return _subject.IndirectCall(sctx, effectiveArgs);
        }
    }
}