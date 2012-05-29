using System;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast
{
    public sealed class AstReferenceToEntity : AstExpr
    {
        private readonly EntityRef _entity;

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

        #region Overrides of AstNode

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of AstExpr

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}