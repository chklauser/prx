// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic
{
    [PublicAPI]
    public abstract class SymbolHandler<TArg, TResult> : ISymbolHandler<TArg, TResult>
    {
        [PublicAPI]
        protected virtual TResult HandleSymbolDefault(Symbol self, TArg argument)
        {
            throw new NotSupportedException(
                string.Format(
                    Resources.SymbolHandler_CannotHandleSymbolOfType, GetType().Name,
                    self.GetType().Name));
        }

        [PublicAPI]
        protected virtual TResult HandleWrappingSymbol(WrappingSymbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        [PublicAPI]
        protected virtual TResult HandleLeafSymbol(Symbol self, TArg argument)
        {
            return HandleSymbolDefault(self, argument);
        }

        #region Implementation of ISymbolHandler<in TArg,out TResult>

        [PublicAPI]
        public virtual TResult HandleReference(ReferenceSymbol self, TArg argument)
        {
            return HandleLeafSymbol(self, argument);
        }

        [PublicAPI]
        public virtual TResult HandleNil(NilSymbol self, TArg argument)
        {
            return HandleLeafSymbol(self, argument);
        }

        [PublicAPI]
        public virtual TResult HandleExpand(ExpandSymbol self, TArg argument)
        {
            return HandleWrappingSymbol(self, argument);
        }

        [PublicAPI]
        public virtual TResult HandleDereference(DereferenceSymbol self, TArg argument)
        {
            return HandleWrappingSymbol(self, argument);
        }

        [PublicAPI]
        public virtual TResult HandleMessage(MessageSymbol self, TArg argument)
        {
            return HandleWrappingSymbol(self, argument);
        }

        [PublicAPI]
        public TResult HandleNamespace(NamespaceSymbol self, TArg argument)
        {
            return HandleLeafSymbol(self, argument);
        }

        #endregion
    }
}