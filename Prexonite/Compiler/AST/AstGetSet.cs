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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstGetSet : AstNode,
                                      IAstEffect,
                                      IAstHasExpressions
    {
        private readonly List<IAstExpression> _arguments = new List<IAstExpression>();
        private readonly ArgumentsProxy _proxy;

        public ArgumentsProxy Arguments
        {
            [DebuggerNonUserCode]
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public virtual IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        private PCall _call;

        /// <summary>
        ///     <para>Indicates whether this node uses get or set syntax</para>
        ///     <para>(set syntax involves an equal sign (=); get syntax does not)</para>
        /// </summary>
        public virtual PCall Call
        {
            get { return _call; }
            set { _call = value; }
        }

        #endregion

        protected AstGetSet(string file, int line, int column, PCall call)
            : base(file, line, column)
        {
            _call = call;
            _proxy = new ArgumentsProxy(_arguments);
        }

        internal AstGetSet(Parser p, PCall call)
            : this(p.scanner.File, p.t.line, p.t.col, call)
        {
        }

        #region IAstExpression Members

        public virtual bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            expr = null;

            //Optimize arguments
            for (var i = 0; i < _arguments.Count; i++)
            {
                var arg = _arguments[i];
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                            ") detected at position " + _arguments.IndexOf(arg) + ".");
                var oArg = _GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                    _arguments[i] = oArg;
            }

            return false;
        }

        public virtual int DefaultAdditionalArguments
        {
            get { return 0; }
        }

        protected void EmitArguments(CompilerTarget target)
        {
            EmitArguments(target, false, DefaultAdditionalArguments);
        }

        protected void EmitArguments(CompilerTarget target, bool duplicateLast)
        {
            EmitArguments(target, duplicateLast, DefaultAdditionalArguments);
        }

        protected void EmitArguments(CompilerTarget target, bool duplicateLast,
            int additionalArguments)
        {
            Object lastArg = null;
            foreach (IAstExpression expr in Arguments)
            {
                Debug.Assert(expr != null,
                    "Argument list of get-set-complex contains null reference");
                if (ReferenceEquals(lastArg, expr))
                    target.EmitDuplicate(this);
                else
                    expr.EmitCode(target);
                lastArg = expr;
            }
            var argc = Arguments.Count;
            if (duplicateLast && argc > 0)
            {
                target.EmitDuplicate(this);
                if (argc + additionalArguments > 1)
                    target.EmitRotate(this, -1, argc + 1 + additionalArguments);
            }
        }

        void IAstEffect.DoEmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, true);
        }

        protected virtual void EmitCode(CompilerTarget target, bool justEffect)
        {
            if (Call == PCall.Get)
            {
                EmitArguments(target);
                EmitGetCode(target, justEffect);
            }
            else
            {
                EmitArguments(target, !justEffect);
                EmitSetCode(target);
            }
        }

        protected override sealed void DoEmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }

        protected abstract void EmitGetCode(CompilerTarget target, bool justEffect);
        protected abstract void EmitSetCode(CompilerTarget target);

        #endregion

        public abstract AstGetSet GetCopy();

        public override string ToString()
        {
            string typeName;
            return String.Format(
                "{0}: {1}",
                Enum.GetName(typeof (PCall), Call).ToLowerInvariant(),
                (typeName = GetType().Name).StartsWith("AstGetSet")
                    ? typeName.Substring(9)
                    : typeName);
        }

        protected string ArgumentsToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("(");
            var i = 0;
            foreach (IAstExpression expr in Arguments)
            {
                i++;

                if (expr != null)
                    buffer.Append(expr);
                else
                    buffer.Append("{null}");

                if (i != Arguments.Count)
                    buffer.Append(", ");
            }
            return buffer + ")";
        }

        /// <summary>
        ///     Copies the base class fields from this to the target.
        /// </summary>
        /// <param name = "target">The object that shall reveice the values from this object.</param>
        protected virtual void CopyBaseMembers(AstGetSet target)
        {
            target._arguments.AddRange(_arguments);
        }

        public override bool CheckForPlaceholders()
        {
            return this is IAstPartiallyApplicable &&
                (base.CheckForPlaceholders() || Arguments.Any(AstPartiallyApplicable.IsPlaceholder));
        }
    }
}