﻿// Prexonite
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
using System.IO;
using JetBrains.Annotations;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Types;

namespace Prexonite.Modular
{
    /// <summary>
    /// Represents a reference to an entity in Prexonite.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Kind</term>
    ///         <term>Attributes</term>
    ///         <term>Options</term>
    ///     </listheader>
    ///     <item>
    ///         <term>Function</term>
    ///         <description>Id, Name</description>
    ///         <description>IsInMacroContext, IsMacro</description>
    ///     </item>
    ///     <item>
    ///         <term>Command</term>
    ///         <description>Id</description>
    ///         <description></description>
    ///     </item>
    ///     <item>
    ///         <term>LocalVariable</term>
    ///         <description>Id</description>
    ///         <description>IsReference</description>
    ///     </item>
    ///     <item>
    ///         <term>GlobalVariable</term>
    ///         <description>Id, Name</description>
    ///         <description>IsReference</description>
    ///     </item>
    ///     <item>
    ///         <term>MacroCommand</term>
    ///         <description>Id</description>
    ///         <description></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public abstract class EntityRef
    {
        #region Pattern Matching

        public abstract TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument);

        public virtual bool TryGetFunction(out Function func)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetMacroCommand(out MacroCommand mcmd)
        {
            mcmd = null;
            return false;
        }

        public virtual bool TryGetCommand(out Command cmd)
        {
            cmd = null;
            return false;
        }

        public virtual bool TryGetVariable(out Variable variable)
        {
            variable = null;
            return false;
        }

        public virtual bool TryGetLocalVariable(out Variable.Local variable)
        {
            variable = null;
            return false;
        }

        public virtual bool TryGetGlobalVariable(out Variable.Global variable)
        {
            variable = null;
            return false;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Creates a <see cref="SymbolEntry"/> that refers to the same entity as the <see cref="EntityRef"/>. This is a narrowing (lossy) conversion.
        /// </summary>
        /// <returns>A <see cref="SymbolEntry"/> that referes to the same entity as the <see cref="EntityRef"/>.</returns>
        public abstract SymbolEntry ToSymbolEntry();

        public static explicit operator SymbolEntry(EntityRef entityRef)
        {
            if (entityRef == null)
                throw new ArgumentNullException("entityRef");
            return entityRef.ToSymbolEntry();
        }

        #endregion

        #region Classification

        #region Nested type: ICompileTime

        public interface ICompileTime
        {
            bool TryGetEntity(Loader ldr, out PValue entity);
        }

        #endregion

        #region Nested type: IMacro

        public interface IMacro : ICompileTime
        {
        }

        #endregion

        #region Nested type: IRunTime

        public interface IRunTime
        {
            bool TryGetEntity(StackContext sctx, out PValue entity);
        }

        #endregion

        #endregion

        #region ToString

        /// <summary>
        /// Writes a human-readable representation of this <see cref="EntityRef"/> to the supplied <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The writer to write to. Must not be null.</param>
        [PublicAPI]
        public abstract void ToString([NotNull] TextWriter writer);

        public sealed override string ToString()
        {
            var sw = new StringWriter();
            ToString(sw);
            return sw.ToString();
        }

        #endregion

        #region Functions

        [DebuggerDisplay("function {Id}/{ModuleName}")]
        public class Function : EntityRef, IEquatable<Function>, IRunTime
        {
            [NotNull]
            private readonly string _id;

            [NotNull]
            private readonly ModuleName _moduleName;

            bool IEquatable<Function>.Equals(Function other)
            {
                return EqualsFunction(other);
            }

            public override bool TryGetFunction(out Function func)
            {
                func = this;
                return true;
            }

            protected bool EqualsFunction(Function other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._id, _id) && Equals(other._moduleName, _moduleName);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return EqualsFunction((Function) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_id.GetHashCode()*397) ^ _moduleName.GetHashCode();
                }
            }

            public bool TryGetEntity(StackContext sctx, out PValue entity)
            {
                PFunction func;
                if(sctx.ParentApplication.TryGetFunction(Id,ModuleName, out func))
                {
                    entity = sctx.CreateNativePValue(func);
                    return true;
                }
                else
                {
                    entity = PType.Null;
                    return false;
                }
            }

