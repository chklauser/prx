using System;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    public class AstTryCatchFinally : AstNode, IAstHasBlocks
    {
        public AstBlock TryBlock;
        public AstBlock CatchBlock;
        public AstBlock FinallyBlock;
        public AstGetSet ExceptionVar = null;        
        
        public AstTryCatchFinally(string file, int line, int column)
            : base(file, line, column)
        {
            TryBlock = new AstBlock(file, line, column);
            CatchBlock = new AstBlock(file, line, column);
            FinallyBlock = new AstBlock(file, line, column);
        }

        internal  AstTryCatchFinally(Parser p)
            : base(p)
        {
            TryBlock = new AstBlock(p);
            CatchBlock = new AstBlock(p);
            FinallyBlock = new AstBlock(p);
        }

        #region IAstHasBlocks Members

        public AstBlock[] Blocks
        {
            get
            {
                return new AstBlock[] {TryBlock, CatchBlock, FinallyBlock};
            }
        }

        #endregion

        public override void EmitCode(CompilerTarget target)
        {
            string prefix = "try\\" + Guid.NewGuid().ToString("N")+  "\\";
            string beginTryLabel = prefix + "beginTry";
            string beginFinallyLabel = prefix + "beginFinally";
            string beginCatchLabel = prefix + "beginCatch";
            string endTry = prefix + "endTry";

            if(TryBlock.IsEmpty)
                if (FinallyBlock.IsEmpty)
                    return;
                else
                {
                    target.Emit(OpCode.@try);
                    FinallyBlock.EmitCode(target);
                }

            //Try block
            target.EmitLabel(beginTryLabel);
            target.Emit(OpCode.@try);
            TryBlock.EmitCode(target);

            //Finally block
            target.EmitLabel(beginFinallyLabel);
            FinallyBlock.EmitCode(target);
            target.EmitLeave(endTry);

            //Catch block
            target.EmitLabel(beginCatchLabel);
            if (ExceptionVar != null)
            {   //Assign exception
                ExceptionVar = GetOptimizedNode(target, ExceptionVar) as AstGetSet ?? ExceptionVar;
                ExceptionVar.Arguments.Add(new AstGetException(File, Line, Column));
                ExceptionVar.Call = PCall.Set;
                ExceptionVar.EmitCode(target);
            }

            if ((!CatchBlock.IsEmpty) || ExceptionVar != null)
            {   //Exception handled
                CatchBlock.EmitCode(target);
            }
            else
            {   //Exception not handled => rethrow.
                AstThrow th = new AstThrow(File, Line, Column);
                th.Expression = new AstGetException(File, Line, Column);
                th.EmitCode(target);
            }

            target.EmitLabel(endTry);

            TryCatchFinallyBlock block =
                new TryCatchFinallyBlock(_getAddress(target, beginTryLabel), _getAddress(target, endTry));

            block.BeginFinally = !FinallyBlock.IsEmpty ? _getAddress(target, beginFinallyLabel) : -1;
            block.BeginCatch = !CatchBlock.IsEmpty ? _getAddress(target, beginCatchLabel) : -1;
            block.UsesException = ExceptionVar != null;

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
