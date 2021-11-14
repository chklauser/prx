#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Prexonite.Commands;
using Prexonite.Commands.Concurrency;
using Prexonite.Commands.Core;
using Prexonite.Commands.Core.Operators;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Commands.Lazy;
using Prexonite.Commands.List;
using Prexonite.Commands.Math;
using Prexonite.Commands.Text;
using Prexonite.Types;
using Char = Prexonite.Commands.Core.Char;
using Debug = Prexonite.Commands.Core.Debug;
using Range = Prexonite.Commands.List.Range;
using TypeExpressionScanner = Prexonite.Internal.Scanner;
using TypeExpressionParser = Prexonite.Internal.Parser;

namespace Prexonite;

/// <summary>
///     Prexonite virtual machines. Engines manage available <see cref = "PType">PTypes</see>, assemblies accessible by the virtual machine as well as available <see
///      cref = "Commands" />.
/// </summary>
public partial class Engine
{
    #region Static

    #region String Comparision standards

    /// <summary>
    ///     The string comparer used throughout the Prexonite VM.
    /// </summary>
    /// <remarks>
    ///     The current implementation is <strong>case-insensitive</strong>
    /// </remarks>
    public static readonly StringComparer DefaultStringComparer =
        StringComparer.OrdinalIgnoreCase;

