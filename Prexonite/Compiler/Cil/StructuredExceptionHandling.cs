// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil.Seh;

namespace Prexonite.Compiler.Cil.Seh
{
}

namespace Prexonite.Compiler.Cil
{
    /// <summary>
    ///     <para>Prepares information required to translate structured exception handling in CIL. Handles implementation of all jump instructions.</para>
    ///     <para>Each instance of <see cref = "StructuredExceptionHandling" /> is tied to one <see cref = "CompilerState" /> and vice-versa.</para>
    /// </summary>
    [DebuggerDisplay("SEH for {State}")]
    public sealed class StructuredExceptionHandling
    {
        private readonly CompilerState _state;
        private readonly InstructionInfo[] _loci;

        /// <summary>
        ///     Creates a new instance of structured exception handling.
        /// </summary>
        /// <param name = "state">The compiler state, this instance of <see cref = "StructuredExceptionHandling" /> is tied to.</param>
        internal StructuredExceptionHandling(CompilerState state) : this(state.Source)
        {
            _state = state;
        }

        internal StructuredExceptionHandling(PFunction source)
        {
            _loci = new InstructionInfo[source.Code.Count + 1];
            //include virtual instruction at the end of the function.

            var regions =
                source.TryCatchFinallyBlocks
                    .Select(CompiledTryCatchFinallyBlock.Create)
                    .SelectMany(Region.FromBlock).ToList();
            for (var instructionOffset = 0; instructionOffset < _loci.Length; instructionOffset++)
            {
                var address = instructionOffset; //make sure that we don't access a modified closure
                var locus = new InstructionInfo(this, address);
                locus.Regions.AddRange(regions.Where(r => r.Contains(address)));
                locus.Regions.Sort(Region.CompareRegions);
                _loci[address] = locus;
            }
            _state = null;
        }

        /// <summary>
        ///     The compiler state, this instance of <see cref = "StructuredExceptionHandling" /> is tied to.
        /// </summary>
        public CompilerState State
        {
            [DebuggerStepThrough]
            get { return _state; }
        }

        /// <summary>
        ///     Determines the handling for a branching instruction, depending on source and target address.
        /// </summary>
        /// <param name = "sourceAddr">The address of the jump instruction-</param>
        /// <param name = "targetAddr">The address the jump instruction targets</param>
        /// <returns>The handling required to implement this jump.</returns>
        public BranchHandling AssessJump(int sourceAddr, int targetAddr)
        {
            var decisions =
                (from st in _involvedRegions(_loci[sourceAddr].Regions, _loci[targetAddr].Regions)
                 select _assesJumpForTwoRegions(st.Item1, st.Item2, sourceAddr, targetAddr));

            return decisions.Aggregate(_integrateBranchHandling);
        }

        private static IEnumerable<Tuple<Region, Region>> _involvedRegions(List<Region> source,
            List<Region> target)
        {
            //Find common ancestor
            var areParallel = false; //have regions of same try-catch-finally construct
            var ss = source.Count;
            var tt = target.Count;
            for (var s = 0; s < ss; s++)
            {
                var sourceRegion = source[s];
                for (var t = 0; t < tt; t++)
                {
                    var targetRegion = target[t];
                    if (sourceRegion.Equals(targetRegion))
                    {
                        ss = s;
                        tt = t;
                    }
                    else if (sourceRegion.Block == targetRegion.Block)
                    {
                        ss = s;
                        tt = t;
                        areParallel = true;
                    }
                }
            }

            //Return pairs up to common ancestor
            for (var s = ss; s >= 0; s--)
            {
                Region sourceRegion;
                if (s == ss && (!areParallel || s == source.Count))
                    sourceRegion = null;
                else
                    sourceRegion = source[s];

                for (var t = tt; t >= 0; t--)
                {
                    Region targetRegion;
                    if (t == tt && (!areParallel || t == target.Count))
                        targetRegion = null;
                    else
                        targetRegion = target[t];

                    yield return Tuple.Create(sourceRegion, targetRegion);
                }
            }
        }

