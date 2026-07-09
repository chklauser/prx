

namespace Prexonite.Compiler;

public static class SourcePositionExtensions
{
    extension(ISourcePosition? positionA)
    {
        public bool SourcePositionEquals(ISourcePosition? positionB)
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

        public string GetSourcePositionString()
        {
            return $"{positionA.Line}: line {positionA.Line}, col {positionA.Column}";
        }
    }
}