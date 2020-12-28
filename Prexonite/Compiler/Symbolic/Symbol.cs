// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
using System.Diagnostics;
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Symbolic
{
    public abstract class Symbol : IEquatable<Symbol>, IObject
    {
        public static readonly TraceSource Trace = new("Prexonite.Compiler.Symbolic");

        internal Symbol()
        {
        }

        public abstract TResult HandleWith<TArg, TResult>([NotNull] ISymbolHandler<TArg, TResult> handler, TArg argument);

        [NotNull]
        public abstract ISourcePosition Position { get; }

        [PublicAPI]
        [ContractAnnotation("=>true,expandSymbol: notnull;=>false,expandSymbol:canbenull")]
        public virtual bool TryGetExpandSymbol(out ExpandSymbol expandSymbol)
        {
            expandSymbol = null;
            return false;
        }

        [PublicAPI]
        [ContractAnnotation("=>true,messageSymbol: notnull;=>false,messageSymbol:canbenull")]
        public virtual bool TryGetMessageSymbol(out MessageSymbol messageSymbol)
        {
            messageSymbol = null;
            return false;
        }

        [PublicAPI]
        [ContractAnnotation("=>true,dereferenceSymbol: notnull;=>false,dereferenceSymbol:canbenull")]
        public virtual bool TryGetDereferenceSymbol(out DereferenceSymbol dereferenceSymbol)
        {
            dereferenceSymbol = null;
            return false;
        }

        [PublicAPI]
        [ContractAnnotation("=>true,referenceSymbol: notnull;=>false,referenceSymbol:canbenull")]
        public virtual bool TryGetReferenceSymbol(out ReferenceSymbol referenceSymbol)
        {
            referenceSymbol = null;
            return false;
        }

        [PublicAPI]
        [ContractAnnotation("=>true,nilSymbol: notnull;=>false,nilSymbol:canbenull")]
        public virtual bool TryGetNilSymbol(out NilSymbol nilSymbol)
        {
            nilSymbol = null;
            return false;
        }

        [PublicAPI]
        [ContractAnnotation("=>true,namespaceSymbol: notnull;=>false,namespaceSymbol:canbenull")]
        public virtual bool TryGetNamespaceSymbol(out NamespaceSymbol namespaceSymbol)
        {
            namespaceSymbol = null;
            return false;
        }

        #region Factory Methods

        [PublicAPI]
        [NotNull]
        public static Symbol CreateReference([NotNull] EntityRef entityRef, [NotNull] ISourcePosition position)
        {
            return ReferenceSymbol._Create(entityRef,position);
        }

        [PublicAPI]
        [NotNull]
        public static Symbol CreateNamespace([NotNull] Namespace @namespace,
            [NotNull] ISourcePosition position)
        {
            return NamespaceSymbol._Create(@namespace, position);
        }

        [PublicAPI]
        [NotNull]
        public static Symbol CreateNil([NotNull] ISourcePosition position)
        {
            return NilSymbol._Create(position);
        }

        [PublicAPI]
        [NotNull]
        public static Symbol CreateDereference([NotNull] Symbol inner, [CanBeNull] ISourcePosition position = null)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            return DereferenceSymbol._Create(inner, position);
        }

        [PublicAPI]
        [NotNull]
        public static Symbol CreateMessage([NotNull] Message message, [NotNull] Symbol inner, ISourcePosition position = null)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            return MessageSymbol._Create(message, inner, position);
        }

        [PublicAPI]
        [NotNull]
        public static Symbol CreateExpand([NotNull] Symbol inner, [CanBeNull] ISourcePosition position = null)
        {
            return ExpandSymbol._Create(inner, position);
        }

        #endregion

        [PublicAPI]
        [NotNull]
        public static Symbol CreateCall([NotNull] EntityRef entity, [NotNull] ISourcePosition position)
        {
            return CreateDereference(CreateReference(entity,position),position);
        }

        public abstract bool Equals(Symbol other);
        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj, null)) return false;
            if (obj.GetType() != GetType()) return false;
            else return Equals((Symbol) obj);
        }
        public abstract override int GetHashCode();

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            static PValue assignOutParameter(StackContext stackContext, PValue[] innerArgs, object value, bool found)
            {
                if (innerArgs.Length > 0)
                {
                    var wrappedValue = found ? stackContext.CreateNativePValue(value) : PType.Null;   
                    innerArgs[0].IndirectCall(stackContext, new[] {wrappedValue});
                }
                return stackContext.CreateNativePValue(found);
            }

            bool found;
            switch (id.ToUpperInvariant())
            {
                case "TRYGETNAMESPACESYMBOL":
                    found = TryGetNamespaceSymbol(out var namespaceSymbol);
                    result = assignOutParameter(sctx, args, namespaceSymbol, found);
                    return true;
                
                case "TRYGETNILSYMBOL":
                    found = TryGetNilSymbol(out var nilSymbol);
                    result = assignOutParameter(sctx, args, nilSymbol, found);
                    return true;
                
                case "TRYGETREFERENCESYMBOL":
                    found = TryGetReferenceSymbol(out var referenceSymbol);
                    result = assignOutParameter(sctx, args, referenceSymbol, found);
                    return true;
                
                case "TRYGETDEREFERENCESYMBOL":
                    found = TryGetDereferenceSymbol(out var dereferenceSymbol);
                    result = assignOutParameter(sctx, args, dereferenceSymbol, found);
                    return true;
                
                case "TRYGETMESSAGESYMBOL":
                    found = TryGetMessageSymbol(out var messageSymbol);
                    result = assignOutParameter(sctx, args, messageSymbol, found);
                    return true;
                case "TRYGETEXPANDSYMBOL":
                    found = TryGetExpandSymbol(out var expandSymbol);
                    result = assignOutParameter(sctx, args, expandSymbol, found);
                    return true;

            }

            result = PType.Null;
            return false;
        }
    }

    public abstract class WrappingSymbol : Symbol
    {
        public override ISourcePosition Position { get; }

        [DebuggerStepThrough]
        internal WrappingSymbol([NotNull] ISourcePosition position, [NotNull] Symbol innerSymbol)
        {
            InnerSymbol = innerSymbol;
            Position = position;
        }

        [NotNull]
        public Symbol InnerSymbol { [DebuggerStepThrough] get; }

        #region Equality members

        protected bool Equals(WrappingSymbol other)
        {
            return other != null 
                && other.GetType() == GetType() 
                && InnerSymbol.Equals(other.InnerSymbol);
        }

        public override int GetHashCode()
        {
            return HashCodeXorFactor ^InnerSymbol.GetHashCode();
        }

        protected abstract int HashCodeXorFactor { get; }

        [NotNull]
        public abstract WrappingSymbol With([NotNull] Symbol newInnerSymbol, [CanBeNull] ISourcePosition newPosition = null);

        #endregion
    }
}