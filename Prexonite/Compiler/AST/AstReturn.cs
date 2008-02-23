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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstReturn : AstNode,
                             IAstHasExpressions
    {
        public ReturnVariant ReturnVariant;
        public IAstExpression Expression;

        public AstReturn(string file, int line, int column, ReturnVariant returnVariant)
            : base(file, line, column)
        {
            ReturnVariant = returnVariant;
        }

        internal AstReturn(Parser p, ReturnVariant returnVariant)
            : this(p.scanner.File, p.t.line, p.t.col, returnVariant)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[] {Expression}; }
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (Expression != null)
            {
                OptimizeNode(target,ref Expression);
                if (ReturnVariant == Ast.ReturnVariant.Exit)
                {
                    emit_tail_call_exit(target);
                    return;
                }
            }
            switch (ReturnVariant)
            {
                case ReturnVariant.Exit:
                    target.Emit(OpCode.ret_exit);
                    break;
                case ReturnVariant.Set:
                    if (Expression == null)
                        throw new PrexoniteException("Return assignment requires an expression.");
                    Expression.EmitCode(target);
                    target.Emit(OpCode.ret_set);
                    break;
                case ReturnVariant.Continue:
                    if (Expression != null)
                    {
                        Expression.EmitCode(target);
                        target.Emit(OpCode.ret_set);
                    }
                    target.Emit(OpCode.ret_continue);
                    break;
                case ReturnVariant.Break:
                    target.Emit(OpCode.ret_break);
                    break;
            }
        }

        private void emit_tail_call_exit(CompilerTarget target)
        {
            if(optimize_conditional_return_expression(target))
                return;

            AstGetSet getset = Expression as AstGetSet;
            AstGetSetSymbol symbol = Expression as AstGetSetSymbol;
            ICanBeReferenced icbr = Expression as ICanBeReferenced;

            AstGetSet reference;
            if((getset != null && getset.Call == PCall.Set || //the 'value' of set-expressions is not the return value of the call
                (symbol != null && symbol.IsObjectVariable))  ||  
                icbr == null || !icbr.TryToReference(out reference)) //tail requires a reference to the continuation
            {   //Cannot be tail call optimized
                Expression.EmitCode(target);
                target.Emit(OpCode.ret_value);
            }
            else //Will be tail called
            {
                if (symbol != null && check_if_stackless_function_recursion_is_possible(target, symbol))
                {
                    // specialized approach
                    // self(arg1, arg2, ..., argn) => { param1 = arg1; param2 = arg2; ... paramn = argn; goto 0; }
                    List<string> symbolParams = target.Function.Parameters;
                    ArgumentsProxy symbolArgs = symbol.Arguments;
                    AstNull nullNode = new AstNull(File, Line, Column);
                    
                    //copy parameters to temporary variables
                    for (int i = 0; i < symbolParams.Count; i++)
                    {
                        if (i < symbolArgs.Count)
                            symbolArgs[i].EmitCode(target);
                        else
                            nullNode.EmitCode(target);
                    }
                    //overwrite parameters
                    for (int i = symbolParams.Count-1; i >= 0; i--)
                    {
                        target.EmitStoreLocal(symbolParams[i]);
                    }

                    target.EmitJump(0);
                }
                else
                {
                    //NOTE: VM tail calls are not used at the moment.
                    Expression.EmitCode(target);
                    target.Emit(OpCode.ret_value);
                    return;

                    // general apporach
                    // getset(arg1,arg2,..,argn) => tail(->getset, arg1, arg2,..,argn)
/*
                    int addrReference, addrCall;

                    //Emit code for the reference
                    addrReference = target.Code.Count;
                    reference.EmitCode(target);

                    //Emit arguments
                    foreach(IAstExpression argument in icbr.Arguments)
                        argument.EmitCode(target);

                    //Emit actual call
                    addrCall = target.Code.Count;
                    target.Emit(OpCode.tail, icbr.Arguments.Count);

                    if(symbol == null)
                        return; //don't add cil compiler hint, as there is no more efficient implementation

                    //Compose CIL compiler hint.
                    MetaEntry[] entry = new MetaEntry[Loader.TailCallHintLength];
                    entry[0] = Loader.TailCallHintKey;

                    entry[Loader.TailCallHintReferenceIndex] = addrReference.ToString();
                    entry[Loader.TailCallHintCallIndex] = addrCall.ToString();

                    if (symbol.Interpretation == SymbolInterpretations.Function)
                        entry[Loader.TailCallHintTypeIndex] = "func";
                    else if (symbol.Interpretation == SymbolInterpretations.Command)
                        entry[Loader.TailCallHintTypeIndex] = "cmd";
                    else 
                        return; //don't add cil compiler hint as there is no more efficient implementation for non-engine calls (e.g., indirect calls)
                    entry[Loader.TailCallHintSymbolIndex] = symbol.Id;

                    //Add hint to the meta table
                    if (target.Meta.ContainsKey(Loader.CilHintsKey))
                        target.Meta.AddTo(Loader.CilHintsKey, (MetaEntry)entry);
                    else
                        target.Meta[Loader.CilHintsKey] = (MetaEntry)new MetaEntry[] { (MetaEntry)entry };

                    //Add hooks for address changes
                    target.AddressChangeHooks.Add(addrReference,
                        delegate(int newAddr)
                        {
                            foreach(MetaEntry hintEntry in target.Meta[Loader.CilHintsKey].List)
                            {
                                MetaEntry[] hint = hintEntry.List;
                                if(hint[0] == Loader.TailCallHintKey && hint[Loader.TailCallHintReferenceIndex].Text == addrReference.ToString())
                                    hint[Loader.TailCallHintReferenceIndex] = newAddr.ToString();
                            }
                        });
                    target.AddressChangeHooks.Add(addrCall,
                        delegate(int newAddr)
                        {
                            foreach (MetaEntry hintEntry in target.Meta[Loader.CilHintsKey].List)
                            {
                                MetaEntry[] hint = hintEntry.List;
                                if (hint[0] == Loader.TailCallHintKey && hint[Loader.TailCallHintCallIndex].Text == addrCall.ToString())
                                    hint[Loader.TailCallHintCallIndex] = newAddr.ToString();
                            }
                        });
//*/
                }
            }
        }

        private static bool check_if_stackless_function_recursion_is_possible(CompilerTarget target, AstGetSetSymbol symbol)
        {
            if(symbol.Interpretation != SymbolInterpretations.Function) //must be function call
                return false;
            if(!Engine.StringsAreEqual(target.Function.Id,symbol.Id)) //must be direct recursive iteration
                return false;
            if(target.Function.Variables.Contains(PFunction.ArgumentListId)) //must not use argument list
                return false;
            if(symbol.Arguments.Count > target.Function.Parameters.Count) //must not supply more arguments than mapped
                return false;
            return true;
        }

        private bool optimize_conditional_return_expression(CompilerTarget target)
        {
            AstConditionalExpression cond = Expression as AstConditionalExpression;
            if (cond == null)
                return false;

            //  return  if( cond )
            //              expr1
            //          else
            //              expr2
            AstCondition retif = new AstCondition(File, Line, Column, cond.Condition);

            AstReturn ret1 = new AstReturn(File, Line, Column, ReturnVariant);
            ret1.Expression = cond.IfExpression;
            retif.IfBlock.Add(ret1);

            AstReturn ret2 = new AstReturn(File, Line, Column, ReturnVariant);
            ret2.Expression = cond.ElseExpression;
            //not added to the condition

            retif.EmitCode(target); //  if( cond )
            //      return expr1
            ret2.EmitCode(target); //  return expr2

            //ret1 and ret2 will continue optimizing
            return true;
        }

        public override string ToString()
        {
            string format = "";
            switch (ReturnVariant)
            {
                case ReturnVariant.Exit:
                    format = Expression != null ? "return {0};" : "return;";
                    break;
                case ReturnVariant.Set:
                    format = "return = {0};";
                    break;
                case ReturnVariant.Continue:
                    format = "continue;";
                    break;
                case ReturnVariant.Break:
                    format = "break;";
                    break;
            }
            return String.Format(format, Expression);
        }
    }

    public enum ReturnVariant
    {
        Exit,
        Set,
        Break,
        Continue
    }
}