

using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

public interface IPlan
{
    /// <summary>
    /// Set of build watchers that will get access to events raised during building.
    /// </summary>
    ISet<IBuildWatcher> BuildWatchers
    {
        get;
    }

    TargetDescriptionSet TargetDescriptions
    {
        get;
    }

    IDictionary<ModuleName,Task<ITarget>> BuildAsync(IEnumerable<ModuleName> names, CancellationToken token);

    Task<ITarget> BuildAsync(ModuleName name, CancellationToken token) => 
        BuildAsync(name.Singleton(), token)[name];

    Task<(Application Application, ITarget Target)> LoadAsync(ModuleName name, CancellationToken token) => 
        LoadAsync(name.Singleton(), token)[name];

    IDictionary<ModuleName, Task<(Application Application, ITarget Target)>> LoadAsync(IEnumerable<ModuleName> names, CancellationToken token);

    LoaderOptions? Options { get; set; }
}