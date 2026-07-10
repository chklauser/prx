using Prexonite.Modular;

namespace Prexonite.Compiler.Modular { }

namespace Prexonite.Compiler.Build
{
    public interface ITargetDescription : IDependent<ModuleName>
    {
        IReadOnlyCollection<ModuleName> Dependencies { get; }

        IReadOnlyList<Message> BuildMessages { get; }

        Task<ITarget> BuildAsync(
            IBuildEnvironment build,
            IDictionary<ModuleName, Task<ITarget>> dependencies,
            CancellationToken token
        );

        IEnumerable<ModuleName> IDependent<ModuleName>.GetDependencies() => Dependencies;
    }
}
