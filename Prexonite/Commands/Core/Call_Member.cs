using System.Diagnostics;
using Prexonite.Commands.List;
using Prexonite.Compiler;
using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Modular;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of (obj, [isSet, ] id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
/// </summary>
public sealed class Call_Member : PCommand
{
    #region Singleton

    Call_Member() { }

    public static Call_Member Instance { get; } = new();

    #endregion

    public const string Alias = @"call\member\perform";

    /// <summary>
    ///     Implementation of (obj, [isSet, ] id, arg1, arg2, arg3, ..., argn) ⇒ obj.id(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Wrap Lists in other lists, if you want to pass them without being unfolded:
    ///         <code>
    ///             function main()
    ///             {   var myList = [1, 2];
    ///             var obj = "{1}hell{0}";
    ///             print( call\member(obj, "format",  [ myList ]) );
    ///             }
    ///
    ///             //Prints "2hell1"
    ///         </code>
    ///     </para>
    /// </remarks>
    /// <param name = "sctx">The stack context in which to call the callable argument.</param>
    /// <param name = "args">A list of the form [ obj, id, arg1, arg2, arg3, ..., argn].<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by the member call.</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args.Length < 2 || args[0] == null)
            throw new ArgumentException(
                "The command callmember has the signature(obj, [isSet,] id [, arg1, arg2,...,argn])."
            );

        var isSet = false;
        string id;
        var i = 2;

        if (args[1].Type == PType.Bool && args.Length > 2)
        {
            isSet = (bool)args[1].Value!;
            id = args[i++].CallToString(sctx);
        }
        else
        {
            id = args[1].CallToString(sctx);
        }

        return Run(sctx, args[0], isSet, id, args.Slice(i, args.Length - i));
    }

    /// <summary>
    ///     Implementation of (obj, id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <param name = "sctx">The stack context in which to call the member of <paramref name = "obj" />.</param>
    /// <param name = "obj">The obj to call.</param>
    /// <param name = "id">The id of the member to call.</param>
    /// <param name = "args">The array of arguments to pass to the member call.<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by the member call.</returns>
    /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null.</exception>
    public PValue Run(StackContext sctx, PValue obj, string id, params PValue[] args)
    {
        return Run(sctx, obj, false, id, args);
    }

    /// <summary>
    ///     Implementation of (obj, id, arg1, arg2, arg3, ..., argn) => obj.id(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <param name = "sctx">The stack context in which to call the member of <paramref name = "obj" />.</param>
    /// <param name = "obj">The obj to call.</param>
    /// <param name = "isSet">Indicates whether to perform a Set-call.</param>
    /// <param name = "id">The id of the member to call.</param>
    /// <param name = "args">The array of arguments to pass to the member call.<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by the member call.</returns>
    /// <exception cref = "ArgumentNullException"><paramref name = "sctx" /> is null.</exception>
    public PValue Run(
        StackContext sctx,
        PValue? obj,
        bool isSet,
        string id,
        params ReadOnlySpan<PValue> args
    )
    {
        if (obj == null)
            return PType.Null.CreatePValue();
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        var iargs = new List<PValue>();
        foreach (var arg in args)
        {
            var folded = Map._ToEnumerable(sctx, arg);
            if (folded == null)
                iargs.Add(arg);
            else
                iargs.AddRange(folded);
        }

        return obj.DynamicCall(sctx, iargs.ToArray(), isSet ? PCall.Set : PCall.Get, id);
    }

    #region Partial application via call\star

    public PartialMemberCall Partial { [DebuggerStepThrough] get; } = new();

    public class PartialMemberCall : PartialCallWrapper
    {
        protected PartialMemberCall(
            string alias,
            string callImplementationId,
            SymbolInterpretations callImplementetaionInterpretation
        )
            : base(alias, EntityRef.Command.Create(Alias)) { }

        public PartialMemberCall()
            : this(Engine.Call_MemberAlias, Alias, SymbolInterpretations.Command) { }

        protected override IEnumerable<AstExpr> GetCallArguments(MacroContext context)
        {
            var argv = context.Invocation.Arguments;
            return Extensions.Append(argv.Take(1), _getIsSetExpr(context)).Append(argv.Skip(1));
        }

        static AstExpr _getIsSetExpr(MacroContext context)
        {
            return context.CreateConstant(context.Invocation.Call == PCall.Set);
        }

        protected override AstGetSet GetTrivialPartialApplication(MacroContext context)
        {
            var pa = base.GetTrivialPartialApplication(context);
            pa.Arguments.Insert(1, _getIsSetExpr(context));
            return pa;
        }

        protected override int GetPassThroughArguments(MacroContext context)
        {
            return 3;
        }
    }

    #endregion
}
