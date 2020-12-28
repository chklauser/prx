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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using NN = JetBrains.Annotations.NotNullAttribute;
using Prexonite.Compiler.Cil;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite.Types
{
    [PTypeLiteral("Object")]
    public sealed class ObjectPType : PType, ICilCompilerAware
    {
        #region Construction

        //Constructor
        [DebuggerStepThrough]
        public ObjectPType(Type clrType)
        {
            if (clrType == null)
                throw new ArgumentNullException(nameof(clrType));
            ClrType = clrType;
        }

        public ObjectPType(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (args.Length < 1)
                throw new PrexoniteException(
                    "The Object type requires exactly one parameter: the type or name of the type to represent.");

            var arg = args[0];
            var oT = arg.Type as ObjectPType;
            if (arg.IsNull)
                ClrType = typeof (object);
            else if ((object) oT != null && typeof (Type).IsAssignableFrom(oT.ClrType))
                ClrType = (Type) arg.Value;
            else if (arg.TryConvertTo(sctx, String, false, out var sarg))
                ClrType = GetType(sctx, (string) sarg.Value);
            else
                throw new PrexoniteException(
                    "The supplied argument (" + arg +
                        ") cannot be used to create an Object<T> type.");
        }

        public ObjectPType(StackContext sctx, string clrTypeName)
        {
            ClrType = GetType(sctx, clrTypeName);
        }

        public static Type GetType(StackContext sctx, string clrTypeName)
        {
            if (TryGetType(sctx, clrTypeName, out var result))
                return result;
            else
                throw new PrexoniteException("Cannot resolve ClrType name \"" + clrTypeName + "\".");
        }

        public static bool TryGetType(StackContext sctx, string clrTypeName, out Type result)
        {
            if (clrTypeName == null)
                throw new ArgumentNullException(nameof(clrTypeName));
            var assemblies = sctx.ParentEngine.GetRegisteredAssemblies();

            result = _getTypeForNamespace(clrTypeName, assemblies);
            if (result != null)
                return true;

            foreach (var ns in sctx.ImportedNamespaces)
            {
                var nsName = ns + '.' + clrTypeName;
                result = _getTypeForNamespace(nsName, assemblies);
                if (result != null)
                    return true;
            }
            return false;
        }

        private static readonly Assembly _prexoniteAssembly = Assembly.GetAssembly(typeof(PValue));

        private static Type _getTypeForNamespace(string clrTypeName,
            IEnumerable<Assembly> assemblies)
        {
            // TODO: drop 'mscorlib' special-casing https://github.com/dotnet/corefx/issues/25968
            // There is a 'hole' in the reflection facade of .NET Core where we can accidentally get our hands on internal 
            // types of the .NET runtime. One example is System.Threading.Thread (public type is in System.Threading.Thread.dll, 
            // but Type.GetType returns an internal class from System.Private.CoreLib.dll).
            // The workaround is to search in 'mscorlib' (which doesn't exist, but gets mapped onto the new .NET Core libraries)
            
            //Try Prexonite
            var result = _prexoniteAssembly.GetType(clrTypeName, false, true);
            if (result != null)
                return result;

            //Try 'mscorlib'
            result = Type.GetType(clrTypeName + ",mscorlib", false, true);
            if (result != null)
                return result;

            //Try registered assemblies
            foreach (var ass in assemblies)
            {
                result = ass.GetType(clrTypeName, false, true);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion

        #region ClrType

        public Type ClrType { [DebuggerStepThrough] get; }

        #endregion

        #region Access Interface Implementation

        #region CLR Interop

        public override bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            return TryDynamicCall(sctx, subject, args, call, id, out result, out var dummy);
        }

        public bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result,
            out MemberInfo resolvedMember)
        {
            return TryDynamicCall(sctx, subject, args, call, id, out result, out resolvedMember,
                false);
        }

        internal bool TryDynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out PValue result,
            out MemberInfo resolvedMember,
            bool suppressIObject)
        {
            result = null;
            resolvedMember = null;

            if (id == null)
                id = "";

            if ((!suppressIObject) && subject.Value is IObject iobj &&
                iobj.TryDynamicCall(sctx, args, call, id, out result))
                return true;

            //Special interop members
            switch (id.ToLowerInvariant())
            {
                case @"\implements":
                    foreach (var arg in args)
                    {
                        Type T;
                        if (arg.Type is ObjectPType &&
                            typeof (Type).IsAssignableFrom(((ObjectPType) arg.Type).ClrType))
                            T = (Type) arg.Value;
                        else
                            T = GetType(sctx, arg.CallToString(sctx));

                        if (!T.IsAssignableFrom(ClrType))
                        {
                            result = false;
                            return true;
                        }
                    }
                    result = true;
                    return true;
                case @"\boxed":
                    result = sctx.CreateNativePValue(subject);
                    return true;
            }

            var cond = new call_conditions(sctx, args, call, id);
            MemberTypes mtypes;
            MemberFilter filter;
            if (id.Length != 0)
            {
                filter = _member_filter;

                if (id.LastIndexOf('\\') == 0)
                    return false; //Default index accessors do not accept calling directives
                mtypes = MemberTypes.Event | MemberTypes.Field | MemberTypes.Method |
                    MemberTypes.Property;
            }
            else
            {
                filter = _default_member_filter;
                mtypes = MemberTypes.Property | MemberTypes.Method;
                cond.memberRestriction = new List<MemberInfo>(ClrType.GetDefaultMembers());
                cond.IgnoreId = true;
                if (subject.Value is Array)
                {
                    cond.memberRestriction.AddRange(
                        ClrType.FindMembers(
                            MemberTypes.Method,
                            BindingFlags.Public | BindingFlags.Instance,
                            Type.FilterName,
                            cond.Call == PCall.Get ? "GetValue" : "SetValue"));
                    cond.memberRestriction.AddRange(
                        ClrType.FindMembers(
                            MemberTypes.Method,
                            BindingFlags.Public | BindingFlags.Instance,
                            Type.FilterName,
                            cond.Call == PCall.Get ? "Get" : "Set"));
                }
            }

            //Get public member candidates
            var candidates = 
                _overloadResolution(ClrType.FindMembers(
                    mtypes,
                    //Member types
                    BindingFlags.Instance | BindingFlags.Public,
                    //Search domain
                    filter,
                    cond), cond).ToImmutableArray();

            if (candidates.Length == 1)
                resolvedMember = candidates[0];

            var ret = _try_execute(candidates, cond, subject, out result);
            if (!ret) //Call did not succeed -> member invalid
                resolvedMember = null;

            return ret;
        }

        public override bool TryStaticCall(
            StackContext sctx,
            PValue[] args,
            PCall call,
            string id,
            out PValue result)
        {
            return TryStaticCall(sctx, args, call, id, out result, out var dummy);
        }

        public bool TryStaticCall(
            StackContext sctx,
            PValue[] args,
            PCall call,
            string id,
            out PValue result,
            out MemberInfo resolvedMember)
        {
            result = null;
            resolvedMember = null;

            if (id == null)
                id = "";

            var cond = new call_conditions(sctx, args, call, id);
            MemberTypes mtypes;
            MemberFilter filter;
            if (id.Length != 0)
            {
                filter = _member_filter;
                if (id.LastIndexOf('\\') == 0)
                    return false; //Default index accessors do not accept calling directives
                mtypes = MemberTypes.Event | MemberTypes.Field | MemberTypes.Method |
                    MemberTypes.Property;
            }
            else
            {
                filter = _default_member_filter;
                mtypes = MemberTypes.Property | MemberTypes.Method;
                cond.memberRestriction = new List<MemberInfo>(ClrType.GetDefaultMembers());
                cond.IgnoreId = true;
            }

            //Get member candidates            
            var candidates = _overloadResolution(
                ClrType.FindMembers(
                    mtypes,
                    //Member types
                    BindingFlags.Static | BindingFlags.Public,
                    //Search domain
                    filter,
                    cond), cond)
                .ToImmutableArray(); //Filter

            if (candidates.Length == 1)
                resolvedMember = candidates[0];

            var ret = _try_execute(candidates, cond, null, out result);
            if (!ret) //Call did not succeed -> member invalid
                resolvedMember = null;
            return ret;
        }

        private bool _try_call_conversion_operator(
            StackContext sctx,
            PValue[] args,
            PCall call,
            string id,
            Type targetType,
            out PValue result)
        {
            result = null;

            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id may not be null or empty.");

            var cond = new call_conditions(sctx, args, call, id)
                {
                    returnType = targetType
                };

            //Get member candidates            
            var candidates = _overloadResolution(
                ClrType.FindMembers(
                    MemberTypes.Method,
                    //Member types
                    BindingFlags.Static | BindingFlags.Public,
                    //Search domain
                    _member_filter,
                    cond), cond).ToImmutableArray(); //Filter

            return _try_execute(candidates, cond, null, out result);
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        internal readonly struct Score : IComparable<Score>, IComparable
        {
            public int NumUpcasts { get; }
            public int NumConversions { get; }

            public bool UsesSctxHack { get; }

            public bool Rejected { get; }

            public Score(int numUpcasts = default, int numConversions = default, bool usesSctxHack = default, bool rejected = default)
            {
                this.NumUpcasts = numUpcasts;
                this.NumConversions = numConversions;
                this.UsesSctxHack = usesSctxHack;
                this.Rejected = rejected;
            }

            public int CompareTo(Score other)
            {
                var rejectedComparison = Rejected.CompareTo(other.Rejected);
                if (rejectedComparison != 0)
                    return rejectedComparison;
                var sctxHackComparison = UsesSctxHack.CompareTo(other.UsesSctxHack);
                if (sctxHackComparison != 0)
                    return sctxHackComparison;
                var numConversionsComparison = NumConversions.CompareTo(other.NumConversions);
                if (numConversionsComparison != 0) 
                    return numConversionsComparison;
                return NumUpcasts.CompareTo(other.NumUpcasts);
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(null, obj)) return 1;
                return obj is Score other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Score)}");
            }

            public static bool operator <(Score left, Score right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(Score left, Score right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator <=(Score left, Score right)
            {
                return left.CompareTo(right) <= 0;
            }

            public static bool operator >=(Score left, Score right)
            {
                return left.CompareTo(right) >= 0;
            }
        }

        /// <summary>
        /// De-sugars higher level members like Properties and Events into lower-level primitives (methods, constructors, fields)
        /// </summary>
        /// <param name="candidate">The member candidate.</param>
        /// <param name="cond">The details of the call.</param>
        /// <returns>The actual member to consider or <c>null</c> if this kind of member is not applicable after all.</returns>
        [ContractAnnotation("candidate:null => null ; candidate:notnull => canbenull")]
        private static MemberInfo _discover(MemberInfo candidate, [NN] call_conditions cond)
        {
            if (candidate == null)
            {
                return null;
            }

            switch (candidate.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Field:
                    return candidate;
                case MemberTypes.Property:
                    var property = (PropertyInfo) candidate;
                    return cond.Call == PCall.Get ? property.GetGetMethod() : property.GetSetMethod();
                case MemberTypes.Event:
                    var info = (EventInfo) candidate;
                    if (cond.Directive == "" ||
                        Engine.DefaultStringComparer.Compare(cond.Directive, "Raise") == 0)
                    {
                        return info.GetRaiseMethod();
                    }
                    else if (Engine.DefaultStringComparer.Compare(cond.Directive, "Add") == 0)
                    {
                        return info.GetAddMethod();
                    }
                    else if (Engine.DefaultStringComparer.Compare(cond.Directive, "Remove") == 0)
                    {
                        return info.GetRemoveMethod();
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Assigns each candidate a score based on how well the candidate matches. Lower scores have higher priority.
        /// The runtime will try to invoke members one after another in ascending score order. The order in which invocation
        /// of members with an equals score is attempted, remains undefined.
        /// </summary>
        /// <param name="candidate">The member candidate to rate.</param>
        /// <param name="cond">The circumstances of the call.</param>
        /// <returns>The score for this member. (Lower indicates better match)</returns>
        private static Score _rate(MemberInfo candidate, call_conditions cond)
        {
            switch (candidate.MemberType)
            {
                case MemberTypes.Field:
                    // It doesn't get much better than a field (there should not be any conflicting overloads).
                    // It *is* possible to still have a conflict if the type is defined in a case-sensitive language.
                    // We are not going to worry about that corner case. CLS-best-practices are pretty clear abou
                    return new Score();
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    var method = (MethodBase) candidate;
                    var parameters = method.GetParameters();
                    var cargs = new object[parameters.Length];
                    //The Sctx hack needs to modify the supplied arguments, so we need a copy of the original reference
                    var sargs = cond.Args;
                    var numUpcasts = 0;
                    var numConversions = 0;

                    var sctxHackOffset = _sctx_hack(parameters, cond) ? 1 : 0;
                    for (var i = 0; i < cargs.Length && i  + sctxHackOffset < parameters.Length; i++)
                    {
                        var arg = sargs[i];
                        if (arg.IsNull)
                        {
                            // null matches anything without penalty
                            continue;
                        }
                        var param = parameters[i + sctxHackOffset];
                        var paramTy = param.ParameterType;
                        var argTy = arg.ClrType;
                        if (paramTy == argTy)
                        {
                            // exact match (no penalty)
                            continue;
                        }

                        if (paramTy.IsAssignableFrom(argTy))
                        {
                            // Matches, but requires an upcast (there may be a more specialized overload)
                            numUpcasts += 1;
                            continue;
                        }

                        // Potentially requires a conversion (we don't really know)
                        numConversions += 1;
                    }

                    return new Score(numUpcasts, numConversions, sctxHackOffset != 0);
                default:
                    // Not really sure what we got ourselves here. 
                    // Note that some higher-level members (events, properties) should have been de-sugared by _discover
                    return new Score(rejected: true);
            }
        }

        private static bool _try_execute_single(MemberInfo candidate, call_conditions cond, PValue subject,
            out PValue ret)
        {
            object result;
            switch (candidate.MemberType)
            {
                case MemberTypes.Method:
                case MemberTypes.Constructor:
                    //Try to execute the method
                    var method = (MethodBase)candidate;
                    var parameters = method.GetParameters();
                    var cargs = new object[parameters.Length];
                    //The Sctx hack needs to modify the supplied arguments, so we need a copy of the original reference
                    var sargs = cond.Args;

                    if (_sctx_hack(parameters, cond))
                    {
                        //Add cond.Sctx to the array of arguments
                        sargs = new PValue[sargs.Length + 1];
                        Array.Copy(cond.Args, 0, sargs, 1, cond.Args.Length);
                        sargs[0] = Object.CreatePValue(cond.Sctx);
                    }

                    for (var i = 0; i < cargs.Length; i++)
                    {
                        var arg = sargs[i];
                        if (!(arg.IsTypeLocked || arg.IsNull)) //Neither Type-locked nor null
                        {
                            var P = parameters[i].ParameterType;
                            var A = arg.ClrType;
                            if (!(P == A || P.IsAssignableFrom(A))) //Is conversion needed?
                            {
                                if (!arg.TryConvertTo(cond.Sctx, P, false, out arg))
                                {
                                    //Try to convert
                                    ret = null;
                                    return false;
                                }
                            }
                        }
                        cargs[i] = arg.Value;
                    }

                    //All conversions were successful, ready to call the method
                    if (method is ConstructorInfo constructorInfo)
                    {
                        result = constructorInfo.Invoke(cargs);
                    }
                    else
                    {
                        try
                        {
                            result = method.Invoke(subject?.Value, cargs);
                        }
                        catch (TargetInvocationException exc)
                        {
                            if (exc.InnerException is PrexoniteRuntimeException {InnerException: {} inner} innerRt)
                                throw inner;
                            throw;
                        }
                    }
                    break;
                case MemberTypes.Field:
                    //Do field access
                    var field = (FieldInfo)candidate;
                    if (cond.Call == PCall.Get)
                        result = field.GetValue(subject?.Value);
                    else
                    {
                        var arg = cond.Args[0];
                        if (!(arg.IsTypeLocked || arg.IsNull)) //Neither Type-locked nor null
                        {
                            var paramTy = field.FieldType;
                            var argTy = arg.ClrType;
                            if (!(paramTy == argTy || paramTy.IsAssignableFrom(argTy))) //Is conversion needed?
                            {
                                if (!arg.TryConvertTo(cond.Sctx, paramTy, false, out arg))
                                {
                                    // failed to convert
                                    ret = null;
                                    return false;
                                }
                            }
                        }
                        field.SetValue(subject?.Value, arg.Value);
                        result = null;
                    }
                    break;
                default:
                    ret = null;
                    return false;
            }

            if (cond.Call == PCall.Get)
            {
                //We'll let the executing engine decide which ptype suits best:
                ret = cond.Sctx.CreateNativePValue(result);
            }
            else
            {
                ret = null;
            }
            return true;
        }

        private static bool _try_execute(
            IEnumerable<MemberInfo> candidates,
            call_conditions cond,
            PValue subject,
            out PValue ret)
        {
            ret = null;
            foreach(var candidate in candidates)
            {
                if (_try_execute_single(candidate, cond, subject, out ret))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// <para>
        /// This method is an optimization for overload resolution. In unambiguous situations, the
        /// resolved member is cached in association with the instruction that triggers the call.
        /// </para>
        /// <para>
        /// The VM will submit the resolved member to us. We don't need to validate/re-resolve it.
        /// </para>
        /// </summary>
        /// <param name="sctx">The context of the call.</param>
        /// <param name="candidate">The unambiguous resolution from a previous invocation.</param>
        /// <param name="args">The arguments to this invocation.</param>
        /// <param name="call">call type (get/set)</param>
        /// <param name="id">The name used to make the call.</param>
        /// <param name="subject">The <c>this</c> pointer.</param>
        /// <returns></returns>
        internal static PValue _execute(
            StackContext sctx,
            MemberInfo candidate,
            PValue[] args,
            PCall call,
            string id,
            PValue subject)
        {
            if (_try_execute(candidate.Singleton(),
                new call_conditions(sctx, args, call, id), subject, out var ret)) 
                return ret;

            // Something went wrong, report as a runtime error.
            var sb = new StringBuilder();
            sb.Append("Cannot call '");
            sb.Append(candidate);
            sb.Append("' on object of Type ");
            sb.Append((subject.IsNull ? "null" : subject.ClrType.FullName));
            sb.Append(" with (");
            foreach (var arg in args)
            {
                sb.Append(arg);
                sb.Append(", ");
            }
            if (args.Length > 0)
                sb.Length -= 2;
            sb.Append(").");
            throw new InvalidCallException(sb.ToString());
        }

        [DebuggerStepThrough]
        private class call_conditions
        {
            public readonly StackContext Sctx;
            public readonly PValue[] Args;
            public readonly PCall Call;
            public readonly string Id;
            public bool IgnoreId;
            public readonly string Directive;
            public Type returnType;
            public List<MemberInfo> memberRestriction;

            public call_conditions(StackContext sctx, PValue[] args, PCall call, string id)
            {
                Sctx = sctx ?? throw new ArgumentNullException(nameof(sctx));
                Args = args ?? Array.Empty<PValue>();
                Call = call;
                Id = id;
                Directive = null;
                returnType = null;
                memberRestriction = null;

                //look for special calling directives
                var idx = id.LastIndexOf('\\');
                if (idx > 0) //calling directive found
                {
                    Id = id.Substring(0, idx);
                    Directive = id.Substring(idx + 1);
                }
            }
        }

        private static bool _default_member_filter(MemberInfo candidate, object arg)
        {
            var property = candidate as PropertyInfo;
            var method = candidate as MethodInfo;
            var cond = (call_conditions) arg;

            //Criteria No.1: Default indices are called "Item" by convention
            if (!(
                //Is default member or...
                (cond.memberRestriction != null && cond.memberRestriction.Contains(candidate)) ||
                    //is called "item"
                    candidate.Name.Equals("Item", StringComparison.OrdinalIgnoreCase)
                ))
                return false;

            if (property != null)
            {
                if (cond.Call == PCall.Get)
                {
                    if (!property.CanRead)
                        return false;
                    return _method_filter(property.GetGetMethod(), cond);
                }
                else //cond.Call == PCall.Set
                {
                    if (!property.CanWrite)
                        return false;
                    return _method_filter(property.GetSetMethod(), cond);
                }
            }
            else if (method != null)
            {
                return _method_filter(method, cond);
            }
            else
                throw new InvalidCallException(
                    "_default_member_filter cannot process anything but properties and methods. Candidate however was of type " +
                        candidate.GetType() + ".");
        }

        private static bool _member_filter(MemberInfo candidate, object arg)
        {
            var cond = (call_conditions) arg;
            //Criteria No.1: The members name (may be supressed)
            if (
                !(cond.IgnoreId ||
                    candidate.Name.Equals(cond.Id, StringComparison.OrdinalIgnoreCase)))
                return false;

            //Criteria No.2: The number of formal parameters
            //Set = min 1 Argument
            if (cond.Call == PCall.Set && cond.Args.Length == 0)
                return false;
            if (candidate is FieldInfo)
            {
                //Get+Field = 0 Parameters, Set+Field = 1 Parameter
                if (cond.Call == PCall.Get)
                {
                    if (cond.Args.Length == 0)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (cond.Args.Length == 1)
                    {
                        //Ensure that type-locked values are acceptable
                        if (cond.Args[0].IsTypeLocked)
                        {
                            var P = (candidate as FieldInfo).FieldType;
                            var A = cond.Args[0].ClrType;
                            if (!(P.Equals(A) || P.IsAssignableFrom(A)))
                                //Neiter Equal nor assignable
                                return false;
                        }

                        return true;
                    }
                    else
                        return false;
                }
            }
            else if (candidate is PropertyInfo)
            {
                var property = candidate as PropertyInfo;
                if (cond.Call == PCall.Get)
                {
                    if (!property.CanRead)
                        return false;
                    else
                        return _method_filter(property.GetGetMethod(), cond);
                }
                else //cond.Call == PCall.Set
                {
                    if (!property.CanWrite)
                        return false;
                    else
                        return _method_filter(property.GetSetMethod(), cond);
                }
            }
            else if (candidate is MethodInfo)
            {
                return _method_filter(candidate as MethodInfo, cond);
            }
            else if (candidate is EventInfo)
            {
                var info = candidate as EventInfo;
                if (cond.Directive == "" ||
                    Engine.DefaultStringComparer.Compare(cond.Directive, "Raise") == 0)
                {
                    return _method_filter(info.GetRaiseMethod(), cond);
                }
                else if (Engine.DefaultStringComparer.Compare(cond.Directive, "Add") == 0)
                {
                    return _method_filter(info.GetAddMethod(), cond);
                }
                else if (Engine.DefaultStringComparer.Compare(cond.Directive, "Remove") == 0)
                {
                    return _method_filter(info.GetRemoveMethod(), cond);
                }
                else
                    return false;
            }
            else //Do not support other members than fields, properties, methods and events
                return false;
        }

        /// <summary>
        ///     Checks whether the StackContext hack can be applied.
        /// </summary>
        /// <param name = "parameters">The parameters array to check.</param>
        /// <param name = "cond">The call_condition object for the current call.</param>
        /// <returns>True if the the hack can be applied, otherwise false.</returns>
        private static bool _sctx_hack(ParameterInfo[] parameters, call_conditions cond)
        {
            //StackContext Hack
            //NOTE: This might be the source of strange problems!
            //If the one argument is missing and the first formal parameter is a StackContext,
            //supply the StackContext received in cond.sctx.
            return (
                //There have to be parameters
                parameters.Length > 0 &&
                    //One argument must be missing
                    cond.Args.Length + 1 == parameters.Length &&
                        //First parameter must be a StackContext
                        typeof (StackContext).IsAssignableFrom(parameters[0].ParameterType));
        }

        private static bool _method_filter(MethodBase method, call_conditions cond)
        {
            var parameters = method.GetParameters();

            //Hide Sctx parameter
            if (_sctx_hack(parameters, cond))
            {
                var relevantParameters = new ParameterInfo[parameters.Length - 1];
                Array.Copy(parameters, 1, relevantParameters, 0, relevantParameters.Length);
                parameters = relevantParameters;
            }

            //Criteria No.1: The number of arguments has to match the number of parameters
            if (cond.Args.Length != parameters.Length)
                return false;

            //Criteria No.2: All Type-Locked arguments must match without a conversion
            for (var i = 0; i < parameters.Length; i++)
            {
                if (cond.Args[i].IsTypeLocked)
                {
                    var P = parameters[i].ParameterType;
                    var A = cond.Args[i].ClrType;
                    if (!(P == A || P.IsAssignableFrom(A))) //Neither Equal nor assignable
                        return false;
                }
            }

            //optional Criteria No.3: Return types must match
            if (cond.returnType != null && method is MethodInfo)
            {
                var methodEx = (MethodInfo) method;
                if (!(methodEx.ReturnType == cond.returnType ||
                      cond.returnType.IsAssignableFrom(methodEx.ReturnType)))
                {
                    return false;
                }
            }

            //The method is a candidate
            return true;
        }

        public override bool IndirectCall(
            StackContext sctx, PValue subject, PValue[] args, out PValue result)
        {
            result = null;
            if (subject.Value is IIndirectCall icall)
                result = icall.IndirectCall(sctx, args);

            return result != null;
        }

        #endregion

        #region Calls

        public PValue DynamicCall(
            StackContext sctx,
            PValue subject,
            PValue[] args,
            PCall call,
            string id,
            out MemberInfo resolvedMember)
        {
            if (!TryDynamicCall(sctx, subject, args, call, id, out var result, out resolvedMember))
            {
                var sb = new StringBuilder();
                sb.Append("Cannot resolve call '");
                sb.Append(id);
                sb.Append("' on object of type ");
                sb.Append(subject.IsNull ? "null" : subject.ClrType.FullName);
                sb.Append(" with (");
                foreach (var arg in args)
                {
                    sb.Append(arg);
                    sb.Append(", ");
                }
                if (args.Length > 0)
                    sb.Length -= 2;
                sb.Append(").");
                throw new InvalidCallException(sb.ToString());
            }
            return result;
        }

        public override PValue DynamicCall(
            StackContext sctx, PValue subject, PValue[] args, PCall call, string id)
        {
            return DynamicCall(sctx, subject, args, call, id, out var dummy);
        }

        public PValue StaticCall(
            StackContext sctx, PValue[] args, PCall call, string id, out MemberInfo resolvedMember)
        {
            if (!TryStaticCall(sctx, args, call, id, out var result, out resolvedMember))
            {
                var sb = new StringBuilder();
                sb.Append("Cannot resolve static call '");
                sb.Append(id);
                sb.Append("' on type ");
                sb.Append(ClrType.FullName);
                sb.Append(" with (");
                foreach (var arg in args)
                {
                    sb.Append(arg);
                    sb.Append(", ");
                }
                if (args.Length > 0)
                    sb.Length -= 2;
                sb.Append(").");
                throw new InvalidCallException(sb.ToString());
            }
            return result;
        }

        public override PValue StaticCall(StackContext sctx, PValue[] args, PCall call, string id)
        {
            return StaticCall(sctx, args, call, id, out var dummy);
        }

        public override bool TryConstruct(StackContext sctx, PValue[] args, out PValue result)
        {
            return TryContruct(sctx, args, out result, out var dummy);
        }

        public bool TryContruct(
            StackContext sctx, PValue[] args, out PValue result, out MemberInfo resolvedMember)
        {
            var cond = new call_conditions(sctx, args, PCall.Get, "")
                {
                    IgnoreId = true
                };

            //Get member candidates            
            var candidates = _overloadResolution(
                ClrType.GetConstructors()
                    .Where(c => _method_filter(c, cond)), cond)
                .ToImmutableArray();

            resolvedMember = null;
            if (candidates.Length == 1)
                resolvedMember = candidates[0];

            var ret = _try_execute(candidates, cond, null, out result);
            if (!ret)
                resolvedMember = null;

            return ret;
        }

        private static IEnumerable<MemberInfo> _overloadResolution(IEnumerable<MemberInfo> candidates, call_conditions cond) => 
            candidates
                .Select(c =>
                {
                    var effectiveCandidate = _discover(c, cond);
                    if (effectiveCandidate == null)
                    {
                        return (null, default);
                    }

                    var score = _rate(effectiveCandidate, cond);
                    if (score.Rejected)
                    {
                        return (null, default);
                    }

                    return (effectiveCandidate, score);
                })
                .Where(pair => pair.effectiveCandidate != null)
                .OrderBy(pair => pair.score)
                .Select(pair => pair.effectiveCandidate);
     

        #endregion

        #region Operators

        public override bool Addition(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall
                    (
                        sctx,
                        new[] {leftOperand, rightOperand},
                        PCall.Get,
                        "op_Addition",
                        out result) ||
                            rightOperand.Type.TryStaticCall
                                (
                                    sctx,
                                    new[] {rightOperand, leftOperand},
                                    PCall.Get,
                                    "op_Addition",
                                    out result) ||
                                        TryDynamicCall
                                            (
                                                sctx,
                                                leftOperand,
                                                new[] {rightOperand},
                                                PCall.Get,
                                                OperatorNames.Prexonite.Addition,
                                                out result);
        }

        public override bool Subtraction(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_Subtraction",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_Subtraction",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.Subtraction,
                                        out result);
        }

        public override bool Multiply(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_Multiply",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_Multiply",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.Multiplication,
                                        out result);
        }

        public override bool Division(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_Division",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_Division",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.Division,
                                        out result);
        }

        public override bool Modulus(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_Modulus",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_Modulus",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.Modulus,
                                        out result);
        }

        public override bool BitwiseAnd(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_BitwiseAnd",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_BitwiseAnd",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.BitwiseAnd,
                                        out result);
        }

        public override bool BitwiseOr(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_BitwiseOr",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_BitwiseOr",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.BitwiseOr,
                                        out result);
        }

        public override bool ExclusiveOr(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_ExclusiveOr",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_ExclusiveOr",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.ExclusiveOr,
                                        out result);
        }

        public override bool Equality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            if (base.Equality(sctx, leftOperand, rightOperand, out result))
                return true;

            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_Equality",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_Equality",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.Equality,
                                        out result);
        }

        public override bool Inequality(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            if (base.Inequality(sctx, leftOperand, rightOperand, out result))
                return true;

            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_Inequality",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_Inequality",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.Inequality,
                                        out result);
        }

        public override bool GreaterThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_GreaterThan",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_GreaterThan",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.GreaterThan,
                                        out result);
        }

        public override bool GreaterThanOrEqual(
            StackContext sctx,
            PValue leftOperand,
            PValue rightOperand,
            out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_GreaterThanOrEqual",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_GreaterThanOrEqual",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.GreaterThanOrEqual,
                                        out result);
        }

        public override bool LessThan(
            StackContext sctx, PValue leftOperand, PValue rightOperand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_LessThan",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_LessThan",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.LessThan,
                                        out result);
        }

        public override bool LessThanOrEqual(
            StackContext sctx,
            PValue leftOperand,
            PValue rightOperand,
            out PValue result)
        {
            return
                TryStaticCall(
                    sctx,
                    new[] {leftOperand, rightOperand},
                    PCall.Get,
                    "op_LessThanOrEqual",
                    out result) ||
                        rightOperand.Type.TryStaticCall(
                            sctx,
                            new[] {rightOperand, leftOperand},
                            PCall.Get,
                            "op_LessThanOrEqual",
                            out result) ||
                                TryDynamicCall
                                    (
                                        sctx,
                                        leftOperand,
                                        new[] {rightOperand},
                                        PCall.Get,
                                        OperatorNames.Prexonite.LessThanOrEqual,
                                        out result);
        }

        public override bool UnaryNegation(StackContext sctx, PValue operand, out PValue result)
        {
            return
                TryStaticCall
                    (
                        sctx, new[] {operand}, PCall.Get, "op_UnaryNegation", out result) ||
                            TryDynamicCall
                                (sctx, operand, Array.Empty<PValue>(), PCall.Get,
                                    OperatorNames.Prexonite.UnaryNegation, out result);
        }

        public override bool LogicalNot(StackContext sctx, PValue operand, out PValue result)
        {
            return
                TryStaticCall(sctx, new[] {operand}, PCall.Get, "op_LogicalNot", out result);
        }

        public override bool OnesComplement(StackContext sctx, PValue operand, out PValue result)
        {
            return
                TryStaticCall(
                    sctx, new[] {operand}, PCall.Get, "op_OnesComplement", out result) ||
                        TryDynamicCall
                            (sctx, operand, Array.Empty<PValue>(), PCall.Get,
                                OperatorNames.Prexonite.OnesComplement, out result);
        }

        public override bool Increment(StackContext sctx, PValue operand, out PValue result)
        {
            return
                TryStaticCall(sctx, new[] {operand}, PCall.Get, "op_Increment", out result) ||
                    TryDynamicCall
                        (sctx, operand, Array.Empty<PValue>(), PCall.Get,
                            OperatorNames.Prexonite.Increment, out result);
        }

        public override bool Decrement(StackContext sctx, PValue operand, out PValue result)
        {
            return
                TryStaticCall(sctx, new[] {operand}, PCall.Get, "op_Decrement", out result) ||
                    TryDynamicCall
                        (sctx, operand, Array.Empty<PValue>(), PCall.Get,
                            OperatorNames.Prexonite.Decrement, out result);
        }

        #endregion

        #region Conversion

        protected override bool InternalConvertTo(
            StackContext sctx,
            PValue subject,
            PType target,
            bool useExplicit,
            out PValue result)
        {
            var arg = new[] {subject};
            var objT = target as ObjectPType;
            result = null;
            if (target is IntPType)
            {
                if (subject.Value is IConvertible)
                {
                    try
                    {
                        result = Int.CreatePValue(Convert.ToInt32(subject.Value));
                        return true;
                    }
                    catch (InvalidCastException)
                    {
                        //ignore invalid cast exceptions
                    }
                }
                else if (_try_clr_convert_to(sctx, subject, typeof (int), useExplicit, out result))
                    return true;

                return false;
            }
            else if (target is RealPType)
            {
                if (subject.Value is IConvertible)
                {
                    try
                    {
                        result = Real.CreatePValue(Convert.ToDouble(subject.Value));
                        return true;
                    }
                    catch (InvalidCastException)
                    {
                        //ignore invalid cast exceptions
                    }
                }

                if (_try_clr_convert_to(sctx, subject, typeof (double), useExplicit, out result))
                    return true;

                return false;
            }
            else if (target is StringPType)
            {
                return _try_clr_convert_to(sctx, subject, typeof (string), useExplicit, out result);
            }
            else if (target is BoolPType)
            {
                // ::op_True > ::op_Implicit > ::op_Explicit
                if (!TryStaticCall(sctx, arg, PCall.Get, "op_True", out var res))
                    if (!_try_clr_convert_to(sctx, subject, typeof (bool), useExplicit, out res))
                        //An object is true by default
                        result = new PValue(true, Bool);
                    else if (res?.Value is bool value)
                        result = new PValue(value, Bool);
                    else
                        result = new PValue(res != null, Bool);

                return true;
            }
            else if ((object) objT != null)
            {
                if (subject.Value == null)
                    return false;

                if (objT.ClrType.IsInstanceOfType(subject.Value))
                {
                    result = objT.CreatePValue(subject.Value);
                    return result != null;
                }
                else
                    return _try_clr_convert_to(sctx, subject, objT.ClrType, useExplicit, out result);
            }
            else
                return false;
        }

        private bool _try_clr_convert_to(
            StackContext sctx,
            PValue subject,
            Type target,
            bool useExplicit,
            out PValue result)
        {
            var arg = new[] {subject};
            if (
                _try_call_conversion_operator(
                    sctx, arg, PCall.Get, "op_Implicit", target, out result) ||
                        (useExplicit &&
                            _try_call_conversion_operator(
                                sctx, arg, PCall.Get, "op_Explicit", target, out result)))
                return true;
            else
                return false;
        }

        protected override bool InternalConvertFrom(
            StackContext sctx,
            PValue subject,
            bool useExplicit,
            out PValue result)
        {
            return _try_clr_convert_to(sctx, subject, ClrType, useExplicit, out result);
        }

        #endregion

        #region Class

        protected override bool InternalIsEqual(PType otherType)
        {
            return (otherType is ObjectPType type && type.ClrType == ClrType);
        }

        public override int GetHashCode()
        {
            return _code ^ ClrType.GetHashCode();
        }

        public const string Literal = "Object";

        private const int _code = -410320954;

        public override string ToString()
        {
            return Literal + "(\"" + StringPType.Escape(ClrType.FullName) + "\")";
        }

        #endregion

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

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            PrexoniteObjectTypeProxy._ImplementInCil(state, ClrType);
        }

        #endregion
    }
}