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
#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Prexonite.Compiler.Cil;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral(Literal)]
    public class HashPType : PType, ICilCompilerAware
    {
        #region Singleton

        private HashPType()
        {
        }

        private static readonly HashPType _instance = new HashPType();

        public static HashPType Instance
        {
            get { return _instance; }
        }

        #endregion

        #region PType Interface

        private static bool _tryConvertToPair(
            StackContext sctx, PValue inpv, out PValueKeyValuePair result)
        {
            PValue res;
            result = null;
            if (!inpv.TryConvertTo(sctx, typeof (PValueKeyValuePair), out res))
                return false;
            else
                result = (PValueKeyValuePair) res.Value;
            return true;
        }

        public override bool IndirectCall(
            StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            if (args == null)
                args = new PValue[] {};

            result = null;

            var argc = args.Length;

            var pvht = (PValueHashtable) subject.Value;

            if (argc == 0)
            {
                result =
                    sctx.CreateNativePValue(new PValueEnumeratorWrapper(pvht.GetPValueEnumerator()));
            }
            else if (argc == 1)
            {
                if (!pvht.TryGetValue(args[0], out result))
                    result = false;
            }
            else
            {
                pvht.AddOverride(args[0], args[1]);
            }

            return result != null;
        }

        public override bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            var pvht = subject.Value as PValueHashtable;

            if (pvht == null)
                throw new ArgumentException("Subject must be a Hash.");

            result = null;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = Null.CreatePValue();
            }

            var argc = args.Length;

            switch (id.ToLowerInvariant())
            {
                case "":
                    if (call == PCall.Get && argc > 0)
                    {
                        var key = args[0];
                        if (pvht.ContainsKey(key))
                            result = pvht[key];
                        else
                            result = Null.CreatePValue();
                    }
                    else if (call == PCall.Set)
                    {
                        if (argc > 1)
                        {
                            pvht.AddOverride(args[0], args[1]);
                            result = Null.CreatePValue();
                        }
                        else if (argc == 1)
                        {
                            goto case "add";
                        }
                    }
                    break;

                case "add":
                    if (argc == 1)
                    {
                        PValueKeyValuePair pair;

                        result = Null.CreatePValue();

                        if (args[0].IsNull)
                        {
                        } //Ignore this one
                        else if (_tryConvertToPair(sctx, args[0], out pair))
                            pvht.AddOverride(pair);
                    }
                    else if (argc > 1)
                    {
                        pvht.AddOverride(args[0], args[1]);
                        result = Null.CreatePValue();
                    }
                    break;

                case "clear":
                    pvht.Clear();
                    result = Null.CreatePValue();
                    break;

                case "containskey":
                    if (argc == 1)
                    {
                        result = pvht.ContainsKey(args[0]);
                    }
                    else if (argc > 1)
                    {
                        var found = true;
                        foreach (var arg in args)
                        {
                            if (!pvht.ContainsKey(arg))
                            {
                                found = false;
                                break;
                            }
                        }
                        result = found;
                    }
                    break;

                case "containsvalue":
                    if (argc == 1)
                    {
                        result = pvht.ContainsValue(args[0]);
                    }
                    else if (argc > 1)
                    {
                        var found = true;
                        foreach (var arg in args)
                        {
                            if (!pvht.ContainsValue(arg))
                            {
                                found = false;
                                break;
                            }
                        }
                        result = found;
                    }
                    break;

                case "count":
                case "length":
                    result = pvht.Count;
                    break;

                case "getenumerator":
                    result =
                        Object.CreatePValue(new PValueEnumeratorWrapper(pvht.GetPValueEnumerator()));
                    break;

                case "gethashcode":
                    result = pvht.GetHashCode();
                    break;

                case "gettype":
                    result = Object.CreatePValue(typeof (PValueHashtable));
                    break;

                case "keys":
                    result = List.CreatePValue(new List<PValue>(pvht.Keys));
                    break;

                case "remove":
                    if (argc == 1)
                    {
                        result = pvht.Remove(args[0]);
                    }
                    else if (argc > 1)
                    {
                        var removed = new List<PValue>(pvht.Count);
                        foreach (var arg in args)
                            removed.Add(pvht.Remove(arg));

                        result = List.CreatePValue(removed);
                    }
                    break;

                case "tostring":
                    var sb = new StringBuilder("{ ");
                    foreach (var pair in pvht)
                    {
                        sb.Append(pair.Key.CallToString(sctx));
                        sb.Append(": ");
                        sb.Append(pair.Value.CallToString(sctx));
                        sb.Append(", ");
                    }
                    if (pvht.Count > 0)
                        sb.Length -= 2;
                    sb.Append(" }");
                    result = sb.ToString();
                    break;

                case "trygetvalue":
                    if (argc >= 2)
                    {
                        PValue value;
                        if (pvht.TryGetValue(args[0], out value))
                        {
                            args[1].IndirectCall(sctx, new[] {value});
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    break;
                case "values":
                    result = List.CreatePValue(new List<PValue>(pvht.Values));
                    break;

                default:
                    return
                        PValueHashtable.ObjectType.TryDynamicCall(
                            sctx, subject, args, call, id, out result);
            }

            return result != null;
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            result = null;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = Null.CreatePValue();
            }

            PValueHashtable pvht;

            switch (id.ToLowerInvariant())
            {
                case "create":
                    //Create(params KeyValuePair[] pairs)
                    pvht = new PValueHashtable(args.Length);
                    foreach (var arg in args)
                    {
                        PValueKeyValuePair pairArg;
                        if (_tryConvertToPair(sctx, arg, out pairArg))
                            pvht.AddOverride(pairArg);
                    }
                    result = new PValue(pvht, this);
                    break;

                case "createFromArgs":
                    if (args.Length%2 != 0)
                        break;
                    pvht = new PValueHashtable(args.Length/2);
                    for (var i = 0; i < args.Length; i += 2)
                        pvht.AddOverride(args[i], args[i + 1]);
                    result = new PValue(pvht, this);
                    break;

                default:
                    return
                        PValueHashtable.ObjectType.TryStaticCall(sctx, args, call, id, out result);
            }

            return result != null;
        }

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            if (args == null)
                args = new PValue[] {};

            result = null;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = Null.CreatePValue();
            }

            var argc = args.Length;
            PValueHashtable pvht = null;

            if (argc == 0)
            {
                pvht = new PValueHashtable();
            }
            else if (args[0].IsNull)
            {
                pvht = new PValueHashtable();
            }
            else if (argc > 0)
            {
                var arg0 = args[0];
                if (arg0.Type == Hash ||
                    (arg0.Type is ObjectPType && arg0.Value is IDictionary<PValue, PValue>))
                {
                    pvht = new PValueHashtable((IDictionary<PValue, PValue>) arg0.Value);
                }
                else if (arg0.Type == Int)
                {
                    pvht = new PValueHashtable((int) arg0.Value);
                }
            }

            if (pvht != null)
                result = new PValue(pvht, this);

            return result != null;
        }

        protected override bool InternalConvertTo(
            StackContext sctx, PValue subject, PType target, bool useExplicit, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            if ((object) target == null)
                throw new ArgumentNullException(nameof(target));

            var pvht = subject.Value as PValueHashtable;

            if (pvht == null)
                throw new ArgumentException("Subject must be a Hash.");

            result = null;

            if (target is ObjectPType)
            {
                var tT = ((ObjectPType) target).ClrType;
                if (tT == typeof (IDictionary<PValue, PValue>) ||
                    tT == typeof (Dictionary<PValue, PValue>) ||
                        tT == typeof (IDictionary) ||
                            tT == typeof (IEnumerable<KeyValuePair<PValue, PValue>>) ||
                                tT == typeof (IEnumerable) ||
                                    tT == typeof (ICollection<KeyValuePair<PValue, PValue>>) ||
                                        tT == typeof (ICollection))
                {
                    result = new PValue(pvht, target);
                }
                else if (tT == typeof (IEnumerable<PValue>) || tT == typeof (IList<PValue>) ||
                    tT == typeof (IList))
                {
                    var lst = new List<PValue>(pvht.Count);
                    foreach (var pair in pvht)
                        lst.Add(sctx.CreateNativePValue(new PValueKeyValuePair(pair)));
                    result = new PValue(lst, target);
                }
            }
            else if (target is ListPType)
            {
                var lst = new List<PValue>(pvht.Count);
                foreach (var pair in pvht)
                    lst.Add(sctx.CreateNativePValue(new PValueKeyValuePair(pair.Key, pair.Value)));
                result = List.CreatePValue(lst);
            }

            return result != null;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx, PValue subject, bool useExplicit, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException(nameof(sctx));
            if (subject == null)
                throw new ArgumentNullException(nameof(subject));

            result = null;
            PValueHashtable pvht = null;

            var sT = subject.Type;

            if (sT is ObjectPType)
            {
                var os = subject.Value;
                var o_pvht = os as PValueHashtable;
                if (o_pvht != null)
                    pvht = o_pvht;
                else
                {
                    var id = os as IDictionary<PValue, PValue>;
                    if (id != null)
                        pvht = new PValueHashtable(id);
                    else
                    {
                        var pvkvp = os as PValueKeyValuePair;
                        if (pvkvp != null)
                        {
                            pvht = new PValueHashtable(1);
                            pvht.Add(pvkvp);
                        }
                        else if (os is KeyValuePair<PValue, PValue>)
                        {
                            pvht = new PValueHashtable(1);
                            pvht.Add((KeyValuePair<PValue, PValue>) os);
                        }
                    }
                }
            }
            else if (sT == Null)
                pvht = new PValueHashtable();

            if (pvht != null)
                result = new PValue(pvht, this);

            return result != null;
        }

        public override bool Addition(StackContext sctx, PValue leftOperand, PValue rightOperand,
            out PValue result)
        {
            result = null;

            if (leftOperand.Type is HashPType && rightOperand.Type is HashPType)
            {
                var pvht1 = (PValueHashtable) leftOperand.Value;
                var pvht2 = (PValueHashtable) rightOperand.Value;

                var pvht = new PValueHashtable(pvht1.Count + pvht2.Count);
                foreach (var pair in pvht1)
                    pvht.Add(pair);
                foreach (var pair in pvht2)
                    pvht.AddOverride(pair);

                result = (PValue) pvht;
            }

            return result != null;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is HashPType;
        }

        private const int _code = 912499480;

        public override int GetHashCode()
        {
            return _code;
        }

        public const string Literal = "Hash";

        public override string ToString()
        {
            return Literal;
        }

        #endregion

        #region ICilCompilerAware Members

        /// <summary>
        ///     Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name = "ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersCustomImplementation;
        }

        private static readonly MethodInfo GetHashPType =
            typeof (PType).GetProperty("Hash").GetGetMethod();

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(GetHashPType);
        }

        #endregion
    }
}