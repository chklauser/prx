// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#region

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

#endregion

namespace Prexonite.Types
{
    /// <summary>
    ///     Implements the interfaces <see cref = "IObject" /> and <see cref = "IIndirectCall" /> in 
    ///     such a way that the object acts like a Prexonite structure (e.g., members can be added at runtime).
    /// </summary>
    [DebuggerNonUserCode]
    public class ExtendableObject : IObject, IIndirectCall
    {
        private ExtensionTable _et;

        protected void InitializeExtensionTable()
        {
            if (_et != null)
                throw new InvalidOperationException(
                    "The extension table for this object has already been created.");
            _et = new ExtensionTable();
        }

        /// <summary>
        ///     Creates a new instance of ExtendableObject.
        /// </summary>
        /// <param name = "tableIsInitialized">Indicates whether the initialization of the 
        ///     extension table (<see cref = "InitializeExtensionTable" />) should be performed by this 
        ///     constructor overload.</param>
        protected ExtendableObject(bool tableIsInitialized)
        {
            if (!tableIsInitialized)
                InitializeExtensionTable();
        }

        protected ExtendableObject()
            : this(false)
        {
        }

        #region IObject Members

        /// <summary>
        ///     Tries to call an instance member of the object (CLR).
        /// </summary>
        /// <param name = "sctx">The context in which to perform the call.</param>
        /// <param name = "subject">The subject to substitute for this</param>
        /// <param name = "args">The arguments for the call.</param>
        /// <param name = "call">The type of call.</param>
        /// <param name = "id">The id of the member. The empty string represents the default member.</param>
        /// <param name = "result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name = "result" /> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see
        ///      cref = "TryDynamicCall(StackContext,PValue,PValue[],PCall,string,out PValue)" /> instead.
        ///     </para>
        /// </remarks>
        protected virtual bool TryDynamicClrCall(
            StackContext sctx, PValue subject, PValue[] args, PCall call, string id,
            out PValue result)
        {
            MemberInfo dummyInfo;
            var objT = subject.Type as ObjectPType;
            if ((object) objT != null)
                return objT.TryDynamicCall(sctx, subject, args, call, id, out result, out dummyInfo,
                    true);
            else
                return subject.TryDynamicCall(sctx, args, call, id, out result);
        }

        /// <summary>
        ///     Tries to call an instance member of the object (CLR). Overwrite this method to intercept object member calls.
        /// </summary>
        /// <param name = "sctx">The context in which to perform the call.</param>
        /// <param name = "args">The arguments for the call.</param>
        /// <param name = "call">The type of call.</param>
        /// <param name = "id">The id of the member. The empty string represents the default member.</param>
        /// <param name = "result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name = "result" /> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see
        ///      cref = "TryDynamicClrCall(StackContext,PValue,PValue[],PCall,string,out PValue)" /> instead.
        ///     </para>
        /// </remarks>
        protected bool TryDynamicClrCall(StackContext sctx, PValue[] args, PCall call, string id,
            out PValue result)
        {
            return
                TryDynamicClrCall(sctx, sctx.CreateNativePValue(this), args, call, id, out result);
        }

        /// <summary>
        ///     Tries to call instance members of the object or members of the extended part.
        /// </summary>
        /// <param name = "sctx">The context in which to perform the call.</param>
        /// <param name = "args">The arguments for the call.</param>
        /// <param name = "call">The type of call.</param>
        /// <param name = "id">The id of the member. The empty string represents the default member.</param>
        /// <param name = "result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name = "result" /> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see
        ///      cref = "TryDynamicClrCall(StackContext,PValue,PValue[],PCall,string,out PValue)" /> instead.
        ///     </para>
        /// </remarks>
        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            return TryDynamicCall(sctx, sctx.CreateNativePValue(this), args, call, id, out result);
        }

