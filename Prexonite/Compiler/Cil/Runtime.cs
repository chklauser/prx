// /*
//  * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
//  *  Copyright (C) 2007  Christian "SealedSun" Klauser
//  *  E-mail  sealedsun a.t gmail d.ot com
//  *  Web     http://www.sealedsun.ch/
//  *
//  *  This program is free software; you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation; either version 2 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
//  * 
//  *  This program is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License along
//  *  with this program; if not, write to the Free Software Foundation, Inc.,
//  *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//  */

#region Namespace Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Prexonite.Commands;
using Prexonite.Types;

#endregion

namespace Prexonite.Compiler.Cil
{
    public static class Runtime
    {
        private static readonly MethodInfo _CallCommandMethod = typeof (Runtime).GetMethod("CallCommand");
        private static readonly MethodInfo _CallFunctionMethod = typeof (Runtime).GetMethod("CallFunction");
        private static readonly MethodInfo _CastMethod = typeof (Runtime).GetMethod("Cast");
        private static readonly MethodInfo _CheckTypeMethod = typeof (Runtime).GetMethod("CheckType");

        private static readonly MethodInfo _ConstructPTypeAsPValueMethod =
            typeof (Runtime).GetMethod("ConstructPTypeAsPValue");

        private static readonly MethodInfo _ExtractBoolMethod = typeof (Runtime).GetMethod("ExtractBool");

        private static readonly MethodInfo _LoadApplicationReferenceMethod =
            typeof (Runtime).GetMethod("LoadApplicationReference");

        private static readonly MethodInfo _LoadCommandReferenceMethod =
            typeof (Runtime).GetMethod("LoadCommandReference");

        private static readonly MethodInfo _LoadEngineReferenceMethod = typeof (Runtime).GetMethod
            ("LoadEngineReference");

        private static readonly MethodInfo _LoadFunctionReferenceMethod =
            typeof (Runtime).GetMethod("LoadFunctionReference");

        private static readonly MethodInfo _LoadGlobalVariableReferenceAsPValueMethod =
            typeof (Runtime).GetMethod("LoadGlobalVariableReferenceAsPValue");

        private static readonly MethodInfo _NewClosureMethod_LateBound = typeof (Runtime).GetMethod(
            "NewClosure", new[] {typeof (StackContext), typeof (PVariable[]), typeof (string)});

        private static readonly MethodInfo _NewClosureMethod_StaticallyBound = typeof (Runtime).GetMethod(
            "NewClosure", new[] {typeof (StackContext), typeof (PVariable[]), typeof (PFunction)});

        private static readonly MethodInfo _NewObjMethod = typeof (Runtime).GetMethod("NewObj");
        private static readonly MethodInfo _NewTypeMethod = typeof (Runtime).GetMethod("NewType");
        private static readonly MethodInfo _ParseExceptionMethod = typeof (Runtime).GetMethod("ParseException");
        private static readonly MethodInfo _RaiseToPowerMethod = typeof (Runtime).GetMethod("RaiseToPower");

        private static readonly MethodInfo _StaticCallMethod =
            typeof (PType).GetMethod
                (
                "StaticCall",
                new[] {typeof (StackContext), typeof (PValue[]), typeof (PCall), typeof (string)});

        private static readonly MethodInfo _ThrowExceptionMethod = typeof (Runtime).GetMethod("ThrowException");

        internal static readonly MethodInfo CheckTypeConstMethod =
            typeof (Runtime).GetMethod("CheckTypeConst", new[] {typeof (PValue), typeof (PType)});

        internal static readonly MethodInfo ConstructPTypeMethod =
            typeof (Runtime).GetMethod("ConstructPType", new[] {typeof (StackContext), typeof (string)});

        internal static readonly MethodInfo DisposeIfPossibleMethod =
            typeof (Runtime).GetMethod("DisposeIfPossible", new[] {typeof (object)});

        public static readonly PValue[] EmptyPValueArray = new PValue[0];
        internal static readonly FieldInfo EmptyPValueArrayField = typeof (Runtime).GetField("EmptyPValueArray");
        public static readonly PVariable[] EmptyPVariableArray = new PVariable[0];
        internal static readonly FieldInfo EmptyPVariableArrayField = typeof (Runtime).GetField("EmptyPVariableArray");

