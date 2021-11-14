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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Prexonite.Modular;

namespace Prexonite.Internal;

/// <summary>
///     Wraps a SymbolicTable&lt;PFunction&gt; to ensure that a function is stored with it's Id as the key.
/// </summary>
public class PFunctionTableImpl : PFunctionTable
{
    #region table

    private readonly SymbolTable<PFunction> _table;

    public PFunctionTableImpl()
    {
        _table = new SymbolTable<PFunction>();
        _idChangingHandler = _onIdChanging;
    }

    public PFunctionTableImpl(int capacity)
    {
        _table = new SymbolTable<PFunction>(capacity);
        _idChangingHandler = _onIdChanging;
    }

    public override bool Contains(string id)
    {
        return _table.ContainsKey(id);
    }

    public override bool TryGetValue(string id, out PFunction func)
    {
        return _table.TryGetValue(id, out func);
    }

    public override PFunction this[string id] => _table.GetDefault(id, null);

    #endregion

    #region Storage

    public override void Store(TextWriter writer)
    {
        foreach (var kvp in _table)
            kvp.Value.Store(writer);
    }

    #endregion

    #region ICollection<PFunction> Members

    private readonly EventHandler<FunctionIdChangingEventArgs> _idChangingHandler;

    [EditorBrowsable(EditorBrowsableState.Never)]
    private void _onIdChanging(object o, FunctionIdChangingEventArgs args)
    {
        var sender = (FunctionDeclaration)o;
        if(TryGetValue(sender.Id,out var func))
        {
            _table.Remove(func.Id);
            _table.Add(args.NewId,func);
        }
        else
        {
            Debug.Assert(false,
                $"PFunction table is still registered to function declaration {sender} even though it is no longer in the table.");
        }
    }

    public override void Add(PFunction item)
    {
        if (_table.ContainsKey(item.Id))
            throw new ArgumentException(
                "The function table already contains a function named " + item.Id);

        item.Declaration.IdChanging += _idChangingHandler;
        _table.Add(item.Id, item);
    }

    public override void AddOverride(PFunction item)
    {
        if (_table.TryGetValue(item.Id,out var oldFunc))
        {
            oldFunc.Declaration.IdChanging -= _idChangingHandler;
            _table.Remove(oldFunc.Id);
        }

        item.Declaration.IdChanging += _idChangingHandler;
        _table.Add(item.Id, item);
    }

    public override void Clear()
    {
        foreach (var func in _table)
            func.Value.Declaration.IdChanging -= _idChangingHandler;

        _table.Clear();
    }

    public override bool Contains(PFunction item)
    {
        return _table.ContainsKey(item.Id);
    }

    public override void CopyTo(PFunction[] array, int arrayIndex)
    {
        if (_table.Count + arrayIndex > array.Length)
            throw new ArgumentException("Array to copy functions into is not long enough.");

        var i = arrayIndex;
        foreach (var kvp in _table)
        {
            if (i >= array.Length)
                break;
            array[i++] = kvp.Value;
        }
    }

    public override int Count => _table.Count;

    public override bool IsReadOnly => _table.IsReadOnly;

    public override bool Remove(PFunction item)
    {
        if(_table.TryGetValue(item.Id,out var f) && ReferenceEquals(f,item))
        {
            f.Declaration.IdChanging -= _idChangingHandler;
            _table.Remove(item.Id);
            return true;
        }
        else
            return false;
    }

    public override bool Remove(string id)
    {
        if (_table.TryGetValue(id,out var oldFunc))
        {
            oldFunc.Declaration.IdChanging -= _idChangingHandler;
            return _table.Remove(id);
        }
        else
            return false;
    }

    #endregion

    #region IEnumerable<PFunction> Members

    public override IEnumerator<PFunction> GetEnumerator()
    {
        foreach (var kvp in _table)
            yield return kvp.Value;
    }

    #endregion

    #region IEnumerable Members

    #endregion
}