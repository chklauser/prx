using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Prexonite.Types;

namespace Prexonite;

internal delegate PValue DynamicObjectMemberTarget(
    CallSite site,
    StackContext sctx,
    PValue subject,
    PValue[] args
);

internal delegate PValue StaticObjectMemberTarget(
    CallSite site,
    StackContext sctx,
    ObjectPType objectType,
    PValue[] args
);

internal abstract class ObjectMemberCallSite(PCall call, string id)
{
    internal const int MaxPolymorphicBindingCount = 4;

    protected PCall Call { get; } = call;
    protected string Id { get; } = id;

    internal abstract bool Describes(PCall call, string id, bool isStatic);
    internal abstract int BindingCount { get; }
    internal abstract int CachedBindingCount { get; }
    internal abstract bool IsMegamorphic { get; }
}

internal sealed class DynamicObjectMemberCallSite : ObjectMemberCallSite
{
    readonly DynamicObjectMemberBinder _binder;
    readonly CallSite<DynamicObjectMemberTarget> _site;

    internal DynamicObjectMemberCallSite(PCall call, string id)
        : base(call, id)
    {
        _binder = new(call, id);
        _site = CallSite<DynamicObjectMemberTarget>.Create(_binder);
    }

    internal PValue Invoke(StackContext sctx, PValue subject, PValue[] args)
    {
        return _site.Target(_site, sctx, subject, args);
    }

    internal override bool Describes(PCall call, string id, bool isStatic)
    {
        return !isStatic && call == Call && string.Equals(id, Id, StringComparison.Ordinal);
    }

    internal override int BindingCount => _binder.BindingCount;
    internal override int CachedBindingCount => _binder.CachedBindingCount;
    internal override bool IsMegamorphic => _binder.IsMegamorphic;
}

internal sealed class StaticObjectMemberCallSite : ObjectMemberCallSite
{
    readonly StaticObjectMemberBinder _binder;
    readonly CallSite<StaticObjectMemberTarget> _site;

    internal StaticObjectMemberCallSite(PCall call, string id)
        : base(call, id)
    {
        _binder = new(call, id);
        _site = CallSite<StaticObjectMemberTarget>.Create(_binder);
    }

    internal PValue Invoke(StackContext sctx, ObjectPType objectType, PValue[] args)
    {
        return _site.Target(_site, sctx, objectType, args);
    }

    internal override bool Describes(PCall call, string id, bool isStatic)
    {
        return isStatic && call == Call && string.Equals(id, Id, StringComparison.Ordinal);
    }

    internal override int BindingCount => _binder.BindingCount;
    internal override int CachedBindingCount => _binder.CachedBindingCount;
    internal override bool IsMegamorphic => _binder.IsMegamorphic;
}

internal sealed class ObjectMemberCallSiteTable(int instructionCount)
{
    readonly object _resizeLock = new();
    ObjectMemberCallSite?[] _sites = new ObjectMemberCallSite?[instructionCount];

    internal DynamicObjectMemberCallSite GetDynamic(int instructionOffset, PCall call, string id)
    {
        return (DynamicObjectMemberCallSite)_get(
            instructionOffset,
            call,
            id,
            false,
            static (call, id) => new DynamicObjectMemberCallSite(call, id)
        );
    }

    internal StaticObjectMemberCallSite GetStatic(int instructionOffset, PCall call, string id)
    {
        return (StaticObjectMemberCallSite)_get(
            instructionOffset,
            call,
            id,
            true,
            static (call, id) => new StaticObjectMemberCallSite(call, id)
        );
    }

    ObjectMemberCallSite _get(
        int instructionOffset,
        PCall call,
        string id,
        bool isStatic,
        Func<PCall, string, ObjectMemberCallSite> create
    )
    {
        var sites = _getSites(instructionOffset);
        var site = Volatile.Read(ref sites[instructionOffset]);
        if (site == null)
        {
            var newSite = create(call, id);
            site =
                Interlocked.CompareExchange(ref sites[instructionOffset], newSite, null) ?? newSite;
        }

        if (!site.Describes(call, id, isStatic))
            throw new PrexoniteException(
                $"Instruction {instructionOffset} attempted to use two incompatible object member call sites."
            );

        return site;
    }

    ObjectMemberCallSite?[] _getSites(int instructionOffset)
    {
        var sites = Volatile.Read(ref _sites);
        if (instructionOffset < sites.Length)
            return sites;

        lock (_resizeLock)
        {
            sites = _sites;
            if (instructionOffset >= sites.Length)
            {
                var newLength = Math.Max(instructionOffset + 1, Math.Max(4, sites.Length * 2));
                Array.Resize(ref sites, newLength);
                Volatile.Write(ref _sites, sites);
            }
        }

        return sites;
    }

    internal ObjectMemberCallSite? GetExisting(int instructionOffset)
    {
        var sites = Volatile.Read(ref _sites);
        return instructionOffset < sites.Length
            ? Volatile.Read(ref sites[instructionOffset])
            : null;
    }
}

internal abstract class ObjectMemberBinder(PCall call, string id) : CallSiteBinder
{
    readonly object _bindingLock = new();
    ObjectPType.MemberBinding[] _cachedBindings = [];
    int _bindingCount;
    int _isMegamorphic;

    protected PCall Call { get; } = call;
    protected string Id { get; } = id;
    internal int BindingCount => Volatile.Read(ref _bindingCount);
    internal int CachedBindingCount => Volatile.Read(ref _cachedBindings).Length;
    internal bool IsMegamorphic => Volatile.Read(ref _isMegamorphic) != 0;

