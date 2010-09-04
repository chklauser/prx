using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{

    /// <summary>
    /// <para>Base implementation of partial applications. Don't use for classification. Partial applications just <see cref="IIndirectCall"/> implementations.</para>
    /// <para>It is not mandatory for partial applications to inherit from <see cref="PartialApplicationBase"/>.</para>
    /// </summary>
    [CLSCompliant(false)]
    public abstract class PartialApplicationBase : IIndirectCall
    {
        /// <summary>
        /// <para>Mappings from effective argument position to closed and open arguments. </para>
        /// <para>Negative values indicate open arguments, positive values indicate closed arguments.</para>
        /// </summary>
        private readonly sbyte[] _mappings;

        private readonly PValue[] _nonArgumentsPrototype;
        private readonly PValue[] _effectiveArgumentsPrototype;

        /// <summary>
        /// <para>The number of open arguments mapped.</para>
        /// <para>Used to determine ideal size of effective argument array</para>
        /// </summary>
        /// <remarks>
        /// <para>It is assumed, that each of the closed arguments is mapped exactly once.</para>
        /// </remarks>
        private readonly int _countOpenArgumentsMapped;

        /// <summary>
        /// <para>Copy of the mappings from effective argument position to closed and open arguments. </para>
        /// <para>Negative values indicate open arguments, positive values indicate closed arguments.</para>
        /// </summary>
        public IEnumerable<sbyte> Mappings
        {
            [DebuggerStepThrough]
            get { return _mappings; }
        }

        /// <summary>
        /// Initializes the partial application.
        /// </summary>
        /// <param name="mappings">Mappings from effective argument position to closed and open arguments. See <see cref="Mappings"/>.</param>
        /// <param name="closedArguments">Already provided (closed) arguments.</param>
        /// <param name="nonArgumentPrefix">Indicates how many of the effective arguments should be isolated and not packed into the arguments array. See remarks on <see cref="NonArgumentPrefix"/>.</param>
        protected PartialApplicationBase(sbyte[] mappings, PValue[] closedArguments, int nonArgumentPrefix)
        {
            if (nonArgumentPrefix < 0)
                throw new ArgumentOutOfRangeException(
                    "nonArgumentPrefix", "non-argument prefix cannot be negative");

            _mappings = mappings;
            _countOpenArgumentsMapped = _computeCountOpenArgumentsMapped();

            _assertMappingsNonZero();

            //Prepare prototype arrays
            _nonArgumentsPrototype = new PValue[nonArgumentPrefix];
            _effectiveArgumentsPrototype = new PValue[System.Math.Max(mappings.Length-nonArgumentPrefix,0)];

            for (var absoluteIndex = 0; absoluteIndex < _mappings.Length; absoluteIndex++)
            {
                var mapping = _mappings[absoluteIndex];
                System.Diagnostics.Debug.Assert(mapping != 0, "Mapping contains zero");

                //Skip open arguments
                if (mapping <= 0) 
                    continue;

                int relativeIndex;
                var argumentList = _determineArgumentList(
                    out relativeIndex, nonArgumentPrefix, absoluteIndex, _nonArgumentsPrototype,
                    _effectiveArgumentsPrototype);
                var index = mapping - 1;
                System.Diagnostics.Debug.Assert(index < closedArguments.Length);
                //maps closed argument
                argumentList[relativeIndex] = closedArguments[index];
            }
        }

        private int _computeCountOpenArgumentsMapped()
        {
            //TODO: Array-based implementation
            return _mappings.Where(m => m < 0).Distinct().Count();
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
            var argc = args.Length;
            var countExcessArguments = System.Math.Max(argc - _countOpenArgumentsMapped, 0);
            var countMappings = _mappings.Length;
            var nonArgumentPrefix = NonArgumentPrefix;
            var countEffectiveArguments = System.Math.Max(countMappings + countExcessArguments - nonArgumentPrefix,0);

            var nonArguments = new PValue[nonArgumentPrefix];
            var effectiveArguments = new PValue[countEffectiveArguments];
            System.Diagnostics.Debug.Assert(effectiveArguments.Length + nonArgumentPrefix >= _mappings.Length);

            //Overlay closed arguments
            Array.Copy(_nonArgumentsPrototype, nonArguments, _nonArgumentsPrototype.Length);
            Array.Copy(_effectiveArgumentsPrototype, effectiveArguments, _effectiveArgumentsPrototype.Length);

            //Apply mapping
            var openArgumentUsed = new bool[argc];
            var absoluteIndex = 0;
            for (; absoluteIndex < _mappings.Length; absoluteIndex++ )
            {
                var mapping = _mappings[absoluteIndex];
                System.Diagnostics.Debug.Assert(mapping != 0, "Mapping contains zero");

                //skip closed arguments
                if (0 <= mapping) 
                    continue;

                int relativeIndex;
                var argumentList = _determineArgumentList(
                    out relativeIndex, nonArgumentPrefix, absoluteIndex, nonArguments, effectiveArguments);

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

            //Add excess arguments
            for (var i = 0; i < openArgumentUsed.Length; i++)
            {
                if (openArgumentUsed[i])
                    continue;

                int relativeIndex;
                var argumentList = _determineArgumentList(out relativeIndex, nonArgumentPrefix, absoluteIndex++, nonArguments, effectiveArguments);
                argumentList[relativeIndex] = args[i];
            }

            System.Diagnostics.Debug.Assert(nonArguments.All(x => !ReferenceEquals(x, null)), "non-argument left unassigned");
            System.Diagnostics.Debug.Assert(effectiveArguments.All(x => !ReferenceEquals(x,null)), "effective argument left unassigned");

            return Invoke(sctx, nonArguments, effectiveArguments);
        }

        private static PValue[] _determineArgumentList(out int relativeIndex, int nonArgumentPrefix, int absoluteIndex, PValue[] nonArguments, PValue[] effectiveArguments)
        {
            PValue[] argumentList;
            if(absoluteIndex < nonArgumentPrefix)
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
            System.Diagnostics.Debug.Assert(0 <= relativeIndex && relativeIndex < argumentList.Length, "Relative index is out of bounds");
            return argumentList;
        }

        /// <summary>
        /// Performs the actual invocation of the partial application.
        /// </summary>
        /// <param name="sctx">The stack context in which the invocation takes place.</param>
        /// <param name="nonArguments">A prefix of the effective arguments. Length is determined by <see cref="NonArgumentPrefix"/>.</param>
        /// <param name="arguments">The rest of the effective arguments, ready to be passed as arguments.</param>
        /// <returns>The value to be returned as the partial application's result</returns>
        protected abstract PValue Invoke(StackContext sctx, PValue[] nonArguments, PValue[] arguments);

        /// <summary>
        /// Indicates how many of the effective arguments should be isolated and not packed into the arguments array.
        /// </summary>
        /// <remarks>
        /// <para>In a lot of cases, not all of the effective arguments will actually get passed as a <see cref="PValue"/>[]. Most likely the first few will indicate the call target (the object to call members on, the indirect call subject and so on).</para>
        /// </remarks>
        protected int NonArgumentPrefix
        {
            get { return _nonArgumentsPrototype.Length; }
        }

        #endregion
    }
}
