// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialCallStar : PartialApplicationBase
    {
        private readonly ArraySegment<int> _wrappingDirectives;

        /// <summary>
        ///     The number of arguments that remain, when all wrapping directives have been applied.
        /// </summary>
        private readonly int _directedArgc;

        /// <summary>
        ///     The number of arguments that have wrapping directions.
        /// </summary>
        private readonly int _undirectedArgc;

        public PartialCallStar(int[] mappings, PValue[] closedArguments)
            : this(new ArraySegment<int>(mappings), closedArguments)
        {
        }

        public PartialCallStar(ArraySegment<int> mappings, PValue[] closedArguments)
            : base(_splitOffWrappingDirectives(ref mappings), closedArguments, 1)
        {
            //Mappings now holds only directives (was split off by _splitOffWrappingDirectives)
            _wrappingDirectives = mappings;
            _getDirectedArgc(out _directedArgc, out _undirectedArgc);
        }

        /// <summary>
        ///     Splits the raw mapping embedded in the code up into the argument mapping (returned) and the list wrapping directives (assigned to ref <paramref
        ///      name = "rawMapping" />).
        /// </summary>
        /// <param name = "rawMapping">[In] The combined mapping (unpacked); [Out] The list wrapping directives</param>
        /// <returns>The actual argument mapping. <see cref = "PartialApplicationBase.Mappings" />.</returns>
        private static ArraySegment<int> _splitOffWrappingDirectives(
            ref ArraySegment<int> rawMapping)
        {
            var dirCount = rawMapping.Array[rawMapping.Offset + rawMapping.Count - 1];
            var actualMapping = new ArraySegment<int>(rawMapping.Array, rawMapping.Offset,
                rawMapping.Count - dirCount - 1);
            rawMapping = new ArraySegment<int>(rawMapping.Array, actualMapping.Count, dirCount);
            return actualMapping;
        }

        #region Overrides of PartialApplicationBase

        protected override PValue Invoke(StackContext sctx, PValue[] nonArguments,
            PValue[] arguments)
        {
            var end = _wrappingDirectives.Offset + _wrappingDirectives.Count;
            var effectiveArguments = new PValue[_getEffectiveArgc(arguments.Length)];
            var effIdx = 0;
            var argIdx = 0;
            for (var i = _wrappingDirectives.Offset; i < end; i++)
            {
                var directive = _wrappingDirectives.Array[i];

                System.Diagnostics.Debug.Assert(directive != 0);

                if (directive > 0)
                {
                    Array.Copy(arguments, argIdx, effectiveArguments, effIdx, directive);
                    argIdx += directive;
                    effIdx += directive;
                }
                else
                {
                    directive = -directive;

                    var list = new List<PValue>(directive);
                    for (var j = 0; j < directive; j++)
                        list.Add(arguments[argIdx++]);

                    effectiveArguments[effIdx++] = sctx.CreateNativePValue(list);
                }
            }

            System.Diagnostics.Debug.Assert(effectiveArguments.Length - effIdx ==
                arguments.Length - argIdx);

            Array.Copy(arguments, argIdx, effectiveArguments, effIdx,
                effectiveArguments.Length - effIdx);

            return nonArguments[0].IndirectCall(sctx, effectiveArguments);
        }

        private int _getEffectiveArgc(int actualArgc)
        {
            System.Diagnostics.Debug.Assert(actualArgc >= _directedArgc);
            return _directedArgc + (actualArgc - _undirectedArgc);
        }

        private void _getDirectedArgc(out int directedArgc, out int undirectedArgc)
        {
            directedArgc = 0;
            undirectedArgc = 0;

            var end = _wrappingDirectives.Offset + _wrappingDirectives.Count;
            for (var i = _wrappingDirectives.Offset; i < end; i++)
            {
                var directive = _wrappingDirectives.Array[i];
                System.Diagnostics.Debug.Assert(directive != 0);

                undirectedArgc += System.Math.Abs(directive);

                if (directive > 0)
                    directedArgc += directive;
                else
                    directedArgc++;
            }
        }

        #endregion
    }
}