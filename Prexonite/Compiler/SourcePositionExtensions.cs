using System;

namespace Prexonite.Compiler
{
    public static class SourcePositionExtensions
    {
        public static bool SourcePositionEquals(this ISourcePosition positionA, ISourcePosition positionB)
        {
            if (ReferenceEquals(positionA, null) && ReferenceEquals(positionB, null))
                return true;
            else if (ReferenceEquals(positionA, null) || ReferenceEquals(positionB, null))
                return false;
            else
                return positionA.Line == positionB.Line
                       && positionA.Column == positionB.Column
                       && positionA.File.Equals(positionB.File, StringComparison.Ordinal);
        }

        public static string GetSourcePositionString(this ISourcePosition position)
        {
            return string.Format("{0}: line {1}, col {2}", position.Line, position.Line, position.Column);
        }
    }
}