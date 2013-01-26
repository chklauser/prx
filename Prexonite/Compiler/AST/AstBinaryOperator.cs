// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Prexonite.Compiler.Ast
{
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
        Cast
    }
}