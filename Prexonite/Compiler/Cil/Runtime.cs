using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Prexonite.Commands;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Cil;

// ReSharper disable InconsistentNaming
public static class Runtime
{
    public static readonly MethodInfo CallFunctionMethod =
        typeof (Runtime).GetMethod(nameof(CallFunction));

    public static readonly MethodInfo CheckTypeConstMethod =
        typeof (Runtime).GetMethod(nameof(CheckTypeConst), new[] {typeof (PValue), typeof (PType)});

    public static readonly MethodInfo ConstructPTypeMethod =
        typeof (StackContext).GetMethod(nameof(StackContext.ConstructPType), new[] {typeof (string)});

    public static readonly MethodInfo DisposeIfPossibleMethod =
        typeof (Runtime).GetMethod(nameof(DisposeIfPossible), new[] {typeof (object)});

    public static readonly PValue[] EmptyPValueArray = Array.Empty<PValue>();

    public static readonly FieldInfo EmptyPValueArrayField =
        typeof (Runtime).GetField(nameof(EmptyPValueArray));

    public static readonly MethodInfo ExtractEnumeratorMethod =
        typeof (Runtime).GetMethod(nameof(ExtractEnumerator),
            new[] {typeof (PValue), typeof (StackContext)});

    public static readonly MethodInfo LoadGlobalVariableReferenceMethod =
        typeof (Runtime).GetMethod(nameof(LoadGlobalVariableReference));

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Coroutine")] public static readonly MethodInfo NewCoroutineMethod =
        typeof (Runtime).GetMethod(nameof(NewCoroutine));

    public static readonly MethodInfo WrapPVariableMethod =
        typeof(Runtime).GetMethod(nameof(WrapPVariable));

    public static readonly MethodInfo CastConstMethod =
        typeof (Runtime).GetMethod(nameof(CastConst),
            new[] {typeof (PValue), typeof (PType), typeof (StackContext)});

    public static MethodInfo LoadGlobalVariableReferenceAsPValueMethod { get; } = typeof(Runtime).GetMethod(nameof(LoadGlobalVariableReferenceAsPValue));

    public static MethodInfo LoadFunctionReferenceInternalMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadFunctionReferenceInternal));

    public static MethodInfo LoadApplicationReferenceMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadApplicationReference));

    public static MethodInfo LoadEngineReferenceMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadEngineReference));

    public static MethodInfo LoadCommandReferenceMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadCommandReference));

    public static MethodInfo ConstructPTypeAsPValueMethod { get; } = typeof (Runtime).GetMethod(nameof(ConstructPTypeAsPValue));

    public static MethodInfo NewTypeMethod { get; } = typeof (Runtime).GetMethod(nameof(NewType));

    public static MethodInfo NewClosureMethodLateBound { get; } = typeof(Runtime).GetMethod(nameof(NewClosureInternal), new[] {typeof (StackContext), typeof (PVariable[]), typeof (string)});

    public static MethodInfo NewClosureMethodStaticallyBound { get; } = typeof(Runtime).GetMethod(nameof(NewClosure),
            new[] {typeof (StackContext), typeof (PVariable[]), typeof (PFunction)});

    public static MethodInfo RaiseToPowerMethod { get; } = typeof (Runtime).GetMethod(nameof(RaiseToPower));

    public static MethodInfo CheckTypeMethod { get; } = typeof (Runtime).GetMethod(nameof(CheckType));

    public static MethodInfo CastMethod { get; } = typeof (Runtime).GetMethod(nameof(Cast));

    public static MethodInfo StaticCallMethod { get; } = typeof(PType).GetMethod(nameof(PType.StaticCall),
        new[] {typeof (StackContext), typeof (PValue[]), typeof (PCall), typeof (string)});

    public static MethodInfo CallInternalFunctionMethod { get; } = typeof (Runtime).GetMethod(nameof(CallInternalFunction));

    public static MethodInfo CallCommandMethod { get; } = typeof (Runtime).GetMethod(nameof(CallCommand));

    public static MethodInfo ThrowExceptionMethod { get; } = typeof (Runtime).GetMethod(nameof(ThrowException));

    public static MethodInfo ExtractBoolMethod { get; } = typeof (Runtime).GetMethod(nameof(ExtractBool));

    public static MethodInfo ParseExceptionMethod { get; } = typeof (Runtime).GetMethod(nameof(ParseException));

    public static MethodInfo LoadModuleNameAsPValueMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadModuleNameAsPValue));

    public static MethodInfo LoadModuleNameMethod { get; } = typeof(Runtime).GetMethod(nameof(LoadModuleName));

    public static MethodInfo LoadGlobalVariableReferenceInternalMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadGlobalVariableReferenceInternal));

    public static MethodInfo LoadGlobalReferenceAsPValueInternalMethod { get; } = typeof(Runtime).GetMethod(nameof(LoadGlobalVariableReferenceAsPValueInternal));

    public static MethodInfo NewClosureMethodCrossModule { get; } = typeof (Runtime).GetMethod(nameof(NewClosure),
        new[]
        {
            typeof (StackContext), typeof (PVariable[]), typeof (string),
            typeof (ModuleName)
        });

    public static MethodInfo LoadFunctionReferenceMethod { get; } = typeof (Runtime).GetMethod(nameof(LoadFunctionReference));

    // ReSharper restore InconsistentNaming

