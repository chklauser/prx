// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System.Reflection.Emit;

namespace Prexonite.Compiler.Cil
{
    /// <summary>
    ///     Try-catch-finally block augmented for CIL compilation. Has no relevance for the Prexonite interpreter.
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
            return new(block);
        }

        /// <summary>
        ///     The label that marks the dedicated leave instruction for this try-block.
        /// </summary>
        public Label SkipTryLabel { get; set; }

        public bool IsInTryBlock(int address)
        {
            return Handles(address);
        }

        public bool IsInFinallyBlock(int address)
        {
            if (!HasFinally)
                return false;
            if (HasCatch)
                return BeginFinally <= address && address < BeginCatch;
            else
                return BeginFinally <= address && address < EndTry;
        }

        public bool IsInCatchBlock(int address)
        {
            if (!HasCatch)
                return false;
            return BeginCatch <= address && address < EndTry;
        }
    }
}