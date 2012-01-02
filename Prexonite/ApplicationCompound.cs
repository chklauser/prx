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