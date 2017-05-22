// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstNode : IObject
    {
        [NotNull] private readonly ISourcePosition _position;

        protected AstNode(string file, int line, int column)
            : this(new SourcePosition(file, line, column))
        {
        }

        protected AstNode([NotNull] ISourcePosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));
            _position = position;
        }

        internal AstNode(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        [NotNull]
        public ISourcePosition Position
        {
            get { return _position; }
        }

        public string File
        {
            get { return _position.File; }
        }

        public int Line
        {
            get { return _position.Line; }
        }

        public int Column
        {
            get { return _position.Column; }
        }

        protected abstract void DoEmitCode([NotNull] CompilerTarget target, StackSemantics semantics);

        public void EmitValueCode([NotNull] CompilerTarget target)
        {
            EmitCode(target, StackSemantics.Value);
        }

        public void EmitEffectCode([NotNull] CompilerTarget target)
        {
            EmitCode(target, StackSemantics.Effect);
        }

        public void EmitCode([NotNull] CompilerTarget target, StackSemantics justEffectCode)
        {
            var partiallyApplicabale = this as IAstPartiallyApplicable;
            var applicationState = partiallyApplicabale?.CheckNodeApplicationState() ?? default(NodeApplicationState);

            if (justEffectCode == StackSemantics.Effect)
            {
                if (applicationState.HasPlaceholders)
                {
                    //A partial application does not have an effect.
                }
                else
                {
                    DoEmitCode(target, StackSemantics.Effect);
                }
            }
            else
            {
                if (applicationState.HasPlaceholders)
                {
                    Debug.Assert(partiallyApplicabale != null, "partiallyApplicabale != null");
                    partiallyApplicabale.DoEmitPartialApplicationCode(target);
                }
                else
                {
                    DoEmitCode(target, StackSemantics.Value);
                }
            }
        }

        /// <summary>
        ///     Checks the nodes immediate child nodes for instances of <see cref = "AstPlaceholder" />. Must yield the same result as <see
        ///      cref = "IAstPartiallyApplicable.CheckForPlaceholders" />, if implemented in derived types.
        /// </summary>
        /// <returns>True if this node has placeholders; false otherwise</returns>
        public virtual bool CheckForPlaceholders()
        {
            var partiallyApplicable = this as IAstPartiallyApplicable;
            return partiallyApplicable?.CheckNodeApplicationState().HasPlaceholders ?? false;
        }

        [NotNull]
        internal static AstExpr _GetOptimizedNode(
            [NotNull] CompilerTarget target, [NotNull] AstExpr expr)
        {
            if (target == null)
                throw new ArgumentNullException(
                    nameof(target), Resources.AstNode__GetOptimizedNode_CompilerTarget_null);
            if (expr == null)
                throw new ArgumentNullException(
                    nameof(expr), Resources.AstNode__GetOptimizedNode_Expression_null);
            AstExpr opt;
            return expr.TryOptimize(target, out opt) ? opt : expr;
        }

        internal static void _OptimizeNode([NotNull] CompilerTarget target, [NotNull] ref AstExpr expr)
        {
            if (target == null)
                throw new ArgumentNullException(
                    nameof(target), Resources.AstNode__GetOptimizedNode_CompilerTarget_null);
            if (expr == null)
                throw new ArgumentNullException(
                    nameof(expr), Resources.AstNode__GetOptimizedNode_Expression_null);
            expr = _GetOptimizedNode(target, expr);
        }

        #region Implementation of IObject

        public virtual bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id,
            out PValue result)
        {
            result = null;

            switch (id.ToUpperInvariant())
            {
                case "GETOPTIMIZEDNODE":
                    CompilerTarget target;
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException(
                            "_GetOptimizedNode(CompilerTarget target) requires target.");
                    var expr = this as AstExpr;
                    if (expr == null)
                        throw new PrexoniteException("The node is not an AstExpr.");

                    result = target.Loader.CreateNativePValue(_GetOptimizedNode(target, expr));
                    break;
                case "EMITEFFECTCODE":
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException(
                            "EmitEffectCode(CompilerTarget target) requires target.");
                    EmitEffectCode(target);
                    result = PType.Null;
                    break;
            }

            return result != null;
        }

        #endregion

        /// <summary>
        /// Resolves the symbol associated with an operator. If no such operator is defined, an error message
        /// is generated and a default symbol returned. If you need more control, access the 
        /// <see cref="SymbolStore"/> directly.
        /// </summary>
        /// <param name="parser">The parser to post the error message to.</param>
        /// <param name="symbolicId">The symbolic id of the operator (the id used in the source code)</param>
        /// <returns>The symbol corresponding to the symbolic id, or a default symbol when no such symbol entry exists.</returns>
        [NotNull]
        internal static Symbol _ResolveOperator(Parser parser, string symbolicId)
        {
            Symbol symbolEntry;
            if (!parser.target.Symbols.TryGet(symbolicId, out symbolEntry))
            {
                parser.Loader.ReportMessage(
                    Message.Error(
                        string.Format(
                            Resources.AstNode_NoImplementationForOperator,
                            symbolicId), parser.GetPosition(),
                        MessageClasses.SymbolNotResolved));

                return Symbol.CreateCall(EntityRef.Command.Create(symbolicId), NoSourcePosition.Instance);
            }
            else
            {
                return symbolEntry;
            }
        }
    }
}