            public static bool operator ==(Function left, Function right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Function left, Function right)
            {
                return !Equals(left, right);
            }

            private Function([NotNull] string id, [NotNull] ModuleName moduleName)
            {
                if(id == null)
                    throw new ArgumentNullException("id");
                if(moduleName == null)
                    throw new ArgumentNullException("moduleName");

                _id = id;
                _moduleName = moduleName;
            }

            public string Id
            {
                get { return _id; }
            }

            public ModuleName ModuleName
            {
                get { return _moduleName; }
            }

            public bool TryGetFunction(Application application, out PFunction func)
            {
                if (!application.Compound.TryGetApplication(_moduleName, out application)
                    || !application.Functions.TryGetValue(_id, out func))
                {
                    func = null;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
            {
                return matcher.OnFunction(this,argument);
            }

            public override SymbolEntry ToSymbolEntry()
            {
                return new SymbolEntry(SymbolInterpretations.Function, Id, ModuleName);
            }

            public override void ToString(TextWriter writer)
            {
                writer.Write("func ");
                writer.Write(_id);
                writer.Write("/");
                writer.Write(_moduleName);
            }

            internal override bool _TryLookup(StackContext sctx, out PValue entity)
            {
                var app = sctx.ParentApplication;
                if (!app.Compound.TryGetApplication(ModuleName, out app))
                {
                    entity = null;
                    return false;
                }

                PFunction func;
                if(!app.Functions.TryGetValue(Id,out func))
                {
                    entity = null;
                    return false;
                }

                entity = sctx.CreateNativePValue(func);
                return true;
            }

            public static Function Create([NotNull] string internalId, [NotNull] ModuleName moduleName)
            {
                Debug.Assert(moduleName != null, string.Format("Module name is null for entity ref to function {0}.", internalId));
                return new Function(internalId, moduleName);
            }
        }

        #endregion

        #region Commands

        [DebuggerDisplay("command {Id}")]
        public sealed class Command : EntityRef, IRunTime, IEquatable<Command>
        {
            public bool Equals(Command other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._id, _id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Command)) return false;
                return Equals((Command) obj);
            }

            public override int GetHashCode()
            {
                return (_id != null ? _id.GetHashCode() : 0);
            }

            public static bool operator ==(Command left, Command right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Command left, Command right)
            {
                return !Equals(left, right);
            }

            private readonly string _id;

            private Command(string id)
            {
                _id = id;
            }

            public string Id
            {
                get { return _id; }
            }

            #region IRunTime Members

