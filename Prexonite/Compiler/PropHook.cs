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
using Prexonite.Compiler.Ast;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    /// <summary>
    ///     Implementation of auto-properties via 
    ///     transformations of calls to the non-existant function "prop".
    /// </summary>
    public static class PropHook
    {
        private static readonly CompilerHook _hook = new CompilerHook(Hook);
        public const string PropFunctionId = "prop";
        public const string StrucPropFunctionId = "struct_prop";

        /// <summary>
        ///     Installs the auto-property compiler hook.
        /// </summary>
        /// <param name = "ldr">The loader to add the hook to.</param>
        public static void InstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Add(_hook);
        }

        /// <summary>
        ///     Uninstalls the auto-property compiler hook.
        /// </summary>
        /// <param name = "ldr">The loader that contains the hook.</param>
        public static void UninstallHook(Loader ldr)
        {
            if (ldr == null)
                throw new ArgumentNullException("ldr");
            ldr.CompilerHooks.Remove(_hook);
        }

        private static void Hook(CompilerTarget target)
        {
            var block = target.Ast;
            for (var i = 0; i < block.Count; i++)
            {
                var any_stmt = block[i];
                var stmt_ret = any_stmt as AstReturn;
                AstGetSetSymbol stmt;

                if (stmt_ret == null || stmt_ret.Expression == null ||
                    (stmt = stmt_ret.Expression as AstGetSetSymbol) == null ||
                        stmt.Interpretation != SymbolInterpretations.Function ||
                            (!Engine.StringsAreEqual(stmt.Id, PropFunctionId) &&
                                !Engine.StringsAreEqual(stmt.Id, StrucPropFunctionId)))
                    continue;

                var args_offset = 0;

                //accomodate for the "this" parameter in struct member functions.
                if (stmt.Id == StrucPropFunctionId)
                    args_offset = 1;

                //Ensure access to function arguments
                var parameters = target.Function.Parameters;

                while (parameters.Count < args_offset)
                    parameters.Add(Engine.GenerateName("dummy_arg"));

                if (parameters.Count == args_offset)
                    target.Function.Parameters.Add(Engine.GenerateName("prop_forward"));

                var prop_arg = new AstGetSetSymbol(
                    stmt.File,
                    stmt.Line,
                    stmt.Column,
                    PCall.Get,
                    target.Function.Parameters[args_offset],
                    SymbolInterpretations.LocalObjectVariable);

                //Determine the type of prop

                var argc = stmt.Arguments.Count;

                AstGetSet prop_get, prop_set;

                switch (argc)
                {
                    case 0:

                        //create backing field
                        AstGetSetSymbol backingField;
                        if (target.ParentTarget == null)
                        {
                            //property is global, using a global backing field.
                            backingField = new AstGetSetSymbol(
                                stmt.File,
                                stmt.Line,
                                stmt.Column,
                                PCall.Get,
                                Engine.GenerateName(target.Function.Id + "_prop_field_"),
                                SymbolInterpretations.GlobalObjectVariable);
                            target.Loader.ParentApplication.Variables.Add(
                                backingField.Id, new PVariable(backingField.Id));
                        }
                        else
                        {
                            //Property is local using a closure field for storage
                            var pt = target.ParentTarget;
                            backingField = new AstGetSetSymbol(
                                stmt.File,
                                stmt.Line,
                                stmt.Column,
                                PCall.Get,
                                Engine.GenerateName(target.Function.Id + "_prop_field_"),
                                SymbolInterpretations.LocalObjectVariable);
                            pt.Function.Variables.Add(backingField.Id);
                            target.RequireOuterVariable(backingField.Id);
                        }
                        prop_get = backingField;
                        prop_set = backingField.GetCopy();
                        prop_set.Call = PCall.Set;
                        break;
                    case 1:
                        var proxy_expr = stmt.Arguments[0] as AstGetSet;
                        if (proxy_expr == null)
                            throw new PrexoniteException(
                                "prop requires an assignable expression as its argument.");
                        prop_get = proxy_expr;
                        prop_set = proxy_expr.GetCopy();
                        prop_set.Call = PCall.Set;
                        break;
                    default:
                        var get_action = stmt.Arguments[0];
                        var set_action = stmt.Arguments[1];

                        prop_get = new AstIndirectCall(stmt.File, stmt.Line, stmt.Column, PCall.Get,
                            get_action);
                        prop_set = new AstIndirectCall(stmt.File, stmt.Line, stmt.Column, PCall.Set,
                            set_action);
                        break;
                }

                //Create get or set check
                var nullCheck = new AstTypecheck(
                    stmt.File,
                    stmt.Line,
                    stmt.Column,
                    prop_arg,
                    new AstConstantTypeExpression(stmt.File, stmt.Line, stmt.Column,
                        NullPType.Literal));
                var getArgs = new AstGetSetSymbol(stmt.File, stmt.Line, stmt.Column,
                    PFunction.ArgumentListId, SymbolInterpretations.LocalObjectVariable);
                if (!target.Function.Variables.Contains(PFunction.ArgumentListId))
                    target.Function.Variables.Add(PFunction.ArgumentListId);
                var getArgc = new AstGetSetMemberAccess(stmt.File, stmt.Line, stmt.Column, getArgs,
                    "Count");
                var cmpEqZero = new AstBinaryOperator(stmt.File, stmt.Line, stmt.Column, getArgc,
                    BinaryOperator.Equality, new AstConstant(stmt.File, stmt.Line, stmt.Column, 0),
                    SymbolInterpretations.Command, OperatorNames.Prexonite.Equality);
                var conj = new AstLogicalAnd(stmt.File, stmt.Line, stmt.Column, nullCheck, cmpEqZero);
                var check = new AstConditionalExpression(
                    stmt.File,
                    stmt.Line,
                    stmt.Column,
                    conj);
                prop_set.Arguments.Add(prop_arg);

                check.IfExpression = prop_get;
                check.ElseExpression = prop_set;

                stmt_ret.Expression = check;
            }
        }
    }
}