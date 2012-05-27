namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// Indicates how an operation behaves with respect to the Prexonite evaluation stack.
    /// </summary>
    /// <remarks>WARNING: Do not extend this enumeration. Users assume that 
    /// <see cref="Value"/> and <see cref="Effect"/> are its only two members.</remarks>
    public enum StackSemantics
    {

        /// <summary>
        /// Indicates that the operation pushes a single value onto the
        /// evaluation stack.  May also have side effects.
        /// </summary>
        Value,
        /// <summary>
        /// Indicates that the operation does not modify the 
        /// evaluation stack, but may have side effects.
        /// </summary>
        Effect
    }
}