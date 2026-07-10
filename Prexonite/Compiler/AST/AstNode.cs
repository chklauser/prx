using System.Diagnostics;
using Prexonite.Properties;

namespace Prexonite.Compiler.Ast;

public abstract class AstNode : IObject
{
    protected AstNode(string file, int line, int column)
        : this(new SourcePosition(file, line, column)) { }

    protected AstNode(ISourcePosition position)
    {
        Position = position ?? throw new ArgumentNullException(nameof(position));
    }

    internal AstNode(Parser p)
        : this(p.scanner.File, p.t.line, p.t.col) { }

    public ISourcePosition Position { get; }

    public string File => Position.File;

    public int Line => Position.Line;

    public int Column => Position.Column;

    protected abstract void DoEmitCode(CompilerTarget target, StackSemantics semantics);

    public void EmitValueCode(CompilerTarget target)
    {
        EmitCode(target, StackSemantics.Value);
    }

    public void EmitEffectCode(CompilerTarget target)
    {
        EmitCode(target, StackSemantics.Effect);
    }

    public void EmitCode(CompilerTarget target, StackSemantics justEffectCode)
    {
        var partiallyApplicabale = this as IAstPartiallyApplicable;
        var applicationState =
            partiallyApplicabale?.CheckNodeApplicationState() ?? default(NodeApplicationState);

        if (justEffectCode == StackSemantics.Effect)
        {
            if (applicationState.HasPlaceholders)
            {
                //A partial application does not have an effect.
            }
            else
            {
                DoEmitCode(target, StackSemantics.Effect);
            }
        }
        else
        {
            if (applicationState.HasPlaceholders)
            {
                Debug.Assert(partiallyApplicabale != null, "partiallyApplicabale != null");
                partiallyApplicabale.DoEmitPartialApplicationCode(target);
            }
            else
            {
                DoEmitCode(target, StackSemantics.Value);
            }
        }
    }

    /// <summary>
    ///     Checks the nodes immediate child nodes for instances of <see cref = "AstPlaceholder" />. Must yield the same result as <see
    ///      cref = "IAstPartiallyApplicable.CheckForPlaceholders" />, if implemented in derived types.
    /// </summary>
    /// <returns>True if this node has placeholders; false otherwise</returns>
    public bool CheckForPlaceholders() =>
        this is IAstPartiallyApplicable pa && pa.CheckNodeApplicationState().HasPlaceholders;

    internal static AstExpr _GetOptimizedNode(CompilerTarget target, AstExpr expr)
    {
        if (target == null)
            throw new ArgumentNullException(
                nameof(target),
                Resources.AstNode__GetOptimizedNode_CompilerTarget_null
            );
        if (expr == null)
            throw new ArgumentNullException(
                nameof(expr),
                Resources.AstNode__GetOptimizedNode_Expression_null
            );
        return expr.TryOptimize(target, out var opt) ? opt : expr;
    }

    internal static void _OptimizeNode(CompilerTarget target, ref AstExpr expr)
    {
        if (target == null)
            throw new ArgumentNullException(
                nameof(target),
                Resources.AstNode__GetOptimizedNode_CompilerTarget_null
            );
        if (expr == null)
            throw new ArgumentNullException(
                nameof(expr),
                Resources.AstNode__GetOptimizedNode_Expression_null
            );
        expr = _GetOptimizedNode(target, expr);
    }

    #region Implementation of IObject

    public virtual bool TryDynamicCall(
        StackContext sctx,
        ReadOnlySpan<PValue> args,
        PCall call,
        string id,
        [NotNullWhen(true)] out PValue? result
    )
    {
        result = null;

        switch (id.ToUpperInvariant())
        {
            case "GETOPTIMIZEDNODE":
                CompilerTarget? target;
                if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                    throw new PrexoniteException(
                        "_GetOptimizedNode(CompilerTarget target) requires target."
                    );
                if (this is not AstExpr expr)
                    throw new PrexoniteException("The node is not an AstExpr.");

                result = target.Loader.CreateNativePValue(_GetOptimizedNode(target, expr));
                break;
            case "EMITEFFECTCODE":
                if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                    throw new PrexoniteException(
                        "EmitEffectCode(CompilerTarget target) requires target."
                    );
                EmitEffectCode(target);
                result = PType.Null;
                break;
        }

        return result != null;
    }

    #endregion
}
