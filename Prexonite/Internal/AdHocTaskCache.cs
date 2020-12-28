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
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Prexonite.Internal
{
    /// <summary>
    /// A thread-safe cache for TPL Tasks with support for cancellation.
    /// </summary>
    /// <typeparam name="TKey">The key that decides whether two tasks compute "the same thing" (e.g. path of file to process)</typeparam>
    /// <typeparam name="TResult">The type of results that tasks in this cache compute.</typeparam>
    /// <remarks>
    /// <para>Every work request (submitted via <see cref="GetOrAdd"/>) can come with its own <see cref="CancellationToken"/>. 
    /// You can request that the task managed by the cache be cancelled via this token. 
    /// However, if there were other requests for the same work item (served via the cache), cancellation will not
    /// propagate to the cached task unless <em>all</em> tokens registered for that task are cancelled.</para>
    /// </remarks>
    public class AdHocTaskCache<TKey,TResult>
    {

        private class TaskInfo
        {
            private const int Cancelled = -7171;


            [NotNull]
            public readonly Task<TResult> Task;

            [NotNull] private readonly CancellationTokenSource _cancelSource = new();

            [NotNull] private readonly TKey _key;

            [NotNull]
            private readonly ConcurrentDictionary<TKey, TaskInfo> _cache; 

            private volatile int _liveTokens;

            public TaskInfo([NotNull] ConcurrentDictionary<TKey, TaskInfo> cache, TKey key, [NotNull] Func<CancellationToken, Task<TResult>> taskImplementation)
            {
                _cache = cache;
                _key = key;
                Task = taskImplementation(_cancelSource.Token);
            }

#pragma warning disable 420 // warns about ref parameters will not be treated as volatile by the method called
                            // However, Interlocked.CompareExchange, being a synchronization primitive will
                            // definitely know how to handle this case ;)
            public bool TryAddCancellationConstraint(CancellationToken constraint)
            {
                int oldLiveCount;

                // Check if cancelled and increment as an atomic operation
                do
                {
                    Interlocked.MemoryBarrier();
                    oldLiveCount = _liveTokens;
                    if (oldLiveCount == Cancelled)
                        // The task has been cancelled or is in the process of being cancelled
                        return false;
                    else if(oldLiveCount < 0)
                        // Observed integer overflow, this is illegal as we can no longer guarantee that tasks
                        // stay alive for long enough.
                        throw new OverflowException("Number of tasks cancellation constraints exceeded Int32.MaxValue.");

                } while (Interlocked.CompareExchange(ref _liveTokens, oldLiveCount + 1, oldLiveCount) != oldLiveCount);

                // By incrementing the live token counter, we have already ensured that 
                // the task won't be cancelled prematurely. There is no hurry to add
                // the cancellation handler (it will be called synchronously if we missed the cancellation)
                constraint.Register(_onCancel);

                return true;
            }

            private void _onCancel()
            {
                int oldLiveCount;
                int newLiveCount;

                // Decrement the live token count and check if we hit zero (cancelled the last live token)
                do
                {
                    Interlocked.MemoryBarrier();
                    oldLiveCount = _liveTokens;
                    Debug.Assert(oldLiveCount != Cancelled,
                        "Cancellation handler invoked after task had already been cancelled.");
                    newLiveCount = oldLiveCount - 1;
                    if (newLiveCount == 0)
                    {
                        // To distinguish a cancelled task from an uninitialized task, we
                        //  set the live count to the sentinel value "Cancelled".
                        // The method TryAddCancellationConstraint checks for this value 
                        //  before incrementing the live token count.
                        newLiveCount = Cancelled;
                    }

                } while (Interlocked.CompareExchange(ref _liveTokens, newLiveCount, oldLiveCount) != oldLiveCount);

                // Only one thread will ever reach this state, as only the last thread to cancel
                //  will arrive at live token count 0. See assertion in loop above.
                if (newLiveCount == Cancelled)
                {
                    // ReSharper disable RedundantAssignment
                    var removed = _cache.TryRemove(_key, out var info);
// ReSharper restore RedundantAssignment
                    Debug.Assert(removed, "Removal from task cache by cancellation handler failed.");
                    Debug.Assert(ReferenceEquals(info,this),"Elements in task cache should never be replaced without cancellation.");

                    // This, and only this thread will reach the cancel call. May throw aggregate exceptions.
                    _cancelSource.Cancel();
                }
            }
        }
#pragma warning restore 420

        private readonly ConcurrentDictionary<TKey, TaskInfo> _cache = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resultTask"></param>
        /// <returns></returns>
        [ContractAnnotation("=>true,resultTask:notnull; =>false,resultTask:null")]
        public bool TryGet([NotNull] TKey key, out Task<TResult> resultTask)
        {
            if (_cache.TryGetValue(key, out var info))
            {
                resultTask = info.Task;
                return true;
            }
            else
            {
                resultTask = null;
                return false;
            }
        }

        /// <summary>
        /// Retrieves a task from the cache, or creates a new task if the cache has no task for the supplied <paramref name="key"/>.
        /// This is an atomic operation.
        /// </summary>
        /// <param name="key">A key that identifies this unit of work. For instance the full path of a file to process.</param>
        /// <param name="taskImplementation">The computation to perform. Will only be instantiated as a <see cref="Task"/> in case of a cache miss.</param>
        /// <param name="cancellationToken">The cancellation token for your task.</param>
        /// <remarks>
        /// <para> If a new task is created as a result of this call, then this task is guaranteed to be
        /// in the cache and be returned by the method call.</para>
        /// <para><see cref="GetOrAdd"/> should only be called from a single location in your code 
        /// (per instance of the cache). In particular, you should avoid having multiple possible 
        /// implementations for any single key. You might not be able to control, which <see cref="GetOrAdd"/> 
        /// call arrives first and thus which implementation will end up in the cache. For the 
        /// same reason, instances of <see cref="AdHocTaskCache{TKey,TResult}"/> should not be 
        /// accessible to clients of your code. 
        /// </para>
        /// <para>
        /// If your implementation supports cancellation, <em>it must use the cancellation token 
        /// (<see cref="CancellationToken"/>) provided to it via the second parameter of the
        /// <paramref name="taskImplementation"/> delegate</em>. 
        /// This token will only be triggered if all parties that have an interest in this task have 
        /// requested its  cancellation. If you use your own cancellation token and your token gets cancelled, 
        /// you might inadvertently cancel work that someone else still depends on.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        [NotNull]
        public Task<TResult> GetOrAdd([NotNull] TKey key, [NotNull] Func<TKey, CancellationToken,Task<TResult>> taskImplementation, CancellationToken cancellationToken)
        {
            // This only starts a new task if the cache doesn't already contain a running version of the task
            TaskInfo info;
            TaskInfo taskFactory(TKey actualKey) => new(_cache, actualKey, ct => taskImplementation(actualKey, ct));

            do
            {
                // Atomically get an existing or start a new task for this key
                info = _cache.GetOrAdd(key, taskFactory);

                // Now we need to make sure that our task doesn't get cancelled before our cancellation token is used
                // ( <==> keep task alive as long as our token is not cancelled)

                // This operation can fail if the task in question was in the process of being cancelled but had
                //  not yet been removed from the dictionary. In that case, we can safely retry.
                // If we create a fresh task (the factory was used), we can be sure that this operation will succeed.
            } while (!info.TryAddCancellationConstraint(cancellationToken));

            return info.Task;
        }
    }
}
