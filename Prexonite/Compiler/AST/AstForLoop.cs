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

using System.Collections.Generic;
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstForLoop : AstLoop
    {
        [DebuggerStepThrough]
        public AstForLoop(string file, int line, int column)
            : base(file, line, column)
        {
            Block = new AstLoopBlock(file, line, column);
            Initialize = new AstBlock(file, line, column);
            NextIteration = new AstBlock(file, line, column);
        }

        [DebuggerStepThrough]
        internal AstForLoop(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        public IAstExpression Condition;
        public AstBlock Initialize;
        public AstBlock NextIteration;
        public bool IsPositive = true;
        public bool IsPrecondition = true;

        public bool IsInitialized
        {
            [DebuggerStepThrough]
            get { return Condition != null; }
        }

        public override void EmitCode(CompilerTarget target)
        {
            if (!IsInitialized)
                throw new PrexoniteException("AstForLoop requires Condition to be set.");

            //Optimize unary not condition
            OptimizeNode(target, ref Condition);
            var unaryCond = Condition as AstUnaryOperator;
            while (unaryCond != null && unaryCond.Operator == UnaryOperator.LogicalNot)
            {
                Condition = unaryCond.Operand;
                IsPositive = !IsPositive;
                unaryCond = Condition as AstUnaryOperator;
            }

            //Constant conditions
            var conditionIsConstant = false;
            if (Condition is AstConstant)
            {
                var constCond = (AstConstant) Condition;
                PValue condValue;
                if (
                    !constCond.ToPValue(target).TryConvertTo(
                         target.Loader, PType.Bool, out condValue))
                    goto continueFull;
                else if ((bool) condValue.Value == IsPositive)
                    conditionIsConstant = true;
                else
                {
                    //Condition is always false
                    return;
                }
            }
            continueFull:

            var conditionLabel = Block.CreateLabel("condition");

            if (!Block.IsEmpty) //Body exists -> complete loop code?
            {
                if (conditionIsConstant) //Infinite, hopefully user managed, loop ->
                {
                    /*  {init}
                     *  begin:
                     *  {block}
                     *  continue:
                     *  {next}
                     *  jump -> begin
                     */
                    target.BeginBlock(Block);
                    Initialize.EmitCode(target);
                    if (!IsPrecondition) //start with nextIteration
                        target.EmitJump(this, Block.ContinueLabel);
                    target.EmitLabel(this, Block.BeginLabel);
                    Block.EmitCode(target);
                    target.EmitLabel(this, Block.ContinueLabel);
                    NextIteration.EmitCode(target);
                    target.EmitJump(this, Block.BeginLabel);
                    target.EndBlock();
                }
                else //Variable condition and body -> full loop code
                {
                    /*  {init}
                     *  jump -> condition
                     *  begin:
                     *  {block}
                     *  continue:
                     *  {next}
                     *  condition:
                     *  {condition}
                     *  jump if true -> begin
                     */
                    target.BeginBlock(Block);
                    Initialize.EmitCode(target);
                    if (IsPrecondition)
                        target.EmitJump(this, conditionLabel);
                    else
                        target.EmitJump(this, Block.ContinueLabel);
                    target.EmitLabel(this, Block.BeginLabel);
                    Block.EmitCode(target);
                    target.EmitLabel(this, Block.ContinueLabel);
                    NextIteration.EmitCode(target);
                    target.EmitLabel(this, conditionLabel);
                    AstLazyLogical.EmitJumpCondition(
                        target, Condition, Block.BeginLabel, IsPositive);
                    target.EndBlock();
                }
            }
            else //Body does not exist -> Condition loop
            {
                /*  {init}
                 *  begin:
                 *  {cond}
                 *  jump if false -> break
                 *  continue:
                 *  {next}
                 *  jump -> begin
                 */
                target.BeginBlock(Block);
                Initialize.EmitCode(target);
                if (!IsPrecondition)
                    target.EmitJump(this, Block.ContinueLabel);
                target.EmitLabel(this, Block.BeginLabel);
                AstLazyLogical.EmitJumpCondition(target, Condition, Block.BreakLabel, !IsPositive);
                if (IsPrecondition)
                    target.EmitLabel(this, Block.ContinueLabel);
                NextIteration.EmitCode(target);
                target.EmitJump(this, Block.BeginLabel);
                target.EndBlock();
            }

            target.EmitLabel(this, Block.BreakLabel);
        }

        public override AstBlock[] Blocks
        {
            get
            {
                var blocks = new List<AstBlock>(base.Blocks)
                {
                    Initialize,
                    NextIteration
                };
                return blocks.ToArray();
            }
        }

        #region IAstHasExpressions Members

        public override IAstExpression[] Expressions
        {
            get { return new[] {Condition}; }
        }

        #endregion
    }
}