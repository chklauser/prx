

using Prexonite.Commands.List;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Modular;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
/// </summary>
/// <remarks>
///     <para>
///         Returns null if no callable object is passed.
///     </para>
///     <para>
///         Uses the <see cref = "IIndirectCall" /> interface.
///     </para>
/// </remarks>
/// <seealso cref = "IIndirectCall" />
public sealed class Call : StackAwareCommand, ICilCompilerAware
{
    Call()
    {
    }

    public const string Alias = @"call\perform";

    public static Call Instance { get; } = new();

    /// <summary>
    ///     Implementation of (ref f, [arg1, arg2, arg3, ..., argn]) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns null if no callable object is passed.
    ///     </para>
    ///     <para>
    ///         Uses the <see cref = "IIndirectCall" /> interface.
    ///     </para>
    ///     <para>
    ///         Wrap Lists in other lists, if you want to pass them without being unfolded: 
    ///         <code>
    ///             function main()
    ///             {   var myList = [1, 2, 3];
    ///             var f = xs => xs.Count;
    ///             print( call(f, [ myList ]) );
    ///             }
    /// 
    ///             //Prints "3"
    ///         </code>
    ///     </para>
    /// </remarks>
    /// <seealso cref = "IIndirectCall" />
    /// <param name = "sctx">The stack context in which to call the callable argument.</param>
    /// <param name = "args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by <see cref = "IIndirectCall.IndirectCall" /> or PValue Null if no callable object has been passed.</returns>
    public static PValue RunStatically(StackContext sctx, PValue[]? args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null)
            return PType.Null.CreatePValue();

        var iargs = FlattenArguments(sctx, args, 1);

        return args[0].IndirectCall(sctx, [..iargs.AsReadOnly()]);
    }

    /// <summary>
    ///     Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Returns null if no callable object is passed.
    ///     </para>
    ///     <para>
    ///         Uses the <see cref = "IIndirectCall" /> interface.
    ///     </para>
    ///     <para>
    ///         Wrap Lists in other lists, if you want to pass them without being unfolded: 
    ///         <code>
    ///             function main()
    ///             {   var myList = [1, 2, 3];
    ///             var f = xs => xs.Count;
    ///             print( call(f, [ myList ]) );
    ///             }
    /// 
    ///             //Prints "3"
    ///         </code>
    ///     </para>
    /// </remarks>
    /// <seealso cref = "IIndirectCall" />
    /// <param name = "sctx">The stack context in which to call the callable argument.</param>
    /// <param name = "args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
    ///     Lists and coroutines are expanded.</param>
    /// <returns>The result returned by <see cref = "IIndirectCall.IndirectCall" /> or PValue Null if no callable object has been passed.</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    /// <summary>
    ///     Takes an argument list and injects elements of top-level lists into that argument list.
    /// </summary>
    /// <param name = "sctx">The stack context in which to convert enumerables.</param>
    /// <param name = "args">The raw list of arguments to process.</param>
    /// <param name = "offset">The offset at which to start processing.</param>
    /// <returns>A copy of the argument list with top-level lists expanded.</returns>
    public static List<PValue> FlattenArguments(StackContext sctx, ReadOnlySpan<PValue> args, int offset = 0)
    {
        var iargs = new List<PValue>(args.Length);
        for (var i = offset; i < args.Length; i++)
        {
            var arg = args[i];
            var folded = Map._ToEnumerable(sctx, arg);
            if (folded == null)
                iargs.Add(arg);
            else
                iargs.AddRange(folded);
        }
        return iargs;
    }

    public override StackContext CreateStackContext(StackContext sctx, PValue[]? args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null || args[0].IsNull)
            return new NullContext(sctx);

        var iargs = FlattenArguments(sctx, args, 1);

        var callable = args[0];
        return CreateStackContext(sctx, callable, iargs.ToArray());
    }

    public static StackContext CreateStackContext(StackContext sctx, PValue callable,
        PValue[] args)
    {
        if (callable is { Type: ObjectPType, Value: IStackAware sa })
            return sa.CreateStackContext(sctx, args);
        else
            return new IndirectCallContext(sctx, callable, args);
    }

    #region ICilCompilerAware Members

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion

    #region Macro for partial application

    readonly PartialCallWrapper _partialCall = new(Engine.CallAlias,
        EntityRef.Command.Create(Alias));

    public PartialMacroCommand Partial => _partialCall;

    #endregion
}