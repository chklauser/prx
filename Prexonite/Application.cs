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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Prexonite.Compiler;
using Prexonite.Internal;

namespace Prexonite
{
    /// <summary>
    ///     An application can be compared to an assembly in the .NET framework. 
    ///     It holds functions and variables together, provides a <see cref = "MetaTable" /> and 
    ///     manages the initialization of global variables.
    /// </summary>
    public class Application : IMetaFilter,
                               IHasMetaTable,
                               IIndirectCall
    {
        #region Construction

        /// <summary>
        ///     Key used to store the id of applications, functions and variables.
        /// </summary>
        public const string IdKey = "id";

        /// <summary>
        ///     Key used to store the name of the <see cref = "EntryFunction" />.
        /// </summary>
        public const string EntryKey = "entry";

        /// <summary>
        ///     Key used to store the list of namespace imports.
        /// </summary>
        public const string ImportKey = "import";

        /// <summary>
        ///     Default name for the <see cref = "EntryFunction" />.
        /// </summary>
        public const string DefaultEntryFunction = "main";

        /// <summary>
        ///     Id of the initialization function.
        /// </summary>
        public const string InitializationId = @"\init";

        /// <summary>
        ///     Meta table key used for storing initialization generation.
        /// </summary>
        [Obsolete("Prexonite always completes partial initialization. This key has no effect.")]
        public const string InitializationGeneration = InitializationId;

        /// <summary>
        ///     Meta table key used for stroing the offset in the initialization function where
        ///     execution should continue to complete initialization.
        /// </summary>
        [Obsolete("Prexonite no longer stores the initialization offset in a meta table.")]
        public const string InitializationOffset = InitializationId;

        /// <summary>
        ///     Meta table key used as an alias for <see cref = "Application.IdKey" />
        /// </summary>
        public const string NameKey = "name";

        public const string AllowOverridingKey = "AllowOverriding";

        public static readonly MetaEntry DefaultImport = new MetaEntry(new MetaEntry[] {"System"});

        /// <summary>
        ///     Creates a new application with a GUID as its Id.
        /// </summary>
        [DebuggerStepThrough]
        public Application()
            : this("A\\" + Guid.NewGuid().ToString("N"))
        {
        }

        /// <summary>
        ///     Creates a new application with a given Id.
        /// </summary>
        /// <param name = "id">An arbitrary id for identifying the application. Prefereably a valid identifier.</param>
        public Application(string id)
        {
            _meta = MetaTable.Create(this);

            if (id == null)
                throw new ArgumentNullException("id", "Application id cannot be null.");
            if (id.Length <= 0)
                throw new ArgumentException("Application Id cannot be null.");
            //Please note that application id's do not have to be Command Script identifiers.
            _meta[IdKey] = id;
            _meta[EntryKey] = DefaultEntryFunction;
            _meta[ImportKey] = DefaultImport;

            _variables = new SymbolTable<PVariable>();

            _functions = new PFunctionTableImpl();

            _initializationFunction = new PFunction(this, InitializationId);
        }

        #endregion

        #region Variables

        private readonly SymbolTable<PVariable> _variables;

        /// <summary>
        ///     Provides access to the table of global variables.
        /// </summary>
        public SymbolTable<PVariable> Variables
        {
            [DebuggerStepThrough]
            get { return _variables; }
        }

        #endregion

        #region Functions

        private readonly PFunctionTable _functions;

        /// <summary>
        ///     Provides access to the table of registered functions.
        /// </summary>
        public PFunctionTable Functions
        {
            [DebuggerStepThrough]
            get { return _functions; }
        }

        /// <summary>
        ///     Provides direct access to the application's entry function.
        /// </summary>
        /// <value>
        ///     A reference to the application's entry function or null, if no such function does not exists.
        /// </value>
        public PFunction EntryFunction
        {
            [DebuggerStepThrough]
            get { return Functions[_meta[EntryKey]]; }
        }

        /// <summary>
        ///     Creates a new function for the application with a random Id.
        /// </summary>
        /// <returns>An unregistered function with a random Id, bound to the current application instance.</returns>
        [DebuggerStepThrough]
        public PFunction CreateFunction()
        {
            return new PFunction(this);
        }

        /// <summary>
        ///     Creates a new function for the application with a given <paramref name = "id" />.
        /// </summary>
        /// <param name = "id">An identifier to name the function.</param>
        /// <returns>An unregistered function with a given Id, bound to the current application instance.</returns>
        [DebuggerStepThrough]
        public PFunction CreateFunction(string id)
        {
            return new PFunction(this, id);
        }

        #endregion

        #region Initialization

        private ApplicationInitializationState _initializationState =
            ApplicationInitializationState.None;

        /// <summary>
        ///     Provides readonly access to the application's <see cref = "ApplicationInitializationState">initialization state</see>.
        ///     <br />
    ///     The <see cref = "InitializationState" /> is only changed by the <see cref = "Loader" /> or by <see
        ///      cref = "EnsureInitialization(Prexonite.Engine)" />.
        /// </summary>
        /// <value>A <see cref = "ApplicationInitializationState" /> that indicates the initialization state the application is currently in.</value>
        public ApplicationInitializationState InitializationState
        {
            get { return _initializationState; }
            internal set { _initializationState = value; }
        }

        private readonly PFunction _initializationFunction;

        /// <summary>
        ///     Provides access to the initialization function.
        /// </summary>
        internal PFunction _InitializationFunction
        {
            get { return _initializationFunction; }
        }

        private int _initializationGeneration = -1;

        /// <summary>
        ///     Allows you to suppress initialization of the application.
        /// </summary>
        internal bool _SuppressInitialization { get; set; }

        /// <summary>
        ///     Notifies the application that a complete initialization absolutely necessary.
        /// </summary>
        internal void _RequireInitialization()
        {
            _initializationState = ApplicationInitializationState.None;
        }

        /// <summary>
        ///     <para>Makes the application ensure that it is initialized to the point where <paramref name = "context" /> can be safely accessed.</para>
        /// </summary>
        /// <param name = "targetEngine">The engine in which to perform initialization.</param>
        /// <param name = "context">The object that triggered this method call. Normally a global variable or a function.</param>
        /// <remarks>
        ///     <para>
        ///         <ul>
        ///             <list type = "table">
        ///                 <listheader>
        ///                     <term><see cref = "InitializationState" /></term>
        ///                     <description>Behaviour</description>
        ///                 </listheader>
        ///                 <item>
        ///                     <term><see cref = "ApplicationInitializationState.None" /></term>
        ///                     <description>Initialization always required.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term><see cref = "ApplicationInitializationState.Partial" /></term>
        ///                     <description>The method checks if the initialization code for <paramref name = "context" /> has already run. 
        ///                         <br />Initialization is only required if that is not the case.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term><see cref = "ApplicationInitializationState.Complete" /></term>
        ///                     <description>No initialization required.</description>
        ///                 </item>
        ///             </list>
        ///         </ul>
        ///     </para>
        /// </remarks>
        [Obsolete("Initialization is no longer dependent on the context.")]
        public void EnsureInitialization(Engine targetEngine, IHasMetaTable context)
        {
            EnsureInitialization(targetEngine);
        }

        /// <summary>
        ///     <para>Makes the application ensure that it is initialized.</para>
        /// </summary>
        /// <param name = "targetEngine">The engine in which to perform initialization.</param>
        /// <remarks>
        ///     <para>
        ///         <ul>
        ///             <list type = "table">
        ///                 <listheader>
        ///                     <term><see cref = "InitializationState" /></term>
        ///                     <description>Behaviour</description>
        ///                 </listheader>
        ///                 <item>
        ///                     <term><see cref = "ApplicationInitializationState.None" /></term>
        ///                     <description>Initialization always required.</description>
        ///                 </item>
        ///                 <item>
        ///                     <term><see cref = "ApplicationInitializationState.Complete" /></term>
        ///                     <description>No initialization required.</description>
        ///                 </item>
        ///             </list>
        ///         </ul>
        ///     </para>
        /// </remarks>
        public void EnsureInitialization(Engine targetEngine)
        {
            if (_SuppressInitialization)
                return;
            var generation = _initializationGeneration + 1;
            switch (_initializationState)
            {
#pragma warning disable 612,618
                case ApplicationInitializationState.Partial:
#pragma warning restore 612,618
                case ApplicationInitializationState.None:
                    try
                    {
                        _SuppressInitialization = true;
                        var fctx =
                            _initializationFunction.CreateFunctionContext
                                (
                                    targetEngine,
                                    new PValue[0], // \init has no arguments
                                    new PVariable[0], // \init is not a closure
                                    true // don't initialize. That's what WE are trying to do here.
                                );

                        //Find offset at which to continue initialization. 
                        fctx.Pointer = _initializationOffset;
#if Verbose
                        Console.WriteLine("#Initialization for generation {0} (offset = {1}) required by {2}.", generation, offset, context);
#endif

                        //Execute the part of the initialize function that is missing
                        targetEngine.Stack.AddLast(fctx);
                        try
                        {
                            targetEngine.Process();
                        }
                        finally
                        {
                            //Save the current initialization state (offset)
                            _initializationOffset = _initializationFunction.Code.Count;
                            _initializationGeneration = generation;
                            _initializationState = ApplicationInitializationState.Complete;
                        }
                    }
                    finally
                    {
                        _SuppressInitialization = false;
                    }
                    break;
                case ApplicationInitializationState.Complete:
                    break;
                default:
                    throw new PrexoniteException(
                        "Invalid InitializationState " + _initializationState);
            }
        }

        private int _initializationOffset;

        #endregion

        #region Execution

        /// <summary>
        ///     Executes the application's <see cref = "EntryFunction">entry function</see> in the given <paramref
        ///      name = "parentEngine">Engine</paramref> and returns it's result.
        /// </summary>
        /// <param name = "parentEngine">The engine in which execute the entry function.</param>
        /// <param name = "args">The actual arguments for the entry function.</param>
        /// <returns>The value returned by the entry function.</returns>
        public PValue Run(Engine parentEngine, PValue[] args)
        {
            string entryName = Meta[EntryKey];
            PFunction func;
            if (!Functions.TryGetValue(entryName, out func))
                throw new PrexoniteException(
                    "Cannot find an entry function named \"" + entryName + "\"");

            //Make sure the functions environment is initialized.
            EnsureInitialization(parentEngine);

            return func.Run(parentEngine, args);
        }

        /// <summary>
        ///     Executes the application's <see cref = "EntryFunction">entry function</see> in the given <paramref
        ///      name = "parentEngine">Engine</paramref> and returns it's result.<br />
        ///     This overload does not supply any arguments.
        /// </summary>
        /// <param name = "parentEngine">The engine in which execute the entry function.</param>
        /// <returns>The value returned by the entry function.</returns>
        public PValue Run(Engine parentEngine)
        {
            return Run(parentEngine, new PValue[] {});
        }

        #endregion

        #region Storage

        /// <summary>
        ///     Writes the application to a file using the default settings.
        /// </summary>
        /// <param name = "path">Path to the file to (over) write.</param>
        /// <remarks>
        ///     Use a <see cref = "Loader" /> for more control over the amount of information stored in the file.
        /// </remarks>
        [DebuggerStepThrough]
        public void StoreInFile(string path)
        {
            //Create a crippled engine for this process
            var eng = new Engine {ExecutionProhibited = true};
            var ldr = new Loader(eng, this);
            ldr.StoreInFile(path);
        }

        /// <summary>
        ///     Writes the application to a string using the default settings.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If you need more control over the amount of information stored in the string, use the <see cref = "Loader" /> class and a customized <see
        ///      cref = "LoaderOptions" /> instance.
        ///     </para>
        ///     <para>
        ///         Use <see cref = "Store" /> if possible as it far more memory friendly than strings in some cases.
        ///     </para>
        /// </remarks>
        /// <returns>A string that contains the serialized application.</returns>
        /// <seealso cref = "Store">Includes a more efficient way to write the application to stdout.</seealso>
        [DebuggerStepThrough]
        public string StoreInString()
        {
            var writer = new StringWriter();
            Store(writer);
            return writer.ToString();
        }

        /// <summary>
        ///     Writes the application to the supplied <paramref name = "writer" /> using the default settings.
        /// </summary>
        /// <param name = "writer">The <see cref = "TextWriter" /> to write the application to.</param>
        /// <remarks>
        ///     <para>
        ///         <c>Store</c> is always superior to <see cref = "StoreInString" />.
        ///     </para>
        ///     <example>
        ///         <para>
        ///             If you want to write the application to stdout, use <see cref = "Store" /> and not <see
        ///      cref = "StoreInString" /> like in the following example:
        ///         </para>
        ///         <code>
        ///             public void WriteApplicationToStdOut(Application app)
        ///             {
        ///             app.Store(Console.Out);
        ///             //instead of
        ///             //  Console.Write(app.StoreInString());
        ///             }
        ///         </code>
        ///         <para>
        ///             By using the <see cref = "Store" />, everything Prexonite assembles is immedeately sent to stdout.
        ///         </para>
        ///     </example>
        /// </remarks>
        public void Store(TextWriter writer)
        {
            //Create a crippled engine for this process
            var eng = new Engine {ExecutionProhibited = true};
            var ldr = new Loader(eng, this);
            ldr.Store(writer);
        }

        #endregion

        #region IMetaFilter Members

        [DebuggerStepThrough]
        string IMetaFilter.GetTransform(string key)
        {
            if (Engine.StringsAreEqual(key, NameKey))
                return IdKey;
            else if (Engine.StringsAreEqual(key, "imports"))
                return ImportKey;
            else
                return key;
        }

        [DebuggerStepThrough]
        KeyValuePair<string, MetaEntry>? IMetaFilter.SetTransform(
            KeyValuePair<string, MetaEntry> item)
        {
            //Unlike the function, the application allows name changes
            if (Engine.StringsAreEqual(item.Key, NameKey))
                item = new KeyValuePair<string, MetaEntry>(IdKey, item.Value);
            else if (Engine.StringsAreEqual(item.Key, "imports"))
                item = new KeyValuePair<string, MetaEntry>(ImportKey, item.Value);
            return item;
        }

        #endregion

        #region IHasMetaTable Members

        private readonly MetaTable _meta;

        /// <summary>
        ///     The applications metadata structure.
        /// </summary>
        public MetaTable Meta
        {
            [DebuggerStepThrough]
            get { return _meta; }
        }

        #endregion

        /// <summary>
        ///     The id of the application. In many cases just a random (using <see cref = "Guid" />) identifier.
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get { return Meta[IdKey].Text; }
        }

        #region IIndirectCall Members

        /// <summary>
        ///     Invokes the application's entry function with the supplied <paramref name = "args">arguments</paramref>.
        /// </summary>
        /// <param name = "sctx">The stack context in which to invoke the entry function.</param>
        /// <param name = "args">The arguments to pass to the function call.</param>
        /// <returns>The value returned by the entry function.</returns>
        /// <seealso cref = "EntryKey" />
        /// <seealso cref = "EntryFunction" />
        [DebuggerStepThrough]
        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return Run(sctx.ParentEngine, args);
        }

        #endregion
    }

    /// <summary>
    ///     Defines the possible states of initialization a application can be in.
    /// </summary>
    public enum ApplicationInitializationState
    {
        /// <summary>
        ///     The application has not benn initialized or needs a complete re-initialization.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The application is only partially initialized.
        /// </summary>
        [Obsolete("Prexonite no longer distinguishes between partial and no initialization. Use None instead. The behaviour is the same.")]
        Partial = 1,

        /// <summary>
        ///     The application is completely initialized.
        /// </summary>
        Complete = 2
    }
}