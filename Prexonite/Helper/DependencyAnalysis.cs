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

#undef DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prexonite.Types;

namespace Prexonite
{
    /// <summary>
    /// </summary>
    /// <typeparam name = "TKey"></typeparam>
    /// <typeparam name = "TValue"></typeparam>
    public class DependencyAnalysis<TKey, TValue> where
                                                      TValue : class, IDependent<TKey>
    {
        private readonly Dictionary<TKey, Node> _nodes = new Dictionary<TKey, Node>();

        /// <summary>
        ///     Creates a new dependency analysis object from the supplied set of items.
        /// </summary>
        /// <param name = "query">The set of (possibly) interdependent items.</param>
        /// <exception cref = "ArgumentNullException">query is null</exception>
        public DependencyAnalysis(IEnumerable<TValue> query) : this(query, true)
        {
        }

        /// <summary>
        ///     Creates a new dependency analysis object from the supplied set of items.
        /// </summary>
        /// <param name = "query">The set of (possibly) interdependent items.</param>
        /// <param name = "ignoreUnknownDependencies">Indicates whether to ignore dependencies to items not included in the set. Enabled by default.</param>
        /// <exception cref = "ArgumentNullException">query is null</exception>
        /// <exception cref = "ArgumentException">Set contains dependency one element not in the set AND <paramref
        ///      name = "ignoreUnknownDependencies" /> is false.</exception>
        public DependencyAnalysis(IEnumerable<TValue> query, bool ignoreUnknownDependencies)
        {
            if (query == null) throw new ArgumentNullException("query");

            //Add all items
            foreach (var item in query)
                _nodes.Add(item.Name, new Node(item));

            //Find dependencies
            foreach (var node in _nodes.Values)
            {
                var dependencies =
                    _nodes.Keys.Intersect(node.Subject.GetDependencies()).ToLinkedList();
                foreach (var dependencyName in dependencies)
                {
                    Node dependency;
                    if (_nodes.TryGetValue(dependencyName, out dependency))
                    {
                        node.Dependencies.Add(dependency);
                        dependency.Clients.Add(node);
                    }
                    else if (!ignoreUnknownDependencies)
                        throw new ArgumentException("Cannot resolve dependency " + dependencyName +
                            " of " + node.Subject + ".");
                    //else ignore
                }
            }
        }

        public DependencyAnalysis(IEnumerable<PValue> query)
            : this(_acceptPValueSequence(query))
        {
        }

        private static IEnumerable<TValue> _acceptPValueSequence(IEnumerable<PValue> query)
        {
            return from pv in query
                   select (TValue) pv.Value;
        }

        public DependencyAnalysis(IEnumerable<PValue> query, bool ignoreUnknownDependencies)
            : this(_acceptPValueSequence(query), ignoreUnknownDependencies)
        {
        }

        [DebuggerStepThrough]
        internal class SearchEnv
        {
            public readonly Stack<Node> Unassigned = new Stack<Node>();
            public int CurrentDfbi;

            public override string ToString()
            {
                return String.Format("DFBI: {0}; {1}", CurrentDfbi,
                    Unassigned.Select(n => n.Name).ToEnumerationString());
            }
        }

        public IEnumerable<Group> GetMutuallyRecursiveGroups()
        {
            var env = new SearchEnv();

            //Search for groups from each node in turn to reach the whole graph
            foreach (var node in _nodes.Values)
            {
                if (!node.HasBeenVisited)
                    foreach (var group in node._Search(env))
                        yield return group;
            }
        }

        #region Immutable group

        [DebuggerStepThrough, DebuggerDisplay("{Extensions.ToEnumerationString(GetNames())}")]
        public class Group : ExtendableObject, ICollection<Node>
        {
            private readonly LinkedList<Node> _list;

            public Group(LinkedList<Node> list)
            {
                if (list == null) throw new ArgumentNullException("list");
                _list = list;
            }

            public Group(IEnumerable<Node> nodes) : this(nodes.ToLinkedList())
            {
            }

            public void CopyTo(Node[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            bool ICollection<Node>.Remove(Node item)
            {
                throw new NotSupportedException("Dependency analysis groups cannot be modified.");
            }

            public int Count
            {
                get { return _list.Count; }
            }

            public bool IsReadOnly
            {
                get { return ((ICollection<Node>) _list).IsReadOnly; }
            }

            public IEnumerator<Node> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            public IEnumerable<TValue> GetValues()
            {
                return from node in _list
                       select node.Subject;
            }

            public IEnumerable<TKey> GetNames()
            {
                return from node in _list
                       select node.Name;
            }

            public TValue[] ToArray()
            {
                var a = new TValue[_list.Count];
                var index = 0;
                foreach (var node in _list)
                    a[index++] = node.Subject;
                return a;
            }

            void ICollection<Node>.Add(Node item)
            {
                Add(item);
            }

            protected void Add(Node item)
            {
                throw new NotSupportedException("Dependency analysis groups cannot be modified.");
            }

            void ICollection<Node>.Clear()
            {
                Clear();
            }

            protected void Clear()
            {
                throw new NotSupportedException("Dependency analysis groups cannot be modified.");
            }

            public bool Contains(Node item)
            {
                return _list.Contains(item);
            }

            #region Implementation of IEnumerable

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion

        #region Node (nested class)

        [DebuggerStepThrough, DebuggerDisplay("{Subject}")]
        public class Node : ExtendableObject, IEquatable<Node>, INamed<TKey>
        {
            private readonly TValue _subject;
            private readonly HashSet<Node> _dependencies = new HashSet<Node>();
            private readonly HashSet<Node> _clients = new HashSet<Node>();
            private bool _hasBeenVisited, _assignmentPending;
            private int _dfbi, _q;

            public bool HasBeenVisited
            {
                [DebuggerStepThrough]
                get { return _hasBeenVisited; }
            }

            public Node(TValue subject)
            {
                if (subject == null) throw new ArgumentNullException("subject");
                _subject = subject;
            }

            public bool IsDirectlyRecursive
            {
                get { return _dependencies.Contains(this); }
            }

            public HashSet<Node> Dependencies
            {
                [DebuggerStepThrough]
                get { return _dependencies; }
            }

            public HashSet<Node> Clients
            {
                [DebuggerStepThrough]
                get { return _clients; }
            }


            public TValue Subject
            {
                [DebuggerStepThrough]
                get { return _subject; }
            }

            #region Class

            /// <summary>
            ///     Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            ///     true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
            /// </returns>
            /// <param name = "other">An object to compare with this object.</param>
            public bool Equals(Node other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Name, other.Name) && Equals(other._subject, _subject);
            }

            /// <summary>
            ///     Determines whether the specified <see cref = "object" /> is equal to the current <see cref = "object" />.
            /// </summary>
            /// <returns>
            ///     true if the specified <see cref = "object" /> is equal to the current <see cref = "object" />; otherwise, false.
            /// </returns>
            /// <param name = "obj">The <see cref = "object" /> to compare with the current <see cref = "object" />. </param>
            /// <exception cref = "NullReferenceException">The <paramref name = "obj" /> parameter is null.</exception>
            /// <filterpriority>2</filterpriority>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Node)) return false;
                return Equals((Node) obj);
            }

            /// <summary>
            ///     Serves as a hash function for a particular type.
            /// </summary>
            /// <returns>
            ///     A hash code for the current <see cref = "object" />.
            /// </returns>
            /// <filterpriority>2</filterpriority>
            public override int GetHashCode()
            {
                return _subject.GetHashCode();
            }

            public static implicit operator TValue(Node node)
            {
                return node._subject;
            }

            #endregion

            internal IEnumerable<Group> _Search(SearchEnv env)
            {
                _hasBeenVisited = true;
                env.CurrentDfbi++;
                _dfbi = env.CurrentDfbi;
                _q = env.CurrentDfbi;
                env.Unassigned.Push(this);
                _assignmentPending = true;

                foreach (var dep in _dependencies)
                    if (!dep._hasBeenVisited)
                    {
                        foreach (var group in dep._Search(env))
                            yield return group;
                        _q = Math.Min(_q, dep._q);
                    }
                    else if (dep._assignmentPending && dep._dfbi < _dfbi)
                    {
                        _q = Math.Min(_q, dep._dfbi);
                    }

                if (_q == _dfbi)
                {
                    var group = new LinkedList<Node>();
                    Node u;
                    do
                    {
                        u = env.Unassigned.Pop();
                        u._assignmentPending = false;
                        group.AddLast(u);
                    } while (u != this);
                    yield return new Group(group);
                }
            }

            #region Implementation of INamed

            public TKey Name
            {
                get { return _subject.Name; }
            }

            #endregion
        }

        #endregion
    }
}