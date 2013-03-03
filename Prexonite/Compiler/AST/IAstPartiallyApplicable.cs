// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System;
using System.Collections.Generic;
using System.Linq;
using Prexonite.Commands.Core;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Modular;
using Prexonite.Types;
using Debug = System.Diagnostics.Debug;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    ///     An AST node that represents a language construct that can be used in a partial application.
    /// </summary>
    public interface IAstPartiallyApplicable
    {
        /// <summary>
        ///     Checks the nodes immediate child nodes for instances of <see cref = "AstPlaceholder" />. Implement this member publicly for interoperation with Prexonite compile-time macros.
        /// </summary>
        /// <returns>True if this node has placeholders; false otherwise</returns>
        bool CheckForPlaceholders();

        /// <summary>
        ///     <para>Emits code that performs the partial application.</para>
        ///     <para>Important: The code generator is free to call this method independent of the result of <see
        ///      cref = "CheckForPlaceholders" />.</para>
        ///     <para>For internal use only. Implement this member explicitly.</para>
        /// </summary>
        /// <param name = "target">The compiler target to emit code to.</param>
        void DoEmitPartialApplicationCode(CompilerTarget target);
    }

    /// <summary>
    ///     Provides helper methods for the implementation of <see cref = "IAstPartiallyApplicable.DoEmitPartialApplicationCode" />.
    /// </summary>
    public static class AstPartiallyApplicable
    {
        /// <summary>
        ///     <para>Takes the sequence of argument nodes, including any placeholders and emits byte code for the constructor call for the partial application.</para>
        ///     <para>The arguments must be passed in the order they would be supplied to the call target normally and include any non-arguments (like the object member call as the first "argument")</para>
        ///     <para>This method determines placeholder indices, i.e., it assigns absolute indices to placeholders that don't have an index assigend. This is a destructive update.</para>
        /// </summary>
        /// <typeparam name = "T">The kind of <see cref = "AstNode" /> this method operates on.</typeparam>
        /// <param name = "node">The <see cref = "AstNode" /> this method operates on.</param>
        /// <param name = "target">The compiler target to compile to.</param>
        /// <param name = "argv">Result of <see cref = "PreprocessPartialApplicationArguments" />.</param>
        public static int EmitConstructorArguments<T>(this T node, CompilerTarget target,
            List<AstExpr> argv) where T : AstNode, IAstPartiallyApplicable
        {
            var mappings8 = new int[argv.Count];
            var closedArguments = new List<AstExpr>(argv.Count);
            GetMapping(argv, mappings8, closedArguments);

            var mappings32 = PartialApplicationCommandBase.PackMappings32(mappings8);

            //Emit arguments and mapping
            foreach (var arg in closedArguments)
                arg.EmitValueCode(target);

            foreach (var mapping in mappings32)
                target.EmitConstant(node.Position, mapping);

            return closedArguments.Count + mappings32.Length;
        }

        /// <summary>
        ///     Goes over <paramref name = "arguments" /> and determines the partial application argument mapping. Closed arguments are collected in <paramref
        ///      name = "closedArguments" />.
        /// </summary>
        /// <param name = "arguments">Result of <see cref = "PreprocessPartialApplicationArguments" />.</param>
        /// <param name = "mappings8">The list (or array) to store the mappings in. Must have space for at least <paramref
        ///      name = "arguments" />.Count elements.</param>
        /// <param name = "closedArguments">The list to store the closed arguments in. Must have space for at least <paramref
        ///      name = "arguments" />.Count elements.</param>
        /// <remarks>
        ///     <para>Does not alter <paramref name = "arguments" />.</para>
        ///     <para><paramref name = "mappings8" /> are not yet packed into 32bit integers. 
        ///         Use <see cref = "PartialApplicationCommandBase.PackMappings32" /> to pack the 
        ///         mappings into a more compact format.</para>
        /// </remarks>
        public static void GetMapping(IList<AstExpr> arguments, IList<int> mappings8,
            List<AstExpr> closedArguments)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                var arg = arguments[i];
                AstPlaceholder placeholder;
                if ((placeholder = arg as AstPlaceholder) != null)
                {
                    //Insert mapping to open argument
                    Debug.Assert(placeholder.Index != null);
                    mappings8[i] = -(placeholder.Index.Value + 1); //mappings are 1-based
                }
                else
                {
                    //Insert mapping to closed argument
                    closedArguments.Add(arg);
                    mappings8[i] = closedArguments.Count; //mappings are 1-based; no need for -1
                }
            }
        }

        /// <summary>
        ///     <para>Performs preprocessing on the sequence of arguments for a partial application (including any non-arguments, like call targets).</para>
        ///     <para>You must use this method to prepare an argument list for <see cref = "EmitConstructorArguments{T}" />.</para>
        /// </summary>
        /// <param name = "arguments">Arguments for the partial call (including things like the call subject)</param>
        /// <returns>A preprocessed copy of the supplied arguments, ready for <see cref = "EmitConstructorArguments{T}" />.</returns>
        public static List<AstExpr> PreprocessPartialApplicationArguments(
            IEnumerable<AstExpr> arguments)
        {
            var placeholders = arguments.MapMaybe(n => n as AstPlaceholder).ToList();
            AstPlaceholder.DeterminePlaceholderIndices(placeholders);
            var processedArgv = arguments.ToList();
            _removeRedundantPlaceholders(processedArgv, placeholders);
            return processedArgv;
        }

        /// <summary>
        ///     <para>Removes placeholders that are redundant in the presence of excess arguments mapping from the supplied list.</para>
        ///     <para>It is not usually necessary to call this method separately. It is called as part of <see
        ///      cref = "PreprocessPartialApplicationArguments" />. Use that method instead.</para>
        /// </summary>
        /// <param name = "argv"></param>
        public static void RemoveRedundantPlaceholders(List<AstExpr> argv)
        {
            _removeRedundantPlaceholders(argv, argv.MapMaybe(n => n as AstPlaceholder));
        }

        private static void _removeRedundantPlaceholders(List<AstExpr> argv,
            IEnumerable<AstPlaceholder> placeholders)
        {
            //Placeholders are redundant iff they map an open argument that would be supplied 
            //  by the excess arguments mechanism anyway.
            //
            //      println(x,?0)
            //  Here the mapping of the first argument is redundant because the first excess 
            //  argument would be mapped to that same position anyway. 
            //
            //      println(?1,x,?3,?0,?2)
            //  Here only the last ?2 is redundant.
            //
            //      println(?3,?0,?1,?2,?3)
            //  No redundant placeholders here, because ?3 is mapped before, therefore
            //  it wouldn't be supplied as an excess argument.
            //
            //      println(?3,?0,?1,?2)
            //  When you remove the ?3 at the end, you can reduce the mapping to
            //  println(?3), since all other open arguments are in their "natural" position

            var maxIndex = placeholders.Max(p => p.Index.HasValue ? (int) p.Index : 0);
            var numUsages = new int[maxIndex + 1];
            foreach (var placeholder in placeholders)
            {
                Debug.Assert(placeholder.Index != null);
                numUsages[placeholder.Index.Value]++;
            }

            //If there are open argument indices that weren't mapped, we are dealing with
            //  a deliberate use of excess arguments already and are therefor
            //  not dealing with any redundancy at all
            if (numUsages.Contains(0))
                return;

            //Redundant placeholders only occur in a placeholder-only postfix with strictly 
            //  ascending indices, that have not been mapped before. Find that postfix
            var lastIndex = Int32.MaxValue;
            int postfixIndex;
            for (postfixIndex = argv.Count - 1; postfixIndex >= 0; postfixIndex--)
            {
                var arg = argv[postfixIndex] as AstPlaceholder;
                if (arg == null || lastIndex <= arg.Index)
                    break;
                Debug.Assert(arg.Index != null);
                if (numUsages[arg.Index.Value] > 1)
                    break;
                lastIndex = arg.Index.Value;
            }
            postfixIndex++; //points to the first argument of the postfix
            if (argv.Count <= postfixIndex)
                return; //there is no qualifying postfix

            //We need to know which arguments are mapped by the rest of argv
            //Simulate open arguments mapping for the rest of the mapping
            for (var i = 0; i < postfixIndex; i++)
            {
                var placeholder = argv[i] as AstPlaceholder;
                if (placeholder == null)
                    continue;

                Debug.Assert(placeholder.Index.HasValue);
                numUsages[placeholder.Index.Value]--;
            }

            //Correlate potential excess arguments with placeholders from the end
            int redundantIndex;
            var excessIndex = numUsages.Length - 1;
            for (redundantIndex = argv.Count - 1; postfixIndex <= redundantIndex; redundantIndex--)
            {
                var placeholder = (AstPlaceholder) argv[redundantIndex];
                Debug.Assert(placeholder.Index != null);

                //find next potential excess argument
                for (; 0 <= excessIndex; excessIndex--)
                    if (numUsages[excessIndex] == 1)
                        break;

                if (placeholder.Index != excessIndex--)
                    break;
            }
            redundantIndex++;

            //Remove redundant placeholders
            argv.RemoveRange(redundantIndex, argv.Count - redundantIndex);
        }

        public static AstExpr ConstFunc(this AstExpr expr)
        {
            var constCmd = new AstIndirectCall(expr.Position,PCall.Get, new AstReference(expr.Position,EntityRef.Command.Create(Const.Alias)));
            constCmd.Arguments.Add(expr);
            return constCmd;
        }

        public static AstExpr ConstFunc(this AstExpr expr, object constantValue)
        {
            return expr.Position.ConstFunc(constantValue);
        }

        public static AstExpr ConstFunc(this ISourcePosition position, object constantValue)
        {
            if (constantValue != null)
                return
                    new AstConstant(position.File, position.Line, position.Column, constantValue).
                        ConstFunc();
            else
                return new AstNull(position.File, position.Line, position.Column).ConstFunc();
        }

        public static AstExpr IdFunc(this ISourcePosition node)
        {
            return IdFunc(new AstPlaceholder(node.File, node.Line, node.Column, 0));
        }

        public static AstExpr IdFunc(this AstPlaceholder placeholder)
        {
            if (!placeholder.Index.HasValue)
                throw new ArgumentException("Placeholder must have its index assigned.",
                    "placeholder");

            var call = new AstIndirectCall(placeholder.Position, PCall.Get,
                                           new AstReference(placeholder.Position, EntityRef.Command.Create(Id.Alias)));
            call.Arguments.Add(placeholder.GetCopy());
            return call;
        }

        public static bool IsPlaceholder(this AstExpr expression)
        {
            return expression is AstPlaceholder;
        }
    }
}