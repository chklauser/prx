// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite
{
    internal class ApplicationCompoundImpl : ApplicationCompound
    {
        private readonly KeyedCollection<ModuleName, Application> _table = new AppTable();

        private CentralCache _cache = CentralCache.Create();

        public override CentralCache Cache
        {
            get { return _cache; }
            internal set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _cache = value;
            }
        }

        public override int Count
        {
            get { return _table.Count; }
        }

        public override IEnumerator<Application> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        internal override void _Unlink(Application application)
        {
            // ReSharper disable RedundantAssignment
            bool r = _table.Remove(application);
            Debug.Assert(r,
                "Tried to _Unlink an application that wasn't part of the compound. Probable cause of bugs");
            // ReSharper restore RedundantAssignment
        }

        internal override void _Link(Application application)
        {
            Application current;
            if (TryGetApplication(application.Module.Name, out current))
            {
                if (Equals(current, application))
                {
                    return; //merging
                }
                else
                {
                    throw new ModuleConflictException(
                        "Attempted to link two instantiations of the same module (or modules wth the same name/version).",
                        application.Module,
                        current.Module);
                }
            }
            application.Module.Cache = application.Module.Cache.LinkInto(Cache);
            _table.Add(application);
        }

        internal override void _Clear()
        {
            _table.Clear();
        }

        public override bool Contains(ModuleName item)
        {
            return _table.Contains(item);
        }

        public override bool TryGetApplication(ModuleName moduleName, out Application application)
        {
            if (_table.Contains(moduleName))
            {
                application = _table[moduleName];
                return true;
            }
            else
            {
                application = null;
                return false;
            }
        }

        public override void CopyTo(Application[] array, int arrayIndex)
        {
            _table.CopyTo(array, arrayIndex);
        }

        #region Nested type: AppTable

        private class AppTable : KeyedCollection<ModuleName, Application>
        {
            protected override ModuleName GetKeyForItem(Application item)
            {
                return item.Module.Name;
            }
        }

        #endregion
    }
}