// ReSharper disable UnusedMember.Global
    public static PValue LoadGlobalVariableReferenceAsPValue(StackContext sctx, string id, ModuleName  moduleName)
    {
        if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out var application))
            throw new PrexoniteException(
                $"Cannot find instance of module {moduleName} containing global variable {id}.");
        if (!application.Variables.TryGetValue(id, out var pv))
            throw new PrexoniteException
            (
                $"Cannot load reference to non existent global variable {id} in module {moduleName}.");
        sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
        return sctx.CreateNativePValue(pv);
    }

    public static PValue LoadGlobalVariableReferenceAsPValueInternal(StackContext sctx, string id)
    {
        if (!sctx.ParentApplication.Variables.TryGetValue(id, out var pv))
            throw new PrexoniteException
            (
                "Cannot load reference to non existent global variable " + id);
        sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
        return sctx.CreateNativePValue(pv);
    }

    public static PVariable LoadGlobalVariableReference(StackContext sctx, string id, ModuleName moduleName)
    {
        if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out var application))
            throw new PrexoniteException(
                $"Cannot find instance of module {moduleName} containing global variable {id}.");
        if (!application.Variables.TryGetValue(id, out var pv))
            throw new PrexoniteException
            (
                $"Cannot load reference to non existent global variable {id} in module {moduleName}.");
        sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
        return pv;
    }

    public static PVariable LoadGlobalVariableReferenceInternal(StackContext sctx, string id)
    {
        if (!sctx.ParentApplication.Variables.TryGetValue(id, out var pv))
            throw new PrexoniteException
            (
                "Cannot load reference to non existent internal global variable " + id);
        sctx.ParentApplication.EnsureInitialization(sctx.ParentEngine);
        return pv;
    }

    public static PValue LoadFunctionReferenceInternal(StackContext sctx, string id)
    {
        if (!sctx.ParentApplication.Functions.TryGetValue(id, out var func))
            throw new PrexoniteException(
                $"Cannot load reference to non existing internal function {id} in module {sctx.ParentApplication.Module.Name}");
        return sctx.CreateNativePValue(func);
    }

    public static PValue LoadFunctionReference(StackContext sctx, string internalId, ModuleName moduleName)
    {
        if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out var application))
            throw new PrexoniteException(
                $"Cannot find instance of module {moduleName} containing function {internalId}.");

        if (!application.Functions.TryGetValue(internalId, out var func))
            throw new PrexoniteException(
                $"Cannot load reference to non existing function {internalId} in module {moduleName}.");
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
        if (!sctx.ParentEngine.Commands.TryGetValue(id, out var cmd))
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
        if (!sctx.ParentApplication.Functions.TryGetValue(funcId, out var func))
            throw new PrexoniteException($"Cannot create closure for non existent function {funcId}");
        return NewClosure(sctx, sharedVariables, func);
    }

    public static PValue NewClosure(StackContext sctx, PVariable[] sharedVariables, string internalId, ModuleName moduleName)
    {
        if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out var application))
            throw new PrexoniteException(
                $"Cannot find instance of module {moduleName} containing function {internalId}.");

        if (!application.Functions.TryGetValue(internalId, out var func))
            throw new PrexoniteException(
                $"Cannot create closure for non-existent function {internalId} in module {moduleName}.");
        return NewClosure(sctx, sharedVariables, func);
    }

    public static PValue NewClosure(StackContext sctx, PVariable[] sharedVariables,
        PFunction function)
    {
        PVariable[] emptyPVariableArray = Array.Empty<PVariable>();
        if (function.HasCilImplementation)
        {
            return
                sctx.CreateNativePValue(new CilClosure(function,
                    sharedVariables ?? emptyPVariableArray));
        }
        else
        {
            return
                sctx.CreateNativePValue(new Closure(function,
                    sharedVariables ?? emptyPVariableArray));
        }
    }

    public static PValue RaiseToPower(PValue left, PValue right, StackContext sctx)
    {
        if (
            !(left.TryConvertTo(sctx, PType.Real, out var rleft) &&
                right.TryConvertTo(sctx, PType.Real, out var rright)))
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
        if (!sctx.ParentApplication.Functions.TryGetValue(id, out var func))
            throw new PrexoniteException("Cannot call non existent function " + id);
        if (func.CilImplementation is {} cilImplementation)
        {
            // Can keep the same stack context
            cilImplementation(func, sctx, args, null, out var result, out _);
            return result;
        }
        else
        {
            return func.Run(sctx.ParentEngine, args);
        }
    }

    public static PValue CallFunction(StackContext sctx, PValue[] args, string internalId, ModuleName moduleName)
    {
        if (!sctx.ParentApplication.Compound.TryGetApplication(moduleName, out var application))
            throw new PrexoniteException(
                $"Cannot find instance of module {moduleName} containing function {internalId}.");

        if (!application.Functions.TryGetValue(internalId, out var func))
            throw new PrexoniteException($"Cannot call non existent function {internalId} in module {moduleName}.");
        if (func.CilImplementation is {} cilImplementation)
        {
            var callCtx = sctx.ParentApplication == func.ParentApplication 
                ? sctx 
                : CilFunctionContext.New(sctx, func);
            cilImplementation(func, callCtx, args, null, out var result, out _);
            return result;
        }
        else
        {
            return func.Run(sctx.ParentEngine, args);
        }
    }

    public static PValue CallCommand(StackContext sctx, PValue[] args, string id)
    {
        if (!sctx.ParentEngine.Commands.TryGetValue(id, out var cmd))
            throw new PrexoniteException("Cannot call non existent command " + id);
        if (cmd is StackAwareCommand sacmd)
        {
            var cctx = sacmd.CreateStackContext(sctx, args);
            return sctx.ParentEngine.Process(cctx);
        }
        else
        {
            return cmd.Run(sctx, args);
        }
    }

    public static void ThrowException(PValue obj, StackContext sctx) =>
        throw (obj.Type switch
        {
            StringPType => PrexoniteRuntimeException.CreateRuntimeException(sctx, (string) obj.Value),
            ObjectPType when obj.Value is Exception exception => PrexoniteRuntimeException.CreateRuntimeException(
                sctx, exception.Message, exception),
            _ => PrexoniteRuntimeException.CreateRuntimeException(sctx, obj.CallToString(sctx))
        });

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
            throw new ArgumentNullException(nameof(sctx));

        IEnumerator genEn;

        if (value.Type is ObjectPType)
            genEn = (IEnumerator) value.Value;
        else
            genEn = value.ConvertTo<IEnumerator>(sctx, true);

        if (genEn is PValueEnumerator pvEn)
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

        disposable?.Dispose();
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

        readonly IEnumerator _enumerator;
        readonly StackContext _sctx;

        public EnumeratorWrapper(IEnumerator enumerator, StackContext sctx)
        {
            _enumerator = enumerator;
            _sctx = sctx;
        }

        #endregion

        #region IEnumerator<PValue> Members

        public PValue Current => _sctx.CreateNativePValue(_enumerator.Current);

        public void Dispose()
        {
            var disp = _enumerator as IDisposable;

            disp?.Dispose();
        }

        object IEnumerator.Current => Current;

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