using System.Collections.Generic;
using System.Linq;
using Prexonite.Compiler.Ast;
using Prexonite.Properties;

namespace Prexonite.Compiler.Macro.Commands;

public class StaticCallBuiltIn : BuiltInTypeCommandBase
{
    public StaticCallBuiltIn(string registryId) : base($"{Loader.StaticCallPrefix}{registryId}", registryId)
    {
    }

    protected override int NumAdditionalArguments => 1;
    protected override string IncompleteMessageClass => MessageClasses.IncompleteBuiltinStaticCall;
    protected override string OperationName => "Static call";

    protected override void Instantiate(MacroContext context, AstDynamicTypeExpression typeExpr, IEnumerable<AstExpr> additionalArguments,
        IEnumerable<AstExpr> operationArguments)
    {
        var methodNameExpr = additionalArguments.First();
        if (context.GetOptimizedNode(methodNameExpr) is not AstConstant { Constant: string methodName })
        {
            var message = Resources.StaticCallBuiltIn_DoExpand_Static_call_requires_a_constant_method_name;
            context.ReportMessage(Message.Error(
                message, 
                methodNameExpr.Position, 
                MessageClasses.IncompleteBuiltinStaticCall));
            return;
        }

        var cast = new AstGetSetStatic(context.Invocation.Position, context.Call, typeExpr, methodName);
        cast.Arguments.AddRange(operationArguments);
        context.Block.Expression = cast;
    }
}