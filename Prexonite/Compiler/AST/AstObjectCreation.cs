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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstObjectCreation : AstExpr,
                                     IAstHasExpressions,
                                     IAstPartiallyApplicable
    {
        public ArgumentsProxy Arguments { get; }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions => Arguments.ToArray();

        public AstTypeExpr TypeExpr { get; set; }

        #endregion

        private readonly List<AstExpr> _arguments = new List<AstExpr>();

        [DebuggerStepThrough]
        public AstObjectCreation(string file, int line, int col, AstTypeExpr type)
            : base(file, line, col)
        {
            TypeExpr = type ?? throw new ArgumentNullException(nameof(type));
            Arguments = new ArgumentsProxy(_arguments);
        }

        [DebuggerStepThrough]
        internal AstObjectCreation(Parser p, AstTypeExpr type)
            : this(p.scanner.File, p.t.line, p.t.col, type)
        {
        }

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;

            TypeExpr = (AstTypeExpr) _GetOptimizedNode(target, TypeExpr);

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

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                foreach (var arg in _arguments)
                    arg.EmitValueCode(target);
                target.Emit(Position,OpCode.newobj, _arguments.Count, constType.TypeExpression);
                if(stackSemantics == StackSemantics.Effect)
                    target.Emit(Position,Instruction.CreatePop());
            }
            else
            {
                //Load type and call construct on it
                TypeExpr.EmitValueCode(target);
                foreach (var arg in _arguments)
                    arg.EmitValueCode(target);
                var justEffect = stackSemantics == StackSemantics.Effect;
                target.EmitGetCall(Position, _arguments.Count, PType.ConstructFromStackId, justEffect);
            }
        }

        #endregion

        #region Implementation of IAstPartiallyApplicable

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() ||
                Arguments.Any(AstPartiallyApplicable.IsPlaceholder);
        }

        public NodeApplicationState CheckNodeApplicationState()
        {
            return new NodeApplicationState(TypeExpr.IsPlaceholder() || Arguments.Any(x => x.IsPlaceholder()),
                TypeExpr.IsArgumentSplice() || Arguments.Any(x => x.IsArgumentSplice()));
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(Arguments.ToList());
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            var constType = TypeExpr as AstConstantTypeExpression;
            if (constType != null)
                target.EmitConstant(Position, constType.TypeExpression);
            else
                TypeExpr.EmitValueCode(target);
            target.EmitCommandCall(Position, ctorArgc + 1, Engine.PartialConstructionAlias);
        }

        #endregion
    }
}