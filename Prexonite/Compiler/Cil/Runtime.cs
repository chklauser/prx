using System;
using System.Collections;
using System.Reflection;
using Prexonite.Commands;
using Prexonite.Types;
using System.Collections.Generic;

namespace Prexonite.Compiler.Cil
{
    public static class Runtime
    {
        private static readonly MethodInfo _CallCommandMethod = typeof(Runtime).GetMethod("CallCommand");
        private static readonly MethodInfo _CallFunctionMethod = typeof(Runtime).GetMethod("CallFunction");
        private static readonly MethodInfo _CastMethod = typeof(Runtime).GetMethod("Cast");
        private static readonly MethodInfo _CheckTypeMethod = typeof(Runtime).GetMethod("CheckType");

        private static readonly MethodInfo _ConstructPTypeAsPValueMethod =
            typeof(Runtime).GetMethod("ConstructPTypeAsPValue");

        private static readonly MethodInfo _ExtractBoolMethod = typeof(Runtime).GetMethod("ExtractBool");

        private static readonly MethodInfo _LoadApplicationReferenceMethod =
            typeof(Runtime).GetMethod("LoadApplicationReference");

        private static readonly MethodInfo _LoadCommandReferenceMethod =
            typeof(Runtime).GetMethod("LoadCommandReference");

        private static readonly MethodInfo _LoadEngineReferenceMethod = typeof(Runtime).GetMethod("LoadEngineReference");

        private static readonly MethodInfo _LoadFunctionReferenceMethod =
            typeof(Runtime).GetMethod("LoadFunctionReference");

        private static readonly MethodInfo _LoadGlobalVariableReferenceAsPValueMethod =
            typeof(Runtime).GetMethod("LoadGlobalVariableReferenceAsPValue");

        private static readonly MethodInfo _NewClosureMethod = typeof(Runtime).GetMethod("NewClosure");
        private static readonly MethodInfo _NewObjMethod = typeof(Runtime).GetMethod("NewObj");
        private static readonly MethodInfo _NewTypeMethod = typeof(Runtime).GetMethod("NewType");
        private static readonly MethodInfo _ParseExceptionMethod = typeof(Runtime).GetMethod("ParseException");
        private static readonly MethodInfo _RaiseToPowerMethod = typeof(Runtime).GetMethod("RaiseToPower");
        private static readonly MethodInfo _StaticCallMethod = typeof(Runtime).GetMethod("StaticCall");
        private static readonly MethodInfo _ThrowExceptionMethod = typeof(Runtime).GetMethod("ThrowException");
        public static readonly PValue[] EmptyPValueArray = new PValue[0];
        internal static readonly FieldInfo EmptyPValueArrayField = typeof(Runtime).GetField("EmptyPValueArray");
        public static readonly PVariable[] EmptyPVariableArray = new PVariable[0];
        internal static readonly FieldInfo EmptyPVariableArrayField = typeof(Runtime).GetField("EmptyPVariableArray");

        internal static readonly MethodInfo LoadGlobalVariableReferenceMethod =
            typeof(Runtime).GetMethod("LoadGlobalVariableReference");

        internal static readonly MethodInfo NewCoroutineMethod = typeof(Runtime).GetMethod("NewCoroutine");
        internal static readonly MethodInfo WrapPVariableMethod = typeof(Runtime).GetMethod("WrapPVariable");

        internal static MethodInfo LoadGlobalVariableReferenceAsPValueMethod
        {
            get
            {
                return _LoadGlobalVariableReferenceAsPValueMethod;
            }
        }

        internal static MethodInfo LoadFunctionReferenceMethod
        {
            get
            {
                return _LoadFunctionReferenceMethod;
            }
        }

        internal static MethodInfo LoadApplicationReferenceMethod
        {
            get
            {
                return _LoadApplicationReferenceMethod;
            }
        }

        internal static MethodInfo LoadEngineReferenceMethod
        {
            get
            {
                return _LoadEngineReferenceMethod;
            }
        }

        internal static MethodInfo LoadCommandReferenceMethod
        {
            get
            {
                return _LoadCommandReferenceMethod;
            }
        }

        internal static MethodInfo ConstructPTypeAsPValueMethod
        {
            get
            {
                return _ConstructPTypeAsPValueMethod;
            }
        }

