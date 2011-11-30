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

        public virtual bool TryGetFunction(out Function func, bool? isRunTime = null, bool? isCompileTime = null, bool? isMacro = null)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetRunTimeFunction(out Function.Runtime func)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetMacroFunction(out Function.Macro func)
        {
            func = null;
            return false;
        }

        public virtual bool TryGetCompileTimeFunction(out  Function.CompileTime func)
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

        #region Classification

        public interface IRunTime
        {
            bool TryGetEntity(StackContext sctx, out PValue entity);
        }

        public interface ICompileTime
        {
            bool TryGetEntity(Loader ldr, out PValue entity);
        }

        public interface IMacro : ICompileTime
        {
            
        }

        #endregion

        #region Functions

        public abstract class Function : EntityRef
        {
            private readonly string _id;
            private readonly ModuleName _moduleName;

            protected abstract bool Satisfies(bool? isRunTime = null, bool? isCompileTime = null,
                bool? isMacro = null);

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

            public override bool TryGetFunction(out Function func, bool? isRunTime = null, bool? isCompileTime = null, bool? isMacro = null)
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

            public abstract class CompileTimeBase : Function, ICompileTime
            {
                protected CompileTimeBase(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

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

                protected override bool Satisfies(bool? isRunTime = null, bool? isCompileTime = null, bool? isMacro = null)
                {
                    if (isRunTime.HasValue && isRunTime.Value || isCompileTime.HasValue && !isCompileTime.Value)
                        return false;
                    if (!isMacro.HasValue)
                        return true;
                    return SatisfiesMacro(isMacro.Value);
                }

                protected abstract bool SatisfiesMacro(bool isMacro);
            }

            public sealed class Macro : CompileTimeBase, IMacro
            {
                public static Macro Create(string id, ModuleName moduleName)
                {
                    return new Macro(id, moduleName);
                }

                private Macro(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

                protected override bool SatisfiesMacro(bool isMacro)
                {
                    return isMacro;
                }

                public override bool TryGetMacroFunction(out Macro func)
                {
                    func = this;
                    return true;
                }
            }

            public class CompileTime : CompileTimeBase
            {
                public static CompileTime Create(string id, ModuleName moduleName)
                {
                    return new CompileTime(id, moduleName);
                }

                private CompileTime(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

                protected override bool SatisfiesMacro(bool isMacro)
                {
                    return !isMacro;
                }

                public override bool TryGetCompileTimeFunction(out CompileTime func)
                {
                    func = this;
                    return true;
                }
            }

            public class Runtime : Function, IRunTime
            {
                public override bool TryGetRunTimeFunction(out Runtime func)
                {
                    func = this;
                    return true;
                }

                public static Runtime Create(string id, ModuleName moduleName)
                {
                    return new Runtime(id, moduleName);
                }

                private Runtime(string id, ModuleName moduleName)
                    : base(id, moduleName)
                {
                }

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

                protected override bool Satisfies(bool? isRunTime = null, bool? isCompileTime = null, bool? isMacro = null)
                {
                    return (!isRunTime.HasValue || isRunTime.Value)
                        && (!isCompileTime.HasValue || !isCompileTime.Value)
                            && (!isMacro.HasValue || !isMacro.Value);
                }
            }
        }

        #endregion

        #region Commands

        public sealed class Command : EntityRef, IRunTime
        {
            public static Command Create(string id)
            {
                return new Command(id);
            }

            private readonly string _id;

            private Command(string id)
            {
                _id = id;
            }

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
            public abstract bool TryGetEntity(StackContext sctx, out PValue entity);

            private Variable(bool isReference)
            {
                _isReference = isReference;
            }

            public bool IsReference
            {
                get { return _isReference; }
            }

            public override bool TryGetVariable(out Variable variable)
            {
                variable = this;
                return true;
            }

            public sealed class Local : Variable
            {
                public static Local Create(string id, bool isReference = false)
                {
                    return new Local(id, isReference);
                }

                private readonly string _id;

                private Local(string id, bool isReference)
                    : base(isReference)
                {
                    _id = id;
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

                public string Id
                {
                    get { return _id; }
                }

                public override bool TryGetLocalVariable(out Local variable)
                {
                    variable = this;
                    return true;
                }
            }

            public sealed class Global : Variable
            {
                public override bool TryGetGlobalVariable(out Global variable)
                {
                    variable = this;
                    return true;
                }

                public static Global Create(string id, ModuleName moduleName, bool isReference = false)
                {
                    return new Global(id, moduleName, isReference);
                }

                private readonly string _id;
                private readonly ModuleName _moduleName;

                private Global(string id, ModuleName moduleName, bool isReference)
                    : base(isReference)
                {
                    _id = id;
                    _moduleName = moduleName;
                }

                public override bool TryGetEntity(StackContext sctx, out PValue entity)
                {
                    Application application;
                    PVariable v;
                    if (sctx.ParentApplication.Compound.TryGetApplication(_moduleName, out application)
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
            }
        }

        #endregion

        #region MacroCommands

        public class MacroCommand : EntityRef, ICompileTime, IMacro
        {
            public static MacroCommand Create(string id)
            {
                return new MacroCommand(id);
            }

            private readonly string _id;

            public string Id
            {
                get { return _id; }
            }

            private MacroCommand(string id)
            {
                _id = id;
            }

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

            public override bool TryGetMacroCommand(out MacroCommand mcmd)
            {
                mcmd = this;
                return true;
            }
        }

        #endregion
    }
}