using System.Collections.Immutable;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro;

public sealed record PatternMacroCaptureDescription(string Name, bool WhenTrue, bool WhenFalse);

public sealed class PatternMacroSignature
{
    public static PatternMacroSignature Empty { get; } = new([]);

    public PatternMacroSignature(IEnumerable<PatternMacroCaptureDescription> captures)
    {
        Captures = captures.ToImmutableArray();
        if (
            Captures.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != Captures.Length
        )
            throw new ArgumentException("A pattern macro signature cannot export a name twice.");
    }

    public ImmutableArray<PatternMacroCaptureDescription> Captures { get; }
}

public enum PatternMacroPhase
{
    Describe,
    Expand,
}

public sealed class PatternMacroDescribeContext
{
    readonly List<PatternMacroCaptureDescription> _captures = [];

    internal PatternMacroDescribeContext(AstGetSet invocation)
    {
        Invocation = invocation;
    }

    public AstGetSet Invocation { get; }
    public PatternMacroPhase Phase => PatternMacroPhase.Describe;
    public bool IsDescribe => true;

    public void Export(string name, bool whenTrue = true, bool whenFalse = false) =>
        _captures.Add(new(name, whenTrue, whenFalse));

    public PatternMacroSignature ExportedSignature => new(_captures);
}

public sealed class PatternMacroExpansionContext
{
    readonly Dictionary<string, PatternCapture> _exported;
    readonly List<PatternCapture> _privateCaptures = [];
    readonly CompilerTarget _target;

    internal PatternMacroExpansionContext(
        CompilerTarget target,
        AstGetSet invocation,
        string subjectLocal,
        IReadOnlyDictionary<string, PatternCapture> exported
    )
    {
        _target = target;
        Invocation = invocation;
        SubjectLocal = subjectLocal;
        _exported = new(exported, StringComparer.OrdinalIgnoreCase);
    }

    public AstGetSet Invocation { get; }
    public string SubjectLocal { get; }
    public IAstFactory Factory => _target.Factory;
    public PatternMacroPhase Phase => PatternMacroPhase.Expand;
    public bool IsDescribe => false;
    public IReadOnlyCollection<PatternCapture> PrivateCaptures => _privateCaptures;

    public PatternCapture ExportedCapture(string name) =>
        _exported.TryGetValue(name, out var capture)
            ? capture
            : throw new KeyNotFoundException(
                $"The pattern macro did not describe an exported capture named {name}."
            );

    public PatternCapture AllocatePrivateCapture(string debugName, ISourcePosition position)
    {
        var physicalId = _target.GenerateLocalId("pattern\\macro\\private\\" + debugName);
        _target.Function.Variables.Add(physicalId);
        var capture = new PatternCapture(debugName, physicalId, physicalId, position, true, false);
        _privateCaptures.Add(capture);
        return capture;
    }
}

/// <summary>
/// Dedicated pattern-position macro ABI. Description runs during parsing; expansion runs after
/// dependent source has already resolved against the frozen signature.
/// </summary>
public abstract class PatternMacroCommand : MacroCommand
{
    protected PatternMacroCommand(string id)
        : base(id) { }

    protected sealed override void DoExpand(MacroContext context)
    {
        context.ReportMessage(
            Message.Error(
                $"Pattern macro {Id} can only be expanded in pattern position.",
                context.Invocation.Position,
                MessageClasses.PatternMacroPositionRequired
            )
        );
    }

    protected abstract PatternMacroSignature DescribePattern(PatternMacroDescribeContext context);
    protected abstract AstPattern ExpandPattern(PatternMacroExpansionContext context);

    public PatternMacroSignature InvokeDescribePattern(PatternMacroDescribeContext context) =>
        DescribePattern(context) ?? PatternMacroSignature.Empty;

    public AstPattern InvokeExpandPattern(PatternMacroExpansionContext context) =>
        ExpandPattern(context)
        ?? throw new InvalidOperationException($"Pattern macro {Id} returned no pattern AST.");
}

public static class ScriptPatternMacroRuntime
{
    public const string PatternMacroKey = @"pattern\macro";

    public static PatternMacroSignature Describe(
        CompilerTarget target,
        PFunction function,
        AstGetSet invocation
    )
    {
        var context = new PatternMacroDescribeContext(invocation);
        _invoke(target, function, invocation, context);
        return context.ExportedSignature;
    }

    public static AstPattern Expand(
        CompilerTarget target,
        PFunction function,
        AstGetSet invocation,
        PatternMacroExpansionContext context
    )
    {
        var result = _invoke(target, function, invocation, context);
        return result.Value as AstPattern
            ?? throw new PrexoniteException(
                $"Script pattern macro {function.LogicalId} must return an AstPattern during expansion."
            );
    }

    static PValue _invoke(
        CompilerTarget target,
        PFunction function,
        AstGetSet invocation,
        object context
    )
    {
        var loader = target.Loader;
        var contextVariable = CompilerTarget.CreateReadonlyVariable(
            loader.CreateNativePValue(context)
        );
        var environment = new SymbolTable<PVariable>(1)
        {
            { MacroAliases.ContextAlias, contextVariable },
        };
        var shared = function
            .Meta[PFunction.SharedNamesKey]
            .List.Select(entry =>
            {
                if (environment[entry.Text] is not { } variable)
                    throw new PrexoniteException(
                        $"Pattern macro references unsupported context variable {entry.Text}."
                    );
                return variable;
            })
            .ToArray();
        var closure = new Closure(function, shared);
        var arguments = invocation.Arguments.Select(loader.CreateNativePValue).ToArray();
        var application = function.ParentApplication;
        try
        {
            application._SuppressInitialization = true;
            return closure.IndirectCall(loader, arguments);
        }
        finally
        {
            application._SuppressInitialization = false;
        }
    }
}
