#nullable enable
using System;
using Prexonite.Compiler.Ast;

namespace Prexonite
{
    /// <summary>
    ///     Provides constants for the names of operators (binary, unary).
    /// </summary>
    public static class OperatorNames
    {
        /// <summary>
        ///     Provides constants for the names of operators in Prexonite
        /// </summary>
        public static class Prexonite
        {
            public const string Addition = "plus";
            public const string Subtraction = "minus";
            public const string Multiplication = "times";
            public const string Division = "dividedBy";
            public const string Modulus = "mod";
            public const string Power = "raisedTo";
            public const string BitwiseAnd = "bitwiseAnd";
            public const string BitwiseOr = "bitwiseOr";
            public const string ExclusiveOr = "xor";
            public const string Equality = "isEqualTo";
            public const string Inequality = "isInequalTo";
            public const string GreaterThan = "isGreaterThan";
            public const string GreaterThanOrEqual = "isGreaterThanOrEqual";
            public const string LessThan = "isLessThan";
            public const string LessThanOrEqual = "isLessThanOrEqual";

            public const string UnaryNegation = "negation";
            public const string OnesComplement = "complement";
            public const string LogicalNot = "not";

            public const string Increment = "increment";
            public const string Decrement = "decrement";

            // Shell scripting extension (the operators itself are generic, they are just necessary to express sh.pxs)
            public const string BinaryDeltaRight = "binaryDeltaRight__";
            public const string UnaryDeltaRightPre = "unaryDeltaRightPre__";
            public const string UnaryDeltaRightPost = "unaryDeltaRightPost__";
            public const string BinaryDeltaLeft = "binaryDeltaLeft__";
            public const string UnaryDeltaLeftPre = "unaryDeltaLeftPre__";
            public const string UnaryDeltaLeftPost = "unaryDeltaLeftPost__";

            public static string GetName(BinaryOperator op) =>
                op switch
                {
                    BinaryOperator.Addition => Addition,
                    BinaryOperator.Subtraction => Subtraction,
                    BinaryOperator.Multiply => Multiplication,
                    BinaryOperator.Division => Division,
                    BinaryOperator.Modulus => Modulus,
                    BinaryOperator.Power => Power,
                    BinaryOperator.BitwiseAnd => BitwiseAnd,
                    BinaryOperator.BitwiseOr => BitwiseOr,
                    BinaryOperator.ExclusiveOr => ExclusiveOr,
                    BinaryOperator.Equality => Equality,
                    BinaryOperator.Inequality => Inequality,
                    BinaryOperator.GreaterThan => GreaterThan,
                    BinaryOperator.GreaterThanOrEqual => GreaterThanOrEqual,
                    BinaryOperator.LessThan => LessThan,
                    BinaryOperator.LessThanOrEqual => LessThanOrEqual,
                    BinaryOperator.DeltaRight => BinaryDeltaRight,
                    BinaryOperator.DeltaLeft => BinaryDeltaLeft,
                    _ => throw new ArgumentOutOfRangeException(nameof(op))
                };

            public static string GetName(UnaryOperator op) =>
                op switch
                {
                    UnaryOperator.UnaryNegation => UnaryNegation,
                    UnaryOperator.OnesComplement => OnesComplement,
                    UnaryOperator.PostIncrement => Increment,
                    UnaryOperator.PreIncrement => Increment,
                    UnaryOperator.PreDecrement => Decrement,
                    UnaryOperator.PostDecrement => Decrement,
                    UnaryOperator.LogicalNot => LogicalNot,
                    UnaryOperator.PreDeltaRight => UnaryDeltaRightPre,
                    UnaryOperator.PostDeltaRight => UnaryDeltaRightPost,
                    UnaryOperator.PreDeltaLeft => UnaryDeltaLeftPre,
                    UnaryOperator.PostDeltaLeft => UnaryDeltaLeftPost,
                    _ => throw new ArgumentOutOfRangeException(nameof(op))
                };
        }
    }
}