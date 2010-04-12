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
using System.Diagnostics;
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

        #endregion

        public PCall Call;


        protected AstGetSet(string file, int line, int column, PCall call)
            : base(file, line, column)
        {
            Call = call;
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
            foreach (var arg in _arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                        ") detected at position " + _arguments.IndexOf(arg) + ".");
                var oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    var idx = _arguments.IndexOf(arg);
                    _arguments.Insert(idx, oArg);
                    _arguments.RemoveAt(idx + 1);
                }
            }

            return false;
        }

        public virtual int DefaultAdditionalArguments
        {
            get
            {
                return 0;
            }
        }

        protected void EmitArguments(CompilerTarget target)
        {
            EmitArguments(target, false, DefaultAdditionalArguments);
        }

        protected void EmitArguments(CompilerTarget target, bool duplicateLast)
        {
            EmitArguments(target, duplicateLast, DefaultAdditionalArguments);
        }

        protected void EmitArguments(CompilerTarget target, bool duplicateLast, int additionalArguments)
        {
            foreach (IAstExpression expr in Arguments)
                expr.EmitCode(target);
            int argc = Arguments.Count;
            if(duplicateLast && argc > 0)
            {
                target.EmitDuplicate();
                if(argc + additionalArguments > 1)
                    target.EmitRotate(-1, argc + 1 + additionalArguments);
            }
        }

        public void EmitEffectCode(CompilerTarget target)
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

        public override sealed void EmitCode(CompilerTarget target)
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
                Enum.GetName(typeof(PCall), Call).ToLowerInvariant(),
                (typeName = GetType().Name).StartsWith("AstGetSet")
                    ? typeName.Substring(9)
                    : typeName);
        }

        protected string ArgumentsToString()
        {
            var buffer = new StringBuilder();
            buffer.Append("(");
            foreach (IAstExpression expr in Arguments)
                if (expr != null)
                    buffer.Append(expr + ", ");
                else
                    buffer.Append("{null}, ");
            return buffer + ")";
        }

        /// <summary>
        /// Copies the base class fields from this to the target.
        /// </summary>
        /// <param name="target">The object that shall reveice the values from this object.</param>
        protected virtual void CopyBaseMembers(AstGetSet target)
        {
            target._arguments.AddRange(_arguments);
        }
    }
}