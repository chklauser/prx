// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstTryCatchFinally : AstNode,
                                      IAstHasBlocks
    {
        public AstBlock TryBlock { get; set; }
        public AstBlock CatchBlock { get; set; }
        public AstBlock FinallyBlock { get; set; }
        public AstGetSet ExceptionVar { get; set; }

        public AstTryCatchFinally(string file, int line, int column)
            : base(file, line, column)
        {
            TryBlock = new AstSubBlock(file, line, column, this);
            CatchBlock = new AstSubBlock(file, line, column, this);
            FinallyBlock = new AstSubBlock(file, line, column, this);
        }

        internal AstTryCatchFinally(Parser p)
            : base(p)
        {
            TryBlock = new AstSubBlock(p, this);
            CatchBlock = new AstSubBlock(p, this);
            FinallyBlock = new AstSubBlock(p, this);
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
            if (FinallyBlock.Count > 0 && target.Code.Count == beforeEmit)
                target.Emit(FinallyBlock, OpCode.nop);
            target.EmitLeave(FinallyBlock, endTry);

            //Emit catch block
            target.EmitLabel(CatchBlock, beginCatchLabel);
            var usesException = ExceptionVar != null;
            var justRethrow = CatchBlock.IsEmpty && !usesException;

            if (usesException)
            {
                //Assign exception
                ExceptionVar = _GetOptimizedNode(target, ExceptionVar) as AstGetSet ?? ExceptionVar;
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
            target.Emit(this, OpCode.nop);

            var block =
                new TryCatchFinallyBlock(
                    _getAddress(target, beginTryLabel), _getAddress(target, endTry))
                    {
                        BeginFinally =
                            (!FinallyBlock.IsEmpty ? _getAddress(target, beginFinallyLabel) : -1),
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