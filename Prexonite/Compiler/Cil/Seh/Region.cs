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

namespace Prexonite.Compiler.Cil.Seh
{
    internal static class RegionExtensions
    {
        public static bool IsTryRegion(this Region region)
        {
            if (region == null)
                return false;
            else
                return region.Kind == RegionKind.Try;
        }

        public static bool IsCatchRegion(this Region region)
        {
            if (region == null)
                return false;
            else
                return region.Kind == RegionKind.Catch;
        }

        public static bool IsFinallyRegion(this Region region)
        {
            if (region == null)
                return false;
            else
                return region.Kind == RegionKind.Finally;
        }
    }

    [DebuggerDisplay("{Kind}-block from {Begin} to {End}")]
    internal sealed class Region : IEquatable<Region>
    {
        public const RegionKind AnyRegionKind =
            RegionKind.Try | RegionKind.Catch | RegionKind.Finally;

        public readonly CompiledTryCatchFinallyBlock Block;
        public readonly RegionKind Kind;

        [DebuggerStepThrough]
        public Region(CompiledTryCatchFinallyBlock block, RegionKind kind)
        {
            Block = block;
            Kind = kind;
        }

        public int Begin
        {
            [DebuggerStepThrough]
            get
            {
                switch (Kind)
                {
                    case RegionKind.Try:
                        return Block.BeginTry;
                    case RegionKind.Catch:
                        return Block.BeginCatch;
                    case RegionKind.Finally:
                        return Block.BeginFinally;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public int End
        {
            [DebuggerStepThrough]
            get
            {
                switch (Kind)
                {
                    case RegionKind.Try:
                        return Block.SkipTry - 1;
                    case RegionKind.Catch:
                        return Block.EndTry - 1;
                    case RegionKind.Finally:
                        if (Block.HasCatch)
                            return Block.BeginCatch - 1;
                        else
                            return Block.EndTry - 1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [DebuggerStepThrough]
        public bool Contains(int address)
        {
            switch (Kind)
            {
                case RegionKind.Try:
                    return Block.IsInTryBlock(address);
                case RegionKind.Catch:
                    return Block.IsInCatchBlock(address);
                case RegionKind.Finally:
                    return Block.IsInFinallyBlock(address);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [DebuggerStepThrough]
        public static IEnumerable<Region> FromBlock(CompiledTryCatchFinallyBlock block)
        {
            yield return new Region(block, RegionKind.Try);
            if (block.HasFinally)
                yield return new Region(block, RegionKind.Finally);
            if (block.HasCatch)
                yield return new Region(block, RegionKind.Catch);
        }

        public static int CompareRegions(Region r1, Region r2)
        {
            if (ReferenceEquals(r1, r2))
                return 0;

            if (r1.StrictlyContains(r2)) //r2 is inside r1 <=> r2 is "smaller"
                return 1;
            else if (r2.StrictlyContains(r1)) //r1 is inside r2 <=> r1 is "smaller"
                return -1;

            //here, r1 and r2 must cover the same region
            //   this can only be the case for try-blocks
            Debug.Assert(r1.Kind == RegionKind.Try && r2.Kind == RegionKind.Try,
                "Exactly overlapping finally/catch regions are illegal.");

            //in this case, the total span of the block is the indicator
            var cmp = r1.Block.Range.CompareTo(r2.Block.Range);
            Debug.Assert(cmp != 0, "Two distinct regions with identical ranges are illegal.");
            return cmp;
        }

        public bool StrictlyContains(Region r)
        {
            return Begin < r.Begin || End > r.End;
        }

        public bool IsIn(Region r)
        {
            if (!(Begin <= r.Begin && r.End <= End))
                return false;

            return CompareRegions(this, r) < 0;
        }

        public bool Equals(Region other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Block, Block) && Equals(other.Kind, Kind);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Region)) return false;
            return Equals((Region) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Block.GetHashCode()*397) ^ Kind.GetHashCode();
            }
        }
    }
}