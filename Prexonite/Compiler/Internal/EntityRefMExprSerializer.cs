

using Prexonite.Modular;

namespace Prexonite.Compiler.Internal;

class EntityRefMExprSerializer : EntityRefMatcher<ISourcePosition, MExpr>
{
    #region Singleton

    public static EntityRefMExprSerializer Instance { get; } = new();

    #endregion

    protected override MExpr OnNotMatched(EntityRef entity, ISourcePosition position)
    {
        throw new ErrorMessageException(
            Message.Error(
                $"Unknown entity reference type {entity.GetType().Name} encountered in MExpr serialization.", position, MessageClasses.UnknownEntityRefType));
    }

    public const string FunctionHead = "function";
    public const string CommandHead = "command";
    public const string LocalVariableHead = "lvar";
    public const string GlobalVariableHead = "var";
    public const string MacroCommandModifierHead = "macro";

    MExpr _serializeRefWithModule(ISourcePosition position, string head,
        string id, ModuleName moduleName)
    {
        return new MExpr.MList(position, head,
        [
            new MExpr.MAtom(position, id),
                new MExpr.MAtom(position, moduleName.Id),
                new MExpr.MAtom(position, moduleName.Version),
        ]);
    }

    MExpr _serializeRef(ISourcePosition position, string head, string id)
    {
        return new MExpr.MList(position, head, [new MExpr.MAtom(position,id)]);
    }

    public override MExpr OnFunction(EntityRef.Function function, ISourcePosition position)
    {
        return _serializeRefWithModule(position, FunctionHead, function.Id, function.ModuleName);
    }

    protected override MExpr OnCommand(EntityRef.Command command, ISourcePosition position)
    {
        return _serializeRef(position, CommandHead, command.Id);
    }

    protected override MExpr OnMacroCommand(EntityRef.MacroCommand macroCommand, ISourcePosition position)
    {
        return new MExpr.MList(position,MacroCommandModifierHead,_serializeRef(position,CommandHead,macroCommand.Id));
    }

    protected override MExpr OnLocalVariable(EntityRef.Variable.Local variable, ISourcePosition position)
    {
        return new MExpr.MList(position,LocalVariableHead, [new MExpr.MAtom(position, variable.Id), new MExpr.MAtom(position,variable.Index),
        ]);
    }

    protected override MExpr OnGlobalVariable(EntityRef.Variable.Global variable, ISourcePosition position)
    {
        return _serializeRefWithModule(position, GlobalVariableHead, variable.Id, variable.ModuleName);
    }
}