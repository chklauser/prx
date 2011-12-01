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

using Prexonite.Commands;
using Prexonite.Compiler;

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
    ///         <description>Id, ModuleName</description>
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
    ///         <description>Id, ModuleName</description>
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

        public abstract T Match<T>(IEntityRefMatcher<T> matcher);

        public virtual bool TryGetFunction(out Function func, bool? isRunTime = null,
            bool? isCompileTime = null, bool? isMacro = null)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetRunTimeFunction(out Function.RunTime func)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetMacroFunction(out Function.Macro func)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetCompileTimeFunction(out Function.CompileTime func)
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

        #region Referencing and Dereferencing

        public virtual bool TryGetSymbolicReference(out EntityRef entity)
        {
            entity = null;
            return false;
        }

        public virtual bool TrySymbolicDereference(out EntityRef entity)
        {
            entity = null;
            return false;
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

        #region Functions

        public abstract class Function : EntityRef
        {
            private readonly string _id;
            private readonly ModuleName _moduleName;

            private Function(string id, ModuleName moduleName)
            {
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

            protected abstract bool Satisfies(bool? isRunTime = null, bool? isCompileTime = null,
                bool? isMacro = null);

            public override bool TryGetFunction(out Function func, bool? isRunTime = null,
                bool? isCompileTime = null, bool? isMacro = null)
            {
                if (!Satisfies(isRunTime, isCompileTime, isMacro))
                {
                    func = null;
                    return false;
                }
                else
                {
                    func = this;
                    return true;
                }
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

            #region Nested type: CompileTime

            public class CompileTime : CompileTimeBase
            {
                private CompileTime(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

                public static CompileTime Create(string id, ModuleName moduleName)
                {
                    return new CompileTime(id, moduleName);
                }

                protected override bool SatisfiesMacro(bool isMacro)
                {
                    return !isMacro;
                }

                public override T Match<T>(IEntityRefMatcher<T> matcher)
                {
                    return matcher.OnCompileTimeFunction(this);
                }

                public override bool TryGetCompileTimeFunction(out CompileTime func)
                {
                    func = this;
                    return true;
                }
            }

            #endregion

            #region Nested type: CompileTimeBase

            public abstract class CompileTimeBase : Function, ICompileTime
            {
                protected CompileTimeBase(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

                #region ICompileTime Members

                public bool TryGetEntity(Loader ldr, out PValue entity)
                {
                    PFunction func;
                    if (TryGetFunction(ldr.ParentApplication, out func))
                    {
                        entity = ldr.CreateNativePValue(func);
                        return true;
                    }
                    else
                    {
                        entity = null;
                        return false;
                    }
                }

                #endregion

                protected override bool Satisfies(bool? isRunTime = null, bool? isCompileTime = null,
                    bool? isMacro = null)
                {
                    if (isRunTime.HasValue && isRunTime.Value ||
                        isCompileTime.HasValue && !isCompileTime.Value)
                        return false;
                    if (!isMacro.HasValue)
                        return true;
                    return SatisfiesMacro(isMacro.Value);
                }

                protected abstract bool SatisfiesMacro(bool isMacro);
            }

            #endregion

            #region Nested type: Macro

            public sealed class Macro : CompileTimeBase, IMacro
            {
                private Macro(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

                public static Macro Create(string id, ModuleName moduleName)
                {
                    return new Macro(id, moduleName);
                }

                protected override bool SatisfiesMacro(bool isMacro)
                {
                    return isMacro;
                }

                public override T Match<T>(IEntityRefMatcher<T> matcher)
                {
                    return matcher.OnMacroFunction(this);
                }

                public override bool TryGetMacroFunction(out Macro func)
                {
                    func = this;
                    return true;
                }
            }

            #endregion

            #region Nested type: Runtime

            public class RunTime : Function, IRunTime
            {
                private RunTime(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

                #region IRunTime Members

                public bool TryGetEntity(StackContext sctx, out PValue entity)
                {
                    PFunction func;
                    if (TryGetFunction(sctx.ParentApplication, out func))
                    {
                        entity = sctx.CreateNativePValue(func);
                        return true;
                    }
                    else
                    {
                        entity = null;
                        return false;
                    }
                }

                #endregion

                public override T Match<T>(IEntityRefMatcher<T> matcher)
                {
                    return matcher.OnRunTimeFunction(this);
                }

                public override bool TryGetRunTimeFunction(out RunTime func)
                {
                    func = this;
                    return true;
                }

                public static RunTime Create(string id, ModuleName moduleName)
                {
                    return new RunTime(id, moduleName);
                }

                protected override bool Satisfies(bool? isRunTime = null, bool? isCompileTime = null,
                    bool? isMacro = null)
                {
                    return (!isRunTime.HasValue || isRunTime.Value)
                        && (!isCompileTime.HasValue || !isCompileTime.Value)
                            && (!isMacro.HasValue || !isMacro.Value);
                }
            }

            #endregion
        }

        #endregion

        #region Commands

        public sealed class Command : EntityRef, IRunTime
        {
            private readonly string _id;

            private Command(string id)
            {
                _id = id;
            }

            #region IRunTime Members

            public bool TryGetEntity(StackContext sctx, out PValue entity)
            {
                PCommand cmd;
                if (sctx.ParentEngine.Commands.TryGetValue(_id, out cmd))
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

            public override T Match<T>(IEntityRefMatcher<T> matcher)
            {
                return matcher.OnCommand(this);
            }

            public override bool TryGetCommand(out Command cmd)
            {
                cmd = this;
                return true;
            }
        }

        #endregion

        #region Variables

        public abstract class Variable : EntityRef, IRunTime
        {
            private readonly bool _isReference;

            private Variable(bool isReference)
            {
                _isReference = isReference;
            }

            public bool IsReference
            {
                get { return _isReference; }
            }

            #region IRunTime Members

            public abstract bool TryGetEntity(StackContext sctx, out PValue entity);

            #endregion

            protected abstract Variable With(bool isReference);

            public override bool TryGetSymbolicReference(out EntityRef entity)
            {
                if (IsReference)
                {
                    entity = With(false);
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            public override bool TrySymbolicDereference(out EntityRef entity)
            {
                if (!IsReference)
                {
                    entity = With(true);
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            public override bool TryGetVariable(out Variable variable)
            {
                variable = this;
                return true;
            }

            #region Nested type: Global

            public sealed class Global : Variable
            {
                private readonly string _id;
                private readonly ModuleName _moduleName;

                private Global(string id, ModuleName moduleName, bool isReference)
                    : base(isReference)
                {
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

                public override T Match<T>(IEntityRefMatcher<T> matcher)
                {
                    return matcher.OnGlobalVariable(this);
                }

                public override bool TryGetGlobalVariable(out Global variable)
                {
                    variable = this;
                    return true;
                }

                public static Global Create(string id, ModuleName moduleName,
                    bool isReference = false)
                {
                    return new Global(id, moduleName, isReference);
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

                protected override Variable With(bool isReference)
                {
                    return Create(Id, ModuleName, isReference);
                }
            }

            #endregion

            #region Nested type: Local

            public sealed class Local : Variable
            {
                private readonly string _id;

                private Local(string id, bool isReference)
                    : base(isReference)
                {
                    _id = id;
                }

                public string Id
                {
                    get { return _id; }
                }

                public static Local Create(string id, bool isReference = false)
                {
                    return new Local(id, isReference);
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

                protected override Variable With(bool isReference)
                {
                    return Create(Id, isReference);
                }

                public override T Match<T>(IEntityRefMatcher<T> matcher)
                {
                    return matcher.OnLocalVariable(this);
                }

                public override bool TryGetLocalVariable(out Local variable)
                {
                    variable = this;
                    return true;
                }
            }

            #endregion
        }

        #endregion

        #region MacroCommands

        public class MacroCommand : EntityRef, ICompileTime, IMacro
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

            public override T Match<T>(IEntityRefMatcher<T> matcher)
            {
                return matcher.OnMacroCommand(this);
            }

            public override bool TryGetMacroCommand(out MacroCommand mcmd)
            {
                mcmd = this;
                return true;
            }
        }

        #endregion
    }

    public interface IEntityRefMatcher<out T>
    {
        T OnRunTimeFunction(EntityRef.Function.RunTime function);
        T OnCompileTimeFunction(EntityRef.Function.CompileTime function);
        T OnMacroFunction(EntityRef.Function.Macro function);

        T OnCommand(EntityRef.Command command);

        T OnMacroCommand(EntityRef.MacroCommand macroCommand);

        T OnLocalVariable(EntityRef.Variable.Local variable);
        T OnGlobalVariable(EntityRef.Variable.Global variable);
    }

    public abstract class EntityRefMatcher<T> : IEntityRefMatcher<T>
    {
        #region IEntityRefMatcher implementation

        T IEntityRefMatcher<T>.OnRunTimeFunction(EntityRef.Function.RunTime function)
        {
            return OnRunTimeFunction(function);
        }

        T IEntityRefMatcher<T>.OnCompileTimeFunction(EntityRef.Function.CompileTime function)
        {
            return OnCompileTimeFunction(function);
        }

        T IEntityRefMatcher<T>.OnMacroFunction(EntityRef.Function.Macro function)
        {
            return OnMacroFunction(function);
        }

        T IEntityRefMatcher<T>.OnCommand(EntityRef.Command command)
        {
            return OnCommand(command);
        }

        T IEntityRefMatcher<T>.OnMacroCommand(EntityRef.MacroCommand macroCommand)
        {
            return OnMacroCommand(macroCommand);
        }

        T IEntityRefMatcher<T>.OnLocalVariable(EntityRef.Variable.Local variable)
        {
            return OnLocalVariable(variable);
        }

        T IEntityRefMatcher<T>.OnGlobalVariable(EntityRef.Variable.Global variable)
        {
            return OnGlobalVariable(variable);
        }

        #endregion

        protected abstract T OnNotMatched(EntityRef entity);

        protected virtual T OnRunTimeFunction(EntityRef.Function.RunTime function)
        {
            return OnNotMatched(function);
        }

        protected virtual T OnCompileTimeFunction(EntityRef.Function.CompileTime function)
        {
            return OnNotMatched(function);
        }

        protected virtual T OnMacroFunction(EntityRef.Function.Macro function)
        {
            return OnNotMatched(function);
        }

        protected virtual T OnCommand(EntityRef.Command command)
        {
            return OnNotMatched(command);
        }

        protected virtual T OnMacroCommand(EntityRef.MacroCommand macroCommand)
        {
            return OnNotMatched(macroCommand);
        }

        protected virtual T OnLocalVariable(EntityRef.Variable.Local variable)
        {
            return OnNotMatched(variable);
        }

        protected virtual T OnGlobalVariable(EntityRef.Variable.Global variable)
        {
            return OnNotMatched(variable);
        }
    }
}