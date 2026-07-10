using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Prexonite;

public static class Extensions
{
    extension<T>(T ignored)
    {
        public void Ignore() { }
    }

    extension<TA, TB, TC>(Func<TA, TB, TC> func)
    {
        public Func<TB, TA, TC> Flip()
        {
            return (b, a) => func(a, b);
        }
    }

    extension<TSource>(IEnumerable<TSource> xs)
    {
        public TAccum Foldr<TAccum>(Func<TSource, TAccum, TAccum> func, TAccum seed)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            // Don't need aggregate for this
            // ReSharper disable LoopCanBeConvertedToQuery
            IEnumerable<TSource> xsr;
            LinkedList<TSource>? xsLinkedList;
            IList<TSource>? xsList;
            if ((xsLinkedList = xs as LinkedList<TSource>) != null)
                xsr = xsLinkedList.InReverse();
            else if ((xsList = xs as IList<TSource>) != null)
                xsr = xsList.InReverse();
            else
                xsr = xs.Reverse();

            foreach (var x in xsr)
            {
                seed = func(x, seed);
            }
            return seed;
            // ReSharper restore LoopCanBeConvertedToQuery
        }

        /// <summary>
        /// Constructs a human-readable enumeration of the form "x<sub>1</sub>, x<sub>2</sub>, x<sub>3</sub>, …, x<sub>n-2</sub>, x<sub>n-1</sub>, and x<sub>n</sub>".
        /// </summary>
        /// <typeparam name="T">Any type that supports <see cref="Object.ToString"/>.</typeparam>
        /// <param name="source">The enumeration to convert to a string.</param>
        /// <returns>A human-readable string.</returns>
        public string? ToEnumerationString()
        {
            var s = new StringBuilder();
            var hasStarted = false;
            string? hold = null;
            foreach (var x in xs)
            {
                if (hold != null)
                    if (hasStarted)
                    {
                        s.Append(", ");
                        s.Append(hold);
                    }
                    else
                    {
                        s.Append(hold);
                        hasStarted = true;
                    }
                hold = x?.ToString() ?? "";
            }

            if (hasStarted)
            {
                s.Append(" and ");
                s.Append(hold);
                return s.ToString();
            }
            else
            {
                return hold;
            }
        }

        /// <summary>
        /// Constructs a machine-readable, comma-separated list. ("x<sub>1</sub>, x<sub>2</sub>, …, x<sub>n</sub>").
        /// </summary>
        /// <typeparam name="T">Any type that supports <see cref="Object.ToString"/>.</typeparam>
        /// <param name="source">The enumeration to convert to a string.</param>
        /// <returns>A human-readbale string. The empty string iff <paramref name="source"/> was empty.</returns>
        public string ToListString()
        {
            var s = new StringBuilder();
            var hasStarted = false;
            foreach (var x in xs)
            {
                if (hasStarted)
                {
                    s.Append(", ");
                }
                else
                {
                    hasStarted = true;
                }
                s.Append(x);
            }
            return s.ToString();
        }

        public TSource? Foldr1(Func<TSource, TSource, TSource> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var seed = default(TSource);
            var haveSeed = false;
            foreach (var x in xs.Reverse())
            {
                if (haveSeed)
                {
                    seed = func(x, seed!);
                }
                else
                {
                    seed = x;
                    haveSeed = true;
                }
            }
            return seed;
        }

        public LinkedList<TSource> ToLinkedList()
        {
            var list = new LinkedList<TSource>();
            foreach (var item in xs)
                list.AddLast(item);
            return list;
        }

