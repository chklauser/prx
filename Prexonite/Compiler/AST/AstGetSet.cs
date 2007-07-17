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
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public abstract class AstGetSet : AstNode,
                                      IAstEffect
    {
        public List<IAstExpression> Arguments = new List<IAstExpression>();
        public PCall Call;
        public BinaryOperator SetModifier;

        protected AstGetSet(string file, int line, int column, PCall call)
            : base(file, line, column)
        {
            Call = call;
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
            foreach (IAstExpression arg in Arguments.ToArray())
            {
                if (arg == null)
                    throw new PrexoniteException("Invalid (null) argument in GetSet node (" + ToString() +
                                                 ") detected at position " + Arguments.IndexOf(arg) + ".");
                oArg = GetOptimizedNode(target, arg);
                if (!ReferenceEquals(oArg, arg))
                {
                    int idx = Arguments.IndexOf(arg);
                    Arguments.Insert(idx, oArg);
                    Arguments.RemoveAt(idx + 1);
                }
            }

            return false;
        }

        public void EmitArguments(CompilerTarget target)
        {
            foreach (IAstExpression expr in Arguments)
                expr.EmitCode(target);
        }

        public void EmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, true);
        }

        public virtual void EmitCode(CompilerTarget target, bool justEffect)
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

                        AstCondition cond = new AstCondition(File, Line, Column, check);
                        cond.IfBlock.Add(assignment);

                        cond.EmitCode(target);
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
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);
                        assignment.Arguments[assignment.Arguments.Count - 1] =
                            new AstBinaryOperator(File, Line, Column,
                                                  getVariation, SetModifier, Arguments[Arguments.Count - 1]);
                        assignment.EmitCode(target);
                    }
                    else
                    {
                        EmitArguments(target);
                        EmitSetCode(target);
                    }
                    break;
            }
        }

        public override sealed void EmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }

        public abstract void EmitGetCode(CompilerTarget target, bool justEffect);
        public abstract void EmitSetCode(CompilerTarget target);

        public void EmitGetCode(CompilerTarget target)
        {
            EmitGetCode(target, false);
        }

        void IAstExpression.EmitCode(CompilerTarget target)
        {
            PCall ocall = Call;
            Call = PCall.Get;
            try
            {
                EmitCode(target, false);    
            }
            finally
            {
                Call = ocall;
            }
        }

        #endregion

        public abstract AstGetSet GetCopy();

        public override string ToString()
        {
            string typeName;
            return String.Format("{0}{2}: {1}",
                                 Enum.GetName(typeof(PCall), Call).ToLowerInvariant(),
                                 (typeName = GetType().Name).StartsWith("AstGetSet") ? typeName.Substring(9) : typeName,
                                 SetModifier != BinaryOperator.None
                                     ? "(" + Enum.GetName(typeof(BinaryOperator), SetModifier) + ")"
                                     : "");
        }

        public string ArgumentsToString()
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
            target.Arguments = new List<IAstExpression>(Arguments.ToArray());
        }
    }
}