// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    ///     <para>Base implementation of partial applications. Don't use for classification. Partial applications just <see
    ///      cref = "IIndirectCall" /> implementations.</para>
    ///     <para>It is not mandatory for partial applications to inherit from <see cref = "PartialApplicationBase" />.</para>
    /// </summary>
    public abstract class PartialApplicationBase : IMaybeStackAware
    {
        /// <summary>
        ///     <para>Mappings from effective argument position to closed and open arguments. </para>
        ///     <para>Negative values indicate open arguments, positive values indicate closed arguments.</para>
        /// </summary>
        private readonly ArraySegment<int> _mappings;

        private readonly PValue[] _closedArguments;

        private readonly int _nonArgumentPrefix;

        /// <summary>
        ///     <para>Copy of the mappings from effective argument position to closed and open arguments. </para>
        ///     <para>Negative values indicate open arguments, positive values indicate closed arguments.</para>
        ///     <para>These values are not packed or compressed. Each array element represents one mapping.</para>
        /// </summary>
        public IEnumerable<int> Mappings
        {
            [DebuggerStepThrough]
            get
            {
                return
                    Enumerable.Range(_mappings.Offset, _mappings.Count).Select(
                        i => _mappings.Array[i]);
            }
        }

        /// <summary>
        ///     Initializes the partial application.
        /// </summary>
        /// <param name = "mappings">Mappings from effective argument position to closed and open arguments. See <see
        ///      cref = "Mappings" />.</param>
        /// <param name = "closedArguments">Already provided (closed) arguments.</param>
        /// <param name = "nonArgumentPrefix">Indicates how many of the effective arguments should be isolated and not packed into the arguments array. See remarks on <see
        ///      cref = "NonArgumentPrefix" />.</param>
        protected PartialApplicationBase(int[] mappings, PValue[] closedArguments,
            int nonArgumentPrefix)
            : this(new ArraySegment<int>(mappings), closedArguments, nonArgumentPrefix)
        {
        }

        /// <summary>
        ///     Initializes the partial application.
        /// </summary>
        /// <param name = "mappings">Mappings from effective argument position to closed and open arguments. See <see
        ///      cref = "Mappings" />.</param>
        /// <param name = "closedArguments">Already provided (closed) arguments.</param>
        /// <param name = "nonArgumentPrefix">Indicates how many of the effective arguments should be isolated and not packed into the arguments array. See remarks on <see
        ///      cref = "NonArgumentPrefix" />.</param>
        protected PartialApplicationBase(ArraySegment<int> mappings, PValue[] closedArguments,
            int nonArgumentPrefix)
        {
            if (nonArgumentPrefix < 0)
                throw new ArgumentOutOfRangeException(
                    "nonArgumentPrefix", "non-argument prefix cannot be negative");

            _closedArguments = closedArguments;

            _mappings = mappings;
            _nonArgumentPrefix = nonArgumentPrefix;
            _assertMappingsNonZero();
        }

        /// <summary>
        ///     <para>The number of open arguments mapped.</para>
        ///     <para>Used to determine ideal size of effective argument array</para>
        /// </summary>
        /// <remarks>
        ///     <para>It is assumed, that each of the closed arguments is mapped exactly once.</para>
        /// </remarks>
        private int _computeCountOpenArgumentsMapped(int argc)
        {
            var mapped = new bool[argc];
            var count = 0;
            var end = _mappings.Offset + _mappings.Count;
            for (var i = _mappings.Offset; i < end; i++)
            {
                var m = _mappings.Array[i];
                int idx;
                if (0 <= m || (idx = -m - 1) >= mapped.Length)
                    continue;

                if (mapped[idx])
                    continue;

                count++;
                mapped[idx] = true;
            }

            return count;
        }

        #region Implementation of IIndirectCall

        [Conditional("DEBUG")]
        private void _assertMappingsNonZero()
        {
            foreach (var mapping in Mappings)
                System.Diagnostics.Debug.Assert(mapping != 0);
        }

        public virtual PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            PValue[] nonArguments;
            PValue[] effectiveArguments;
            _combineArguments(args, out nonArguments, out effectiveArguments);

            return Invoke(sctx, nonArguments, effectiveArguments);
        }

        private void _combineArguments(PValue[] args, out PValue[] nonArguments,
            out PValue[] effectiveArguments)
        {
            System.Diagnostics.Debug.Assert(args.All(value => value != null),
                "Actual (CLI) null references passed to " +
                    GetType().Name + ".IndirectCall");

            var argc = args.Length;
            var countExcessArguments = System.Math.Max(
                argc - _computeCountOpenArgumentsMapped(argc), 0);
            var nonArgumentPrefix = NonArgumentPrefix;
            var mappingLength = _mappings.Count;
            var countEffectiveArguments =
                System.Math.Max(mappingLength + countExcessArguments - nonArgumentPrefix, 0);

            nonArguments = new PValue[nonArgumentPrefix];
            effectiveArguments = new PValue[countEffectiveArguments];
            System.Diagnostics.Debug.Assert(effectiveArguments.Length + nonArgumentPrefix >=
                mappingLength);

            //Apply mapping
            var openArgumentUsed = new bool[argc];
            var absoluteIndex = 0;
            for (; absoluteIndex < mappingLength; absoluteIndex++)
            {
                var mapping = _mappings.Array[_mappings.Offset + absoluteIndex];
                System.Diagnostics.Debug.Assert(mapping != 0, "Mapping contains zero");

                int relativeIndex;
                var argumentList = _determineArgumentList(
                    out relativeIndex, nonArgumentPrefix, absoluteIndex, nonArguments,
                    effectiveArguments);

                if (0 <= mapping)
                {
                    var index = mapping - 1;
                    //maps closed arguments   
                    argumentList[relativeIndex] = _closedArguments[index];
                }
                else
                {
                    var index = (-mapping) - 1;
                    //maps open argument
                    if (index < argc)
                    {
                        argumentList[relativeIndex] = args[index];
                        openArgumentUsed[index] = true;
                    }
                    else
                    {
                        argumentList[relativeIndex] = PType.Null;
                    }
                }
            }

            //Add excess arguments
            for (var i = 0; i < openArgumentUsed.Length; i++)
            {
                if (openArgumentUsed[i])
                    continue;

                int relativeIndex;
                var argumentList = _determineArgumentList(out relativeIndex, nonArgumentPrefix,
                    absoluteIndex++, nonArguments,
                    effectiveArguments);
                argumentList[relativeIndex] = args[i];
            }

            System.Diagnostics.Debug.Assert(nonArguments.All(x => !ReferenceEquals(x, null)),
                "non-argument left unassigned");
            System.Diagnostics.Debug.Assert(effectiveArguments.All(x => !ReferenceEquals(x, null)),
                "effective argument left unassigned");
        }

        public bool TryDefer(StackContext sctx, PValue[] args,
            out StackContext partialApplicationContext, out PValue result)
        {
            PValue[] nonArguments;
            PValue[] effectiveArguments;
            _combineArguments(args, out nonArguments, out effectiveArguments);

            return DoTryDefer(sctx, nonArguments, effectiveArguments, out partialApplicationContext,
                out result);
        }

        /// <summary>
        ///     If this partial application can create <see cref = "StackContext" />s, then this method creates that <see
        ///      cref = "StackContext" />.
        ///     Otherwise it just must call <see cref = "Invoke" />.
        /// </summary>
        /// <param name = "sctx">The caller's stack context</param>
        /// <param name = "nonArguments">The non-arguments as determined by the kind of partial application.</param>
        /// <param name = "arguments">The arguments supplied by comining closed- and open arguments.</param>
        /// <param name = "partialApplicationContext">The stack context for this partial application</param>
        /// <param name = "result"></param>
        /// <returns>True if the <see cref = "StackContext" /> has been created; false otherwise.</returns>
        protected virtual bool DoTryDefer(StackContext sctx, PValue[] nonArguments,
            PValue[] arguments, out StackContext partialApplicationContext, out PValue result)
        {
            partialApplicationContext = null;
            result = Invoke(sctx, nonArguments, arguments);
            return false;
        }

        private static PValue[] _determineArgumentList(out int relativeIndex, int nonArgumentPrefix,
            int absoluteIndex, PValue[] nonArguments, PValue[] effectiveArguments)
        {
            PValue[] argumentList;
            if (absoluteIndex < nonArgumentPrefix)
            {
                argumentList = nonArguments;
                relativeIndex = absoluteIndex;
            }
            else
            {
                argumentList = effectiveArguments;
                relativeIndex = absoluteIndex - nonArgumentPrefix;
            }

            System.Diagnostics.Debug.Assert(argumentList != null, "ArgumentList cannot be null");
            System.Diagnostics.Debug.Assert(
                0 <= relativeIndex && relativeIndex < argumentList.Length,
                "Relative index is out of bounds");
            return argumentList;
        }

        /// <summary>
        ///     Performs the actual invocation of the partial application.
        /// </summary>
        /// <param name = "sctx">The stack context in which the invocation takes place.</param>
        /// <param name = "nonArguments">A prefix of the effective arguments. Length is determined by <see
        ///      cref = "NonArgumentPrefix" />.</param>
        /// <param name = "arguments">The rest of the effective arguments, ready to be passed as arguments.</param>
        /// <returns>The value to be returned as the partial application's result</returns>
        protected abstract PValue Invoke(StackContext sctx, PValue[] nonArguments,
            PValue[] arguments);

        /// <summary>
        ///     Indicates how many of the effective arguments should be isolated and not packed into the arguments array.
        /// </summary>
        /// <remarks>
        ///     <para>In a lot of cases, not all of the effective arguments will actually get passed as a <see cref = "PValue" />[]. Most likely the first few will indicate the call target (the object to call members on, the indirect call subject and so on).</para>
        /// </remarks>
        protected int NonArgumentPrefix
        {
            get { return _nonArgumentPrefix; }
        }

        #endregion
    }
}