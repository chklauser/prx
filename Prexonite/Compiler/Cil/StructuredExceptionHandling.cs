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

using System.Diagnostics;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil.Seh;

namespace Prexonite.Compiler.Cil;

public sealed class StructuredExceptionHandlingCompiler : StructuredExceptionHandling
{
    internal StructuredExceptionHandlingCompiler(CompilerState state) : base(state.Source)
    {
        State = state;
    }

    /// <summary>
    ///     The compiler state, this instance of <see cref = "StructuredExceptionHandling" /> is tied to.
    /// </summary>
    public CompilerState? State { [DebuggerStepThrough] get; }

    /// <summary>
    ///     Emits the jump or return using the appropriate equivalent in CIL.
    /// </summary>
    /// <param name = "sourceAddr">The address where the jump originates (the address of the jump/leave instruction normally)</param>
    /// <param name = "ins">The instruction for this jump. Must be a jump/leave instruction.</param>
    /// <exception cref = "PrexoniteException">when the jump is invalid in CIL (as per <see cref = "BranchHandling.Invalid" />)</exception>
    /// <exception cref = "ArgumentException">when the instruction supplied is not a jump/leave instruction.</exception>
    public void EmitJump(int sourceAddr, Instruction ins)
    {
        if (State == null)
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
                targetAddr = State.Source.Code.Count;
                returning = true;
                break;
            default:
                throw new ArgumentException(
                    "The supplied instruction does not involve branching.", nameof(ins));
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
                _emitEndFinally(ins);
                break;
            case BranchHandling.LeaveSkipTry:
                _emitLeave(sourceAddr,
                    _loci[sourceAddr].InnermostRegion?.Block.EndTry ?? throw new PrexoniteException(
                        $"Internal error branch mode for jump instruction {sourceAddr}: {ins} implies a surrounding region that was not found."),
                    ins);
                break;
            case BranchHandling.Invalid:
                throw new PrexoniteException(
                    "Attempted to compile function with invalid SEH construct");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void _emitEndFinally(Instruction ins)
    {
        if (State == null)
        {
            throw new InvalidOperationException(
                "Cannot emit code on a SEH instance that is not tied to a compiler state.");
        }

        Action endfinally = () => State.Il.Emit(OpCodes.Endfinally);
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

    void _emitLeave(int sourceAddr, int targetAddr, Instruction ins)
    {
        if (State == null)
        {
            throw new InvalidOperationException(
                "Cannot emit code on a SEH instance that is not tied to a compiler state.");
        }
        
        Action leave = () => State.Il.Emit(OpCodes.Leave, State.InstructionLabels[targetAddr]);
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
                State.EmitSetReturnValue();
                goto case OpCode.ret_exit;
            case OpCode.ret_exit:
                _clearStack(sourceAddr);
                leave();
                break;
            case OpCode.ret_break:
                State._EmitAssignReturnMode(ReturnMode.Break);
                goto case OpCode.ret_exit;
            case OpCode.ret_continue:
                State._EmitAssignReturnMode(ReturnMode.Continue);
                goto case OpCode.ret_exit;
        }
    }

    void _emitBranch(int sourceAddr, int targetAddr, Instruction ins)
    {
        if (State == null)
        {
            throw new InvalidOperationException(
                "Cannot emit code on a SEH instance that is not tied to a compiler state.");
        }
        
        switch (ins.OpCode)
        {
            case OpCode.jump:
            case OpCode.leave:
                State.Il.Emit(OpCodes.Br, State.InstructionLabels[targetAddr]);
                break;
            case OpCode.jump_f:
                _emitUnboxBool();
                State.Il.Emit(OpCodes.Brfalse, State.InstructionLabels[targetAddr]);
                break;
            case OpCode.jump_t:
                _emitUnboxBool();
                State.Il.Emit(OpCodes.Brtrue, State.InstructionLabels[targetAddr]);
                break;
            case OpCode.ret_value:
                State.EmitSetReturnValue();
                goto case OpCode.ret_exit;
            case OpCode.ret_exit:
                //return mode is set implicitly by function header
                _clearStack(sourceAddr);
                State.Il.Emit(OpCodes.Br, State.InstructionLabels[State.Source.Code.Count]);
                break;
            case OpCode.ret_continue:
                State._EmitAssignReturnMode(ReturnMode.Continue);
                goto case OpCode.ret_exit;
            case OpCode.ret_break:
                State._EmitAssignReturnMode(ReturnMode.Break);
                goto case OpCode.ret_exit;
        }
    }

    void _clearStack(int sourceAddress)
    {
        if (State == null)
        {
            throw new InvalidOperationException(
                "Cannot emit code on a SEH instance that is not tied to a compiler state.");
        }
        Debug.Assert(0 <= sourceAddress && sourceAddress <= State.StackSize.Length);
        State.EmitIgnoreArguments(State.StackSize[sourceAddress]);
    }

    void _emitSkipFalse(Action skippable)
    {
        _emitUnboxBool();
        var cont = State.Il.DefineLabel();
        State.Il.Emit(OpCodes.Brfalse_S, cont);
        skippable();
        State.Il.MarkLabel(cont);
    }

    void _emitSkipTrue(Action skippable)
    {
        _emitUnboxBool();
        var cont = State.Il.DefineLabel();
        State.Il.Emit(OpCodes.Brtrue_S, cont);
        skippable();
        State.Il.MarkLabel(cont);
    }

    [MemberNotNull(nameof(State))]
    void _emitUnboxBool()
    {
        if (State == null)
        {
            throw new InvalidOperationException(
                "Cannot emit code on a SEH instance that is not tied to a compiler state.");
        }
        State.EmitLoadLocal(State.SctxLocal);
        State.Il.EmitCall(OpCodes.Call, Runtime.ExtractBoolMethod, null);
    }
}

/// <summary>
///     <para>Prepares information required to translate structured exception handling in CIL. Handles implementation of all jump instructions.</para>
///     <para>Each instance of <see cref = "StructuredExceptionHandling" /> is tied to one <see cref = "CompilerState" /> and vice-versa.</para>
/// </summary>
public class StructuredExceptionHandling
{
    internal readonly InstructionInfo[] _loci;
    
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
            _involvedRegions(_loci[sourceAddr].Regions, _loci[targetAddr].Regions)
            .Select(st => _assesJumpForTwoRegions(st.Item1, st.Item2, sourceAddr, targetAddr));

