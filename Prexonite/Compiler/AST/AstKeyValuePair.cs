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
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast
{
    public class AstKeyValuePair : AstExpr,
                                   IAstHasExpressions, IAstPartiallyApplicable
    {
        public AstKeyValuePair(string file, int line, int column)
            : this(file, line, column, null, null)
        {
        }

        public AstKeyValuePair(
            string file, int line, int column, AstExpr key, AstExpr value)
            : base(file, line, column)
        {
            Key = key;
            Value = value;
        }

        internal AstKeyValuePair(Parser p)
            : this(p, null, null)
        {
        }

        internal AstKeyValuePair(Parser p, AstExpr key, AstExpr value)
            : base(p)
        {
            Key = key;
            Value = value;
        }

        public AstExpr Key;
        public AstExpr Value;

        #region IAstHasExpressions Members

        public AstExpr[] Expressions => new[] {Key, Value};

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (Key == null)
                throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
            if (Value == null)
                throw new ArgumentNullException(nameof(target));

            if (Key.IsArgumentSplice())
            {
                AstArgumentSplice.ReportNotSupported(Key, target, stackSemantics);
            }

            if (Value.IsArgumentSplice())
            {
                AstArgumentSplice.ReportNotSupported(Value, target, stackSemantics);
            }

            var call = target.Factory.Call(Position, EntityRef.Command.Create(Engine.PairAlias));
            call.Arguments.Add(Key);
            call.Arguments.Add(Value);
            call.EmitCode(target, stackSemantics);
        }

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            if (Key == null)
                throw new PrexoniteException("AstKeyValuePair.Key must be initialized.");
            if (Value == null)
                throw new ArgumentNullException(nameof(target));

            _OptimizeNode(target, ref Key);
            _OptimizeNode(target, ref Value);

            expr = null;

            return false;
        }

        #endregion

        #region Implementation of IAstPartiallyApplicable

        public NodeApplicationState CheckNodeApplicationState()
        {
            return new(
                (Key?.IsPlaceholder() ?? false) || (Value?.IsPlaceholder() ?? false), 
                (Key?.IsArgumentSplice() ?? false) || (Value?.IsArgumentSplice() ?? false));
        }

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {            
            if (Key.IsArgumentSplice())
            {
                AstArgumentSplice.ReportNotSupported(Key, target, StackSemantics.Value);
            }

            if (Value.IsArgumentSplice())
            {
                AstArgumentSplice.ReportNotSupported(Value, target, StackSemantics.Value);
            }
            DoEmitCode(target,StackSemantics.Value);
            //Partial application is handled by AstGetSetSymbol. Code is the same
        }

        #endregion

        public override string ToString()
        {
            var key = Key?.ToString() ?? "-null-";
            var value = Value?.ToString() ?? "-null-";
            return $"Key = ({key}): Value = ({value})";
        }
    }
}