using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Prexonite.Modular;

namespace Prexonite
{
    public abstract class CentralCache
    {
        public abstract IObjectCache<ModuleName> ModuleNames { get; }
        public abstract IObjectCache<EntityRef> EntityRefs { get; }
        public abstract ModuleName this[string internalId, Version version] { get; }
        public abstract ModuleName this[ModuleName moduleName] { get; }
        public abstract EntityRef this[EntityRef entityRef] { get; }

        /// <summary>
        /// Links two central caches together.
        /// </summary>
        /// <param name="cache">The cache to link together with this cache.</param>
// ReSharper disable InconsistentNaming
        protected internal abstract CentralCache LinkInto(CentralCache cache);
// ReSharper restore InconsistentNaming
        protected abstract int EstimateSize();

        private CentralCache()
        {
        }

        public static CentralCache Create()
        {
            return new Impl();
        }

        private class Impl : CentralCache
        {
            private MergingCache<EntityRef> _entityRefCache;

            private MergingCache<ModuleName> _moduleNameCache;

            public override IObjectCache<EntityRef> EntityRefs
            {
                get { return _entityRefCache ?? (_entityRefCache = _createEntityRefCache()); }
            }

            private static MergingCache<EntityRef> _createEntityRefCache()
            {
                return MergingCache<EntityRef>.Create(1000);
            }

            public override IObjectCache<ModuleName> ModuleNames
            {
                get { return _moduleNameCache ??  (_moduleNameCache = _createModuleNameCache()); }
            }

            private static MergingCache<ModuleName> _createModuleNameCache()
            {
                return MergingCache<ModuleName>.Create();
            }

            protected override int EstimateSize()
            {
                return (_entityRefCache == null ? 0 : _entityRefCache.EstimateSize()) + (_moduleNameCache == null ? 0 : _moduleNameCache.EstimateSize());
            }

            public override ModuleName this[string internalId, Version version]
            {
                get { return _moduleNameCache.GetCached(new ModuleName(internalId, version)); }
            }

            public override ModuleName this[ModuleName moduleName]
            {
                get { return _moduleNameCache.GetCached(moduleName); }
            }

            public override EntityRef this[EntityRef entityRef]
            {
                get { return _entityRefCache.GetCached(entityRef); }
            }

            protected internal override CentralCache LinkInto(CentralCache cache)
            {
                var c = cache as Impl;
                if(c == null)
                {
                    throw new ArgumentException(
                        "Can only link with proper implementations of CentralCache.", "cache");
                }
                var thisSize = EstimateSize();
                var otherSize = cache.EstimateSize();
                Trace.TraceInformation(string.Format("Linking caches with sizes {0} and {1}.", thisSize, otherSize));
                if(thisSize <= otherSize)
                {
                    return _linkInto(c);
                }
                else
                {
                    return c._linkInto(this);
                }
            }

            private CentralCache _linkInto(Impl targetCache)
            {
                if (ReferenceEquals(targetCache, this))
                    return this;

                //Make sure entity ref caches are the same
                if (_entityRefCache == null)
                    if (targetCache._entityRefCache == null)
                        _entityRefCache = targetCache._entityRefCache = _createEntityRefCache();
                    else
                        _entityRefCache = targetCache._entityRefCache;
                else
                    targetCache._entityRefCache.LinkWith(_entityRefCache);

                //Make sure module name caches are the same
                if (_moduleNameCache == null)
                    if (targetCache._moduleNameCache == null)
                        _moduleNameCache = targetCache._moduleNameCache = _createModuleNameCache();
                    else
                        _moduleNameCache = targetCache._moduleNameCache;
                else
                    targetCache._moduleNameCache.LinkWith(_moduleNameCache);

                return targetCache;
            }
        }
    }

    public abstract class MergingCache<T> : IObjectCache<T>
    {
        private MergingCache()
        {
        }

        public static MergingCache<T> Create(int capacity = 100)
        {
            return new Impl(capacity);
        }

        public abstract T GetCached(T name);

        public abstract void LinkWith(MergingCache<T> cache);

        private class Cache : LastAccessCache<T>
        {
            public Cache(int capacity) : base(capacity)
            {
            }

            public new IEnumerable<T> Contents()
            {
                return base.Contents();
            }

            public new int Count
            {
                get { return base.Count; }
            }
        }

        private class Impl : MergingCache<T>
        {
            private Cache _cache;

            public Impl(int capacity)
            {
                _cache = new Cache(capacity);
            }

            public override T GetCached(T name)
            {
                return _cache.GetCached(name);
            }

            public override void LinkWith(MergingCache<T> cache)
            {
                if (cache == null)
                    throw new ArgumentNullException("cache");
                
                Merge(this, (Impl) cache);
            }

            public override int EstimateSize()
            {
                return _cache.Count;
            }

            public static void Merge(Impl left, Impl right)
            {
                if(ReferenceEquals(left,right))
                    return;

                while (true)
                {
                    var leftCache = left._cache;
                    var rightCache = right._cache;
                    const int moveThreshold = 15;
                    if (leftCache.Count <= (moveThreshold*rightCache.Capacity)/100)
                    {
                        foreach (var moduleName in leftCache.Contents())
                            rightCache.GetCached(moduleName);
                        if(_trySwap(left, leftCache, rightCache))
                            break;
                    }
                    else if (rightCache.Count <= (moveThreshold*leftCache.Capacity)/100)
                    {
                        foreach (var moduleName in rightCache.Contents())
                            leftCache.GetCached(moduleName);
                        if(_trySwap(right,rightCache,leftCache))
                            break;
                    }
                    else
                    {
                        FullMerge(left, right);
                        break;
                    }
                }
            }

            private static bool _trySwap(Impl victim, Cache victimCache, Cache targetCache)
            {
                return ReferenceEquals(Interlocked.CompareExchange(
                    ref victim._cache, targetCache, victimCache),victimCache);
            }

            public static void FullMerge(Impl left, Impl right)
            {
                
                var target = left; // a truly random selection
                var victim = right;
                
                while(true)
                {
                    var targetCache = target._cache;
                    var victimCache = victim._cache;
                    var ls = targetCache.Contents();
                    var rs = victimCache.Contents();

                    using (var li = ls.GetEnumerator())
                    using (var ri = rs.GetEnumerator())
                    {
                        bool hasLeft, hasRight;
                        //iterate in-step
                        while (true)
                        {
                            hasLeft = li.MoveNext();
                            hasRight = ri.MoveNext();

                            if (!hasLeft || !hasRight)
                                break;

                            targetCache.GetCached(li.Current);
                            targetCache.GetCached(ri.Current);
                        }

                        //since we check the conditions at the same time, one might
                        // have triggered the creation of a value
                        if (hasLeft)
                            targetCache.GetCached(li.Current);
                        if (hasRight)
                            targetCache.GetCached(ri.Current);

                        //Add the rest of the longer stream (only one of the two loops will ever run)
                        while (li.MoveNext())
                            targetCache.GetCached(li.Current);
                        while (ri.MoveNext())
                            targetCache.GetCached(ri.Current);
                    }

                    if(_trySwap(victim,victimCache,targetCache))
                        break;
                }
            }
        }

        public abstract int EstimateSize();
    }
}