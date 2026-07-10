namespace Prexonite.Compiler.Build.Internal;

class StringSource : ISource
{
    readonly string _source;

    public StringSource(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    #region Implementation of ISource

    public bool CanOpen => true;

    public bool IsSingleUse => false;

    public bool TryOpen(out TextReader reader)
    {
        reader = new StringReader(_source);
        return true;
    }

    #endregion
}
