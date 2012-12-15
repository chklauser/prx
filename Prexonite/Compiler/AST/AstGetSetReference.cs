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
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetReference : AstGetSetSymbol, ICanBeReferenced
    {
        public AstGetSetReference(string file, int line, int column, PCall call, SymbolEntry implementation)
            : base(file, line, column, call, implementation)
        {
        }

        public AstGetSetReference(string file, int line, int column, SymbolEntry implementation)
            : base(file, line, column, PCall.Get, implementation)
        {
        }

        internal AstGetSetReference(Parser p, PCall call, SymbolEntry implementation)
            : base(p.scanner.File, p.t.line, p.t.col, call, implementation)
        {
        }

        internal AstGetSetReference(Parser p, SymbolEntry implementation)
            : base(p, PCall.Get, implementation)
        {
        }

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            var justEffect = stackSemantics == StackSemantics.Effect;
            if (justEffect)
                return;
            switch (Implementation.Interpretation)
            {
                case SymbolInterpretations.Command:
                    target.Emit(Position,OpCode.ldr_cmd, Implementation.InternalId);
                    break;
                case SymbolInterpretations.Function:
                    PFunction func;
                    //Check if the function is a macro (Cannot create references to macros)
                    if(target.Loader.ParentApplication.TryGetFunction(Implementation.InternalId, Implementation.Module, out func)
                        && func.IsMacro)
                    {
                        target.Loader.ReportMessage(Message.Create(MessageSeverity.Warning,
                                                                   string.Format(
                                                                       "Reference to macro {0} detected. Prexonite version {1} treats this " +
                                                                       "as a partial application. This behavior might change in the future. " +
                                                                       "Use partial application syntax explicitly {0}(?) or use the {2} command " +
                                                                       "to obtain a reference to the macro.",
                                                                       Implementation, Engine.PrexoniteVersion,
                                                                       Reference.Alias), Position,
                                                                   MessageClasses.ReferenceToMacro));

                        _emitAsPartialApplication(target);
                    }
                    else
                    {
                        target.Emit(Position,OpCode.ldr_func, Implementation.InternalId, target.ToInternalModule(Implementation.Module));
                    }
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    target.Emit(Position,OpCode.ldr_glob, Implementation.InternalId, target.ToInternalModule(Implementation.Module));
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    target.EmitLoadGlobal(Position, Implementation.InternalId, Implementation.Module);
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    target.Emit(Position,OpCode.ldr_loc, Implementation.InternalId);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    target.Emit(Position,OpCode.ldloc, Implementation.InternalId);
                    break;
                case SymbolInterpretations.MacroCommand:
                    target.Loader.ReportMessage(Message.Create(MessageSeverity.Warning,
                                                       string.Format(
                                                           Resources.AstGetSetReference_ReferenceToMacroTreatedAsPartialApplication,
                                                           Implementation.InternalId, Engine.PrexoniteVersion, Reference.Alias), Position, MessageClasses.ReferenceToMacro));

                    _emitAsPartialApplication(target);

                    break;
                default:
                    target.Loader.ReportMessage(
                        Message.Create(
                            MessageSeverity.Error,
                            string.Format(
                                Resources.AstGetSetReference_CannotCreateReference,
                                Enum.GetName(
                                    typeof (SymbolInterpretations), Implementation.Interpretation),
                                Implementation.InternalId), Position, MessageClasses.InvalidReference));
                    target.EmitNull(Position);
                    break;
            }
        }

        private void _emitAsPartialApplication(CompilerTarget target)
        {
            var pa = new AstMacroInvocation(File, Line, Column, Implementation) {Call = Call};
            pa.Arguments.Add(new AstPlaceholder(File, Line, Column, 0));
            var ipa = (AstExpr) pa;
            _OptimizeNode(target, ref ipa);
            ipa.EmitValueCode(target);
        }

        //"Assigning to a reference"
        protected override void EmitSetCode(CompilerTarget target)
        {
            switch (Implementation.Interpretation)
            {
                case SymbolInterpretations.Command:
                case SymbolInterpretations.Function:
                case SymbolInterpretations.JumpLabel:
                case SymbolInterpretations.KnownType:
                    throw new PrexoniteException(
// ReSharper disable PossibleNullReferenceException
                        string.Format(Resources.AstGetSetReference_CannotAssignReference, (Enum.GetName(typeof(SymbolInterpretations), Implementation.Interpretation) ?? Enum.GetName(typeof(SymbolInterpretations),SymbolInterpretations.Undefined)).ToLower()));
// ReSharper restore PossibleNullReferenceException

                    //Variables are not automatically dereferenced
                case SymbolInterpretations.GlobalObjectVariable:
                case SymbolInterpretations.GlobalReferenceVariable:
                    target.EmitStoreGlobal(Position, Implementation.InternalId, Implementation.Module);
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                case SymbolInterpretations.LocalReferenceVariable:
                    target.EmitStoreLocal(Position, Implementation.InternalId);
                    break;
            }
        }

        #region ICanBeReferenced Members

        ICollection<AstExpr> ICanBeReferenced.Arguments
        {
            get { return Arguments; }
        }

        public override bool TryToReference(out AstExpr reference)
        {
            reference = null;
            switch (Implementation.Interpretation)
            {
                case SymbolInterpretations.Command:
                case SymbolInterpretations.Function:
                case SymbolInterpretations.JumpLabel:
                case SymbolInterpretations.KnownType:
                case SymbolInterpretations.GlobalObjectVariable:
                case SymbolInterpretations.LocalObjectVariable:
                    return false;

                    //Variables are not automatically dereferenced

                case SymbolInterpretations.GlobalReferenceVariable:
                    reference =
                        new AstGetSetReference(
                            File,
                            Line,
                            Column,
                            PCall.Get, Implementation.With(SymbolInterpretations.GlobalObjectVariable));
                    break;

                case SymbolInterpretations.LocalReferenceVariable:
                    reference =
                        new AstGetSetReference(
                            File,
                            Line,
                            Column,
                            PCall.Get, Implementation.With(SymbolInterpretations.LocalObjectVariable));
                    break;
            }

            return reference != null;
        }

        #endregion
    }
}