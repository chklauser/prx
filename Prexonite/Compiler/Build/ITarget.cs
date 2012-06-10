using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    [PublicAPI]
    public interface ITarget
    {
        [PublicAPI]
        Module Module
        {
            get;
        }

        [PublicAPI]
        ICollection<IResourceDescriptor> Resources
        {
            get;
        }

        [PublicAPI]
        SymbolStore Symbols
        {
            get;
        }

        [PublicAPI]
        ModuleName Name
        {
            get;
        }

        //TODO: replace with IReadOnlyCollection in .NET 4.5
        [PublicAPI]
        ICollection<Message> Messages { get; }

        [PublicAPI]
        Exception Exception { get; }

        bool IsSuccessful { get; }
    }

    public static class Target
    {
        public static void ThrowIfFailed(this ITarget target, ITargetDescription description)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (description == null)
                throw new ArgumentNullException("description");
            if (target.Exception != null)
                throw target.Exception;
            else if (target.Messages.Any(m => m.Severity == MessageSeverity.Error))
                throw new BuildFailureException(description,
                                                "There were {0} {1} while translating " + description.Name + ".",
                                                target.Messages);
        }
    }
}
