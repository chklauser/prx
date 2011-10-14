// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetSymbol : AstGetSet, ICanBeReferenced, IAstPartiallyApplicable
    {
        public SymbolInterpretations Interpretation;
        public string Id;

        public AstGetSetSymbol(
            string file,
            int line,
            int column,
            PCall call,
            string id,
            SymbolInterpretations interpretation)
            : base(file, line, column, call)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            Interpretation = interpretation;
            Id = id;
        }

        public AstGetSetSymbol(
            string file, int line, int column, string id, SymbolInterpretations interpretation)
            : this(file, line, column, PCall.Get, id, interpretation)
        {
        }

        internal AstGetSetSymbol(
            Parser p, PCall call, string id, SymbolInterpretations interpretation)
            : this(p.scanner.File, p.t.line, p.t.col, call, id, interpretation)
        {
        }

        internal AstGetSetSymbol(Parser p, string id, SymbolInterpretations interpretation)
            : this(p, PCall.Get, id, interpretation)
        {
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            switch (Interpretation)
            {
                case SymbolInterpretations.Command:
                    target.EmitCommandCall(this, Arguments.Count, Id, justEffect);
                    break;
                case SymbolInterpretations.Function:
                    target.EmitFunctionCall(this, Arguments.Count, Id, justEffect);
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    if (!justEffect)
                        target.EmitLoadGlobal(this, Id);
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    if (!justEffect)
                        target.EmitLoadLocal(this, Id);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    target.Emit(this,
                        Instruction.CreateLocalIndirectCall(Arguments.Count, Id, justEffect));
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    target.Emit(this,
                        Instruction.CreateGlobalIndirectCall(Arguments.Count, Id, justEffect));
                    break;
                default:
                    throw new PrexoniteException(
                        "Invalid symbol " +
                            Enum.GetName(typeof (SymbolInterpretations), Interpretation) +
                                " in AST.");
            }
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            const bool justEffect = true;
            switch (Interpretation)
            {
                case SymbolInterpretations.Command:
                    target.EmitCommandCall(this, Arguments.Count, Id, justEffect);
                    break;
                case SymbolInterpretations.Function:
                    target.EmitFunctionCall(this, Arguments.Count, Id, justEffect);
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    target.EmitStoreGlobal(this, Id);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    target.Emit(this,
                        Instruction.CreateLocalIndirectCall(Arguments.Count, Id, justEffect));
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    target.Emit(this,
                        Instruction.CreateGlobalIndirectCall(Arguments.Count, Id, justEffect));
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    target.EmitStoreLocal(this, Id);
                    break;
                default:
                    throw new PrexoniteException(
                        "Invalid symbol " +
                            Enum.GetName(typeof (SymbolInterpretations), Interpretation) +
                                " in AST.");
            }
        }

        public bool IsObjectVariable
        {
            get
            {
                return
                    Interpretation == SymbolInterpretations.GlobalObjectVariable ||
                        Interpretation == SymbolInterpretations.LocalObjectVariable;
            }
        }

        public bool IsVariable
        {
            get
            {
                return
                    Interpretation == SymbolInterpretations.GlobalObjectVariable ||
                        Interpretation == SymbolInterpretations.GlobalReferenceVariable ||
                            Interpretation == SymbolInterpretations.LocalObjectVariable ||
                                Interpretation == SymbolInterpretations.LocalReferenceVariable;
            }
        }

        public override AstGetSet GetCopy()
        {
            AstGetSet copy = new AstGetSetSymbol(File, Line, Column, Call, Id, Interpretation);
            CopyBaseMembers(copy);
            return copy;
        }

        public override string ToString()
        {
            return
                base.ToString() +
                    String.Format(
                        " {0}-{1} {2}",
                        Enum.GetName(typeof (SymbolInterpretations), Interpretation),
                        Id,
                        ArgumentsToString());
        }

        #region ICanBeReferenced Members

        ICollection<IAstExpression> ICanBeReferenced.Arguments
        {
            get { return Arguments; }
        }

        public virtual bool TryToReference(out AstGetSet result)
        {
            result = null;
            switch (Interpretation)
            {
                case SymbolInterpretations.Function:
                case SymbolInterpretations.GlobalObjectVariable:
                case SymbolInterpretations.LocalObjectVariable:
                case SymbolInterpretations.LocalReferenceVariable:
                case SymbolInterpretations.GlobalReferenceVariable:
                case SymbolInterpretations.Command:
                    result =
                        new AstGetSetReference(File, Line, Column, PCall.Get, Id, Interpretation);
                    break;
            }

            return result != null;
        }

        #endregion

        #region Implementation of IAstPartiallyApplicable

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            AstGetSet refNode;
            if (!TryToReference(out refNode))
                throw new PrexoniteException("Cannot partially apply " + this +
                    " because it can't be converted to a reference.");

            var indTemplate = new AstIndirectCall(File, Line, Column, Call, refNode);
            indTemplate.Arguments.AddRange(Arguments);
            Debug.Assert(indTemplate.CheckForPlaceholders());
            indTemplate.EmitCode(target);
        }

        #endregion
    }
}