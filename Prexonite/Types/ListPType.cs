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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Prexonite.Compiler.Cil;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("List")]
    public class ListPType : PType, ICilCompilerAware
    {
        private ListPType()
        {
        }

        public static ListPType Instance { get; } = new ListPType();

        public override PValue CreatePValue(object value)
        {
            var listOfPValue = value as List<PValue>;
            var enumerableOfPValue = value as IEnumerable<PValue>;
            var enumerable = value as IEnumerable;

            if (listOfPValue != null)
                return new PValue(listOfPValue, this);
            if (enumerableOfPValue != null)
                return new PValue(new List<PValue>(enumerableOfPValue), this);
            if (enumerable == null)
                throw new PrexoniteException(
                    "Cannot create a PValue from the supplied " + value + ".");
            
            var lst = new List<PValue>();
            foreach (var v in enumerable)
            {
                if (v is PValue pv)
                    lst.Add(pv);
                else
                    throw new PrexoniteException(
                        "Cannot create List from IEnumerable that contains elements of any type other than PValue. Use List.CreateFromList for this purpose.");
            }
            return new PValue(lst, this);
        }

        public override bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            result = null;

            if (!(subject.Value is List<PValue> lst))
                throw new PrexoniteException(subject + " is not a List.");

            if (id.Length == 0)
            {
                if (call == PCall.Get)
                {
                    switch (args.Length)
                    {
                        case 0:
                            result = lst.Count == 0 ? Null.CreatePValue() : lst[lst.Count - 1];
                            break;
                        case 1:
                            result = lst[(int)args[0].ConvertTo(sctx, Int).Value];
                            break;
                        default:
                            //Multi-index lookup
                            var n_lst = new List<PValue>(args.Length);
                            foreach (var index in args)
                                n_lst.Add(lst[(int)index.ConvertTo(sctx, Int).Value]);
                            result = new PValue(n_lst, this);
                            break;
                    }
                }
                else
                {
                    if (args.Length == 1)
                        lst.Add(args[0] ?? Null.CreatePValue());
                    else //Multi index set
                    {
                        var v = args[args.Length - 1] ?? Null.CreatePValue();
                        for (var i = 0; i < args.Length - 1; i++)
                            lst[(int)args[i].ConvertTo(sctx, Int).Value] = v;
                    }
                    result = Null.CreatePValue();
                }
            }
            else
            {
                int index;
                switch (id.ToLowerInvariant())
                {
                    case "length":
                    case "count":
                        result = lst.Count;
                        break;

                    case "getenumerator":
                        result =
                            sctx.CreateNativePValue(new PValueEnumeratorWrapper(lst.GetEnumerator()));
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
                        var r = true;
                        foreach (var arg in args)
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
                            index = (int)args[1].ConvertTo(sctx, Int).Value;
                        else if (args.Length == 0)
                            throw new PrexoniteException("List.CopyTo requires a target array.");
                        var targetAsArray = args[0].Value as PValue[];

                        if (targetAsArray == null)
                            throw new PrexoniteException(
                                "List.CopyTo requires it's first argument to be of type Object(\"" +
                                    typeof(PValue[]) + "\")");
                        lst.CopyTo(targetAsArray, index);
                        result = Null.CreatePValue();
                        break;
                    case "remove":
                        var cnt = 0;
                        foreach (var arg in args)
                        {
                            if (lst.Remove(arg))
                                cnt++;
                        }
                        result = cnt;
                        break;
                    case "removeat":
                        var toRemove = new List<bool>(lst.Count);
                        for (var i = 0; i < lst.Count; i++)
                            toRemove.Add(false);

                        foreach (var arg in args)
                        {
                            var li = (int)arg.ConvertTo(sctx, Int).Value;
                            if (li > lst.Count - 1 || li < 0)
                                throw new ArgumentOutOfRangeException(
                                    "The index " + li + " is out of the range of the supplied list.");
                            toRemove[li] = true;
                        }

                        for (var i = 0; i < toRemove.Count; i++)
                        {
                            if (toRemove[i])
                            {
                                toRemove.RemoveAt(i);
                                lst.RemoveAt(i);
                                i--;
                            }
                        }
                        result = Null.CreatePValue();
                        break;
                    case "indexof":
                        if (args.Length == 0)
                            result = -1;
                        else if (args.Length == 1)
                            result = lst.IndexOf(args[0]);
                        else
                        {
                            var indices = new List<PValue>(args.Length);
                            foreach (var arg in args)
                                indices.Add(lst.IndexOf(arg));
                            result = new PValue(indices, this);
                        }
                        break;
                    case "insert":
                    case "insertat":
                        if (args.Length < 1)
                            throw new PrexoniteException(
                                "List.InsertAt requires at least an index.");
                        index = (int)args[0].ConvertTo(sctx, Int).Value;
                        for (var i = 1; i < args.Length; i++)
                            lst.Insert(index, args[i]);
                        result = Null.CreatePValue();
                        break;
                    case "sort":
                        if (args.Length < 1)
                        {
                            lst.Sort(new PValueComparer(sctx));
                            break;
                        }
                        if (args.Length == 1 && args[0].Type is ObjectPType)
                        {
                            //Maybe: comparison using IComparer or Comparison
                            var icmp = args[0].Value as IComparer<PValue>;
                            if (icmp != null)
                            {
                                lst.Sort(icmp);
                                result = Null.CreatePValue();
                                break;
                            }
                            var cmp = args[0].Value as Comparer<PValue>;
                            if (cmp != null)
                            {
                                lst.Sort(icmp);
                                result = Null.CreatePValue();
                                break;
                            }
                        }
                        //else
                        //Comparison using lambda expressions
                        lst.Sort(
                            delegate(PValue a, PValue b)
                            {
                                foreach (var f in args)
                                {
                                    var pdec = f.IndirectCall(sctx, new[] { a, b });
                                    if (!(pdec.Type is IntPType))
                                        pdec = pdec.ConvertTo(sctx, Int);
                                    var dec = (int)pdec.Value;
                                    if (dec != 0)
                                        return dec;
                                }
                                return 0;
                            });
                        result = Null.CreatePValue();
                        break;
                    case "tostring":
                        result = _getStringRepresentation(lst, sctx);
                        break;
                    case @"\implements":
                        foreach (var arg in args)
                        {
                            Type T;
                            if (arg.Type is ObjectPType &&
                                typeof(Type).IsAssignableFrom(((ObjectPType)arg.Type).ClrType))
                                T = (Type)arg.Value;
                            else
                            {
                                var typeName = arg.CallToString(sctx);
                                switch (typeName.ToUpperInvariant())
                                {
                                    case "IENUMERABLE":
                                    case "ILIST":
                                    case "ICOLLECTION":
                                        result = true;
                                        return true;
                                    default:
                                        T = ObjectPType.GetType(sctx, typeName);
                                        break;
                                }
                            }

                            if (!T.IsAssignableFrom(typeof(List<PValue>)))
                            {
                                result = false;
                                return true;
                            }
                        }
                        result = true;
                        return true;

                    default:
                        if (
                            Object[subject.ClrType].TryDynamicCall(
                                sctx, subject, args, call, id, out result))
                        {
                            if (call == PCall.Get)
                                if (result == null)
                                    result = Null.CreatePValue();
                                else if (result.Value is PValue)
                                    result = (PValue)result.Value;
                                else
                                    result = Null.CreatePValue();
                        }
                        break;
                }
            }
            return result != null;
        }

        private static string _getStringRepresentation(IEnumerable<PValue> lst, StackContext sctx)
        {
            var sb = new StringBuilder("[ ");
            foreach (var v in lst)
            {
                sb.Append(v.CallToString(sctx));
                sb.Append(", ");
            }
            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            sb.Append(" ]");
            return sb.ToString();
        }

        public override bool TryStaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;

            if (Engine.StringsAreEqual(id, "Create"))
            {
                result = new PValue(new List<PValue>(args), this);
            }
            else if (Engine.StringsAreEqual(id, "CreateFromSize") && args.Length >= 1)
            {
                result =
                    new PValue(new List<PValue>((int)args[0].ConvertTo(sctx, Int).Value), this);
            }
            else if (Engine.StringsAreEqual(id, "CreateFromList"))
            {
                var lst = new List<PValue>();

                foreach (var arg in args)
                {
                    var enumerableP = arg.Value as IEnumerable<PValue>;
                    var enumerable = arg.Value as IEnumerable;
                    if (enumerableP != null)
                        lst.AddRange(enumerableP);
                    else if (enumerable != null)
                    {
                        foreach (var e in enumerable)
                            lst.Add(e as PValue ?? sctx.CreateNativePValue(e));
                    }
                }
                result = new PValue(lst, this);
            }

            return result != null;
        }

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            result = new PValue(new List<PValue>(args), this);
            return true;
        }

        protected override bool InternalConvertTo(
            StackContext sctx,
            PValue subject,
            PType target,
            bool useExplicit,
            out PValue result)
        {
            var objT = target as ObjectPType;

            result = null;
            if ((object)objT != null)
            {
                var clrType = objT.ClrType;
                var genericTypeTemplate = clrType.IsGenericType ? clrType.GetGenericTypeDefinition() : null;
                if (clrType == typeof(IEnumerable<PValue>) ||
                    clrType == typeof(List<PValue>) ||
                        clrType == typeof(ICollection<PValue>) ||
                            clrType == typeof(IList<PValue>) ||
                                clrType == typeof(IEnumerable) ||
                                    clrType == typeof(ICollection) ||
                                        clrType == typeof(IList))
                    result = target.CreatePValue(subject);
                else if (clrType == typeof(PValue[]) && useExplicit)
                    result = target.CreatePValue(((List<PValue>)subject.Value).ToArray());
                else if (clrType == typeof(PValueKeyValuePair))
                {
                    var lst = (List<PValue>)subject.Value;
                    var key = lst.Count > 0 ? lst[0] : Null.CreatePValue();
                    var valueList = new List<PValue>(lst.Count > 0 ? lst.Count - 1 : 0);
                    for (var i = 1; i < lst.Count; i++)
                        valueList.Add(lst[i]);
                    var value = List.CreatePValue(valueList);
                    result = target.CreatePValue(new PValueKeyValuePair(key, value));
                }
                else if (clrType.IsArray)
                {
                    //Convert each element in the list to the element type of the array.
                    var et = clrType.GetElementType();
                    var lst = (List<PValue>)subject.Value;
                    var array = Array.CreateInstance(et, lst.Count);
                    var success = true;
                    for (var i = 0; i < lst.Count; i++)
                    {
                        PValue converted;
                        if (lst[i].TryConvertTo(sctx, et, useExplicit, out converted))
                        {
                            array.SetValue(converted.Value, i);
                        }
                        else
                        {
                            success = false;
                            break;
                        }
                    }
                    if (success)
                        result = sctx.CreateNativePValue(array);
                }
                else if (genericTypeTemplate == typeof (IEnumerable<>)
                        || genericTypeTemplate == typeof(ICollection<>) 
                        || genericTypeTemplate == typeof(IReadOnlyCollection<>) 
                        || genericTypeTemplate == typeof(IReadOnlyList<>) 
                        || genericTypeTemplate == typeof(IList<>))
                {
                    // Convert each element in the list to the element type of the sequence
                    var elementT = clrType.GetGenericArguments()[0];
                    var listT = typeof (List<>).MakeGenericType(elementT);
                    // ReSharper disable once PossibleNullReferenceException
                    var list = (IList) listT.GetConstructor(new Type[0]).Invoke(new object[0]);
                    var success = true;
                    foreach (var pv in (List<PValue>)subject.Value)
                    {
                        PValue converted;
                        if (pv.TryConvertTo(sctx, elementT, useExplicit, out converted))
                        {
                            list.Add(converted.Value);
                        }
                        else
                        {
                            success = false;
                            break;
                        }
                    }
                    if (success)
                    {
                        if (genericTypeTemplate == typeof (IReadOnlyCollection<>)
                            || genericTypeTemplate == typeof (IReadOnlyList<>))
                        {
                            // Wrap in readonly list view
                            var readonlyListT = typeof (ReadOnlyCollection<>).MakeGenericType(elementT);
                            var readonlyList =
                            // ReSharper disable once PossibleNullReferenceException
                                readonlyListT.GetConstructor(new[] {typeof (IList<>).MakeGenericType(elementT)})
                                    .Invoke(new object[]{list});
                            result = sctx.CreateNativePValue(readonlyList);
                        }
                        else
                        {
                            result = sctx.CreateNativePValue(list);
                        }
                    }
                }
            }
            return result != null;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx,
            PValue subject,
            bool useExplicit,
            out PValue result)
        {
            //TODO: Create to List conversions (KeyValuePair)
            result = null;
            return false;
        }

        protected override bool InternalIsEqual(PType otherType)
        {
            return otherType is ListPType;
        }

        public override bool IndirectCall(
            StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            var lst = new List<PValue>();
            result = List.CreatePValue(lst);
            PValue r;
            foreach (var e in ((IEnumerable<PValue>)subject.Value))
                if (e.TryIndirectCall(sctx, args, out r))
                    lst.Add(r);
                else
                    lst.Add(Null.CreatePValue());
            return true;
        }

        /// <summary>
        ///     Concatenates lists.
        /// </summary>
        /// <param name = "sctx">The stack context in which to concatenate the lists.</param>
        /// <param name = "leftOperand">Any PValue.</param>
        /// <param name = "rightOperand">Any PValue.</param>
        /// <param name = "result">The resulting list, wrapped in a PValue object.</param>
        /// <returns>Always true</returns>
        /// <exception cref = "ArgumentNullException">either <paramref name = "leftOperand" /> or <paramref name = "rightOperand" /> is null.</exception>
        /// <remarks>
        ///     <para>
        ///         The operator does not modify it's arguments but instead creates a new list.
        ///     </para>
        ///     <para>
        ///         Lists passed as operands are unfolded; the lists contents are added, not the list itself.<br />
        ///         <code>~List.Create(1,2,3) + 4 == ~List.Create(1,2,3,4)</code>
        ///     </para>
        /// </remarks>
        public override bool Addition(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            if (leftOperand == null)
                throw new ArgumentNullException(nameof(leftOperand));
            if (rightOperand == null)
                throw new ArgumentNullException(nameof(rightOperand));

            var nlst = new List<PValue>();
            var npv = List.CreatePValue(nlst);

            if (leftOperand.Type is ListPType)
                nlst.AddRange((List<PValue>)leftOperand.Value);
            else
                nlst.Add(leftOperand);

            if (rightOperand.Type is ListPType)
                nlst.AddRange((List<PValue>)rightOperand.Value);
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

        private static readonly MethodInfo GetListPType =
            typeof(PType).GetProperty("List").GetGetMethod();

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitCall(GetListPType);
        }

        #endregion
    }
}