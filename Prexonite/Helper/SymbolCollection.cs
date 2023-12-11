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

using System.Collections;
using System.Diagnostics;

namespace Prexonite;

[DebuggerStepThrough]
public class SymbolCollection(int capacity) : ICollection<string>
{
    readonly HashSet<string> _set = new(capacity, StringComparer.OrdinalIgnoreCase);

    public SymbolCollection() : this(0)
    {
        _set = new(0, StringComparer.OrdinalIgnoreCase);
    }

    public SymbolCollection(IEnumerable<string> items)
        : this()
    {
        foreach (var item in items)
            Add(item);
    }

    #region ICollection<string> Members

    public void Add(string item)
    {
        _set.Add(item);
    }

    public void Clear()
    {
        _set.Clear();
    }

    public bool Contains(string? item)
    {
        return item != null && _set.Contains(item);
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
        _set.CopyTo(array, arrayIndex);
    }

    public int Count => _set.Count;

    public bool IsReadOnly => false;

    public bool Remove(string? item)
    {
        if (item == null)
        {
            return false;
        }
        
        return _set.Remove(item);
    }

    #endregion

    #region IEnumerable<string> Members

    public IEnumerator<string> GetEnumerator() => _set.GetEnumerator();

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _set.GetEnumerator();
    }

    #endregion
}