        internal static readonly MethodInfo ExtractEnumeratorMethod =
            typeof (Runtime).GetMethod("ExtractEnumerator", new[] {typeof (PValue), typeof (StackContext)});

        internal static readonly MethodInfo LoadGlobalVariableReferenceMethod =
            typeof (Runtime).GetMethod("LoadGlobalVariableReference");

        internal static readonly MethodInfo NewCoroutineMethod = typeof (Runtime).GetMethod("NewCoroutine");
        internal static readonly MethodInfo WrapPVariableMethod = typeof (Runtime).GetMethod("WrapPVariable");

        internal static MethodInfo CastConstMethod =
            typeof (Runtime).GetMethod("CastConst", new[] {typeof (PValue), typeof (PType), typeof (StackContext)});

        internal static MethodInfo LoadGlobalVariableReferenceAsPValueMethod
        {
            get { return _LoadGlobalVariableReferenceAsPValueMethod; }
        }

        internal static MethodInfo LoadFunctionReferenceMethod
        {
            get { return _LoadFunctionReferenceMethod; }
        }

        internal static MethodInfo LoadApplicationReferenceMethod
        {
            get { return _LoadApplicationReferenceMethod; }
        }

        internal static MethodInfo LoadEngineReferenceMethod
        {
            get { return _LoadEngineReferenceMethod; }
        }

        internal static MethodInfo LoadCommandReferenceMethod
        {
            get { return _LoadCommandReferenceMethod; }
        }

        internal static MethodInfo ConstructPTypeAsPValueMethod
        {
            get { return _ConstructPTypeAsPValueMethod; }
        }

        internal static MethodInfo NewObjMethod
        {
            get { return _NewObjMethod; }
        }

        internal static MethodInfo NewTypeMethod
        {
            get { return _NewTypeMethod; }
        }

        internal static MethodInfo newClosureMethod_LateBound
        {
            get { return _NewClosureMethod_LateBound; }
        }

        internal static MethodInfo newClosureMethod_StaticallyBound
        {
            get { return _NewClosureMethod_StaticallyBound; }
        }

        internal static MethodInfo RaiseToPowerMethod
        {
            get { return _RaiseToPowerMethod; }
        }

        internal static MethodInfo CheckTypeMethod
        {
            get { return _CheckTypeMethod; }
        }

        internal static MethodInfo CastMethod
        {
            get { return _CastMethod; }
        }

        internal static MethodInfo StaticCallMethod
        {
            get { return _StaticCallMethod; }
        }

        internal static MethodInfo CallFunctionMethod
        {
            get { return _CallFunctionMethod; }
        }

        internal static MethodInfo CallCommandMethod
        {
            get { return _CallCommandMethod; }
        }

        internal static MethodInfo ThrowExceptionMethod
        {
            get { return _ThrowExceptionMethod; }
        }

        internal static MethodInfo ExtractBoolMethod
        {
            get { return _ExtractBoolMethod; }
        }

        internal static MethodInfo ParseExceptionMethod
        {
            get { return _ParseExceptionMethod; }
        }