        return decisions.Aggregate(_integrateBranchHandling);
    }

    static IEnumerable<(Region?, Region?)> _involvedRegions(List<Region> source,
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
            Region? sourceRegion;
            if (s == ss && (!areParallel || s == source.Count))
                sourceRegion = null;
            else
                sourceRegion = source[s];

            for (var t = tt; t >= 0; t--)
            {
                Region? targetRegion;
                if (t == tt && (!areParallel || t == target.Count))
                    targetRegion = null;
                else
                    targetRegion = target[t];

                yield return (sourceRegion, targetRegion);
            }
        }
    }

    static BranchHandling _integrateBranchHandling(BranchHandling h1, BranchHandling h2)
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
                $"Invalid decision by SEH checking algorithm: {Enum.GetName(typeof(BranchHandling), h1)} and {Enum.GetName(typeof(BranchHandling), h1)}.");
        }
    }

    static BranchHandling _assesJumpForTwoRegions(
        Region? sourceRegion,
        Region? targetRegion,
        int sourceAddr,
        int targetAddr
    )
    {
        switch (sourceRegion, targetRegion)
        {
            case (null, null):
            // case var (source, target) when Equals(source, target):
            //     return BranchHandling.Branch;
            case (null, { Kind: RegionKind.Try })
                when targetAddr == targetRegion.Begin || targetRegion.Contains(sourceAddr):
                //Jump into try block only legal if target is first instruction of said try block
                //Or else target must be in a surrounding try block
                return BranchHandling.Branch;
            case ({ Kind: RegionKind.Try }, null):
                return BranchHandling.Leave;
            case ({ Kind: RegionKind.Try }, { Kind: RegionKind.Try })
                when targetAddr == targetRegion.Begin || sourceRegion.IsIn(targetRegion):
                //Jump into try block only legal if target is first instruction of said try block
                //Or else target must be in a surrounding try block
                return BranchHandling.Leave;
            case ({ Kind: RegionKind.Try }, { Kind: RegionKind.Finally }) when targetAddr == targetRegion.Begin:
                return BranchHandling.LeaveSkipTry;
            case ({ Kind: RegionKind.Catch }, null):
                return BranchHandling.Leave;
            case ({ Kind: RegionKind.Catch }, { Kind: RegionKind.Try })
                when targetAddr == targetRegion.Begin || sourceRegion.IsIn(targetRegion):
                return BranchHandling.Leave;
            case ({ Kind: RegionKind.Catch }, { Kind: RegionKind.Finally }) when targetAddr == targetRegion.Begin:
                return BranchHandling.LeaveSkipTry;
            case ({ Kind: RegionKind.Finally }, _) when targetRegion == null || !targetRegion.IsIn(sourceRegion):
                //Prexonite byte code ends a finally block sometimes by jumping to the 
                //  instruction right after the whole try-catch-finally. In CIL this
                //  has to be implemented by the endfinally opcode.
                return targetAddr == sourceRegion.Block.EndTry ? BranchHandling.EndFinally : BranchHandling.Invalid;
            case ({ Kind: RegionKind.Finally }, { Kind: RegionKind.Try }) when targetAddr == targetRegion.Begin ||
                sourceRegion.IsIn(targetRegion):
                return BranchHandling.Leave;
            case (_, _):
                return BranchHandling.Invalid;
        }
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
        "{address}: {Instruction} [Regions: {Regions.Count}, Innermost: {InnermostRegion}]")]
    internal sealed class InstructionInfo(StructuredExceptionHandling seh, int address)
    {
        readonly int address = address;
        public readonly List<Region> Regions = new(8);

        public Region? InnermostRegion => Regions.Count > 0 ? Regions[0] : default;

        public IEnumerable<CompiledTryCatchFinallyBlock> OpeningTryBlocks()
        {
            for (var i = Regions.Count - 1; i >= 0; i--)
            {
                var region = Regions[i];
                if (region.Begin == address && region.Kind == RegionKind.Try)
                    yield return region.Block;
            }
        }

        public Instruction Instruction
        {
            [DebuggerStepThrough]
            get
            {
                if (seh is StructuredExceptionHandlingCompiler { State.Source.Code: var source } &&
                    address < source.Count)
                {
                    return source[address];
                }
                else
                {
                    return new(OpCode.nop);
                }
            }
        }
    }
}