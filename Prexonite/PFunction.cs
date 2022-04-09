#nullable enable
#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite;

/// <summary>
///     A function in the Prexonite Script VM.
/// </summary>
public class  PFunction : IHasMetaTable,
    IIndirectCall,
    IStackAware,
    IDependent<EntityRef.Function>
{
    /// <summary>
    ///     The meta key under which the function's id is stored.
    /// </summary>
    public const string IdKey = "id";

    /// <summary>
    ///     The meta key under which the list of shared names is stored.
    /// </summary>
    public const string SharedNamesKey = @"\sharedNames";

    /// <summary>
    ///     The name of the variable that holds the list of arguments.
    /// </summary>
    public const string ArgumentListId = "args";

    public const string SymbolMappingKey = @"\symbol_mapping";

    /// <summary>
    ///     Signals that the function cannot be compiled to CIL.
    /// </summary>
    public const string VolatileKey = "volatile";

    /// <summary>
    ///     Signals that the function operates on its caller and thus causes the caller to be volatile (<see cref = "VolatileKey" />).
    /// </summary>
    public const string DynamicKey = "dynamic";

    /// <summary>
    ///     The reason why a function was marked volatile by the CIL compiler.
    /// </summary>
    public const string DeficiencyKey = "deficiency";

    /// <summary>
    ///     The id used in the source code. (As the nested function)
    /// </summary>
    public const string LogicalIdKey = "LogicalId";

    /// <summary>
    ///     The name of the functions logical parent.
    /// </summary>
    public const string ParentFunctionKey = "ParentFunction";

    /// <summary>
    ///     Indicates whether a function requires its arguments to be lazy.
    /// </summary>
    public const string LazyKey = "lazy";

    /// <summary>
    ///     The list of let-bound local names (local variables and shared variables)
    /// </summary>
    public const string LetKey = "let";

    #region Construction

    /// <summary>
    ///     Creates a new instance of PFunction.
    /// </summary>
    /// <param name = "parentApplication">The application of which the new function is part of.</param>
    /// <param name="declaration">The declaration this PFunction is based on.</param>
    [DebuggerStepThrough]
    internal PFunction(Application parentApplication, FunctionDeclaration declaration)
    {
        if (parentApplication == null)
            throw new ArgumentNullException(nameof(parentApplication));
        if (declaration == null)
            throw new ArgumentNullException(nameof(declaration));

        if (!parentApplication.Module.Functions.Contains(declaration))
            throw new ArgumentException(
                $"The supplied application (instance of module {parentApplication.Module.Name}) does not define the function {declaration}.");

        ParentApplication = parentApplication;
        Declaration = declaration;
    }

    #endregion

    #region Properties

    public FunctionDeclaration Declaration { get; }

    /// <summary>
    ///     The functions id
    /// </summary>
    public string Id
    {
        [DebuggerStepThrough]
        get => Declaration.Id;
    }

    public string LogicalId
    {
        [DebuggerStepThrough]
        get => Declaration.Id;
    }

    /// <summary>
    ///     The application the function belongs to.
    /// </summary>
    public Application ParentApplication { get; }

    /// <summary>
    ///     The set of namespaces imported by this particular function.
    /// </summary>
    public SymbolCollection ImportedNamespaces
    {
        [DebuggerStepThrough]
        get => Declaration.ImportedClrNamespaces;
    }

    /// <summary>
    ///     The bytecode for this function.
    /// </summary>
    public List<Instruction> Code
    {
        [DebuggerStepThrough]
        get => Declaration.Code;
    }

    /// <summary>
    ///     The list of formal parameters for this function.
    /// </summary>
    public List<string> Parameters
    {
        [DebuggerStepThrough]
        get => Declaration.Parameters;
    }

    /// <summary>
    ///     The collection of variable names used by this function.
    /// </summary>
    public SymbolCollection Variables
    {
        [DebuggerStepThrough]
        get => Declaration.LocalVariables;
    }

    /// <summary>
    ///     The table that maps indices to local names.
    /// </summary>
    public SymbolTable<int> LocalVariableMapping
    {
        [DebuggerNonUserCode]
        get => Declaration.LocalVariableMapping;
    }

    public CilFunction? CilImplementation => Declaration.CilImplementation?.Implementation;

    public bool HasCilImplementation => Declaration.HasCilImplementation;

    public bool IsMacro => Declaration.IsMacro;

    #endregion

    #region Storage

    public IEnumerable<EntityRef.Function> GetDependencies()
    {
        return Declaration.GetDependencies();
    }

    /// <summary>
    ///     Returns a string describing the function.
    /// </summary>
    /// <returns>A string describing the function.</returns>
    /// <remarks>
    ///     If you need a complete string representation, use <see cref = "Store(StringBuilder)" />.
    /// </remarks>
    public override string ToString()
    {
        return Declaration.ToString() ?? Declaration.Id;
    }

    public void Store(StringBuilder buffer)
    {
        Declaration.Store(buffer);
    }

    public string Store()
    {
        return Declaration.Store();
    }

    public void Store(TextWriter writer)
    {
        Declaration.Store(writer);
    }

    #endregion

    #region IHasMetaTable Members

    /// <summary>
    ///     Returns a reference to the meta table associated with this function.
    /// </summary>
    public MetaTable Meta
    {
        [DebuggerNonUserCode]
        get => Declaration.Meta;
    }

    #endregion

    #region Invocation

    /// <summary>
    ///     Creates a new function context for execution.
    /// </summary>
    /// <param name = "engine">The engine in which to execute the function.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
    /// <param name = "suppressInitialization">A boolean indicating whether to suppress initialization of the parent application.</param>
    /// <returns>A function context for the execution of this function.</returns>
    [PublicAPI]
    internal FunctionContext CreateFunctionContext
    (
        Engine engine,
        PValue[]? args,
        PVariable[]? sharedVariables,
        bool suppressInitialization)
    {
        return
            new(
                engine, this, args, sharedVariables, suppressInitialization);
    }

    /// <summary>
    ///     Creates a new function context for execution.
    /// </summary>
    /// <param name = "engine">The engine in which to execute the function.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
    /// <returns>A function context for the execution of this function.</returns>
    [PublicAPI]
    public FunctionContext CreateFunctionContext
    (
        Engine engine, PValue[]? args, PVariable[]? sharedVariables)
    {
        return new(engine, this, args, sharedVariables);
    }

    /// <summary>
    ///     Creates a new function context for execution.
    /// </summary>
    /// <param name = "sctx">The stack context in which to create the new context.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <returns>A function context for the execution of this function.</returns>
    [PublicAPI]
    public FunctionContext CreateFunctionContext(StackContext sctx, PValue[]? args)
    {
        return CreateFunctionContext(sctx.ParentEngine, args);
    }


    /// <summary>
    ///     Creates a new function context for execution.
    /// </summary>
    /// <param name = "engine">The engine for which to create the new context.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <returns>A function context for the execution of this function.</returns>
    public FunctionContext CreateFunctionContext(Engine engine, PValue[]? args)
    {
        return new(engine, this, args);
    }

    /// <summary>
    ///     Creates a new function context for execution.
    /// </summary>
    /// <param name = "engine">The engine in which to execute the function.</param>
    /// <returns>A function context for the execution of this function.</returns>
    [PublicAPI]
    public FunctionContext CreateFunctionContext(Engine engine)
    {
        return new(engine, this);
    }

    /// <summary>
    ///     Executes the function on the supplied engine and returns the result.
    /// </summary>
    /// <param name = "engine">The engine in which to execute the function.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
    /// <returns>The value returned by the function or {null~Null}</returns>
    [PublicAPI]
    public PValue Run(Engine engine, PValue[]? args, PVariable[]? sharedVariables)
    {
        if (CilImplementation is {} cilImplementation)
        {
            //Fix #8
            ParentApplication.EnsureInitialization(engine);
            cilImplementation
            (
                this,
                new NullContext(engine, ParentApplication, ImportedNamespaces),
                args,
                sharedVariables,
                out var result, out _);
            return result;
        }
        else
        {
            var fctx = CreateFunctionContext(engine, args, sharedVariables);
            engine.Stack.AddLast(fctx);
            return engine.Process();
        }
    }

    /// <summary>
    ///     Executes the function on the supplied engine and returns the result.
    /// </summary>
    /// <param name = "engine">The engine in which to execute the function.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <returns>A function context for the execution of this function.</returns>
    /// <returns>The value returned by the function or {null~Null}</returns>
    [PublicAPI]
    public PValue Run(Engine engine, PValue[]? args)
    {
        return Run(engine, args, null);
    }

    /// <summary>
    ///     Executes the function on the supplied engine and returns the result.
    /// </summary>
    /// <param name = "engine">The engine in which to execute the function.</param>
    /// <returns>A function context for the execution of this function.</returns>
    /// <returns>The value returned by the function or {null~Null}</returns>
    [PublicAPI]
    public PValue Run(Engine engine)
    {
        return Run(engine, null);
    }

    #endregion

    #region IIndirectCall Members

    /// <summary>
    ///     Executes the function and returns its result.
    /// </summary>
    /// <param name = "sctx">The stack context from which the function is called.</param>
    /// <param name = "args">The list of arguments to be passed to the function.</param>
    /// <returns>The value returned by the function or {null~Null}</returns>
    PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[]? args)
    {
        return Run(sctx.ParentEngine, args);
    }

    #endregion

    #region IStackAware Members

    /// <summary>
    ///     Creates a new stack context for the execution of this function.
    /// </summary>
    /// <param name = "sctx">The engine in which to execute the function.</param>
    /// <param name = "args">The arguments to pass to the function.</param>
    /// <returns>A function context for the execution of this function.</returns>
    [DebuggerNonUserCode]
    StackContext IStackAware.CreateStackContext(StackContext sctx, PValue[]? args)
    {
        return CreateFunctionContext(sctx, args);
    }

    #endregion

    #region Exception Handling

    /// <summary>
    ///     Causes the set of try-catch-finally blocks to be re-read on the next occurence of an exception.
    /// </summary>
    public void InvalidateTryCatchFinallyBlocks()
    {
        Declaration.InvalidateTryCatchFinallyBlocks();
    }

    /// <summary>
    ///     The cached set of try-catch-finally blocks.
    /// </summary>
    public ReadOnlyCollection<TryCatchFinallyBlock> TryCatchFinallyBlocks => Declaration.TryCatchFinallyBlocks;

    #endregion

    EntityRef.Function INamed<EntityRef.Function>.Name => ((INamed<EntityRef.Function>) Declaration).Name;
}