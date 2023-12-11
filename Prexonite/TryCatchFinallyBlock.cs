// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Prexonite;

/// <summary>
///     Represents a try-catch-finally block
/// </summary>
//[DebuggerNonUserCode]
public class TryCatchFinallyBlock : IEquatable<TryCatchFinallyBlock>
{
    /// <summary>
    ///     Creates a new try-catch-finally block.
    /// </summary>
    /// <remarks>
    ///     As there is no initialization with this overload, the resulting 
    ///     instance will be invalid (<see cref = "IsValid" />) until you set <see cref = "BeginTry" /> and
    ///     <see cref = "EndTry" /> to appropriate values.
    /// </remarks>
    public TryCatchFinallyBlock()
    {
    }

    /// <summary>
    ///     Creates a new try-catch-finally block.
    /// </summary>
    /// <param name = "beginTry"></param>
    /// <param name = "endTry"></param>
    public TryCatchFinallyBlock(int beginTry, int endTry)
    {
        BeginTry = beginTry;
        EndTry = endTry;
    }

    /// <summary>
    ///     Used to store and retrieve try-catch-finally blocks from metadata.
    /// </summary>
    public const string MetaKey = @"\trycatchfinally";

    /// <summary>
    ///     The address of the first instruction inside the try-block.
    /// </summary>
    public int BeginTry
    {
        get => _beginTry;
        set
        {
            if ((_endTry > 0 ? value >= _endTry : false) ||
                (HasFinally ? value >= _beginFinally : false) ||
                (HasCatch ? value >= _beginCatch : false))
                throw new ArgumentOutOfRangeException
                (
                    nameof(value),
                    "BeginTry(" + value +
                    ") has to be less than BeginFinally,BeginCatch and EndTry.");
            _beginTry = value;
        }
    }

    int _beginTry = -1;

    /// <summary>
    ///     The address of the first instruction inside the finally-block.
    /// </summary>
    /// <remarks>
    ///     This property can have the value -1 if no finally-block has been specified.
    /// </remarks>
    public int BeginFinally
    {
        get => _beginFinally;
        set
        {
            if (value > 0 &&
                ((_beginTry > 0 ? value <= _beginTry : false) ||
                    (_endTry > 0 ? value >= _endTry : false) ||
                    (HasCatch ? value >= _beginCatch : false)))
                throw new ArgumentOutOfRangeException
                (
                    nameof(value),
                    "BeginFinally(" + value +
                    ") has to be within the whole try-catch-finally structure but before a catch-clause.");
            _beginFinally = value;
        }
    }

    int _beginFinally = -1;

    /// <summary>
    ///     The address of the first instruction inside the catch-block.
    /// </summary>
    /// <remarks>
    ///     This property can have the value -1 if no catch-block has been specified.
    /// </remarks>
    public int BeginCatch
    {
        get => _beginCatch;
        set
        {
            if (value > 0 &&
                ((_beginTry > 0 ? value <= _beginTry : false) ||
                    (_endTry > 0 ? value >= _endTry : false) ||
                    (HasFinally ? value <= _beginFinally : false)))
                throw new ArgumentOutOfRangeException
                (
                    nameof(value),
                    "BeginCatch(" + value +
                    ") has to be within whole try-catch-finally structure but after a finally-clause.");
            _beginCatch = value;
        }
    }

    int _beginCatch = -1;

    /// <summary>
    ///     The address of the first instruction after the try-catch-finally construct.
    /// </summary>
    /// <remarks>
    ///     This property might point to an invalid address after the last instruction of a function.
    /// </remarks>
    public int EndTry
    {
        get => _endTry;
        set
        {
            if ((_beginTry > 0 ? value <= _beginTry : false) ||
                (HasCatch ? value <= _beginCatch : false) ||
                (HasFinally ? value <= _beginFinally : false))
                throw new ArgumentOutOfRangeException
                (
                    nameof(value),
                    "EndTry(" + value +
                    ") has to be greater than BeginTry, BeginFinally and BeginCatch.");
            _endTry = value;
        }
    }

    int _endTry = -1;

    public bool UsesException { get; set; }

    /// <summary>
    ///     Indicates whether this instance has correctly been initialized.
    /// </summary>
    /// <remarks>
    ///     Requires <see cref = "BeginTry" /> and <see cref = "EndTry" /> to be set.
    /// </remarks>
    public bool IsValid => _beginTry >= 0 && _endTry >= 0 && _beginTry < _endTry;

    /// <summary>
    ///     Indicates whether the try-catch-finally block has a finally-clause.
    /// </summary>
    public bool HasFinally => _beginFinally > 0;

    /// <summary>
    ///     Indicates whether the try-catch-finally block has a catch-clause.
    /// </summary>
    public bool HasCatch => _beginCatch > 0;

    /// <summary>
    ///     Returns the number of instructions covered by the block.
    /// </summary>
    /// <remarks>
    ///     Range only contains the instructions in the protected block. Catch- and finally-clauses are ignored.
    /// </remarks>
    public int Range
    {
        get
        {
            if (!IsValid)
                return -1;
            else
                return
                    (HasFinally ? _beginFinally : HasCatch ? _beginCatch : _endTry) - _beginTry -
                    1;
        }
    }

    /// <summary>
    ///     Determines whether a block handles exceptions at the supplied address.
    /// </summary>
    /// <param name = "address">An instruction address</param>
    /// <returns>True if the block handles exceptions at the supplied address, false otherwise.</returns>
    public bool Handles(int address)
    {
        return
            address >= _beginTry &&
            address < (HasFinally ? _beginFinally : HasCatch ? _beginCatch : _endTry);
    }

