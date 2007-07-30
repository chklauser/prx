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

//#define UseNonCLSIntegers
//Behaviour is not defined for these types
using System;
using System.Collections.Generic;
using System.Reflection;
using Prexonite.Commands;
using Prexonite.Types;
using TScanner = Prexonite.Internal.Scanner;
using TParser = Prexonite.Internal.Parser;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// Prexonite virtual machines. Engines manage available <see cref="PType">PTypes</see>, assemblies accessible by the virtual machine as well as available <see cref="Commands" />.
    /// </summary>
    public partial class Engine
    {
        #region Static

        #region String Comparision standards

        /// <summary>
        /// The string comparer used throughout the Prexonite VM.
        /// </summary>
        /// <remarks>The current implementation is <strong>case-insensitive</strong></remarks>
        public static readonly StringComparer DefaultStringComparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// This method is used throughout the whole Prexonite VM to compare strings.
        /// </summary>
        /// <param name="x">The left operand.</param>
        /// <param name="y">The right operand.</param>
        /// <remarks>The current implementation is <strong>case-insensitive</strong>.</remarks>
        /// <returns>True if the two strings <paramref name="x"/> and <paramref name="y"/> are equal; false otherwise.</returns>
        [NoDebug()]
        public static bool StringsAreEqual(string x, string y)
        {
            return String.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        /// <summary>
        /// Generates a random identifier using <see cref="Guid"/>.
        /// </summary>
        /// <returns>A unique identifier.</returns>
        public static string GenerateName()
        {
            return "gen" + Guid.NewGuid().ToString("N");
        }

        #endregion

        #region Meta

        private MetaTable meta;

        /// <summary>
        /// The metatable provided to store settings and information about the virtual machine. This table also provides default values, should both applications and functions lack specific entries.
        /// </summary>
        public MetaTable Meta
        {
            get { return meta; }
        }

        #endregion

        #region PType management

        #region PType map

        private Dictionary<Type, PType> _pTypeMap;
        private PTypeMapIterator _ptypemapiterator;

        /// <summary>
        /// Provides access to the PType to CLR <see cref="System.Type">Type</see> mapping.
        /// </summary>
        /// <remarks>The property uses a proxy type to hide implementation details.</remarks>
        /// <seealso cref="PTypeMapIterator"/>
        public PTypeMapIterator PTypeMap
        {
            [NoDebug]
            get { return _ptypemapiterator; }
        }

        /// <summary>
        /// Proxy type that is used to provide access to the <see cref="PType"/> to CLR <see cref="System.Type">Type</see> mapping.
        /// </summary>
        /// <seealso cref="Engine.PTypeMap"/>
        [NoDebug]
        public class PTypeMapIterator
        {
            private readonly Engine outer;

            internal PTypeMapIterator(Engine outer)
            {
                this.outer = outer;
            }

            /// <summary>
            /// Provides readonly access to the number of mappings in the current engine.
            /// </summary>
            /// <value>The number of mappings in the current engine.</value>
            public int Count
            {
                get { return outer._pTypeMap.Count; }
            }

            /// <summary>
            /// Provides index based access to the mappings using their CLR <see cref="Type">Type</see> as the index.
            /// </summary>
            /// <param name="clrType">CLR <see cref="Type">Type</see> of a mapping.</param>
            /// <returns>The <see cref="PType"/> of a mapping or <c><see cref="PType.Object"/>[<paramref name="clrType"/>]</c> if no such mapping exists.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
            public PType this[Type clrType]
            {
                get
                {
                    if (outer._pTypeMap.ContainsKey(clrType))
                        return outer._pTypeMap[clrType];
                    else
                        return PType.Object[clrType];
                }
                set
                {
                    if (outer._pTypeMap.ContainsKey(clrType))
                    {
                        if (value == null)
                            outer._pTypeMap.Remove(clrType);
                        else
                            outer._pTypeMap[clrType] = value;
                    }
                    else if (value == null)
                        throw new ArgumentNullException("value");
                    else
                        outer._pTypeMap.Add(clrType, value);
                }
            }

            /// <summary>
            /// Adds a new mapping from a CLR <see cref="Type"/> to a <see cref="PType"/>.
            /// </summary>
            /// <param name="clrType">The CLR <see cref="Type"/>.</param>
            /// <param name="type">The <see cref="PType"/></param>
            /// <exception cref="ArgumentNullException">Either <paramref name="clrType"/> or <paramref name="type"/> is null.</exception>
            public void Add(Type clrType, PType type)
            {
                if (clrType == null)
                    throw new ArgumentNullException("clrType");
                if (type == null)
                    throw new ArgumentNullException("type");
                if (outer._pTypeMap.ContainsKey(clrType))
                    throw new InvalidOperationException("A mapping for the CLR Type " + clrType.FullName +
                                                        " already exists");
                outer._pTypeMap.Add(clrType, type);
            }

            /// <summary>
            /// Removed a mapping identified by it's CLR <see cref="Type"/>.
            /// </summary>
            /// <param name="clrType">The CLR <see cref="Type"/> of the mapping to remove.</param>
            public void Remove(Type clrType)
            {
                if (clrType == null)
                    throw new ArgumentNullException("clrType");
                if (outer._pTypeMap.ContainsKey(clrType))
                    outer._pTypeMap.Remove(clrType);
            }

            /// <summary>
            /// Returns an IEnumerator to enumerate over all mappings in the current engine.
            /// </summary>
            /// <returns>An IEnumerator to enumerate over all mappings in the current engine.</returns>
            public IEnumerator<KeyValuePair<Type, PType>> GetEnumerator()
            {
                return outer._pTypeMap.GetEnumerator();
            }
        }

        #endregion

        #region PType registry

        private SymbolTable<Type> _pTypeRegistry;
        private PTypeRegistryIterator _pTypeRegistryIterator;

        /// <summary>
        /// Provides access to the dictionary of <see cref="PType">PTypes</see> registered 
        /// for recognition in type expressions.
        /// </summary>
        /// <seealso cref="PTypeRegistryIterator"/>
        public PTypeRegistryIterator PTypeRegistry
        {
            [NoDebug]
            get { return _pTypeRegistryIterator; }
        }

        /// <summary>
        /// The proxy class that is used to provide access to the <see cref="Engine.PTypeRegistry"/>
        /// </summary>
        /// <seealso cref="Engine.PTypeRegistry"/>
        [NoDebug]
        public class PTypeRegistryIterator
        {
            private readonly Engine outer;

            internal PTypeRegistryIterator(Engine outer)
            {
                this.outer = outer;
            }

            /// <summary>
            /// The number of registered <see cref="PType">PTypes</see>.
            /// </summary>
            /// <value>The number of registered <see cref="PType">PTypes</see>.</value>
            public int Count
            {
                get { return outer._pTypeRegistry.Count; }
            }

            /// <summary>
            /// Determines whether a particular type name has been registered.
            /// </summary>
            /// <param name="name">A name to check for registration.</param>
            /// <returns>True if the name has been registered; false otherwise.</returns>
            public bool Contains(string name)
            {
                return outer._pTypeRegistry.ContainsKey(name);
            }

            /// <summary>
            /// Provides index based access to the <see cref="Engine.PTypeRegistry"/>.
            /// </summary>
            /// <param name="name">The name of the <see cref="PType"/> to lookup.</param>
            /// <returns>A <see cref="Type"/> that inherits from <see cref="PType"/> or null, if no such mapping exists.</returns>
            /// <exception cref="ArgumentException"><paramref name="value"/> does not inherit from <see cref="PType"/>.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
            public Type this[string name]
            {
                get
                {
                    if (outer._pTypeRegistry.ContainsKey(name))
                        return outer._pTypeRegistry[name];
                    else
                        return null;
                }
                set
                {
                    if (value != null && !PType.IsPType(value))
                        throw new ArgumentException("ClrType " + value + " is not a PType.");
                    if (outer._pTypeRegistry.ContainsKey(name))
                    {
                        if (value == null)
                            outer._pTypeRegistry.Remove(name);
                        else
                            outer._pTypeRegistry[name] = value;
                    }
                    else if (value == null)
                        throw new ArgumentNullException("value");
                    else
                        outer._pTypeRegistry.Add(name, value);
                }
            }

            /// <summary>
            /// Registers a given CLR <see cref="Type"/> using the literal name specified in the <see cref="PTypeLiteralAttribute"/>.
            /// </summary>
            /// <param name="type">A <see cref="Type"/> that inherits from <see cref="PType"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
            /// <exception cref="PrexoniteException"><paramref name="type"/> does not have a <see cref="PTypeLiteralAttribute"/> applied to it.</exception>
            /// <remarks>Should the supplied type have multiple instances of the <see cref="PTypeLiteralAttribute"/> applied to it, all of them are registered.</remarks>
            public void Add(Type type)
            {
                if (type == null)
                    throw new ArgumentNullException("type");
                PTypeLiteralAttribute[] literals =
                    (PTypeLiteralAttribute[]) type.GetCustomAttributes(typeof(PTypeLiteralAttribute), false);
                if (literals.Length == 0)
                    throw new PrexoniteException("Supplied PType " + type +
                                                 " does not have any PTypeLiteral attributes.");
                foreach (PTypeLiteralAttribute literal in literals)
                    Add(literal.Literal, type);
            }

            /// <summary>
            /// Registers a CLR <see cref="Type"/> using a supplied name.
            /// </summary>
            /// <param name="name">The type name to register for the supplied type.</param>
            /// <param name="type">A <see cref="Type"/> that inherits from <see cref="PType"/>.</param>
            /// <remarks>If the type has a <see cref="PTypeLiteralAttribute" /> applied to it,
            ///  you might want to use the overload <see cref="Add(Type)"/> which automatically 
            /// registers the type.</remarks>
            public void Add(string name, Type type)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                if (type == null)
                    throw new ArgumentNullException("type");
                else if (!PType.IsPType(type))
                    throw new ArgumentException("ClrType " + type + " is not a PType.");
                if (outer._pTypeRegistry.ContainsKey(name))
                    throw new ArgumentException("The registry already contains an entry " + name + " => " +
                                                outer._pTypeRegistry[name] + ".");
                outer._pTypeRegistry.Add(name, type);
            }

            /// <summary>
            /// Removes a <see cref="PType"/> registration with a give name.
            /// </summary>
            /// <param name="name">The name of a registration to remove.</param>
            public void Remove(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");
                if (outer._pTypeRegistry.ContainsKey(name))
                    outer._pTypeRegistry.Remove(name);
            }

            /// <summary>
            /// Returns an IEnumerator to enumerate over all registrations.
            /// </summary>
            /// <returns>An IEnumerator to enumerate over all registrations.</returns>
            public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
            {
                return outer._pTypeRegistry.GetEnumerator();
            }
        }

        #endregion

        /// <summary>
        /// Turns the supplied <paramref name="value"/> into a PValue according to the <see cref="PTypeMap"/>.
        /// </summary>
        /// <param name="value">The object to be encapsulated.</param>
        /// <returns>A PValue that contains the supplied <paramref name="value"/> together with it's natural <see cref="PType"/> or <see cref="PType.Null">PValue(null)</see> if <paramref name="value"/> is null.</returns>
        public PValue CreateNativePValue(object value)
        {
            if (value == null)
                return PType.Null.CreatePValue();
            else
                return _ptypemapiterator[value.GetType()].CreatePValue(value);
        }

        #region CreatePType

        /// <summary>
        /// Creates a new <see cref="PType" /> instance given its CLR <see cref="Type"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to create the <see cref="PType"/></param>
        /// <param name="ptypeClrType">A CLR <see cref="Type"/> that inherits from <see cref="PType"/>.</param>
        /// <param name="args">An array of type arguments.</param>
        /// <returns>The created <see cref="PType"/> instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="ptypeClrType"/> does not inherit from <see cref="PType"/>.</exception>
        public PType CreatePType(StackContext sctx, Type ptypeClrType, PValue[] args)
        {
            return CreatePType(sctx, PType.Object[ptypeClrType], args);
        }

        /// <summary>
        /// Creates a new <see cref="PType" /> instance given its CLR <see cref="Type"/>, encapsulated in an <see cref="ObjectPType"/>.
        /// </summary>
        /// <param name="sctx">The stack context in which to create the <see cref="PType"/></param>
        /// <param name="ptypeClrType">A CLR <see cref="Type"/> that inherits from <see cref="PType"/>, encapsulated in a <see cref="ObjectPType"/> object.</param>
        /// <param name="args">An array of type arguments.</param>
        /// <returns>The created <see cref="PType"/> instance.</returns>
        /// <exception cref="ArgumentException">The <see cref="System.Type"/> provided by the <see cref="ObjectPType.ClrType"/> property of <paramref name="ptypeClrType"/> does not inherit from <see cref="PType"/>.</exception>
        /// <exception cref="PrexoniteException">If a silent error occured during the creation of the <see cref="PType"/> instance.</exception>
        public PType CreatePType(StackContext sctx, ObjectPType ptypeClrType, PValue[] args)
        {
            if (!PType.IsPType(ptypeClrType))
                throw new ArgumentException("Cannot construct PType. ClrType " + ptypeClrType.ClrType +
                                            " is not a PType.");

            //Performance optimizations
            Type clrType = ptypeClrType.ClrType;
            if (clrType == typeof(IntPType))
                return PType.Int;
            if (clrType == typeof(RealPType))
                return PType.Real;
            if (clrType == typeof(BoolPType))
                return PType.Bool;
            if (clrType == typeof(StringPType))
                return PType.String;
            if (clrType == typeof(NullPType))
                return PType.Null;
            if (clrType == typeof(ObjectPType) && args.Length > 0 && args[0].Type == PType.String)
                return PType.Object[sctx, (string) args[0].Value];
            if (clrType == typeof(ListPType))
                return PType.List;
            if(clrType == typeof(HashPType))
                return PType.Hash;

            PValue result = ptypeClrType.Construct(sctx, new PValue[] { PType.Object.CreatePValue(args) } );
            if (result == null || result.IsNull)
                throw new PrexoniteException("Could not construct PType (resulted in null reference)");
            if (!PType.IsPType(result))
                throw new PrexoniteException("Could not construct PType (" + result.ClrType +
                                             " is not a PType).");
            return result.Value as PType;
        }

        /// <summary>
        /// Creates a new <see cref="PType"/> instance based on it's type name.
        /// </summary>
        /// <param name="sctx">The stack context in which to create the <see cref="PType"/>.</param>
        /// <param name="typeName">The type's name.</param>
        /// <param name="args">An array of type arguments.</param>
        /// <returns>The created <see cref="PType"/> instance.</returns>
        /// <exception cref="SymbolNotFoundException"><paramref name="typeName"/> cannot be found in the 
        /// <see cref="PTypeRegistry"/>.</exception>
        public PType CreatePType(StackContext sctx, string typeName, PValue[] args)
        {
            if (!PTypeRegistry.Contains(typeName))
                throw new SymbolNotFoundException("PTypeRegistry does not hold a record for \"" + typeName + "\".");
            return CreatePType(sctx, PTypeRegistry[typeName], args);
        }

        /// <summary>
        /// Creates a new <see cref="PType"/> instance based on a constant type expression.
        /// </summary>
        /// <param name="sctx">The stack context in which to create the <see cref="PType"/>.</param>
        /// <param name="expression">A constant type expression.</param>
        /// <returns>The created <see cref="PType"/> instance.</returns>
        /// <remarks>
        /// <para>
        ///     While it may seem convenient to use this overload, bear in mind this it 
        ///     requires a fully featured parser to translate the supplied string 
        ///     to a <see cref="PType"/> instance.
        /// </para>
        /// <para>
        ///     You are advised to stick with the other overloads unless you get such an expression from a third source (e.g., a configuration file or an <see cref="Instruction"/>).
        /// </para>
        /// </remarks>
        public PType CreatePType(StackContext sctx, string expression)
        {
            using (TScanner lexer = TScanner.CreateFromString(expression))
            {
                TParser parser = new TParser(lexer, sctx);
                parser.Parse();
                if (parser.errors.count > 0)
                    throw new PrexoniteException("Could not construct PType. (Errors in PType expression: "+expression+")");
                else
                    return parser.LastType;
            }
        }

        #endregion

        #endregion

        #region Assembly management

        private List<Assembly> _registeredAssemblies;

        /// <summary>
        /// Determines whether an assembly is already registered for use by the Prexonite VM.
        /// </summary>
        /// <param name="ass">An assembly reference.</param>
        /// <returns>True if the supplied assembly is registered; false otherwise.</returns>
        [NoDebug]
        public bool IsAssemblyRegistered(Assembly ass)
        {
            if (ass == null)
                return false;
            return _registeredAssemblies.Contains(ass);
        }

        /// <summary>
        /// Gets a list of all registered assemblies.
        /// </summary>
        /// <returns>A copy of the list of registered assemblies.</returns>
        [NoDebug]
        public Assembly[] GetRegisteredAssemblies()
        {
            return _registeredAssemblies.ToArray();
        }

        /// <summary>
        /// Registers a new assembly for use by the Prexonite VM.
        /// </summary>
        /// <param name="ass">An assembly reference.</param>
        /// <exception cref="ArgumentNullException"><paramref name="ass"/> is null.</exception>
        [NoDebug]
        public void RegisterAssembly(Assembly ass)
        {
            if (ass == null)
                throw new ArgumentNullException("ass");
            if (!_registeredAssemblies.Contains(ass))
                _registeredAssemblies.Add(ass);
        }

        /// <summary>
        /// Removes an assembly from the list registered ones.
        /// </summary>
        /// <param name="ass">The assembly to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="ass"/> is null.</exception>
        [NoDebug]
        public void RemoveAssembly(Assembly ass)
        {
            if (ass == null)
                throw new ArgumentNullException("ass");
            if (_registeredAssemblies.Contains(ass))
                _registeredAssemblies.Remove(ass);
        }

        #endregion

        #region Command management

        private SymbolTable<PCommand> _commands;
        private CommandsIterator _commandsIterator;

        /// <summary>
        /// A proxy to list commands provided by the engine.
        /// </summary>
        public CommandsIterator Commands
        {
            [NoDebug]
            get { return _commandsIterator; }
        }

        /// <summary>
        /// A proxy class to the command provided by the current engine.
        /// </summary>
        [NoDebug]
        public class CommandsIterator
        {
            private readonly Engine outer;

            internal CommandsIterator(Engine outer)
            {
                this.outer = outer;
            }

            /// <summary>
            /// The number of commands available.
            /// </summary>
            public int Count
            {
                get { return outer._commands.Count; }
            }

            /// <summary>
            /// Determines whether a particular name is registered for a command.
            /// </summary>
            /// <param name="name">A name.</param>
            /// <returns>True if a command with the supplied name exists; false otherwise.</returns>
            public bool Contains(string name)
            {
                return outer._commands.ContainsKey(name);
            }

            /// <summary>
            /// Tries to retrieve a command by it's name.
            /// </summary>
            /// <param name="id">The command's id</param>
            /// <param name="cmd">An out parameter in which the result of the query is stored.</param>
            /// <returns>True if the attempt was successful; false otherwise.</returns>
            /// <remarks>The value of <paramref name="cmd"/> is only defined when the method returns true.</remarks>
            public bool TryGetValue(string id, out PCommand cmd)
            {
                return outer._commands.TryGetValue(id, out cmd);
            }

            /// <summary>
            /// Index based access to the dictionary of available commands.
            /// </summary>
            /// <param name="name">The name of the command to retrieve.</param>
            /// <returns>The command registered with the supplied name.</returns>
            public PCommand this[string name]
            {
                get
                {
                    if (outer._commands.ContainsKey(name))
                        return outer._commands[name];
                    else
                        return null;
                }
                set
                {
                    if (value != null && (!value.BelongsToAGroup))
                        value.AddToGroup(PCommandGroups.User);

                    if (outer._commands.ContainsKey(name))
                    {
                        if (value == null)
                            outer._commands.Remove(name);
                        else
                            outer._commands[name] = value;
                    }
                    else if (value == null)
                        throw new ArgumentNullException("value");
                    else
                        outer._commands.Add(name, value);
                }
            }

            /// <summary>
            /// Adds a new command in the user space.
            /// </summary>
            /// <param name="alias">The alias that shall refer to the supplied command.</param>
            /// <param name="command">A command instance.</param>
            /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
            /// or <paramref name="command"/> is null.</exception>
            public void AddUserCommand(string alias, PCommand command)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (command == null)
                    throw new ArgumentNullException("command");
                command.AddToGroup(PCommandGroups.User);
                this[alias] = command;
            }

            /// <summary>
            /// Adds a new command in the user space.
            /// </summary>
            /// <param name="alias">The alias that shall refer to the supplied command.</param>
            /// <param name="action">An action to be turned into a command.</param>
            /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
            /// or <paramref name="action"/> is null.</exception>
            public void AddUserCommand(string alias, PCommandAction action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddUserCommand(alias, new DelegatePCommand(action));
            }

            /// <summary>
            /// Adds a new command in the user space.
            /// </summary>
            /// <param name="alias">The alias that shall refer to the supplied command.</param>
            /// <param name="action">An action to be turned into a command.</param>
            /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
            /// or <paramref name="action"/> is null.</exception>
            public void AddUserCommand(string alias, ICommand action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddUserCommand(alias, new NestedPCommand(action));
            }

            internal void AddEngineCommand(string alias, PCommand command)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (command == null)
                    throw new ArgumentNullException("command");
                command.AddToGroup(PCommandGroups.Engine);
                this[alias] = command;
            }

            internal void AddEngineCommand(string alias, PCommandAction action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action"); 
                AddEngineCommand(alias, new DelegatePCommand(action));
            }

            internal void AddEngineCommand(string alias, ICommand action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddEngineCommand(alias, new NestedPCommand(action));
            }

            internal void AddCompilerCommand(string alias, PCommand command)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (command == null)
                    throw new ArgumentNullException("command");
                command.AddToGroup(PCommandGroups.Compiler);
                this[alias] = command;
            }

            internal void AddCompilerCommand(string alias, PCommandAction action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddCompilerCommand(alias, new DelegatePCommand(action));
            }

            internal void AddCompilerCommand(string alias, ICommand action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddCompilerCommand(alias, new NestedPCommand(action));
            }

            /// <summary>
            /// Adds a new command in the host space.
            /// </summary>
            /// <param name="alias">The alias that shall refer to the supplied command.</param>
            /// <param name="command">A command instance.</param>
            /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
            /// or <paramref name="command"/> is null.</exception>
            public void AddHostCommand(string alias, PCommand command)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (command == null)
                    throw new ArgumentNullException("command");
                command.AddToGroup(PCommandGroups.Host);
                this[alias] = command;
            }

            /// <summary>
            /// Adds a new command in the host space.
            /// </summary>
            /// <param name="alias">The alias that shall refer to the supplied command.</param>
            /// <param name="action">An action to be turned into a command.</param>
            /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
            /// or <paramref name="action"/> is null.</exception>
            public void AddHostCommand(string alias, PCommandAction action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddHostCommand(alias, new DelegatePCommand(action));
            }

            /// <summary>
            /// Adds a new command in the host space.
            /// </summary>
            /// <param name="alias">The alias that shall refer to the supplied command.</param>
            /// <param name="action">An action to be turned into a command.</param>
            /// <exception cref="ArgumentNullException">If either <paramref name="alias"/> 
            /// or <paramref name="action"/> is null.</exception>
            public void AddHostCommand(string alias, ICommand action)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                if (action == null)
                    throw new ArgumentNullException("action");
                AddHostCommand(alias, new NestedPCommand(action));
            }

            /// <summary>
            /// Removes all command previously added to the user space.
            /// </summary>
            public void RemoveUserCommands()
            {
                _remove_commands(PCommandGroups.User);
            }

            /// <summary>
            /// Removes all commands previously added to the host space.
            /// </summary>
            public void RemoveHostCommands()
            {
                _remove_commands(PCommandGroups.Host);
            }

            internal void RemoveCompilerCommands()
            {
                _remove_commands(PCommandGroups.Compiler);
            }

            internal void RemoveEngineCommands()
            {
                _remove_commands(PCommandGroups.Engine);
            }

            private void _remove_commands(PCommandGroups groups)
            {
                KeyValuePair<string, PCommand>[] commands = new KeyValuePair<string, PCommand>[outer._commands.Count];
                outer._commands.CopyTo(commands, 0);
                foreach (KeyValuePair<string, PCommand> kvp in commands)
                {
                    PCommand cmd = kvp.Value;
                    cmd.RemoveFromGroup(groups);
                    if (!cmd.BelongsToAGroup)
                        outer._commands.Remove(kvp.Key);
                }
            }

            /// <summary>
            /// Returns an IEnumerator
            /// </summary>
            /// <returns></returns>
            public IEnumerator<KeyValuePair<string, PCommand>> GetEnumerator()
            {
                return outer._commands.GetEnumerator();
            }
        }

        #endregion

        #region File paths

        /// <summary>
        /// Provides access to the search paths used by this particular engine.
        /// </summary>
        public List<String> Paths
        {
            get { return _paths; }
        }

        private List<String> _paths = new List<string>();

        #endregion

        #region Class

        /// <summary>
        /// Creates a new Prexonite virtual machine.
        /// </summary>
        public Engine()
        {
            //Metatable
            meta = new MetaTable();

            //PTypes
            _pTypeMap = new Dictionary<Type, PType>();
            _ptypemapiterator = new PTypeMapIterator(this);
            //int
            PTypeMap[typeof(int)] = PType.Int;
            PTypeMap[typeof(long)] = PType.Int;
            PTypeMap[typeof(short)] = PType.Int;
            PTypeMap[typeof(byte)] = PType.Int;

#if UseNonCTSIntegers
            PTypeMap[typeof(uint)]      = IntPType.Instance;
            PTypeMap[typeof(ulong)]     = IntPType.Instance;
            PTypeMap[typeof(ushort)]    = IntPType.Instance;
            PTypeMap[typeof(sbyte)]     = IntPType.Instance;
#endif

            //bool
            PTypeMap[typeof(bool)] = PType.Bool;

            //real
            PTypeMap[typeof(float)] = PType.Real;
            PTypeMap[typeof(double)] = PType.Real;

            //string
            PTypeMap[typeof(string)] = PType.String;

            PTypeMap[typeof(List<PValue>)] = PType.List;
            PTypeMap[typeof(PValue[])] = PType.List;
            PTypeMap[typeof(PValueHashtable)] = PType.Hash;

            //Registry
            _pTypeRegistry = new SymbolTable<Type>();
            _pTypeRegistryIterator = new PTypeRegistryIterator(this);
            PTypeRegistry[IntPType.Literal] = PType.Int.GetType();
            PTypeRegistry[BoolPType.Literal] = PType.Bool.GetType();
            PTypeRegistry[RealPType.Literal] = PType.Real.GetType();
            PTypeRegistry[StringPType.Literal] = PType.String.GetType();
            PTypeRegistry[NullPType.Literal] = typeof(NullPType);
            PTypeRegistry[ObjectPType.Literal] = typeof(ObjectPType);
            PTypeRegistry[ListPType.Literal] = typeof(ListPType);
            PTypeRegistry[StructurePType.Literal] = typeof(StructurePType);
            PTypeRegistry[HashPType.Literal] = typeof(HashPType);

            //Assembly registry
            _registeredAssemblies = new List<Assembly>();
            foreach (AssemblyName assName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                _registeredAssemblies.Add(Assembly.Load(assName.FullName));

            //Commands
            _commands = new SymbolTable<PCommand>();
            _commandsIterator = new CommandsIterator(this);

            Commands.AddEngineCommand(PrintCommand, new Print());

            Commands.AddEngineCommand(PrintLineCommand,new PrintLine());

            Commands.AddEngineCommand(MetaCommand,
            #region Meta command implementation
 new DelegatePCommand(delegate(StackContext sctx, PValue[] args)
                                                           {
                                                               if (args.Length == 0)
                                                                   return
                                                                       sctx.CreateNativePValue(
                                                                           sctx.Implementation.
                                                                               Meta);
                                                               else
                                                               {
                                                                   List<PValue> lst =
                                                                       new List<PValue>(
                                                                           args.Length);
                                                                   MetaTable funcMT =
                                                                       sctx.Implementation.Meta;
                                                                   MetaTable appMP =
                                                                       sctx.ParentApplication.
                                                                           Meta;
                                                                   MetaTable engMT = sctx.ParentEngine.Meta;
                                                                   foreach (PValue arg in args)
                                                                   {
                                                                       string key =
                                                                           arg.CallToString(sctx);
                                                                       MetaEntry entry;
                                                                       if (
                                                                           funcMT.TryGetValue(
                                                                               key, out entry) ||
                                                                           appMP.TryGetValue(
                                                                               key, out entry) ||
                                                                           engMT.TryGetValue(
                                                                                key, out entry)
                                                                           )
                                                                           lst.Add(entry);
                                                                       else
                                                                           lst.Add(
                                                                               PType.Null.
                                                                                   CreatePValue());
                                                                   }

                                                                   if (lst.Count == 1)
                                                                       return lst[0];
                                                                   else
                                                                       return
                                                                           (PValue)lst;
                                                               }
            #endregion
                }));

            Commands.AddEngineCommand(ConcatenateCommand, new Concatenate());

            Commands.AddEngineCommand(MapCommand,new MapAll());

            Commands.AddEngineCommand(FoldLCommand,new FoldL());

            Commands.AddEngineCommand(FoldRCommand, new FoldR());

            Commands.AddEngineCommand(DisposeCommand,new Dispose());

            Commands.AddEngineCommand(CallCommand, new Call());

            Commands.AddEngineCommand(CallerCommand, new Caller());

            Commands.AddEngineCommand(PairCommand, new Pair());

            Commands.AddEngineCommand(UnbindCommand, new Unbind());
        }

        /// <summary>
        /// Alias used for the <c>concat</c> command.
        /// </summary>
        public const string ConcatenateCommand = "concat";

        /// <summary>
        /// Alias used for the <c>meta</c> command.
        /// </summary>
        public const string MetaCommand = "meta";

        /// <summary>
        /// Alias used for the <c>println</c> command.
        /// </summary>
        public const string PrintLineCommand = "println";

        /// <summary>
        /// Alias used for the <c>print</c> command.
        /// </summary>
        public const string PrintCommand = "print";

        /// <summary>
        /// Alias used for the <c>map</c> command.
        /// </summary>
        public const string MapCommand = "mapall";

        /// <summary>
        /// Alias used for the <c>foldl</c> command.
        /// </summary>
        public const string FoldLCommand = "foldl";

        /// <summary>
        /// Alias used for the <c>foldr</c> command.
        /// </summary>
        public const string FoldRCommand = "foldr";

        /// <summary>
        /// Alias used for the <c>dispose</c> command.
        /// </summary>
        public const string DisposeCommand = "dispose";

        /// <summary>
        /// Alias used for the <c>call</c> command.
        /// </summary>
        public const string CallCommand = "call";

        /// <summary>
        /// Alias used for the <c>caller</c> command.
        /// </summary>
        public const string CallerCommand = "caller";

        /// <summary>
        /// Alias used for the "pair" command.
        /// </summary>
        public const string PairCommand = "pair";

        /// <summary>
        /// Alias used for the "unbind" command.
        /// </summary>
        public const string UnbindCommand = "unbind";

        #endregion
    }
}