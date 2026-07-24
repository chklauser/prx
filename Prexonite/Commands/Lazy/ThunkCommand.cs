using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.Lazy;

public class ThunkCommand : PCommand, ICilCompilerAware
{
    #region Singleton

    ThunkCommand() { }

    public static ThunkCommand Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
    }

    // ReSharper disable MemberCanBePrivate.Global
    // Part of CIL compiler infrastructure.
    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null)
            throw new PrexoniteException("The thunk command requires an expression.");

        var expr = args[0];
        var parameters = args.Skip(1).Select(_EnforceThunk).ToArray();

        return PType.Object.CreatePValue(Thunk.NewExpression(expr, parameters));
    }

    // ReSharper restore MemberCanBePrivate.Global

    internal static PValue _EnforceThunk(PValue value)
    {
        if (value.Type.Equals(PType.Object[typeof(Thunk)]))
            return value;
        else
            return PType.Object.CreatePValue(Thunk.NewValue(value));
    }

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException(
            "The command "
                + GetType().Name
                + " does not support CIL compilation via ICilCompilerAware."
        );
    }
}

/// <summary>
///     Represents a value that is computed at most once, on first access.
/// </summary>
public class Thunk : IIndirectCall, IObject
{
    readonly object _initializationLock = new();
    PValue? _expression;
    PValue[]? _parameters;
    readonly PValue? _value;
    Lazy<PValue>? _evaluation;

    #region Construction

    Thunk(PValue expr, PValue[] parameters)
    {
        _expression = expr ?? throw new ArgumentNullException(nameof(expr));
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    Thunk(PValue value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static Thunk NewValue(PValue value)
    {
        return new(value);
    }

    public static Thunk NewExpression(PValue expr, PValue[] parameters)
    {
        return new(expr, parameters);
    }

    #endregion

    #region Interface

    public PValue Force(StackContext sctx)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        if (_value is { } value)
            return value;

        var evaluation = Volatile.Read(ref _evaluation);
        if (evaluation == null)
        {
            lock (_initializationLock)
            {
                evaluation = _evaluation;
                if (evaluation == null)
                {
                    evaluation = new(
                        () => _evaluate(sctx),
                        LazyThreadSafetyMode.ExecutionAndPublication
                    );
                    Volatile.Write(ref _evaluation, evaluation);
                }
            }
        }

        return evaluation.Value;
    }

    public bool IsEvaluated =>
        _value != null || Volatile.Read(ref _evaluation)?.IsValueCreated == true;

    public bool TryDynamicCall(
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
            case "FORCE":
                result = Force(sctx);
                break;
            case "EVALUATED":
            case "ISEVALUATED":
                result = IsEvaluated;
                break;
        }

        return result != null;
    }

    #endregion

    PValue _evaluate(StackContext sctx)
    {
        var expression = _expression;
        var parameters = _parameters;
        if (expression == null || parameters == null)
            throw new InvalidOperationException("Thunk expression is not available.");

        try
        {
            var result = expression.IndirectCall(sctx, parameters.AsSpan());
            while (result.Value is Thunk nested)
                result = nested.Force(sctx);
            return result;
        }
        finally
        {
            // Lazy caches both values and exceptions, so the factory inputs are no longer needed.
            _expression = null;
            _parameters = null;
        }
    }

    PValue IIndirectCall.IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
    {
        return Force(sctx);
    }
}
