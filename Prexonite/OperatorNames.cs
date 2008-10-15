using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Ast;

namespace Prexonite
{
    /// <summary>
    /// Provides constants for the names of operators (binar, unary).
    /// </summary>
    public static class OperatorNames
    {

        /// <summary>
        /// Provides constants for the names of operators in Prexonite
        /// </summary>
        public static class Prexonite
        {

            public const string Addition = "plus";
            public const string Subtraction = "minus";
            public const string Multiplication = "times";
            public const string Division = "dividedBy";
            public const string Modulus = "remainder";
            public const string Power = "raisedTo";
            public const string BitwiseAnd = "bitwiseAnd";
            public const string BitwiseOr = "bitwiseOr";
            public const string ExclusiveOr = "exclusiveOr";
            public const string Equality = "isEqualTo";
            public const string Inequality = "isInequalTo";
            public const string GreaterThan = "isGreaterThan";
            public const string GreaterThanOrEqual = "isGreaterThanOrEqual";
            public const string LessThan = "isLessThan";
            public const string LessThanOrEqual = "isLessThanOrEqual";

            public const string UnaryNegation = "negation";
            public const string OnesComplement = "complement";

            public const string Increment = "increment";
            public const string Decrement = "decrement";

            public static string GetName(BinaryOperator op)
            {
                switch (op)
                {
                    case BinaryOperator.Addition:
                        return Addition;
                    case BinaryOperator.Subtraction:
                        return Subtraction;
                    case BinaryOperator.Multiply:
                        return Multiplication;
                    case BinaryOperator.Division:
                        return Division;
                    case BinaryOperator.Modulus:
                        return Modulus;
                    case BinaryOperator.Power:
                        return Power;
                    case BinaryOperator.BitwiseAnd:
                        return BitwiseAnd;
                    case BinaryOperator.BitwiseOr:
                        return BitwiseOr;
                    case BinaryOperator.ExclusiveOr:
                        return ExclusiveOr;
                    case BinaryOperator.Equality:
                        return Equality;
                    case BinaryOperator.Inequality:
                        return Inequality;
                    case BinaryOperator.GreaterThan:
                        return GreaterThan;
                    case BinaryOperator.GreaterThanOrEqual:
                        return GreaterThanOrEqual;
                    case BinaryOperator.LessThan:
                        return LessThan;
                    case BinaryOperator.LessThanOrEqual:
                        return LessThanOrEqual;
                    default:
                        return null;
                }
            }

            public static string GetName(UnaryOperator op)
            {
                switch (op)
                {
                    case UnaryOperator.UnaryNegation:
                        return UnaryNegation;
                    case UnaryOperator.OnesComplement:
                        return OnesComplement;
                    case UnaryOperator.PostIncrement:
                    case UnaryOperator.PreIncrement:
                        return Increment;
                    case UnaryOperator.PreDecrement:
                    case UnaryOperator.PostDecrement:
                        return Decrement;
                    default:
                        throw new ArgumentOutOfRangeException("op");
                }
            }
        }

    }
}