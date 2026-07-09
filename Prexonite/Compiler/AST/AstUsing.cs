

using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast;

public class AstUsing : AstScopedBlock,
    IAstHasBlocks
{
    const string LabelPrefix = "using";

    public AstUsing(ISourcePosition p, 
        AstBlock lexicalScope)
        : base(p, lexicalScope)
    {
        Block = new(p, this,prefix:LabelPrefix);
    }

    #region IAstHasBlocks Members

    public AstBlock[] Blocks
    {
        get { return [Block]; }
    }

    #region IAstHasExpressions Members

    public override AstExpr[] Expressions
    {
        get 
        { 
            var b = base.Expressions;
            if (ResourceExpression != null)
            {
                var r = new AstExpr[b.Length + 1];
                b.CopyTo(r, 0);
                r[b.Length] = ResourceExpression;
                
                return r;
            }
            else
            {
                return b;
            }
        }
    }

    [PublicAPI]
    public AstScopedBlock Block { get; }

    [PublicAPI]
    public AstExpr? ResourceExpression { get; set; }

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