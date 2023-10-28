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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

/// <summary>
///     <para>Wraps a get-set node in the new-declaration of a local variable.</para>
///     <para>Syntax:
///         <code>var new x</code> (iff <code>x</code> isn't the first reference to the variable in the current scope)</para>
///     <para>In addition to the supplied expression, the variables identity is changed (similar to the unbind command)</para>
/// </summary>
public sealed class AstGetSetNewDecl : AstGetSet
{
    PCall _fallbackCall;
    [CanBeNull]
    readonly ArgumentsProxy _arguments;

    public AstGetSetNewDecl([NotNull] ISourcePosition position, [NotNull] string id, [CanBeNull] AstGetSet expression)
        : base(position)
    {
        Expression = expression;
        Id = id ?? throw new ArgumentNullException(nameof(id));
        _arguments = Expression == null ? new ArgumentsProxy(new List<AstExpr>()) : null;
    }

    #region Overrides of AstGetSet

    public override AstExpr[] Expressions
    {
        get
        {
            var expr = Expression;
            return expr == null ? base.Expressions : Extensions.Append(base.Expressions, expr).ToArray();
        }
    }

    /// <summary>
    ///     Emits code responsible for changing the variables identity.
    /// </summary>
    /// <param name = "target">The target to compile to</param>
    void _emitNewDeclareCode(CompilerTarget target)
    {
        _ensureValid();
        //create command call
        //  unbind(->variable)
        var unlinkCall = new AstIndirectCall(Position, PCall.Get,
            new AstReference(Position, EntityRef.Command.Create(Engine.UnbindAlias)));
        var targetRef = new AstReference(Position, EntityRef.Variable.Local.Create(Id));
        unlinkCall.Arguments.Add(targetRef);

        //Optimize call and emit code
        var call = (AstExpr) unlinkCall;
        _OptimizeNode(target, ref call);
        call.EmitEffectCode(target);
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        // If we are wrapping an existing expression, then just forward calls directly to that expression
        // otherwise, pretend to be a GetSet node.
        if (Expression == null)
            base.DoEmitCode(target, stackSemantics);
        else
            _emitCode(target, stackSemantics);
    }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        _emitCode(target, stackSemantics);
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        _emitCode(target, StackSemantics.Effect);
    }

    void _emitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        _emitNewDeclareCode(target);
        if (Expression != null)
            Expression.EmitCode(target, stackSemantics);
        else if(_arguments != null)
        {
            // Make sure effects of attached expressions are compiled
            // This branch is unlikely to be ever taken. It is just there
            // to fulfill the GetSet contract.
            foreach (AstExpr arg in _arguments)
                arg.EmitEffectCode(target);
        }
    }

    public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
    {
        var wrappedExpr = (AstExpr) Expression;
        if (wrappedExpr != null)
        {
            _OptimizeNode(target, ref wrappedExpr);
            if (wrappedExpr is AstGetSet optExpr)
            {
                expr = new AstGetSetNewDecl(Position, Id, optExpr);
                return true;
            }
        }

        expr = null;
        return false;
    }

    public override AstGetSet GetCopy()
    {
        var expr2 = Expression?.GetCopy();
        var newDecl2 = new AstGetSetNewDecl(Position, Id, expr2);
        CopyBaseMembers(newDecl2);
        return newDecl2;
    }

    #endregion

    /// <summary>
    ///     <para>The expression wrapped by the new decl.</para>
    ///     <para>Other expressions are possible as well, though they make little sense wrapped by a new-declaration.</para>
    /// </summary>
    [CanBeNull]
    public AstGetSet Expression { get; }

    public override ArgumentsProxy Arguments
    {
        get
        {
            if (Expression != null) 
                return Expression.Arguments;
            else if (_arguments != null)
                return _arguments;
            else
                throw new InvalidOperationException($"The new-decl expression for {Id} is invalid.");
        }
    }

    public override PCall Call
    {
        get
        {
            var expr = Expression;
            if (expr == null)
                return _fallbackCall;
            else
                return expr.Call;
        }
        set
        {
            var expr = Expression;
            if (expr == null)
                _fallbackCall = value;
            else
                expr.Call = value;
        }
    }

    /// <summary>
    ///     The physical id of the local variable to be new-declared.
    /// </summary>
    [NotNull]
    public string Id { get; }

    void _ensureValid()
    {
        if (Id == null)
            throw new InvalidOperationException("NewDecl node must have a non-null id");
    }
}