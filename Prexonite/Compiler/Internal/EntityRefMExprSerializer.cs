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
        public const string LocalVariableHead = "lvar";
        public const string GlobalVariableHead = "var";
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