        /// <summary>
        ///     Tries to call instance members of the object or members of the extended part.
        /// </summary>
        /// <param name = "sctx">The context in which to perform the call.</param>
        /// <param name = "subject">The subject to substitue for <value>this</value>.</param>
        /// <param name = "args">The arguments for the call.</param>
        /// <param name = "call">The type of call.</param>
        /// <param name = "id">The id of the member. The empty string represents the default member.</param>
        /// <param name = "result">The result of the call.</param>
        /// <returns>True if the call was successful, false otherwise.</returns>
        /// <remarks>
        ///     <para>
        ///         <paramref name = "result" /> is only defined if the method returns true.
        ///     </para>
        ///     <para>
        ///         If you want to intercept object member calls yourself, overwrite <see
        ///      cref = "TryDynamicClrCall(StackContext,PValue,PValue[],PCall,string,out PValue)" /> instead.
        ///     </para>
        /// </remarks>
        public bool TryDynamicCall(
            StackContext sctx, PValue subject, PValue[] args, PCall call, string id,
            out PValue result)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};
            if (id == null)
                id = "";

            if (_et == null)
                _et = new ExtensionTable();

            if (TryDynamicClrCall(sctx, subject, args, call, id, out result) ||
                //Try conventional call
                _tryDynamicExtensionCall(sctx, subject, args, call, id, out result))
                //Try extension call
                return true;
            else if (call == PCall.Set && args.Length > 0) //Add field if it does not exist
                result = _dynamicCall(
                    sctx, new[] {id, args[0]}, PCall.Set, StructurePType.SetId);
            else
                result = null; //Make sure result is really null.

            return result != null;
        }

        #endregion

        protected void AddRefMember(string id, PValue value)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (value == null)
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

        private bool _tryDynamicExtensionCall(
            StackContext sctx, PValue subject, PValue[] args, PCall call, string id,
            out PValue result)
        {
            result = null;

            var argst = StructurePType._AddThis(subject, args);

            var reference = false;
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

                        var mid = (string) args[0].ConvertTo(sctx, PType.String).Value;

                        if (reference || args.Length > 2)
                            reference = (bool) args[1].ConvertTo(sctx, PType.Bool).Value;

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
                                    StructurePType._AddThis(subject, argst),
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

        private PValue _dynamicCall(StackContext sctx, PValue subject, PValue[] args, PCall call,
            string id)
        {
            PValue result;
            if (!_tryDynamicExtensionCall(sctx, subject, args, call, id, out result))
                throw new InvalidCallException(
                    "Cannot call " + id + " on extension of type " + GetType().Name);
            return result ?? PType.Null;
        }

        #region IIndirectCall Members

        /// <summary>
        ///     Indirectly calls the extended part of the object.
        /// </summary>
        /// <param name = "sctx">The stack context in which to perform the call.</param>
        /// <param name = "args">The arguments to pass to the handling function.</param>
        /// <returns>The value returned by the extended part of the object.</returns>
        public virtual PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return IndirectCall(sctx, sctx.CreateNativePValue(this), args);
        }

        /// <summary>
        ///     Indirectly calls the extended part of this object using a different subject (used together with object facades).
        /// </summary>
        /// <param name = "sctx">The stack context in which to perform the call.</param>
        /// <param name = "subject">The subject to substitute for this.</param>
        /// <param name = "args">The arguments to pass to the handling function.</param>
        /// <returns>The value returned by the extended part of the object.</returns>
        public PValue IndirectCall(StackContext sctx, PValue subject, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            if (_et == null)
                _et = new ExtensionTable();

            ExtensionMember m;
            if (!_et.TryGetValue(StructurePType.IndirectCallId, out m))
                throw new PrexoniteException(this + " does not support indirect calls.");
            return
                m.DynamicCall(
                    sctx, StructurePType._AddThis(subject, args), PCall.Get);
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

            Id = id;
            _indirect = indirect;
            Value = value;
        }

        public bool Indirect
        {
            get { return _indirect; }
            set { _indirect = value; }
        }

        private bool _indirect;

        public string Id { get; set; }

        public PValue Value { get; set; }

        public bool TryDynamicCall(
            StackContext sctx, PValue[] args, PCall call, out PValue result)
        {
            result = null;
            if (_indirect)
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
        ///    When implemented in a derived class, extracts the key from the specified element.
        ///</summary>
        ///<returns>
        ///    The key for the specified element.
        ///</returns>
        ///<param name = "item">The element from which to extract the key.</param>
        protected override string GetKeyForItem(ExtensionMember item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            return item.Id;
        }

        public bool TryGetValue(string key, out ExtensionMember value)
        {
            if (Contains(key))
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