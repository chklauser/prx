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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstGetSet : AstNode,
                                      IAstEffect,
                                      IAstHasExpressions
    {

        private List<IAstExpression> _arguments = new List<IAstExpression>();
        private readonly ArgumentsProxy _proxy;

        public ArgumentsProxy Arguments
        {
            [DebuggerNonUserCode()]
            get { return _proxy; }
        }

        #region IAstHasExpressions Members

        public virtual IAstExpression[] Expressions
        {
            get { return Arguments.ToArray(); }
        }

        #endregion

        public PCall Call;
        public BinaryOperator SetModifier;

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
            IAstExpression oArg;
            foreach (IAstExpression arg in _arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException(
                        "Invalid (null) argument in GetSet node (" + ToString() +
                        ") detected at position " + _arguments.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = _arguments.IndexOf(arg);
                    _arguments.Insert(idx, oArg);
                    _arguments.RemoveAt(idx + 1);
                }
            }

            return false;
        }

        protected virtual int DefaultAdditionalArguments
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
            switch (Call)
            {
                case PCall.Get:
                    EmitArguments(target);
                    EmitGetCode(target, justEffect);
                    break;
                case PCall.Set:
                    if (SetModifier == BinaryOperator.Coalescence)
                    {
                        AstGetSet assignment = GetCopy();
                        assignment.SetModifier = BinaryOperator.None;

                        AstGetSet getVariation = GetCopy();
                        getVariation.Call = PCall.Get;
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);

                        AstTypecheck check =
                            new AstTypecheck(
                                File,
                                Line,
                                Column,
                                getVariation,
                                new AstConstantTypeExpression(File, Line, Column, "Null"));

                        if (justEffect)
                        {
                            //Create a traditional condition
                            AstCondition cond = new AstCondition(File, Line, Column, check);
                            cond.IfBlock.Add(assignment);
                            cond.EmitCode(target);
                        }
                        else
                        {
                            //Create a conditional expression
                            AstConditionalExpression cond = new AstConditionalExpression(File, Line, Column, check);
                            cond.IfExpression = assignment;
                            cond.ElseExpression = getVariation;
                            cond.EmitCode(target);
                        }
                    }
                    else if(SetModifier == BinaryOperator.Cast)
                    {
                        // a(x,y) ~= T         //a(x,y,~T)~=
                        //to
                        // a(x,y) = a(x,y)~T   //a(x,y,a(x,y)~T)=
                        AstGetSet assignment = GetCopy(); //a'(x,y,~T)~=
                        assignment.SetModifier = BinaryOperator.None; //a'(x,y,~T)=

                        AstGetSet getVariation = assignment.GetCopy(); //a''(x,y,~T)=
                        getVariation.Call = PCall.Get; //a''(x,y,~String)
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1); //a''(x,y)

                        IAstType T = assignment.Arguments[assignment.Arguments.Count -1] as IAstType; //~T
                        if (T == null)
                            throw new PrexoniteException(
                                String.Format(
                                    "The right hand side of a cast operation must be a type expression (in {0} on line {1}).",
                                    File,
                                    Line));
                        assignment.Arguments[assignment.Arguments.Count -1] =
                            new AstTypecast(File, Line, Column, getVariation, T); //a(x,y,a(x,y)~T)=
                        assignment.EmitCode(target, justEffect);
                    }
                    else if (SetModifier != BinaryOperator.None)
                    {
                        //Without more detailed information, a Set call with a set modifier has to be expressed using 
                        //  conventional set call and binary operator nodes.
                        //Note that code generator for this original node is completely bypassed.
                        AstGetSet assignment = GetCopy();
                        assignment.SetModifier = BinaryOperator.None;
                        AstGetSet getVariation = GetCopy();
                        getVariation.Call = PCall.Get;
                        getVariation._arguments.RemoveAt(getVariation._arguments.Count - 1);
                        assignment._arguments[assignment._arguments.Count - 1] =
                            new AstBinaryOperator(
                                File,
                                Line,
                                Column,
                                getVariation,
                                SetModifier,
                                _arguments[_arguments.Count - 1]);
                        assignment.EmitCode(target, justEffect);
                    }
                    else
                    {
                        EmitArguments(target,!justEffect);
                        EmitSetCode(target);
                    }
                    break;
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
                "{0}{2}: {1}",
                Enum.GetName(typeof(PCall), Call).ToLowerInvariant(),
                (typeName = GetType().Name).StartsWith("AstGetSet")
                    ? typeName.Substring(9)
                    : typeName,
                SetModifier != BinaryOperator.None
                    ? "(" + Enum.GetName(typeof(BinaryOperator), SetModifier) + ")"
                    : "");
        }

        protected string ArgumentsToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("(");
            foreach (IAstExpression expr in Arguments)
                if (expr != null)
                    buffer.Append(expr + ", ");
                else
                    buffer.Append("{null}, ");
            return buffer + ")";
        }

        protected virtual void CopyBaseMembers(AstGetSet target)
        {
            target._arguments.AddRange(_arguments);
        }
    }
}