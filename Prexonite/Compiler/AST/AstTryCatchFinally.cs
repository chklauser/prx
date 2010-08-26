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
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstTryCatchFinally : AstNode,
                                      IAstHasBlocks
    {
        public AstBlock TryBlock;
        public AstBlock CatchBlock;
        public AstBlock FinallyBlock;
        public AstGetSet ExceptionVar;

        public AstTryCatchFinally(string file, int line, int column)
            : base(file, line, column)
        {
            TryBlock = new AstBlock(file, line, column);
            CatchBlock = new AstBlock(file, line, column);
            FinallyBlock = new AstBlock(file, line, column);
        }

        internal AstTryCatchFinally(Parser p)
            : base(p)
        {
            TryBlock = new AstBlock(p);
            CatchBlock = new AstBlock(p);
            FinallyBlock = new AstBlock(p);
        }

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get { return new[] {TryBlock, CatchBlock, FinallyBlock}; }
        }

        #endregion

        protected override void DoEmitCode(CompilerTarget target)
        {
            var prefix = "try\\" + Guid.NewGuid().ToString("N") + "\\";
            var beginTryLabel = prefix + "beginTry";
            var beginFinallyLabel = prefix + "beginFinally";
            var beginCatchLabel = prefix + "beginCatch";
            var endTry = prefix + "endTry";

            if (TryBlock.IsEmpty)
                if (FinallyBlock.IsEmpty)
                    return;
                else
                {
                    //The finally block is not protected
                    //  A trycatchfinally with just a finally block is equivalent to the contents of the finally block
                    //  " try {} finally { $code } " => " $code "
                    FinallyBlock.EmitCode(target);
                    return;
                }

            //Emit try block
            target.EmitLabel(this, beginTryLabel);
            target.Emit(this, OpCode.@try);
            TryBlock.EmitCode(target);

            //Emit finally block
            target.EmitLabel(FinallyBlock, beginFinallyLabel);
            var beforeEmit = target.Code.Count;
            FinallyBlock.EmitCode(target);
            if(FinallyBlock.Count > 0 && target.Code.Count == beforeEmit)
                target.Emit(FinallyBlock, OpCode.nop);
            target.EmitLeave(FinallyBlock, endTry);

            //Emit catch block
            target.EmitLabel(CatchBlock, beginCatchLabel);
            var usesException = ExceptionVar != null;
            var justRethrow = CatchBlock.IsEmpty && !usesException;

            if (usesException)
            {
                //Assign exception
                ExceptionVar = GetOptimizedNode(target, ExceptionVar) as AstGetSet ?? ExceptionVar;
                ExceptionVar.Arguments.Add(new AstGetException(File, Line, Column));
                ExceptionVar.Call = PCall.Set;
                ExceptionVar.EmitEffectCode(target);
            }

            if (!justRethrow)
            {
                //Exception handled
                CatchBlock.EmitCode(target);
            }
            else
            {
                //Exception not handled => rethrow.
                // * Rethrow is implemented in the runtime *
                //AstThrow th = new AstThrow(File, Line, Column);
                //th.Expression = new AstGetException(File, Line, Column);
                //th.EmitCode(target);
            }

            target.EmitLabel(this, endTry);

            var block =
                new TryCatchFinallyBlock(
                    _getAddress(target, beginTryLabel), _getAddress(target, endTry))
                {
                    BeginFinally = (!FinallyBlock.IsEmpty ? _getAddress(target, beginFinallyLabel) : -1),
                    BeginCatch = (!justRethrow ? _getAddress(target, beginCatchLabel) : -1),
                    UsesException = usesException
                };

            //Register try-catch-finally block
            target.Function.Meta.AddTo(TryCatchFinallyBlock.MetaKey, block);
            target.Function.InvalidateTryCatchFinallyBlocks();
        }

        private static int _getAddress(CompilerTarget target, string label)
        {
            int address;
            if (target.TryResolveLabel(label, out address))
                return address;
            else
                return -1;
        }
    }
}