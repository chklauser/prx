namespace Prexonite.Compiler.Cil.Seh;

[Flags]
enum RegionKind
{
    Try = 1,
    Catch = 2,
    Finally = 4,
}

static class RegionKindExtensions
{
    extension(RegionKind subject)
    {
        public bool IsIn(RegionKind mask) => (subject & mask) == subject;
    }

    extension(Region? subject)
    {
        public bool IsOfKind(RegionKind mask) => subject != null && subject.Kind.IsIn(mask);
    }
}
