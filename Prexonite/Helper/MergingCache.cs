// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite;

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

    CentralCache()
    {
    }

    public static CentralCache Create()
    {
        return new Impl();
    }

    class Impl : CentralCache
    {
        MergingCache<EntityRef>? _entityRefCache;

        MergingCache<ModuleName>? _moduleNameCache;

        public override IObjectCache<EntityRef> EntityRefs => _entityRefCache ??= _createEntityRefCache();

        static MergingCache<EntityRef> _createEntityRefCache()
        {
            return MergingCache<EntityRef>.Create(1000);
        }

        public override IObjectCache<ModuleName> ModuleNames => _moduleNameCache ??= _createModuleNameCache();

        static MergingCache<ModuleName> _createModuleNameCache()
        {
            return MergingCache<ModuleName>.Create();
        }

        protected override int EstimateSize()
        {
            return (_entityRefCache?.EstimateSize() ?? 0) + (_moduleNameCache?.EstimateSize() ?? 0);
        }

        public override ModuleName this[string internalId, Version version] => ModuleNames.GetCached(new(internalId, version));

        public override ModuleName this[ModuleName moduleName] => ModuleNames.GetCached(moduleName);

        public override EntityRef this[EntityRef entityRef] => EntityRefs.GetCached(entityRef);

        protected internal override CentralCache LinkInto(CentralCache cache)
        {
            if(cache is not Impl c)
            {
                throw new ArgumentException(
                    "Can only link with proper implementations of CentralCache.", nameof(cache));
            }
            var thisSize = EstimateSize();
            var otherSize = cache.EstimateSize();
            if(thisSize <= otherSize)
            {
                return _linkInto(c);
            }
            else
            {
                return c._linkInto(this);
            }
        }

        CentralCache _linkInto(Impl targetCache)
        {
            if (ReferenceEquals(targetCache, this))
                return this;

            Debug.Assert(targetCache != null, "targetCache cannot be null.",
                "CentralCache MergingCache._linkInto(targetCache): targetCache cannot be null.");

            //Make sure entity ref caches are the same
            if (_entityRefCache == null)
                if (targetCache._entityRefCache == null)
                    _entityRefCache = targetCache._entityRefCache = _createEntityRefCache();
                else
                    _entityRefCache = targetCache._entityRefCache;
            else
            if (targetCache._entityRefCache == null)
                targetCache._entityRefCache = _entityRefCache;
            else
                targetCache._entityRefCache.LinkWith(_entityRefCache);

            //Make sure module name caches are the same
            if (_moduleNameCache == null)
                if (targetCache._moduleNameCache == null)
                    _moduleNameCache = targetCache._moduleNameCache = _createModuleNameCache();
                else
                    _moduleNameCache = targetCache._moduleNameCache;
            else
            if (targetCache._moduleNameCache == null)
                targetCache._moduleNameCache = _moduleNameCache;
            else
                targetCache._moduleNameCache.LinkWith(_moduleNameCache);

            return targetCache;
        }
    }
}

public abstract class MergingCache<T> : IObjectCache<T>
    where T : notnull
{
    MergingCache()
    {
    }

    public static MergingCache<T> Create(int capacity = 100)
    {
        return new Impl(capacity);
    }

    public abstract T GetCached(T name);

    public abstract void LinkWith(MergingCache<T> cache);

    class Cache(int capacity) : LastAccessCache<T>(capacity)
    {
        public new IEnumerable<T> Contents()
        {
            return base.Contents();
        }

        public new int Count => base.Count;
    }

    class Impl(int capacity) : MergingCache<T>
    {
        Cache _cache = new(capacity);

        public override T GetCached(T name)
        {
            return _cache.GetCached(name);
        }

        public override void LinkWith(MergingCache<T> cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));
                
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
                if (leftCache.Count <= moveThreshold*rightCache.Capacity/100)
                {
                    foreach (var moduleName in leftCache.Contents())
                        rightCache.GetCached(moduleName);
                    if(_trySwap(left, leftCache, rightCache))
                        break;
                }
                else if (rightCache.Count <= moveThreshold*leftCache.Capacity/100)
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

        static bool _trySwap(Impl victim, Cache victimCache, Cache targetCache)
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