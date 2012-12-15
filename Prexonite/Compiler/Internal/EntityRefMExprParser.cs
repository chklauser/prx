using System;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Internal
{
    internal static class EntityRefMExprParser
    {
        [NotNull]
        public static EntityRef Parse([NotNull] MExpr expr)
        {
            // ReSharper disable TooWideLocalVariableScope
            MExpr internalIdExpr;
            MExpr moduleIdExpr;
            MExpr moduleVersionExpr;
            MExpr commandExpr;
            MExpr indexExpr;
            string internalId;
            string moduleId;
            Version moduleVersion;
            object rawIndex;
            // ReSharper restore TooWideLocalVariableScope
            if (expr.TryMatchHead(EntityRefMExprSerializer.FunctionHead, out internalIdExpr, out moduleIdExpr, out moduleVersionExpr))
            {
                if (internalIdExpr.TryMatchStringAtom(out internalId)
                    && moduleIdExpr.TryMatchStringAtom(out moduleId)
                    && moduleVersionExpr.TryMatchVersionAtom(out moduleVersion))
                {
                    return EntityRef.Function.Create(internalId, new ModuleName(moduleId, moduleVersion));
                }
                else
                {
                    throw new ErrorMessageException(
                        Message.Error(String.Format("Cannot parse function entity reference {0}.", expr),
                                      expr.Position, MessageClasses.CannotParseMExpr));
                }
            }
            else if (expr.TryMatchHead(EntityRefMExprSerializer.CommandHead, out internalIdExpr))
            {
                if (internalIdExpr.TryMatchStringAtom(out internalId))
                {
                    return EntityRef.Command.Create(internalId);
                }
                else
                {
                    throw new ErrorMessageException(
                        Message.Error(String.Format("Cannot parse command entity reference {0}.", expr),
                                      expr.Position, MessageClasses.CannotParseMExpr));
                }
            }
            else if (expr.TryMatchHead(EntityRefMExprSerializer.LocalVariableHead, out internalIdExpr, out indexExpr) &&
                     indexExpr.TryMatchAtom(out rawIndex))
            {
                if (internalIdExpr.TryMatchStringAtom(out internalId))
                {
                    var index = rawIndex == null ? null : rawIndex as int?;

                    var local = EntityRef.Variable.Local.Create(internalId);
                    if (index != null)
                        return local.WithIndex(index.Value);
                    else
                        return local;
                }
                else
                {
                    throw new ErrorMessageException(
                        Message.Error(String.Format("Cannot parse local variable entity reference {0}.", expr),
                                      expr.Position, MessageClasses.CannotParseMExpr));
                }
            }
            else if (expr.TryMatchHead(EntityRefMExprSerializer.GlobalVariableHead, out internalIdExpr,
                                       out moduleIdExpr, out moduleVersionExpr))
            {
                if (internalIdExpr.TryMatchStringAtom(out internalId)
                    && moduleIdExpr.TryMatchStringAtom(out moduleId)
                    && moduleVersionExpr.TryMatchVersionAtom(out moduleVersion))
                {
                    return EntityRef.Variable.Global.Create(internalId, new ModuleName(moduleId,moduleVersion));
                }
                else
                {
                    throw new ErrorMessageException(
                        Message.Error(String.Format("Cannot parse global variable entity reference {0}.", expr),
                                      expr.Position, MessageClasses.CannotParseMExpr));
                }
            }
            else if (expr.TryMatchHead(EntityRefMExprSerializer.MacroCommandModifierHead, out commandExpr)
                     && commandExpr.TryMatchHead(EntityRefMExprSerializer.CommandHead, out internalIdExpr))
            {
                if (internalIdExpr.TryMatchStringAtom(out internalId))
                {
                    return EntityRef.MacroCommand.Create(internalId);
                }
                else
                {
                    throw new ErrorMessageException(
                        Message.Error(String.Format("Cannot parse macro command entity reference {0}.", expr),
                                      expr.Position, MessageClasses.CannotParseMExpr));
                }
            }
            else
            {
                throw new ErrorMessageException(
                    Message.Error(String.Format("Cannot parse entity reference (unknown head) {0}", expr),
                                  expr.Position, MessageClasses.CannotParseMExpr));
            }
        }
    }
}