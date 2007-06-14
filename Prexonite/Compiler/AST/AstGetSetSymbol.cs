/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetSymbol : AstGetSet,
                                   IAstExpression
    {
        public SymbolInterpretations Interpretation;
        public string Id;

        public AstGetSetSymbol(string file, int line, int column, PCall call, string id,
                               SymbolInterpretations interpretation)
            : base(file, line, column, call)
        {
            Interpretation = interpretation;
            Id = id;
        }

        public AstGetSetSymbol(string file, int line, int column, string id, SymbolInterpretations interpretation)
            : this(file, line, column, PCall.Get, id, interpretation)
        {
        }

        internal AstGetSetSymbol(Parser p, PCall call, string id, SymbolInterpretations interpretation)
            : this(p.scanner.File, p.t.line, p.t.col, call, id, interpretation)
        {
        }

        internal AstGetSetSymbol(Parser p, string id, SymbolInterpretations interpretation)
            : this(p, PCall.Get, id, interpretation)
        {
        }

        public override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            switch (Interpretation)
            {
                case SymbolInterpretations.Command:
                    target.EmitCommandCall(Arguments.Count, Id, justEffect);
                    break;
                case SymbolInterpretations.Function:
                    target.EmitFunctionCall(Arguments.Count, Id, justEffect);
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    if (!justEffect)
                        target.EmitLoadGlobal(Id);
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    if (!justEffect)
                        target.EmitLoadLocal(Id);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    target.Emit(Instruction.CreateLocalIndirectCall(Arguments.Count, Id, justEffect));
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    target.Emit(Instruction.CreateGlobalIndirectCall(Arguments.Count, Id, justEffect));
                    break;
                default:
                    throw new PrexoniteException("Invalid symbol " +
                                                 Enum.GetName(typeof(SymbolInterpretations), Interpretation) +
                                                 " in AST.");
            }
        }

        public override void EmitSetCode(CompilerTarget target)
        {
            switch (Interpretation)
            {
                case SymbolInterpretations.Command:
                    target.EmitCommandCall(Arguments.Count, Id, true);
                    break;
                case SymbolInterpretations.Function:
                    target.EmitFunctionCall(Arguments.Count, Id, true);
                    break;
                case SymbolInterpretations.GlobalObjectVariable:
                    target.EmitStoreGlobal(Id);
                    break;
                case SymbolInterpretations.LocalReferenceVariable:
                    target.Emit(Instruction.CreateLocalIndirectCall(Arguments.Count, Id));
                    break;
                case SymbolInterpretations.GlobalReferenceVariable:
                    target.Emit(Instruction.CreateGlobalIndirectCall(Arguments.Count, Id));
                    break;
                case SymbolInterpretations.LocalObjectVariable:
                    target.EmitStoreLocal(Id);
                    break;
                default:
                    throw new PrexoniteException("Invalid symbol " +
                                                 Enum.GetName(typeof(SymbolInterpretations), Interpretation) +
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
                String.Format(" {0}-{1} {2}", Enum.GetName(typeof(SymbolInterpretations), Interpretation), Id,
                              ArgumentsToString());
        }
    }
}