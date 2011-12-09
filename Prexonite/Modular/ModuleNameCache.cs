// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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

using System;
using System.Collections.Generic;
using System.Threading;
using Prexonite;
using Prexonite.Helper;

namespace Prexonite.Modular
{
    public interface IModuleNameCache : IObjectCache<ModuleName>
    {
        void Link(IModuleNameCache cache);
        ModuleName Create(string id, Version version);
        ModuleNameCache ToModuleNameCache();
    }

    public abstract class ModuleNameCache : IModuleNameCache
    {
        private ModuleNameCache()
        {
        }

        public static ModuleNameCache Create(int capacity = 100)
        {
            return new Impl(capacity);
        }

        public abstract ModuleName GetCached(ModuleName name);

        public abstract void Link(IModuleNameCache cache);

        public ModuleName Create(string id, Version version)
        {
            return GetCached(new ModuleName(id, version));
        }

        public ModuleNameCache ToModuleNameCache()
        {
            return this;
        }

        private class Cache : LastAccessCache<ModuleName>
        {
            public Cache(int capacity) : base(capacity)
            {
            }

            public new IEnumerable<ModuleName> Contents()
            {
                return base.Contents();
            }

            public new int Count
            {
                get { return base.Count; }
            }
        }

        private class Impl : ModuleNameCache
        {
            private Cache _cache;

            public Impl(int capacity)
            {
                _cache = new Cache(capacity);
            }

            public override ModuleName GetCached(ModuleName name)
            {
                return _cache.GetCached(name);
            }

            public override void Link(IModuleNameCache cache)
            {
                if (cache == null)
                    throw new ArgumentNullException("cache");
                
                Merge(this, (Impl) cache.ToModuleNameCache());
            }

            public static void Merge(Impl left, Impl right)
            {
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
    }
}