﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Commands.Core.PartialApplication;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// An AST node that represents a language construct that can be used in a partial application.
    /// </summary>
    public interface IAstPartiallyApplicable
    {
        /// <summary>
        /// Checks the nodes immediate child nodes for instances of <see cref="AstPlaceholder"/>. Implement this member publicly for interoperation with Prexonite compile-time macros.
        /// </summary>
        /// <returns>True if this node has placeholders; false otherwise</returns>
        bool CheckForPlaceholders();

        /// <summary>
        /// <para>Emits code that performs the partial application.</para>
        /// <para>Important: The code generator is free to call this method independent of the result of <see cref="CheckForPlaceholders"/>.</para>
        /// <para>For internal use only. Implement this member explicitly.</para>
        /// </summary>
        /// <param name="target">The compiler target to emit code to.</param>
        void DoEmitPartialApplicationCode(CompilerTarget target);
    }

    /// <summary>
    /// Provides helper methods for the implementation of <see cref="IAstPartiallyApplicable.DoEmitPartialApplicationCode"/>.
    /// </summary>
    public static class AstPartiallyApplicable
    {
        /// <summary>
        /// <para>Takes the sequence of argument nodes, including any placeholders and emits byte code for the constructor call for the partial application.</para>
        /// <para>The arguments must be passed in the order they would be supplied to the call target normally and include any non-arguments (like the object member call as the first "argument")</para>
        /// <para>This method determines placeholder indices, i.e., it assigns absolute indices to placeholders that don't have an index assigend. This is a destructive update.</para>
        /// </summary>
        /// <typeparam name="T">The kind of <see cref="AstNode"/> this method operates on.</typeparam>
        /// <param name="node">The <see cref="AstNode"/> this method operates on.</param>
        /// <param name="target">The compiler target to compile to.</param>
        /// <param name="argv">Result of <see cref="PreprocessPartialApplicationArguments"/>.</param>
        public static int EmitConstructorArguments<T>(this T node, CompilerTarget target, List<IAstExpression> argv) where T : AstNode, IAstPartiallyApplicable
        {
            var mappings8 = new int[argv.Count];
            var closedArguments = new List<IAstExpression>(argv.Count);

            for (var i = 0; i < argv.Count; i++)
            {
                var arg = argv[i];
                AstPlaceholder placeholder;
                if((placeholder = arg as AstPlaceholder) != null)
                {
                    //Insert mapping to open argument
                    System.Diagnostics.Debug.Assert(placeholder.Index != null);
                    mappings8[i] = -(placeholder.Index.Value+1); //mappings are 1-based
                }
                else
                {
                    //Insert mapping to closed argument
                    closedArguments.Add(arg);
                    mappings8[i] = closedArguments.Count; //mappings are 1-based; no need for -1
                }
            }

            var mappings32 = PartialApplicationCommandBase.PackMappings32(mappings8);

            //Emit constructor call
            foreach (var arg in closedArguments)
                arg.EmitCode(target);

            foreach (var mapping in mappings32)
                target.EmitConstant(node, mapping);

            return closedArguments.Count + mappings32.Length;
        }

        /// <summary>
        /// <para>Performs preprocessing on the sequence of arguments for a partial application (including any non-arguments, like call targets).</para>
        /// <para>You must use this method to prepare an argument list for <see cref="EmitConstructorArguments{T}"/>.</para>
        /// </summary>
        /// <param name="arguments">Arguments for the partial call (including things like the call subject)</param>
        /// <returns>A preprocessed copy of the supplied arguments, ready for <see cref="EmitConstructorArguments{T}"/>.</returns>
        public static List<IAstExpression> PreprocessPartialApplicationArguments(IEnumerable<IAstExpression> arguments)
        {
            var placeholders = arguments.MapMaybe(n => n as AstPlaceholder).ToList();
            AstPlaceholder.DeterminePlaceholderIndices(placeholders);
            var processedArgv = arguments.ToList();
            _removeRedundantPlaceholders(processedArgv, placeholders);
            return processedArgv;
        }

        /// <summary>
        /// <para>Removes placeholders that are redundant in the presence of excess arguments mapping from the supplied list.</para>
        /// <para>It is not usually necessary to call this method separately. It is called as part of <see cref="PreprocessPartialApplicationArguments"/>. Use that method instead.</para>
        /// </summary>
        /// <param name="argv"></param>
        public static void RemoveRedundantPlaceholders(List<IAstExpression> argv)
        {
            _removeRedundantPlaceholders(argv, argv.MapMaybe(n => n as AstPlaceholder));
        }

        private static void _removeRedundantPlaceholders(List<IAstExpression> argv, IEnumerable<AstPlaceholder> placeholders)
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

            var maxIndex = placeholders.Max(p => p.Index.HasValue ? (int)p.Index : 0);
            var numUsages = new int[maxIndex + 1];
            foreach (var placeholder in placeholders)
            {
                System.Diagnostics.Debug.Assert(placeholder.Index != null);
                numUsages[placeholder.Index.Value]++;
            }

            //If there are open argument indices that weren't mapped, we are dealing with
            //  a deliberate use of excess arguments already and are therefor
            //  not dealing with any redundancy at all
            foreach (var usage in numUsages)
                if (usage == 0)
                    return;

            //Redundant placeholders only occur in a placeholder-only postfix with strictly 
            //  ascending indices, that have not been mapped before. Find that postfix
            var lastIndex = Int32.MaxValue;
            int postfixIndex;
            for(postfixIndex = argv.Count-1; postfixIndex >= 0; postfixIndex--)
            {
                var arg = argv[postfixIndex] as AstPlaceholder;
                if(arg == null || lastIndex <= arg.Index)
                    break;
                System.Diagnostics.Debug.Assert(arg.Index != null);
                if( numUsages[arg.Index.Value] > 1)
                    break;
                lastIndex = arg.Index.Value;
            }
            postfixIndex++; //points to the first argument of the postfix
            if(argv.Count <= postfixIndex)
                return; //there is no qualifying postfix

            //We need to know which arguments are mapped by the rest of argv
            //Simulate open arguments mapping for the rest of the mapping
            for (var i = 0; i < postfixIndex; i++)
            {
                var placeholder = argv[i] as AstPlaceholder;
                if(placeholder == null)
                    continue;

                System.Diagnostics.Debug.Assert(placeholder.Index != null);
                numUsages[placeholder.Index.Value]--;
            }

            //Correlate potential excess arguments with placeholders from the end
            int redundantIndex;
            var excessIndex = numUsages.Length - 1;
            for(redundantIndex = argv.Count - 1; postfixIndex <= redundantIndex; redundantIndex--)
            {
                var placeholder = (AstPlaceholder) argv[redundantIndex];
                System.Diagnostics.Debug.Assert(placeholder.Index != null);

                //find next potential excess argument
                for(;0 <= excessIndex; excessIndex--)
                    if(numUsages[excessIndex] == 1)
                        break;

                if(placeholder.Index != excessIndex--)
                    break;
            }
            redundantIndex++;

            //Remove redundant placeholders
            argv.RemoveRange(redundantIndex, argv.Count - redundantIndex);
        }
    }
}