using System;
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstReference : AstExpr
    {
        private readonly EntityRef _entity;

        public AstReference(ISourcePosition position, [NotNull] EntityRef entity) : base(position)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            _entity = entity;
        }

        [NotNull]
        public EntityRef Entity
        {
            get { return _entity; }
        }

        #region Overrides of AstNode

        private class EmitLoadReferenceHandler : IEntityRefMatcher<Tuple<AstReference, CompilerTarget>, object>
        {
            #region Implementation of IEntityRefMatcher<in Tuple<AstReference,CompilerTarget>,out object>

            public object OnFunction(EntityRef.Function function, Tuple<AstReference, CompilerTarget> argument)
            {
                argument.Item2.Emit(
                    argument.Item1.Position, OpCode.ldr_func, function.Id, function.ModuleName);
                return null;
            }

            public object OnCommand(EntityRef.Command command, Tuple<AstReference, CompilerTarget> argument)
            {
                argument.Item2.Emit(
                    argument.Item1.Position, OpCode.ldr_cmd, command.Id);
                return null;
            }

            public object OnMacroCommand(EntityRef.MacroCommand macroCommand, Tuple<AstReference, CompilerTarget> argument)
            {
                // Currently illegal.
                //  => Emit ldc.null instead
                //  => Report error
                argument.Item2.EmitNull(argument.Item1.Position);
                argument.Item2.Loader.ReportMessage(_macroCommandErrorMessage(argument.Item1.Position));
                return null;
            }

            public object OnLocalVariable(EntityRef.Variable.Local variable, Tuple<AstReference, CompilerTarget> argument)
            {
                argument.Item2.Emit(
                                    argument.Item1.Position, OpCode.ldr_loc, variable.Id);
                return null;
            }

            public object OnGlobalVariable(EntityRef.Variable.Global variable, Tuple<AstReference, CompilerTarget> argument)
            {
                argument.Item2.Emit(
                    argument.Item1.Position, OpCode.ldr_glob, variable.Id, variable.ModuleName);
                return null;
            }

            #endregion
        }

        private static readonly EmitLoadReferenceHandler _emitLoadReference =
            new EmitLoadReferenceHandler();

        protected override void DoEmitCode(CompilerTarget target, StackSemantics semantics)
        {
            switch (semantics)
            {
                case StackSemantics.Value:
                    Entity.Match(_emitLoadReference, Tuple.Create(this, target));
                    break;
                case StackSemantics.Effect:
                    // Even though no code would be generated, we still want to catch
                    // references to macro commands.
                    EntityRef.MacroCommand mcmd;
                    if(Entity.TryGetMacroCommand(out mcmd))
                    {
                        target.Loader.ReportMessage(_macroCommandErrorMessage(Position));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("semantics");
            }
        }

        #endregion

        #region Overrides of AstExpr

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;
            return false;
        }

        public override string ToString()
        {
            return String.Format("->{0}", Entity);
        }

        #endregion

        [NotNull]
        private static Message _macroCommandErrorMessage([NotNull] ISourcePosition position)
        {
            return Message.Error(
                Resources.AstReference_MacroCommandReferenceNotPossible, position,
                MessageClasses.CannotCreateReference);
        }
    }
}