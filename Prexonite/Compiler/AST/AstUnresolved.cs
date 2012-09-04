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
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstUnresolved : AstGetSet
    {
        public AstUnresolved(string file, int line, int column, string id)
            : base(file, line, column, PCall.Get)
        {
            _id = id;
        }

        internal AstUnresolved(Parser p, string id) : base(p, PCall.Get)
        {
            _id = id;
        }

        #region Overrides of AstGetSet

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            _reportUnresolved(target);
        }

        private void _reportUnresolved(CompilerTarget target)
        {
            target.Loader.ReportMessage(
                Message.Error(
                    string.Format(Resources.AstUnresolved_The_symbol__0__has_not_been_resolved_, Id), this,
                    MessageClasses.SymbolNotResolved));
        }

        private string _id;

        public string Id
        {
            get { return _id; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _id = value;
            }
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            _reportUnresolved(target);
        }

        public override AstGetSet GetCopy()
        {
            var copy = new AstUnresolved(File, Line, Column, _id);
            CopyBaseMembers(copy);
            return copy;
        }

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            if (base.TryOptimize(target, out expr))
                return true;
            else
            {
                AstExpr sol = this;
                do
                {
                    foreach (var resolver in target.Loader.CustomResolvers)
                    {
                        sol = resolver.Resolve(target, sol as AstUnresolved);
                        if (sol != null)
                            break;
                    }
                    expr = sol;
                } while ((sol != this) && (expr is AstUnresolved));
                if (sol == this)
                    return false;
                else
                    return expr != null;
            }
        }

        #endregion
    }
}