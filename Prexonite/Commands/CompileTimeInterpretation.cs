using Prexonite.Modular;

namespace Prexonite.Commands
{
    /// <summary>
    ///     The different interpretations of a compile-time value.
    /// </summary>
    public enum CompileTimeInterpretation
    {
        /// <summary>
        ///     A <code>null</code>-literal. Obtained from <code>ldc.null</code>. Null is the default interpretation.
        /// </summary>
        Null = 0,

        /// <summary>
        ///     A string literal. Obtained from <code>ldc.string</code>. Represented as <see cref = "string" />.
        /// </summary>
        String,

        /// <summary>
        ///     An integer literal. Obtained from <code>ldc.int</code>. Represented as <see cref = "int" />.
        /// </summary>
        Int,

        /// <summary>
        ///     A boolean literal. Obtained from <code>ldc.bool</code>. Represented as <see cref = "bool" />.
        /// </summary>
        Bool,

        /// <summary>
        ///     A local variable reference literal. Obtained from <code>ldr.loc</code> and <code>ldr.loci</code>. Represented as an <see
        ///      cref = "EntityRef" />.
        /// </summary>
        LocalVariableReference,

        /// <summary>
        ///     A global variable reference literal. Obtained from <code>ldr.glob</code>. Represented as an <see cref = "EntityRef" />.
        /// </summary>
        GlobalVariableReference,

        /// <summary>
        ///     A function reference literal. Obtained from <code>ldr.func</code>. Represented as an <see cref = "EntityRef" />.
        /// </summary>
        FunctionReference,

        /// <summary>
        ///     A command reference literal. Obtained from <code>ldr.cmd</code>. Represented as an <see cref = "EntityRef" />.
        /// </summary>
        CommandReference
    }
}