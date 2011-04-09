using System;
using System.Collections;
using System.Collections.Generic;

namespace Prexonite.Helper
{
    public class ReadOnlyCollectionView<T> : ICollection<T>
    {
        private readonly ICollection<T> _list;

        public ReadOnlyCollectionView(ICollection<T> list)
        {
            _list = list;
        }

        #region Implementation of interface (throwing exceptions)

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Delegate to _list

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

    }
}