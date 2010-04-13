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
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetStatic : AstGetSet,
                                   IAstExpression
    {
        public IAstType TypeExpr;
        public string MemberId;

        [DebuggerStepThrough]
        public AstGetSetStatic(
            string file, int line, int col, PCall call, IAstType typeExpr, string memberId)
            : base(file, line, col, call)
        {
            if (typeExpr == null)
                throw new ArgumentNullException("typeExpr");
            if (memberId == null)
                throw new ArgumentNullException("memberId");
            TypeExpr = typeExpr;
            MemberId = memberId;
        }

        [DebuggerStepThrough]
        internal AstGetSetStatic(Parser p, PCall call, IAstType typeExpr, string memberId)
            : this(p.scanner.File, p.t.line, p.t.col, call, typeExpr, memberId)
        {
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            TypeExpr = (IAstType) GetOptimizedNode(target, TypeExpr);
            return base.TryOptimize(target, out expr);
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                EmitArguments(target);
                target.EmitStaticGetCall(
                    Arguments.Count, constType.TypeExpression, MemberId, justEffect);
            }
            else
            {
                TypeExpr.EmitCode(target);
                target.EmitConstant(MemberId);
                EmitArguments(target);
                target.EmitGetCall(Arguments.Count + 1, "StaticCall\\FromStack", justEffect);
            }
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            EmitSetCode(target, true);
        }

        private void EmitSetCode(CompilerTarget target, bool justEffect)
        {
            var constType = TypeExpr as AstConstantTypeExpression;

            if (constType != null)
            {
                EmitArguments(target, !justEffect, 0);
                target.EmitStaticSetCall(
                    Arguments.Count, constType.TypeExpression + "::" + MemberId);
            }
            else
            {
                TypeExpr.EmitCode(target);
                target.EmitConstant(MemberId);
                EmitArguments(target, !justEffect, 2); //type.StaticCall\FromStack(memberId, args...)
                target.EmitSetCall(Arguments.Count + 1, "StaticCall\\FromStack");
            }
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            //Do not yet emit arguments.
            if (Call == PCall.Get)
                EmitGetCode(target, justEffect);
            else
                EmitSetCode(target, justEffect);
        }

        public override AstGetSet GetCopy()
        {
            AstGetSet copy = new AstGetSetStatic(File, Line, Column, Call, TypeExpr, MemberId);
            CopyBaseMembers(copy);
            return copy;
        }
    }
}