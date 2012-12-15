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
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
#pragma warning disable 628 //ignore warnings about proteced members in sealed classes for now

    [Obsolete("Will be removed in favor of IndirectCall(Reference(*))")]
    public sealed class AstGetSetEntity : AstGetSet, ICanBeReferenced, IAstPartiallyApplicable
    {
        private readonly EntityRef _entity;

        public EntityRef Entity
        {
            get { return _entity; }
        }

        public static AstGetSet Create(ISourcePosition position, EntityRef entity)
        {
            return Create(position, PCall.Get, entity);
        }

        public static AstGetSet Create(ISourcePosition position, PCall call, EntityRef entity)
        {
            
            return new AstGetSetEntity(position,call,entity);
        }

        private AstGetSetEntity(ISourcePosition position, PCall call, EntityRef entity) : base(position, call)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            
            _entity = entity;
        }

        private class CodeGenInfo
        {
            public CompilerTarget Target { get; set; }
            public StackSemantics StackSemantics { get; set; }
            public AstGetSetEntity Node { get; set; }

            public bool JustEffect
            {
                get { return StackSemantics == StackSemantics.Effect; }
            }
        }

        private class EmitGetCodeHandler : EntityRefMatcher<CodeGenInfo,object>
        {
            #region Overrides of EntityRefMatcher<CodeGenInfo,object>

            protected override object OnNotMatched(EntityRef entity, CodeGenInfo argument)
            {
                throw new NotSupportedException(string.Format(
                    "AstGetSetEntity cannot be used to generate code for {0}.", entity));
            }

            #endregion

            protected override object OnCommand(EntityRef.Command command, CodeGenInfo argument)
            {
                argument.Target.EmitCommandCall(argument.Node.Position, argument.Node.Arguments.Count, command.Id,
                                                argument.JustEffect);
                return null;
            }

            public override object OnFunction(EntityRef.Function function, CodeGenInfo argument)
            {
                argument.Target.EmitFunctionCall(argument.Node.Position, argument.Node.Arguments.Count, function.Id,
                                                 function.ModuleName, argument.JustEffect);
                return null;
            }

            protected override object OnGlobalVariable(EntityRef.Variable.Global variable, CodeGenInfo argument)
            {
                if(!argument.JustEffect)
                    argument.Target.EmitLoadGlobal(argument.Node.Position,variable.Id,variable.ModuleName);
                return null;
            }

            protected override object OnLocalVariable(EntityRef.Variable.Local variable, CodeGenInfo argument)
            {
                if(!argument.JustEffect)
                    argument.Target.EmitLoadLocal(argument.Node.Position,variable.Id);
                return null;
            }
        }

        private class EmitSetCodeHandler : EntityRefMatcher<CodeGenInfo,object>
        {
            #region Overrides of EntityRefMatcher<CodeGenInfo,object>

            protected override object OnNotMatched(EntityRef entity, CodeGenInfo argument)
            {
                throw new NotSupportedException(string.Format(
                    "AstGetSetEntity cannot be used to generate code for assigning to {0}.", entity));
            }

            #endregion

            // set calls are always done just for the effect. The "return" value of a set call is the RHS 
            //  and that is already handled by AstGetSet.
            private const bool JustEffect = true;

            protected override object OnCommand(EntityRef.Command command, CodeGenInfo argument)
            {
                argument.Target.EmitCommandCall(argument.Node.Position, argument.Node.Arguments.Count, command.Id,
                                                JustEffect);
                return null;
            }

            public override object OnFunction(EntityRef.Function function, CodeGenInfo argument)
            {
                argument.Target.EmitFunctionCall(argument.Node.Position, argument.Node.Arguments.Count, function.Id,
                                                 function.ModuleName, JustEffect);
                return null;
            }

            protected override object OnGlobalVariable(EntityRef.Variable.Global variable, CodeGenInfo argument)
            {
                argument.Target.EmitStoreGlobal(argument.Node.Position,variable.Id,variable.ModuleName);
                return null;
            }

            protected override object OnLocalVariable(EntityRef.Variable.Local variable, CodeGenInfo argument)
            {
                if(!argument.JustEffect)
                    argument.Target.EmitStoreLocal(argument.Node.Position,variable.Id);
                return null;
            }
        }

        private static readonly EmitGetCodeHandler _emitGetCode = new EmitGetCodeHandler();
        private static readonly EmitSetCodeHandler _emitSetCode = new EmitSetCodeHandler();

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            _entity.Match(_emitGetCode, new CodeGenInfo {Target = target, StackSemantics = stackSemantics, Node = this});
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            _entity.Match(_emitSetCode, new CodeGenInfo {Target = target, Node = this});
        }

        public override AstGetSet GetCopy()
        {
            var e = Create(Position, Call, Entity);
            CopyBaseMembers(e);
            return e;
        }

        ICollection<AstExpr> ICanBeReferenced.Arguments
        {
            get { return Arguments; }
        }

        public bool TryToReference(out AstExpr reference)
        {
            reference = AstReferenceToEntity.Create(Position, Entity);
            return true;
        }

        #region Implementation of IAstPartiallyApplicable

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            AstExpr refNode;
            if (!TryToReference(out refNode))
                throw new PrexoniteException("Cannot partially apply " + this +
                                             " because it can't be converted to a reference.");

            var indTemplate = new AstIndirectCall(Position, Call, refNode);
            indTemplate.Arguments.AddRange(Arguments);
            Debug.Assert(indTemplate.CheckForPlaceholders());
            indTemplate.EmitValueCode(target);
        }

        #endregion
    }
    #pragma warning restore 628
}