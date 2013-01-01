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

using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Ast
{
    [DebuggerNonUserCode]
    public class AstCreateClosure : AstExpr
    {
        [NotNull] private readonly EntityRef.Function _implementation;

        public AstCreateClosure(ISourcePosition position, EntityRef.Function implementation)
            : base(position)
        {
            _implementation = implementation;
        }

        public EntityRef.Function Implementation
        {
            get { return _implementation; }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            if (stackSemantics == StackSemantics.Effect)
                return;

            PFunction targetFunction;
            MetaEntry sharedNamesEntry;
            if (target.Loader.ParentApplication.TryGetFunction(_implementation.Id, _implementation.ModuleName, out targetFunction)
                && (!targetFunction.Meta.TryGetValue(PFunction.SharedNamesKey, out sharedNamesEntry)
                    || !sharedNamesEntry.IsList
                        || sharedNamesEntry.List.Length == 0))
                target.Emit(Position,OpCode.ldr_func, _implementation.Id, target.ToInternalModule(_implementation.ModuleName));
            else
                target.Emit(Position,OpCode.newclo, _implementation.Id, target.ToInternalModule(_implementation.ModuleName));
        }

        #region AstExpr Members

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            expr = null;
            return false;
        }

        #endregion
    }
}