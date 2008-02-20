using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstModifyingAssignment : AstNode, IAstExpression, IAstHasExpressions, IAstEffect
    {
        public BinaryOperator SetModifier;
        public AstGetSet ModifyingAssignment;

        public AstModifyingAssignment(string file, int line, int column, BinaryOperator setModifier, AstGetSet complex)
            : base(file, line, column)
        {
            SetModifier = setModifier;
            ModifyingAssignment = complex;
        }

        internal AstModifyingAssignment(Parser p, BinaryOperator setModifier, AstGetSet complex)
            : this(p.scanner.File, p.t.line, p.t.col,setModifier, complex)
        {
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[]{ModifyingAssignment}; }
        }

        #endregion

        #region IAstExpression Members

        public bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            IAstExpression newAssignment;
            if (ModifyingAssignment.TryOptimize(target, out newAssignment) && newAssignment is AstGetSet)
                ModifyingAssignment = (AstGetSet) newAssignment;
            expr = null;
            return false;
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            EmitCode(target, false);
        }

        public void EmitCode(CompilerTarget target, bool justEffect)
        {
            switch(SetModifier)
            {
                case BinaryOperator.Coalescence:
                    {
                        AstGetSet assignment = ModifyingAssignment.GetCopy();

                        AstGetSet getVariation = ModifyingAssignment.GetCopy();
                        getVariation.Call = PCall.Get;
                        //remove last argument (the assigned value)
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
                            AstConditionalExpression cond =
                                new AstConditionalExpression(File, Line, Column, check);
                            cond.IfExpression = assignment;
                            cond.ElseExpression = getVariation;
                            cond.EmitCode(target);
                        }
                    }
                    break;
                case BinaryOperator.Cast:
                    {
                        // a(x,y) ~= T         //a(x,y,~T)~=
                        //to
                        // a(x,y) = a(x,y)~T   //a(x,y,a(x,y)~T)=
                        AstGetSet assignment = ModifyingAssignment.GetCopy(); //a'(x,y,~T)~=

                        AstGetSet getVariation = assignment.GetCopy(); //a''(x,y,~T)=
                        getVariation.Call = PCall.Get; //a''(x,y,~String)
                        getVariation.Arguments.RemoveAt(getVariation.Arguments.Count - 1);
                            //a''(x,y)

                        IAstType T =
                            assignment.Arguments[assignment.Arguments.Count - 1] as IAstType; //~T
                        if (T == null)
                            throw new PrexoniteException(
                                String.Format(
                                    "The right hand side of a cast operation must be a type expression (in {0} on line {1}).",
                                    File,
                                    Line));
                        assignment.Arguments[assignment.Arguments.Count - 1] =
                            new AstTypecast(File, Line, Column, getVariation, T); //a(x,y,a(x,y)~T)=
                        if(justEffect)
                            assignment.EmitEffectCode(target);
                        else 
                            assignment.EmitCode(target);
                    }
                    break;
                case BinaryOperator.None:
                    if (justEffect)
                        ModifyingAssignment.EmitEffectCode(target);
                    else
                        ModifyingAssignment.EmitCode(target);
                    break;
                default: // +=, *= etc.
                    {
                        //Without more detailed information, a Set call with a set modifier has to be expressed using 
                        //  conventional set call and binary operator nodes.
                        //Note that code generator for this original node is completely bypassed.
                        AstGetSet assignment = ModifyingAssignment.GetCopy();
                        AstGetSet getVersion = ModifyingAssignment.GetCopy();
                        getVersion.Call = PCall.Get;
                        getVersion.Arguments.RemoveAt(getVersion.Arguments.Count - 1);
                        assignment.Arguments[assignment.Arguments.Count - 1] =
                            new AstBinaryOperator(
                                File,
                                Line,
                                Column,
                                getVersion,
                                SetModifier,
                                ModifyingAssignment.Arguments[ModifyingAssignment.Arguments.Count - 1]);
                        if (justEffect)
                            assignment.EmitEffectCode(target);
                        else
                            assignment.EmitCode(target);
                    }
                    break;
            }
        }

        #region IAstEffect Members

        public void EmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, true);
        }

        #endregion
    }
}
