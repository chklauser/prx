// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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

using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Compiler;

namespace Prexonite.Modular;

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

    public virtual bool TryGetFunction([NotNullWhen(true)] out Function? func)
    {
        func = null;
        return false;
    }

    public virtual bool TryGetMacroCommand([NotNullWhen(true)] out MacroCommand? mcmd)
    {
        mcmd = null;
        return false;
    }

    public virtual bool TryGetCommand([NotNullWhen(true)] out Command? cmd)
    {
        cmd = null;
        return false;
    }

    public virtual bool TryGetVariable([NotNullWhen(true)] out Variable? variable)
    {
        variable = null;
        return false;
    }

    public virtual bool TryGetLocalVariable([NotNullWhen(true)] out Variable.Local? variable)
    {
        variable = null;
        return false;
    }

    public virtual bool TryGetGlobalVariable([NotNullWhen(true)] out Variable.Global? variable)
    {
        variable = null;
        return false;
    }

    #endregion

    #region Conversion

    /// <summary>
    /// Creates a <see cref="SymbolEntry"/> that refers to the same entity as the <see cref="EntityRef"/>. This is a narrowing (lossy) conversion.
    /// </summary>
    /// <returns>A <see cref="SymbolEntry"/> that refers to the same entity as the <see cref="EntityRef"/>.</returns>
    public abstract SymbolEntry ToSymbolEntry();

    public static explicit operator SymbolEntry(EntityRef entityRef)
    {
        if (entityRef == null)
            throw new ArgumentNullException(nameof(entityRef));
        return entityRef.ToSymbolEntry();
    }

    #endregion

    #region Classification

    #region Nested type: ICompileTime

    public interface ICompileTime
    {
        bool TryGetEntity(Loader ldr, [NotNullWhen(true)] out PValue? entity);
    }

    #endregion

    #region Nested type: IMacro

    public interface IMacro : ICompileTime;

    #endregion

    #region Nested type: IRunTime

    public interface IRunTime
    {
        bool TryGetEntity(StackContext sctx, [NotNullWhen(true)] out PValue? entity);
    }

    #endregion

    #endregion

    #region ToString

    /// <summary>
    /// Writes a human-readable representation of this <see cref="EntityRef"/> to the supplied <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="writer">The writer to write to. Must not be null.</param>
    [PublicAPI]
    public abstract void ToString(TextWriter writer);

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
        bool IEquatable<Function>.Equals(Function? other)
        {
            return EqualsFunction(other);
        }

        public override bool TryGetFunction(out Function func)
        {
            func = this;
            return true;
        }

        protected bool EqualsFunction(Function? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Id, Id) && Equals(other.ModuleName, ModuleName);
        }

        public override bool Equals(object? obj)
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
                return (Id.GetHashCode()*397) ^ ModuleName.GetHashCode();
            }
        }

        public bool TryGetEntity(StackContext sctx, out PValue entity)
        {
            if(sctx.ParentApplication.TryGetFunction(Id,ModuleName, out var func))
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

        public static bool operator ==(Function? left, Function? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Function? left, Function? right)
        {
            return !Equals(left, right);
        }

        Function(string id, ModuleName moduleName)
        {
            if(moduleName == null)
                throw new ArgumentNullException(nameof(moduleName));

            Id = id ?? throw new ArgumentNullException(nameof(id));
            ModuleName = moduleName;
        }

        public string Id { get; }

        public ModuleName ModuleName { get; }

        public bool TryGetFunction(Application application, out PFunction? func)
        {
            if (application.Compound.TryGetApplication(ModuleName, out var declaringApp)
                && declaringApp.Functions.TryGetValue(Id, out func))
            {
                return true;
            }
            else
            {
                func = null;
                return false;
            }
        }

        public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
        {
            return matcher.OnFunction(this,argument);
        }

        public override SymbolEntry ToSymbolEntry()
        {
            return new(SymbolInterpretations.Function, Id, ModuleName);
        }

        public override void ToString(TextWriter writer)
        {
            writer.Write("func ");
            writer.Write(Id);
            writer.Write("/");
            writer.Write(ModuleName);
        }

        internal override bool _TryLookup(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
        {
            var app = sctx.ParentApplication;
            if (!app.Compound.TryGetApplication(ModuleName, out app))
            {
                entity = null;
                return false;
            }

            if(!app.Functions.TryGetValue(Id,out var func))
            {
                entity = null;
                return false;
            }

            entity = sctx.CreateNativePValue(func);
            return true;
        }

        public static Function Create(string internalId, ModuleName moduleName)
        {
            Debug.Assert(moduleName != null, $"Module name is null for entity ref to function {internalId}.");
            return new(internalId, moduleName);
        }
    }

    #endregion

    #region Commands

    [DebuggerDisplay("command {Id}")]
    public sealed class Command : EntityRef, IRunTime, IEquatable<Command>
    {
        public bool Equals(Command? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Id, Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Command)) return false;
            return Equals((Command) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Command? left, Command? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Command? left, Command? right)
        {
            return !Equals(left, right);
        }

        Command(string id)
        {
            Id = id;
        }

        public string Id { get; }

        #region IRunTime Members

        public bool TryGetEntity(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
        {
            if (sctx.ParentEngine.Commands.TryGetValue(Id, out var cmd))
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
            return new(id);
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
            return new(SymbolInterpretations.Command, Id, null);
        }

        public override void ToString(TextWriter writer)
        {
            writer.Write("cmd ");
            writer.Write(Id);
        }

        internal override bool _TryLookup(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
        {
            if(sctx.ParentEngine.Commands.TryGetValue(Id,out var command))
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
        Variable()
        {
        }

        #region IRunTime Members

        public abstract bool TryGetEntity(StackContext sctx, [NotNullWhen(true)] out PValue? entity);

        #endregion

        public override bool TryGetVariable(out Variable variable)
        {
            variable = this;
            return true;
        }


        protected abstract bool EqualsVariable(Variable other);
        bool IEquatable<Variable>.Equals(Variable? other)
        {
            return other != null && EqualsVariable(other);
        }

        #region Nested type: Global

        [DebuggerDisplay("global var {Id}/{ModuleName}")]
        public sealed class Global : Variable, IEquatable<Global>
        {
            Global(string id, ModuleName moduleName)
            {
                if (moduleName == null)
                    throw new ArgumentNullException(nameof(moduleName));

                Id = id ?? throw new ArgumentNullException(nameof(id));
                ModuleName = moduleName;
            }

            public string Id { get; }

            public ModuleName ModuleName { get; }

            public override TResult Match<TArg, TResult>(IEntityRefMatcher<TArg, TResult> matcher, TArg argument)
            {
                return matcher.OnGlobalVariable(this,argument);
            }

            public override bool TryGetGlobalVariable(out Global variable)
            {
                variable = this;
                return true;
            }

            public static Global Create(string id, ModuleName moduleName)
            {
                Debug.Assert(moduleName != null, $"Module name is null for entity ref to global variable {id}.");
                return new(id, moduleName);
            }

            public override bool TryGetEntity(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
            {
                if (sctx.ParentApplication.Compound.TryGetApplication(ModuleName,
                        out var application)
                    && application.Variables.TryGetValue(Id, out var v))
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
                return new(SymbolInterpretations.GlobalObjectVariable, Id,ModuleName);
            }

            public override void ToString(TextWriter writer)
            {
                writer.Write("gvar ");
                writer.Write(Id);
                writer.Write("/");
                writer.Write(ModuleName);
            }

            internal override bool _TryLookup(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
            {
                var app = sctx.ParentApplication;
                if (!app.Compound.TryGetApplication(ModuleName, out app))
                {
                    entity = null;
                    return false;
                }

                if (!app.Variables.TryGetValue(Id, out var pvar))
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

            public bool Equals(Global? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                    
                return Equals(other.Id, Id) && Equals(other.ModuleName, ModuleName);
            }

            public override bool Equals(object? obj)
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
                    return (Id.GetHashCode()*397) ^ ModuleName.GetHashCode();
                }
            }

            public static bool operator ==(Global? left, Global? right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Global? left, Global? right)
            {
                return !Equals(left, right);
            }
        }

        #endregion

        #region Nested type: Local

        [DebuggerDisplay("local var {Id}")]
        public sealed class Local : Variable, IEquatable<Local>
        {
            Local(string id, int? index = null)
            {
                Id = id;
                Index = index;
            }

            public string Id { get; }

            public int? Index { get; }

            public bool Equals(Local? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Id, Id);
            }

            protected override bool EqualsVariable(Variable other)
            {
                var local = other as Local;
                return local != null && Equals(local);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Local)) return false;
                return Equals((Local) obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(Local? left, Local? right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Local? left, Local? right)
            {
                return !Equals(left, right);
            }

            public static Local Create(string id)
            {
                return new(id);
            }

            public Local WithIndex(int index)
            {
                return new(Id, index);
            }

            public override bool TryGetEntity(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
            {
                if (sctx is FunctionContext fctx && fctx.LocalVariables.TryGetValue(Id, out var v))
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
                return new(SymbolInterpretations.LocalObjectVariable, Id, null);
            }

            public override void ToString(TextWriter writer)
            {
                writer.Write("var ");
                writer.Write(Id);
            }

            internal override bool _TryLookup(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
            {
                if(sctx is not FunctionContext fctx)
                {
                    entity = null;
                    return false;
                }

                if(fctx.LocalVariables.TryGetValue(Id, out var pvar))
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
        MacroCommand(string id)
        {
            Id = id;
        }

        public string Id { get; }

        #region ICompileTime Members

        public bool TryGetEntity(Loader ldr, [NotNullWhen(true)] out PValue? entity)
        {
            if (ldr.MacroCommands.TryGetValue(Id, out var mcmd))
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
            return new(id);
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
            return new(SymbolInterpretations.MacroCommand, Id, null);
        }

        public override void ToString(TextWriter writer)
        {
            writer.Write("mcmd ");
            writer.Write(Id);
        }

        internal override bool _TryLookup(StackContext sctx, [NotNullWhen(true)] out PValue? entity)
        {
            //first: lookup in sctx (if it is a loader)
            if (sctx is Loader ldr)
                return _tryMcmdFromLoader(sctx, ldr, out entity);

            //else: search stack beginning at sctx
            if (sctx.ParentEngine.Stack.FindLast(sctx) is { } matchingFrame &&
                _tryMcmdFromStack(sctx, matchingFrame, out entity)) 
                return entity != null;

            //finally: search stack from bottom
            entity = null;
            return sctx.ParentEngine.Stack.Last is { } lastFrame &&
                _tryMcmdFromStack(sctx, lastFrame, out entity) && entity != null;
        }

        bool _tryMcmdFromStack(
            StackContext sctx,
            LinkedListNode<StackContext>? node,
            out PValue? entity
        )
        {
            Loader? ldr;
            while (node != null)
            {
                ldr = node.Value as Loader;
                if (ldr != null)
                {
                    entity = null;
                    _tryMcmdFromLoader(sctx, ldr, out entity);
                    return true;
                }
                node = node.Previous;
            }

            entity = null;
            return false;
        }

        bool _tryMcmdFromLoader(StackContext sctx, Loader ldr, out PValue? entity)
        {
            if (ldr.MacroCommands.TryGetValue(Id, out var mcmd))
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

        public bool Equals(MacroCommand? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Id, Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MacroCommand)) return false;
            return Equals((MacroCommand) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(MacroCommand? left, MacroCommand? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MacroCommand? left, MacroCommand? right)
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
    internal abstract bool _TryLookup(StackContext sctx, [NotNullWhen(true)] out PValue?  entity);

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