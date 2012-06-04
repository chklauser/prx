// Prexonite
// 
// Copyright (c) 2012, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.$
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

using System.Collections.Generic;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class Symbol
    {
         internal Symbol()
         {
         }

        public abstract TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument);
        public virtual bool TryGetEntitySymbol(out EntitySymbol entitySymbol)
        {
            entitySymbol = null;
            return false;
        }
        public virtual bool TryGetMessageSymbol(out MessageSymbol messageSymbol)
        {
            messageSymbol = null;
            return false;
        }
        public virtual bool TryGetMacroInstanceSymbol(out MacroInstanceSymbol macroInstanceSymbol)
        {
            macroInstanceSymbol = null;
            return false;
        }
    }

    public interface ISymbolHandler<in TArg, out TResult>
    {
        TResult HandleEntity(EntitySymbol symbol, TArg argument);
        TResult HandleMessage(MessageSymbol symbol, TArg argument);
        TResult HandleMacroInstance(MacroInstanceSymbol symbol, TArg argument);
    }

    public sealed class EntitySymbol : Symbol
    {
        private readonly EntityRef _entity;
        private readonly bool _isDereferenced;

        public EntitySymbol(EntityRef entity, bool isDereferenced = false)
        {
            _entity = entity;
            _isDereferenced = isDereferenced;
        }

        public EntityRef Entity
        {
            get { return _entity; }
        }

        public bool IsDereferenced
        {
            get { return _isDereferenced; }
        }

        public bool Equals(EntitySymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._entity, _entity) && other._isDereferenced.Equals(_isDereferenced);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (EntitySymbol)) return false;
            return Equals((EntitySymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_entity.GetHashCode()*397) ^ _isDereferenced.GetHashCode();
            }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleEntity(this, argument);
        }

        public override bool TryGetEntitySymbol(out EntitySymbol entitySymbol)
        {
            entitySymbol = this;
            return true;
        }
    }

    public sealed class MessageSymbol : Symbol
    {
        private readonly Message _message;
        private readonly Symbol _symbol;

        public MessageSymbol(Message message, Symbol symbol)
        {
            _message = message;
            _symbol = symbol;
        }

        public Message Message
        {
            get { return _message; }
        }

        public Symbol Symbol
        {
            get { return _symbol; }
        }

        public bool Equals(MessageSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._message, _message) && Equals(other._symbol, _symbol);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (MessageSymbol)) return false;
            return Equals((MessageSymbol) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_message.GetHashCode()*397) ^ (_symbol != null ? _symbol.GetHashCode() : 0);
            }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleMessage(this, argument);
        }

        public override bool TryGetMessageSymbol(out MessageSymbol messageSymbol)
        {
            messageSymbol = this;
            return true;
        }
    }

    public sealed class MacroInstanceSymbol : Symbol
    {
        private readonly EntityRef.IMacro _macroReference;
        private readonly ReadOnlyDictionaryView<string,AstExpr> _arguments;

        internal MacroInstanceSymbol(EntityRef.IMacro macroReference, IEnumerable<KeyValuePair<string,AstExpr>> arguments)
        {
            _macroReference = macroReference;
            var d = new Dictionary<string, AstExpr>();
            foreach (var entry in arguments)
                d[entry.Key] = entry.Value;
            _arguments = new ReadOnlyDictionaryView<string, AstExpr>(d);
        }

        public static MacroInstanceSymbol Create<T>(T macroEntity, IEnumerable<KeyValuePair<string, AstExpr>> arguments) where T : EntityRef, EntityRef.IMacro
        {
            return new MacroInstanceSymbol(macroEntity, arguments);
        }

        public EntityRef.IMacro MacroReference
        {
            get { return _macroReference; }
        }

        public ReadOnlyDictionaryView<string, AstExpr> Arguments
        {
            get { return _arguments; }
        }

        public override TResult HandleWith<TArg, TResult>(ISymbolHandler<TArg, TResult> handler, TArg argument)
        {
            return handler.HandleMacroInstance(this, argument);
        }

        public override bool TryGetMacroInstanceSymbol(out MacroInstanceSymbol macroInstanceSymbol)
        {
            macroInstanceSymbol = this;
            return true;
        }
    }
}