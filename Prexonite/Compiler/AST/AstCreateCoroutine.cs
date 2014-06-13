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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Prexonite.Compiler.Ast
{
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Coroutine")]
    public class AstCreateCoroutine : AstExpr,
                                      IAstHasExpressions
    {
        private AstExpr _expression;

        private ArgumentsProxy _proxy;

        public ArgumentsProxy Arguments
        {
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        public AstExpr Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        #endregion

        private List<AstExpr> _arguments = new List<AstExpr>();

        public AstCreateCoroutine(string file, int line, int col)
            : base(file, line, col)
        {
            _proxy = new ArgumentsProxy(_arguments);
        }

        internal AstCreateCoroutine(Parser p)
            : base(p)
        {
            _proxy = new ArgumentsProxy(_arguments);
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(stackSemantics == StackSemantics.Effect)
                return;

            if (Expression == null)
                throw new PrexoniteException("CreateCoroutine node requires an Expression.");

            Expression.EmitValueCode(target);
            foreach (var argument in _arguments)
                argument.EmitValueCode(target);

            target.Emit(Position,OpCode.newcor, _arguments.Count);
        }

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            Expression = _GetOptimizedNode(target, Expression);

            //Optimize arguments
            foreach (var arg in _arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in CreateCoroutine node (" + ToString() +
                            ") detected at position " + _arguments.IndexOf(arg) + ".");
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    var idx = _arguments.IndexOf(arg);
                    _arguments.Insert(idx, oArg);
                    _arguments.RemoveAt(idx + 1);
                }
            }
            expr = null;
            return false;
        }

        #endregion
    }
}