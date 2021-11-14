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

using System.Collections.Generic;
using System.Linq;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal;

internal class DependencySet : System.Collections.ObjectModel.KeyedCollection<string,ModuleName>, ISet<ModuleName>
{
    private readonly HashSet<ModuleName> _nameSet = new();
    private readonly ModuleName _correspondingModule;

    protected override void InsertItem(int index, ModuleName item)
    {
        _nameSet.Add(item);
        base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        var existing = this[index];
        if(existing != null)
            _nameSet.Remove(existing);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, ModuleName item)
    {
        var existing = this[index];
        if (existing != null)
            _nameSet.Remove(existing);
        _nameSet.Add(item);
        base.SetItem(index, item);
    }

    public DependencySet(ModuleName correspondingModule)
    {
        this._correspondingModule = correspondingModule;
    }

    protected override string GetKeyForItem(ModuleName item)
    {
        return item.Id;
    }

    private void _throwConflict(ModuleName newModule, ModuleName existingModule)
    {
        throw new VersionConflictException(existingModule, newModule, _correspondingModule);
    }

    public bool IsSubsetOf(IEnumerable<ModuleName> other)
    {
        return _nameSet.IsSubsetOf(other);
    }

    public bool IsProperSubsetOf(IEnumerable<ModuleName> other)
    {
        return _nameSet.IsProperSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<ModuleName> other)
    {
        return _nameSet.IsSupersetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<ModuleName> other)
    {
        return _nameSet.IsProperSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<ModuleName> other)
    {
        return _nameSet.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<ModuleName> other)
    {
        return _nameSet.SetEquals(other);
    }

    bool ISet<ModuleName>.Add(ModuleName item)
    {
        if(TryGetValue(GetKeyForItem(item),out var existing))
        {
            if(existing == item)
            {
                return false;
            }
            else
            {
                _throwConflict(item, existing);
                return false;
            }
        } 
        else
        {
            Add(item);
            return true;
        }
    }

    public void AddRange(IEnumerable<ModuleName> items)
    {
        foreach (var item in items)
            Add(item);
    }

    public void UnionWith(IEnumerable<ModuleName> other)
    {
        AddRange(other.Except(_nameSet));
    }

    public void IntersectWith(IEnumerable<ModuleName> other)
    {
        foreach (var item in other.Except(_nameSet))
            Remove(item);
    }

    public void ExceptWith(IEnumerable<ModuleName> other)
    {
        foreach (var item in other.Intersect(_nameSet))
            Remove(item);
    }

    public void SymmetricExceptWith(IEnumerable<ModuleName> other)
    {
        ExceptWith(other);
    }
}