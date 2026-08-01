using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;
using CilCompiler = Prexonite.Compiler.Cil.Compiler;

namespace Prexonite.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class PatternMatchingCilBenchmarks
{
    const int MatchesPerInvoke = 256;

    readonly Engine _engine = new();
    readonly Application _application = new("pattern-matching-cil-benchmarks");

    BenchmarkStackContext _sctx = null!;
    CompiledBenchmark _control = default;
    CompiledBenchmark _literal = default;
    CompiledBenchmark _bindingGuard = default;
    CompiledBenchmark _listShape = default;
    CompiledBenchmark _list = default;
    CompiledBenchmark _property = default;
    CompiledBenchmark _view = default;
    CompiledBenchmark _complexMatch = default;
    PValue[] _controlArgs = null!;
    PValue[] _literalHitArgs = null!;
    PValue[] _literalMissArgs = null!;
    PValue[] _bindingGuardArgs = null!;
    PValue[] _listArgs = null!;
    PValue[] _complexHitArgs = null!;
    PValue[] _complexLateMissArgs = null!;

    [GlobalSetup]
    public void Setup()
    {
        _engine.RegisterAssembly(typeof(PatternMatchingCilBenchmarks).Assembly);
        _sctx = new BenchmarkStackContext(_engine, _application);

        var loader = new Loader(new LoaderOptions(_engine, _application));
        loader.LoadFromString(_source);
        if (loader.ErrorCount != 0)
            throw new InvalidOperationException(
                "Cannot compile the pattern-matching CIL benchmark source: "
                    + string.Join(Environment.NewLine, loader.Errors)
            );

        CilCompiler.Compile(_application, _engine, FunctionLinking.FullyStatic);

        _control = _getCompiled("control");
        _literal = _getCompiled("literal");
        _bindingGuard = _getCompiled("binding_guard");
        _listShape = _getCompiled("list_shape");
        _list = _getCompiled("list_pattern");
        _property = _getCompiled("property_pattern");
        _view = _getCompiled("view_pattern");
        _complexMatch = _getCompiled("complex_match");

        var iterations = PType.Int.CreatePValue(MatchesPerInvoke);
        _controlArgs = [iterations];
        _literalHitArgs = [PType.Bool.CreatePValue(true), iterations];
        _literalMissArgs = [PType.Bool.CreatePValue(false), iterations];
        _bindingGuardArgs = [PType.Int.CreatePValue(7), iterations];
        _listArgs =
        [
            PType.List.CreatePValue(
                new List<PValue>
                {
                    PType.Int.CreatePValue(3),
                    PType.Int.CreatePValue(5),
                    PType.Int.CreatePValue(8),
                }
            ),
            iterations,
        ];
        _complexHitArgs =
        [
            _engine.CreateNativePValue(
                new PatternBenchmarkEnvelope(
                    new PatternBenchmarkPolicy(true, "active"),
                    (List<PValue>)_listArgs[0].Value!,
                    11
                )
            ),
            iterations,
        ];
        _complexLateMissArgs =
        [
            _engine.CreateNativePValue(
                new PatternBenchmarkEnvelope(
                    new PatternBenchmarkPolicy(false, "inactive"),
                    (List<PValue>)_listArgs[0].Value!,
                    11
                )
            ),
            iterations,
        ];

        _assertResult(_control.Invoke(_sctx, _controlArgs), MatchesPerInvoke);
        _assertResult(_literal.Invoke(_sctx, _literalHitArgs), MatchesPerInvoke);
        _assertResult(_literal.Invoke(_sctx, _literalMissArgs), 0);
        _assertResult(_bindingGuard.Invoke(_sctx, _bindingGuardArgs), 7 * MatchesPerInvoke);
        _assertResult(_listShape.Invoke(_sctx, _listArgs), MatchesPerInvoke);
        _assertResult(_list.Invoke(_sctx, _listArgs), 3 * MatchesPerInvoke);
        _assertResult(_property.Invoke(_sctx, _complexHitArgs), MatchesPerInvoke);
        _assertResult(_view.Invoke(_sctx, _complexHitArgs), MatchesPerInvoke);
        _assertResult(_complexMatch.Invoke(_sctx, _complexHitArgs), 3 * MatchesPerInvoke);
        _assertResult(_complexMatch.Invoke(_sctx, _complexLateMissArgs), 11 * MatchesPerInvoke);
    }

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue Control() => _control.Invoke(_sctx, _controlArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke, Baseline = true)]
    public PValue LiteralHit() => _literal.Invoke(_sctx, _literalHitArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue LiteralMiss() => _literal.Invoke(_sctx, _literalMissArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue BindingGuardHit() => _bindingGuard.Invoke(_sctx, _bindingGuardArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue ListShapeHit() => _listShape.Invoke(_sctx, _listArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue ListPatternHit() => _list.Invoke(_sctx, _listArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue PropertyPatternHit() => _property.Invoke(_sctx, _complexHitArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue ViewPatternHit() => _view.Invoke(_sctx, _complexHitArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue ComplexMatchHit() => _complexMatch.Invoke(_sctx, _complexHitArgs);

    [Benchmark(OperationsPerInvoke = MatchesPerInvoke)]
    public PValue ComplexMatchLateMiss() => _complexMatch.Invoke(_sctx, _complexLateMissArgs);

    CompiledBenchmark _getCompiled(string id)
    {
        var function =
            _application.Functions[id]
            ?? throw new InvalidOperationException($"Benchmark function {id} is missing.");
        return new CompiledBenchmark(
            function,
            function.CilImplementation
                ?? throw new InvalidOperationException($"Benchmark function {id} was not compiled.")
        );
    }

    static void _assertResult(PValue value, int expected)
    {
        if (!Equals(value.Value, expected))
            throw new InvalidOperationException(
                $"Pattern benchmark setup produced {value.Value ?? "null"}, expected {expected}."
            );
    }

    readonly record struct CompiledBenchmark(PFunction Function, CilFunction Implementation)
    {
        internal PValue Invoke(StackContext sctx, PValue[] args)
        {
            Implementation(Function, sctx, args, null, out var result, out _);
            return result;
        }
    }

    const string _source = """
        pattern_matching enabled;

        function control(iterations)
        {
            var i = 0;
            while (i < iterations)
                i++;
            return i;
        }

        function literal(value, iterations)
        {
            var hits = 0;
            var i = 0;
            while (i < iterations)
            {
                if (value is true)
                    hits++;
                i++;
            }
            return hits;
        }

        function binding_guard(value, iterations)
        {
            var sum = 0;
            var i = 0;
            while (i < iterations)
            {
                if (value is item when item > 0)
                    sum += item;
                i++;
            }
            return sum;
        }

        function list_pattern(value, iterations)
        {
            var sum = 0;
            var i = 0;
            while (i < iterations)
            {
                if (value is [head, _, _])
                    sum += head;
                i++;
            }
            return sum;
        }

        function list_shape(value, iterations)
        {
            var hits = 0;
            var i = 0;
            while (i < iterations)
            {
                if (value is [_, _, _])
                    hits++;
                i++;
            }
            return hits;
        }

        function view_record(value) = [value.Policy, value.Values];

        function property_pattern(value, iterations)
        {
            var hits = 0;
            var i = 0;
            while (i < iterations)
            {
                if (value is {Policy: _})
                    hits++;
                i++;
            }
            return hits;
        }

        function view_pattern(value, iterations)
        {
            var hits = 0;
            var i = 0;
            while (i < iterations)
            {
                if (value is record(_, _))
                    hits++;
                i++;
            }
            return hits;
        }

        function complex_match(value, iterations)
        {
            var sum = 0;
            var i = 0;
            while (i < iterations)
            {
                sum += match (value)
                {
                    record({Active: true, Name: name}, [head, _, _])
                        when name != null && head > 0 -> head;
                    {Fallback: fallback} -> fallback;
                    _ -> 0;
                };
                i++;
            }
            return sum;
        }
        """;
}

public sealed record PatternBenchmarkPolicy(bool Active, string Name);

public sealed record PatternBenchmarkEnvelope(
    PatternBenchmarkPolicy Policy,
    List<PValue> Values,
    int Fallback
);
