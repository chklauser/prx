using System;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Internal
{
    internal class EntityRefMExprSerializer : EntityRefMatcher<ISourcePosition, MExpr>
    {
        #region Singleton

        private static readonly EntityRefMExprSerializer _instance = new EntityRefMExprSerializer();

        public static EntityRefMExprSerializer Instance
        {
            get { return _instance; }
        }

        #endregion

        protected override MExpr OnNotMatched(EntityRef entity, ISourcePosition position)
        {
            throw new ErrorMessageException(
                Message.Error(
                    String.Format("Unknown entity reference type {0} encountered in MExpr serialization.",
                                  entity.GetType().Name), position, MessageClasses.UnknownEntityRefType));
        }

        public const string FunctionHead = "function";
        public const string CommandHead = "command";
        public const string LocalVariableHead = "var";
        public const string GlobalVariableHead = "gvar";
        public const string MacroCommandModifierHead = "macro";

        [NotNull]
        private MExpr _serializeRefWithModule([NotNull] ISourcePosition position, [NotNull] string head,
                                              [NotNull] string id, [NotNull] ModuleName moduleName)
        {
            return new MExpr.MList(position, head,
                                    new[]
                                        {
                                            new MExpr.MAtom(position, id),
                                            new MExpr.MAtom(position, moduleName.Id),
                                            new MExpr.MAtom(position, moduleName.Version)
                                        });
        }

        [NotNull]
        private MExpr _serializeRef(ISourcePosition position, string head, string id)
        {
            return new MExpr.MList(position, head,new []{new MExpr.MAtom(position,id) });
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
            return new MExpr.MList(position,LocalVariableHead,new []{new MExpr.MAtom(position, variable.Id), new MExpr.MAtom(position,variable.Index)});
        }

        protected override MExpr OnGlobalVariable(EntityRef.Variable.Global variable, ISourcePosition position)
        {
            return _serializeRefWithModule(position, GlobalVariableHead, variable.Id, variable.ModuleName);
        }
    }
}