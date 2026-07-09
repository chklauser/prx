

using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Core;

public sealed class Char : PCommand, ICilCompilerAware, ICilExtension
{
    Char()
    {
    }

    public static Char Instance { get; } = new();

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1)
            throw new PrexoniteException("Char requires at least one argument.");

        var arg = args[0];
        if (arg.Type == PType.String)
        {
            var s = (string) arg.Value!;
            if (s.Length == 0)
                throw new PrexoniteException("Cannot create char from empty string.");
            else
                return s[0];
        }
        else if (arg.TryConvertTo(sctx, PType.Char, true, out var v))
        {
            return v;
        }
        else if (arg.TryConvertTo(sctx, PType.Int, true, out v))
        {
            return (char) (int) v.Value!;
        }
        else
        {
            throw new PrexoniteException("Cannot create char from " + arg);
        }
    }

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    #region Implementation of ICilCompilerAware

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException("The command " + GetType().Name +
            " does not support CIL compilation via ICilCompilerAware.");
    }

    #endregion

    #region Implementation of ICilExtension

    bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        return dynamicArgc == 0 && staticArgv.Length == 1 &&
            (staticArgv[0].TryGetString(out var literal) && literal.Length > 0 ||
                staticArgv[0].TryGetInt(out var code) && code >= 0);
    }

    void ICilExtension.Implement(CompilerState state, Instruction ins,
        CompileTimeValue[] staticArgv, int dynamicArgc)
    {
        if (ins.JustEffect)
            return; // Usually for commands without side-effects you have to at least
        //  pop dynamic arguments from the stack.
        // ValidateArguments proved that there are no arguments on the stack.
        int code;
        if (staticArgv[0].TryGetString(out var literal))
            code = literal[0];
        else if (!staticArgv[0].TryGetInt(out code))
            throw new ArgumentException(
                "char command requires one argument that is either a string or a 32-bit integer with the most significant bit cleared.");

        state.EmitLdcI4(code);
        state.EmitWrapChar();
    }

    #endregion
}