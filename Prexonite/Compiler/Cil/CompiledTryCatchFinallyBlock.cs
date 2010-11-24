using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Compiler.Cil
{
    /// <summary>
    /// Try-catch-finally block augmented for CIL compilation. Has no relevance for the Prexonite interpreter.
    /// </summary>
    public class CompiledTryCatchFinallyBlock : TryCatchFinallyBlock
    {
        public CompiledTryCatchFinallyBlock(TryCatchFinallyBlock block)
        {
            BeginTry = block.BeginTry;
            BeginCatch = block.BeginCatch;
            BeginFinally = block.BeginFinally;
            EndTry = block.EndTry;
        }

        public static CompiledTryCatchFinallyBlock Create(TryCatchFinallyBlock block)
        {
            return new CompiledTryCatchFinallyBlock(block);
        }

        /// <summary>
        /// The label that marks the dedicated leave instruction for this try-block.
        /// </summary>
        public System.Reflection.Emit.Label SkipTryLabel { get; set; }

        public bool IsInTryBlock(int address)
        {
            return Handles(address);
        }

        public bool IsInFinallyBlock(int address)
        {
            if(!HasFinally)
                return false;
            if (HasCatch)
                return BeginFinally <= address && address < BeginCatch;
            else
                return BeginFinally <= address && address < EndTry;
        }

        public bool IsInCatchBlock(int address)
        {
            if(!HasCatch)
                return false;
            return BeginCatch <= address && address < EndTry;
        }
    }
}
