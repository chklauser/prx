using System.Collections.Generic;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CheckIsBuiltIn : BuiltInTypeCommandBase
    {
        public CheckIsBuiltIn(string registryId) : base($"{Loader.TypeCheckPrefix}{registryId}", registryId)
        {
        }

        protected override int NumAdditionalArguments => 0;
        protected override string IncompleteMessageClass => MessageClasses.IncompleteBuiltinTypeCheck;
        protected override string OperationName => "Type check";

        protected override void Instantiate(MacroContext context, AstDynamicTypeExpression typeExpr, IEnumerable<AstExpr> additionalArguments,
            IEnumerable<AstExpr> operationArguments)
        {   
            var subject = operationArguments.First();
            var check = new AstTypecheck(context.Invocation.Position, subject, typeExpr);
            context.Block.Expression = check;
        }
    }
}