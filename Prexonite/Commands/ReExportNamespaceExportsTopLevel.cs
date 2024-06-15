using System.Collections.Immutable;
using Prexonite.Commands.List;
using Prexonite.Compiler;
using Prexonite.Compiler.Symbolic;

namespace Prexonite.Commands;

public class ReExportNamespaceExportsTopLevel(Loader loader) : ICommand
{
    public PValue Run(StackContext sctx, PValue[] args)
    {
        if (args.Length < 1)
        {
            throw new PrexoniteException("Re-exporting namespace export at the top level requires at least one argument.");
        }

        var nsPathBuf = Map._ToEnumerable(sctx, args[0]).Select(x => x.CallToString(sctx)).ToImmutableArray();
        if (nsPathBuf.Length == 0)
        {
            return PType.Null;
        }

        var nsPath = nsPathBuf.AsSpan();
        ISymbolView<Symbol> currentScope = loader.Symbols;

        while (nsPath.Length > 0)
        {
            if (!currentScope.TryGet(nsPath[0], out var resolved) || !resolved.TryGetNamespaceSymbol(out var resolvedNs))
            {
                throw new PrexoniteException($"Cannot find '{nsPath[0]}' of namespace '{string.Join(".", nsPathBuf)}'.");
            }

            currentScope = resolvedNs.Namespace;
            nsPath = nsPath[1..];
        }

        foreach (var (name, symbol) in currentScope)
        {
            loader.TopLevelSymbols.Declare(name, symbol);
        }

        return PType.Null;
    }
}