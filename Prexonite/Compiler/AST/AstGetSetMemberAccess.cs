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

using JetBrains.Annotations;

namespace Prexonite.Compiler.Ast;

[method: PublicAPI]
public class AstGetSetMemberAccess(
    string file,
    int line,
    int column,
    PCall call,
    AstExpr subject,
    string id
)
    : AstGetSetImplBase(file, line, column, call),
        IAstPartiallyApplicable
{
    public string Id { get; set; } = id;
    public AstExpr Subject { get; set; } = subject;

    public override AstExpr[] Expressions
    {
        get
        {
            var len = Arguments.Count;
            var ary = new AstExpr[len + 1];
            Array.Copy(Arguments.ToArray(), 0, ary, 1, len);
            ary[0] = Subject;
            return ary;
        }
    }

    [PublicAPI]
    public AstGetSetMemberAccess(
        string file, int line, int column, AstExpr subject, string id)
        : this(file, line, column, PCall.Get, subject, id)
    {
    }

    public override int DefaultAdditionalArguments => base.DefaultAdditionalArguments + 1;

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        Subject.EmitValueCode(target);
        base.DoEmitCode(target, stackSemantics);
    }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        target.EmitGetCall(Position, Arguments.Count, Id, stackSemantics == StackSemantics.Effect);
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        target.EmitSetCall(Position, Arguments.Count, Id);
    }

    public override bool TryOptimize(CompilerTarget target, [NotNullWhen(true)] out AstExpr? expr)
    {
        base.TryOptimize(target, out expr);
        var subject = Subject;
        _OptimizeNode(target, ref subject);
        Subject = subject;
        return false;
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstGetSetMemberAccess(File, Line, Column, Call, Subject, Id);
        CopyBaseMembers(copy);
        return copy;
    }

    public override string ToString()
    {
        var name = Enum.GetName(typeof (PCall), Call);
        return $"{name?.ToLowerInvariant() ?? "-"}: ({Subject}).{Id}{ArgumentsToString()}";
    }

    #region Implementation of IAstPartiallyApplicable

    public void DoEmitPartialApplicationCode(CompilerTarget target)
    {
        var argv =
            AstPartiallyApplicable.PreprocessPartialApplicationArguments(Subject.Singleton().Append(Arguments));
        var ctorArgc = this.EmitConstructorArguments(target, argv);
        target.EmitConstant(Position, (int) Call);
        target.EmitConstant(Position, Id);
        target.EmitCommandCall(Position, ctorArgc + 2, Engine.PartialMemberCallAlias);
    }

    public override NodeApplicationState CheckNodeApplicationState()
    {
        var state = base.CheckNodeApplicationState();
        return state.WithPlaceholders(state.HasPlaceholders || Subject.IsPlaceholder());
    }

    #endregion
}