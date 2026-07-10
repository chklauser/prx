#region Namespace Imports

using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using Prexonite.Modular;

#endregion

namespace Prexonite.Compiler.Cil;

public class CompilerPass
{
    static int _numberOfPasses;

    [DebuggerStepThrough]
    static string _createNextTypeName(string? applicationId)
    {
        if (string.IsNullOrEmpty(applicationId))
            applicationId = "cilimpl";

        return applicationId + "_" + Interlocked.Increment(ref _numberOfPasses) + "";
    }

    readonly PersistedAssemblyBuilder? _assemblyBuilder;

    public PersistedAssemblyBuilder Assembly
    {
        [DebuggerStepThrough]
        get
        {
            if (!MakeAvailableForLinking)
                throw new NotSupportedException(
                    "The compiler pass is not configured to make implementations available for static linking."
                );
            return _assemblyBuilder;
        }
    }

    readonly ModuleBuilder? _moduleBuilder;

    public ModuleBuilder Module
    {
        [DebuggerStepThrough]
        get
        {
            if (!MakeAvailableForLinking)
                throw new NotSupportedException(
                    "The compiler pass is not configured to make implementations available for static linking."
                );
            return _moduleBuilder;
        }
    }

    readonly TypeBuilder? _typeBuilder;

    public TypeBuilder TargetType
    {
        [DebuggerStepThrough]
        get
        {
            if (!MakeAvailableForLinking)
                throw new NotSupportedException(
                    "The compiler pass is not configured to make implementations available for static linking."
                );
            return _typeBuilder;
        }
    }

    public CompilerPass(Application? app, bool makeAvailableForLinking)
    {
        MakeAvailableForLinking = makeAvailableForLinking;
        if (MakeAvailableForLinking)
        {
            var sequenceName = _createNextTypeName(app?.Id);
            var asmName = new AssemblyName(sequenceName);
            _assemblyBuilder = new PersistedAssemblyBuilder(asmName, typeof(object).Assembly) { };

            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(asmName.Name!);
            _typeBuilder = _moduleBuilder.DefineType(sequenceName);
        }
    }

