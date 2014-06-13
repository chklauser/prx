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
                var refNode = argument.Item1;
                var target = argument.Item2;
                target.Emit(refNode.Position, OpCode.ldr_func, function.Id, function.ModuleName);
                return null;
            }

            public object OnCommand(EntityRef.Command command, Tuple<AstReference, CompilerTarget> argument)
            {
                var target = argument.Item2;
                var refNode = argument.Item1;
                target.Emit(refNode.Position, OpCode.ldr_cmd, command.Id);
                return null;
            }

            public object OnMacroCommand(EntityRef.MacroCommand macroCommand, Tuple<AstReference, CompilerTarget> argument)
            {
                // Currently illegal.
                //  => Emit ldc.null instead
                //  => Report error
                var refNode = argument.Item1;
                argument.Item2.EmitNull(refNode.Position);
                argument.Item2.Loader.ReportMessage(_macroCommandErrorMessage(refNode.Position));
                return null;
            }

            public object OnLocalVariable(EntityRef.Variable.Local variable, Tuple<AstReference, CompilerTarget> argument)
            {
                argument.Item2.Emit(argument.Item1.Position, OpCode.ldr_loc, variable.Id);
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