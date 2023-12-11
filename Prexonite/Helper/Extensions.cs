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

using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Prexonite;

[DebuggerNonUserCode]
public static class Extensions
{
    public static void Ignore<T>(this T ignored)
    {
    }

    public static Func<TB, TA, TC> Flip<TA, TB, TC>(this Func<TA, TB, TC> func)
    {
        return (b, a) => func(a, b);
    }

    public static TAccum Foldr<TSource, TAccum>(this IEnumerable<TSource> xs,
        Func<TSource, TAccum, TAccum> func,
        TAccum seed)
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

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        foreach (var item in items)
            collection.Add(item);
    }

    /// <summary>
    /// Constructs a human-readable enumeration of the form "x<sub>1</sub>, x<sub>2</sub>, x<sub>3</sub>, …, x<sub>n-2</sub>, x<sub>n-1</sub>, and x<sub>n</sub>".
    /// </summary>
    /// <typeparam name="T">Any type that supports <see cref="Object.ToString"/>.</typeparam>
    /// <param name="source">The enumeration to convert to a string.</param>
    /// <returns>A human-readable string.</returns>
    public static string? ToEnumerationString<T>(this IEnumerable<T> source)
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

    /// <summary>
    /// Constructs a machine-readable, comma-separated list. ("x<sub>1</sub>, x<sub>2</sub>, …, x<sub>n</sub>").
    /// </summary>
    /// <typeparam name="T">Any type that supports <see cref="Object.ToString"/>.</typeparam>
    /// <param name="source">The enumeration to convert to a string.</param>
    /// <returns>A human-readbale string. The empty string iff <paramref name="source"/> was empty.</returns>
    public static string ToListString<T>(this IEnumerable<T> source)
    {
        var s = new StringBuilder();
        var hasStarted = false;
        foreach (var x in source)
        {
            if(hasStarted)
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

    public static void MapInPlace<T>(this List<T> source, Func<T, T> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        for (var i = 0; i < source.Count; i++)
            source[i] = func(source[i]);
    }

    [DebuggerNonUserCode]
    public static IEnumerable<T> InReverse<T>(this LinkedList<T> source)
    {
        for (var node = source.Last; node != null; node = node.Previous)
            yield return node.Value;
    }

    [DebuggerNonUserCode]
    public static IEnumerable<T> InReverse<T>(this IList<T> source)
    {
        for (var i = source.Count - 1; i >= 0; i--)
            yield return source[i];
    }

    public static T? Foldr1<T>(this IEnumerable<T> xs, Func<T, T, T> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        var seed = default(T);
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

    public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
    {
        var list = new LinkedList<T>();
        foreach (var item in source)
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
    public static IEnumerable<TResult> MapMaybe<TSource, TResult>(
        this IEnumerable<TSource> source, Func<TSource, TResult?> func) where TResult : class
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

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
    public static IEnumerable<TResult> SelectMaybe<TSource, TResult>(
        this IEnumerable<TSource> source, Func<TSource, TResult?> func) where TResult : struct
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

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
    public static IEnumerable<TResult> SelectMaybe<TSource, TResult>(
        this IEnumerable<TSource> source, Func<TSource, TResult?> func) where TResult : class
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        // ReSharper disable LoopCanBeConvertedToQuery
        foreach (var item in source)
        {
            var y = func(item!);
            if (y != null)
                yield return y;
        }
        // ReSharper restore LoopCanBeConvertedToQuery
    }

    [DebuggerNonUserCode]
    public static IEnumerable<TResult> Zip<TLeft, TRight, TResult>(
        this IEnumerable<TLeft> leftHandSide,
        IEnumerable<TRight> rightHandSide,
        Func<TLeft, TRight, TResult> func)
    {
        if (rightHandSide == null)
            throw new ArgumentNullException(nameof(rightHandSide));
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        using var le = leftHandSide.GetEnumerator();
        using var re = rightHandSide.GetEnumerator();
        while (le.MoveNext() && re.MoveNext())
            yield return func(le.Current, re.Current);
    }

    [DebuggerNonUserCode]
    public static void DoZipped<TLeft, TRight>(this IEnumerable<TLeft> leftHandSide,
        IEnumerable<TRight> rightHandSide,
        Action<TLeft, TRight> f)
    {
        if (rightHandSide == null)
            throw new ArgumentNullException(nameof(rightHandSide));
        if (f == null)
            throw new ArgumentNullException(nameof(f));

        using var le = leftHandSide.GetEnumerator();
        using var re = rightHandSide.GetEnumerator();
        while (le.MoveNext() && re.MoveNext())
            f(le.Current, re.Current);
    }

    public static IEnumerable<T> Singleton<T>(this T element)
    {
        return new SingletonEnum<T>(element);
    }

    public static IEnumerable<T> Append<T>(this IEnumerable<T> left,
        IEnumerable<T> right)
    {
        return left.Concat(right);
    }

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

    [DebuggerNonUserCode]
    public static IDictionary<TK, TC> ToGroupedDictionary<TK, TV, TC>(this IEnumerable<TV> items, Func<TV, TK> keySelector)
        where TK : notnull
        where TC : ICollection<TV>, new()
    {
        var dict = new Dictionary<TK, TC>();
        foreach (var group in items.GroupBy(keySelector))
        {
            var xs = new TC();
            foreach (var item in group)
                xs.Add(item);
            dict.Add(group.Key, xs);
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
    public static IEnumerable<T> Except<T>(this IEnumerable<T> sequence, ISet<T> exceptionSet)
    {
        return sequence.Where(x => !exceptionSet.Contains(x));
    }

    public static IEnumerable<T> WithActionAfter<T>(this IEnumerable<T> sequence, Action action)
    {
        if (sequence == null)
            throw new ArgumentNullException(nameof(sequence));

        if (action == null)
            throw new ArgumentNullException(nameof(action));
            
        foreach (var item in sequence)
            yield return item;
        action();
    }

    public static IEnumerable<LinkedListNode<T>> ToNodeSequence<T>(this LinkedList<T> list)
    {
        if(list.Count == 0)
            yield break;

        var node = list.First;
        while(node != null)
        {
#if !DEBUG
            yield return node;
#else
                var prev = node.Previous;
                var next = node.Next;
                yield return node;
                Debug.Assert(ReferenceEquals(node.Next,next),"Linked list has changed while enumerating over elements. (next node)");
                Debug.Assert(ReferenceEquals(node.Previous, prev), "Linked list has changed while enumerating over elements. (prev node)");
#endif
            node = node.Next;
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
}