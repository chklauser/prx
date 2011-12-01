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

using System.Collections.Generic;
using System.Linq;
using Prexonite.Commands.Core;
using Debug = System.Diagnostics.Debug;

namespace Prexonite.Compiler.Ast
{
    public class AstCoalescence : AstExpr,
                                  IAstHasExpressions,
                                  IAstPartiallyApplicable
    {
        public AstCoalescence(string file, int line, int column)
            : base(file, line, column)
        {
        }

        internal AstCoalescence(Parser p)
            : base(p)
        {
        }

        private readonly List<AstExpr> _expressions = new List<AstExpr>(2);

        #region IAstHasExpressions Members

        AstExpr[] IAstHasExpressions.Expressions
        {
            get { return _expressions.ToArray(); }
        }

        public List<AstExpr> Expressions
        {
            get { return _expressions; }
        }

        #endregion

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;

            //Optimize arguments
            for (var i = 0; i < _expressions.Count; i++)
            {
                var arg = _expressions[i];
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                            ") detected at position " + _expressions.IndexOf(arg) + ".");
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                    _expressions[i] = oArg;
            }

            var nonNullExpressions = _expressions.Where(_exprIsNotNull).ToArray();
            _expressions.Clear();
            _expressions.AddRange(nonNullExpressions);

            if (_expressions.Count == 1)
            {
                var pExpr = _expressions[0];
                expr = pExpr is AstPlaceholder ? ((AstPlaceholder) pExpr).IdFunc() : pExpr;
                return true;
            }
            else if (_expressions.Count == 0)
            {
                expr = new AstNull(File, Line, Column);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool _exprIsNotNull(AstExpr iexpr)
        {
            return !(iexpr is AstNull ||
                (iexpr is AstConstant && ((AstConstant) iexpr).Constant == null));
        }

        #endregion

        private static int _count = -1;
        private static readonly object _labelCountLock = new object();

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            //Expressions contains at least two expressions
            var endLabel = _generateEndLabel();
            _emitCode(target, endLabel, stackSemantics);
            target.EmitLabel(this, endLabel);
        }

        private void _emitCode(CompilerTarget target, string endLabel, StackSemantics stackSemantics)
        {
            for (var i = 0; i < _expressions.Count; i++)
            {
                var expr = _expressions[i];

                // Value semantics: duplicate of previous, rejected value needs to be popped
                // Effect semantics: no duplicates were created in the first place
                if (i > 0 && stackSemantics == StackSemantics.Value)
                    target.EmitPop(this);

                //For value semantics, we always generate a value
                //For effect semantics, we only need the intermediate expressions to create a value
                StackSemantics needValue;
                if (stackSemantics == StackSemantics.Value || i < _expressions.Count - 1)
                    needValue = StackSemantics.Value;
                else 
                    needValue = StackSemantics.Effect;

                expr.EmitCode(target, needValue);

                //The last element doesn't need special handling, control just 
                //  falls into the surrounding code with the correct value on top of the stack
                if (i + 1 >= _expressions.Count)
                    continue;

                if(stackSemantics == StackSemantics.Value)
                    target.EmitDuplicate(this);
                target.Emit(this, OpCode.check_null);
                target.EmitJumpIfFalse(this, endLabel);
            }
        }

        private static string _generateEndLabel()
        {
            lock (_labelCountLock)
            {
                _count++;
                return "coal\\n" + _count + "\\end";
            }
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() ||
                Expressions.Any(AstPartiallyApplicable.IsPlaceholder);
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            AstPlaceholder.DeterminePlaceholderIndices(Expressions.OfType<AstPlaceholder>());

            var count = Expressions.Count;
            if (count == 0)
            {
                this.ConstFunc(null).EmitValueCode(target);
                return;
            }

            //only the very last condition may be a placeholder
            for (var i = 0; i < count; i++)
            {
                var value = Expressions[i];
                var isPlaceholder = value.IsPlaceholder();
                if (i == count - 1)
                {
                    if (!isPlaceholder)
                    {
                        //there is no placeholder at all, wrap expression in const
                        Debug.Assert(Expressions.All(e => !e.IsPlaceholder()));
                        DoEmitCode(target,StackSemantics.Value);
                        target.EmitCommandCall(this, 1, Const.Alias);
                        return;
                    }
                }
                else
                {
                    if (isPlaceholder)
                    {
                        _reportInvalidPlaceholders(target);
                        return;
                    }
                }
            }

            if (count == 0)
            {
                this.ConstFunc(null).EmitValueCode(target);
            }
            else if (count == 1)
            {
                Debug.Assert(Expressions[0].IsPlaceholder(),
                    "Singleton ??-chain expected to consist of placeholder.");
                var placeholder = (AstPlaceholder) Expressions[0];
                placeholder.IdFunc().EmitValueCode(target);
            }
            else
            {
                Debug.Assert(Expressions[count - 1].IsPlaceholder(),
                    "Last expression in ??-chain expected to be placeholder.");
                var placeholder = (AstPlaceholder) Expressions[count - 1];
                var prefix = new AstCoalescence(File, Line, Column);
                prefix.Expressions.AddRange(Expressions.Take(count - 1));

                //check for null (keep a copy of prefix on stack)
                var constLabel = _generateEndLabel();
                var endLabel = _generateEndLabel();
                prefix._emitCode(target, constLabel, StackSemantics.Value);
                target.EmitDuplicate(this);
                target.Emit(this, OpCode.check_null);
                target.EmitJumpIfFalse(this, constLabel);
                //prefix is null, identity function
                target.EmitPop(this);
                placeholder.IdFunc().EmitValueCode(target);
                target.EmitJump(this, endLabel);
                //prefix is not null, const function
                target.EmitLabel(this, constLabel);
                target.EmitCommandCall(this, 1, Const.Alias);
                target.EmitLabel(this, endLabel);
            }
        }

        private void _reportInvalidPlaceholders(CompilerTarget target)
        {
            target.Loader.ReportSemanticError(Line, Column,
                "In partial applications of lazy coalescence expressions, only one placeholder at the end of a sequence is allowed. Consider using a lambda expression instead.");
        }
    }
}