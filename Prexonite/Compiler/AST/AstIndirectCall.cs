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
using System.Linq;
using Prexonite.Commands.Core.PartialApplication;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstIndirectCall : AstGetSet, IAstPartiallyApplicable
    {
        public IAstExpression Subject;

        public override IAstExpression[] Expressions
        {
            get
            {
                var len = Arguments.Count;
                var ary = new IAstExpression[len + 1];
                Array.Copy(Arguments.ToArray(), 0, ary, 1, len);
                ary[0] = Subject;
                return ary;
            }
        }

        public AstIndirectCall(
            string file, int line, int column, PCall call, IAstExpression subject)
            : base(file, line, column, call)
        {
            if (subject == null)
                throw new ArgumentNullException("subject");
            Subject = subject;
        }

        internal AstIndirectCall(Parser p, PCall call, IAstExpression subject)
            : this(p.scanner.File, p.t.line, p.t.col, call, subject)
        {
        }

        public AstIndirectCall(string file, int line, int column, PCall call)
            : this(file, line, column, call, null)
        {
        }

        public AstIndirectCall(string file, int line, int column)
            : this(file, line, column, PCall.Get)
        {
        }

        public AstIndirectCall(string file, int line, int column, IAstExpression subject)
            : this(file, line, column, PCall.Get, subject)
        {
        }

        internal AstIndirectCall(Parser p, IAstExpression subject)
            : this(p, PCall.Get, subject)
        {
        }

        public override int DefaultAdditionalArguments
        {
            get { return base.DefaultAdditionalArguments + 1; //include subject
            }
        }

        protected override void EmitCode(CompilerTarget target, bool justEffect)
        {
            Subject.EmitCode(target);
            base.EmitCode(target, justEffect);
        }

        protected override void EmitGetCode(CompilerTarget target, bool justEffect)
        {
            target.EmitIndirectCall(this, Arguments.Count, justEffect);
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            //Indirect set does not have a return value, therefore justEffect is true
            target.EmitIndirectCall(this, Arguments.Count, true);
        }

        public override bool TryOptimize(CompilerTarget target, out IAstExpression expr)
        {
            base.TryOptimize(target, out expr);
            _OptimizeNode(target, ref Subject);

            //Try to replace { ldloc var ; indarg.x } by { indloc.x var } (same for glob)
            var symbol = Subject as AstGetSetSymbol;
            if (symbol != null && symbol.IsObjectVariable)
            {
                var kind =
                    symbol.Interpretation == SymbolInterpretations.GlobalObjectVariable
                        ?
                            SymbolInterpretations.GlobalReferenceVariable
                        :
                            SymbolInterpretations.LocalReferenceVariable;
                var refcall =
                    new AstGetSetSymbol(File, Line, Column, Call, symbol.Id, kind);
                refcall.Arguments.AddRange(Arguments);
                expr = refcall;
                return true;
            }

            expr = null;
            return false;
        }

        public override AstGetSet GetCopy()
        {
            AstGetSet copy = new AstIndirectCall(File, Line, Column, Call, Subject);
            CopyBaseMembers(copy);
            return copy;
        }

        #region Implementation of IAstPartiallyApplicable

        void IAstPartiallyApplicable.DoEmitPartialApplicationCode(CompilerTarget target)
        {
            var argv =
                AstPartiallyApplicable.PreprocessPartialApplicationArguments(
                    Subject.Singleton().Append(Arguments));
            var argc = argv.Count;
            AstPlaceholder p;
            if(argc == 0)
            {
                //There are no mappings at all, use default constructor
                target.EmitConstant(this, 0);
                target.EmitCommandCall(this, 1, Engine.PartialCallAlias);
            }
            else if(argc == 1 && !argv[0].IsPlaceholder())
            {
                //We have just a call target, this is actually the identity function
                Subject.EmitCode(target);
            }
            else if(
                argc >= 2 
                && !argv[0].IsPlaceholder() 
                && argv.Skip(2).All(expr => !expr.IsPlaceholder())
                && ((p = argv[1] as AstPlaceholder) == null || p.Index == 0))
                //Matches the patterns 
                //  subj.(c_1, c_2,...,c_n, ?0,?1,?2,...,?m) 
                //and 
                //  subj.(?0, c_1,c_2,...,c_n, ?1,?2,?3,...,?m)
            {
                //This partial application was reduced to just closed arguments in prefix position
                //  with an optional open argument in front. No mapping is necessary in this case. 
                
                //Check for optional open argument
                if(p != null)
                {
                    //There is an open argument in front. This is handled by FlippedFunctionalPartialCall
                    argv[0].EmitCode(target);
                    foreach (var arg in argv.Skip(2))
                        arg.EmitCode(target);
                    target.EmitCommandCall(this, argc-1,FlippedFunctionalPartialCallCommand.Alias);
                }
                else
                {
                    //There is no open argument in front. This is implemented by FunctionalPartialCall
                    foreach (var arg in argv)
                        arg.EmitCode(target);
                    target.EmitCommandCall(this, argc, FunctionalPartialCallCommand.Alias);   
                }
            }
            else
            {
                //Use full-blown partial application mechanism for indirect calls.
                var ctorArgc = this.EmitConstructorArguments(target, argv);
                target.EmitCommandCall(this,ctorArgc, Engine.PartialCallAlias);
            }
        }

        public override bool CheckForPlaceholders()
        {
            return base.CheckForPlaceholders() || Subject is AstPlaceholder;
        }

        public override string ToString()
        {
            return string.Format("{0}: ({1}).{2}",
                                 Enum.GetName(typeof(PCall), Call).ToLowerInvariant(),
                                 Subject, ArgumentsToString());
        }

        #endregion
    }
}