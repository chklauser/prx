using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Modular
{
    public class VariableTable : System.Collections.ObjectModel.KeyedCollection<string,VariableDeclaration>
    {
        protected override string GetKeyForItem(VariableDeclaration item)
        {
            return item.Id;
        }

        public VariableTable() : base(Engine.DefaultStringComparer)
        {
        }

        public bool TryGetVariable(string id, out VariableDeclaration variable)
        {
            if (Contains(id))
            {
                variable = this[id];
                return true;
            }
            else
            {
                variable = null;
                return false;
            }
        }
    }
}
