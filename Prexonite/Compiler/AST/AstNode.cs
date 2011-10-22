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

using System;
using System.ComponentModel;
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    [DebuggerStepThrough]
    public abstract class AstNode : IObject, ISourcePosition
    {
        private readonly string _file;
        private readonly int _line;
        private readonly int _column;

        protected AstNode(string file, int line, int column)
        {
            _file = file ?? "unknown~";
            _line = line;
            _column = column;
        }

        internal AstNode(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        public string File
        {
            get { return _file; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int Column
        {
            get { return _column; }
        }

        public void EmitCode(CompilerTarget target)
        {
            _dispatchDoEmitCode(target, false);
        }

        protected abstract void DoEmitCode(CompilerTarget target);

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void _EmitEffectCode(CompilerTarget target)
        {
            _dispatchDoEmitCode(target, true);
        }

        private void _dispatchDoEmitCode(CompilerTarget target, bool justEffectCode)
        {
            var effect = this as IAstEffect;
            var partiallyApplicabale = this as IAstPartiallyApplicable;
            var isPartialApplication = partiallyApplicabale != null &&
                partiallyApplicabale.CheckForPlaceholders();

            if (justEffectCode && effect != null)
            {
                if (isPartialApplication)
                {
                    //A partial application does not have an effect.
                }
                else
                {
                    effect.DoEmitEffectCode(target);
                }
            }
            else
            {
                if (isPartialApplication)
                {
                    partiallyApplicabale.DoEmitPartialApplicationCode(target);
                }
                else
                {
                    DoEmitCode(target);
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
            return false;
        }

        internal static IAstExpression _GetOptimizedNode(CompilerTarget target, IAstExpression expr)
        {
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target cannot be null.");
            if (expr == null)
                throw new ArgumentNullException(
                    "expr", "Expression to be optimized can not be null.");
            IAstExpression opt;
            return expr.TryOptimize(target, out opt) ? opt : expr;
        }

        internal static void _OptimizeNode(CompilerTarget target, ref IAstExpression expr)
        {
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target cannot be null.");
            if (expr == null)
                throw new ArgumentNullException(
                    "expr", "Expression to be optimized can not be null.");
            expr = _GetOptimizedNode(target, expr);
        }

        #region Implementation of IObject

        public virtual bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
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
                    var expr = this as IAstExpression;
                    if (expr == null)
                        throw new PrexoniteException("The node is not an IAstExpression.");

                    result = target.Loader.CreateNativePValue(_GetOptimizedNode(target, expr));
                    break;
                case "EMITEFFECTCODE":
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException(
                            "_GetOptimizedNode(CompilerTarget target) requires target.");
                    var effect = this as IAstEffect;
                    if (effect == null)
                        throw new PrexoniteException("The node is not an IAstExpression.");
                    effect.EmitEffectCode(target);
                    result = PType.Null;
                    break;
            }

            return result != null;
        }

        #endregion

        internal static SymbolEntry Resolve(Parser parser, string symbolicId)
        {
            SymbolEntry symbolEntry;
            if (!parser.target.Symbols.TryGetValue(symbolicId, out symbolEntry))
            {
                parser.SemErr(string.Format("No implementation defined for operator `{0}`",
                    symbolEntry));
                return new SymbolEntry(SymbolInterpretations.Command, symbolicId, null);
            }
            else
            {
                return symbolEntry;
            }
        }
    }
}