

using System.Diagnostics;
using Prexonite.Compiler;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Modular;

namespace Prexonite.Commands.Core;

public sealed class Call_Tail : StackAwareCommand
{
    #region Singleton

    Call_Tail()
    {
    }

    public static Call_Tail Instance { get; } = new();

    #endregion

    public const string Alias = @"call\tail\perform";


    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        if (args.IsEmpty || (PValue?)args[0] == null || args[0].IsNull)
            return PType.Null;

        var iargs = make_tailcall(sctx, args);

        return args[0].IndirectCall(sctx, [..iargs.AsReadOnly()]);
    }

    public override StackContext CreateStackContext(StackContext sctx, PValue[]? args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        if (args == null || args.Length < 1 || (PValue?)args[0] == null || args[0].IsNull)
            return new NullContext(sctx);

        var iargs = make_tailcall(sctx, args);

        return Call.CreateStackContext(sctx, args[0], iargs.ToArray());
    }

    static List<PValue> make_tailcall(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        var iargs = Call.FlattenArguments(sctx, args, 1);

        //remove caller from stack
        var stack = sctx.ParentEngine.Stack;
        var node = stack.FindLast(sctx);
        if (node == null)
        {
            throw new PrexoniteException(
                $"{Engine.Call_TailAlias} only works on the interpreted stack.");
        }
        stack.Remove(node);
        return iargs;
    }

    #region Partial application via call\star

    public PartialTailCall Partial { [DebuggerStepThrough] get; } = new();

    public class PartialTailCall : PartialCallWrapper
    {
        protected PartialTailCall(string alias, string callImplementationId,
            SymbolInterpretations callImplementetaionInterpretation)
            : base(alias, EntityRef.Command.Create(Alias))
        {
        }

        public PartialTailCall()
            : this(Engine.Call_TailAlias, Alias, SymbolInterpretations.Command)
        {
        }

        protected override void DoExpand(MacroContext context)
        {
            _specifyDeficiency(context);

            base.DoExpand(context);
        }

        static void _specifyDeficiency(MacroContext context)
        {
            context.Function.Meta[PFunction.VolatileKey] = true;
            if (!context.Function.Meta.TryGetValue(PFunction.DeficiencyKey, out var deficiency) ||
                deficiency.Text == "")
                context.Function.Meta[PFunction.DeficiencyKey] = $"Uses {Engine.Call_TailAlias}.";
        }
    }

    #endregion
}