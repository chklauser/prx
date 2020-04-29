#nullable enable
using System;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    /// <summary>
    ///     A managed symbol resolver.
    /// </summary>
    /// <param name = "t">The compiler target for which to compile code.</param>
    /// <param name = "unresolved">The unresolved AST node.</param>
    /// <returns>Null if no solution could be found. A compatible node otherwise.</returns>
    public delegate AstExpr? ResolveSymbol(CompilerTarget t, AstUnresolved unresolved);

    /// <summary>
    ///     Encapsulates a user provided resolver.
    /// </summary>
    public sealed class CustomResolver
    {
        private readonly ResolveSymbol? _managed;
        private readonly PValue? _interpreted;

        /// <summary>
        ///     Creates a new CustomResolver from managed code.
        /// </summary>
        /// <param name = "managedResolver">The implementation of the managed resolver.</param>
        public CustomResolver(ResolveSymbol managedResolver)
        {
            _managed = managedResolver;
        }

        /// <summary>
        ///     Creates a new CustomResolver from interpreted code.
        /// </summary>
        /// <param name = "interpretedResolver">The implementation of the interpreted resolver.</param>
        public CustomResolver(PValue interpretedResolver)
        {
            _interpreted = interpretedResolver;
        }

        /// <summary>
        ///     Applies the encapsulated custom resolver to the supplied AST node.
        /// </summary>
        /// <param name = "t">The compiler target for which to resolve the node.</param>
        /// <param name = "unresolved">The unresolved AST node.</param>
        /// <returns>Null if no solution has been found. A compatible AST node otherwise.</returns>
        public AstExpr? Resolve(CompilerTarget t, AstUnresolved unresolved)
        {
            if (_managed != null)
            {
                return _managed(t, unresolved);
            }
            else if (_interpreted != null)
            {
                var presult = _interpreted.IndirectCall
                    (
                        t.Loader, new[]
                            {
                                t.Loader.CreateNativePValue(t),
                                t.Loader.CreateNativePValue(unresolved)
                            });
                if (presult.Type is ObjectPType)
                    return (AstExpr) presult.Value;
                else
                    return null;
            }
            else
            {
                throw new InvalidOperationException(
                    "Invalid custom resolver. No implementation provided.");
            }
        }
    }
}