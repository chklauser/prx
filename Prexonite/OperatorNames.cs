// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Prexonite.Compiler.Ast;

namespace Prexonite
{
    /// <summary>
    ///     Provides constants for the names of operators (binar, unary).
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
                    case UnaryOperator.LogicalNot:
                        return LogicalNot;
                    default:
                        throw new ArgumentOutOfRangeException("op");
                }
            }
        }
    }
}