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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using Prexonite.Types;

namespace Prexonite.Compiler.Internal
{
    public abstract class MExpr
    {
        private MExpr([NotNull] ISourcePosition position)
        {
            _position = position;
        }

        [NotNull]
        public ISourcePosition Position
        {
            get { return _position; }
        }

        [PublicAPI]
        public abstract void ToString([NotNull] TextWriter writer);

        public override sealed string ToString()
        {
            var writer = new StringWriter();
            ToString(writer);
            return writer.ToString();
        }

        [ContractAnnotation("=>true,args: notnull;=>false,args:canbenull")]
        public abstract bool TryMatchHead(string head, out List<MExpr> args);

        [ContractAnnotation("=>false,value:null;=>true")]
        public abstract bool TryMatchAtom(out object value);

        #region Parsing helper methods

        [ContractAnnotation("=> true, arg: notnull; =>false,arg: canbenull")]
        public bool TryMatchHead(string head, out MExpr arg)
        {
            List<MExpr> args;
            if (TryMatchHead(head, out args) && args.Count == 1)
            {
                arg = args[0];
                return true;
            }
            else
            {
                arg = null;
                return false;
            }
        }

        [ContractAnnotation("=> true, arg: notnull; =>false,arg: null")]
        public bool TryMatchHeadPrefix(string head, out MExpr arg)
        {
            List<MExpr> args;
            if (TryMatchHead(head, out args) && args.Count >= 1)
            {
                arg = args[0];
                return true;
            }
            else
            {
                arg = null;
                return false;
            }
        }

        [ContractAnnotation("=> true, arg1: notnull, arg2: notnull; =>false,arg1: null,arg2: null")]
        public bool TryMatchHeadPrefix(string head, out MExpr arg1, out MExpr arg2)
        {
            List<MExpr> args;
            if (TryMatchHead(head, out args) && args.Count >= 2)
            {
                arg1 = args[0];
                arg2 = args[1];
                return true;
            }
            else
            {
                arg1 = arg2 = null;
                return false;
            }
        }

        [ContractAnnotation("=> true, arg1: notnull, arg2: notnull; =>false,arg1: null,arg2: null")]
        public bool TryMatchHead(string head, out MExpr arg1, out MExpr arg2)
        {
            List<MExpr> args;
            if (TryMatchHead(head, out args) && args.Count == 2)
            {
                arg1 = args[0];
                arg2 = args[1];
                return true;
            }
            else
            {
                arg1 = arg2 = null;
                return false;
            }
        }

        [ContractAnnotation("=> true, arg1: notnull, arg2: notnull; =>false,arg1: null,arg2: null")]
        public bool TryMatchHeadPrefix(string head, out MExpr arg1, out MExpr arg2, out MExpr arg3)
        {
            List<MExpr> args;
            if (TryMatchHead(head, out args) && args.Count >= 3)
            {
                arg1 = args[0];
                arg2 = args[1];
                arg3 = args[2];
                return true;
            }
            else
            {
                arg1 = arg2 = arg3 = null;
                return false;
            }
        }

        [ContractAnnotation("=> true, arg1: notnull, arg2: notnull, arg3: notnull; =>false,arg1: null,arg2: null, arg3: notnull")]
        public bool TryMatchHead(string head, out MExpr arg1, out MExpr arg2, out MExpr arg3)
        {
            List<MExpr> args;
            if (TryMatchHead(head, out args) && args.Count == 3)
            {
                arg1 = args[0];
                arg2 = args[1];
                arg3 = args[2];
                return true;
            }
            else
            {
                arg1 = arg2 = arg3 = null;
                return false;
            }
        }

