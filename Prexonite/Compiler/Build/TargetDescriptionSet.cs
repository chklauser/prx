// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public sealed class TargetDescriptionSet 
        : ICollection<ITargetDescription>
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<ModuleName, ITargetDescription> _table = new ConcurrentDictionary<ModuleName, ITargetDescription>();

        private TargetDescriptionSet()
        {
        }

        public static TargetDescriptionSet Create()
        {
            return new TargetDescriptionSet();
        }

        public bool TryGetValue(ModuleName name, out ITargetDescription description)
        {
            if ((object) name == null)
                throw new ArgumentNullException("name");
            return _table.TryGetValue(name, out description);
        }

        public void Replace(ITargetDescription oldDescription, ITargetDescription newDescription)
        {
            if (oldDescription == null)
                throw new ArgumentNullException("oldDescription");
            if (newDescription == null)
                throw new ArgumentNullException("newDescription");
            if(oldDescription.Name != newDescription.Name)
                throw new ArgumentException(
                    string.Format(
                        "Cannot replace description for {0} with a description for a different module ({1}).",
                        oldDescription.Name, newDescription.Name));
            if (!_table.TryUpdate(oldDescription.Name, newDescription, oldDescription))
            {
                throw new PrexoniteException(
                    "Failed to update target description set. Propably due to concurrent modification.");
            }
        }

        public ITargetDescription this[ModuleName name]
        {
            get
            {
                ITargetDescription description;
                if (_table.TryGetValue(name, out description))
                    return description;
                else
                    throw new KeyNotFoundException(string.Format("Cannot find target description for module {0}.", name));
            }
        }

        public bool Contains(ModuleName name)
        {
            return _table.ContainsKey(name);
        }

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<out ITargetDescription>

        public IEnumerator<ITargetDescription> GetEnumerator()
        {
            return _table.Values.GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<ITargetDescription>

        public void Add(ITargetDescription item)
        {
            if(!_table.TryAdd(item.Name,item))
                throw new ArgumentException(string.Format("A target description for this module name already exists: {0}", item.Name));
        }

        public void Clear()
        {
            _table.Clear();
        }

        public bool Contains(ITargetDescription item)
        {
            if (item == null)
                throw new System.ArgumentNullException("item");
            ITargetDescription value;
            return _table.TryGetValue(item.Name, out value) && item.Equals(value);
        }

        public void CopyTo(ITargetDescription[] array, int arrayIndex)
        {
            _table.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(ITargetDescription item)
        {
            if (item == null)
                throw new System.ArgumentNullException("item");
            ITargetDescription value;
            return _table.TryRemove(item.Name, out value);
        }

        public int Count
        {
            get { return _table.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}