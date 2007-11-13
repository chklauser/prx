/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Types
{
    [PTypeLiteral(Literal)]
    public class HashPType : PType
    {
        #region Singleton

        private HashPType()
        {
        }

        private static HashPType _instance = new HashPType();

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
            if (!inpv.TryConvertTo(sctx, typeof(PValueKeyValuePair), out res))
                return false;
            else
                result = (PValueKeyValuePair) res.Value;
            return true;
        }

        public override bool IndirectCall(
            StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (args == null)
                args = new PValue[] {};

            result = null;

            int argc = args.Length;

            PValueHashtable pvht = (PValueHashtable) subject.Value;

            if (argc == 0)
            {
                result = sctx.CreateNativePValue(new PValueEnumerator(pvht.GetPValueEnumerator()));
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
                throw new ArgumentNullException("sctx");
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            PValueHashtable pvht = subject.Value as PValueHashtable;

            if (pvht == null)
                throw new ArgumentException("Subject must be a Hash.");

            result = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = Null.CreatePValue();
            }

            int argc = args.Length;

            switch (id.ToLowerInvariant())
            {
                case "":
                    if (call == PCall.Get && argc > 0)
                    {
                        PValue key = args[0];
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
                        bool found = true;
                        foreach (PValue arg in args)
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
                        bool found = true;
                        foreach (PValue arg in args)
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
                    result = Object.CreatePValue(new PValueEnumerator(pvht.GetPValueEnumerator()));
                    break;

                case "gethashcode":
                    result = pvht.GetHashCode();
                    break;

                case "gettype":
                    result = Object.CreatePValue(typeof(PValueHashtable));
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
                        List<PValue> removed = new List<PValue>(pvht.Count);
                        foreach (PValue arg in args)
                            removed.Add(pvht.Remove(arg));

                        result = List.CreatePValue(removed);
                    }
                    break;

                case "tostring":
                    StringBuilder sb = new StringBuilder("{ ");
                    foreach (KeyValuePair<PValue, PValue> pair in pvht)
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
                            args[1].IndirectCall(sctx, new PValue[] {value});
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
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            result = null;

            for (int i = 0; i < args.Length; i++)
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
                    foreach (PValue arg in args)
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
                    for (int i = 0; i < args.Length; i += 2)
                        pvht.AddOverride(args[i], args[i + 1]);
                    result = new PValue(pvht, this);
                    break;

                default:
                    return
                        PValueHashtable.ObjectType.TryStaticCall(sctx, args, call, id, out result);
            }

            return result != null;
        }

        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            result = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = Null.CreatePValue();
            }

            int argc = args.Length;
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
                PValue arg0 = args[0];
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
                throw new ArgumentNullException("sctx");
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (target == null)
                throw new ArgumentNullException("target");

            PValueHashtable pvht = subject.Value as PValueHashtable;

            if (pvht == null)
                throw new ArgumentException("Subject must be a Hash.");

            result = null;

            if (target is ObjectPType)
            {
                Type tT = ((ObjectPType) target).ClrType;
                if (tT == typeof(IDictionary<PValue, PValue>) ||
                    tT == typeof(Dictionary<PValue, PValue>) ||
                    tT == typeof(IDictionary) ||
                    tT == typeof(IEnumerable<KeyValuePair<PValue, PValue>>) ||
                    tT == typeof(IEnumerable) ||
                    tT == typeof(ICollection<KeyValuePair<PValue, PValue>>) ||
                    tT == typeof(ICollection))
                {
                    result = new PValue(pvht, target);
                }
            }
            else if (target is ListPType)
            {
                List<PValue> lst = new List<PValue>(pvht.Count);
                foreach (KeyValuePair<PValue, PValue> pair in pvht)
                    lst.Add(sctx.CreateNativePValue(new PValueKeyValuePair(pair.Key, pair.Value)));
                result = List.CreatePValue(lst);
            }

            return result != null;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx, PValue subject, bool useExplicit, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (subject == null)
                throw new ArgumentNullException("subject");

            result = null;
            PValueHashtable pvht = null;

            PType sT = subject.Type;

            if (sT is ObjectPType)
            {
                object os = subject.Value;
                PValueHashtable o_pvht = os as PValueHashtable;
                if (o_pvht != null)
                    pvht = o_pvht;
                else
                {
                    IDictionary<PValue, PValue> id = os as IDictionary<PValue, PValue>;
                    if (id != null)
                        pvht = new PValueHashtable(id);
                    else
                    {
                        PValueKeyValuePair pvkvp = os as PValueKeyValuePair;
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
    }
}