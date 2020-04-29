using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Prexonite.Types;

#nullable enable

namespace Prexonite.Compiler.Symbolic
{
    public abstract class Namespace : ISymbolView<Symbol>, IObject
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract IEnumerator<KeyValuePair<string, Symbol>> GetEnumerator();
        public abstract bool IsEmpty { get; }
        public abstract bool TryGet(string id, [NotNullWhen(true)] out Symbol? value);
        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            if ("TRYGET".Equals(id, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length < 1 || args[0] == null || args[0].IsNull)
                {
                    throw new PrexoniteException("Namespace.TryGet(id, ref symbol) requires a non-null id.");
                }

                var found = TryGet(args[0].CallToString(sctx), out var symbol);
                if (args.Length >= 2)
                {
                    args[1].IndirectCall(sctx, new[] {sctx.CreateNativePValue(symbol)});
                }

                result = sctx.CreateNativePValue(found);
                return true;
            }

            result = PType.Null;
            return false;
        }
    }
}