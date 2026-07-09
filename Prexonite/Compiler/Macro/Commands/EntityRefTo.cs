

using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler.Macro.Commands;

/// <summary>
/// Implements the <code>entityref_to(x)</code> syntax, where <code>x</code> is a simple call or expansion prototype. 
/// Expands into an expression that constructs a reference to the entity called by or expanded by <code>x</code>.
/// </summary>
public class EntityRefTo : MacroCommand
{
    public const string Alias = "entityref_to";

    public EntityRefTo() : base(Alias)
    {
    }

    #region Singleton

    public static EntityRefTo Instance { get; } = new();

    #endregion

    protected override void DoExpand(MacroContext context)
    {
        // entityref_to(IndirectCall(Reference(X))) or entityref_to(Expand(X))
        //  results in an expression that produces the entity reference X at runtime

        if (context.Invocation.Arguments.Count == 0)
        {
            context.ReportMessage(Message.Error($"{Alias} requires one argument.",
                context.Invocation.Position, MessageClasses.EntityRefTo));
            return;
        }
            
        var prototype = context.Invocation.Arguments[0];
        EntityRef? entityRef;
        AstExpand? expand;
        if ((expand = prototype as AstExpand) != null)
        {
            entityRef = expand.Entity;
        }
        else if (prototype.TryMatchCall(out entityRef))
        {
            // entityRef already assigned
        }
        else
        {
            context.ReportMessage(
                Message.Error(
                    $"{Alias} requires its argument to be a direct call or expansion. Instead a {prototype.GetType().Name} was supplied.", context.Invocation.Position, MessageClasses.EntityRefTo));
            return;
        }

        context.Block.Expression = ToExpr(context.Factory, context.Invocation.Position, entityRef);
    }

    class Lifter : IEntityRefMatcher<Tuple<IAstFactory,ISourcePosition>,AstExpr>
    {
        static AstExpr _lift<T>(Tuple<IAstFactory, ISourcePosition> argument, params object[] callArgs) where T : EntityRef
        {
            var create = argument.Item1;
            var pos = argument.Item2;
            var call = create.StaticMemberAccess(pos,
                create.ConstantType(pos, PType.Object[typeof (T)].ToString()), "Create");
            call.Arguments.AddRange(from arg in callArgs select create.Constant(pos, arg));
            return call;
        }

        public AstExpr OnFunction(EntityRef.Function function, Tuple<IAstFactory, ISourcePosition> argument)
        {
            return _lift<EntityRef.Function>(argument, function.Id, function.ModuleName);
        }

        public AstExpr OnCommand(EntityRef.Command command, Tuple<IAstFactory, ISourcePosition> argument)
        {
            return _lift<EntityRef.Command>(argument, command.Id);
        }

        public AstExpr OnMacroCommand(EntityRef.MacroCommand macroCommand, Tuple<IAstFactory, ISourcePosition> argument)
        {
            return _lift<EntityRef.MacroCommand>(argument, macroCommand.Id);
        }

        public AstExpr OnLocalVariable(EntityRef.Variable.Local variable, Tuple<IAstFactory, ISourcePosition> argument)
        {
            return _lift<EntityRef.Variable.Local>(argument, variable.Id);
        }

        public AstExpr OnGlobalVariable(EntityRef.Variable.Global variable, Tuple<IAstFactory, ISourcePosition> argument)
        {
            return _lift<EntityRef.Variable.Global>(argument, variable.Id, variable.ModuleName);
        }
    }

    static readonly Lifter _lifter = new();

    public static AstExpr ToExpr(IAstFactory factory, ISourcePosition position, EntityRef entityRef)
    {
        if (entityRef == null)
            throw new ArgumentNullException(nameof(entityRef));

        return entityRef.Match(_lifter, Tuple.Create(factory, position));
    }
}