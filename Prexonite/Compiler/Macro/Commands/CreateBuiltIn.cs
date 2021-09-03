using System.Collections.Generic;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CreateBuiltIn : BuiltInTypeCommandBase
    {

        public CreateBuiltIn(string registryId) : base($"{Loader.ObjectCreationPrefix}{registryId}", registryId)
        {
        }

        protected override int NumAdditionalArguments => 0;
        protected override string IncompleteMessageClass => MessageClasses.IncompleteBuiltinObjectCreation;
        protected override string OperationName => "Value creation";

        protected override void Instantiate(MacroContext context, AstDynamicTypeExpression typeExpr, IEnumerable<AstExpr> additionalArguments,
            IEnumerable<AstExpr> operationArguments)
        {
            var creation = new AstObjectCreation(context.Invocation.Position, typeExpr);
            creation.Arguments.AddRange(operationArguments);
            context.Block.Expression = creation;
        }
    }
}