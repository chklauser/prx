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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Prexonite.Commands;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Cil
{
    // ReSharper disable InconsistentNaming
    public static class Runtime
    {

        private static readonly MethodInfo _CallCommandMethod =

            typeof (Runtime).GetMethod("CallCommand");

        private static readonly MethodInfo _callInternalFunctionMethod =
            typeof (Runtime).GetMethod("CallInternalFunction");

        public static readonly MethodInfo CallFunctionMethod =
            typeof (Runtime).GetMethod("CallFunction");

        private static readonly MethodInfo _CastMethod = typeof (Runtime).GetMethod("Cast");
        private static readonly MethodInfo _CheckTypeMethod = typeof (Runtime).GetMethod("CheckType");

        private static readonly MethodInfo _ConstructPTypeAsPValueMethod =
            typeof (Runtime).GetMethod("ConstructPTypeAsPValue");

        private static readonly MethodInfo _ExtractBoolMethod =
            typeof (Runtime).GetMethod("ExtractBool");

        private static readonly MethodInfo _LoadApplicationReferenceMethod =
            typeof (Runtime).GetMethod("LoadApplicationReference");

        private static readonly MethodInfo _LoadCommandReferenceMethod =
            typeof (Runtime).GetMethod("LoadCommandReference");

        private static readonly MethodInfo _LoadEngineReferenceMethod = typeof (Runtime).GetMethod
            ("LoadEngineReference");

        private static readonly MethodInfo _LoadFunctionReferenceInternalMethod =
            typeof (Runtime).GetMethod("LoadFunctionReferenceInternal");

        private static readonly MethodInfo _loadFunctionReferenceMethod =
            typeof (Runtime).GetMethod("LoadFunctionReference");

        private static readonly MethodInfo _LoadGlobalVariableReferenceAsPValueMethod =
            typeof (Runtime).GetMethod("LoadGlobalVariableReferenceAsPValue");

        private static readonly MethodInfo _NewClosureMethod_LateBound = typeof (Runtime).GetMethod(
            "NewClosureInternal", new[] {typeof (StackContext), typeof (PVariable[]), typeof (string)});

        private static readonly MethodInfo _NewClosureMethod_StaticallyBound = typeof (Runtime).
            GetMethod(
                "NewClosure",
                new[] {typeof (StackContext), typeof (PVariable[]), typeof (PFunction)});

        private static readonly MethodInfo _newClosureMethodCrossModule =
            typeof (Runtime).GetMethod("NewClosure",
                new[]
                    {
                        typeof (StackContext), typeof (PVariable[]), typeof (string),
                        typeof (ModuleName)
                    });

        private static readonly MethodInfo _NewObjMethod = typeof (Runtime).GetMethod("NewObj");
        private static readonly MethodInfo _NewTypeMethod = typeof (Runtime).GetMethod("NewType");

        private static readonly MethodInfo _ParseExceptionMethod =
            typeof (Runtime).GetMethod("ParseException");

        private static readonly MethodInfo _RaiseToPowerMethod =
            typeof (Runtime).GetMethod("RaiseToPower");

        private static readonly MethodInfo _StaticCallMethod =
            typeof (PType).GetMethod
                (
                    "StaticCall",
                    new[]
                        {typeof (StackContext), typeof (PValue[]), typeof (PCall), typeof (string)});

        private static readonly MethodInfo _ThrowExceptionMethod =
            typeof (Runtime).GetMethod("ThrowException");

        public static readonly MethodInfo CheckTypeConstMethod =
            typeof (Runtime).GetMethod("CheckTypeConst", new[] {typeof (PValue), typeof (PType)});

        public static readonly MethodInfo ConstructPTypeMethod =
            typeof (StackContext).GetMethod("ConstructPType", new[] {typeof (string)});

        public static readonly MethodInfo DisposeIfPossibleMethod =
            typeof (Runtime).GetMethod("DisposeIfPossible", new[] {typeof (object)});

        public static readonly PValue[] EmptyPValueArray = new PValue[0];

        public static readonly FieldInfo EmptyPValueArrayField =
            typeof (Runtime).GetField("EmptyPValueArray");

        public static readonly PVariable[] EmptyPVariableArray = new PVariable[0];

        public static readonly FieldInfo EmptyPVariableArrayField =
            typeof (Runtime).GetField("EmptyPVariableArray");

        public static readonly MethodInfo ExtractEnumeratorMethod =
            typeof (Runtime).GetMethod("ExtractEnumerator",
                new[] {typeof (PValue), typeof (StackContext)});

        public static readonly MethodInfo LoadGlobalVariableReferenceMethod =
            typeof (Runtime).GetMethod("LoadGlobalVariableReference");

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Coroutine")] public static readonly MethodInfo NewCoroutineMethod =
                typeof (Runtime).GetMethod("NewCoroutine");

        public static readonly MethodInfo WrapPVariableMethod =
            typeof (Runtime).GetMethod("WrapPVariable");

        public static readonly MethodInfo CastConstMethod =
            typeof (Runtime).GetMethod("CastConst",
                new[] {typeof (PValue), typeof (PType), typeof (StackContext)});

        private static readonly MethodInfo _loadModuleNameAsPValueMethod =
            typeof (Runtime).GetMethod("LoadModuleNameAsPValue");

        private static readonly MethodInfo _loadGlobalVariableReferenceInternalMethod =
            typeof (Runtime).GetMethod("LoadGlobalVariableReferenceInternal");

        private static readonly MethodInfo _loadModuleNameMethod =
            typeof(Runtime).GetMethod("LoadModuleName");

        private static readonly MethodInfo _loadGlobalReferenceAsPValueInternalMethod =
            typeof (Runtime).GetMethod("LoadGlobalReferenceAsPValueInternal");

        public static MethodInfo LoadGlobalVariableReferenceAsPValueMethod
        {
            get { return _LoadGlobalVariableReferenceAsPValueMethod; }
        }

        public static MethodInfo LoadFunctionReferenceInternalMethod
        {
            get { return _LoadFunctionReferenceInternalMethod; }
        }

        public static MethodInfo LoadApplicationReferenceMethod
        {
            get { return _LoadApplicationReferenceMethod; }
        }

        public static MethodInfo LoadEngineReferenceMethod
        {
            get { return _LoadEngineReferenceMethod; }
        }

        public static MethodInfo LoadCommandReferenceMethod
        {
            get { return _LoadCommandReferenceMethod; }
        }

        public static MethodInfo ConstructPTypeAsPValueMethod
        {
            get { return _ConstructPTypeAsPValueMethod; }
        }

        public static MethodInfo NewObjMethod
        {
            get { return _NewObjMethod; }
        }

        public static MethodInfo NewTypeMethod
        {
            get { return _NewTypeMethod; }
        }

        public static MethodInfo NewClosureMethodLateBound
        {
            get { return _NewClosureMethod_LateBound; }
        }

        public static MethodInfo NewClosureMethodStaticallyBound
        {
            get { return _NewClosureMethod_StaticallyBound; }
        }

        public static MethodInfo RaiseToPowerMethod
        {
            get { return _RaiseToPowerMethod; }
        }

        public static MethodInfo CheckTypeMethod
        {
            get { return _CheckTypeMethod; }
        }

        public static MethodInfo CastMethod
        {
            get { return _CastMethod; }
        }

        public static MethodInfo StaticCallMethod
        {
            get { return _StaticCallMethod; }
        }

        public static MethodInfo CallInternalFunctionMethod
        {
            get { return _callInternalFunctionMethod; }
        }

        public static MethodInfo CallCommandMethod
        {
            get { return _CallCommandMethod; }
        }

        public static MethodInfo ThrowExceptionMethod
        {
            get { return _ThrowExceptionMethod; }
        }

        public static MethodInfo ExtractBoolMethod
        {
            get { return _ExtractBoolMethod; }
        }

        public static MethodInfo ParseExceptionMethod
        {
            get { return _ParseExceptionMethod; }
        }

        public static MethodInfo LoadModuleNameAsPValueMethod
        {
            get { return _loadModuleNameAsPValueMethod; }
        }

        public static MethodInfo LoadModuleNameMethod
        {
            get { return _loadModuleNameMethod; }
        }

        public static MethodInfo LoadGlobalVariableReferenceInternalMethod
        {
            get { return _loadGlobalVariableReferenceInternalMethod; }
        }

        public static MethodInfo LoadGlobalReferenceAsPValueInternalMethod
        {
            get { return _loadGlobalReferenceAsPValueInternalMethod; }
        }

        public static MethodInfo NewClosureMethodCrossModule
        {
            get { return _newClosureMethodCrossModule; }
        }

        public static MethodInfo LoadFunctionReferenceMethod
        {
            get { return _loadFunctionReferenceMethod; }
        }

        // ReSharper restore InconsistentNaming

// ReSharper disable UnusedMember.Global
        public static PValue LoadGlobalVariableReferenceAsPValue(StackContext sctx, string id, ModuleName  moduleName)
        {
            Application application;
            if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out application))
                throw new PrexoniteException(
                    string.Format(
                        "Cannot find instance of module {0} containing global variable {1}.",
                        moduleName, id));
            PVariable pv;
            if (!application.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException
                    (
                    string.Format(
                        "Cannot load reference to non existant global variable {0} in module {1}.",
                        id, moduleName));
            sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
            return sctx.CreateNativePValue(pv);
        }

        public static PValue LoadGlobalVariableReferenceAsPValueInternal(StackContext sctx, string id)
        {
            PVariable pv;
            if (!sctx.ParentApplication.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException
                    (
                    "Cannot load reference to non existant global variable " + id);
            sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
            return sctx.CreateNativePValue(pv);
        }

        public static PVariable LoadGlobalVariableReference(StackContext sctx, string id, ModuleName moduleName)
        {
            Application application;
            if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out application))
                throw new PrexoniteException(
                    string.Format(
                        "Cannot find instance of module {0} containing global variable {1}.",
                        moduleName, id));
            PVariable pv;
            if (!application.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException
                    (
                    string.Format(
                        "Cannot load reference to non existant global variable {0} in module {1}.",
                        id, moduleName));
            sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
            return pv;
        }

        public static PVariable LoadGlobalVariableReferenceInternal(StackContext sctx, string id)
        {
            PVariable pv;
            if (!sctx.ParentApplication.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException
                    (
                    "Cannot load reference to non existant internal global variable " + id);
            sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
            return pv;
        }

        public static PValue LoadFunctionReferenceInternal(StackContext sctx, string id)
        {
            PFunction func;
            if (!sctx.ParentApplication.Functions.TryGetValue(id, out func))
                throw new PrexoniteException("Cannot load reference to non existing internal function " + id);
            return sctx.CreateNativePValue(func);
        }

        public static PValue LoadFunctionReference(StackContext sctx, string internalId, ModuleName moduleName)
        {
            Application application;
            if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out application))
                throw new PrexoniteException(
                    string.Format(
                        "Cannot find instance of module {0} containing function {1}.",
                        moduleName, internalId));

            PFunction func;
            if (!application.Functions.TryGetValue(internalId, out func))
                throw new PrexoniteException(string.Format("Cannot load reference to non existing function {0} in module {1}.", internalId, moduleName));
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

        public static PValue ConstructPTypeAsPValue(StackContext sctx, string expr)
        {
            return sctx.CreateNativePValue(sctx.ConstructPType(expr));
        }

        public static PValue NewObj(StackContext sctx, PValue[] args, string expr)
        {
            return sctx.ConstructPType(expr).Construct(sctx, args);
        }

        public static PValue NewType(StackContext sctx, PValue[] args, string name)
        {
            return sctx.CreateNativePValue(sctx.ParentEngine.CreatePType(sctx, name, args));
        }

        public static PValue NewClosureInternal(StackContext sctx, PVariable[] sharedVariables,
            string funcId)
        {
            PFunction func;
            if (!sctx.ParentApplication.Functions.TryGetValue(funcId, out func))
                throw new PrexoniteException(string.Format("Cannot create closure for non existant function {0}", funcId));
            return NewClosure(sctx, sharedVariables, func);
        }

        public static PValue NewClosure(StackContext sctx, PVariable[] sharedVariables, string internalId, ModuleName moduleName)
        {
            Application application;
            if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out application))
                throw new PrexoniteException(
                    string.Format(
                        "Cannot find instance of module {0} containing function {1}.",
                        moduleName, internalId));

            PFunction func;
            if (!application.Functions.TryGetValue(internalId, out func))
                throw new PrexoniteException(
                    string.Format(
                        "Cannot create closure for non-existant function {0} in module {1}.",
                        internalId, moduleName));
            return NewClosure(sctx, sharedVariables, func);
        }

        public static PValue NewClosure(StackContext sctx, PVariable[] sharedVariables,
            PFunction function)
        {
            if (function.HasCilImplementation)
            {
                return
                    sctx.CreateNativePValue(new CilClosure(function,
                        sharedVariables ?? EmptyPVariableArray));
            }
            else
            {
                return
                    sctx.CreateNativePValue(new Closure(function,
                        sharedVariables ?? EmptyPVariableArray));
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
        public static PValue CallInternalFunction(StackContext sctx, PValue[] args, string id)
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

        public static PValue CallFunction(StackContext sctx, PValue[] args, string internalId, ModuleName moduleName)
        {
            Application application;
            if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out application))
                throw new PrexoniteException(
                    string.Format(
                        "Cannot find instance of module {0} containing function {1}.",
                        moduleName, internalId));

            PFunction func;
            if (!application.Functions.TryGetValue(internalId, out func))
                throw new PrexoniteException(string.Format("Cannot call non existant function {0} in module {1}.", internalId, moduleName));
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

        public static Boolean ExtractBool(PValue left, StackContext sctx)
        {
            if (!ReferenceEquals(left.Type, PType.Bool))
                left = left.ConvertTo(sctx, PType.Bool);
            return (bool) left.Value;
        }

        public static PValue CreateList(StackContext sctx, params PValue[] args)
        {
            return PType.List.CreatePValue(args);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Coroutine")]
        public static PValue NewCoroutine(PValue routine, StackContext sctx, PValue[] argv)
        {
            var routineobj = routine.Value;

            if (routineobj == null)
            {
                return PType.Null.CreatePValue();
            }
            else
            {
                StackContext corctx;
                IStackAware routinesa;
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
                    PrexoniteRuntimeException.UnpackException(
                        rexc ?? PrexoniteRuntimeException.CreateRuntimeException(sctx, exc))
                );
        }

        /// <summary>
        ///     Extracts an IEnumerator[PValue] from the supplied value. The input can either directly supply a 
        ///     sequence of PValue objects or arbitrary CLR objects that will be transparently mapped to PValues with respect to the stack context.
        /// </summary>
        /// <param name = "value">The value sequence (implements IEnumerator or IEnumerator[PValue].</param>
        /// <param name = "sctx">The stack context-</param>
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

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames",
            MessageId = "obj")]
        public static void DisposeIfPossible(object obj)
        {
            var disposable = obj as IDisposable;

            if (disposable != null)
                disposable.Dispose();
        }

        public static PValue LoadModuleNameAsPValue(StackContext sctx, string id, Version version)
        {
            return sctx.CreateNativePValue(sctx.Cache[id, version]);
        }

        public static ModuleName LoadModuleName(StackContext sctx, string id, Version version)
        {
            return sctx.Cache[id, version];
        }

        // ReSharper restore UnusedMember.Global

        #region Nested type: EnumeratorWrapper

        /// <summary>
        ///     Used to transparently convert arbitrary sequences to PValue sequences
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