    /// <summary>
    ///     This method is used throughout the whole Prexonite VM to compare strings.
    /// </summary>
    /// <param name = "x">The left operand.</param>
    /// <param name = "y">The right operand.</param>
    /// <remarks>
    ///     The current implementation is <strong>case-insensitive</strong>.
    /// </remarks>
    /// <returns>True if the two strings <paramref name = "x" /> and <paramref name = "y" /> are equal; false otherwise.</returns>
    [DebuggerStepThrough]
    public static bool StringsAreEqual(string? x, string? y)
    {
        return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    /// <summary>
    ///     Generates a random identifier using <see cref = "Guid" />.
    /// </summary>
    /// <returns>A unique identifier.</returns>
    public static string GenerateName()
    {
        return "gen" + Guid.NewGuid().ToString("N");
    }

    [DebuggerStepThrough]
    public static string GenerateName(string prefix)
    {
        return prefix + "\\" + GenerateName();
    }

    #endregion

    #region Meta

    /// <summary>
    ///     The metatable provided to store settings and information about the virtual machine. This table also provides default values, should both applications and functions lack specific entries.
    /// </summary>
    public MetaTable Meta { get; }

    #endregion

    #region PType management

    #region PType map

    private readonly Dictionary<Type, PType> _pTypeMap;

    /// <summary>
    ///     Provides access to the PType to CLR <see cref = "System.Type">Type</see> mapping.
    /// </summary>
    /// <remarks>
    ///     The property uses a proxy type to hide implementation details.
    /// </remarks>
    /// <seealso cref = "PTypeMapIterator" />
    public PTypeMapIterator PTypeMap { get; }

    /// <summary>
    ///     Proxy type that is used to provide access to the <see cref = "PType" /> to CLR <see cref = "System.Type">Type</see> mapping.
    /// </summary>
    /// <seealso cref = "Prexonite.Engine.PTypeMap" />
    [DebuggerStepThrough]
    public class PTypeMapIterator
    {
        private readonly Engine _outer;

        internal PTypeMapIterator(Engine outer)
        {
            _outer = outer;
        }

        /// <summary>
        ///     Provides readonly access to the number of mappings in the current engine.
        /// </summary>
        /// <value>The number of mappings in the current engine.</value>
        public int Count => _outer._pTypeMap.Count;

        /// <summary>
        ///     Provides index based access to the mappings using their CLR <see cref = "Type">Type</see> as the index.
        /// </summary>
        /// <param name = "clrType">CLR <see cref = "Type">Type</see> of a mapping.</param>
        /// <returns>The <see cref = "PType" /> of a mapping or <c><see cref = "PType.Object" />[<paramref name = "clrType" />]</c> if no such mapping exists.</returns>
        /// <exception cref = "ArgumentNullException"><paramref name = "value" /> is null.</exception>
        public PType this[Type clrType]
        {
            get
            {
                if (_outer._pTypeMap.ContainsKey(clrType))
                    return _outer._pTypeMap[clrType];
                else
                    return PType.Object[clrType];
            }
            set
            {
                if (_outer._pTypeMap.ContainsKey(clrType))
                {
                    _outer._pTypeMap[clrType] = value;
                }
                else
                {
                    _outer._pTypeMap.Add(clrType, value);
                }
            }
        }

        /// <summary>
        ///     Adds a new mapping from a CLR <see cref = "Type" /> to a <see cref = "PType" />.
        /// </summary>
        /// <param name = "clrType">The CLR <see cref = "Type" />.</param>
        /// <param name = "type">The <see cref = "PType" /></param>
        /// <exception cref = "ArgumentNullException">Either <paramref name = "clrType" /> or <paramref name = "type" /> is null.</exception>
        public void Add(Type clrType, PType type)
        {
            if (clrType == null)
                throw new ArgumentNullException(nameof(clrType));
            if ((object) type == null)
                throw new ArgumentNullException(nameof(type));
            if (_outer._pTypeMap.ContainsKey(clrType))
                throw new InvalidOperationException(
                    "A mapping for the CLR Type " + clrType.FullName +
                    " already exists");
            _outer._pTypeMap.Add(clrType, type);
        }

        /// <summary>
        ///     Removed a mapping identified by it's CLR <see cref = "Type" />.
        /// </summary>
        /// <param name = "clrType">The CLR <see cref = "Type" /> of the mapping to remove.</param>
        public void Remove(Type clrType)
        {
            if (clrType == null)
                throw new ArgumentNullException(nameof(clrType));
            if (_outer._pTypeMap.ContainsKey(clrType))
                _outer._pTypeMap.Remove(clrType);
        }

        /// <summary>
        ///     Returns an IEnumerator to enumerate over all mappings in the current engine.
        /// </summary>
        /// <returns>An IEnumerator to enumerate over all mappings in the current engine.</returns>
        public IEnumerator<KeyValuePair<Type, PType>> GetEnumerator()
        {
            return _outer._pTypeMap.GetEnumerator();
        }
    }

    #endregion

    #region PType registry

    private readonly SymbolTable<Type> _pTypeRegistry;

    /// <summary>
    ///     Provides access to the dictionary of <see cref = "PType">PTypes</see> registered 
    ///     for recognition in type expressions.
    /// </summary>
    /// <seealso cref = "PTypeRegistryIterator" />
    public PTypeRegistryIterator PTypeRegistry { get; }

    /// <summary>
    ///     The proxy class that is used to provide access to the <see cref = "Prexonite.Engine.PTypeRegistry" />
    /// </summary>
    /// <seealso cref = "Prexonite.Engine.PTypeRegistry" />
    [DebuggerStepThrough]
    public class PTypeRegistryIterator
    {
        private readonly Engine _outer;

        internal PTypeRegistryIterator(Engine outer)
        {
            _outer = outer;
        }

        /// <summary>
        ///     The number of registered <see cref = "PType">PTypes</see>.
        /// </summary>
        /// <value>The number of registered <see cref = "PType">PTypes</see>.</value>
        public int Count => _outer._pTypeRegistry.Count;

        /// <summary>
        ///     Determines whether a particular type name has been registered.
        /// </summary>
        /// <param name = "name">A name to check for registration.</param>
        /// <returns>True if the name has been registered; false otherwise.</returns>
        public bool Contains(string name)
        {
            return _outer._pTypeRegistry.ContainsKey(name);
        }

        public bool TryGet(string name, [NotNullWhen(true)] out Type? result)
        {
            return _outer._pTypeRegistry.TryGetValue(name, out result);
        }

        /// <summary>
        ///     Provides index based access to the <see cref = "Prexonite.Engine.PTypeRegistry" />.
        /// </summary>
        /// <param name = "name">The name of the <see cref = "PType" /> to lookup.</param>
        /// <returns>A <see cref = "Type" /> that inherits from <see cref = "PType" /> or null, if no such mapping exists.</returns>
        /// <exception cref = "ArgumentException"><paramref name = "value" /> does not inherit from <see cref = "PType" />.</exception>
        /// <exception cref = "ArgumentNullException"><paramref name = "value" /> is null.</exception>
        public Type? this[string name]
        {
            get
            {
                if (_outer._pTypeRegistry.ContainsKey(name))
                    return _outer._pTypeRegistry[name];
                else
                    return null;
            }
            set
            {
                if (value != null && !PType.IsPType(value))
                    throw new ArgumentException("ClrType " + value + " is not a PType.");
                if (_outer._pTypeRegistry.ContainsKey(name))
                {
                    if (value == null)
                        _outer._pTypeRegistry.Remove(name);
                    else
                        _outer._pTypeRegistry[name] = value;
                }
                else if (value == null)
                    throw new ArgumentNullException(nameof(value));
                else
                    _outer._pTypeRegistry.Add(name, value);
            }
        }

        /// <summary>
        ///     Registers a given CLR <see cref = "Type" /> using the literal name specified in the <see cref = "PTypeLiteralAttribute" />.
        /// </summary>
        /// <param name = "type">A <see cref = "Type" /> that inherits from <see cref = "PType" />.</param>
        /// <exception cref = "ArgumentNullException"><paramref name = "type" /> is null.</exception>
        /// <exception cref = "PrexoniteException"><paramref name = "type" /> does not have a <see cref = "PTypeLiteralAttribute" /> applied to it.</exception>
        /// <remarks>
        ///     Should the supplied type have multiple instances of the <see cref = "PTypeLiteralAttribute" /> applied to it, all of them are registered.
        /// </remarks>
        public void Add(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            var literals =
                (PTypeLiteralAttribute[])
                type.GetCustomAttributes(typeof (PTypeLiteralAttribute), false);
            if (literals.Length == 0)
                throw new PrexoniteException(
                    "Supplied PType " + type +
                    " does not have any PTypeLiteral attributes.");
            foreach (var literal in literals)
                Add(literal.Literal, type);
        }

        /// <summary>
        ///     Registers a CLR <see cref = "Type" /> using a supplied name.
        /// </summary>
        /// <param name = "name">The type name to register for the supplied type.</param>
        /// <param name = "type">A <see cref = "Type" /> that inherits from <see cref = "PType" />.</param>
        /// <remarks>
        ///     If the type has a <see cref = "PTypeLiteralAttribute" /> applied to it,
        ///     you might want to use the overload <see cref = "Add(Type)" /> which automatically 
        ///     registers the type.
        /// </remarks>
        public void Add(string name, Type type)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            else if (!PType.IsPType(type))
                throw new ArgumentException("ClrType " + type + " is not a PType.");
            if (_outer._pTypeRegistry.ContainsKey(name))
                throw new ArgumentException(
                    "The registry already contains an entry " + name + " => " +
                    _outer._pTypeRegistry[name] + ".");
            _outer._pTypeRegistry.Add(name, type);
        }

        /// <summary>
        ///     Removes a <see cref = "PType" /> registration with a give name.
        /// </summary>
        /// <param name = "name">The name of a registration to remove.</param>
        public void Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (_outer._pTypeRegistry.ContainsKey(name))
                _outer._pTypeRegistry.Remove(name);
        }

