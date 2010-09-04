using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Prexonite
{
    [DebuggerNonUserCode]
    public static class Extensions
    {
        public static void Ignore<T>(this T ignored)
        {
        }

        public static Func<TB,TA,TC> Flip<TA,TB,TC>(this Func<TA,TB,TC> func)
        {
            return (b, a) => func(a, b);
        }

        public static TAccum Foldr<TSource, TAccum>(this IEnumerable<TSource> xs,
            Func<TSource, TAccum, TAccum> func,
            TAccum seed)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(xs != null, "xs must not be null.");

            // Don't need aggregate for this
            // ReSharper disable LoopCanBeConvertedToQuery
            IEnumerable<TSource> xsr;
            LinkedList<TSource> xsLinkedList;
            IList<TSource> xsList;
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

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            Contract.Requires(collection != null, "collection must not be null.");

            foreach (var item in items)
                collection.Add(item);
        }

        public  static string ToEnumerationString<T>(this IEnumerable<T> source)
        {
            Contract.Requires(source != null, "source must not be null.");

            var s = new StringBuilder();
            var hasStarted = false;
            string hold = null;
            foreach (var x in source)
            {
                if(hold != null)
                    if(hasStarted)
                    {
                        s.Append(", ");
                        s.Append(hold);
                    }
                    else
                    {
                        s.Append(hold);
                        hasStarted = true;
                    }
                hold = x.ToString();
            }

            if(hasStarted)
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

        public static void MapInPlace<T>(this List<T> source, Func<T,T> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(source != null, "source must not be null.");

            for (var i = 0; i < source.Count; i++)
                source[i] = func(source[i]);
        }

        [DebuggerNonUserCode]
        public  static IEnumerable<T> InReverse<T>(this LinkedList<T> source)
        {
            for(var node = source.Last; node != null; node = node.Previous)
                yield return node.Value;
        }

        [DebuggerNonUserCode]
        public static IEnumerable<T> InReverse<T>(this IList<T> source)
        {
            for (var i = source.Count - 1; i >= 0; i--)
                yield return source[i];
        }

        public static T Foldr1<T>(this IEnumerable<T> xs, Func<T, T, T> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(xs != null, "xs must not be null.");
            

            var seed = default(T);
            var haveSeed = false;
            foreach (var x in xs.Reverse())
            {
                if (haveSeed)
                {
                    seed = func(x, seed);
                }
                else
                {
                    seed = x;
                    haveSeed = true;
                }
            }
            return seed;
        }

        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<LinkedList<T>>() != null);

            var list = new LinkedList<T>();
            foreach (var item in source)
                list.AddLast(item);
            return list;
        }

        /// <summary>
        /// Applies <paramref name="func"/> to every element of <paramref name="source"/> and then filters out any <code>null</code> results.
        /// </summary>
        /// <typeparam name="TSource">The type of the input elements</typeparam>
        /// <typeparam name="TResult">The type of the output elements</typeparam>
        /// <param name="source">The sequence of input elements to map and filter.</param>
        /// <param name="func">The mapping function. <code>null</code> return values will be filtered out and not returned by <see cref="MapMaybe{TSource,TResult}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TResult})"/>.</param>
        /// <returns>The sequence of mapped elements that are not <code>null</code></returns>
        [DebuggerNonUserCode]
        public static IEnumerable<TResult> MapMaybe<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> func) where TResult : class
        {
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(source != null);
            Contract.EndContractBlock();

// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in source)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                var y = func(item);
                if (y != null)
                    yield return y;
            }
        }

        [DebuggerNonUserCode]
        public static IEnumerable<TResult> MapMaybe<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult?> func) where TResult : struct 
        {
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(source != null);
            Contract.EndContractBlock();

// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in source)
            {
                var y = func(item);
                if (y != null)
                    yield return y.Value;
            }
// ReSharper restore LoopCanBeConvertedToQuery
        }

        [DebuggerNonUserCode]
        public static IEnumerable<TResult> Map<TSource, TResult>(this IEnumerable<TSource> source, IDictionary<TSource, TResult> mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException("mapping");
            Contract.Requires(source != null);
            Contract.EndContractBlock();

// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var item in source)
                yield return mapping[item];
// ReSharper restore LoopCanBeConvertedToQuery
        }

        [DebuggerNonUserCode]
        public static IEnumerable<T> MapSome<T>(this IEnumerable<T> source, IDictionary<T, T> mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException("mapping");
            Contract.Requires(source != null);
            Contract.EndContractBlock();
            
            foreach (var item in source)
            {
                T newValue;
                yield return mapping.TryGetValue(item, out newValue) 
                    ? newValue 
                    : item;
            }
        }

        public static T TransformSome<T>(this IDictionary<T,T> mapping, T value)
        {
            Contract.Requires(mapping != null);

            T newValue;
            return mapping.TryGetValue(value, out newValue) ? newValue : value;
        }

        [DebuggerNonUserCode]
        public static IEnumerable<KeyValuePair<TLeft, TRight>> Zip<TLeft, TRight>(
            this IEnumerable<TLeft> ls,
            IEnumerable<TRight> rs)
        {
            return Zip(ls, rs, (l, r) => new KeyValuePair<TLeft, TRight>(l, r));
        }

        [DebuggerNonUserCode]
        public static IEnumerable<TResult> Zip<TLeft, TRight, TResult>(
            this IEnumerable<TLeft> leftHandSide,
            IEnumerable<TRight> rightHandSide,
            Func<TLeft, TRight, TResult> func)
        {
            if (rightHandSide == null)
                throw new ArgumentNullException("rightHandSide");
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(leftHandSide != null);
            Contract.EndContractBlock();

            using (var le = leftHandSide.GetEnumerator())
            using (var re = rightHandSide.GetEnumerator())
                while (le.MoveNext() && re.MoveNext())
                    yield return func(le.Current, re.Current);
        }

        [DebuggerNonUserCode]
        public static void DoZipped<TLeft, TRight>(this IEnumerable<TLeft> leftHandSide,
            IEnumerable<TRight> rightHandSide,
            Action<TLeft, TRight> f)
        {
            if (rightHandSide == null)
                throw new ArgumentNullException("rightHandSide");
            if (f == null)
                throw new ArgumentNullException("f");
            Contract.Requires(leftHandSide != null);
            Contract.EndContractBlock();

            using (var le = leftHandSide.GetEnumerator())
            using (var re = rightHandSide.GetEnumerator())
                while (le.MoveNext() && re.MoveNext())
                    f(le.Current, re.Current);
        }

        public static IEnumerable<T> Singleton<T>(this T element)
        {
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null, "Return value must not be null");
            return new SingletonEnum<T>(element);
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> left,
            IEnumerable<T> right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null, "right must not be null.");
            return left.Concat(right);
        }

        [DebuggerNonUserCode]
        public static IEnumerable<T> Append<T>(this IEnumerable<T> left, T right)
        {
            Contract.Requires(left != null);
            foreach (var item in left)
                yield return item;
            yield return right;
        }

        [DebuggerNonUserCode]
        public static IEnumerable<T> Append<T>(this T left, IEnumerable<T> right)
        {
            yield return left;
            foreach (var item in right)
                yield return item;
        }

        public static TResult[] MapArray<TSource, TResult>(this TSource[] xs,
            Func<TSource, TResult> func)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            Contract.Requires(xs != null);
            Contract.Ensures(Contract.Result<TResult[]>() != null);
            
            Contract.EndContractBlock();


            var result = new TResult[xs.Length];
            for (var i = 0; i < xs.Length; i++)
                result[i] = func(xs[i]);
            return result;
        }

        #region Nested type: SingletonEnum

        private class SingletonEnum<T> : IEnumerable<T>
        {
            private readonly T _element;

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

            private class SingletonEnumerator : IEnumerator<T>
            {
                private readonly T _element;
                private bool _hasNext = true;

                public SingletonEnumerator(T element)
                {
                    _element = element;
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

                public T Current
                {
                    get { return _element; }
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}