    protected T BindRule<T>(
        Func<ObjectPType.MemberBinding, bool> matchesCurrentCall,
        Func<ObjectPType.MemberBinding> bind,
        Func<ObjectPType.MemberBinding, T> createGuardedTarget,
        Func<ObjectPType.MemberBinding[], T> createMegamorphicTarget
    )
        where T : class
    {
        lock (_bindingLock)
        {
            var cachedBindings = _cachedBindings;
            if (_isMegamorphic != 0)
                return createMegamorphicTarget(cachedBindings);

            // Two callers can miss the DLR rule cache concurrently. Avoid retaining the
            // same Prexonite binding twice when the first caller wins that race.
            foreach (var cachedBinding in cachedBindings)
                if (matchesCurrentCall(cachedBinding))
                    return createGuardedTarget(cachedBinding);

            Interlocked.Increment(ref _bindingCount);
            var newBinding = bind();
            if (cachedBindings.Length < ObjectMemberCallSite.MaxPolymorphicBindingCount)
            {
                var newBindings = new ObjectPType.MemberBinding[cachedBindings.Length + 1];
                newBindings[0] = newBinding;
                Array.Copy(cachedBindings, 0, newBindings, 1, cachedBindings.Length);
                Volatile.Write(ref _cachedBindings, newBindings);
                return createGuardedTarget(newBinding);
            }

            // Keep the four most recent shapes. The returned target handles all future
            // calls itself, so the DLR will install it as a sticky megamorphic rule.
            var retainedBindings = new ObjectPType.MemberBinding[
                ObjectMemberCallSite.MaxPolymorphicBindingCount
            ];
            retainedBindings[0] = newBinding;
            Array.Copy(cachedBindings, 0, retainedBindings, 1, retainedBindings.Length - 1);
            Volatile.Write(ref _cachedBindings, retainedBindings);
            Volatile.Write(ref _isMegamorphic, 1);
            return createMegamorphicTarget(retainedBindings);
        }
    }

    protected ObjectPType.MemberBinding BindUncached(Func<ObjectPType.MemberBinding> bind)
    {
        Interlocked.Increment(ref _bindingCount);
        return bind();
    }

    public override Expression Bind(
        object[] args,
        ReadOnlyCollection<ParameterExpression> parameters,
        LabelTarget returnLabel
    )
    {
        throw new NotSupportedException("This binder supplies call-site delegates directly.");
    }
}

internal sealed class DynamicObjectMemberBinder(PCall call, string id)
    : ObjectMemberBinder(call, id)
{
    public override T BindDelegate<T>(CallSite<T> site, object[] args)
    {
        if (typeof(T) != typeof(DynamicObjectMemberTarget))
            return null!;

        var sctx = (StackContext)args[0];
        var subject = (PValue)args[1];
        var arguments = (PValue[])args[2];
        var objectType = (ObjectPType)subject.Type;
        var target = BindRule<DynamicObjectMemberTarget>(
            binding => binding.MatchesDynamic(subject, arguments),
            () => objectType.BindDynamicMember(sctx, subject, arguments, Call, Id),
            _createGuardedTarget,
            _createMegamorphicTarget
        );
        return (T)(object)target;
    }

    static DynamicObjectMemberTarget _createGuardedTarget(ObjectPType.MemberBinding binding)
    {
        return (callSite, context, receiver, actualArguments) =>
        {
            if (binding.MatchesDynamic(receiver, actualArguments))
                return binding.Invoke(context, receiver, actualArguments);

            return ((CallSite<DynamicObjectMemberTarget>)callSite).Update(
                callSite,
                context,
                receiver,
                actualArguments
            );
        };
    }

    DynamicObjectMemberTarget _createMegamorphicTarget(ObjectPType.MemberBinding[] retainedBindings)
    {
        return (_, context, receiver, actualArguments) =>
        {
            foreach (var binding in retainedBindings)
                if (binding.MatchesDynamic(receiver, actualArguments))
                    return binding.Invoke(context, receiver, actualArguments);

            var objectType = (ObjectPType)receiver.Type;
            var rebound = BindUncached(() =>
                objectType.BindDynamicMember(context, receiver, actualArguments, Call, Id)
            );
            return rebound.Invoke(context, receiver, actualArguments);
        };
    }
}

internal sealed class StaticObjectMemberBinder(PCall call, string id) : ObjectMemberBinder(call, id)
{
    public override T BindDelegate<T>(CallSite<T> site, object[] args)
    {
        if (typeof(T) != typeof(StaticObjectMemberTarget))
            return null!;

        var sctx = (StackContext)args[0];
        var objectType = (ObjectPType)args[1];
        var arguments = (PValue[])args[2];
        var target = BindRule<StaticObjectMemberTarget>(
            binding => binding.MatchesStatic(objectType, arguments),
            () => objectType.BindStaticMember(sctx, arguments, Call, Id),
            _createGuardedTarget,
            _createMegamorphicTarget
        );
        return (T)(object)target;
    }

    static StaticObjectMemberTarget _createGuardedTarget(ObjectPType.MemberBinding binding)
    {
        return (callSite, context, actualType, actualArguments) =>
        {
            if (binding.MatchesStatic(actualType, actualArguments))
                return binding.Invoke(context, null, actualArguments);

            return ((CallSite<StaticObjectMemberTarget>)callSite).Update(
                callSite,
                context,
                actualType,
                actualArguments
            );
        };
    }

    StaticObjectMemberTarget _createMegamorphicTarget(ObjectPType.MemberBinding[] retainedBindings)
    {
        return (_, context, actualType, actualArguments) =>
        {
            foreach (var binding in retainedBindings)
                if (binding.MatchesStatic(actualType, actualArguments))
                    return binding.Invoke(context, null, actualArguments);

            var rebound = BindUncached(() =>
                actualType.BindStaticMember(context, actualArguments, Call, Id)
            );
            return rebound.Invoke(context, null, actualArguments);
        };
    }
}