        internal static MethodInfo NewObjMethod
        {
            get
            {
                return _NewObjMethod;
            }
        }

        internal static MethodInfo NewTypeMethod
        {
            get
            {
                return _NewTypeMethod;
            }
        }

        internal static MethodInfo NewClosureMethod
        {
            get
            {
                return _NewClosureMethod;
            }
        }

        internal static MethodInfo RaiseToPowerMethod
        {
            get
            {
                return _RaiseToPowerMethod;
            }
        }

        internal static MethodInfo CheckTypeMethod
        {
            get
            {
                return _CheckTypeMethod;
            }
        }

        internal static MethodInfo CastMethod
        {
            get
            {
                return _CastMethod;
            }
        }

        internal static MethodInfo StaticCallMethod
        {
            get
            {
                return _StaticCallMethod;
            }
        }

        internal static MethodInfo CallFunctionMethod
        {
            get
            {
                return _CallFunctionMethod;
            }
        }

        internal static MethodInfo CallCommandMethod
        {
            get
            {
                return _CallCommandMethod;
            }
        }

        internal static MethodInfo ThrowExceptionMethod
        {
            get
            {
                return _ThrowExceptionMethod;
            }
        }

        internal static MethodInfo ExtractBoolMethod
        {
            get
            {
                return _ExtractBoolMethod;
            }
        }

        internal static MethodInfo ParseExceptionMethod
        {
            get
            {
                return _ParseExceptionMethod;
            }
        }

