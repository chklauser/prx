// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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