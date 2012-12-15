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
using JetBrains.Annotations;
using Prexonite.Commands.Core.Operators;
using Prexonite.Compiler.Ast;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands
{
    public class CallSubInterpret : MacroCommand
    {
        public const string Alias = @"call\sub\interpret";

        #region Singleton pattern

        private static readonly CallSubInterpret _instance = new CallSubInterpret();

        public static CallSubInterpret Instance
        {
            get { return _instance; }
        }

        private CallSubInterpret() : base(Alias)
        {
        }

        #endregion

        #region Overrides of MacroCommand

        protected override void DoExpand(MacroContext context)
        {
            if (context.Invocation.Arguments.Count == 0)
            {
                context.ReportMessage(
                    Message.Error(
                        string.Format(Resources.CallSubInterpret_OneArgument, Alias), context.Invocation.Position,
                        MessageClasses.SubUsage));
                return;
            }

            if (context.CurrentLoopBlock != null && !context.IsJustEffect)
            {
                context.ReportMessage(
                    Message.Error(
                        string.Format(
                            Resources.CallSubInterpret_asExpressionInLoop, CallSub.Alias, Alias),
                        context.Invocation.Position, MessageClasses.SubAsExpressionInLoop));
                return;
            }

            //Store result of call
            var resultV = context.AllocateTemporaryVariable();
            _storeResult(context, resultV);

            //Extract return variant as int into retVarV
            var retVarV = context.AllocateTemporaryVariable();
            _extractReturnVariant(context, resultV, retVarV);

            Func<AstGetSetSymbol> retVar = () =>
                context.CreateGetSetSymbol(SymbolEntry.LocalObjectVariable(retVarV), PCall.Get);

            //Extract return value into retValueV (which happens to be the same as resultV)
            var retValueV = resultV;
            _extractReturnValue(context, resultV, retValueV);

// ReSharper disable ImplicitlyCapturedClosure // perfectly safe as neither lambda survives the method
            Func<AstGetSetSymbol> retValue = () =>
// ReSharper restore ImplicitlyCapturedClosure
                context.CreateGetSetSymbol(SymbolEntry.LocalObjectVariable(retValueV), PCall.Get);

            //Break and Continue behave differently outside loop blocks
            AstNode contStmt, breakStmt;
            _determineActions(context, retValue, out contStmt, out breakStmt);

            //Generate check for continue
            _genChecks(context, retVar, contStmt, breakStmt);

            context.Block.Expression = retValue();

            context.FreeTemporaryVariable(retVarV);
            context.FreeTemporaryVariable(resultV);
        }

        private static void _genChecks(MacroContext context, [InstantHandle] Func<AstGetSetSymbol> retVar,
            AstNode contStmt, AstNode breakStmt)
        {
            var inv = context.Invocation;

            //Generate check for continue
            AstCondition checkCont;
            {
                var contCond = _genCompare(context, retVar(), ReturnVariant.Continue);
                checkCont = new AstCondition(inv.Position, context.CurrentBlock, contCond);
                checkCont.IfBlock.Add(contStmt);
            }

            //Generate check for break
            AstCondition checkBreak;
            {
                var breakCond = _genCompare(context, retVar(), ReturnVariant.Break);
                checkBreak = new AstCondition(inv.Position, context.CurrentBlock, breakCond);
                checkBreak.IfBlock.Add(breakStmt);
            }

            //Connect break-check to continue check
            checkCont.ElseBlock.Add(checkBreak);
            context.Block.Add(checkCont);
        }

        private static void _determineActions(MacroContext context, [InstantHandle] Func<AstGetSetSymbol> retValue,
            out AstNode contStmt, out AstNode breakStmt)
        {
            var inv = context.Invocation;
            var bl = context.CurrentLoopBlock;
            if (bl == null)
            {
                contStmt = new AstReturn(inv.File, inv.Line, inv.Column, ReturnVariant.Continue)
                    {Expression = retValue()};
                breakStmt = new AstReturn(inv.File, inv.Line, inv.Column, ReturnVariant.Break)
                    {Expression = retValue()};
            }
            else
            {
                contStmt = new AstExplicitGoTo(inv.File, inv.Line, inv.Column, bl.ContinueLabel);
                breakStmt = new AstExplicitGoTo(inv.File, inv.Line, inv.Column, bl.BreakLabel);
            }
        }

        private static void _extractReturnValue(MacroContext context, string resultV,
            string retValueV)
        {
            var getRetValue =
                context.CreateGetSetMember(
                    context.CreateGetSetSymbol(SymbolEntry.LocalObjectVariable(resultV), 
                        PCall.Get), PCall.Get, "Value");
            var setRetValue =
                context.CreateGetSetSymbol(SymbolEntry.LocalObjectVariable(retValueV), 
                    PCall.Set, getRetValue);
            context.Block.Add(setRetValue);
        }

        private static void _extractReturnVariant(MacroContext context, string resultV,
            string retVarV)
        {
            var inv = context.Invocation;
            var intT = new AstConstantTypeExpression(inv.File, inv.Line, inv.Column,
                IntPType.Literal);
            var getRetVar =
                context.CreateGetSetMember(
                    context.CreateGetSetSymbol(SymbolEntry.LocalObjectVariable(resultV), 
                        PCall.Get), PCall.Get, "Key");
            var asInt = new AstTypecast(inv.File, inv.Line, inv.Column, getRetVar, intT);
            var setRetVar = context.CreateGetSetSymbol(
                SymbolEntry.LocalObjectVariable(retVarV), PCall.Set, asInt);
            context.Block.Add(setRetVar);
        }

        private static void _storeResult(MacroContext context, string resultV)
        {
            var computeKvp = context.Invocation.Arguments[0];
            var setResult = context.CreateGetSetSymbol(
                SymbolEntry.LocalObjectVariable(resultV), 
                PCall.Set, computeKvp);
            context.Block.Add(setResult);
        }

        #endregion

        private static AstExpr _genCompare(MacroContext context, AstExpr retVar,
            ReturnVariant expected)
        {
            const BinaryOperator eq = BinaryOperator.Equality;
            var inv = context.Invocation;
            AstExpr expectedNode = new AstConstant(inv.File,
                inv.Line,
                inv.Column, (int) expected);
            var cmp = new AstBinaryOperator(inv.File, inv.Line,
                                            inv.Column, retVar, eq, expectedNode,
                                            new SymbolEntry(SymbolInterpretations.Command, Equality.DefaultAlias, null),
                                            context.CurrentBlock);
            return cmp;
        }
    }
}