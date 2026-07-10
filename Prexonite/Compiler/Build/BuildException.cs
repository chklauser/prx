namespace Prexonite.Compiler.Build;

public class BuildException : PrexoniteException
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public BuildException(ITargetDescription relatedTarget)
    {
        RelatedTarget = relatedTarget;
    }

    public BuildException(string message, ITargetDescription? relatedTarget)
        : base(message)
    {
        RelatedTarget = relatedTarget;
    }

    public BuildException(string message, ITargetDescription? relatedTarget, Exception inner)
        : base(message, inner)
    {
        RelatedTarget = relatedTarget;
    }

    public ITargetDescription? RelatedTarget { get; }
}
