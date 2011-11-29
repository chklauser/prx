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
    }
}
