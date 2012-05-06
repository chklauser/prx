// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal
{
    class DefaultPlan : IPlan
    {
        private readonly List<IResolver> _resolvers = new List<IResolver>(10);
        private readonly HashSet<IBuildWatcher> _buildWatchers = new HashSet<IBuildWatcher>();
        private readonly TargetDescriptionSet _targetDescriptions = TargetDescriptionSet.Create();

        public IList<IResolver> Resolvers
        {
            get { return _resolvers; }
        }

        public ISet<IBuildWatcher> BuildWatchers
        {
            get { return _buildWatchers; }
        }

        public TargetDescriptionSet TargetDescriptions
        {
            get { return _targetDescriptions; }
        }

        public Task Resolve(CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
                {

                }, token);
        }

        protected IEnumerable<ITargetDescription> FindUnresolvedDependencies()
        {
            return TargetDescriptions.Where(HasUnresolvedDependencies);
        }

        protected bool HasUnresolvedDependencies(ITargetDescription description)
        {
            return !description.Dependencies.All(
                pat => TargetDescriptions.Any(other => pat.SatisfiedBy(other.Name)));
        }

        protected void EnsureIsResolved(ITargetDescription description)
        {
            if (HasUnresolvedDependencies(description))
                throw new BuildException(
                    string.Format("Not all dependencies of target named {0} have been resolved.",
                        description.Name), description);
        }

        public Task<ITarget> Build(ITargetDescription targetDescription, CancellationToken token)
        {
            EnsureIsResolved(targetDescription);
            var taskMap =
                new System.Collections.Concurrent.ConcurrentDictionary<ITargetDescription, Task<ITarget>>(5,TargetDescriptions.Count);
            return BuildWithMap(targetDescription, taskMap, token);
        }

        protected Task<IBuildEnvironment> GetBuildEnvironment(Dictionary<ModuleName,Task<ITarget>> dependencies, CancellationToken token)
        {
            return Task.Factory.ContinueWhenAll(dependencies.Values.ToArray(),
                deps => (IBuildEnvironment)
                        new DefaultBuildEnvironment(deps.Select(d => d.Result), token));
        }

        protected Task<ITarget> BuildWithMap(ITargetDescription targetDescription, System.Collections.Concurrent.ConcurrentDictionary<ITargetDescription,Task<ITarget>> taskMap, CancellationToken token)
        {
            return taskMap.GetOrAdd(targetDescription, desc =>
                {
                    var deps =
                        desc.Dependencies.Select(
                            pat =>
                                {
                                    var depDesc =
                                        TargetDescriptions.First(d => pat.SatisfiedBy(d.Name));
                                    return new KeyValuePair<ModuleName, Task<ITarget>>(depDesc.Name,
                                        BuildWithMap(depDesc, taskMap, token));
                                });

                    var depMap = new Dictionary<ModuleName, Task<ITarget>>();
                    depMap.AddRange(deps);

                    token.ThrowIfCancellationRequested();

                    return GetBuildEnvironment(depMap, token)
                        .ContinueWith(bet => desc.Build(bet.Result, depMap, token),token)
                        .Unwrap();
                });
        }
    }
}