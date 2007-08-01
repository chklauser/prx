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
    [PTypeLiteral("List")]
    public class ListPType : PType
    {
        private ListPType()
        {
        }

        private static ListPType _instance = new ListPType();

        public static ListPType Instance
        {
            get { return _instance; }
        }

        public override PValue CreatePValue(object value)
        {
            List<PValue> listOfPValue = value as List<PValue>;
            IEnumerable<PValue> enumerableOfPValue = value as IEnumerable<PValue>;
            IEnumerable enumerable = value as IEnumerable;

            if (listOfPValue != null)
                return new PValue(listOfPValue, this);
            else if (enumerableOfPValue != null)
                return new PValue(new List<PValue>(enumerableOfPValue), this);
            else if (enumerable != null)
            {
                List<PValue> lst = new List<PValue>();
                foreach (object v in enumerable)
                {
                    PValue pv = v as PValue;
                    if (pv != null)
                        lst.Add(pv);
                    else
                        throw new PrexoniteException(
                            "Cannot create List from IEnumerable that contains elements of any type other than PValue. Use List.CreateFromList for this purpose.");
                }
                return new PValue(lst, this);
            }
            else
                throw new PrexoniteException("Cannot create a PValue from the supplied " + value + ".");
        }

        public override bool TryDynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id,
                                            out PValue result)
        {
            result = null;

            List<PValue> lst = subject.Value as List<PValue>;
            if (lst == null)
                throw new PrexoniteException(subject + " is not a List.");

            if (id.Length == 0)
            {
                if (call == PCall.Get)
                {
                    if (args.Length == 0)
                        if (lst.Count == 0)
                            result = Null.CreatePValue();
                        else
                            result = lst[lst.Count - 1];
                    else if (args.Length == 1)
                        result = lst[(int) args[0].ConvertTo(sctx, Int).Value];
                    else
                    {
                        //Multi-index lookup
                        List<PValue> n_lst = new List<PValue>(args.Length);
                        foreach (PValue index in args)
                            n_lst.Add(lst[(int) index.ConvertTo(sctx, Int).Value]);
                        result = new PValue(n_lst, this);
                    }
                }
                else
                {
                    if (args.Length == 1)
                        lst.Add(args[0] ?? Null.CreatePValue());
                    else //Multi index set
                    {
                        PValue v = args[args.Length - 1] ?? Null.CreatePValue();
                        for (int i = 0; i < args.Length - 1; i++)
                            lst[(int) args[i].ConvertTo(sctx, Int).Value] = v;
                    }
                    result = Null.CreatePValue();
                }
            }
            else
            {
                int index;
                switch(id.ToLowerInvariant())
                {
                    case "length":
                    case "count":
                        result = lst.Count;
                        break;

                    case "getenumerator":
                        result = sctx.CreateNativePValue(new PValueEnumerator(lst.GetEnumerator()));
                        break;
                    case "add":
                        lst.AddRange(args);
                        result = Null.CreatePValue();
                        break;
                    case "clear":
                        lst.Clear();
                        result = Null.CreatePValue();
                        break;
                    case "contains":
                        bool r = true;
                        foreach (PValue arg in args)
                            if (!lst.Contains(arg))
                            {
                                r = false;
                                break;
                            }
                        result = r;
                        break;
                    case "copyto":
                        index = 0;
                        if (args.Length > 1)
                            index = (int) args[1].ConvertTo(sctx, Int).Value;
                        else if (args.Length == 0)
                            throw new PrexoniteException("List.CopyTo requires a target array.");
                        PValue[] targetAsArray = args[0].Value as PValue[];

                        if (targetAsArray == null)
                            throw new PrexoniteException(
                                "List.CopyTo requires it's first argument to be of type Object(\"" +
                                typeof(PValue[]) + "\")");
                        lst.CopyTo(targetAsArray, index);
                        result = Null.CreatePValue();
                        break;
                    case "remove":
                        foreach (PValue arg in args)
                        lst.Remove(arg);
                        result = Null.CreatePValue();
                        break;
                    case "removeat":
                        foreach (PValue arg in args)
                        lst.RemoveAt((int) arg.ConvertTo(sctx, Int).Value);
                        result = Null.CreatePValue();
                        break;
                    case "indexof":
                        if (args.Length == 0)
                            result = -1;
                        else if (args.Length == 1)
                            result = lst.IndexOf(args[0]);
                        else
                        {
                            List<PValue> indices = new List<PValue>(args.Length);
                            foreach (PValue arg in args)
                                indices.Add(lst.IndexOf(arg));
                            result = new PValue(indices, this);
                        }
                        break;
                    case "insert":
                    case "insertat":
                        if (args.Length < 1)
                            throw new PrexoniteException("List.InsertAt requires at least an index.");
                        index = (int) args[0].ConvertTo(sctx, Int).Value;
                        for (int i = 1; i < args.Length; i++)
                            lst.Insert(index, args[i]);
                        result = Null.CreatePValue();
                        break;
                    case "sort":
                        if(args.Length < 1)
                        {
                            lst.Sort(new PValueComparer(sctx));
                            break;
                        }
                        if(args.Length == 1 && args[0].Type is ObjectPType)
                        { //Maybe: comparison using IComparer or Comparison
                            IComparer<PValue> icmp = args[0].Value as IComparer<PValue>;
                            if (icmp != null)
                            {
                                lst.Sort(icmp);
                                result = Null.CreatePValue();
                                break;
                            }
                            Comparer<PValue> cmp = args[0].Value as Comparer<PValue>;
                            if(cmp != null)
                            {
                                lst.Sort(icmp);
                                result = Null.CreatePValue();
                                break;
                            }
                        }
                        //else
                        //Comparison using lambda expressions
                        lst.Sort(delegate(PValue a, PValue b)
                        {
                            foreach(PValue f in args)
                            {
                                PValue pdec = f.IndirectCall(sctx, new PValue[] {a, b});
                                if (!(pdec.Type is IntPType))
                                    pdec = pdec.ConvertTo(sctx, Int);
                                int dec = (int) pdec.Value;
                                if(dec != 0)
                                    return dec;
                            }
                            return 0;
                        });
                        result = Null.CreatePValue();
                        break;
                    case "tostring":
                        StringBuilder sb = new StringBuilder("[ ");
                        foreach (PValue v in lst)
                        {
                            sb.Append(v.CallToString(sctx));
                            sb.Append(", ");
                        }
                        if (sb.Length > 2)
                            sb.Remove(sb.Length - 2, 2);
                        sb.Append(" ]");
                        result = sb.ToString();
                        break;
                
                    default:
                        if (Object[subject.ClrType].TryDynamicCall(sctx, subject, args, call, id, out result))
                        {
                            if (call == PCall.Get)
                                if (result == null)
                                    result = Null.CreatePValue();
                                else if (result.Value is PValue)
                                    result = (PValue) result.Value;
                                else
                                {
                                }
                            else
                                result = Null.CreatePValue();
                        }
                        break;
                }

            }
            return result != null;
        }

        public override bool TryStaticCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;

            if (Engine.StringsAreEqual(id, "Create"))
            {
                result = new PValue(new List<PValue>(args), this);
            }
            else if (Engine.StringsAreEqual(id, "CreateFromSize") && args.Length >= 1)
            {
                result = new PValue(new List<PValue>((int) args[0].ConvertTo(sctx, Int).Value), this);
            }
            else if (Engine.StringsAreEqual(id, "CreateFromList"))
            {
                List<PValue> lst = new List<PValue>();

                foreach (PValue arg in args)
                {
                    PType argT = arg.Type;
                    IEnumerable<PValue> enumerableP = arg.Value as IEnumerable<PValue>;
                    IEnumerable enumerable = arg.Value as IEnumerable;
                    if (argT is ListPType || enumerableP != null)
                        lst.AddRange(enumerableP);
                    else if (enumerable != null)
                    {
                        foreach (object e in enumerable)
                            lst.Add(e as PValue ?? sctx.CreateNativePValue(e));
                    }
                }
                result = new PValue(lst, this);
            }

            return result != null;
        }

        public override bool TryContruct(StackContext sctx, PValue[] args, out PValue result)
        {
            result = new PValue(new List<PValue>(args), this);
            return true;
        }

        protected override bool InternalConvertTo(StackContext sctx, PValue subject, PType target, bool useExplicit,
                                                  out PValue result)
        {
            ObjectPType objT = target as ObjectPType;
            result = null;
            if (objT != null)
            {
                Type clrType = objT.ClrType;
                if (clrType == typeof(IEnumerable<PValue>) ||
                    clrType == typeof(List<PValue>) ||
                    clrType == typeof(ICollection<PValue>) ||
                    clrType == typeof(IList<PValue>) ||
                    clrType == typeof(IEnumerable) ||
                    clrType == typeof(ICollection) ||
                    clrType == typeof(IList))
                    result = target.CreatePValue(subject);
                else if (clrType == typeof(PValue[]) && useExplicit)
                    result = target.CreatePValue(((List<PValue>) subject.Value).ToArray());
                else if(clrType == typeof(PValueKeyValuePair))
                {
                    List<PValue> lst = (List<PValue>) subject.Value;
                    PValue key = lst.Count > 0 ? lst[0] : Null.CreatePValue();
                    List<PValue> valueList = new List<PValue>(lst.Count > 0 ? lst.Count - 1 : 0);
                    for (int i = 1; i < lst.Count; i++)
                        valueList.Add(lst[i]);
                    PValue value = List.CreatePValue(valueList);
                    result = target.CreatePValue(new PValueKeyValuePair(key, value));
                }
            }
            return result != null;
        }

        protected override bool InternalConvertFrom(StackContext sctx, PValue subject, bool useExplicit,
                                                    out PValue result)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is ListPType;
        }

        public override bool IndirectCall(StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            List<PValue> lst = new List<PValue>();
            result = List.CreatePValue(lst);
            PValue r;
            foreach (PValue e in ((IEnumerable<PValue>) subject.Value))
                if (e.TryIndirectCall(sctx, args, out r))
                    lst.Add(r);
                else
                    lst.Add(Null.CreatePValue());
            return true;
        }

        /// <summary>
        /// Concatenates lists.
        /// </summary>
        /// <param name="sctx">The stack context in which to concatenate the lists.</param>
        /// <param name="leftOperand">Any PValue.</param>
        /// <param name="rightOperand">Any PValue.</param>
        /// <param name="result">The resulting list, wrapped in a PValue object.</param>
        /// <returns>Always true</returns>
        /// <exception cref="ArgumentNullException">either <paramref name="leftOperand"/> or <paramref name="rightOperand"/> is null.</exception>
        /// <remarks>
        ///     <para>
        ///         The operator does not modify it's arguments but instead creates a new list.
        ///     </para>
        ///     <para>
        ///         Lists passed as operands are unfolded; the lists contents are added, not the list itself.<br />
        ///         <code>~List.Create(1,2,3) + 4 == ~List.Create(1,2,3,4)</code>
        ///     </para>
        /// </remarks>
        public override bool Addition(StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            if (leftOperand == null)
                throw new ArgumentNullException("leftOperand");
            if (rightOperand == null)
                throw new ArgumentNullException("rightOperand"); 

            List<PValue> nlst = new List<PValue>();
            PValue npv = List.CreatePValue(nlst);

            if (leftOperand.Type is ListPType)
                nlst.AddRange((List<PValue>) leftOperand.Value);
            else 
                nlst.Add(leftOperand);

            if (rightOperand.Type is ListPType)
                nlst.AddRange((List<PValue>) rightOperand.Value);
            else 
                nlst.Add(rightOperand);

            result = npv;
            return true;
        }

        public const string Literal = "List";

        public override string ToString()
        {
            return Literal;
        }

        private const int _code = 86312339;

        public override int GetHashCode()
        {
            return _code;
        }
    }
}