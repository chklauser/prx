using System;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstReferenceToEntity : AstExpr
    {
        private readonly EntityRef _entity;

        [PublicAPI]
        public EntityRef Entity
        {
            get { return _entity; }
        }

        private AstReferenceToEntity(ISourcePosition position, EntityRef entity) : base(position)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            _entity = entity;
        }

        public static AstExpr Create(ISourcePosition position, EntityRef entity)
        {
            return new AstReferenceToEntity(position,entity);
        }

        #region Overrides of AstNode

        private class EmitReferenceMatcher : EntityRefMatcher<object,object>
        {
            private readonly CompilerTarget _target;
            private readonly AstReferenceToEntity _node;

            public EmitReferenceMatcher(CompilerTarget target, AstReferenceToEntity node)
            {
                _target = target;
                _node = node;
            }

            #region Overrides of EntityRefMatcher<CompilerTarget,object>

            protected override object OnNotMatched(EntityRef entity, object argument)
            {
                throw new NotSupportedException(string.Format("Cannot emit reference to a {0}.", entity));
            }

            public override object OnFunction(EntityRef.Function function, object argument)
            {
                _target.Emit(_node.Position, OpCode.ldr_func, function.Id, function.ModuleName);
                return null;
            }

            protected override object OnCommand(EntityRef.Command command, object argument)
            {
                _target.Emit(_node.Position, OpCode.ldr_cmd, command.Id);
                return null;
            }

            protected override object OnLocalVariable(EntityRef.Variable.Local variable, object argument)
            {
                _target.Emit(_node.Position, OpCode.ldr_loc, variable.Id);
                return null;
            }

            protected override object OnGlobalVariable(EntityRef.Variable.Global variable, object argument)
            {
                _target.Emit(_node.Position, OpCode.ldr_func, variable.Id, variable.ModuleName);
                return null;
            }

            #endregion
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if(stackSemantics != StackSemantics.Value)
                return;

            _entity.Match(new EmitReferenceMatcher(target, this), target);
        }

        #endregion

        #region Implementation of AstExpr

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;
            return false;
        }

        #endregion
    }
}