        public static PValue LoadGlobalVariableReferenceAsPValue(StackContext sctx, string id)
        {
            PVariable pv;
            if(!sctx.ParentApplication.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException(
                    "Cannot load reference to non existant global variable " + id);
            return sctx.CreateNativePValue(pv);
        }

        public static PVariable LoadGlobalVariableReference(StackContext sctx, string id)
        {
            PVariable pv;
            if(!sctx.ParentApplication.Variables.TryGetValue(id, out pv))
                throw new PrexoniteException(
                    "Cannot load reference to non existant global variable " + id);
            return pv;
        }

        public static PValue LoadFunctionReference(StackContext sctx, string id)
        {
            PFunction func;
            if(!sctx.ParentApplication.Functions.TryGetValue(id, out func))
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
            if(!sctx.ParentEngine.Commands.TryGetValue(id, out cmd))
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
            if(sharedVariables == null)
                sharedVariables = EmptyPVariableArray;
            PFunction func;
            if(!sctx.ParentApplication.Functions.TryGetValue(funcId, out func))
                throw new PrexoniteException("Cannot create closure for non existant function " + funcId);
            if(func.HasCilImplementation)
            {
                return sctx.CreateNativePValue(new CilClosure(func, sharedVariables));
            }
            else
            {
                return sctx.CreateNativePValue(new Closure(func, sharedVariables));
            }
        }

        public static PValue RaiseToPower(PValue left, PValue right, StackContext sctx)
        {
            PValue rleft,
                   rright;
            if(
                !(left.TryConvertTo(sctx, PType.Real, out rleft) &&
                  right.TryConvertTo(sctx, PType.Real, out rright)))
                throw new PrexoniteException(
                    "The arguments supplied to the power operator are invalid (cannot be converted to Real).");
            return
                Math.Pow(Convert.ToDouble(rleft.Value), Convert.ToDouble(rright.Value));
        }

        public static PValue CheckType(PValue obj, PValue type)
        {
            return obj.Type.Equals(type.Value);
        }

        public static PValue Cast(PValue obj, PValue type, StackContext sctx)
        {
            return obj.ConvertTo(sctx, (PType) type.Value, true);
        }

        public static PValue StaticCall(StackContext sctx, string typeExpr, PValue[] args, PCall call, string memId)
        {
            return ConstructPType(sctx, typeExpr).StaticCall(sctx, args, call, memId);
        }

        //[System.Diagnostics.DebuggerHidden]
        public static PValue CallFunction(StackContext sctx, PValue[] args, string id)
        {
            PFunction func;
            if(!sctx.ParentApplication.Functions.TryGetValue(id, out func))
                throw new PrexoniteException("Cannot call non existant function " + id);
            if(func.HasCilImplementation)
            {
                PValue result;
                func.CilImplementation(func, sctx, args, null, out result);
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
            if(!sctx.ParentEngine.Commands.TryGetValue(id, out cmd))
                throw new PrexoniteException("Cannot call non existant command " + id);
            StackAwareCommand sacmd = cmd as StackAwareCommand;
            if(sacmd != null)
            {
                StackContext cctx = sacmd.CreateStackContext(sctx, args);
                return sctx.ParentEngine.Process(cctx);
            }
            else
            {
                return cmd.Run(sctx, args);
            }
        }

        public static void ThrowException(PValue obj, StackContext sctx)
        {
            PType t = obj.Type;
            PrexoniteRuntimeException prexc;
            if(t is StringPType)
                prexc =
                    PrexoniteRuntimeException.CreateRuntimeException(
                        sctx, (string) obj.Value);
            else if(t is ObjectPType && obj.Value is Exception)
                prexc =
                    PrexoniteRuntimeException.CreateRuntimeException(
                        sctx,
                        ((Exception) obj.Value).Message,
                        (Exception) obj.Value);
            else
                prexc =
                    PrexoniteRuntimeException.CreateRuntimeException(
                        sctx, obj.CallToString(sctx));

            throw prexc;
        }

        public static bool ExtractBool(PValue left, StackContext sctx)
        {
            if(left.Type != PType.Bool)
                left = left.ConvertTo(sctx, PType.Bool);
            return (bool) left.Value;
        }

        public static PValue CreateList(StackContext sctx, PValue[] args)
        {
            return PType.List.CreatePValue(args);
        }

        public static PValue NewCoroutine(PValue routine, StackContext sctx, PValue[] argv)
        {
            object routineobj = routine.Value;
            IStackAware routinesa = routineobj as IStackAware;
            if(routineobj == null)
            {
                return PType.Null.CreatePValue();
            }
            else
            {
                StackContext corctx;
                if(routinesa != null)
                    corctx = routinesa.CreateStackContext(sctx, argv);
                else
                    corctx = (StackContext)
                             routine.DynamicCall(
                                 sctx,
                                 new PValue[]
                                     {
                                         PType.Object.CreatePValue(sctx.ParentEngine),
                                         PType.Object.CreatePValue(argv)
                                     },
                                 PCall.Get,
                                 "CreateStackContext").Value;

                return
                    PType.Object[typeof(Coroutine)].CreatePValue(new Coroutine(corctx));
            }
        }

        public static PValue WrapPVariable(PVariable pv)
        {
            return PType.Object.CreatePValue(pv);
        }

        public static PValue ParseException(Exception exc, StackContext sctx)
        {
            PrexoniteRuntimeException rexc = exc as PrexoniteRuntimeException;
            return sctx.CreateNativePValue(
                rexc ?? PrexoniteRuntimeException.CreateRuntimeException(sctx, exc)
                );
        }

        public static IEnumerator<PValue> ExtractEnumerator(PValue value, StackContext sctx)
        {
            if(value == null)
                throw new ArgumentNullException("sctx");

            IEnumerator genEn;

            if (value.Type is ObjectPType)
                genEn =  (IEnumerator)value.Value;
            else
                genEn = value.ConvertTo<IEnumerator>(sctx, true);

            PValueEnumerator pvEn = genEn as PValueEnumerator;

            if (pvEn != null)
            {
                return pvEn;
            }
            else
            {
                return new EnumeratorWrapper(genEn, sctx);
            }
        }

        internal static readonly MethodInfo ExtractEnumeratorMethod =
            typeof(Runtime).GetMethod("ExtractEnumerator", new Type[] {typeof(PValue), typeof(StackContext)});

        public static void DisposeIfPossible(object obj)
        {
            IDisposable disposable = obj as IDisposable;

            if(disposable != null)
                disposable.Dispose();
        }

        internal static readonly MethodInfo DisposeIfPossibleMethod =
            typeof(Runtime).GetMethod("DisposeIfPossible", new Type[] {typeof(object)});

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
                get
                {
                    return _sctx.CreateNativePValue(_enumerator.Current);
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                IDisposable disp = _enumerator as IDisposable;

                if(disp != null)
                {
                    disp.Dispose();
                }
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get 
                {
                    return Current; 
                }
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
    }
}