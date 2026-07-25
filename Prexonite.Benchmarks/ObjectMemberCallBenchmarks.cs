using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using Prexonite.Types;

namespace Prexonite.Benchmarks;

[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ObjectMemberCallBenchmarks
{
    const string ExactMember = nameof(DispatchProbe.Exact);
    const string OverloadedMember = nameof(DispatchProbe.Overloaded);
    const string ShapeMember = nameof(DispatchProbe.Shape);
    const string StaticExactMember = nameof(DispatchProbe.StaticExact);
    const string WideExactMember = nameof(DispatchProbe.WideExact);
    const string WideShapeMember = nameof(DispatchProbe.WideShape);

    readonly Engine _engine = new();
    readonly Application _application = new("benchmarks");
    readonly DispatchProbe _probe = new();

    BenchmarkStackContext _sctx = null!;
    PValue _subject = null!;
    ObjectPType _staticType = null!;
    DynamicObjectMemberCallSite _exactSite = null!;
    DynamicObjectMemberCallSite _overloadedSite = null!;
    DynamicObjectMemberCallSite _polymorphicSite = null!;
    DynamicObjectMemberCallSite _wideExactSite = null!;
    DynamicObjectMemberCallSite _lateGuardMissSite = null!;
    DynamicObjectMemberCallSite _freshReceiverTagSite = null!;
    DynamicObjectMemberCallSite _freshArgumentTagSite = null!;
    StaticObjectMemberCallSite _staticSite = null!;
    PValue[] _exactArgs = null!;
    PValue[] _overloadedArgs = null!;
    PValue[][] _polymorphicArgs = null!;
    PValue[] _wideExactArgs = null!;
    PValue[][] _lateGuardMissArgs = null!;
    PValue[] _uncachedFreshArgumentTagArgs = null!;
    PValue[] _cachedFreshArgumentTagArgs = null!;
    int _cachedPolymorphicIndex;
    int _uncachedPolymorphicIndex;
    int _cachedLateGuardMissIndex;
    int _uncachedLateGuardMissIndex;

    [GlobalSetup]
    public void Setup()
    {
        _sctx = new(_engine, _application);
        _subject = _engine.CreateNativePValue(_probe);
        _staticType = PType.Object[typeof(DispatchProbe)];
        _exactArgs = [PType.Int.CreatePValue(17), PType.Int.CreatePValue(25)];
        _overloadedArgs =
        [
            PType.Int.CreatePValue(1),
            PType.Int.CreatePValue(2),
            PType.Int.CreatePValue(3),
        ];
        _polymorphicArgs =
        [
            _overloadedArgs,
            [
                PType.Int.CreatePValue(1).WithTypeLock(),
                PType.Int.CreatePValue(2),
                PType.Int.CreatePValue(3),
            ],
        ];
        _wideExactArgs = Enumerable.Range(1, 16).Select(PType.Int.CreatePValue).ToArray();
        _lateGuardMissArgs =
        [
            _createWideShapeArgs(PType.Object[typeof(int)].CreatePValue(16)),
            _createWideShapeArgs(PType.Object[typeof(string)].CreatePValue("sixteen")),
        ];
        _uncachedFreshArgumentTagArgs = new PValue[1];
        _cachedFreshArgumentTagArgs = new PValue[1];

        _exactSite = new(PCall.Get, ExactMember);
        _overloadedSite = new(PCall.Get, OverloadedMember);
        _polymorphicSite = new(PCall.Get, OverloadedMember);
        _wideExactSite = new(PCall.Get, WideExactMember);
        _lateGuardMissSite = new(PCall.Get, WideShapeMember);
        _freshReceiverTagSite = new(PCall.Get, ExactMember);
        _freshArgumentTagSite = new(PCall.Get, ShapeMember);
        _staticSite = new(PCall.Get, StaticExactMember);

        _assertResult(_exactSite.Invoke(_sctx, _subject, _exactArgs), 42);
        _assertResult(_overloadedSite.Invoke(_sctx, _subject, _overloadedArgs), "automatic");
        _assertResult(_polymorphicSite.Invoke(_sctx, _subject, _polymorphicArgs[0]), "automatic");
        _assertResult(_polymorphicSite.Invoke(_sctx, _subject, _polymorphicArgs[1]), "locked");
        _assertResult(_wideExactSite.Invoke(_sctx, _subject, _wideExactArgs), 136);
        _assertResult(_lateGuardMissSite.Invoke(_sctx, _subject, _lateGuardMissArgs[0]), 16);
        _assertResult(_lateGuardMissSite.Invoke(_sctx, _subject, _lateGuardMissArgs[1]), 16);
        _assertResult(_staticSite.Invoke(_sctx, _staticType, _exactArgs), 42);

        if (_polymorphicSite.BindingCount != 2)
            throw new InvalidOperationException(
                "The polymorphic site did not bind exactly two rules."
            );

        _assertForcedRebinding();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Instance exact")]
    public PValue UncachedInstanceExact()
    {
        return _subject.DynamicCall(_sctx, _exactArgs.AsSpan(), PCall.Get, ExactMember);
    }

    [Benchmark]
    [BenchmarkCategory("Instance exact")]
    public PValue CachedInstanceExact()
    {
        return _exactSite.Invoke(_sctx, _subject, _exactArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Instance overloaded")]
    public PValue UncachedInstanceOverloaded()
    {
        return _subject.DynamicCall(_sctx, _overloadedArgs.AsSpan(), PCall.Get, OverloadedMember);
    }

    [Benchmark]
    [BenchmarkCategory("Instance overloaded")]
    public PValue CachedInstanceOverloaded()
    {
        return _overloadedSite.Invoke(_sctx, _subject, _overloadedArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Instance polymorphic")]
    public PValue UncachedInstancePolymorphic()
    {
        return _subject.DynamicCall(
            _sctx,
            _polymorphicArgs[_uncachedPolymorphicIndex++ & 1].AsSpan(),
            PCall.Get,
            OverloadedMember
        );
    }

    [Benchmark]
    [BenchmarkCategory("Instance polymorphic")]
    public PValue CachedInstancePolymorphic()
    {
        return _polymorphicSite.Invoke(
            _sctx,
            _subject,
            _polymorphicArgs[_cachedPolymorphicIndex++ & 1]
        );
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Wide cache hit")]
    public PValue UncachedWideExact()
    {
        return _subject.DynamicCall(_sctx, _wideExactArgs.AsSpan(), PCall.Get, WideExactMember);
    }

    [Benchmark]
    [BenchmarkCategory("Wide cache hit")]
    public PValue CachedWideExact()
    {
        return _wideExactSite.Invoke(_sctx, _subject, _wideExactArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Late guard miss")]
    public PValue UncachedLateGuardMiss()
    {
        return _subject.DynamicCall(
            _sctx,
            _lateGuardMissArgs[_uncachedLateGuardMissIndex++ & 1].AsSpan(),
            PCall.Get,
            WideShapeMember
        );
    }

    [Benchmark]
    [BenchmarkCategory("Late guard miss")]
    public PValue CachedLateGuardMiss()
    {
        return _lateGuardMissSite.Invoke(
            _sctx,
            _subject,
            _lateGuardMissArgs[_cachedLateGuardMissIndex++ & 1]
        );
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Rebind: receiver tag")]
    public PValue UncachedFreshReceiverTag()
    {
        var subject = new ObjectPType(typeof(DispatchProbe)).CreatePValue(_probe);
        return subject.DynamicCall(_sctx, _exactArgs.AsSpan(), PCall.Get, ExactMember);
    }

    [Benchmark]
    [BenchmarkCategory("Rebind: receiver tag")]
    public PValue CachedFreshReceiverTag()
    {
        var subject = new ObjectPType(typeof(DispatchProbe)).CreatePValue(_probe);
        return _freshReceiverTagSite.Invoke(_sctx, subject, _exactArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Rebind: argument tag")]
    public PValue UncachedFreshArgumentTag()
    {
        _uncachedFreshArgumentTagArgs[0] = new ObjectPType(typeof(int)).CreatePValue(1);
        return _subject.DynamicCall(
            _sctx,
            _uncachedFreshArgumentTagArgs.AsSpan(),
            PCall.Get,
            ShapeMember
        );
    }

    [Benchmark]
    [BenchmarkCategory("Rebind: argument tag")]
    public PValue CachedFreshArgumentTag()
    {
        _cachedFreshArgumentTagArgs[0] = new ObjectPType(typeof(int)).CreatePValue(1);
        return _freshArgumentTagSite.Invoke(_sctx, _subject, _cachedFreshArgumentTagArgs);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Static exact")]
    public PValue UncachedStaticExact()
    {
        return _staticType.StaticCall(_sctx, _exactArgs, PCall.Get, StaticExactMember);
    }

    [Benchmark]
    [BenchmarkCategory("Static exact")]
    public PValue CachedStaticExact()
    {
        return _staticSite.Invoke(_sctx, _staticType, _exactArgs);
    }

    static void _assertResult(PValue value, object expected)
    {
        if (!Equals(value.Value, expected))
            throw new InvalidOperationException(
                $"Benchmark setup resolved an unexpected overload: {value.Value ?? "null"}."
            );
    }

    PValue[] _createWideShapeArgs(PValue last)
    {
        var args = Enumerable.Range(1, 15).Select(PType.Int.CreatePValue).ToArray();
        Array.Resize(ref args, 16);
        args[^1] = last;
        return args;
    }

    void _assertForcedRebinding()
    {
        var receiverBindings = _freshReceiverTagSite.BindingCount;
        for (var i = 0; i < ObjectMemberCallSite.MaxPolymorphicBindingCount + 2; i++)
            _assertResult(CachedFreshReceiverTag(), 42);
        if (
            _freshReceiverTagSite.BindingCount
                != receiverBindings + ObjectMemberCallSite.MaxPolymorphicBindingCount + 2
            || !_freshReceiverTagSite.IsMegamorphic
            || _freshReceiverTagSite.CachedBindingCount
                != ObjectMemberCallSite.MaxPolymorphicBindingCount
        )
            throw new InvalidOperationException(
                "Fresh receiver tags did not saturate the call site and continue rebinding."
            );

        var argumentBindings = _freshArgumentTagSite.BindingCount;
        for (var i = 0; i < ObjectMemberCallSite.MaxPolymorphicBindingCount + 2; i++)
            _assertResult(CachedFreshArgumentTag(), 1);
        if (
            _freshArgumentTagSite.BindingCount
                != argumentBindings + ObjectMemberCallSite.MaxPolymorphicBindingCount + 2
            || !_freshArgumentTagSite.IsMegamorphic
            || _freshArgumentTagSite.CachedBindingCount
                != ObjectMemberCallSite.MaxPolymorphicBindingCount
        )
            throw new InvalidOperationException(
                "Fresh argument tags did not saturate the call site and continue rebinding."
            );
    }

    public sealed class DispatchProbe
    {
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

        public object Shape(object value)
        {
            return value;
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
}
