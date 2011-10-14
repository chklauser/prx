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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstGetSetMemberAccess : AstGetSet,
                                         IAstPartiallyApplicable
    {
        public string Id { get; set; }
        public IAstExpression Subject { get; set; }

        public override IAstExpression[] Expressions
        {
            get
            {
                var len = Arguments.Count;
                var ary = new IAstExpression[len + 1];
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
            get { return base.DefaultAdditionalArguments + 1; //include subject
            }
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            Subject.EmitCode(target);
            base.EmitCode(target, justEffect);
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            target.EmitGetCall(this, Arguments.Count, Id, justEffect);
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            target.EmitSetCall(this, Arguments.Count, Id);
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            base.TryOptimize(target, out expr);
            var subject = Subject;
            _OptimizeNode(target, ref subject);
            Subject = subject;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            AstGetSet copy = new AstGetSetMemberAccess(File, Line, Column, Call, Subject, Id);
            CopyBaseMembers(copy);
            return copy;
        }

        public override string ToString()
        {
            return string.Format("{0}: ({1}).{2}{3}",
                Enum.GetName(typeof (PCall), Call).ToLowerInvariant(),
                Subject, Id, ArgumentsToString());
        }

        #region Implementation of IAstPartiallyApplicable

        public void DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(
                    Subject.Singleton().Append(Arguments));
            var ctorArgc = this.EmitConstructorArguments(target, argv);
            target.EmitConstant(this, (int) Call);
            target.EmitConstant(this, Id);
            target.EmitCommandCall(this, ctorArgc + 2, Engine.PartialMemberCallAlias);
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Subject.IsPlaceholder();
        }

        #endregion
    }
}