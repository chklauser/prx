using Prexonite.Compiler.Ast;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Properties;

namespace Prexonite.Compiler;

public static class AstFactoryExtensions
{
    extension(IAstFactory factory)
    {
        public AstGetSet Call(
            ISourcePosition position,
            EntityRef entity,
            PCall call = PCall.Get,
            params AstExpr[] arguments
        )
        {
            var c = factory.IndirectCall(position, factory.Reference(position, entity), call);
            c.Arguments.AddRange(arguments);
            return c;
        }

        public AstExpr ExprFor(
            ISourcePosition position,
            QualifiedId qualifiedId,
            ISymbolView<Symbol> scope
        )
        {
            var currentScope = scope;
            for (var i = 0; i < qualifiedId.Count; i++)
            {
                // Lookup name part
                if (!currentScope.TryGet(qualifiedId[i], out var sym))
                {
                    return new AstUnresolved(position, qualifiedId[i]);
                }

                var expr = factory.ExprFor(position, sym);

                // last part of qualified ID does not need to be a namespace, so we're done here
                if (i == qualifiedId.Count - 1)
                    return expr;

                if (expr is not AstNamespaceUsage nsUsage)
                {
                    factory.ReportMessage(
                        Message.Error(
                            string.Format(Resources.Parser_NamespaceExpected, qualifiedId[i], sym),
                            position,
                            MessageClasses.NamespaceExcepted
                        )
                    );
                    return factory.IndirectCall(position, factory.Null(position));
                }

                currentScope = nsUsage.Namespace;
            }

            throw new InvalidOperationException(
                "Failed to resolve qualified Id (program control should never reach this point)"
            );
        }
    }
}
