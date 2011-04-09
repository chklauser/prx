/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

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
            var isPartialApplication = partiallyApplicabale != null && partiallyApplicabale.CheckForPlaceholders();

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
        /// Checks the nodes immediate child nodes for instances of <see cref="AstPlaceholder"/>. Must yield the same result as <see cref="IAstPartiallyApplicable.CheckForPlaceholders"/>, if implemented in derived types.
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

        public virtual bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;

            switch (id.ToUpperInvariant())
            {
                case "GETOPTIMIZEDNODE":
                    CompilerTarget target;
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException("_GetOptimizedNode(CompilerTarget target) requires target.");
                    var expr = this as IAstExpression;
                    if (expr == null)
                        throw new PrexoniteException("The node is not an IAstExpression.");

                    result = target.Loader.CreateNativePValue(_GetOptimizedNode(target, expr));
                    break;
                case "EMITEFFECTCODE":
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException("_GetOptimizedNode(CompilerTarget target) requires target.");
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

        internal static SymbolInterpretations Resolve(Parser parser, string symbolicId, out string physicalId)
        {
            SymbolInterpretations interpretation;
            SymbolEntry symbolEntry;
            if(!parser.target.Symbols.TryGetValue(symbolicId,out symbolEntry))
            {
                physicalId = symbolicId;
                interpretation = SymbolInterpretations.Command;
                parser.SemErr(string.Format("No implementation defined for operator `{0}`",
                                            physicalId));
            }
            else
            {
                interpretation = symbolEntry.Interpretation;
                physicalId = symbolEntry.Id;
            }
            return interpretation;
        }
    }
}