            public bool TryGetEntity(StackContext sctx, out PValue entity)
            {
                PCommand cmd;
                if (sctx.ParentEngine.Commands.TryGetValue(Id, out cmd))
                {
                    entity = sctx.CreateNativePValue(cmd);
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            #endregion

            public static Command Create(string id)
            {
                return new Command(id);
            }

            public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
            {
                return matcher.OnCommand(this,argument);
            }

            public override bool TryGetCommand(out Command cmd)
            {
                cmd = this;
                return true;
            }

            public override SymbolEntry ToSymbolEntry()
            {
                return new SymbolEntry(SymbolInterpretations.Command, Id, null);
            }

            public override void ToString(TextWriter writer)
            {
                writer.Write("cmd ");
                writer.Write(_id);
            }

            internal override bool _TryLookup(StackContext sctx, out PValue entity)
            {
                PCommand command;
                if(sctx.ParentEngine.Commands.TryGetValue(Id,out command))
                {
                    entity = sctx.CreateNativePValue(command);
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }
        }

        #endregion

        #region Variables

        public abstract class Variable : EntityRef, IRunTime, IEquatable<Variable>
        {
            private Variable()
            {
            }

            #region IRunTime Members

            public abstract bool TryGetEntity(StackContext sctx, out PValue entity);

            #endregion

            public override bool TryGetVariable(out Variable variable)
            {
                variable = this;
                return true;
            }


            protected abstract bool EqualsVariable(Variable other);
            bool IEquatable<Variable>.Equals(Variable other)
            {
                return EqualsVariable(other);
            }

            #region Nested type: Global

            [DebuggerDisplay("global var {Id}/{ModuleName}")]
            public sealed class Global : Variable, IEquatable<Global>
            {
                [NotNull]
                private readonly string _id;

                [NotNull]
                private readonly ModuleName _moduleName;

                private Global([NotNull] string id, [NotNull] ModuleName moduleName)
                {
                    if(id == null)
                        throw new ArgumentNullException("id");
                    if (moduleName == null)
                        throw new ArgumentNullException("moduleName");

                    _id = id;
                    _moduleName = moduleName;
                }

                [NotNull]
                public string Id
                {
                    get { return _id; }
                }

                [NotNull]
                public ModuleName ModuleName
                {
                    get { return _moduleName; }
                }

                public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
                {
                    return matcher.OnGlobalVariable(this,argument);
                }

                public override bool TryGetGlobalVariable(out Global variable)
                {
                    variable = this;
                    return true;
                }

                public static Global Create([NotNull] string id, [NotNull] ModuleName moduleName)
                {
                    Debug.Assert(moduleName != null,string.Format("Module name is null for entity ref to global variable {0}.", id));
                    return new Global(id, moduleName);
                }

                public override bool TryGetEntity(StackContext sctx, out PValue entity)
                {
                    Application application;
                    PVariable v;
                    if (sctx.ParentApplication.Compound.TryGetApplication(_moduleName,
                        out application)
                            && application.Variables.TryGetValue(_id, out v))
                    {
                        entity = sctx.CreateNativePValue(v);
                        return true;
                    }
                    else
                    {
                        entity = null;
                        return false;
                    }
                }

                public override SymbolEntry ToSymbolEntry()
                {
                    return new SymbolEntry(SymbolInterpretations.GlobalObjectVariable, Id,ModuleName);
                }

                public override void ToString(TextWriter writer)
                {
                    writer.Write("gvar ");
                    writer.Write(_id);
                    writer.Write("/");
                    writer.Write(_moduleName);
                }

                internal override bool _TryLookup(StackContext sctx, out PValue entity)
                {
                    var app = sctx.ParentApplication;
                    if (!app.Compound.TryGetApplication(ModuleName, out app))
                    {
                        entity = null;
                        return false;
                    }

                    PVariable pvar;
                    if (!app.Variables.TryGetValue(Id, out pvar))
                    {
                        entity = null;
                        return false;
                    }

                    entity = sctx.CreateNativePValue(pvar);
                    return true;
                }

                protected override bool EqualsVariable(Variable other)
                {
                    var g = other as Global;
                    return g != null && Equals(g);
                }

                public bool Equals(Global other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    
                    return Equals(other._id, _id) && Equals(other._moduleName, _moduleName);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != typeof (Global)) return false;
                    return Equals((Global) obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return ((_id.GetHashCode())*397) ^ (_moduleName.GetHashCode());
                    }
                }

                public static bool operator ==(Global left, Global right)
                {
                    return Equals(left, right);
                }

                public static bool operator !=(Global left, Global right)
                {
                    return !Equals(left, right);
                }
            }

            #endregion

            #region Nested type: Local

            [DebuggerDisplay("local var {Id}")]
            public sealed class Local : Variable, IEquatable<Local>
            {
                private readonly string _id;
                private readonly int? _index;

                private Local(string id, int? index = null)
                {
                    _id = id;
                    _index = index;
                }

                public string Id
                {
                    get { return _id; }
                }

                public int? Index
                {
                    get { return _index; }
                }

                public bool Equals(Local other)
                {
                    if (ReferenceEquals(null, other)) return false;
                    if (ReferenceEquals(this, other)) return true;
                    return Equals(other._id, _id);
                }

                protected override bool EqualsVariable(Variable other)
                {
                    var local = other as Local;
                    return local != null && Equals(local);
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != typeof (Local)) return false;
                    return Equals((Local) obj);
                }

                public override int GetHashCode()
                {
                    return (_id != null ? _id.GetHashCode() : 0);
                }

                public static bool operator ==(Local left, Local right)
                {
                    return Equals(left, right);
                }

                public static bool operator !=(Local left, Local right)
                {
                    return !Equals(left, right);
                }

                public static Local Create(string id)
                {
                    return new Local(id);
                }

                public Local WithIndex(int index)
                {
                    return new Local(Id, index);
                }

                public override bool TryGetEntity(StackContext sctx, out PValue entity)
                {
                    var fctx = sctx as FunctionContext;
                    PVariable v;
                    if (fctx != null && fctx.LocalVariables.TryGetValue(_id, out v))
                    {
                        entity = sctx.CreateNativePValue(v);
                        return true;
                    }
                    else
                    {
                        entity = null;
                        return false;
                    }
                }

                public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
                {
                    return matcher.OnLocalVariable(this,argument);
                }

                public override bool TryGetLocalVariable(out Local variable)
                {
                    variable = this;
                    return true;
                }

                public override SymbolEntry ToSymbolEntry()
                {
                    return new SymbolEntry(SymbolInterpretations.LocalObjectVariable, Id, null);
                }

                public override void ToString(TextWriter writer)
                {
                    writer.Write("var ");
                    writer.Write(_id);
                }

                internal override bool _TryLookup(StackContext sctx, out PValue entity)
                {
                    var fctx = sctx as FunctionContext;
                    if(fctx == null)
                    {
                        entity = null;
                        return false;
                    }

                    PVariable pvar;
                    if(fctx.LocalVariables.TryGetValue(Id, out pvar))
                    {
                        entity = sctx.CreateNativePValue(pvar);
                        return true;
                    }
                    else
                    {
                        entity = null;
                        return false;
                    }
                }
            }

            #endregion

        }

