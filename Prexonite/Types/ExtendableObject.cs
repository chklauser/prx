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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace Prexonite.Types
{
    /// <summary>
    /// Implements the interfaces <see cref="IObject"/> and <see cref="IIndirectCall"/> in 
    /// such a way that the object acts like a Prexonite structure (e.g., members can be added at runtime).
    /// </summary>
    //[DebuggerNonUserCode]
    public class ExtendableObject : IObject, IIndirectCall
    {
        private ExtensionTable _et;

        protected void InitializeExtensionTable()
        {
            if(_et != null)
                throw new InvalidOperationException("The extension table for this object has already been created.");
            _et = new ExtensionTable();
        }

        protected ExtendableObject(bool _tableIsInitialized)
        {
            if(!_tableIsInitialized)
                InitializeExtensionTable();
        }

        protected ExtendableObject()
            : this(false)
        {
        }

        #region IObject Members

        /// <summary>
        /// Tries to call an instance member of the object (CLR).
        /// </summary>
        /// <param name="sctx">The context in which to perform the call.</param>
        /// <param name="subject">The subject to substitute for this</param>
        /// <param name="args">The arguments for the call.</param>
        /// <param name="call">The type of call.</param>
        /// <param name="id">The id of the member. The empty string represents the default member.</param>
        /// <param name="result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name="result"/> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see cref="TryDynamicCall(StackContext,PValue,PValue[],PCall,string,out PValue)"/> instead.
        ///     </para>
        /// </remarks>
        protected virtual bool TryDynamicClrCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id, out PValue result)
        {
            MemberInfo dummyInfo;
            ObjectPType objT = subject.Type as ObjectPType;
            if ((object)objT != null)
                return objT.TryDynamicCall(sctx, subject, args, call, id, out result, out dummyInfo, true);
            else
                return subject.TryDynamicCall(sctx, args, call, id, out result);
        }

        /// <summary>
        /// Tries to call an instance member of the object (CLR). Overwrite this method to intercept object member calls.
        /// </summary>
        /// <param name="sctx">The context in which to perform the call.</param>
        /// <param name="args">The arguments for the call.</param>
        /// <param name="call">The type of call.</param>
        /// <param name="id">The id of the member. The empty string represents the default member.</param>
        /// <param name="result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name="result"/> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see cref="TryDynamicClrCall(StackContext,PValue,PValue[],PCall,string,out PValue)"/> instead.
        ///     </para>
        /// </remarks>
        protected bool TryDynamicClrCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            return
                TryDynamicClrCall(sctx, sctx.CreateNativePValue(this), args, call, id, out result);
        }

        /// <summary>
        /// Tries to call instance members of the object or members of the extended part.
        /// </summary>
        /// <param name="sctx">The context in which to perform the call.</param>
        /// <param name="args">The arguments for the call.</param>
        /// <param name="call">The type of call.</param>
        /// <param name="id">The id of the member. The empty string represents the default member.</param>
        /// <param name="result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name="result"/> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see cref="TryDynamicClrCall(StackContext,PValue,PValue[],PCall,string,out PValue)"/> instead.
        ///     </para>
        /// </remarks>
        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            return TryDynamicCall(sctx, sctx.CreateNativePValue(this), args, call, id, out result);
        }

        /// <summary>
        /// Tries to call instance members of the object or members of the extended part.
        /// </summary>
        /// <param name="sctx">The context in which to perform the call.</param>
        /// <param name="subject">The subject to substitue for <value>this</value>.</param>
        /// <param name="args">The arguments for the call.</param>
        /// <param name="call">The type of call.</param>
        /// <param name="id">The id of the member. The empty string represents the default member.</param>
        /// <param name="result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name="result"/> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see cref="TryDynamicClrCall(StackContext,PValue,PValue[],PCall,string,out PValue)"/> instead.
        ///     </para>
        /// </remarks>
        public bool TryDynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id, out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] { };
            if (id == null)
                id = "";

            if(_et == null)
                _et = new ExtensionTable();

            if (TryDynamicClrCall(sctx, subject, args, call, id, out result) ||  //Try conventional call
                _tryDynamicExtensionCall(sctx, subject, args, call, id, out result)) //Try extension call
                return true;
            else if (call == PCall.Set && args.Length > 0) //Add field if it does not exist
                result = _dynamicCall(
                    sctx, new PValue[] { id, args[0] }, PCall.Set, StructurePType.SetId);
            else
                result = null; //Make sure result is really null.

            return result != null;
        }

        #endregion

        protected void AddRefMember(string id, PValue value)
        {
            if(id == null)
                throw new ArgumentNullException("id");
            if(value == null)
                throw new ArgumentNullException("value"); 
            _et.Add(new ExtensionMember(id, true, value));
        }

        protected void AddMember(string id, PValue value)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (value == null)
                throw new ArgumentNullException("value"); 
            _et.Add(new ExtensionMember(id, value));
        }

        protected void RemoveMember(string id)
        {
            _et.Remove(id);
        }

        private bool _tryDynamicExtensionCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;

            PValue[] argst = StructurePType._addThis(subject, args);

            bool reference = false;
            ExtensionMember m;

            //Try to call the member
            if (_et.TryGetValue(id, out m) && m != null)
                result = m.DynamicCall(sctx, argst, call);
            else
                switch (id.ToLowerInvariant())
                {
                    case StructurePType.SetRefId:
                        reference = true;
                        goto case StructurePType.SetId;
                    case StructurePType.SetId:
                    case StructurePType.SetIdAlternative:
                        if (args.Length < 2)
                            goto default;

                        string mid = (string)args[0].ConvertTo(sctx, PType.String).Value;

                        if (reference || args.Length > 2)
                            reference = (bool)args[1].ConvertTo(sctx, PType.Bool).Value;

                        if (_et.Contains(mid))
                            m = _et[mid];
                        else
                        {
                            m = new ExtensionMember(mid);
                            _et.Add(m);
                        }

                        m.Value = args[args.Length - 1];
                        m.Indirect = reference;

                        result = m.Value;

                        break;
                    default:
                        //Try to call the generic "call" member
                        if (_et.TryGetValue(StructurePType.CallId, out m) && m != null)
                            result =
                                m.DynamicCall(
                                    sctx,
                                    StructurePType._addThis(subject, argst),
                                    call);
                        else
                            return false;
                        break;
                }

            return result != null;
        }

        private PValue _dynamicCall(StackContext sctx, PValue[] args, PCall call, string id)
        {
            return _dynamicCall(sctx, sctx.CreateNativePValue(this), args, call, id);
        }

        private PValue _dynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call, string id)
        {
            PValue result;
            if (!_tryDynamicExtensionCall(sctx, subject, args, call, id, out result))
                throw new InvalidCallException(
                    "Cannot call " + id + " on extension of type " + GetType().Name);
            return result ?? PType.Null;
        }

        #region IIndirectCall Members

        /// <summary>
        /// Indirectly calls the extended part of the object.
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the call.</param>
        /// <param name="args">The arguments to pass to the handling function.</param>
        /// <returns>The value returned by the extended part of the object.</returns>
        public virtual PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return IndirectCall(sctx, sctx.CreateNativePValue(this), args);
        }

        /// <summary>
        /// Indirectly calls the extended part of this object using a different subject (used together with object facades).
        /// </summary>
        /// <param name="sctx">The stack context in which to perform the call.</param>
        /// <param name="subject">The subject to substitute for this.</param>
        /// <param name="args">The arguments to pass to the handling function.</param>
        /// <returns>The value returned by the extended part of the object.</returns>
        public PValue IndirectCall(StackContext sctx, PValue subject, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] { };

            if (_et == null)
                _et = new ExtensionTable();

            ExtensionMember m;
            if(!_et.TryGetValue(StructurePType.IndirectCallId, out m))
                throw new PrexoniteException(this + " does not support indirect calls.");
            return
                m.DynamicCall(
                    sctx, StructurePType._addThis(subject, args), PCall.Get);
        }

        #endregion
    }

    [DebuggerNonUserCode]
    internal class ExtensionMember
    {
        internal ExtensionMember(string id)
            : this(id, false, null)
        {
        }

        internal ExtensionMember(string id, bool indirect)
            : this(id, indirect, null)
        {
        }

        internal ExtensionMember(string id, PValue value)
            : this(id, false, value)
        {
        }

        internal ExtensionMember(string id, bool indirect, PValue value)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (value == null)
                value = PType.Null.CreatePValue();

            _id = id;
            _indirect = indirect;
            _value = value;
        }

        public bool Indirect
        {
            get { return _indirect; }
            set { _indirect = value; }
        }
        private bool _indirect;

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private string _id;

        public PValue Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private PValue _value;

        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, out PValue result)
        {
            result = null;
            if(_indirect)
            {
                result = Value.IndirectCall(sctx, args);
            }
            else // direct
            {
                if (args == null)
                    throw new ArgumentNullException("args");
                if (sctx == null)
                    throw new ArgumentNullException("sctx");

                result = Value;

                if (call == PCall.Set && args.Length > 0)
                    Value = args[args.Length - 1];
            }

            return result != null;
        }

        public PValue DynamicCall(StackContext sctx, PValue[] args, PCall call)
        {
            PValue result;
            if (!TryDynamicCall(sctx, args, call, out result))
                throw new InvalidCallException(
                    "Cannot call extension member " + Id + " with " + args.Length + " arguments.");
            return result;
        }
    }

    [DebuggerNonUserCode]
    internal class ExtensionTable : KeyedCollection<string, ExtensionMember>
    {
        internal ExtensionTable()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }

        ///<summary>
        ///When implemented in a derived class, extracts the key from the specified element.
        ///</summary>
        ///
        ///<returns>
        ///The key for the specified element.
        ///</returns>
        ///
        ///<param name="item">The element from which to extract the key.</param>
        protected override string GetKeyForItem(ExtensionMember item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            return item.Id;
        }

        public bool TryGetValue(string key, out ExtensionMember value)
        {
            if(Contains(key))
            {
                value = this[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
