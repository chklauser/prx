// Prexonite
//
// Copyright (c) 2024, Christian Klauser
// All rights reserved.
//
// Licensed under the BSD 3-Clause License. See LICENSE for details.

namespace Prexonite.Compiler.ParserV2;

/// <summary>A 1-based line/column position in a source file.</summary>
public readonly record struct SourcePos(int Line, int Column)
{
    public static readonly SourcePos Unknown = new(0, 0);
    public override string ToString() => $"{Line}:{Column}";
}

/// <summary>A contiguous span of source text identified by file + start/end positions.</summary>
public readonly record struct SourceSpan(string File, SourcePos Start, SourcePos End)
{
    public static SourceSpan Synthetic(string file = "<synthetic>") =>
        new(file, SourcePos.Unknown, SourcePos.Unknown);

    /// <summary>Smallest span covering both <paramref name="a"/> and <paramref name="b"/>.</summary>
    public static SourceSpan Merge(SourceSpan a, SourceSpan b) =>
        new(a.File, a.Start, b.End);

    /// <summary>Merge multiple spans, ignoring any with unknown positions.</summary>
    public static SourceSpan MergeAll(IEnumerable<SourceSpan> spans)
    {
        string? file = null;
        SourcePos? start = null;
        SourcePos? end = null;
        foreach (var s in spans)
        {
            if (s.Start == SourcePos.Unknown)
                continue;
            file ??= s.File;
            if (start == null || ComparePos(s.Start, start.Value) < 0) start = s.Start;
            if (end == null || ComparePos(s.End, end.Value) > 0) end = s.End;
        }
        return new(file ?? "<synthetic>", start ?? SourcePos.Unknown, end ?? SourcePos.Unknown);
    }

    static int ComparePos(SourcePos a, SourcePos b)
    {
        var lineCmp = a.Line.CompareTo(b.Line);
        return lineCmp != 0 ? lineCmp : a.Column.CompareTo(b.Column);
    }

    public static readonly SourceSpan Unknown = Synthetic();
    public override string ToString() => Start == SourcePos.Unknown
        ? $"{File}(unknown)"
        : $"{File}({Start}-{End})";

    /// <summary>A zero-width span at the start of this span, useful for marking the opening of a construct.</summary>
    public SourceSpan AtStart() => new(File, Start, Start);
}
