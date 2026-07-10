namespace Prexonite.Compiler;

public class SourcePosition : ISourcePosition, IEquatable<SourcePosition>
{
    public SourcePosition(string? file, int line, int column)
    {
        File = file ?? "unknown~";
        Line = line;
        Column = column;
    }

    #region Implementation of ISourcePosition

    /// <summary>
    ///     The source file that declared this object.
    /// </summary>
    public string File { get; }

    /// <summary>
    ///     The line in the source file that declared this object.
    /// </summary>
    public int Line { get; }

    /// <summary>
    ///     The column in the source file that declared this object.
    /// </summary>
    public int Column { get; }

    #endregion

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <returns>
    ///     true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
    /// </returns>
    /// <param name = "other">An object to compare with this object.</param>
    public bool Equals(SourcePosition? other)
    {
        return !ReferenceEquals(null, other);
    }

    /// <summary>
    ///     Determines whether the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     true if the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />; otherwise, false.
    /// </returns>
    /// <param name = "obj">The <see cref = "T:System.Object" /> to compare with the current <see cref = "T:System.Object" />. </param>
    /// <filterpriority>2</filterpriority>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != typeof(SourcePosition))
            return false;
        return Equals((SourcePosition)obj);
    }

    /// <summary>
    ///     Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    ///     A hash code for the current <see cref = "T:System.Object" />.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode()
    {
        return (File ?? string.Empty).GetHashCode() ^ Line ^ (Column + 256);
    }

    public override string ToString()
    {
        return File != null ? $"{File} {Line}.{Column}" : $"{Line}.{Column}";
    }
}
