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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstBlock : AstNode,
                            IList<AstNode>
    {
        [DebuggerStepThrough]
        public AstBlock(string file, int line, int column)
            : base(file, line, column)
        {
        }

        [DebuggerStepThrough]
        internal AstBlock(Parser p)
            : base(p)
        {
        }

        public List<AstNode> Statements = new List<AstNode>();

        public override void EmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }

        public void EmitCode(CompilerTarget target, bool isTopLevel)
        {
            if (target == null)
                throw new ArgumentNullException("target", "The compiler target cannot be null");

            if (isTopLevel)
                tail_call_optimize_top_level_block();

            foreach (var node in Statements)
            {
                var stmt = node;
                var expr = stmt as IAstExpression;
                if (expr != null)
                    stmt = (AstNode) GetOptimizedNode(target, expr);

                if (stmt is IAstEffect)
                    ((IAstEffect) stmt).EmitEffectCode(target);
                else
                    stmt.EmitCode(target);
            }
        }

        private static void tail_call_optimize_expressions_of_nested_block(IAstHasExpressions hasExpressions)
        {
            foreach (var expression in hasExpressions.Expressions)
            {
                var blockItself = expression as AstBlock;
                var hasExpressionsItself = expression as IAstHasExpressions;
                var hasBlocksItself = expression as IAstHasBlocks;

                if(blockItself != null)
                    blockItself.tail_call_optimize_nested_block();
                else if(hasExpressionsItself != null)
                    tail_call_optimize_expressions_of_nested_block(hasExpressionsItself);
                else if(hasBlocksItself != null)
                    tail_call_optimize_all_nested_blocks_of(hasBlocksItself);
            }
        }

        private void tail_call_optimize_nested_block()
        {
            int i;
            for (i = 1; i < Statements.Count; i++)
            {
                var stmt = Statements[i];
                var ret = stmt as AstReturn;
                var getset = Statements[i - 1] as AstGetSet;
                var hasBlocks = stmt as IAstHasBlocks;
                var hasExpressions = stmt as IAstHasExpressions;
                var blockItself = stmt as AstBlock;

                if (ret != null && ret.Expression == null &&
                    (ret.ReturnVariant == ReturnVariant.Exit ||
                     ret.ReturnVariant == ReturnVariant.Continue) && getset != null)
                {
                    //NOTE: Aggressive TCO disabled
                    
                    //ret.Expression = getset;
                    //Statements.RemoveAt(i--);
                }
                else if(blockItself != null)
                {
                    blockItself.tail_call_optimize_nested_block();
                }
                else if(hasBlocks != null)
                {
                    tail_call_optimize_all_nested_blocks_of(hasBlocks);
                }
                else if(hasExpressions != null)
                {
                    tail_call_optimize_expressions_of_nested_block(hasExpressions);
                }
            }
        }

        private static void tail_call_optimize_all_nested_blocks_of(IAstHasBlocks hasBlocks)
        {
            foreach (var block in hasBlocks.Blocks)
                block.tail_call_optimize_nested_block();
        }

        private void tail_call_optimize_top_level_block()
        {
            // { GetSetComplex; return; } -> { return GetSetComplex; }

            tail_call_optimize_nested_block();
            AstGetSet getset;

            if (Statements.Count == 0)
                return;
            var lastStmt = Statements[Statements.Count -1];
            AstCondition cond;

            // { if(cond) block1 else block2 } -> { if(cond) block1' else block2' }
            if ((cond = lastStmt as AstCondition) != null)
            {
                cond.IfBlock.tail_call_optimize_top_level_block();
                cond.ElseBlock.tail_call_optimize_top_level_block();
            }
            // { ...; GetSet(); } -> { ...; return GetSet(); }
            else if((getset = lastStmt as AstGetSet) != null)
            {
                var ret = new AstReturn(getset.File, getset.Line, getset.Column, ReturnVariant.Exit)
                {
                    Expression = getset
                };
                Statements[Statements.Count - 1] = ret;
            }
        }

        public virtual bool IsEmpty
        {
            get { return Count == 0; }
        }

        public virtual bool IsSingleStatement
        {
            get { return Count == 1; }
        }

        #region IList<AstNode> Members

        [DebuggerStepThrough]
        public int IndexOf(AstNode item)
        {
            return Statements.IndexOf(item);
        }

        [DebuggerStepThrough]
        public void Insert(int index, AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item"); 
            Statements.Insert(index, item);
        }

        [DebuggerStepThrough]
        public void RemoveAt(int index)
        {
            Statements.RemoveAt(index);
        }

        public AstNode this[int index]
        {
            [DebuggerStepThrough]
            get { return Statements[index]; }
            [DebuggerStepThrough]
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value"); 
                Statements[index] = value;
            }
        }

        #endregion

        #region ICollection<AstNode> Members

        [DebuggerStepThrough]
        public void Add(AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item"); 
            Statements.Add(item);
        }

        [DebuggerStepThrough]
        public void AddRange(IEnumerable<AstNode> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            foreach (AstNode node in collection)
            {
                if (node == null)
                    throw new ArgumentException(
                        "AstNode collection may not contain null.", "collection");
            }
            Statements.AddRange(collection);
        }

        [DebuggerStepThrough]
        public void Clear()
        {
            Statements.Clear();
        }

        [DebuggerStepThrough]
        public bool Contains(AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item"); 
            return Statements.Contains(item);
        }

        [DebuggerStepThrough]
        public void CopyTo(AstNode[] array, int arrayIndex)
        {
            Statements.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            [DebuggerStepThrough]
            get { return Statements.Count; }
        }

        public bool IsReadOnly
        {
            [DebuggerStepThrough]
            get { return ((IList<AstNode>) Statements).IsReadOnly; }
        }

        [DebuggerStepThrough]
        public bool Remove(AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item"); 
            return Statements.Remove(item);
        }

        #endregion

        #region IEnumerable<AstNode> Members

        [DebuggerStepThrough]
        public IEnumerator<AstNode> GetEnumerator()
        {
            return Statements.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Statements.GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            var buffer = new StringBuilder();
            foreach (var node in Statements)
                buffer.AppendFormat("{0} ;", node);
            return buffer.ToString();
        }
    }
}