        /// <summary>
        ///     Returns an IEnumerator to enumerate over all registrations.
        /// </summary>
        /// <returns>An IEnumerator to enumerate over all registrations.</returns>
        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
        {
            return _outer._pTypeRegistry.GetEnumerator();
        }
    }

    #endregion

    /// <summary>
    ///     Turns the supplied <paramref name = "value" /> into a PValue according to the <see cref = "PTypeMap" />.
    /// </summary>
    /// <param name = "value">The object to be encapsulated.</param>
    /// <returns>A PValue that contains the supplied <paramref name = "value" /> together with it's natural <see cref = "PType" /> or <see
    ///      cref = "PType.Null">PValue(null)</see> if <paramref name = "value" /> is null.</returns>
    public PValue CreateNativePValue(object? value)
    {
        if (value == null)
            return PType.Null.CreatePValue();
        else
            return PTypeMap[value.GetType()].CreatePValue(value);
    }

    #region CreatePType

    /// <summary>
    ///     Creates a new <see cref = "PType" /> instance given its CLR <see cref = "Type" />.
    /// </summary>
    /// <param name = "sctx">The stack context in which to create the <see cref = "PType" /></param>
    /// <param name = "ptypeClrType">A CLR <see cref = "Type" /> that inherits from <see cref = "PType" />.</param>
    /// <param name = "args">An array of type arguments.</param>
    /// <returns>The created <see cref = "PType" /> instance.</returns>
    /// <exception cref = "ArgumentException"><paramref name = "ptypeClrType" /> does not inherit from <see cref = "PType" />.</exception>
    public PType CreatePType(StackContext sctx, Type ptypeClrType, PValue[] args)
    {
        return CreatePType(sctx, PType.Object[ptypeClrType], args);
    }

    /// <summary>
    ///     Creates a new <see cref = "PType" /> instance given its CLR <see cref = "Type" />, encapsulated in an <see
    ///      cref = "ObjectPType" />.
    /// </summary>
    /// <param name = "sctx">The stack context in which to create the <see cref = "PType" /></param>
    /// <param name = "ptypeClrType">A CLR <see cref = "Type" /> that inherits from <see cref = "PType" />, encapsulated in a <see
    ///      cref = "ObjectPType" /> object.</param>
    /// <param name = "args">An array of type arguments.</param>
    /// <returns>The created <see cref = "PType" /> instance.</returns>
    /// <exception cref = "ArgumentException">The <see cref = "System.Type" /> provided by the <see
    ///      cref = "ObjectPType.ClrType" /> property of <paramref name = "ptypeClrType" /> does not inherit from <see
    ///      cref = "PType" />.</exception>
    /// <exception cref = "PrexoniteException">If a silent error occured during the creation of the <see cref = "PType" /> instance.</exception>
    public PType CreatePType(StackContext sctx, ObjectPType ptypeClrType, PValue[] args)
    {
        if (!PType.IsPType(ptypeClrType))
            throw new ArgumentException(
                "Cannot construct PType. ClrType " + ptypeClrType.ClrType +
                " is not a PType.");

        //Performance optimizations
        var clrType = ptypeClrType.ClrType;
        if (clrType == typeof (IntPType))
            return PType.Int;
        if (clrType == typeof (RealPType))
            return PType.Real;
        if (clrType == typeof (BoolPType))
            return PType.Bool;
        if (clrType == typeof (StringPType))
            return PType.String;
        if (clrType == typeof (NullPType))
            return PType.Null;
        if (clrType == typeof (ObjectPType) && args.Length > 0 && args[0].Type == PType.String)
            return PType.Object[sctx, (string) args[0].Value];
        if (clrType == typeof (ListPType))
            return PType.List;
        if (clrType == typeof (HashPType))
            return PType.Hash;
        if (clrType == typeof (CharPType))
            return PType.Char;
        if (clrType == typeof (StructurePType))
            return PType.Structure;

        var result =
            ptypeClrType.Construct(sctx, new[] {PType.Object.CreatePValue(args)});
        if (result == null || result.IsNull)
            throw new PrexoniteException(
                "Could not construct PType (resulted in null reference)");
        if (!PType.IsPType(result))
            throw new PrexoniteException(
                "Could not construct PType (" + result.ClrType +
                " is not a PType).");
        return (PType)result.Value;
    }

    /// <summary>
    ///     Creates a new <see cref = "PType" /> instance based on it's type name.
    /// </summary>
    /// <param name = "sctx">The stack context in which to create the <see cref = "PType" />.</param>
    /// <param name = "typeName">The type's name.</param>
    /// <param name = "args">An array of type arguments.</param>
    /// <returns>The created <see cref = "PType" /> instance.</returns>
    /// <exception cref = "SymbolNotFoundException"><paramref name = "typeName" /> cannot be found in the 
    ///     <see cref = "PTypeRegistry" />.</exception>
    public PType CreatePType(StackContext sctx, string typeName, PValue[] args)
    {
        if (!PTypeRegistry.TryGet(typeName, out var type))
            throw new SymbolNotFoundException(
                "PTypeRegistry does not hold a record for \"" + typeName + "\".");
        return CreatePType(sctx, type, args);
    }

    /// <summary>
    ///     Creates a new <see cref = "PType" /> instance based on a constant type expression.
    /// </summary>
    /// <param name = "sctx">The stack context in which to create the <see cref = "PType" />.</param>
    /// <param name = "expression">A constant type expression.</param>
    /// <returns>The created <see cref = "PType" /> instance.</returns>
    /// <remarks>
    ///     <para>
    ///         While it may seem convenient to use this overload, bear in mind this it 
    ///         requires a fully featured parser to translate the supplied string 
    ///         to a <see cref = "PType" /> instance.
    ///     </para>
    ///     <para>
    ///         You are advised to stick with the other overloads unless you get such 
    ///         an expression from a third source (e.g., a configuration file or an <see cref = "Instruction" />).
    ///     </para>
    /// </remarks>
    public PType CreatePType(StackContext sctx, string expression)
    {
        using var lexer = TypeExpressionScanner.CreateFromString(expression);
        var parser = new TypeExpressionParser(lexer, sctx);
        parser.Parse();
        if (parser.errors.count > 0)
            throw new PrexoniteException(
                "Could not construct PType. (Errors in PType expression: " + expression +
                ")");
        else
            return parser.LastType;
    }

    #endregion

    #endregion

    #region Assembly management

    private readonly List<Assembly> _registeredAssemblies;

    /// <summary>
    ///     Determines whether an assembly is already registered for use by the Prexonite VM.
    /// </summary>
    /// <param name = "ass">An assembly reference.</param>
    /// <returns>True if the supplied assembly is registered; false otherwise.</returns>
    [DebuggerStepThrough]
    public bool IsAssemblyRegistered(Assembly? ass)
    {
        if (ass == null)
            return false;
        return _registeredAssemblies.Contains(ass);
    }

    /// <summary>
    ///     Gets a list of all registered assemblies.
    /// </summary>
    /// <returns>A copy of the list of registered assemblies.</returns>
    [DebuggerStepThrough]
    public Assembly[] GetRegisteredAssemblies()
    {
        return _registeredAssemblies.ToArray();
    }

    /// <summary>
    ///     Registers a new assembly for use by the Prexonite VM.
    /// </summary>
    /// <param name = "ass">An assembly reference.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "ass" /> is null.</exception>
    [DebuggerStepThrough]
    public void RegisterAssembly(Assembly? ass)
    {
        if (ass == null)
            throw new ArgumentNullException(nameof(ass));
        if (!_registeredAssemblies.Contains(ass))
            _registeredAssemblies.Add(ass);
    }

    /// <summary>
    ///     Removes an assembly from the list registered ones.
    /// </summary>
    /// <param name = "ass">The assembly to remove.</param>
    /// <exception cref = "ArgumentNullException"><paramref name = "ass" /> is null.</exception>
    [DebuggerStepThrough]
    public void RemoveAssembly(Assembly? ass)
    {
        if (ass == null)
            throw new ArgumentNullException(nameof(ass));
        if (_registeredAssemblies.Contains(ass))
            _registeredAssemblies.Remove(ass);
    }

    #endregion

    #region Command management

    /// <summary>
    ///     A proxy to list commands provided by the engine.
    /// </summary>
    public CommandTable Commands { get; }

    #endregion

    #region File paths

    /// <summary>
    ///     Provides access to the search paths used by this particular engine.
    /// </summary>
    public List<string> Paths { get; } = new();

    #endregion

    #region Class

    private readonly LocalDataStoreSlot _stackSlot;

    /// <summary>
    ///     Creates a new Prexonite virtual machine.
    /// </summary>
    public Engine()
    {
        //Thread local storage for stack
        _stackSlot = Thread.AllocateDataSlot();

        //Metatable
        Meta = MetaTable.Create();

        //PTypes
        _pTypeMap = new Dictionary<Type, PType>();
        PTypeMap = new PTypeMapIterator(this);
        //int
        PTypeMap[typeof (int)] = PType.Int;
        PTypeMap[typeof (long)] = PType.Int;
        PTypeMap[typeof (short)] = PType.Int;
        PTypeMap[typeof (byte)] = PType.Int;

#if UseNonCTSIntegers
            PTypeMap[typeof(uint)]      = IntPType.Instance;
            PTypeMap[typeof(ulong)]     = IntPType.Instance;
            PTypeMap[typeof(ushort)]    = IntPType.Instance;
            PTypeMap[typeof(sbyte)]     = IntPType.Instance;
#endif

        //char
        PTypeMap[typeof (char)] = PType.Char;

        //bool
        PTypeMap[typeof (bool)] = PType.Bool;

        //real
        PTypeMap[typeof (float)] = PType.Real;
        PTypeMap[typeof (double)] = PType.Real;

        //string
        PTypeMap[typeof (string)] = PType.String;

        PTypeMap[typeof (List<PValue>)] = PType.List;
        PTypeMap[typeof (PValue[])] = PType.List;
        PTypeMap[typeof (PValueHashtable)] = PType.Hash;

        //Registry
        _pTypeRegistry = new SymbolTable<Type>();
        PTypeRegistry = new PTypeRegistryIterator(this)
        {
            [IntPType.Literal] = typeof(IntPType),
            [BoolPType.Literal] = typeof(BoolPType),
            [RealPType.Literal] = typeof(RealPType),
            [CharPType.Literal] = typeof(CharPType),
            [StringPType.Literal] = typeof(StringPType),
            [NullPType.Literal] = typeof(NullPType),
            [ObjectPType.Literal] = typeof(ObjectPType),
            [ListPType.Literal] = typeof(ListPType),
            [StructurePType.Literal] = typeof(StructurePType),
            [HashPType.Literal] = typeof(HashPType),
            [StructurePType.Literal] = typeof(StructurePType)
        };

        //Assembly registry
        _registeredAssemblies = new List<Assembly>();
        foreach (
            var assName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            _registeredAssemblies.Add(Assembly.Load(assName.FullName));

        //Commands
        Commands = new CommandTable();
        PCommand cmd;

        Commands.AddEngineCommand(PrintAlias, ConsolePrint.Instance);

        Commands.AddEngineCommand(PrintLineAlias, ConsolePrintLine.Instance);

        Commands.AddEngineCommand(MetaAlias, Prexonite.Commands.Core.Meta.Instance);

        Commands.AddEngineCommand(BoxedAlias, Boxed.Instance);

        // The concatenate command has been renamed to `string_concat` for Prexonite 2
        Commands.AddEngineCommand(ConcatenateAlias, Concat.Instance);
        Commands.AddEngineCommand(OldConcatenateAlias, Concat.Instance);

        Commands.AddEngineCommand(MapAlias, cmd = Map.Instance);
        Commands.AddEngineCommand(SelectAlias, cmd);
        Commands.AddEngineCommand(FlatMap.Alias, FlatMap.Instance);

        Commands.AddEngineCommand(FoldLAlias, FoldL.Instance);

        Commands.AddEngineCommand(FoldRAlias, FoldR.Instance);

        Commands.AddEngineCommand(DisposeAlias, Dispose.Instance);

        // There is a macro that uses the same alias (CallAlias)
        //  it has the same purpose. For backwards compatibility,
        //  the command table will retain the old binding.
        Commands.AddEngineCommand(CallAlias, Call.Instance);
        Commands.AddEngineCommand(Call.Alias, Call.Instance);

        Commands.AddEngineCommand(ThunkAlias, ThunkCommand.Instance);
        Commands.AddEngineCommand(AsThunkAlias, AsThunkCommand.Instance);
        Commands.AddEngineCommand(ForceAlias, ForceCommand.Instance);
        Commands.AddEngineCommand(ToSeqAlias, ToSeqCommand.Instance);

        // There is a macro that uses the same alias (Call_MemberAlias)
        //  it has the same purpose. For backwards compatibility,
        //  the command table will retain the old binding.
        Commands.AddEngineCommand(Call_MemberAlias, Call_Member.Instance);
        Commands.AddEngineCommand(Call_Member.Alias, Call_Member.Instance);

        Commands.AddEngineCommand(CallerAlias, Caller.Instance);

        Commands.AddEngineCommand(PairAlias, Pair.Instance);

        Commands.AddEngineCommand(UnbindAlias, Unbind.Instance);

        Commands.AddEngineCommand(SortAlias, Sort.Instance);
        Commands.AddEngineCommand(SortAlternativeAlias, Sort.Instance);

        Commands.AddEngineCommand(LoadAssemblyAlias, LoadAssembly.Instance);

        Commands.AddEngineCommand(DebugAlias, new Debug());

        Commands.AddEngineCommand(SetCenterAlias, SetCenterCommand.Instance);

        Commands.AddEngineCommand(SetLeftAlias, SetLeftCommand.Instance);

        Commands.AddEngineCommand(SetRightAlias, SetRightCommand.Instance);

        Commands.AddEngineCommand(AllAlias, All.Instance);

        Commands.AddEngineCommand(WhereAlias, Where.Instance);

        Commands.AddEngineCommand(SkipAlias, Skip.Instance);

        Commands.AddEngineCommand(LimitAlias, cmd = Limit.Instance);
        Commands.AddEngineCommand(TakeAlias, cmd);

        Commands.AddEngineCommand(AbsAlias, Abs.Instance);

        Commands.AddEngineCommand(CeilingAlias, Ceiling.Instance);

        Commands.AddEngineCommand(ExpAlias, Exp.Instance);

        Commands.AddEngineCommand(FloorAlias, Floor.Instance);

        Commands.AddEngineCommand(LogAlias, Log.Instance);

        Commands.AddEngineCommand(MaxAlias, Max.Instance);

        Commands.AddEngineCommand(MinAlias, Min.Instance);

        Commands.AddEngineCommand(PiAlias, Pi.Instance);

        Commands.AddEngineCommand(RoundAlias, Round.Instance);

        Commands.AddEngineCommand(SinAlias, Sin.Instance);
        Commands.AddEngineCommand(CosAlias, Cos.Instance);

        Commands.AddEngineCommand(SqrtAlias, Sqrt.Instance);

        Commands.AddEngineCommand(TanAlias, Tan.Instance);

        Commands.AddEngineCommand(CharAlias, Char.Instance);

        Commands.AddEngineCommand(CountAlias, Count.Instance);

        Commands.AddEngineCommand(DistinctAlias, cmd = new Distinct());
        Commands.AddEngineCommand(UnionAlias, cmd);
        Commands.AddEngineCommand(UniqueAlias, cmd);

        Commands.AddEngineCommand(FrequencyAlias, new Frequency());

        Commands.AddEngineCommand(GroupByAlias, new GroupBy());

        Commands.AddEngineCommand(IntersectAlias, new Intersect());

        // There is a macro that uses the same alias (Call_TailAlias)
        //  it has the same purpose. For backwards compatibility,
        //  the command table will retain the old binding.
        Commands.AddEngineCommand(Call_TailAlias, Call_Tail.Instance);
        Commands.AddEngineCommand(Call_Tail.Alias, Call_Tail.Instance);

        Commands.AddEngineCommand(ListAlias, List.Instance);

        Commands.AddEngineCommand(EachAlias, Each.Instance);

        Commands.AddEngineCommand(ExistsAlias, new Exists());

        Commands.AddEngineCommand(ForAllAlias, new ForAll());

        Commands.AddEngineCommand(CompileToCilAlias, CompileToCil.Instance);

        Commands.AddEngineCommand(TakeWhileAlias, TakeWhile.Instance);

        Commands.AddEngineCommand(ExceptAlias, Except.Instance);

        Commands.AddEngineCommand(RangeAlias, Range.Instance);

        Commands.AddEngineCommand(ReverseAlias, Reverse.Instance);

        Commands.AddEngineCommand(HeadTailAlias, HeadTail.Instance);

        Commands.AddEngineCommand(AppendAlias, Append.Instance);

        Commands.AddEngineCommand(SumAlias, Sum.Instance);

        Commands.AddEngineCommand(Contains.Alias, Contains.Instance);

        Commands.AddEngineCommand(ChanAlias, Chan.Instance);

        Commands.AddEngineCommand(SelectAlias, Select.Instance);

        Commands.AddEngineCommand(Call_AsyncAlias, CallAsync.Instance);
        Commands.AddEngineCommand(CallAsync.Alias, CallAsync.Instance);

        Commands.AddEngineCommand(AsyncSeqAlias, AsyncSeq.Instance);

        Commands.AddEngineCommand(CallSubPerformAlias, CallSubPerform.Instance);

        Commands.AddEngineCommand(PartialCallAlias, new PartialCallCommand());

        Commands.AddEngineCommand(PartialMemberCallAlias, PartialMemberCallCommand.Instance);

        Commands.AddEngineCommand(PartialConstructionAlias, PartialConstructionCommand.Instance);

        Commands.AddEngineCommand(PartialTypeCheckAlias, PartialTypeCheckCommand.Instance);
        Commands.AddEngineCommand(PartialTypeCastAlias, PartialTypecastCommand.Instance);
        Commands.AddEngineCommand(PartialStaticCallAlias, PartialStaticCallCommand.Instance);
        Commands.AddEngineCommand(FunctionalPartialCallCommand.Alias,
            FunctionalPartialCallCommand.Instance);
        Commands.AddEngineCommand(FlippedFunctionalPartialCallCommand.Alias,
            FlippedFunctionalPartialCallCommand.Instance);
        Commands.AddEngineCommand(PartialCallStarImplCommand.Alias,
            PartialCallStarImplCommand.Instance);

        Commands.AddEngineCommand(ThenAlias, ThenCommand.Instance);

        Commands.AddEngineCommand(Id.Alias, Id.Instance);
        Commands.AddEngineCommand(Const.Alias, Const.Instance);

        OperatorCommands.AddToEngine(this);

        Commands.AddEngineCommand(CreateEnumerator.Alias, CreateEnumerator.Instance);

        Commands.AddEngineCommand(CreateModuleName.Alias, CreateModuleName.Instance);

        Commands.AddEngineCommand(GetUnscopedAstFactory.Alias, GetUnscopedAstFactory.Instance);

        Commands.AddEngineCommand(CreateSourcePosition.Alias, CreateSourcePosition.Instance);

        Commands.AddEngineCommand(SeqConcat.Alias, SeqConcat.Instance);
    }

    internal Engine(Engine prototype)
    {
        _stackSlot = Thread.AllocateDataSlot();
        Meta = prototype.Meta.Clone();
        _pTypeMap = new Dictionary<Type, PType>(prototype._pTypeMap);
        PTypeMap = new PTypeMapIterator(this);
        _pTypeRegistry = new SymbolTable<Type>(prototype._pTypeRegistry.Count);
        _pTypeRegistry.AddRange(prototype._pTypeRegistry);
        PTypeRegistry = new PTypeRegistryIterator(this);
        _registeredAssemblies = new List<Assembly>(prototype._registeredAssemblies);
        Commands = new CommandTable();
        Commands.AddRange(prototype.Commands);
            
    }

    /// <summary>
    ///     Alias used for the <c>string_concat</c> command.
    /// </summary>
    public const string ConcatenateAlias = "string_concat";

    /// <summary>
    ///     An old alias used for the <c>string_concat</c> command.
    /// </summary>
    public const string OldConcatenateAlias = "concat";

    /// <summary>
    ///     Alias used for the <c>meta</c> command.
    /// </summary>
    public const string MetaAlias = "meta";

    /// <summary>
    ///     Alias used for the <c>boxed</c> command.
    /// </summary>
    public const string BoxedAlias = "boxed";

    /// <summary>
    ///     Alias used for the <c>println</c> command.
    /// </summary>
    public const string PrintLineAlias = "println";

    /// <summary>
    ///     Alias used for the <c>print</c> command.
    /// </summary>
    public const string PrintAlias = "print";

    /// <summary>
    ///     Alias used for the <c>map</c> command.
    /// </summary>
    public const string MapAlias = "map";

    /// <summary>
    ///     Alias used for the <c>foldl</c> command.
    /// </summary>
    public const string FoldLAlias = "foldl";

    /// <summary>
    ///     Alias used for the <c>foldr</c> command.
    /// </summary>
    public const string FoldRAlias = "foldr";

    /// <summary>
    ///     Alias used for the <c>dispose</c> command.
    /// </summary>
    public const string DisposeAlias = "dispose";

    /// <summary>
    ///     Alias used for the <c>call</c> command (macro). 
    ///     If you intend to refer to the <see cref = "Call" /> command, 
    ///     use <see cref = "Call.Alias" />.
    /// </summary>
    public const string CallAlias = "call";

    /// <summary>
    ///     Alias used for the <c>call\async</c> command.
    /// </summary>
    public const string Call_AsyncAlias = @"call\async";

    /// <summary>
    ///     Alias used for the "async_seq" command.
    /// </summary>
    public const string AsyncSeqAlias = @"async_seq";

    /// <summary>
    ///     Alias used for the "chan" command.
    /// </summary>
    public const string ChanAlias = "chan";

    /// <summary>
    ///     Alias used for the "select" command.
    /// </summary>
    public const string SelectAlias = "select";

    /// <summary>
    ///     Alias used for the <c>callmember</c> command (macro). 
    ///     If you intend to refer to the <see cref = "Call_Member" /> command, 
    ///     use <see cref = "Call_Member.Alias" />.
    /// </summary>
    public const string Call_MemberAlias = @"call\member";

    public const string ThunkAlias = "thunk";
    public const string AsThunkAlias = "asthunk";
    public const string ForceAlias = "force";
    public const string ToSeqAlias = "toseq";

    /// <summary>
    ///     Alias used for the <c>caller</c> command.
    /// </summary>
    public const string CallerAlias = "caller";

    /// <summary>
    ///     Alias used for the "pair" command.
    /// </summary>
    public const string PairAlias = "pair";

    /// <summary>
    ///     Alias used for the "unbind" command.
    /// </summary>
    public const string UnbindAlias = "unbind";

    /// <summary>
    ///     Alias used for the "sort" command.
    /// </summary>
    public const string SortAlias = "sort";

    public const string SortAlternativeAlias = "orderby";

    /// <summary>
    ///     Alias used for the "loadAssembly" command.
    /// </summary>
    public const string LoadAssemblyAlias = "LoadAssembly";

    /// <summary>
    ///     Alias used for the debug command.
    /// </summary>
    public const string DebugAlias = "debug";

    /// <summary>
    ///     Alias used for the setcenter command
    /// </summary>
    public const string SetCenterAlias = "setcenter";

    /// <summary>
    ///     Alias used for the setleft command
    /// </summary>
    public const string SetLeftAlias = "setleft";

    /// <summary>
    ///     Alias used for the setright command
    /// </summary>
    public const string SetRightAlias = "setright";

    /// <summary>
    ///     Alias used for the all command
    /// </summary>
    public const string AllAlias = "all";

    /// <summary>
    ///     Alias used for the where command.
    /// </summary>
    public const string WhereAlias = "where";

    /// <summary>
    ///     Alias used for the skip command.
    /// </summary>
    public const string SkipAlias = "skip";

    /// <summary>
    ///     Alias used for the limit command.
    /// </summary>
    public const string LimitAlias = "limit";

    /// <summary>
    ///     Alias used for the limit command.
    /// </summary>
    public const string TakeAlias = "take";

    /// <summary>
    ///     Alias used for the abs command.
    /// </summary>
    public const string AbsAlias = "abs";

    /// <summary>
    ///     Alias used for the ceiling command.
    /// </summary>
    public const string CeilingAlias = "ceiling";

    /// <summary>
    ///     Alias used for the cos command.
    /// </summary>
    public const string CosAlias = "cos";

    /// <summary>
    ///     Alias used for the exp command.
    /// </summary>
    public const string ExpAlias = "exp";

    /// <summary>
    ///     Alias used for the floor command.
    /// </summary>
    public const string FloorAlias = "floor";

    /// <summary>
    ///     Alias used for the log command.
    /// </summary>
    public const string LogAlias = "log";

    /// <summary>
    ///     Alias used for the max command.
    /// </summary>
    public const string MaxAlias = "max";

    /// <summary>
    ///     Alias used for the min command.
    /// </summary>
    public const string MinAlias = "min";

    /// <summary>
    ///     Alias used for the pi command.
    /// </summary>
    public const string PiAlias = "pi";

    /// <summary>
    ///     Alias used for the round command.
    /// </summary>
    public const string RoundAlias = "round";

    /// <summary>
    ///     Alias used for the sin command.
    /// </summary>
    public const string SinAlias = "sin";

    /// <summary>
    ///     Alias used for the sqrt command.
    /// </summary>
    public const string SqrtAlias = "sqrt";

    /// <summary>
    ///     Alias used for the tan command.
    /// </summary>
    public const string TanAlias = "tan";

    /// <summary>
    ///     Alias used for the char command.
    /// </summary>
    public const string CharAlias = "char";

    /// <summary>
    ///     Alias used for the groupBy command.
    /// </summary>
    public const string GroupByAlias = "groupby";

    /// <summary>
    ///     Alias used for the frequency command.
    /// </summary>
    public const string FrequencyAlias = "frequency";

    /// <summary>
    ///     Alias used for the count command.
    /// </summary>
    public const string CountAlias = "count";

    /// <summary>
    ///     Alias used for the distinct command.
    /// </summary>
    public const string DistinctAlias = "distinct";

    /// <summary>
    ///     Alias used for the intersect command.
    /// </summary>
    public const string IntersectAlias = "intersect";

    /// <summary>
    ///     Alias used for the distinct command.
    /// </summary>
    public const string UnionAlias = "union";

    /// <summary>
    ///     Alias used for the distinct command.
    /// </summary>
    public const string UniqueAlias = "unique";

    /// <summary>
    ///     Alias used for the call\tail command.
    ///     If you intend to refer to the <see cref = "Call_Tail" /> command, 
    ///     use <see cref = "Call_Tail.Alias" />.
    /// </summary>
    public const string Call_TailAlias = @"call\tail";

    /// <summary>
    ///     Alias used for the list command.
    /// </summary>
    public const string ListAlias = @"list";

    /// <summary>
    ///     Alias used for the each command.
    /// </summary>
    public const string EachAlias = "each";

    /// <summary>
    ///     Alias used for the exists command.
    /// </summary>
    public const string ExistsAlias = "exists";

    /// <summary>
    ///     Alias used for the forAll command.
    /// </summary>
    public const string ForAllAlias = "forall";

    /// <summary>
    ///     Alias used for the CompileToCil command.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")] public const string CompileToCilAlias = "CompileToCil";

    /// <summary>
    ///     Alias used for the TakeWhile command.
    /// </summary>
    public const string TakeWhileAlias = "takewhile";

    public const string ExceptAlias = "except";

    public const string RangeAlias = "range";

    public const string ReverseAlias = "reverse";

    public const string HeadTailAlias = "headtail";

    /// <summary>
    ///     Alias used for the Append command.
    /// </summary>
    public const string AppendAlias = "append";

    /// <summary>
    ///     Alias used for the Sum command.
    /// </summary>
    public const string SumAlias = "sum";

    /// <summary>
    ///     Alias used for the call\sub\perform command.
    /// </summary>
    public const string CallSubPerformAlias = @"call\sub\perform";

    /// <summary>
    ///     Alias used for the pa\ind command (constructor for partial applications of indirect calls)
    /// </summary>
    public const string PartialCallAlias = @"pa\ind";

    /// <summary>
    ///     Alias used for the pa\mem command (constructor for partial application of instance member calls)
    /// </summary>
    public const string PartialMemberCallAlias = @"pa\mem";

    /// <summary>
    ///     Alias used for the pa\ctor command (constructor for partial application of object constructions)
    /// </summary>
    public const string PartialConstructionAlias = @"pa\ctor";

    /// <summary>
    ///     Alias used for the pa\check command (constructor for partial application of type checks)
    /// </summary>
    public const string PartialTypeCheckAlias = @"pa\check";

    /// <summary>
    ///     Alias used for the pa\cast command (constructor for partial application of type casts)
    /// </summary>
    public const string PartialTypeCastAlias = @"pa\cast";

    /// <summary>
    ///     Alias used for the pa\smem command (constructor for partial application of static calls)
    /// </summary>
    public const string PartialStaticCallAlias = @"pa\smem";

    /// <summary>
    ///     Alias used for the then command (constructor for function composition)
    /// </summary>
    public const string ThenAlias = @"then";

    #endregion

    #region Version

    public static Version PrexoniteVersion => typeof (Engine).Assembly.GetName().Version 
        ?? new Version(0,0);

    #endregion
}