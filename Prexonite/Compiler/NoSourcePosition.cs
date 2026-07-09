

using JetBrains.Annotations;

namespace Prexonite.Compiler;

[PublicAPI]
public sealed class NoSourcePosition : ISourcePosition
{
    #region Singleton
    public static ISourcePosition Instance { get; } = new NoSourcePosition();

    #endregion

    public const string MissingFileName = "-";

    #region Implementation of ISourcePosition

    public string File => MissingFileName;

    public int Line => -1;

    public int Column => -1;

    #endregion
}