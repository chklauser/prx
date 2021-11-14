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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast;

public class AstBlock : AstExpr,
    IList<AstNode>, IAstHasExpressions
{
    #region Construction

    protected AstBlock(ISourcePosition position, [NotNull] SymbolStore symbols, string uid = null, string prefix = null)
        : base(position)
    {
        _prefix = (prefix ?? DefaultPrefix) + "\\";
        BlockUid = string.IsNullOrEmpty(uid) ? "\\" + Guid.NewGuid().ToString("N") : uid; 
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
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

    /// <summary>
    /// Symbol table for the scope of this block.
    /// </summary>
    [NotNull]
    public SymbolStore Symbols { get; private set; }

    /// <summary>
    /// Replaces symbol store backing this scope. Does not affect existing nested scopes!
    /// </summary>
    /// <param name="newStore">The new symbol store.</param>
    internal void _ReplaceSymbols([NotNull] SymbolStore newStore)
    {
        Symbols = newStore;
    }

    [NotNull]
    private List<AstNode> _statements = new();

    [NotNull]
    public List<AstNode> Statements
    {
        get => _statements;
        set => _statements = value ?? throw new ArgumentNullException(nameof(value));
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        EmitCode(target, false, stackSemantics);
    }

    public void EmitCode(CompilerTarget target, bool isTopLevel, StackSemantics stackSemantics)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target), "The compiler target cannot be null");

        if (isTopLevel)
            _tailCallOptimizeTopLevelBlock();

        foreach (var node in _statements)
        {
            var stmt = node;
            if (stmt is AstExpr expr)
                stmt = _GetOptimizedNode(target, expr);

            stmt.EmitEffectCode(target);
        }

        Expression?.EmitCode(target, stackSemantics);
    }

    #region Tail call optimization

    private static void tail_call_optimize_expressions_of_nested_block(
        IAstHasExpressions hasExpressions)
    {
        foreach (var expression in hasExpressions.Expressions)
        {
            if (expression is AstBlock blockItself)
                blockItself._tailCallOptimizeNestedBlock();
            else if (expression is IAstHasExpressions hasExpressionsItself)
                tail_call_optimize_expressions_of_nested_block(hasExpressionsItself);
            else if (expression is IAstHasBlocks hasBlocksItself)
                _tailCallOptimizeAllNestedBlocksOf(hasBlocksItself);
        }
    }

    private void _tailCallOptimizeNestedBlock()
    {
        int i;
        for (i = 1; i < _statements.Count; i++)
        {
            var stmt = _statements[i];

            switch (stmt)
            {
                case AstReturn {Expression: null} ret when (ret.ReturnVariant == ReturnVariant.Exit ||
                    ret.ReturnVariant == ReturnVariant.Continue) && _statements[i - 1] is AstGetSet:
                    //NOTE: Aggressive TCO disabled

                    //ret.Expression = getset;
                    //Statements.RemoveAt(i--);
                    break;
                case AstBlock blockItself:
                    blockItself._tailCallOptimizeNestedBlock();
                    break;
                case IAstHasBlocks hasBlocks:
                    _tailCallOptimizeAllNestedBlocksOf(hasBlocks);
                    break;
                case IAstHasExpressions hasExpressions:
                    tail_call_optimize_expressions_of_nested_block(hasExpressions);
                    break;
            }
        }
    }

    private static void _tailCallOptimizeAllNestedBlocksOf(IAstHasBlocks hasBlocks)
    {
        foreach (var block in hasBlocks.Blocks)
            block._tailCallOptimizeNestedBlock();
    }

    private void _tailCallOptimizeTopLevelBlock()
    {
        // { GetSetComplex; return; } -> { return GetSetComplex; }

        _tailCallOptimizeNestedBlock();
        AstGetSet getset;

        if (_statements.Count == 0)
            return;
        var lastStmt = _statements[^1];
        AstCondition cond;

        // { if(cond) block1 else block2 } -> { if(cond) block1' else block2' }
        if ((cond = lastStmt as AstCondition) != null)
        {
            cond.IfBlock._tailCallOptimizeTopLevelBlock();
            cond.ElseBlock._tailCallOptimizeTopLevelBlock();
        }
        // { ...; GetSet(); } -> { ...; return GetSet(); }
        else if ((getset = lastStmt as AstGetSet) != null)
        {
            var ret = new AstReturn(getset.File, getset.Line, getset.Column, ReturnVariant.Exit)
            {
                Expression = getset
            };
            _statements[^1] = ret;
        }
    }

    #endregion

    public virtual bool IsEmpty => Count == 0;

    public virtual bool IsSingleStatement => Count == 1;

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
            throw new ArgumentNullException(nameof(item));
        _statements.Insert(index, item);
    }

    public void InsertRange(int index, IEnumerable<AstNode> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));
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
        get => _statements[index];
        [DebuggerStepThrough]
        set => _statements[index] = value ?? throw new ArgumentNullException(nameof(value));
    }

    #endregion

    #region ICollection<AstNode> Members

    [DebuggerStepThrough]
    public void Add(AstNode item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        _statements.Add(item);
    }

    [DebuggerStepThrough]
    public void AddRange(IEnumerable<AstNode> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));
        foreach (var node in collection)
        {
            if (node == null)
                throw new ArgumentException(
                    "AstNode collection may not contain null.", nameof(collection));
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
            throw new ArgumentNullException(nameof(item));
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
        get => _statements.Count;
    }

    public bool IsReadOnly
    {
        [DebuggerStepThrough]
        get => ((IList<AstNode>) _statements).IsReadOnly;
    }

    [DebuggerStepThrough]
    public bool Remove(AstNode item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
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

    private readonly string _prefix;
    public AstExpr Expression;

    public string Prefix => _prefix.Substring(0, _prefix.Length - 1);

    public string BlockUid { get; }

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
        return string.Concat(_prefix, verb, BlockUid);
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
        return new(position,symbols,uid, prefix);
    }
}