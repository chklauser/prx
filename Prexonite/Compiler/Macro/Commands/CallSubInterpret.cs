using System;
using Prexonite.Compiler.Ast;
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
                context.ReportMessage(ParseMessageSeverity.Error, Alias + " requires one argument.");
                return;
            }

            if(context.CurrentLoopBlock != null && !context.IsJustEffect)
            {
                context.ReportMessage(ParseMessageSeverity.Error,
                    "Due to an internal compiler limitation, " + CallSub.Alias +
                        " and " + Alias + " cannot be used in an expression inside a loop, only as a statement.");
                return;
            }

            //Store result of call
            var resultV = context.AllocateTemporaryVariable();
            _storeResult(context, resultV);

            //Extract return variant as int into retVarV
            var retVarV = context.AllocateTemporaryVariable();
            _extractReturnVariant(context, resultV, retVarV);

            Func<AstGetSetSymbol> retVar =
                () =>
                context.CreateGetSetSymbol(SymbolInterpretations.LocalObjectVariable, PCall.Get,
                                           retVarV);

            //Extract return value into retValueV (which happens to be the same as resultV)
            var retValueV = resultV;
            _extractReturnValue(context, resultV, retValueV);

            Func<AstGetSetSymbol> retValue =
                () =>
                context.CreateGetSetSymbol(SymbolInterpretations.LocalObjectVariable, PCall.Get,
                                           retValueV);

            //Break and Continue behave differently outside loop blocks
            AstNode contStmt, breakStmt;
            _determineActions(context, retValue, out contStmt, out breakStmt);

            //Generate check for continue
            _genChecks(context, retVar, contStmt, breakStmt);

            context.Block.Expression = retValue();

            context.FreeTemporaryVariable(retVarV);
            context.FreeTemporaryVariable(resultV);
        }

        private static void _genChecks(MacroContext context, Func<AstGetSetSymbol> retVar, AstNode contStmt, AstNode breakStmt)
        {
            var inv = context.Invocation;

            //Generate check for continue
            AstCondition checkCont;
            {
                var contCond = _genCompare(context, retVar(), ReturnVariant.Continue);
                checkCont = new AstCondition(inv.File, inv.Line, inv.Column, contCond);
                checkCont.IfBlock.Add(contStmt);
            }

            //Generate check for break
            AstCondition checkBreak;
            {
                var breakCond = _genCompare(context, retVar(), ReturnVariant.Break);
                checkBreak = new AstCondition(inv.File, inv.Line, inv.Column, breakCond);
                checkBreak.IfBlock.Add(breakStmt);
            }

            //Connect break-check to continue check
            checkCont.ElseBlock.Add(checkBreak);
            context.Block.Add(checkCont);
        }

        private static void _determineActions(MacroContext context, Func<AstGetSetSymbol> retValue, out AstNode contStmt, out AstNode breakStmt)
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

        private static void _extractReturnValue(MacroContext context, string resultV, string retValueV)
        {
            var getRetValue =
                context.CreateGetSetMember(
                    context.CreateGetSetSymbol(SymbolInterpretations.LocalObjectVariable,
                                               PCall.Get,
                                               resultV), PCall.Get, "Value");
            var setRetValue =
                context.CreateGetSetSymbol(SymbolInterpretations.LocalObjectVariable,
                                           PCall.Set, retValueV, getRetValue);
            context.Block.Add(setRetValue);
        }

        private static void _extractReturnVariant(MacroContext context, string resultV, string retVarV)
        {
            var inv = context.Invocation;
            var intT = new AstConstantTypeExpression(inv.File, inv.Line, inv.Column,
                                                     IntPType.Literal);
            var getRetVar =
                context.CreateGetSetMember(
                    context.CreateGetSetSymbol(SymbolInterpretations.LocalObjectVariable,
                                               PCall.Get, resultV), PCall.Get, "Key");
            var asInt = new AstTypecast(inv.File, inv.Line, inv.Column, getRetVar, intT);
            var setRetVar = context.CreateGetSetSymbol(
                SymbolInterpretations.LocalObjectVariable, PCall.Set, retVarV, asInt);
            context.Block.Add(setRetVar);
        }

        private static void _storeResult(MacroContext context, string resultV)
        {
            var computeKvp = context.Invocation.Arguments[0];
            var setResult = context.CreateGetSetSymbol(
                SymbolInterpretations.LocalObjectVariable,
                PCall.Set, resultV, computeKvp);
            context.Block.Add(setResult);
        }

        #endregion

        private static IAstExpression _genCompare(MacroContext context, IAstExpression retVar, ReturnVariant expected)
        {
            const BinaryOperator eq = BinaryOperator.Equality;
            var inv = context.Invocation;
            IAstExpression expectedNode = new AstConstant(inv.File,
                                                          inv.Line,
                                                          inv.Column, (int) expected);
            var cmp = new AstBinaryOperator(inv.File, inv.Line,
                                            inv.Column, retVar, eq, expectedNode,
                                            SymbolInterpretations.Command,
                                            Prexonite.Commands.Core.Operators.Equality.DefaultAlias);
            return cmp;
        }
    }
}
