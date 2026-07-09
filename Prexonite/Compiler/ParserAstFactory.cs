

using Prexonite.Compiler.Ast;
using Prexonite.Properties;

namespace Prexonite.Compiler;

class ParserAstFactory : AstFactoryBase
{
    readonly Parser _parser;

    protected override AstBlock CurrentBlock => _parser.CurrentBlock ??
        throw new PrexoniteException("Internal error: current block cannot be accessed on the top level.");

    protected override AstGetSet CreateNullNode(ISourcePosition position)
    {
        return Parser._NullNode(position);
    }

    protected override bool IsOuterVariable(string id)
    {
        if (_parser.target == null)
            return false;
        else
            return _parser.target._IsOuterVariable(id);
    }

    protected override void RequireOuterVariable(string id)
    {
        if (_parser.target == null)
        {
            ReportMessage(
                Message.Error(Resources.ParserAstFactory_RequireOuterVariable_Outside_function,
                    _parser.GetPosition(),
                    MessageClasses.ParserInternal));
        }
        else
        {
            _parser.target.RequireOuterVariable(id);
        }
    }

    public override void ReportMessage(Message message)
    {
        _parser.Loader.ReportMessage(message);
    }

    protected override CompilerTarget CompileTimeExecutionContext
    {
        get
        {
            var compilerTarget = _parser.target;
            if (compilerTarget == null)
            {
                throw new InvalidOperationException("Internal parser error. Cannot access compilation target on top level.");
            }
            else
            {
                return compilerTarget;
            }
        }
    }

    public ParserAstFactory(Parser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }
}