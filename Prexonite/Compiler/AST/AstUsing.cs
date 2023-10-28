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
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

public class AstUsing : AstScopedBlock,
    IAstHasBlocks
{
    const string LabelPrefix = "using";

    public AstUsing([NotNull] ISourcePosition p, 
        [NotNull] AstBlock lexicalScope)
        : base(p, lexicalScope)
    {
        Block = new AstScopedBlock(p, this,prefix:LabelPrefix);
    }

    #region IAstHasBlocks Members

    public AstBlock[] Blocks
    {
        get { return new AstBlock[] {Block}; }
    }

    #region IAstHasExpressions Members

    public override AstExpr[] Expressions
    {
        get 
        { 
            var b = base.Expressions;
            var r = new AstExpr[b.Length + 1];
            b.CopyTo(r,0);
            r[b.Length] = ResourceExpression;
            return r;
        }
    }

    [PublicAPI]
    public AstScopedBlock Block { get; }

    [PublicAPI]
    public AstExpr ResourceExpression { get; set; }

    #endregion

    #endregion

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        if(stackSemantics == StackSemantics.Value)
            throw new NotSupportedException("Using blocks do not produce values and can thus not be used as expressions.");

        if (ResourceExpression == null)
            throw new PrexoniteException("AstUsing requires Expression to be initialized.");

        var tryNode = new AstTryCatchFinally(Position, this);
        var vContainer = Block.CreateLabel("container");
        target.Function.Variables.Add(vContainer);
        //Try block => Container = {Expression}; {Block};
        var setCont = target.Factory.Call(Position, EntityRef.Variable.Local.Create(vContainer),PCall.Set);
        setCont.Arguments.Add(ResourceExpression);

        var getCont = target.Factory.Call(Position, EntityRef.Variable.Local.Create(vContainer));

        var tryBlock = tryNode.TryBlock;
        tryBlock.Add(setCont);
        tryBlock.AddRange(Block);

        //Finally block => dispose( Container );
        var dispose = target.Factory.Call(Position, EntityRef.Command.Create(Engine.DisposeAlias));
        dispose.Arguments.Add(getCont);

        tryNode.FinallyBlock.Add(dispose);

        //Emit code!
        tryNode.EmitEffectCode(target);
    }
}