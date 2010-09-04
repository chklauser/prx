using System;
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
        /// <param name="constructorAlias">The alias of the command to use as the partial application constructor.</param>
        /// <param name="arguments">The sequence of arguments, including any non-arguments as a perfix (like the subject of an object member call).</param>
        public static void EmitConstructorCall<T>(this T node, CompilerTarget target, string constructorAlias, IEnumerable<IAstExpression> arguments) where T : AstNode, IAstPartiallyApplicable
        {
            var argv = arguments.ToList();
            AstPlaceholder.DeterminePlaceholderIndices(argv.MapMaybe(n => n as AstPlaceholder));

            //Create mapping
            var mappings8 = new sbyte[argv.Count];
            var closedArguments = new List<IAstExpression>(argv.Count);

            for (var i = 0; i < argv.Count; i++)
            {
                var arg = argv[i];
                AstPlaceholder placeholder;
                if((placeholder = arg as AstPlaceholder) != null)
                {
                    //Insert mapping to open argument
                    System.Diagnostics.Debug.Assert(placeholder.Index != null);
                    mappings8[i] = (sbyte) -(placeholder.Index.Value+1); //mappings are 1-based
                }
                else
                {
                    //Insert mapping to closed argument
                    closedArguments.Add(arg);
                    mappings8[i] = (sbyte) closedArguments.Count; //mappings are 1-based; no need for -1
                }
            }

            var mappings32 = PartialApplicationCommandBase.PackMappings32(mappings8);

            //Emit constructor call
            foreach (var arg in closedArguments)
                arg.EmitCode(target);

            foreach (var mapping in mappings32)
                target.EmitConstant(node, mapping);

            target.EmitCommandCall(node, closedArguments.Count + mappings32.Length, constructorAlias);
        }
    }
}
