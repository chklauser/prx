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
    protected PCall Call { get; } = call;
    protected string Id { get; } = id;

    internal abstract bool Describes(PCall call, string id, bool isStatic);
    internal abstract int BindingCount { get; }
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

internal sealed class DynamicObjectMemberBinder(PCall call, string id) : CallSiteBinder
{
    int _bindingCount;

    internal int BindingCount => Volatile.Read(ref _bindingCount);

    public override T BindDelegate<T>(CallSite<T> site, object[] args)
    {
        if (typeof(T) != typeof(DynamicObjectMemberTarget))
            return null!;

        Interlocked.Increment(ref _bindingCount);

        var sctx = (StackContext)args[0];
        var subject = (PValue)args[1];
        var arguments = (PValue[])args[2];
        var objectType = (ObjectPType)subject.Type;
        var binding = objectType.BindDynamicMember(sctx, subject, arguments, call, id);

        DynamicObjectMemberTarget target = (callSite, context, receiver, actualArguments) =>
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
        return (T)(object)target;
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

internal sealed class StaticObjectMemberBinder(PCall call, string id) : CallSiteBinder
{
    int _bindingCount;

    internal int BindingCount => Volatile.Read(ref _bindingCount);

    public override T BindDelegate<T>(CallSite<T> site, object[] args)
    {
        if (typeof(T) != typeof(StaticObjectMemberTarget))
            return null!;

        Interlocked.Increment(ref _bindingCount);

        var sctx = (StackContext)args[0];
        var objectType = (ObjectPType)args[1];
        var arguments = (PValue[])args[2];
        var binding = objectType.BindStaticMember(sctx, arguments, call, id);

        StaticObjectMemberTarget target = (callSite, context, actualType, actualArguments) =>
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
        return (T)(object)target;
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
