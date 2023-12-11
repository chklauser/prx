
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
namespace Prexonite.Compiler.Ast;

public class AstThrow : AstExpr,
    IAstHasExpressions
{
    public required AstExpr Expression;

    public AstThrow(string file, int line, int column)
        : base(file, line, column)
    {
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return new[] {Expression}; }
    }

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;
        return false;
    }

    #endregion


    public override string ToString()
    {
        return "throw " + (Expression.ToString() ?? "");
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if (Expression == null)
            throw new PrexoniteException("Expression must be assigned.");

        Expression.EmitValueCode(target);
        target.Emit(Position,OpCode.@throw);

        if (stackSemantics == StackSemantics.Value)
            target.Emit(Position,OpCode.ldc_null);
    }
}