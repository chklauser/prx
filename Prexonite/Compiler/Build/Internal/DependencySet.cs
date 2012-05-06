using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    internal class DependencySet : System.Collections.ObjectModel.KeyedCollection<string,ModuleName>, ISet<ModuleName>
    {
        private readonly HashSet<ModuleName> _nameSet = new HashSet<ModuleName>();
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
            ModuleName existing;
            if(TryGetValue(GetKeyForItem(item),out existing))
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

        public bool TryGetValue(string id, out ModuleName moduleName)
        {
            if(Contains(id))
            {
                moduleName = this[id];
                return true;
            }
            else
            {
                moduleName = null;
                return false;
            }
        }
    }
}
