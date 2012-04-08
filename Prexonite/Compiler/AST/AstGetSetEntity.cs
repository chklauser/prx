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
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
#pragma warning disable 628 //ignore warnings about proteced members in sealed classes for now
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

    public sealed class AstGetSetEntity : AstGetSet, ICanBeReferenced
    {
        private readonly EntityRef _entity;

        public EntityRef Entity
        {
            get { return _entity; }
        }

        public static AstGetSetEntity Create(ISourcePosition position, EntityRef entity)
        {
            return Create(position, PCall.Get, entity);
        }

        public static AstGetSetEntity Create(ISourcePosition position, PCall call, EntityRef entity)
        {
            return new AstGetSetEntity(position,call,entity);
        }

        private AstGetSetEntity(ISourcePosition position, PCall call, EntityRef entity) : base(position, call)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            
            _entity = entity;
        }

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            throw new System.NotImplementedException();
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            throw new System.NotImplementedException();
        }

        public override AstGetSet GetCopy()
        {
            return Create(Position, Call, Entity);
        }

        ICollection<AstExpr> ICanBeReferenced.Arguments
        {
            get { return Arguments; }
        }

        public bool TryToReference(out AstExpr reference)
        {
            reference = null;
            return false;
        }
    }
    #pragma warning restore 628
}