        #endregion

        #region MacroCommands

        [DebuggerDisplay("macro command {Id}")]
        public class MacroCommand : EntityRef, IMacro, IEquatable<MacroCommand>
        {
            private readonly string _id;

            private MacroCommand(string id)
            {
                _id = id;
            }

            public string Id
            {
                get { return _id; }
            }

            #region ICompileTime Members

            public bool TryGetEntity(Loader ldr, out PValue entity)
            {
                Compiler.Macro.MacroCommand mcmd;
                if (ldr.MacroCommands.TryGetValue(_id, out mcmd))
                {
                    entity = ldr.CreateNativePValue(mcmd);
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            #endregion

            public static MacroCommand Create(string id)
            {
                return new MacroCommand(id);
            }

            public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
            {
                return matcher.OnMacroCommand(this, argument);
            }

            public override bool TryGetMacroCommand(out MacroCommand mcmd)
            {
                mcmd = this;
                return true;
            }

            public override SymbolEntry ToSymbolEntry()
            {
                return new SymbolEntry(SymbolInterpretations.MacroCommand, Id, null);
            }

            public override void ToString(TextWriter writer)
            {
                writer.Write("mcmd ");
                writer.Write(_id);
            }

            internal override bool _TryLookup(StackContext sctx, out PValue entity)
            {
                //first: lookup in sctx (if it is a loader)
                var ldr = sctx as Loader;
                if (ldr != null)
                    return _tryMcmdFromLoader(sctx, ldr, out entity);

                //else: search stack beginning at sctx
                bool foundEntity;
                if (_tryMcmdFromStack(sctx, sctx.ParentEngine.Stack.FindLast(sctx), out foundEntity, out entity)) 
                    return foundEntity;

                //finally: search stack from bottom
                return _tryMcmdFromStack(sctx, sctx.ParentEngine.Stack.Last, out foundEntity, out entity) && foundEntity;
            }

            private bool _tryMcmdFromStack(StackContext sctx, LinkedListNode<StackContext> node, out bool foundEntity, out PValue entity)
            {
                Loader ldr;
                while (node != null)
                {
                    ldr = node.Value as Loader;
                    if (ldr != null)
                    {
                        foundEntity = _tryMcmdFromLoader(sctx, ldr, out entity);
                        return true;
                    }
                    node = node.Previous;
                }

                entity = null;
                foundEntity = false;
                return false;
            }

            private bool _tryMcmdFromLoader(StackContext sctx, Loader ldr, out PValue entity)
            {
                Compiler.Macro.MacroCommand mcmd;
                if (ldr.MacroCommands.TryGetValue(Id, out mcmd))
                {
                    entity = sctx.CreateNativePValue(mcmd);
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            public bool Equals(MacroCommand other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other._id, _id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (MacroCommand)) return false;
                return Equals((MacroCommand) obj);
            }

            public override int GetHashCode()
            {
                return (_id != null ? _id.GetHashCode() : 0);
            }

            public static bool operator ==(MacroCommand left, MacroCommand right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(MacroCommand left, MacroCommand right)
            {
                return !Equals(left, right);
            }
        }

        #endregion

        #region Temporary

        //TODO Find a better place for these methods

        /// <summary>
        /// Searches a <see cref="StackContext"/> for this entity and wraps it in a PValue, if found.
        /// </summary>
        /// <param name="sctx">The stack context to search.</param>
        /// <param name="entity">Holds the wrapped reference to this entity on success; undefined on failure.</param>
        /// <returns>True if the entity was found in the context; false otherwise</returns>
        internal abstract bool _TryLookup(StackContext sctx,out PValue  entity);

        #endregion

    }

    public interface IEntityRefMatcher<in TArg, out TResult>
    {
        TResult OnFunction(EntityRef.Function function, TArg argument);

        TResult OnCommand(EntityRef.Command command, TArg argument);

        TResult OnMacroCommand(EntityRef.MacroCommand macroCommand, TArg argument);

        TResult OnLocalVariable(EntityRef.Variable.Local variable, TArg argument);

        TResult OnGlobalVariable(EntityRef.Variable.Global variable, TArg argument);
    }

    public abstract class EntityRefMatcher<TArg, TResult> : IEntityRefMatcher<TArg, TResult>
    {
        #region IEntityRefMatcher implementation

        TResult IEntityRefMatcher<TArg, TResult>.OnFunction(EntityRef.Function function, TArg argument)
        {
            return OnFunction(function, argument);
        }

        TResult IEntityRefMatcher<TArg, TResult>.OnCommand(EntityRef.Command command, TArg argument)
        {
            return OnCommand(command, argument);
        }

        TResult IEntityRefMatcher<TArg, TResult>.OnMacroCommand(EntityRef.MacroCommand macroCommand, TArg argument)
        {
            return OnMacroCommand(macroCommand, argument);
        }

        TResult IEntityRefMatcher<TArg, TResult>.OnLocalVariable(EntityRef.Variable.Local variable, TArg argument)
        {
            return OnLocalVariable(variable, argument);
        }

        TResult IEntityRefMatcher<TArg, TResult>.OnGlobalVariable(EntityRef.Variable.Global variable, TArg argument)
        {
            return OnGlobalVariable(variable, argument);
        }

        #endregion

        protected abstract TResult OnNotMatched(EntityRef entity, [PublicAPI] TArg argument);

        [PublicAPI]
        public virtual TResult OnFunction(EntityRef.Function function, TArg argument)
        {
            return OnNotMatched(function, argument);
        }

        [PublicAPI]
        protected virtual TResult OnCommand(EntityRef.Command command, TArg argument)
        {
            return OnNotMatched(command, argument);
        }

        [PublicAPI]
        protected virtual TResult OnMacroCommand(EntityRef.MacroCommand macroCommand, TArg argument)
        {
            return OnNotMatched(macroCommand, argument);
        }

        [PublicAPI]
        protected virtual TResult OnLocalVariable(EntityRef.Variable.Local variable, TArg argument)
        {
            return OnNotMatched(variable, argument);
        }

        [PublicAPI]
        protected virtual TResult OnGlobalVariable(EntityRef.Variable.Global variable, TArg argument)
        {
            return OnNotMatched(variable, argument);
        }
    }
}