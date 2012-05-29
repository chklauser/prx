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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    public class AstBlock : AstExpr,
                            IList<AstNode>, IAstHasExpressions
    {
        #region Construction

        protected AstBlock(ISourcePosition position, [NotNull] SymbolStore symbols, string uid = null, string prefix = null)
            : base(position)
        {
            if (symbols == null)
                throw new ArgumentNullException("symbols");
            _prefix = (prefix ?? DefaultPrefix) + "\\";
            _uid = String.IsNullOrEmpty(uid) ? "\\" + Guid.NewGuid().ToString("N") : uid; 
            _symbols = symbols;
        }

        protected AstBlock(ISourcePosition position, AstBlock lexicalScope, string prefix = null, string uid = null)
            : this(position, _deriveSymbolStore(lexicalScope),uid, prefix)
        {   
        }

        private static SymbolStore _deriveSymbolStore(AstBlock parentBlock)
        {
            return SymbolStore.Create(parentBlock.Symbols);
        }

        #endregion

        [NotNull]
        private readonly SymbolStore _symbols;

        [NotNull]
        public SymbolStore Symbols
        {
            get { return _symbols; }
        }

        [NotNull]
        private List<AstNode> _statements = new List<AstNode>();

        [NotNull]
        public List<AstNode> Statements
        {
            get { return _statements; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _statements = value;
            }
        }

        protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            EmitCode(target, false, stackSemantics);
        }

        public void EmitCode(CompilerTarget target, bool isTopLevel, StackSemantics stackSemantics)
        {
            if (target == null)
                throw new ArgumentNullException("target", "The compiler target cannot be null");

            if (isTopLevel)
                tail_call_optimize_top_level_block();

            foreach (var node in _statements)
            {
                var stmt = node;
                var expr = stmt as AstExpr;
                if (expr != null)
                    stmt = _GetOptimizedNode(target, expr);

                stmt.EmitEffectCode(target);
            }

            if(Expression != null)
                Expression.EmitCode(target, stackSemantics);
        }

        #region Tail call optimization

        private static void tail_call_optimize_expressions_of_nested_block(
            IAstHasExpressions hasExpressions)
        {
            foreach (var expression in hasExpressions.Expressions)
            {
                var blockItself = expression as AstBlock;
                var hasExpressionsItself = expression as IAstHasExpressions;
                var hasBlocksItself = expression as IAstHasBlocks;

                if (blockItself != null)
                    blockItself.tail_call_optimize_nested_block();
                else if (hasExpressionsItself != null)
                    tail_call_optimize_expressions_of_nested_block(hasExpressionsItself);
                else if (hasBlocksItself != null)
                    tail_call_optimize_all_nested_blocks_of(hasBlocksItself);
            }
        }

        private void tail_call_optimize_nested_block()
        {
            int i;
            for (i = 1; i < _statements.Count; i++)
            {
                var stmt = _statements[i];
                var ret = stmt as AstReturn;
                var getset = _statements[i - 1] as AstGetSet;
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
                else if (blockItself != null)
                {
                    blockItself.tail_call_optimize_nested_block();
                }
                else if (hasBlocks != null)
                {
                    tail_call_optimize_all_nested_blocks_of(hasBlocks);
                }
                else if (hasExpressions != null)
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

            if (_statements.Count == 0)
                return;
            var lastStmt = _statements[_statements.Count - 1];
            AstCondition cond;

            // { if(cond) block1 else block2 } -> { if(cond) block1' else block2' }
            if ((cond = lastStmt as AstCondition) != null)
            {
                cond.IfBlock.tail_call_optimize_top_level_block();
                cond.ElseBlock.tail_call_optimize_top_level_block();
            }
                // { ...; GetSet(); } -> { ...; return GetSet(); }
            else if ((getset = lastStmt as AstGetSet) != null)
            {
                var ret = new AstReturn(getset.File, getset.Line, getset.Column, ReturnVariant.Exit)
                    {
                        Expression = getset
                    };
                _statements[_statements.Count - 1] = ret;
            }
        }

        #endregion

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
            return _statements.IndexOf(item);
        }

        [DebuggerStepThrough]
        public void Insert(int index, AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            _statements.Insert(index, item);
        }

        public void InsertRange(int index, IEnumerable<AstNode> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            _statements.InsertRange(index, items);
        }

        [DebuggerStepThrough]
        public void RemoveAt(int index)
        {
            _statements.RemoveAt(index);
        }

        public AstNode this[int index]
        {
            [DebuggerStepThrough]
            get { return _statements[index]; }
            [DebuggerStepThrough]
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _statements[index] = value;
            }
        }

        #endregion

        #region ICollection<AstNode> Members

        [DebuggerStepThrough]
        public void Add(AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            _statements.Add(item);
        }

        [DebuggerStepThrough]
        public void AddRange(IEnumerable<AstNode> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            foreach (var node in collection)
            {
                if (node == null)
                    throw new ArgumentException(
                        "AstNode collection may not contain null.", "collection");
            }
            _statements.AddRange(collection);
        }

        [DebuggerStepThrough]
        public void Clear()
        {
            _statements.Clear();
        }

        [DebuggerStepThrough]
        public bool Contains(AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return _statements.Contains(item);
        }

        [DebuggerStepThrough]
        public void CopyTo(AstNode[] array, int arrayIndex)
        {
            _statements.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            [DebuggerStepThrough]
            get { return _statements.Count; }
        }

        public bool IsReadOnly
        {
            [DebuggerStepThrough]
            get { return ((IList<AstNode>) _statements).IsReadOnly; }
        }

        [DebuggerStepThrough]
        public bool Remove(AstNode item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return _statements.Remove(item);
        }

        #endregion

        #region IEnumerable<AstNode> Members

        [DebuggerStepThrough]
        public IEnumerator<AstNode> GetEnumerator()
        {
            return _statements.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _statements.GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            var buffer = new StringBuilder();
            foreach (var node in _statements)
                buffer.AppendFormat("{0}; ", node);
            if (Expression != null)
                buffer.AppendFormat(" (return {0})", Expression);
            return buffer.ToString();
        }

        #region Block labels

        public bool JumpLabelsEnabled { get; set; }
        public AstBlock LexicalParentBlock { get; set; }

        private readonly string _prefix;
        private readonly string _uid;
        public AstExpr Expression;

        public string Prefix
        {
            get { return _prefix.Substring(0, _prefix.Length - 1); }
        }

        public string BlockUid
        {
            get { return _uid; }
        }

        public virtual AstExpr[] Expressions
        {
            get
            {
                if(Expression == null)
                    return new AstExpr[0];
                else
                    return new[] {Expression};
            }
        }

        public const string DefaultPrefix = "_";
        public const string RootBlockName = "root";

        public string CreateLabel(string verb)
        {
            return String.Concat(_prefix, verb, _uid);
        }

        #endregion

        public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
        {
            //Will be optimized after code generation, hopefully
            if (Expression != null)
                _OptimizeNode(target, ref Expression);

            expr = null;
            return false;
        }

        public static AstBlock CreateRootBlock(ISourcePosition position, SymbolStore symbols, string prefix, string uid)
        {
            return new AstBlock(position,symbols,uid, prefix);
        }
    }
}