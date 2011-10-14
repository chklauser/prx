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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstObjectCreation : AstNode,
                                     IAstExpression,
                                     IAstHasExpressions,
                                     IAstPartiallyApplicable
    {
        private IAstType _typeExpr;
        private readonly ArgumentsProxy _proxy;

        public ArgumentsProxy Arguments
        {
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        public IAstType TypeExpr
        {
            get { return _typeExpr; }
            set { _typeExpr = value; }
        }

        #endregion

        private readonly List<IAstExpression> _arguments = new List<IAstExpression>();

        [DebuggerStepThrough]
        public AstObjectCreation(string file, int line, int col, IAstType type)
            : base(file, line, col)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            _typeExpr = type;
            _proxy = new ArgumentsProxy(_arguments);
        }

        [DebuggerStepThrough]
        internal AstObjectCreation(Parser p, IAstType type)
            : this(p.scanner.File, p.t.line, p.t.col, type)
        {
        }

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            _typeExpr = (IAstType) _GetOptimizedNode(target, _typeExpr);

            //Optimize arguments
            for (var i = 0; i < _arguments.Count; i++)
            {
                var arg = _arguments[i];
                var oArg = _GetOptimizedNode(target, arg);
                if (ReferenceEquals(oArg, arg))
                    continue;
                _arguments[i] = oArg;
            }

            return false;
        }

        protected override void DoEmitCode(CompilerTarget target)
        {
            var constType = _typeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                foreach (var arg in _arguments)
                    arg.EmitCode(target);
                target.Emit(this, OpCode.newobj, _arguments.Count, constType.TypeExpression);
            }
            else
            {
                //Load type and call construct on it
                _typeExpr.EmitCode(target);
                foreach (var arg in _arguments)
                    arg.EmitCode(target);
                target.EmitGetCall(this, _arguments.Count, PType.ConstructFromStackId);
            }
        }

        #endregion

        #region Implementation of IAstPartiallyApplicable

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() ||
                Arguments.Any(AstPartiallyApplicable.IsPlaceholder);
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(Arguments.ToList());
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constType = _typeExpr as AstConstantTypeExpression;
            if (constType != null)
                target.EmitConstant(this, constType.TypeExpression);
            else
                _typeExpr.EmitCode(target);
            target.EmitCommandCall(this, ctorArgc + 1, Engine.PartialConstructionAlias);
        }

        #endregion
    }
}