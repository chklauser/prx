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

namespace Prexonite.Compiler.Ast
{
    public class AstHashLiteral : AstExpr,
                                  IAstHasExpressions
    {
        public List<AstExpr> Elements = new List<AstExpr>();

        internal AstHashLiteral(Parser p)
            : base(p)
        {
        }

        public AstHashLiteral(string file, int line, int column)
            : base(file, line, column)
        {
        }

        #region IAstHasExpressions Members

        public AstExpr[] Expressions
        {
            get { return Elements.ToArray(); }
        }

        #endregion

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            AstExpr oArg;
            foreach (var arg in Elements.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in HashLiteral node (" + ToString() +
                            ") detected at position " + Elements.IndexOf(arg) + ".");
                oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    var idx = Elements.IndexOf(arg);
                    Elements.Insert(idx, oArg);
                    Elements.RemoveAt(idx + 1);
                }
            }
            expr = null;
            return false;
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (Elements.Count == 0)
            {
                target.Emit(Position,OpCode.newobj, 0, "Hash");
            }
            else
            {
                foreach (var element in Elements)
                {
                    if (element is AstConstant)
                        throw new PrexoniteException(
                            String.Concat(
                                "Hashes are built from key-value pairs, not constants like ",
                                element,
                                ". [File: ",
                                File,
                                ", Line: ",
                                Line,
                                "]"));
                    element.EmitCode(target,stackSemantics);
                }

                if(stackSemantics == StackSemantics.Effect)
                    return;

                target.EmitStaticGetCall(Position, Elements.Count, "Hash", "Create", false);
            }
        }
    }
}