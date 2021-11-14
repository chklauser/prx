#nullable enable
using System;
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Internal;

internal static class EntityRefMExprParser
{
    [NotNull]
    public static EntityRef Parse([NotNull] MExpr expr)
    {
        // ReSharper disable TooWideLocalVariableScope
        string internalId;
        string moduleId;
        Version moduleVersion;
        // ReSharper restore TooWideLocalVariableScope
        if (expr.TryMatchHead(EntityRefMExprSerializer.FunctionHead, out var internalIdExpr, out var moduleIdExpr, out var moduleVersionExpr))
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
                    Message.Error($"Cannot parse function entity reference {expr}.",
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
                    Message.Error($"Cannot parse command entity reference {expr}.",
                        expr.Position, MessageClasses.CannotParseMExpr));
            }
        }
        else if (expr.TryMatchHead(EntityRefMExprSerializer.LocalVariableHead, out internalIdExpr, out var indexExpr) &&
                 indexExpr.TryMatchAtom(out var rawIndex))
        {
            if (internalIdExpr.TryMatchStringAtom(out internalId))
            {
                var index = rawIndex as int?;

                var local = EntityRef.Variable.Local.Create(internalId);
                if (index != null)
                    return local.WithIndex(index.Value);
                else
                    return local;
            }
            else
            {
                throw new ErrorMessageException(
                    Message.Error($"Cannot parse local variable entity reference {expr}.",
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
                    Message.Error($"Cannot parse global variable entity reference {expr}.",
                        expr.Position, MessageClasses.CannotParseMExpr));
            }
        }
        else if (expr.TryMatchHead(EntityRefMExprSerializer.MacroCommandModifierHead, out MExpr commandExpr)
                 && commandExpr.TryMatchHead(EntityRefMExprSerializer.CommandHead, out internalIdExpr))
        {
            if (internalIdExpr.TryMatchStringAtom(out internalId))
            {
                return EntityRef.MacroCommand.Create(internalId);
            }
            else
            {
                throw new ErrorMessageException(
                    Message.Error($"Cannot parse macro command entity reference {expr}.",
                        expr.Position, MessageClasses.CannotParseMExpr));
            }
        }
        else
        {
            throw new ErrorMessageException(
                Message.Error($"Cannot parse entity reference (unknown head) {expr}",
                    expr.Position, MessageClasses.CannotParseMExpr));
        }
    }
}