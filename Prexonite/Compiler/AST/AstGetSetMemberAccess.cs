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
    public class AstGetSetMemberAccess : AstGetSet,
                                         IAstExpression,
                                         IAstHasExpressions
    {
        public string Id;
        public IAstExpression Subject;

        public override IAstExpression[] Expressions
        {
            get
            {
                int len = Arguments.Count;
                IAstExpression[] ary = new IAstExpression[len + 1];
                Array.Copy(Arguments.ToArray(), 0, ary, 1, len);
                ary[0] = Subject;
                return ary;
            }
        }

        public AstGetSetMemberAccess(
            string file, int line, int column, PCall call, IAstExpression subject, string id)
            : base(file, line, column, call)
        {
            if (subject == null)
                throw new ArgumentNullException("subject");
            if (id == null)
                id = "";
            Subject = subject;
            Id = id;
        }

        internal AstGetSetMemberAccess(Parser p, PCall call, IAstExpression subject, string id)
            : this(p.scanner.File, p.t.line, p.t.col, call, subject, id)
        {
        }

        public AstGetSetMemberAccess(string file, int line, int column, PCall call, string id)
            : this(file, line, column, call, null, id)
        {
        }

        internal AstGetSetMemberAccess(Parser p, PCall call, string id)
            : this(p, call, null, id)
        {
        }

        public AstGetSetMemberAccess(string file, int line, int column, string id)
            : this(file, line, column, PCall.Get, id)
        {
        }

        internal AstGetSetMemberAccess(Parser p, string id)
            : this(p.scanner.File, p.t.line, p.t.col, PCall.Get, id)
        {
        }

        public AstGetSetMemberAccess(
            string file, int line, int column, IAstExpression subject, string id)
            : this(file, line, column, PCall.Get, subject, id)
        {
        }

        internal AstGetSetMemberAccess(Parser p, IAstExpression subject, string id)
            : this(p, PCall.Get, subject, id)
        {
        }

        public override int DefaultAdditionalArguments
        {
            get
            {
                return base.DefaultAdditionalArguments + 1; //include subject
            }
        } 
        
        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            Subject.EmitCode(target);
            base.EmitCode(target, justEffect);
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            target.EmitGetCall(Arguments.Count, Id, justEffect);
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            target.EmitSetCall(Arguments.Count, Id);
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            base.TryOptimize(target, out expr);
            OptimizeNode(target, ref Subject);
            return false;
        }

        public override AstGetSet GetCopy()
        {
            AstGetSet copy = new AstGetSetMemberAccess(File, Line, Column, Call, Subject, Id);
            CopyBaseMembers(copy);
            return copy;
        }
    }
}