        /// <summary>
        ///     Applies <paramref name = "func" /> to every element of <paramref name = "source" /> and then filters out any <code>null</code> results.
        /// </summary>
        /// <typeparam name = "TSource">The type of the input elements</typeparam>
        /// <typeparam name = "TResult">The type of the output elements</typeparam>
        /// <param name = "source">The sequence of input elements to map and filter.</param>
        /// <param name = "func">The mapping function. <code>null</code> return values will be filtered out and not returned by <see
        ///      cref = "MapMaybe{TSource,TResult}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TResult})" />.</param>
        /// <returns>The sequence of mapped elements that are not <code>null</code></returns>
        [DebuggerNonUserCode]
        public IEnumerable<TResult> MapMaybe<TResult>(Func<TSource, TResult?> func)
            where TResult : class
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in xs)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var y = func(item);
                if (y != null)
                    yield return y;
            }
        }

        [DebuggerNonUserCode]
        public IEnumerable<TResult> SelectMaybe<TResult>(Func<TSource, TResult?> func)
            where TResult : struct
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in xs)
            {
                var y = func(item);
                if (y != null)
                    yield return y.Value;
            }
            // ReSharper restore LoopCanBeConvertedToQuery
        }

        [DebuggerNonUserCode]
        public IEnumerable<TResult> SelectMaybe<TResult>(Func<TSource, TResult?> func)
            where TResult : class
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in xs)
            {
                var y = func(item!);
                if (y != null)
                    yield return y;
            }
            // ReSharper restore LoopCanBeConvertedToQuery
        }

        [DebuggerNonUserCode]
        public IEnumerable<TResult> Zip<TRight, TResult>(
            IEnumerable<TRight> rightHandSide,
            Func<TSource, TRight, TResult> func
        )
        {
            if (rightHandSide == null)
                throw new ArgumentNullException(nameof(rightHandSide));
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using var le = xs.GetEnumerator();
            using var re = rightHandSide.GetEnumerator();
            while (le.MoveNext() && re.MoveNext())
                yield return func(le.Current, re.Current);
        }

        [DebuggerNonUserCode]
        public void DoZipped<TRight>(IEnumerable<TRight> rightHandSide, Action<TSource, TRight> f)
        {
            if (rightHandSide == null)
                throw new ArgumentNullException(nameof(rightHandSide));
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            using var le = xs.GetEnumerator();
            using var re = rightHandSide.GetEnumerator();
            while (le.MoveNext() && re.MoveNext())
                f(le.Current, re.Current);
        }

        public IEnumerable<TSource> Append(IEnumerable<TSource> right)
        {
            return xs.Concat(right);
        }

        [DebuggerNonUserCode]
        public IDictionary<TK, TC> ToGroupedDictionary<TK, TC>(Func<TSource, TK> keySelector)
            where TK : notnull
            where TC : ICollection<TSource>, new()
        {
            var dict = new Dictionary<TK, TC>();
            foreach (var group in xs.GroupBy(keySelector))
            {
                var xsCollection = new TC();
                foreach (var item in group)
                    xsCollection.Add(item);
                dict.Add(group.Key, xsCollection);
            }
            return dict;
        }

        /// <summary>
        /// An efficient version of except when the exception set is already implemented as a set.
        /// </summary>
        /// <typeparam name="T">Type of the items in the sequence/set. Should support efficient hash-based equality comparison.</typeparam>
        /// <param name="sequence">The sequence of candidate values to be filtered.</param>
        /// <param name="exceptionSet">The set of elements to be excluded from the sequence.</param>
        /// <returns>The original sequence with all elements also in the <paramref name="exceptionSet"/> removed.</returns>
        /// <remarks><para>This is a streaming operator. The order of elements in the sequence is not changed. Multiple instances of the same element in the input sequence are retained (if they are not filtered out).</para></remarks>
        public IEnumerable<TSource> Except(ISet<TSource> exceptionSet)
        {
            return xs.Where(x => !exceptionSet.Contains(x));
        }

        public IEnumerable<TSource> WithActionAfter(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in xs)
                yield return item;
            action();
        }
    }

    extension<T>(ReadOnlySpan<T> source)
    {
        /// <summary>
        /// Constructs a human-readable enumeration of the form "x<sub>1</sub>, x<sub>2</sub>, x<sub>3</sub>, …, x<sub>n-2</sub>, x<sub>n-1</sub>, and x<sub>n</sub>".
        /// </summary>
        /// <typeparam name="T">Any type that supports <see cref="Object.ToString"/>.</typeparam>
        /// <param name="source">The enumeration to convert to a string.</param>
        /// <returns>A human-readable string.</returns>
        public string? ToEnumerationString()
        {
            var s = new StringBuilder();
            var hasStarted = false;
            string? hold = null;
            foreach (var x in source)
            {
                if (hold != null)
                    if (hasStarted)
                    {
                        s.Append(", ");
                        s.Append(hold);
                    }
                    else
                    {
                        s.Append(hold);
                        hasStarted = true;
                    }
                hold = x?.ToString() ?? "";
            }

            if (hasStarted)
            {
                s.Append(" and ");
                s.Append(hold);
                return s.ToString();
            }
            else
            {
                return hold;
            }
        }
    }

    extension<T>(List<T> source)
    {
        public void MapInPlace(Func<T, T> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            for (var i = 0; i < source.Count; i++)
                source[i] = func(source[i]);
        }
    }

    extension<T>(LinkedList<T> source)
    {
        public IEnumerable<T> InReverse()
        {
            for (var node = source.Last; node != null; node = node.Previous)
                yield return node.Value;
        }

        public IEnumerable<LinkedListNode<T>> ToNodeSequence()
        {
            if (source.Count == 0)
                yield break;

            var node = source.First;
            while (node != null)
            {
#if !DEBUG
                yield return node;
#else
                var prev = node.Previous;
                var next = node.Next;
                yield return node;
                Debug.Assert(
                    ReferenceEquals(node.Next, next),
                    "Linked list has changed while enumerating over elements. (next node)"
                );
                Debug.Assert(
                    ReferenceEquals(node.Previous, prev),
                    "Linked list has changed while enumerating over elements. (prev node)"
                );
#endif
                node = node.Next;
            }
        }
    }

    extension<T>(IList<T> source)
    {
        public IEnumerable<T> InReverse()
        {
            for (var i = source.Count - 1; i >= 0; i--)
                yield return source[i];
        }
    }

    extension<T>(T element)
    {
        public IEnumerable<T> Singleton()
        {
            return new SingletonEnum<T>(element);
        }
    }

    extension<T>(ICollection<T> collection)
    {
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
                collection.Add(item);
        }
    }

    #region Nested type: SingletonEnum

    class SingletonEnum<T> : IEnumerable<T>
    {
        readonly T _element;

        public SingletonEnum(T element)
        {
            _element = element;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new SingletonEnumerator(_element);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Nested type: SingletonEnumerator

        class SingletonEnumerator : IEnumerator<T>
        {
            bool _hasNext = true;

            public SingletonEnumerator(T element)
            {
                Current = element;
            }

            #region IEnumerator<T> Members

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public bool MoveNext()
            {
                var t = _hasNext;
                _hasNext = !_hasNext;
                return t;
            }

            public void Reset()
            {
                _hasNext = true;
            }

            public T Current { get; }

            object? IEnumerator.Current => Current;

            #endregion
        }

        #endregion
    }

    #endregion

    [DebuggerNonUserCode]
    public static IEnumerable<T> Append<T>(IEnumerable<T> left, T right)
    {
        foreach (var item in left)
            yield return item;
        yield return right;
    }

    [DebuggerNonUserCode, Obsolete("Functionality now included in framework.")]
    public static IEnumerable<T> Append<T>(T left, IEnumerable<T> right)
    {
        yield return left;
        foreach (var item in right)
            yield return item;
    }
}
