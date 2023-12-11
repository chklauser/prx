using Prexonite.Compiler.Ast;

namespace Prexonite.Compiler.Macro.Commands;

public class ConvertToBuiltIn : BuiltInTypeCommandBase
{

    public ConvertToBuiltIn(string registryId) : base($"{Loader.ConversionPrefix}{registryId}", registryId)
    {
    }

    protected override int NumAdditionalArguments => 0;
    protected override string IncompleteMessageClass => MessageClasses.IncompleteBuiltinConversion;
    protected override string OperationName => "Type cast";

    protected override void Instantiate(MacroContext context, AstDynamicTypeExpression typeExpr, IEnumerable<AstExpr> additionalArguments,
        IEnumerable<AstExpr> operationArguments)
    {
        var subject = operationArguments.First();
        var cast = new AstTypecast(context.Invocation.Position, subject, typeExpr);
        context.Block.Expression = cast;
    }
}