using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

[SuppressMessage(
    "Microsoft.Naming",
    "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Cil"
)]
public class CompileToCil : PCommand, ICilCompilerAware
{
    #region Singleton

    CompileToCil() { }

    public static CompileToCil Instance { get; } = new();

    #endregion

    public static bool AlreadyCompiledStatically { get; private set; }

    #region ICilCompilerAware Members

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execute the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execute the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    /// <remarks>
    ///     <para>
    ///         This variation is independent of the executing engine and can take advantage from static binding in CIL compilation.
    ///     </para>
    /// </remarks>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        args ??= [];

        var linking = FunctionLinking.FullyStatic;
        switch (args.Length)
        {
            case 0:
                //come from case 1
                if (sctx.ParentEngine.StaticLinkingAllowed)
                {
                    if (args.Length == 0)
                    {
                        if (AlreadyCompiledStatically)
                            throw new PrexoniteException(
                                $"You should only use static compilation once per process. Use {Engine.CompileToCilAlias}(true)"
                                    + " to force recompilation (warning: memory leak!). Should your program recompile dynamically, "
                                    + $"use {Engine.CompileToCilAlias}(false) for disposable implementations."
                            );
                        else
                            AlreadyCompiledStatically = true;
                    }
                }
                else
                {
                    linking = FunctionLinking.FullyIsolated;
                }
                Compiler.Cil.Compiler.Compile(sctx.ParentApplication, sctx.ParentEngine, linking);
                break;
            case 1:
                var arg0 = args[0];

                if (arg0 == null || arg0.IsNull)
                    goto case 0;
                if (arg0.Type == PType.Bool)
                {
                    if ((bool)arg0.Value!)
                        linking = FunctionLinking.FullyStatic;
                    else
                        linking = FunctionLinking.FullyIsolated;
                    goto case 0;
                }
                else if (arg0.Type == typeof(FunctionLinking))
                {
                    linking = (FunctionLinking)arg0.Value!;
                    goto case 0;
                }
                else
                {
                    goto default;
                }
            default:
                throw new PrexoniteException("Expecting 0 or 1 argument.");
        }

        return PType.Null;
    }
}
