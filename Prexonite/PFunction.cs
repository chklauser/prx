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

#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

#endregion

namespace Prexonite
{
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
                throw new ArgumentNullException("parentApplication");
            if (declaration == null)
                throw new ArgumentNullException("declaration");

            if (!parentApplication.Module.Functions.Contains(declaration))
                throw new ArgumentException(
                    string.Format(
                        "The supplied application (instance of module {0}) does not define the function {1}.",
                        parentApplication.Module.Name, declaration));

            _parentApplication = parentApplication;
            _declaration = declaration;
        }

        #endregion

        #region Properties

        public FunctionDeclaration Declaration
        {
            get { return _declaration; }
        }

        /// <summary>
        ///     The functions id
        /// </summary>
        public string Id
        {
            [DebuggerStepThrough]
            get { return _declaration.Id; }
        }

        public string LogicalId
        {
            [DebuggerStepThrough]
            get { return _declaration.Id; }
        }

        private readonly Application _parentApplication;
        private readonly FunctionDeclaration _declaration;

        /// <summary>
        ///     The application the function belongs to.
        /// </summary>
        public Application ParentApplication
        {
            [DebuggerStepThrough]
            get { return _parentApplication; }
        }

        /// <summary>
        ///     The set of namespaces imported by this particular function.
        /// </summary>
        public SymbolCollection ImportedNamespaces
        {
            [DebuggerStepThrough]
            get { return _declaration.ImportedClrNamespaces; }
        }

        /// <summary>
        ///     The bytecode for this function.
        /// </summary>
        public List<Instruction> Code
        {
            [DebuggerStepThrough]
            get { return _declaration.Code; }
        }

        /// <summary>
        ///     The list of formal parameters for this function.
        /// </summary>
        public List<string> Parameters
        {
            [DebuggerStepThrough]
            get { return _declaration.Parameters; }
        }

        /// <summary>
        ///     The collection of variable names used by this function.
        /// </summary>
        public SymbolCollection Variables
        {
            [DebuggerStepThrough]
            get { return _declaration.LocalVariables; }
        }

        /// <summary>
        ///     Updates the mapping of local names.
        /// </summary>
        internal void CreateLocalVariableMapping()
        {
            _declaration.CreateLocalVariableMapping();
        }

        /// <summary>
        ///     The table that maps indices to local names.
        /// </summary>
        public SymbolTable<int> LocalVariableMapping
        {
            [DebuggerNonUserCode]
            get { return _declaration.LocalVariableMapping; }
        }

        public CilFunction CilImplementation
        {
            get { return _declaration.CilImplementation; }
        }

        public bool HasCilImplementation
        {
            get { return _declaration.HasCilImplementation; }
        }

        public bool IsMacro
        {
            get { return _declaration.IsMacro; }
        }

        #endregion

        #region Storage

        public IEnumerable<EntityRef.Function> GetDependencies()
        {
            return _declaration.GetDependencies();
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
            return _declaration.ToString();
        }

        public void Store(StringBuilder buffer)
        {
            _declaration.Store(buffer);
        }

        public string Store()
        {
            return _declaration.Store();
        }

        public void Store(TextWriter writer)
        {
            _declaration.Store(writer);
        }

        #endregion

        #region IHasMetaTable Members

        /// <summary>
        ///     Returns a reference to the meta table associated with this function.
        /// </summary>
        public MetaTable Meta
        {
            [DebuggerNonUserCode]
            get { return _declaration.Meta; }
        }

        #endregion

        #region IMetaFilter Members

        

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
        internal FunctionContext CreateFunctionContext
            (
            Engine engine,
            PValue[] args,
            PVariable[] sharedVariables,
            bool suppressInitialization)
        {
            return
                new FunctionContext
                    (
                    engine, this, args, sharedVariables, suppressInitialization);
        }

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext
            (
            Engine engine, PValue[] args, PVariable[] sharedVariables)
        {
            return new FunctionContext(engine, this, args, sharedVariables);
        }

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "sctx">The stack context in which to create the new context.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(StackContext sctx, PValue[] args)
        {
            return CreateFunctionContext(sctx.ParentEngine, args);
        }


        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine for which to create the new context.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(Engine engine, PValue[] args)
        {
            return new FunctionContext(engine, this, args);
        }

        /// <summary>
        ///     Creates a new function context for execution.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        public FunctionContext CreateFunctionContext(Engine engine)
        {
            return new FunctionContext(engine, this);
        }

        /// <summary>
        ///     Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <param name = "args">The arguments to pass to the function.</param>
        /// <param name = "sharedVariables">The list of variables shared with the caller.</param>
        /// <returns>The value returned by the function or {null~Null}</returns>
        public PValue Run(Engine engine, PValue[] args, PVariable[] sharedVariables)
        {
            if (HasCilImplementation)
            {
                //Fix #8
                ParentApplication.EnsureInitialization(engine);
                PValue result;
                ReturnMode returnMode;
                CilImplementation
                    (
                        this,
                        new NullContext(engine, ParentApplication, ImportedNamespaces),
                        args,
                        sharedVariables,
                        out result, out returnMode);
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
        public PValue Run(Engine engine, PValue[] args)
        {
            return Run(engine, args, null);
        }

        /// <summary>
        ///     Executes the function on the supplied engine and returns the result.
        /// </summary>
        /// <param name = "engine">The engine in which to execute the function.</param>
        /// <returns>A function context for the execution of this function.</returns>
        /// <returns>The value returned by the function or {null~Null}</returns>
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
        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
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
        StackContext IStackAware.CreateStackContext(StackContext sctx, PValue[] args)
        {
            return CreateFunctionContext(sctx, args);
        }

        #endregion

        #region Exception Handling

        /// <summary>
        ///     Causes the set of try-catch-finally blocks to be re-read on the next occurance of an exception.
        /// </summary>
        public void InvalidateTryCatchFinallyBlocks()
        {
            _declaration.InvalidateTryCatchFinallyBlocks();
        }

        /// <summary>
        ///     The cached set of try-catch-finally blocks.
        /// </summary>
        public ReadOnlyCollection<TryCatchFinallyBlock> TryCatchFinallyBlocks
        {
            get
            {
                return _declaration.TryCatchFinallyBlocks;
            }
        }

        #endregion

        EntityRef.Function INamed<EntityRef.Function>.Name
        {
            get { return ((INamed<EntityRef.Function>) _declaration).Name; }
        }
    }
}