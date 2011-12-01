using System;
using System.Collections;
using System.Collections.Generic;
using Prexonite.Modular;

namespace Prexonite
{
    public abstract class ApplicationCompound : ICollection<Application>
    {
        public static ApplicationCompound Create()
        {
            return new ApplicationCompoundImpl();
        }

        public abstract IEnumerator<Application> GetEnumerator();
        public void Add(Application item)
        {
            throw new NotSupportedException();   
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public abstract bool Contains(ModuleName item);
        public abstract bool TryGetApplication(ModuleName moduleName, out Application application);
        public abstract void CopyTo(Application[] array, int arrayIndex);

        public bool Contains(Application  application)
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

        public abstract Application this[ModuleName name] { get; }

        internal abstract void _Unlink(Application application);
        internal abstract void _Link(Application application);
        internal abstract void _Clear();
    }
}
