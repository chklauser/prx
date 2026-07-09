using System.Collections.ObjectModel;


namespace Prexonite.Modular;

public class VariableTable : KeyedCollection<string,VariableDeclaration>
{
    protected override string GetKeyForItem(VariableDeclaration item)
    {
        return item.Id;
    }

    public VariableTable() : base(Engine.DefaultStringComparer)
    {
    }

    public bool TryGetVariable(string id, [NotNullWhen(true)] out VariableDeclaration? variable)
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