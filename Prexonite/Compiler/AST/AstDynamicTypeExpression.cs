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

using System.Text;

namespace Prexonite.Compiler.Ast;

public class AstDynamicTypeExpression : AstTypeExpr,
    IAstHasExpressions
{
    public List<AstExpr> Arguments { get; } = new();
    public string TypeId { get; }

    public AstDynamicTypeExpression(string file, int line, int column, string typeId)
        : this(new SourcePosition(file, line, column), typeId)
    {
    }

    public AstDynamicTypeExpression(ISourcePosition position, string typeId)
        :base(position)
    {
        TypeId = typeId ?? throw new ArgumentNullException(nameof(typeId));
    }

    internal AstDynamicTypeExpression(Parser p, string typeId)
        : this(p.GetPosition(), typeId)
    {
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions => Arguments.ToArray();

    #endregion

    #region AstExpr Members

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        expr = null;

        var isConstant = true;
        var buffer = new StringBuilder(TypeId);
        buffer.Append("(");

        //Optimize arguments
        AstExpr oArg;
        foreach (var arg in Arguments.ToArray())
        {
            oArg = _GetOptimizedNode(target, arg);
            if (!ReferenceEquals(oArg, arg))
            {
                Arguments.Remove(arg);
                Arguments.Add(oArg);
            }

            var constValue = oArg as AstConstant;
            var constType = oArg as AstConstantTypeExpression;

            if (constValue == null && constType == null)
            {
                isConstant = false;
            }
            else if (isConstant)
            {
                if (constValue != null)
                {
                    buffer.Append('"');
                    buffer.Append(
                        StringPType.Escape(
                            constValue.ToPValue(target).CallToString(target.Loader)));
                    buffer.Append('"');
                }
                else if (constType != null)
                {
                    buffer.Append(constType.TypeExpression);
                }

                buffer.Append(",");
            }
        }
        if (!isConstant)
            return false;

        buffer.Remove(buffer.Length - 1, 1); //remove , or (
        if (Arguments.Count != 0)
            buffer.Append(")"); //Add ) if necessary

        expr = new AstConstantTypeExpression(File, Line, Column, buffer.ToString());
        return true;
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        foreach (var expr in Arguments)
            expr.EmitCode(target,stackSemantics);

        if(stackSemantics == StackSemantics.Value)
            target.Emit(Position,OpCode.newtype, Arguments.Count, TypeId);
    }

    #endregion
}