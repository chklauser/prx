// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Collections.Generic;
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    public static class StructHook
    {
        public const string CtorId = @"\ctorId";
        public const string PrivateKey = "Private";
        public const string StructId = "SId";
        public const string TriggerId = "struct";
        private static readonly CompilerHook _hook = new CompilerHook(Hook);

        public static void InstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Add(_hook);
        }

        public static void UninstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Remove(_hook);
        }

        public static void Hook(CompilerTarget target)
        {
            replace_struct(target, target.Ast);
        }

        private static void replace_struct(CompilerTarget t, IList<AstNode> block)
        {
            for (var i = 0; i < block.Count; i++)
            {
                var stmt = block[i];
                AstReturn ret;
                AstGetSetSymbol symbolRef;
                if ((ret = stmt as AstReturn) != null && ret.ReturnVariant == ReturnVariant.Exit &&
                    (symbolRef = ret.Expression as AstGetSetSymbol) != null &&
                        symbolRef.Interpretation == SymbolInterpretations.Function &&
                            Engine.StringsAreEqual(symbolRef.Id, TriggerId))
                {
                    //Found a return struct();
                    var caller = t.Function;
                    var parentId = caller.Id;

                    //Get all methods (Mapping from method id to logical id (id of the variable containing the reference to the closure)
                    var methods = new Dictionary<string, string>();
                    foreach (var f in caller.ParentApplication.Functions)
                    {
                        var fmeta = f.Meta;
                        if (
                            Engine.StringsAreEqual(fmeta[PFunction.ParentFunctionKey].Text, parentId) &&
                                !fmeta[PrivateKey].Switch)
                        {
                            MetaEntry e;
                            if (!fmeta.TryGetValue(PFunction.LogicalIdKey, out e))
                                continue;
                            var logicalId = e.Text;

                            string methodId;
                            if (fmeta.TryGetValue(StructId, out e))
                                methodId = e.Text;
                            else
                                methodId = logicalId;
                            methods.Add(methodId, logicalId);
                        }
                    }

                    //Insert code that creates and returns the structure
                    var newCode = new List<AstNode>(2 + methods.Count);
                    var vtemps = t.CurrentBlock.CreateLabel("temp");

                    //(s)
                    var structGet =
                        new AstGetSetSymbol(
                            stmt.File,
                            stmt.Line,
                            stmt.Column,
                            PCall.Get,
                            vtemps,
                            SymbolInterpretations.LocalObjectVariable);

                    //var s = new Structure;
                    caller.Variables.Add(vtemps);
                    var structAssignment =
                        new AstGetSetSymbol(
                            stmt.File,
                            stmt.Line,
                            stmt.Column,
                            PCall.Set,
                            vtemps,
                            SymbolInterpretations.LocalObjectVariable);
                    structAssignment.Arguments.Add(
                        new AstObjectCreation(
                            stmt.File,
                            stmt.Line,
                            stmt.Column,
                            new AstConstantTypeExpression(
                                stmt.File,
                                stmt.Line,
                                stmt.Column,
                                StructurePType.Literal)));
                    newCode.Add(structAssignment);

                    //set \ctorId
                    //s.\(@"\ctorId") = "ctorId";
                    var setCtorId =
                        new AstGetSetMemberAccess(
                            stmt.File,
                            stmt.Line,
                            stmt.Column,
                            PCall.Set,
                            structGet,
                            StructurePType.SetIdAlternative);
                    setCtorId.Arguments.Add(new AstConstant(stmt.File, stmt.Line, stmt.Column,
                        CtorId));
                    setCtorId.Arguments.Add(new AstConstant(stmt.File, stmt.Line, stmt.Column,
                        parentId));
                    newCode.Add(setCtorId);

                    foreach (var method in methods)
                    {
                        //Key => member id //Value => logical id
                        var setMethod =
                            new AstGetSetMemberAccess(
                                stmt.File,
                                stmt.Line,
                                stmt.Column,
                                PCall.Set,
                                structGet,
                                StructurePType.SetRefId);
                        setMethod.Arguments.Add(new AstConstant(stmt.File, stmt.Line, stmt.Column,
                            method.Key));
                        setMethod.Arguments.Add(
                            new AstGetSetSymbol(
                                stmt.File,
                                stmt.Line,
                                stmt.Column,
                                PCall.Get,
                                method.Value,
                                SymbolInterpretations.LocalObjectVariable));
                        newCode.Add(setMethod);
                    }

                    //Return the struct
                    var r = new AstReturn(stmt.File, stmt.Line, stmt.Column, ReturnVariant.Exit)
                        {
                            Expression = structGet
                        };
                    newCode.Add(r);

                    //Insert new code
                    foreach (var node in newCode)
                        block.Insert(i++, node);

                    //remove call to struct
                    block.RemoveAt(i--);
                } //end found struct();
                else
                {
                    //Recursively replace 'debug' in nested blocks.
                    var complex = block[i] as IAstHasBlocks;
                    if (complex != null)
                        foreach (var subBlock in complex.Blocks)
                            replace_struct(t, subBlock);
                }
            } //End of statement loop
        }
    }
}