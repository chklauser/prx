namespace Prexonite.Compiler.Ast;

/// <summary>
///     Represents the binary operators supported by <see cref = "IAstFactory.BinaryOperation" />.
/// </summary>
/// <seealso cref = "IAstFactory" />
/// <seealso cref = "AstUnaryOperator" />
/// <seealso cref = "AstNode" />
/// <seealso cref = "AstExpr" />
public enum BinaryOperator
{
    /// <summary>
    ///     No operator, invalid in most contexts.
    /// </summary>
    None,

    //Binary operators with a direct equivalent in assembler code

    /// <summary>
    ///     The addition operator
    /// </summary>
    Addition,

    /// <summary>
    ///     The subtraction operator
    /// </summary>
    Subtraction,

    /// <summary>
    ///     The multiplication operator
    /// </summary>
    Multiply,

    /// <summary>
    ///     The division operator
    /// </summary>
    Division,

    /// <summary>
    ///     The modulus division operator
    /// </summary>
    Modulus,

    /// <summary>
    ///     The exponential operator
    /// </summary>
    Power,

    /// <summary>
    ///     The bitwise AND operator
    /// </summary>
    BitwiseAnd,

    /// <summary>
    ///     The bitwise OR operator
    /// </summary>
    BitwiseOr,

    /// <summary>
    ///     The bitwise XOR operator
    /// </summary>
    ExclusiveOr,

    /// <summary>
    ///     The equality operator
    /// </summary>
    Equality,

    /// <summary>
    ///     The inequality operator
    /// </summary>
    Inequality,

    /// <summary>
    ///     The greater than operator
    /// </summary>
    GreaterThan,

    /// <summary>
    ///     The greater than or equal operator
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    ///     The less than operator
    /// </summary>
    LessThan,

    /// <summary>
    ///     The less than or equal operator
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    ///     The ??-operator.
    /// </summary>
    /// <remarks>
    ///     Will result in a AstCoalescence node when optimized.
    /// </remarks>
    Coalescence,

    /// <summary>
    ///     Entry for ~= expressions.
    /// </summary>
    /// <remarks>
    ///     Will result in a AstTypecast node when optimized.
    /// </remarks>
    Cast,

    ///<summary>
    ///     The binary |&gt; operator. No fixed semantics.
    /// </summary>
    DeltaRight,

    ///<summary>
    ///    The binary &lt;| operator. No fixed semantics
    ///</summary>
    DeltaLeft,
}
