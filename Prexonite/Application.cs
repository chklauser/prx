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

using System;
using System.Collections.Generic;
using System.IO;
using Prexonite.Compiler;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    /// <summary>
    /// An application can be compared to an assembly in the .NET framework. 
    /// It holds functions and variables together, provides a <see cref="MetaTable"/> and m
    /// anages the initialization of global variables.
    /// </summary>
    public class Application : IMetaFilter,
                               IHasMetaTable,
                               IIndirectCall
    {
        #region Construction

        /// <summary>
        /// Key used to store the id of applications, functions and variables.
        /// </summary>
        public const string IdKey = "id";

        /// <summary>
        /// Key used to store the name of the <see cref="EntryFunction"/>.
        /// </summary>
        public const string EntryKey = "entry";

        /// <summary>
        /// Key used to store the list of namespace imports.
        /// </summary>
        public const string ImportKey = "import";

        /// <summary>
        /// Default name for the <see cref="EntryFunction"/>.
        /// </summary>
        public const string DefaultEntryFunction = "main";

        /// <summary>
        /// Id for the initialization function.
        /// </summary>
        public const string InitializationId = @"\init";

        /// <summary>
        /// Metatable key used as an alias for <see cref="Application.IdKey"/>
        /// </summary>
        public const string NameKey = "name";

        public static readonly MetaEntry DefaultImport = new MetaEntry(new MetaEntry[] {"System"});

        /// <summary>
        /// Creates a new application with a GUID as its Id.
        /// </summary>
        [NoDebug]
        public Application()
            : this("A\\" + Guid.NewGuid().ToString("N"))
        {
        }

        /// <summary>
        /// Creates a new application with a given Id.
        /// </summary>
        /// <param name="id">An arbitrary id for identifying the application. Prefereably a valid identifier.</param>
        [NoDebug]
        public Application(string id)
        {
            _meta = new MetaTable(this);

            if (id == null)
                throw new ArgumentNullException("Application id cannot be null.");
            if (id.Length <= 0)
                throw new ArgumentException("Application Id cannot be null.");
            //Please note that application id's do not have to be Command Script identifiers.
            _meta[IdKey] = id;
            _meta[EntryKey] = DefaultEntryFunction;
            _meta[ImportKey] = DefaultImport;

            _variables = new SymbolTable<PVariable>();

            _functions = new PFunctionTable();

            _initializationFunction = new PFunction(this, InitializationId);
        }

        #endregion

        #region Variables

        private SymbolTable<PVariable> _variables;

        /// <summary>
        /// Provides access to the table of global variables.
        /// </summary>
        public SymbolTable<PVariable> Variables
        {
            [NoDebug()]
            get { return _variables; }
        }

        #endregion

        #region Functions

        private PFunctionTable _functions;

        /// <summary>
        /// Provides access to the table of registered functions.
        /// </summary>
        public PFunctionTable Functions
        {
            [NoDebug]
            get { return _functions; }
        }

        /// <summary>
        /// Provides direct access to the application's entry function.
        /// </summary>
        /// <value>
        /// A reference to the application's entry function or null, if no such function does not exists.
        /// </value>
        public PFunction EntryFunction
        {
            [NoDebug]
            get { return Functions[_meta[EntryKey]]; }
        }

        /// <summary>
        /// Creates a new function for the application with a random Id.
        /// </summary>
        /// <returns>An unregistered function with a random Id, bound to the current application instance.</returns>
        [NoDebug]
        public PFunction CreateFunction()
        {
            return new PFunction(this);
        }

        /// <summary>
        /// Creates a new function for the application with a given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">An identifier to name the function.</param>
        /// <returns>An unregistered function with a given Id, bound to the current application instance.</returns>
        [NoDebug]
        public PFunction CreateFunction(string id)
        {
            return new PFunction(this, id);
        }

        #endregion

        #region Initialization

        private ApplicationInitializationState _initalizationState = ApplicationInitializationState.None;

        /// <summary>
        /// Provides readonly access to the application's <see cref="ApplicationInitializationState">initialization state</see>.
        /// <br />
        /// The <see cref="InitalizationState"/> is only changed by the <see cref="Loader"/> or by <see cref="EnsureInitialization"/>.
        /// </summary>
        /// <value>A <see cref="ApplicationInitializationState"/> that indicates the initialization state the application is currently in.</value>
        public ApplicationInitializationState InitalizationState
        {
            get { return _initalizationState; }
            internal set { _initalizationState = value; }
        }

        private PFunction _initializationFunction;

        /// <summary>
        /// Provides access to the initialization function.
        /// </summary>
        internal PFunction _InitializationFunction
        {
            get { return _initializationFunction; }
            set { _initializationFunction = value; }
        }

        private int _initializationGeneration = -1;

        /// <summary>
        /// Provides access to the appliaction's initialization generation
        /// </summary>
        internal int _InitializationGeneration
        {
            get { return _initializationGeneration; }
            set { _initializationGeneration = value; }
        }

        private bool _suppressInitialization = false;

        /// <summary>
        /// Allows you to suppress initialization of the application.
        /// </summary>
        internal bool _SuppressInitialization
        {
            get { return _suppressInitialization; }
            set { _suppressInitialization = value; }
        }

        /// <summary>
        /// Notifies the application of a change in it's code (more specifically the initialize function)
        /// </summary>
        /// <returns>The initialization generation that is required to trigger initialization.</returns>
        internal int _RegisterInitializationUpdate()
        {
            if (_initalizationState == ApplicationInitializationState.Complete)
                _initalizationState = ApplicationInitializationState.Partial;

            return _initializationGeneration + 1;
        }

        /// <summary>
        /// Notifies the application that a complete initialization absolutely necessary.
        /// </summary>
        internal void _RequireInitialization()
        {
            _initalizationState = ApplicationInitializationState.None;
        }

        /// <summary>
        /// <para>Makes the application ensure that it is initialized to the point where <paramref name="context"/> can be safely accessed.</para>
        /// </summary>
        /// <param name="targetEngine">The engine in which to perform initialization.</param>
        /// <param name="context">The object that triggered this method call. Normally a global variable or a function.</param>
        /// <remarks>
        /// <para>
        ///     <ul>
        ///         <list type="table">
        ///             <listheader>
        ///                 <term><see cref="InitalizationState"/></term>
        ///                 <description>Behaviour</description>
        ///             </listheader>
        ///             <item>
        ///                 <term><see cref="ApplicationInitializationState.None"/></term>
        ///                 <description>Initialization always required.</description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="ApplicationInitializationState.Partial"/></term>
        ///                 <description>The method checks if the initialization code for <paramref name="context"/> has already run. 
        ///                 <br />Initialization is only required if that is not the case.</description>
        ///             </item>
        ///             <item>
        ///                 <term><see cref="ApplicationInitializationState.Complete"/></term>
        ///                 <description>No initialization required.</description>
        ///             </item>
        ///         </list>
        ///     </ul>
        /// </para>
        /// </remarks>
        public void EnsureInitialization(Engine targetEngine, IHasMetaTable context)
        {
            if (_suppressInitialization)
                return;
            MetaEntry init;
            int generation = _initializationGeneration + 1;
            int offset;
            switch (_initalizationState)
            {
                case ApplicationInitializationState.None:
                    try
                    {
                        _suppressInitialization = true;
                        FunctionContext fctx =
                            _initializationFunction.CreateFunctionContext(targetEngine, new PValue[] {},
                                                                          new PVariable[] {}, true);
                        if (
                            (!(_initializationFunction.Meta.TryGetValue(InitializationId, out init) &&
                               int.TryParse(init.Text, out offset))) || offset < 0)
                            offset = 0;
                        fctx.Pointer = offset;
#if Verbose
                        Console.WriteLine("#Initialization for generation {0} (offset = {1}) required by {2}.", generation, offset, context);
#endif

                        //Execute the part of the initialize function that is missing
                        targetEngine.Stack.AddLast(fctx);
                        targetEngine.Process();

                        //Save the current initialization state
                        _initializationFunction.Meta[InitializationId] = _initializationFunction.Code.Count.ToString();
                        _initializationGeneration = generation;
                        _initalizationState = ApplicationInitializationState.Complete;
                    }
                    finally
                    {
                        _suppressInitialization = false;
                    }
                    break;
                case ApplicationInitializationState.Complete:
                    break;
                case ApplicationInitializationState.Partial:
                    if (context.Meta.TryGetValue(InitializationId, out init))
                    {
                        context.Meta.Remove(InitializationId); //Entry no longer required
                        if (int.TryParse(init.Text, out generation) && generation > _initializationGeneration)
                            goto case ApplicationInitializationState.None;
                    }
                    break;
                default:
                    throw new PrexoniteException("Invalid InitializationState " + _initalizationState);
            }
        }

        #endregion

        #region Execution

        /// <summary>
        /// Executes the application's <see cref="EntryFunction">entry function</see> in the given <paramref name="parentEngine">Engine</paramref> and returns it's result.
        /// </summary>
        /// <param name="parentEngine">The engine in which execute the entry function.</param>
        /// <param name="args">The actual arguments for the entry function.</param>
        /// <returns>The value returned by the entry function.</returns>
        public PValue Run(Engine parentEngine, PValue[] args)
        {
            string entryName = Meta[EntryKey];
            if (!Functions.Contains(entryName))
                throw new PrexoniteException("Cannot find an entry function named \"" + entryName + "\"");
            FunctionContext fctx = Functions[entryName].CreateFunctionContext(parentEngine, args);
            parentEngine.Stack.AddLast(fctx);
            parentEngine.Process();
            return fctx.ReturnValue;
        }

        /// <summary>
        /// Executes the application's <see cref="EntryFunction">entry function</see> in the given <paramref name="parentEngine">Engine</paramref> and returns it's result.<br />
        /// This overload does not supply any arguments.
        /// </summary>
        /// <param name="parentEngine">The engine in which execute the entry function.</param>
        /// <returns>The value returned by the entry function.</returns>
        public PValue Run(Engine parentEngine)
        {
            return Run(parentEngine, new PValue[] {});
        }

        #endregion

        #region Storage

        /// <summary>
        /// Writes the application to a file using the default settings.
        /// </summary>
        /// <param name="path">Path to the file to (over) write.</param>
        /// <remarks>Use a <see cref="Loader"/> for more control over the amount of information stored in the file.</remarks>
        [NoDebug]
        public void StoreInFile(string path)
        {
            //Create a crippled engine for this process
            Engine eng = new Engine();
            eng.ExecutionProhibited = true;
            Loader ldr = new Loader(eng, this);
            ldr.StoreInFile(path);
        }

        /// <summary>
        /// Writes the application to a string using the default settings.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     If you need more control over the amount of information stored in the string, use the <see cref="Loader"/> class and a customized <see cref="LoaderOptions"/> instance.
        /// </para>
        /// <para>
        ///     Use <see cref="Store"/> if possible as it far more memory friendly than strings in some cases.
        /// </para>
        /// </remarks>
        /// <returns>A string that contains the serialized application.</returns>
        /// <seealso cref="Store">Includes a more efficient way to write the application to stdout.</seealso>
        [NoDebug]
        public string StoreInString()
        {
            StringWriter writer = new StringWriter();
            Store(writer);
            return writer.ToString();
        }

        /// <summary>
        /// Writes the application to the supplied <paramref name="writer"/> using the default settings.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the application to.</param>
        /// <remarks>
        /// <para>
        ///     <c>Store</c> is always superior to <see cref="StoreInString"/>.
        /// </para>
        /// <example>
        /// <para>
        ///     If you want to write the application to stdout, use <see cref="Store"/> and not <see cref="StoreInString"/> like in the following example:
        /// </para>
        /// <code>
        /// public void WriteApplicationToStdOut(Application app)
        /// {
        ///     app.Store(Console.Out);
        ///     //instead of
        ///     //  Console.Write(app.StoreInString());
        /// }
        /// </code>
        /// <para>
        ///     By using the <see cref="Store"/>, everything Prexonite assembles is immedeately sent to stdout.
        /// </para>
        /// </example>
        /// </remarks>
        public void Store(TextWriter writer)
        {
            //Create a crippled engine for this process
            Engine eng = new Engine();
            eng.ExecutionProhibited = true;
            Loader ldr = new Loader(eng, this);
            ldr.Store(writer);
        }

        #endregion

        #region IMetaFilter Members

        [NoDebug]
        string IMetaFilter.GetTransform(string key)
        {
            if (Engine.StringsAreEqual(key, "name"))
                return IdKey;
            else if (Engine.StringsAreEqual(key, "imports"))
                return "import";
            else
                return key;
        }

        [NoDebug]
        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(KeyValuePair<string, MetaEntry> item)
        {
            //Unlike the function, the application allows name changes
            if (Engine.StringsAreEqual(item.Key, "name"))
                item = new KeyValuePair<string, MetaEntry>(IdKey, item.Value);
            else if (Engine.StringsAreEqual(item.Key, "imports"))
                item = new KeyValuePair<string, MetaEntry>("import", item.Value);
            return item;
        }

        #endregion

        #region IHasMetaTable Members

        private MetaTable _meta;

        /// <summary>
        /// The applications metadata structure.
        /// </summary>
        public MetaTable Meta
        {
            [NoDebug()]
            get { return _meta; }
        }

        #endregion

        /// <summary>
        /// The id of the application. In many cases just a random (using <see cref="Guid"/>) identifier.
        /// </summary>
        public string Id
        {
            [NoDebug]
            get { return Meta[IdKey].Text; }
        }

        #region IIndirectCall Members

        /// <summary>
        /// Invokes the application's entry function with the supplied <paramref name="args">arguments</paramref>.
        /// </summary>
        /// <param name="sctx">The stack context in which to invoke the entry function.</param>
        /// <param name="args">The arguments to pass to the function call.</param>
        /// <returns>The value returned by the entry function.</returns>
        /// <seealso cref="EntryKey"/>
        /// <seealso cref="EntryFunction"/>
        [NoDebug]
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return Run(sctx.ParentEngine, args);
        }

        #endregion
    }

    /// <summary>
    /// Defines the possible states of initialization a application can be in.
    /// </summary>
    public enum ApplicationInitializationState
    {
        /// <summary>
        /// The application has not benn initialized or needs a complete re-initialization.
        /// </summary>
        None = 0,
        /// <summary>
        /// The application is only partially initialized.
        /// </summary>
        Partial = 1,
        /// <summary>
        /// The application is completely initialized.
        /// </summary>
        Complete = 2
    }
}