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
using System.Linq;
using Prexonite.Commands.List;

namespace Prexonite;

public static class DependencyEntity<T>
{
    public static DependencyEntity<T, PValue> CreateDynamic(StackContext sctx, T name,
        PValue value, PValue getDependencies)
    {
        return new(name, value,
            _dynamicallyCallGetDependencies(sctx, getDependencies));
    }

    private static Func<PValue, IEnumerable<T>> _dynamicallyCallGetDependencies(
        StackContext sctx, PValue getDependenciesPV)
    {
        if (getDependenciesPV == null)
            return null;

        return value =>
        {
            var depsPV = getDependenciesPV.IndirectCall(sctx, new[] {value});

            var depsDynamic = Map._ToEnumerable(sctx, depsPV);
            if (depsDynamic == null)
                throw new PrexoniteException(
                    "getDependencies function did not return enumerable.");

            return depsDynamic as IEnumerable<T>
                ?? (from pv in depsDynamic
                    select pv.ConvertTo<T>(sctx, true));
        };
    }
}

public class DependencyEntity<TKey, TValue> : IDependent<TKey>
{
    private readonly Func<TValue, IEnumerable<TKey>> _getDependencies;

    public DependencyEntity(TKey name, TValue value,
        Func<TValue, IEnumerable<TKey>> getDependencies)
    {
        Name = name;
        _getDependencies = getDependencies ?? throw new NullReferenceException("getDependencies");
        Value = value;
    }

    #region Implementation of INamed<TKey>

    public TKey Name { get; }

    #endregion

    public TValue Value { get; }

    #region Implementation of IDependent<TKey>

    public IEnumerable<TKey> GetDependencies()
    {
        return _getDependencies(Value);
    }

    #endregion
}