    /// <summary>
    ///     Determines whether an address belongs to the "guarded block" (CIL). Use <see cref = "Handles" /> to determine whether a try-block provides handlers
    /// </summary>
    /// <param name = "address">An instruction address</param>
    /// <returns>True if the address falls into the "guarded block" (CIL), false otherwise.</returns>
    public bool Spans(int address)
    {
        return
            address >= _beginTry &&
            address < _endTry;
    }

    /// <summary>
    ///     The address one has to jump to in order to skip (finish) the try block.
    /// </summary>
    public int SkipTry
    {
        get
        {
            if (HasFinally)
                return BeginFinally;
            else
                return EndTry;
        }
    }

    /// <summary>
    ///     Determines which of the supplied try-catch-finally blocks
    ///     is supposed to handle an exception at the supplied address.
    /// </summary>
    /// <param name = "address">The address where the excpetion has been caught.</param>
    /// <param name = "blocks">An collection of try-catch-finally candidates.</param>
    /// <returns>The block closest to the address or null if none of the blocks handles that specific address.</returns>
    public static TryCatchFinallyBlock Closest
    (
        int address, IEnumerable<TryCatchFinallyBlock> blocks)
    {
        if (blocks == null)
            throw new ArgumentNullException(nameof(blocks));
        if (address < 0)
            throw new ArgumentOutOfRangeException(nameof(address), "address must be positive.");

        return blocks
            .Where(block => block.Handles(address))
            .Aggregate<TryCatchFinallyBlock, TryCatchFinallyBlock>(null,
                (current, block) =>
                    Closer(address, current, block));
    }

    /// <summary>
    ///     Determines which of the supplied try-catch-finally blocks
    ///     is supposed to handle an exception at the supplied address.
    /// </summary>
    /// <param name = "address">The address where the excpetion has been caught.</param>
    /// <param name = "blocks">An array of try-catch-finally candidates.</param>
    /// <returns>The block closest to the address or null if none of the blocks handles that specific address.</returns>
    public static TryCatchFinallyBlock Closest
    (
        int address, params TryCatchFinallyBlock[] blocks)
    {
        return Closest(address, (ICollection<TryCatchFinallyBlock>) blocks);
    }

    /// <summary>
    ///     Determines which of two try-catch-finally constructs is closer to an address.
    /// </summary>
    /// <param name = "address">The address where an exception has been thrown.</param>
    /// <param name = "a">A try-catch-finally block that handles the address.</param>
    /// <param name = "b">A try-catch-finally block that handles the address.</param>
    /// <returns>The try-catch-finally block that is closer to the address.
    ///     If neither <paramref name = "a" /> nor <paramref name = "b" /> handle the supplied address, null is returned.</returns>
    public static TryCatchFinallyBlock Closer
    (
        int address, TryCatchFinallyBlock a, TryCatchFinallyBlock b)
    {
        if (address < 0)
            throw new ArgumentOutOfRangeException(nameof(address), "address must be positive.");

        if (b == null)
            return a;
        else if (a == null)
            return b;

        if (ReferenceEquals(a, b) || a == b)
            return a;

        if (!a.IsValid || !b.IsValid)
            throw new ArgumentException("One of the try-catch-finally blocks is not valid.");

        var aHandles = a.Handles(address);
        var bHandles = b.Handles(address);

        if (aHandles && !bHandles)
            return a;
        else if (bHandles && !aHandles)
            return b;
        else if (!aHandles)
            return null; //None of the two blocks handles an exception at the given address.

        var aRange = a.Range;
        var bRange = b.Range;

        if (aRange < bRange)
            return a;
        else if (bRange < aRange)
            return b;
        else if (b.HasFinally)
            return b;
        else
            return b;
    }

    /// <summary>
    ///     Returns a human-readable representation of the try-catch-finally construct.
    /// </summary>
    /// <returns>A human-readable representation of the try-catch-finally construct.</returns>
    public override string ToString()
    {
        return
            "try{" + _beginTry + (!(HasFinally || HasCatch) ? ", " + (_beginTry + Range) : "") +
            "}" +
            (HasFinally
                ? "finally{" + _beginFinally + (!HasCatch ? ", " + _endTry : "") + "}"
                : "") +
            (HasCatch ? "catch{" + _beginCatch + ", " + _endTry + "}" : "");
    }

    public MetaEntry ToMetaEntry()
    {
        return (MetaEntry) new MetaEntry[]
        {
            BeginTry.ToString(),
            BeginFinally.ToString(),
            BeginCatch.ToString(),
            EndTry.ToString(),
            UsesException
        };
    }

    public static implicit operator MetaEntry(TryCatchFinallyBlock block)
    {
        if (block == null)
            return (MetaEntry)Array.Empty<MetaEntry>();
        else
            return block.ToMetaEntry();
    }

    #region Comparison

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof (TryCatchFinallyBlock)) return false;
        return Equals((TryCatchFinallyBlock) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var result = _beginTry;
            result = (result*397) ^ _beginFinally;
            result = (result*397) ^ _beginCatch;
            result = (result*397) ^ _endTry;
            result = (result*397) ^ UsesException.GetHashCode();
            return result;
        }
    }

    public bool Equals(TryCatchFinallyBlock other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return other._beginTry == _beginTry && other._beginFinally == _beginFinally &&
            other._beginCatch == _beginCatch && other._endTry == _endTry &&
            other.UsesException.Equals(UsesException);
    }

    public static bool operator ==(TryCatchFinallyBlock a, TryCatchFinallyBlock b)
    {
        if ((object) a == null && (object) b == null)
            return true;
        else if ((object) a == null || (object) b == null)
            return false;
        else
            return a.Equals(b);
    }

    public static bool operator !=(TryCatchFinallyBlock a, TryCatchFinallyBlock b)
    {
        return !(a == b);
    }

    #endregion
}