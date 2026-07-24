using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;
using CilCompiler = Prexonite.Compiler.Cil.Compiler;

namespace Prexonite.Benchmarks;

[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CilObjectMemberCallBenchmarks
{
    const string UncachedPrefix = "uncached";

    readonly Engine _engine = new();
    readonly Application _application = new("cil-benchmarks");
    readonly CilDispatchProbe _probe = new();

    BenchmarkStackContext _sctx = null!;
    PValue _subject = null!;
    PValue[] _subjectArgs = null!;
    PValue[][] _polymorphicArgs = null!;
    PValue[][] _lateGuardMissArgs = null!;
    PValue[] _cachedFreshReceiverArgs = null!;
    PValue[] _uncachedFreshReceiverArgs = null!;

    CompiledBenchmark _cachedZero;
    CompiledBenchmark _uncachedZero;
    CompiledBenchmark _cachedPolymorphic;
    CompiledBenchmark _uncachedPolymorphic;
    CompiledBenchmark _cachedStatic;
    CompiledBenchmark _uncachedStatic;
    CompiledBenchmark _cachedWide;
    CompiledBenchmark _uncachedWide;
    CompiledBenchmark _cachedLateGuardMiss;
    CompiledBenchmark _uncachedLateGuardMiss;
    CompiledBenchmark _cachedFreshReceiver;
    CompiledBenchmark _uncachedFreshReceiver;
    int _cachedPolymorphicIndex;
    int _uncachedPolymorphicIndex;
    int _cachedLateGuardMissIndex;
    int _uncachedLateGuardMissIndex;

    [GlobalSetup]
    public void Setup()
    {
        _engine.RegisterAssembly(typeof(CilDispatchProbe).Assembly);
        _sctx = new(_engine, _application);
        _subject = _engine.CreateNativePValue(_probe);
        _subjectArgs = [_subject];
        _polymorphicArgs =
        [
            [_subject, PType.Int.CreatePValue(1)],
            [_subject, PType.Int.CreatePValue(1).WithTypeLock()],
        ];
        _lateGuardMissArgs =
        [
            [_subject, PType.Object[typeof(int)].CreatePValue(16)],
            [_subject, PType.Object[typeof(string)].CreatePValue("sixteen")],
        ];
        _cachedFreshReceiverArgs = new PValue[1];
        _uncachedFreshReceiverArgs = new PValue[1];

        var loader = new Loader(new(_engine, _application));
        loader.LoadFromString(_source);
        if (loader.ErrorCount != 0)
            throw new InvalidOperationException(
                "Cannot compile the CIL benchmark source: "
                    + string.Join(Environment.NewLine, loader.Errors)
            );

        foreach (
            var function in _application.Functions.Where(function =>
                function.Id.StartsWith(UncachedPrefix, StringComparison.Ordinal)
            )
        )
            function.Meta[CilCompiler.DisableObjectMemberCallSitesKey] = true;

        CilCompiler.Compile(_application, _engine, FunctionLinking.FullyIsolated);

        _cachedZero = _getCompiled("cachedZero");
        _uncachedZero = _getCompiled("uncachedZero");
        _cachedPolymorphic = _getCompiled("cachedPolymorphic");
        _uncachedPolymorphic = _getCompiled("uncachedPolymorphic");
        _cachedStatic = _getCompiled("cachedStatic");
        _uncachedStatic = _getCompiled("uncachedStatic");
        _cachedWide = _getCompiled("cachedWide");
        _uncachedWide = _getCompiled("uncachedWide");
        _cachedLateGuardMiss = _getCompiled("cachedLateGuardMiss");
        _uncachedLateGuardMiss = _getCompiled("uncachedLateGuardMiss");
        _cachedFreshReceiver = _getCompiled("cachedFreshReceiver");
        _uncachedFreshReceiver = _getCompiled("uncachedFreshReceiver");

        _assertResult(_cachedZero.Invoke(_sctx, _subjectArgs), 42);
        _assertResult(_uncachedZero.Invoke(_sctx, _subjectArgs), 42);
        _assertResult(_cachedPolymorphic.Invoke(_sctx, _polymorphicArgs[0]), "automatic");
        _assertResult(_cachedPolymorphic.Invoke(_sctx, _polymorphicArgs[1]), "locked");
        _assertResult(_uncachedPolymorphic.Invoke(_sctx, _polymorphicArgs[0]), "automatic");
        _assertResult(_cachedStatic.Invoke(_sctx, []), 42);
        _assertResult(_uncachedStatic.Invoke(_sctx, []), 42);
        _assertResult(_cachedWide.Invoke(_sctx, _subjectArgs), 136);
        _assertResult(_uncachedWide.Invoke(_sctx, _subjectArgs), 136);
        _assertResult(_cachedLateGuardMiss.Invoke(_sctx, _lateGuardMissArgs[0]), 16);
        _assertResult(_cachedLateGuardMiss.Invoke(_sctx, _lateGuardMissArgs[1]), 16);
        _assertResult(_uncachedLateGuardMiss.Invoke(_sctx, _lateGuardMissArgs[0]), 16);

        _assertStableBindingCount(_cachedZero, 1);
        _assertStableBindingCount(_cachedPolymorphic, 2);
        _assertStableBindingCount(_cachedStatic, 1);
        _assertStableBindingCount(_cachedWide, 1);
        _assertStableBindingCount(_cachedLateGuardMiss, 2);
        _assertUncached(_uncachedZero);
        _assertUncached(_uncachedPolymorphic);
        _assertUncached(_uncachedStatic);
        _assertUncached(_uncachedWide);
        _assertUncached(_uncachedLateGuardMiss);
        _assertForcedReceiverRebinding();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CIL zero-argument")]
    public PValue UncachedZeroArgument()
    {
        return _uncachedZero.Invoke(_sctx, _subjectArgs);
    }

    [Benchmark]
    [BenchmarkCategory("CIL zero-argument")]
    public PValue CachedZeroArgument()
    {
        return _cachedZero.Invoke(_sctx, _subjectArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CIL polymorphic lock")]
    public PValue UncachedPolymorphicLock()
    {
        return _uncachedPolymorphic.Invoke(
            _sctx,
            _polymorphicArgs[_uncachedPolymorphicIndex++ & 1]
        );
    }

    [Benchmark]
    [BenchmarkCategory("CIL polymorphic lock")]
    public PValue CachedPolymorphicLock()
    {
        return _cachedPolymorphic.Invoke(_sctx, _polymorphicArgs[_cachedPolymorphicIndex++ & 1]);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CIL static")]
    public PValue UncachedStatic()
    {
        return _uncachedStatic.Invoke(_sctx, []);
    }

    [Benchmark]
    [BenchmarkCategory("CIL static")]
    public PValue CachedStatic()
    {
        return _cachedStatic.Invoke(_sctx, []);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CIL wide hit")]
    public PValue UncachedWide()
    {
        return _uncachedWide.Invoke(_sctx, _subjectArgs);
    }

    [Benchmark]
    [BenchmarkCategory("CIL wide hit")]
    public PValue CachedWide()
    {
        return _cachedWide.Invoke(_sctx, _subjectArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CIL late miss")]
    public PValue UncachedLateGuardMiss()
    {
        return _uncachedLateGuardMiss.Invoke(
            _sctx,
            _lateGuardMissArgs[_uncachedLateGuardMissIndex++ & 1]
        );
    }

    [Benchmark]
    [BenchmarkCategory("CIL late miss")]
    public PValue CachedLateGuardMiss()
    {
        return _cachedLateGuardMiss.Invoke(
            _sctx,
            _lateGuardMissArgs[_cachedLateGuardMissIndex++ & 1]
        );
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CIL receiver rebind")]
    public PValue UncachedFreshReceiver()
    {
        _uncachedFreshReceiverArgs[0] = new ObjectPType(typeof(CilDispatchProbe)).CreatePValue(
            _probe
        );
        return _uncachedFreshReceiver.Invoke(_sctx, _uncachedFreshReceiverArgs);
    }

    [Benchmark]
    [BenchmarkCategory("CIL receiver rebind")]
    public PValue CachedFreshReceiver()
    {
        _cachedFreshReceiverArgs[0] = new ObjectPType(typeof(CilDispatchProbe)).CreatePValue(
            _probe
        );
        return _cachedFreshReceiver.Invoke(_sctx, _cachedFreshReceiverArgs);
    }

    CompiledBenchmark _getCompiled(string id)
    {
        var function =
            _application.Functions[id]
            ?? throw new InvalidOperationException($"Benchmark function {id} is missing.");
        var implementation =
            function.CilImplementation
            ?? throw new InvalidOperationException($"Benchmark function {id} was not compiled.");
        return new(function, implementation, _memberCallOffset(function));
    }

    static int _memberCallOffset(PFunction function)
    {
        var offset = function.Code.FindIndex(instruction =>
            instruction.OpCode is OpCode.get or OpCode.sget
        );
        if (offset < 0)
            throw new InvalidOperationException(
                $"Benchmark function {function.Id} has no member call."
            );
        return offset;
    }

    static void _assertResult(PValue value, object expected)
    {
        if (!Equals(value.Value, expected))
            throw new InvalidOperationException(
                $"CIL benchmark setup produced {value.Value ?? "null"}, expected {expected}."
            );
    }

    static void _assertStableBindingCount(CompiledBenchmark benchmark, int expected)
    {
        var site = benchmark.Function.ObjectMemberCallSites.GetExisting(benchmark.CallOffset);
        if (site?.BindingCount != expected)
            throw new InvalidOperationException(
                $"{benchmark.Function.Id} bound {site?.BindingCount ?? 0} rules; expected {expected}."
            );
    }

    static void _assertUncached(CompiledBenchmark benchmark)
    {
        if (benchmark.Function.ObjectMemberCallSites.GetExisting(benchmark.CallOffset) != null)
            throw new InvalidOperationException(
                $"Diagnostic baseline {benchmark.Function.Id} unexpectedly created a call site."
            );
    }

    void _assertForcedReceiverRebinding()
    {
        var siteBefore = _cachedFreshReceiver.Function.ObjectMemberCallSites.GetExisting(
            _cachedFreshReceiver.CallOffset
        );
        var bindingsBefore = siteBefore?.BindingCount ?? 0;
        _assertResult(CachedFreshReceiver(), 42);
        _assertResult(CachedFreshReceiver(), 42);
        _assertResult(UncachedFreshReceiver(), 42);

        var site = _cachedFreshReceiver.Function.ObjectMemberCallSites.GetExisting(
            _cachedFreshReceiver.CallOffset
        );
        if (site?.BindingCount != bindingsBefore + 2)
            throw new InvalidOperationException(
                "Fresh receiver tags did not force the compiled call site to rebind."
            );
        _assertUncached(_uncachedFreshReceiver);
    }

    readonly record struct CompiledBenchmark(
        PFunction Function,
        CilFunction Implementation,
        int CallOffset
    )
    {
        internal PValue Invoke(StackContext sctx, PValue[] args)
        {
            Implementation(Function, sctx, args, null, out var result, out _);
            return result;
        }
    }

    const string _source = """
        function cachedZero(target) = target.Zero();
        function uncachedZero(target) = target.Zero();

        function cachedPolymorphic(target, value) = target.Overloaded(value, 2, 3);
        function uncachedPolymorphic(target, value) = target.Overloaded(value, 2, 3);

        function cachedStatic() = Prexonite::Benchmarks::CilDispatchProbe.StaticExact(17, 25);
        function uncachedStatic() = Prexonite::Benchmarks::CilDispatchProbe.StaticExact(17, 25);

        function cachedWide(target) =
            target.WideExact(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
        function uncachedWide(target) =
            target.WideExact(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);

        function cachedLateGuardMiss(target, last) =
            target.WideShape(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, last);
        function uncachedLateGuardMiss(target, last) =
            target.WideShape(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, last);

        function cachedFreshReceiver(target) = target.Exact(17, 25);
        function uncachedFreshReceiver(target) = target.Exact(17, 25);
        """;
}

public sealed class CilDispatchProbe
{
    public int Zero()
    {
        return 42;
    }

    public int Exact(int left, int right)
    {
        return left + right;
    }

    public static int StaticExact(int left, int right)
    {
        return left + right;
    }

    public string Overloaded(double value, int second, int third)
    {
        return "automatic";
    }

    public string Overloaded(int value, double second, double third)
    {
        return "locked";
    }

    public int WideExact(
        int value01,
        int value02,
        int value03,
        int value04,
        int value05,
        int value06,
        int value07,
        int value08,
        int value09,
        int value10,
        int value11,
        int value12,
        int value13,
        int value14,
        int value15,
        int value16
    )
    {
        return value01
            + value02
            + value03
            + value04
            + value05
            + value06
            + value07
            + value08
            + value09
            + value10
            + value11
            + value12
            + value13
            + value14
            + value15
            + value16;
    }

    public int WideShape(
        int value01,
        int value02,
        int value03,
        int value04,
        int value05,
        int value06,
        int value07,
        int value08,
        int value09,
        int value10,
        int value11,
        int value12,
        int value13,
        int value14,
        int value15,
        object value16
    )
    {
        return 16;
    }
}
