

using System.Collections.ObjectModel;
using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite;

class ApplicationCompoundImpl : ApplicationCompound
{
    readonly KeyedCollection<ModuleName, Application> _table = new AppTable();

    CentralCache _cache = CentralCache.Create();

    public override CentralCache Cache
    {
        get => _cache;
        internal set => _cache = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override int Count => _table.Count;

    public override IEnumerator<Application> GetEnumerator()
    {
        return _table.GetEnumerator();
    }

    internal override void _Unlink(Application application)
    {
        // ReSharper disable RedundantAssignment
        var r = _table.Remove(application);
        Debug.Assert(r,
            "Tried to _Unlink an application that wasn't part of the compound. Probable cause of bugs");
        // ReSharper restore RedundantAssignment
    }

    internal override void _Link(Application application)
    {
        if (TryGetApplication(application.Module.Name, out var current))
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

    public override bool TryGetApplication(ModuleName moduleName, [NotNullWhen(true)] out Application? application)
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

    class AppTable : KeyedCollection<ModuleName, Application>
    {
        protected override ModuleName GetKeyForItem(Application item)
        {
            return item.Module.Name;
        }
    }

    #endregion
}