        [ContractAnnotation("=>true,value: notnull;=>false,value:null")]
        public bool TryMatchStringAtom(out string value)
        {
            object raw;
            if (TryMatchAtom(out raw) && (value = raw as string) != null)
            {
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool TryMatchIntAtom(out int value)
        {
            object raw;
            if (TryMatchAtom(out raw) && raw is int)
            {
                value = (int) raw;
                return true;
            }
            else
            {
                value = default(int);
                return false;
            }
        }

        public bool TryMatchVersionAtom(out Version version)
        {
            object raw;
            if (TryMatchAtom(out raw) && (version = raw as Version) != null)
            {
                return true;
            }
            else
            {
                version = null;
                return false;
            }
        }


        #endregion
        
        [NotNull]
        private readonly ISourcePosition _position;

        public sealed class MAtom : MExpr, IEquatable<MAtom>
        {
            [CanBeNull]
            private readonly Object _value;

            public MAtom([NotNull] ISourcePosition position, Object value) : base(position)
            {
                _value = value;
            }

            [CanBeNull]
            public object Value
            {
                get { return _value; }
            }

            public override void ToString(TextWriter writer)
            {
                String str;
                if (Value == null)
                    writer.Write("null");
                else if ((str = Value as string) != null)
                {
                    writer.Write("\"");
                    StringPType.Escape(str, writer);
                    writer.Write("\"");
                }
                else
                    writer.Write(Value);
            }

            public override bool TryMatchHead(string head, out List<MExpr> args)
            {
                args = null;
                return false;
            }

            public override bool TryMatchAtom(out object value)
            {
                value = _value;
                return true;
            }

            public bool Equals(MAtom other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is MAtom && Equals((MAtom) obj);
            }

            public override int GetHashCode()
            {
                return (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public sealed class MList : MExpr, IEquatable<MList>
        {
            [NotNull]
            private readonly string _head;
            [NotNull]
            private readonly List<MExpr> _args;

            public MList([NotNull] ISourcePosition position, [NotNull] string head, [NotNull] IEnumerable<MExpr> args) : base(position)
            {
                if (head == null) throw new ArgumentNullException("head");
                if (args == null) throw new ArgumentNullException("args");
                _head = head;
                _args = new List<MExpr>(args);
            }

            public MList([NotNull] ISourcePosition position, [NotNull] String head, MExpr arg) : base(position)
            {
                if (head == null) throw new ArgumentNullException("head");
                if (arg == null) throw new ArgumentNullException("arg");
                _head = head;
                _args = new List<MExpr>(1) {arg};
            }

            public MList([NotNull] ISourcePosition position, [NotNull] String head)
                : base(position)
            {
                if (head == null) throw new ArgumentNullException("head");
                _head = head;
                _args = new List<MExpr>();
            }

            public MList([NotNull] ISourcePosition position, [NotNull] String head, Object arg) : base(position)
            {
                if (head == null) throw new ArgumentNullException("head");
                if (arg == null) throw new ArgumentNullException("arg");
                _head = head;
                _args = new List<MExpr>(1) { new MAtom(position, arg) };
            }

            public override void ToString(TextWriter builder)
            {
                builder.Write(_head);
                switch (_args.Count)
                {
                    case 0:
                        break;
                    case 1:
                        builder.Write(" ");
                        builder.Write(_args[0]);
                        break;
                    default:
                        {
                            builder.Write("(");
                            var first = true;
                            foreach (var mExpr in _args)
                            {
                                if (!first)
                                    builder.Write(",");
                                else
                                    first = false;
                                Debug.Assert(mExpr != null,"MExpr contains null element in list.");
                                mExpr.ToString(builder);
                            }
                            builder.Write(")");
                        }
                        break;
                }
            }

            public override bool TryMatchHead(string head, out List<MExpr> args)
            {
                if (_head.Equals(head))
                {
                    args = _args;
                    return true;
                }
                else
                {
                    args = null;
                    return false;
                }
            }

            public override bool TryMatchAtom(out object value)
            {
                value = null;
                return false;
            }

            public bool Equals(MList other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return String.Equals(_head, other._head) && _args.Equals(other._args);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is MList && Equals((MList) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_head.GetHashCode()*397) ^ _args.GetHashCode();
                }
            }
        }

    }
}