    public MethodInfo DefineImplementationMethod(ModuleName moduleName, string id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));

        var parameterTypes = new[]
        {
            typeof(PFunction),
            typeof(StackContext),
            typeof(PValue[]),
            typeof(PVariable[]),
            typeof(PValue).MakeByRefType(),
            typeof(ReturnMode).MakeByRefType(),
        };

        var makeAvailableForLinking = MakeAvailableForLinking;
        if (makeAvailableForLinking)
        {
            //Create method stub

            var dm = TargetType.DefineMethod(
                id,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(void),
                parameterTypes
            );
            dm.DefineParameter(1, ParameterAttributes.In, "source");
            dm.DefineParameter(2, ParameterAttributes.In, "sctx");
            dm.DefineParameter(3, ParameterAttributes.In, "args");
            dm.DefineParameter(4, ParameterAttributes.In, "sharedVariables");
            dm.DefineParameter(5, ParameterAttributes.Out, "result");
            dm.DefineParameter(6, ParameterAttributes.Out, "returnMode");

            Implementations.Add(moduleName, id, dm);

            //Create function field
            var fb = TargetType.DefineField(
                _mkFieldName(moduleName, id),
                typeof(PFunction),
                FieldAttributes.Public | FieldAttributes.Static
            );
            FunctionFields.Add(moduleName, id, fb);

            return dm;
        }

        var cilm = new DynamicMethod(id, typeof(void), parameterTypes, typeof(Runtime));

        cilm.DefineParameter(1, ParameterAttributes.In, "source");
        cilm.DefineParameter(2, ParameterAttributes.In, "sctx");
        cilm.DefineParameter(3, ParameterAttributes.In, "args");
        cilm.DefineParameter(4, ParameterAttributes.In, "sharedVariables");
        cilm.DefineParameter(5, ParameterAttributes.Out, "result");

        Implementations.Add(moduleName, id, cilm);

        return cilm;
    }

    static string _mkFieldName(ModuleName moduleName, string id)
    {
        return $"{moduleName.Id}/{moduleName.Version}/{id}<src>";
    }

    readonly ModuleSymbolTable<FieldInfo> _functionFieldTable = new();

    public IModuleSymbolTable<FieldInfo> FunctionFields
    {
        get
        {
            if (!MakeAvailableForLinking)
                throw new NotSupportedException(
                    "The compiler pass is not configured to make implementations available for static linking."
                );
            return _functionFieldTable;
        }
    }

    public ModuleSymbolTable<MethodInfo> Implementations { [DebuggerStepThrough] get; } = new();

    public ILGenerator GetIlGenerator(ModuleName moduleName, string id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
        if (!Implementations.TryGetValue(moduleName, id, out var m))
            throw new PrexoniteException(
                "No implementation stub for a function named " + id + " exists."
            );

        return GetIlGenerator(m);
    }

    public static ILGenerator GetIlGenerator(MethodInfo m)
    {
        DynamicMethod? dm;
        MethodBuilder? mb;
        if ((dm = m as DynamicMethod) != null)
            return dm.GetILGenerator();
        if ((mb = m as MethodBuilder) != null)
            return mb.GetILGenerator();
        throw new PrexoniteException(
            "CIL Implementation "
                + m.Name
                + " is neither a dynamic method nor a method builder but a "
                + m.GetType()
        );
    }

    [MemberNotNullWhen(
        true,
        nameof(_assemblyBuilder),
        nameof(_moduleBuilder),
        nameof(_typeBuilder)
    )]
    public bool MakeAvailableForLinking { get; }

    public CompilerPass(FunctionLinking linking)
        : this(null, linking) { }

    public CompilerPass(bool makeAvailableForLinking)
        : this(null, makeAvailableForLinking) { }

    public CompilerPass(Application? app, FunctionLinking linking)
        : this(
            app,
            (linking & FunctionLinking.AvailableForLinking) == FunctionLinking.AvailableForLinking
        ) { }

    readonly Dictionary<MethodInfo, ICilImplementation> _delegateCache = new();

    Type? _cachedTypeReference;

    public ICilImplementation GetImplementation(ModuleName moduleName, string id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
        if (!Implementations.TryGetValue(moduleName, id, out var m))
            throw new PrexoniteException(
                $"No implementation for a function named {id} in module {moduleName} exists."
            );

        return getDelegate(m);
    }

    record CilImplementation(MethodInfo Declaration, CilFunction Implementation)
        : ICilImplementation;

    ICilImplementation getDelegate(MethodInfo m)
    {
        if (_delegateCache.TryGetValue(m, out var @delegate))
            return @delegate;

        DynamicMethod? dm;
        if ((dm = m as DynamicMethod) != null)
            return _delegateCache[m] = new CilImplementation(
                m,
                (CilFunction)dm.CreateDelegate(typeof(CilFunction))
            );
        return _delegateCache[m] = new CilImplementation(
            m,
            (CilFunction)
                Delegate.CreateDelegate(
                    typeof(CilFunction),
                    _getRuntimeType().GetMethod(m.Name)!,
                    true
                )!
        ); // !-safety: throwOnBindFailure = true
    }

    Type _getRuntimeType()
    {
        if (_cachedTypeReference == null)
        {
            if (!MakeAvailableForLinking)
            {
                throw new PrexoniteException(
                    "Cannot get CIL implementation type when static linking is not enabled."
                );
            }

            // Emit type IL
            _ = TargetType.CreateType();

            // Unlike the old .NET implementation of Reflection.Emit, we have to decide between
            // a persisted and a runtime assembly builder. They are essentially two separate implementations of the
            // Reflection.Emit interface. The persisted assembly builder has the drawback that the types it generates
            // cannot be used without serializing the IL and loading it as an assembly.
            // It's a bit misleading that Microsoft kept the old Reflection.Emit API where `CreateType` returns a "Type".
            // That type is secretly still a type builder and can thus not be used with Delegate.CreateDelegate.
            using var mem = new MemoryStream();
            _assemblyBuilder.Save(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var ldCtx =
                AssemblyLoadContext.GetLoadContext(typeof(CompilerPass).Assembly)
                ?? throw new PrexoniteException("Failed to construct assembly load context.");
            var loaded = ldCtx.LoadFromStream(mem);

            _cachedTypeReference =
                loaded.GetType(TargetType.FullName!, throwOnError: true)
                ?? throw new PrexoniteException(
                    "Generated assembly did not contain the type that holds CIL implementations."
                );
        }

        return _cachedTypeReference;
    }

    public void LinkMetadata(PFunction func)
    {
        if (!MakeAvailableForLinking)
            return;

        var T = _getRuntimeType();

        var functionField = T.GetField(_mkFieldName(func.ParentApplication.Module.Name, func.Id));
        if (functionField == null)
        {
            throw new PrexoniteException(
                "Internal error. Expected a generated field for function {func}."
            );
        }

        functionField.SetValue(null, func);
    }
}
