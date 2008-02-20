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

using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstUsing : AstNode,
                            IAstHasBlocks,
                            IAstHasExpressions
    {
        private const string LabelPrefix = "using";

        internal AstUsing(Parser p)
            : base(p)
        {
            Block = new AstBlock(File, Line, Column);
            Labels = new BlockLabels(LabelPrefix);
        }

        public AstUsing(string file, int line, int column)
            : base(file, line, column)
        {
            Block = new AstBlock(File, Line, Column);
            Labels = new BlockLabels(LabelPrefix);
        }

        public IAstExpression Expression;
        public AstBlock Block;
        public BlockLabels Labels;

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return new AstBlock[] {Block}; }
        }

        #region IAstHasExpressions Members

        public IAstExpression[] Expressions
        {
            get { return new IAstExpression[] {Expression}; }
        }

        #endregion

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            if (Expression == null)
                throw new PrexoniteException("AstUsing requires Expression to be initialized.");

            AstTryCatchFinally _try = new AstTryCatchFinally(File, Line, Column);
            string vContainer = Labels.CreateLabel("container");
            target.Function.Variables.Add(vContainer);
            //Try block => Container = {Expression}; {Block};
            AstGetSetSymbol setCont =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Set,
                    vContainer,
                    SymbolInterpretations.LocalObjectVariable);
            setCont.Arguments.Add(Expression);

            AstGetSetSymbol getCont =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Get,
                    vContainer,
                    SymbolInterpretations.LocalObjectVariable);

            AstBlock _tryBlock = _try.TryBlock;
            _tryBlock.Add(setCont);
            _tryBlock.AddRange(Block);

            //Finally block => dispose( Container );
            AstGetSetSymbol dispose =
                new AstGetSetSymbol(
                    File,
                    Line,
                    Column,
                    PCall.Get,
                    Engine.DisposeCommand,
                    SymbolInterpretations.Command);

            dispose.Arguments.Add(getCont);

            _try.FinallyBlock.Add(dispose);

            //Emit code!
            _try.EmitCode(target);
        }
    }
}