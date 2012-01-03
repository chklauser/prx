using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Prexonite.Modular;

namespace Prexonite.Internal
{
    /// <summary>
    ///     Wraps a SymbolicTable&lt;PFunction&gt; to ensure that a function is stored with it's Id as the key.
    /// </summary>
    [DebuggerStepThrough]
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

        public override PFunction this[string id]
        {
            get { return _table.GetDefault(id, null); }
        }

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
            PFunction func;
            if(TryGetValue(sender.Id,out func))
            {
                _table.Remove(func.Id);
                _table.Add(args.NewId,func);
            }
            else
            {
                Debug.Assert(false,
                    string.Format(
                        "PFunction table is still registered to function declaration {0} even though it is no longer in the table.",
                        sender));
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
            PFunction oldFunc;
            if (_table.TryGetValue(item.Id,out oldFunc))
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

        public override int Count
        {
            get { return _table.Count; }
        }

        public override bool IsReadOnly
        {
            get { return _table.IsReadOnly; }
        }

        public override bool Remove(PFunction item)
        {
            PFunction f;
            if(_table.TryGetValue(item.Id,out f) && ReferenceEquals(f,item))
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
            PFunction oldFunc;
            if (_table.TryGetValue(id,out oldFunc))
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
}