/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

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
            for (int i = 0; i < block.Count; i++)
            {
                AstNode stmt = block[i];
                AstReturn ret;
                AstGetSetSymbol symbolRef;
                if ((ret = stmt as AstReturn) != null && ret.ReturnVariant == ReturnVariant.Exit &&
                    (symbolRef = ret.Expression as AstGetSetSymbol) != null &&
                    symbolRef.Interpretation == SymbolInterpretations.Function &&
                    Engine.StringsAreEqual(symbolRef.Id, TriggerId))
                {
                    //Found a return struct();
                    PFunction caller = t.Function;
                    string parentId = caller.Id;

                    //Get all methods (Mapping from method id to logical id (id of the variable containing the reference to the closure)
                    Dictionary<string, string> methods = new Dictionary<string, string>();
                    foreach (PFunction f in caller.ParentApplication.Functions)
                    {
                        MetaTable fmeta = f.Meta;
                        if (Engine.StringsAreEqual(fmeta[PFunction.ParentFunctionKey].Text, parentId) &&
                            !fmeta[PrivateKey].Switch)
                        {
                            MetaEntry e;
                            if (!fmeta.TryGetValue(PFunction.LogicalIdKey, out e))
                                continue;
                            string logicalId = e.Text;

                            string methodId;
                            if (fmeta.TryGetValue(StructId, out e))
                                methodId = e.Text;
                            else
                                methodId = logicalId;
                            methods.Add(methodId, logicalId);
                        }
                    }

                    //Insert code that creates and returns the structure
                    List<AstNode> newCode = new List<AstNode>(2 + methods.Count);
                    BlockLabels l = new BlockLabels("struct");
                    string vtemps = l.CreateLabel("temp");

                    //(s)
                    AstGetSetSymbol structGet =
                        new AstGetSetSymbol(stmt.File,
                                            stmt.Line,
                                            stmt.Column,
                                            PCall.Get,
                                            vtemps,
                                            SymbolInterpretations.LocalObjectVariable);

                    //var s = new Structure;
                    caller.Variables.Add(vtemps);
                    AstGetSetSymbol structAssignment =
                        new AstGetSetSymbol(stmt.File,
                                            stmt.Line,
                                            stmt.Column,
                                            PCall.Set,
                                            vtemps,
                                            SymbolInterpretations.LocalObjectVariable);
                    structAssignment.Arguments.Add(
                        new AstObjectCreation(stmt.File,
                                              stmt.Line,
                                              stmt.Column,
                                              new AstConstantTypeExpression(stmt.File,
                                                                            stmt.Line,
                                                                            stmt.Column,
                                                                            StructurePType.Literal)));
                    newCode.Add(structAssignment);

                    //set \ctorId
                    //s.\(@"\ctorId") = "ctorId";
                    AstGetSetMemberAccess setCtorId =
                        new AstGetSetMemberAccess(stmt.File,
                                                  stmt.Line,
                                                  stmt.Column,
                                                  PCall.Set,
                                                  structGet,
                                                  StructurePType.SetIdAlternative);
                    setCtorId.Arguments.Add(new AstConstant(stmt.File, stmt.Line, stmt.Column, CtorId));
                    setCtorId.Arguments.Add(new AstConstant(stmt.File, stmt.Line, stmt.Column, parentId));
                    newCode.Add(setCtorId);

                    foreach (KeyValuePair<string, string> method in methods)
                    {
                        //Key => member id //Value => logical id
                        AstGetSetMemberAccess setMethod =
                            new AstGetSetMemberAccess(stmt.File,
                                                      stmt.Line,
                                                      stmt.Column,
                                                      PCall.Set,
                                                      structGet,
                                                      StructurePType.SetRefId);
                        setMethod.Arguments.Add(new AstConstant(stmt.File, stmt.Line, stmt.Column, method.Key));
                        setMethod.Arguments.Add(
                            new AstGetSetSymbol(stmt.File,
                                                stmt.Line,
                                                stmt.Column,
                                                PCall.Get,
                                                method.Value,
                                                SymbolInterpretations.LocalObjectVariable));
                        newCode.Add(setMethod);
                    }

                    //Return the struct
                    AstReturn r = new AstReturn(stmt.File, stmt.Line, stmt.Column, ReturnVariant.Exit);
                    r.Expression = structGet;
                    newCode.Add(r);

                    //Insert new code
                    foreach (AstNode node in newCode)
                        block.Insert(i++, node);

                    //remove call to struct
                    block.RemoveAt(i--);
                } //end found struct();
                else
                {
                    //Recursively replace 'debug' in nested blocks.
                    IAstHasBlocks complex = block[i] as IAstHasBlocks;
                    if (complex != null)
                        foreach (AstBlock subBlock in complex.Blocks)
                            replace_struct(t, subBlock);
                }
            } //End of statement loop
        }
    }
}