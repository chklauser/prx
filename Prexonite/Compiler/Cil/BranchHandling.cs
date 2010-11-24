namespace Prexonite.Compiler.Cil
{
    /// <summary>
    /// Indicates how a branching instruction must be handled.
    /// </summary>
    public enum BranchHandling
    {
        /// <summary>
        /// A normal branching instruction can be used (br, brtrue, etc.)
        /// </summary>
        Branch,

        /// <summary>
        /// A leave instruction must be used (leave, leave.s)
        /// </summary>
        Leave,

        /// <summary>
        /// An endfinally must be used.
        /// </summary>
        EndFinally,

        /// <summary>
        /// A leave instruction is used, and the target is the natural control flow target after the try block.
        /// </summary>
        LeaveSkipTry,

        /// <summary>
        /// The jump in question is illegal in CIL. It cannot be repaired on the fly.
        /// </summary>
        Invalid
    }
}