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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite
{
    public abstract class ApplicationCompound : ICollection<Application>, IObject
    {
        private static readonly ObjectPType _compoundType =
            PType.Object[typeof (ApplicationCompound)];

        public abstract CentralCache Cache { get; internal set; }

        #region ICollection<Application> Members

        public abstract IEnumerator<Application> GetEnumerator();

        public void Add(Application item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public abstract void CopyTo(Application[] array, int arrayIndex);

        public bool Contains(Application application)
        {
            Application currentApplication;
            if (TryGetApplication(application.Module.Name, out currentApplication))
                return Equals(application, currentApplication);
            else
                return false;
        }

        public bool Remove(Application item)
        {
            throw new NotSupportedException();
        }

        public abstract int Count { get; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IObject Members

        bool IObject.TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
            out PValue result)
        {
            return TryDynamicCall(sctx, args, call, id, out result);
        }

        #endregion

        public static ApplicationCompound Create()
        {
            return new ApplicationCompoundImpl();
        }

        public abstract bool TryGetApplication(ModuleName moduleName, out Application application);

        internal abstract void _Unlink(Application application);
        internal abstract void _Link(Application application);
        internal abstract void _Clear();

        public abstract bool Contains(ModuleName moduleName);

        protected virtual bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call,
            string id, out PValue result)
        {
            switch (id.ToUpperInvariant())
            {
                case "TRYGETAPPLICATION":
                    if (args.Length >= 2)
                    {
                        var moduleName = args[0].ConvertTo<ModuleName>(sctx);
                        PValue target = args[1];
                        Application application;
                        if (TryGetApplication(moduleName, out application))
                        {
                            target.IndirectCall(sctx,
                                new[] {sctx.CreateNativePValue(application)});
                            result = true;
                        }
                        else
                        {
                            target.IndirectCall(sctx, new PValue[] {PType.Null});
                            result = false;
                        }
                        return true;
                    }
                    goto default;
                default:
                    MemberInfo dummyMember;
                    return _compoundType.TryDynamicCall(sctx, sctx.CreateNativePValue(this), args,
                        call, id, out result, out dummyMember, true);
            }
        }
    }
}