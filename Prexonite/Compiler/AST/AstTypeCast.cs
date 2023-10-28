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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

public class AstTypecast : AstExpr,
    IAstHasExpressions,
    IAstPartiallyApplicable
{
    public AstExpr Subject { get; private set; }
    public AstTypeExpr Type { get; private set; }

    public AstTypecast(ISourcePosition position, AstExpr subject, AstTypeExpr type)
        :base(position)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Type = type ?? throw new ArgumentNullException(nameof(type));   
    }

    internal AstTypecast(Parser p, AstExpr subject, AstTypeExpr type)
        : this(p.GetPosition(), subject, type)
    {
    }

    #region IAstHasExpressions Members

    public AstExpr[] Expressions
    {
        get { return new[] {Subject}; }
    }

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Effect)
            return;
            
            
        if (Subject.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Subject, target, stackSemantics);
            return;
        }
        if (Type.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Type, target, stackSemantics);
            return;
        }

        Subject.EmitValueCode(target);
        if (Type is AstConstantTypeExpression constType)
            target.Emit(Position,OpCode.cast_const, constType.TypeExpression);
        else
        {
            Type.EmitValueCode(target);
            target.Emit(Position,OpCode.cast_arg);
        }
    }

    public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
    {
        Subject = _GetOptimizedNode(target, Subject);
        Type = (AstTypeExpr) _GetOptimizedNode(target, Type);

        expr = null;

        if (!(Type is AstConstantTypeExpression constType))
            return false;

        //Constant cast
        if (Subject is AstConstant constSubject)
            return _tryOptimizeConstCast(target, constSubject, constType, out expr);

        //Redundant cast
        AstTypecast castSubject;
        AstConstantTypeExpression sndCastType;
        if ((castSubject = Subject as AstTypecast) != null &&
            (sndCastType = castSubject.Type as AstConstantTypeExpression) != null)
        {
            if (Engine.StringsAreEqual(sndCastType.TypeExpression, constType.TypeExpression))
            {
                //remove the outer cast.
                expr = castSubject;
                return true;
            }
        }

        return false;
    }

    bool _tryOptimizeConstCast(CompilerTarget target, AstConstant constSubject,
        AstConstantTypeExpression constType, out AstExpr expr)
    {
        expr = null;
        PType type;
        try
        {
            type = target.Loader.ConstructPType(constType.TypeExpression);
        }
        catch (PrexoniteException)
        {
            //ignore, cast failed. cannot be optimized
            return false;
        }

        if (constSubject.ToPValue(target).TryConvertTo(target.Loader, type, out var result))
            return AstConstant.TryCreateConstant(target, Position, result, out expr);
        else
            return false;
    }
        
    public NodeApplicationState CheckNodeApplicationState()
    {
        return new(Subject.IsPlaceholder() || Type.IsPlaceholder(), 
            Subject.IsArgumentSplice() || Type.IsArgumentSplice());
    }

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        if (Subject.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Subject, target, StackSemantics.Value);
            return;
        }
        if (Type.IsArgumentSplice())
        {
            AstArgumentSplice.ReportNotSupported(Type, target, StackSemantics.Value);
            return;
        }
            
        var argv =
            AstPartiallyApplicable.PreprocessPartialApplicationArguments(Subject.Singleton());
        var ctorArgc = this.EmitConstructorArguments(target, argv);
        if (Type is AstConstantTypeExpression constType)
            target.EmitConstant(Position, constType.TypeExpression);
        else
            Type.EmitValueCode(target);

        target.EmitCommandCall(Position, ctorArgc + 1, Engine.PartialTypeCastAlias);
    }
}