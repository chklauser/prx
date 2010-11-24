using System;

namespace Prexonite.Compiler.Cil.Seh
{
    [Flags]
    internal enum RegionKind
    {
        Try = 1,
        Catch = 2,
        Finally = 4
    }

    internal static class RegionKindExtensions
    {
        public static bool IsIn(this RegionKind subject, RegionKind mask)
        {
            return (subject & mask) == subject;
        }

        public static bool IsOfKind(this Region subject, RegionKind mask)
        {
            if(subject == null)
                return false;
            else
                return subject.Kind.IsIn(mask);
        }
    }
}