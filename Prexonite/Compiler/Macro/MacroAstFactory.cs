﻿// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
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

using JetBrains.Annotations;
using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro
{
    public class MacroAstFactory : AstFactoryBase
    {
        [NotNull]
        private readonly MacroSession _session;


        public MacroAstFactory(MacroSession session)
        {
            if (session == null)
                throw new System.ArgumentNullException("session");
            _session = session;
        }

        #region Overrides of AstFactoryBase

        protected override AstBlock CurrentBlock
        {
            get { return _session.CurrentBlock; }
        }

        protected override bool TryUseSymbolEntry(string symbolicId, ISourcePosition position, out SymbolEntry entry)
        {
            return _session.Target._TryUseSymbolEntry(symbolicId, position, out entry);
        }

        protected override AstGetSet CreateNullNode(ISourcePosition position)
        {
            return IndirectCall(position, Null(position));
        }

        #endregion
    }
}