        public static PValue LoadGlobalVariableReferenceAsPValue(StackContext sctx, string id)
        {
            PVariable pv;
            if (!sctx.ParentApplication.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException
                    (
                    "Cannot load reference to non existant global variable " + id);
            return sctx.CreateNativePValue(pv);
        }

        public static PVariable LoadGlobalVariableReference(StackContext sctx, string id)
        {
            PVariable pv;
            if (!sctx.ParentApplication.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException
                    (
                    "Cannot load reference to non existant global variable " + id);
            return pv;
        }

        public static PValue LoadFunctionReference(StackContext sctx, string id)
        {
            PFunction func;
            if (!sctx.ParentApplication.Functions.TryGetValue(id, out func))
                throw new PrexoniteException("Cannot load reference to non existing function " + id);
            return sctx.CreateNativePValue(func);
        }

        public static PValue LoadApplicationReference(StackContext sctx)
        {
            return sctx.CreateNativePValue(sctx.ParentApplication);
        }

        public static PValue LoadEngineReference(StackContext sctx)
        {
            return sctx.CreateNativePValue(sctx.ParentEngine);
        }

        public static PValue LoadCommandReference(StackContext sctx, string id)
        {
            PCommand cmd;
            if (!sctx.ParentEngine.Commands.TryGetValue(id, out cmd))
                throw new PrexoniteException("Cannot load reference to non existing command " + id);
            return sctx.CreateNativePValue(cmd);
        }

        public static PType ConstructPType(StackContext sctx, string expr)
        {
            return sctx.ConstructPType(expr);
        }

        public static PValue ConstructPTypeAsPValue(StackContext sctx, string expr)
        {
            return sctx.CreateNativePValue(ConstructPType(sctx, expr));
        }

        public static PValue NewObj(StackContext sctx, PValue[] args, string expr)
        {
            return ConstructPType(sctx, expr).Construct(sctx, args);
        }

        public static PValue NewType(StackContext sctx, PValue[] args, string name)
        {
            return sctx.CreateNativePValue(sctx.ParentEngine.CreatePType(sctx, name, args));
        }

        public static PValue NewClosure(StackContext sctx, PVariable[] sharedVariables, string funcId)
        {
            PFunction func;
            if (!sctx.ParentApplication.Functions.TryGetValue(funcId, out func))
                throw new PrexoniteException("Cannot create closure for non existant function " + funcId);
            return NewClosure(sctx, sharedVariables, func);
        }

        public static PValue NewClosure(StackContext sctx, PVariable[] sharedVariables, PFunction function)
        {
            if (function.HasCilImplementation)
            {
                return sctx.CreateNativePValue(new CilClosure(function, sharedVariables ?? EmptyPVariableArray));
            }
            else
            {
                return sctx.CreateNativePValue(new Closure(function, sharedVariables ?? EmptyPVariableArray));
            }
        }

        public static PValue RaiseToPower(PValue left, PValue right, StackContext sctx)
        {
            PValue rleft,
                   rright;
            if (
                !(left.TryConvertTo(sctx, PType.Real, out rleft) &&
                  right.TryConvertTo(sctx, PType.Real, out rright)))
                throw new PrexoniteException
                    (
                    "The arguments supplied to the power operator are invalid (cannot be converted to Real).");
            return
                Math.Pow(Convert.ToDouble(rleft.Value), Convert.ToDouble(rright.Value));
        }

        public static PValue CheckTypeConst(PValue obj, PType type)
        {
            return obj.Type.Equals(type);
        }

        public static PValue CheckType(PValue obj, PValue type)
        {
            return obj.Type.Equals(type.Value);
        }

        public static PValue CastConst(PValue obj, PType type, StackContext sctx)
        {
            return obj.ConvertTo(sctx, type, true);
        }

        public static PValue Cast(PValue obj, PValue type, StackContext sctx)
        {
            return obj.ConvertTo(sctx, (PType) type.Value, true);
        }

        //[System.Diagnostics.DebuggerHidden]
        public static PValue CallFunction(StackContext sctx, PValue[] args, string id)
        {
            PFunction func;
            if (!sctx.ParentApplication.Functions.TryGetValue(id, out func))
                throw new PrexoniteException("Cannot call non existant function " + id);
            if (func.HasCilImplementation)
            {
                PValue result;
                ReturnMode returnMode;
                func.CilImplementation(func, sctx, args, null, out result, out returnMode);
                return result;
            }
            else
            {
                return func.Run(sctx.ParentEngine, args);
            }
        }

        public static PValue CallCommand(StackContext sctx, PValue[] args, string id)
        {
            PCommand cmd;
            if (!sctx.ParentEngine.Commands.TryGetValue(id, out cmd))
                throw new PrexoniteException("Cannot call non existant command " + id);
            var sacmd = cmd as StackAwareCommand;
            if (sacmd != null)
            {
                var cctx = sacmd.CreateStackContext(sctx, args);
                return sctx.ParentEngine.Process(cctx);
            }
            else
            {
                return cmd.Run(sctx, args);
            }
        }

        public static void ThrowException(PValue obj, StackContext sctx)
        {
            var t = obj.Type;
            PrexoniteRuntimeException prexc;
            if (t is StringPType)
                prexc =
                    PrexoniteRuntimeException.CreateRuntimeException
                        (
                        sctx, (string) obj.Value);
            else if (t is ObjectPType && obj.Value is Exception)
                prexc =
                    PrexoniteRuntimeException.CreateRuntimeException
                        (
                        sctx,
                        ((Exception) obj.Value).Message,
                        (Exception) obj.Value);
            else
                prexc =
                    PrexoniteRuntimeException.CreateRuntimeException
                        (
                        sctx, obj.CallToString(sctx));

            throw prexc;
        }

        public static bool ExtractBool(PValue left, StackContext sctx)
        {
            if (!ReferenceEquals(left.Type, PType.Bool))
                left = left.ConvertTo(sctx, PType.Bool);
            return (bool) left.Value;
        }

        public static PValue CreateList(StackContext sctx, params PValue[] args)
        {
            return PType.List.CreatePValue(args);
        }

        public static PValue NewCoroutine(PValue routine, StackContext sctx, PValue[] argv)
        {
            var routineobj = routine.Value;
            IStackAware routinesa;
            
            if (routineobj == null)
            {
                return PType.Null.CreatePValue();
            }
            else
            {

                StackContext corctx;
                if ((routinesa = routineobj as IStackAware) != null)
                    corctx = routinesa.CreateStackContext(sctx, argv);
                else
                    corctx = (StackContext)
                             routine.DynamicCall
                                 (
                                 sctx,
                                 new[]
                                 {
                                     PType.Object.CreatePValue(sctx.ParentEngine),
                                     PType.Object.CreatePValue(argv)
                                 },
                                 PCall.Get,
                                 "CreateStackContext").Value;

                return
                    PType.Object[typeof (Coroutine)].CreatePValue(new Coroutine(corctx));
            }
        }

        public static PValue WrapPVariable(PVariable pv)
        {
            return PType.Object.CreatePValue(pv);
        }

        public static PValue ParseException(Exception exc, StackContext sctx)
        {
            var rexc = exc as PrexoniteRuntimeException;
            return sctx.CreateNativePValue
                (
                rexc ?? PrexoniteRuntimeException.CreateRuntimeException(sctx, exc)
                );
        }

        /// <summary>
        /// Extracts an IEnumerator[PValue] from the supplied value. The input can either directly supply a 
        /// sequence of PValue objects or arbitrary CLR objects that will be transparently mapped to PValues with respect to the stack context.
        /// </summary>
        /// <param name="value">The value sequence (implements IEnumerator or IEnumerator[PValue].</param>
        /// <param name="sctx">The stack context-</param>
        /// <returns>A sequence of PValue objects.</returns>
        public static IEnumerator<PValue> ExtractEnumerator(PValue value, StackContext sctx)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");

            IEnumerator genEn;

            if (value.Type is ObjectPType)
                genEn = (IEnumerator) value.Value;
            else
                genEn = value.ConvertTo<IEnumerator>(sctx, true);

            var pvEn = genEn as PValueEnumerator;

            if (pvEn != null)
            {
                return pvEn;
            }
            else
            {
                return new EnumeratorWrapper(genEn, sctx);
            }
        }

        public static void DisposeIfPossible(object obj)
        {
            var disposable = obj as IDisposable;

            if (disposable != null)
                disposable.Dispose();
        }

        #region Nested type: EnumeratorWrapper

        /// <summary>
        /// Used to transparently convert arbitrary sequences to PValue sequences
        /// </summary>
        public sealed class EnumeratorWrapper
            : IEnumerator<PValue>
        {
            #region Class

            private readonly IEnumerator _enumerator;
            private readonly StackContext _sctx;

            public EnumeratorWrapper(IEnumerator enumerator, StackContext sctx)
            {
                _enumerator = enumerator;
                _sctx = sctx;
            }

            #endregion

            #region IEnumerator<PValue> Members

            public PValue Current
            {
                get { return _sctx.CreateNativePValue(_enumerator.Current); }
            }

            public void Dispose()
            {
                var disp = _enumerator as IDisposable;

                if (disp != null)
                {
                    disp.Dispose();
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            #endregion
        }

        #endregion
    }
}