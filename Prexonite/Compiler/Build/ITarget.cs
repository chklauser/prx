using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

[PublicAPI]
public interface ITarget
{
    [PublicAPI]
    Module Module { get; }

    [PublicAPI]
    IReadOnlyCollection<IResourceDescriptor> Resources { get; }

    [PublicAPI]
    SymbolStore Symbols { get; }

    [PublicAPI]
    ModuleName Name { get; }

    [PublicAPI]
    IReadOnlyCollection<Message> Messages { get; }

    [PublicAPI]
    Exception? Exception { get; }

    bool IsSuccessful { get; }
}

public static class Target
{
    extension(ITarget target)
    {
        public void ThrowIfFailed(ITargetDescription description)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (target.Exception != null)
                throw target.Exception;
            else if (target.Messages.Any(m => m.Severity == MessageSeverity.Error))
                throw new BuildFailureException(
                    description,
                    "There {2} {0} {1} while translating " + description.Name + ".",
                    target.Messages
                );
        }
    }
}
