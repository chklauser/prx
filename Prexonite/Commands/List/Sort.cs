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
using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Commands.List;

/// <summary>
///     Implementation of the 'sort' command.
/// </summary>
public class Sort : PCommand
{
    #region Singleton pattern

    /// <summary>
    ///     As <see cref = "Sort" /> cannot be parametrized, Instance returns the one and only instance of the <see cref = "Sort" /> command.
    /// </summary>
    public static Sort Instance { get; } = new();

    Sort()
    {
    }

    #endregion

    /// <summary>
    ///     Sorts an IEnumerable.
    ///     <code>function sort(ref f1(a,b), ref f2(a,b), ... , xs)
    ///         { ... }</code>
    /// </summary>
    /// <param name = "sctx">The stack context in which the sort is performed.</param>
    /// <param name = "args">A list of sort expressions followed by the list to sort.</param>
    /// <returns>The a sorted copy of the list.</returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        args ??= Array.Empty<PValue>();
        var lst = new List<PValue>();
        if (args.Length == 0)
            return PType.Null.CreatePValue();
        else if (args.Length == 1)
        {
            var set = Map._ToEnumerable(sctx, args[0]);
            lst.AddRange(set);
            return (PValue) lst;
        }
        else
        {
            var clauses = new List<PValue>();
            for (var i = 0; i + 1 < args.Length; i++)
                clauses.Add(args[i]);
            lst.AddRange(Map._ToEnumerable(sctx, args[^1]));
            lst.Sort(
                delegate(PValue a, PValue b)
                {
                    foreach (var f in clauses)
                    {
                        var pdec = f.IndirectCall(sctx, new[] {a, b});
                        if (!(pdec.Type is IntPType))
                            pdec = pdec.ConvertTo(sctx, PType.Int);
                        var dec = (int) pdec.Value;
                        if (dec != 0)
                            return dec;
                    }
                    return 0;
                });
            return (PValue) lst;
        }
    }

    //which might lead to initialization of the application.
}