        private static BranchHandling _integrateBranchHandling(BranchHandling h1, BranchHandling h2)
        {
            if (h1 == h2)
                return h1;

            if (h1 == BranchHandling.Branch)
                return h2;
            else if (h2 == BranchHandling.Branch)
                return h1;
            else if (h1 == BranchHandling.Invalid)
                return h1;
            else if (h2 == BranchHandling.Invalid)
                return h2;
            else if (h1 == BranchHandling.EndFinally || h2 == BranchHandling.EndFinally)
                return BranchHandling.Invalid; //end finally is only compatible with itself
            else if (h1 == BranchHandling.Leave && h2 == BranchHandling.LeaveSkipTry ||
                h2 == BranchHandling.Leave && h1 == BranchHandling.LeaveSkipTry)
                return BranchHandling.LeaveSkipTry;
            else
            {
                throw new PrexoniteException(
                    string.Format(
                        "Invalid decision by SEH checking algorithm: {0} and {1}.",
                        Enum.GetName(typeof (BranchHandling), h1),
                        Enum.GetName(typeof (BranchHandling), h1)));
            }
        }

        private BranchHandling _assesJumpForTwoRegions(Region sourceRegion, Region targetRegion,
            int sourceAddr, int targetAddr)
        {
            if (sourceRegion == targetRegion)
                return BranchHandling.Branch;

            if (sourceRegion == null)
            {
                if (targetRegion == null)
                    return BranchHandling.Branch;
                switch (targetRegion.Kind)
                {
                        //Jump into try block only legal if target is first instruction of said try block
                    case RegionKind.Try:
                        if (targetAddr == targetRegion.Begin || targetRegion.Contains(sourceAddr))
                            return BranchHandling.Branch;
                        else
                            return BranchHandling.Invalid;
                    case RegionKind.Catch:
                        return BranchHandling.Invalid;
                    case RegionKind.Finally:
                        return BranchHandling.Invalid;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            switch (sourceRegion.Kind)
            {
                case RegionKind.Try:
                    if (targetRegion == null)
                        return BranchHandling.Leave;
                    switch (targetRegion.Kind)
                    {
                        case RegionKind.Try:
                            //Jump into try block only legal if target is first instruction of said try block
                            //Or else target must be in a surrounding try block
                            if (targetAddr == targetRegion.Begin || sourceRegion.IsIn(targetRegion))
                                return BranchHandling.Leave;
                            else
                                return BranchHandling.Invalid;
                        case RegionKind.Catch:
                            return BranchHandling.Invalid;
                        case RegionKind.Finally:
                            //Jump to finally appears in Prexonite byte code as a jump to the first finally
                            //  instruction. There is no equivalent jump in CIL (finally is invoked automatically)
                            if (targetAddr == targetRegion.Begin)
                                return BranchHandling.LeaveSkipTry;
                            else
                                return BranchHandling.Invalid;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case RegionKind.Catch:
                    if (targetRegion == null)
                        return BranchHandling.Leave;
                    switch (targetRegion.Kind)
                    {
                        case RegionKind.Try:
                            if (targetAddr == targetRegion.Begin || sourceRegion.IsIn(targetRegion))
                                return BranchHandling.Leave;
                            else
                                return BranchHandling.Invalid;
                        case RegionKind.Catch:
                            return BranchHandling.Invalid;
                        case RegionKind.Finally:
                            if (targetAddr == targetRegion.Begin)
                                return BranchHandling.LeaveSkipTry;
                            else
                                return BranchHandling.Invalid;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case RegionKind.Finally:
                    if (targetRegion == null || !targetRegion.IsIn(sourceRegion))
                    {
                        //Prexonite byte code ends a finally block sometimes by jumping to the 
                        //  instruction right after the whole try-catch-finally. In CIL this
                        //  has to be implemented by the endfinally opcode.
                        if (targetAddr == sourceRegion.Block.EndTry)
                            return BranchHandling.EndFinally;
                        else
                            return BranchHandling.Invalid;
                    }
                    else
                    {
                        switch (targetRegion.Kind)
                        {
                            case RegionKind.Try:
                                if (targetAddr == targetRegion.Begin ||
                                    sourceRegion.IsIn(targetRegion))
                                    return BranchHandling.Leave;
                                else
                                    return BranchHandling.Invalid;
                            case RegionKind.Catch:
                                return BranchHandling.Invalid;
                            case RegionKind.Finally:
                                return BranchHandling.Invalid;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Emits the jump or return using the appropriate equivalent in CIL.
        /// </summary>
        /// <param name = "sourceAddr">The address where the jump originates (the address of the jump/leave instruction normally)</param>
        /// <param name = "ins">The instruction for this jump. Must be a jump/leave instruction.</param>
        /// <exception cref = "PrexoniteException">when the jump is invalid in CIL (as per <see cref = "BranchHandling.Invalid" />)</exception>
        /// <exception cref = "ArgumentException">when the instruction supplied is not a jump/leave instruction.</exception>
        public void EmitJump(int sourceAddr, Instruction ins)
        {
            if (_state == null)
                throw new InvalidOperationException(
                    "Cannot emit code on a SEH instance that is not tied to a compiler state.");
            int targetAddr;
            var returning = false;
            switch (ins.OpCode)
            {
                case OpCode.jump:
                case OpCode.jump_f:
                case OpCode.jump_t:
                case OpCode.leave:
                    targetAddr = ins.Arguments;
                    break;
                case OpCode.ret_value:
                case OpCode.ret_exit:
                case OpCode.ret_continue:
                case OpCode.ret_break:
                    targetAddr = _state.Source.Code.Count;
                    returning = true;
                    break;
                default:
                    throw new ArgumentException(
                        "The supplied instruction does not involve branching.", "ins");
            }

            var handling = AssessJump(sourceAddr, targetAddr);

            switch (handling)
            {
                case BranchHandling.Branch:
                    _emitBranch(sourceAddr, targetAddr, ins);
                    break;
                case BranchHandling.Leave:
                    _emitLeave(sourceAddr, targetAddr, ins);
                    break;
                case BranchHandling.EndFinally:
                    Debug.Assert(!returning);
                    _emitEndFinally(sourceAddr, ins);
                    break;
                case BranchHandling.LeaveSkipTry:
                    _emitLeave(sourceAddr, _loci[sourceAddr].InnerMostRegion.Block.EndTry, ins);
                    break;
                case BranchHandling.Invalid:
                    throw new PrexoniteException(
                        "Attempted to compile function with invalid SEH construct");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void _emitEndFinally(int sourceAddr, Instruction ins)
        {
            Action endfinally = () => _state.Il.Emit(OpCodes.Endfinally);
            switch (ins.OpCode)
            {
                case OpCode.jump:
                case OpCode.leave:
                    endfinally();
                    break;
                case OpCode.jump_t:
                    _emitSkipFalse(endfinally);
                    break;
                case OpCode.jump_f:
                    _emitSkipTrue(endfinally);
                    break;
                default:
                    throw new PrexoniteException("Cannot implement " + ins + " using endfinally.");
            }
        }

        private void _emitLeave(int sourceAddr, int targetAddr, Instruction ins)
        {
            Action leave = () => _state.Il.Emit(OpCodes.Leave, _state.InstructionLabels[targetAddr]);
            switch (ins.OpCode)
            {
                case OpCode.jump:
                case OpCode.leave:
                    leave();
                    break;
                case OpCode.jump_t:
                    _emitSkipFalse(leave);
                    break;
                case OpCode.jump_f:
                    _emitSkipTrue(leave);
                    break;
                case OpCode.ret_value:
                    _state.EmitSetReturnValue();
                    goto case OpCode.ret_exit;
                case OpCode.ret_exit:
                    var max = _state.Source.Code.Count;
                    _clearStack(sourceAddr);
                    _state.Il.Emit(OpCodes.Br, _state.InstructionLabels[max]);
                    break;
                case OpCode.ret_break:
                    _state._EmitAssignReturnMode(ReturnMode.Break);
                    goto case OpCode.ret_exit;
                case OpCode.ret_continue:
                    _state._EmitAssignReturnMode(ReturnMode.Continue);
                    goto case OpCode.ret_exit;
            }
        }

        private void _emitBranch(int sourceAddr, int targetAddr, Instruction ins)
        {
            switch (ins.OpCode)
            {
                case OpCode.jump:
                case OpCode.leave:
                    _state.Il.Emit(OpCodes.Br, _state.InstructionLabels[targetAddr]);
                    break;
                case OpCode.jump_f:
                    _emitUnboxBool();
                    _state.Il.Emit(OpCodes.Brfalse, _state.InstructionLabels[targetAddr]);
                    break;
                case OpCode.jump_t:
                    _emitUnboxBool();
                    _state.Il.Emit(OpCodes.Brtrue, _state.InstructionLabels[targetAddr]);
                    break;
                case OpCode.ret_value:
                    _state.EmitSetReturnValue();
                    goto case OpCode.ret_exit;
                case OpCode.ret_exit:
                    //return mode is set implicitly by function header
                    _clearStack(sourceAddr);
                    _state.Il.Emit(OpCodes.Br, _state.InstructionLabels[_state.Source.Code.Count]);
                    break;
                case OpCode.ret_continue:
                    _state._EmitAssignReturnMode(ReturnMode.Continue);
                    goto case OpCode.ret_exit;
                case OpCode.ret_break:
                    _state._EmitAssignReturnMode(ReturnMode.Break);
                    goto case OpCode.ret_exit;
            }
        }

        private void _clearStack(int sourceAddress)
        {
            Debug.Assert(0 <= sourceAddress && sourceAddress <= _state.StackSize.Length);
            _state.EmitIgnoreArguments(_state.StackSize[sourceAddress]);
        }

        private void _emitSkipFalse(Action skippable)
        {
            _emitUnboxBool();
            var cont = _state.Il.DefineLabel();
            _state.Il.Emit(OpCodes.Brfalse_S, cont);
            skippable();
            _state.Il.MarkLabel(cont);
        }

        private void _emitSkipTrue(Action skippable)
        {
            _emitUnboxBool();
            var cont = _state.Il.DefineLabel();
            _state.Il.Emit(OpCodes.Brtrue_S, cont);
            skippable();
            _state.Il.MarkLabel(cont);
        }

        private void _emitUnboxBool()
        {
            _state.EmitLoadLocal(_state.SctxLocal);
            _state.Il.EmitCall(OpCodes.Call, Runtime.ExtractBoolMethod, null);
        }

        /// <summary>
        ///     Returns the try blocks opening at this instruction in reverse order (closest block comes last)
        /// </summary>
        /// <param name = "address">The address of the instruction.</param>
        /// <returns>The try blocks opening at this instruction in reverse order (closest block comes last)</returns>
        public IEnumerable<CompiledTryCatchFinallyBlock> GetOpeningTryBlocks(int address)
        {
            if (address > _loci.Length)
                address = _loci.Length - 1;
            return _loci[address].OpeningTryBlocks();
        }

        [DebuggerDisplay(
            "{_address}: {Instruction} [Regions: {Regions.Count}, Innermost: {InnerMostRegion}]")]
        private sealed class InstructionInfo
        {
            private readonly int _address;
            private readonly StructuredExceptionHandling _seh;
            public readonly List<Region> Regions = new List<Region>(8);

            private bool _isInRegion
            {
                [DebuggerStepThrough]
                get { return Regions.Count > 0; }
            }

            public Region InnerMostRegion
            {
                [DebuggerStepThrough]
                get
                {
                    if (_isInRegion)
                        return Regions[0];
                    else
                        return default(Region);
                }
            }

            [DebuggerStepThrough]
            public InstructionInfo(StructuredExceptionHandling seh, int address)
            {
                _seh = seh;
                _address = address;
            }

            public IEnumerable<CompiledTryCatchFinallyBlock> OpeningTryBlocks()
            {
                for (var i = Regions.Count - 1; i >= 0; i--)
                {
                    var region = Regions[i];
                    if (region.Begin == _address && region.Kind == RegionKind.Try)
                        yield return region.Block;
                }
            }

            public Instruction Instruction
            {
                [DebuggerStepThrough]
                get
                {
                    if (_seh._state == null)
                        return new Instruction(OpCode.nop);

                    if (_address >= _seh._state.Source.Code.Count)
                        return new Instruction(OpCode.nop);
                    else
                        return _seh._state.Source.Code[_address];
                }
            }
        }
    }
}