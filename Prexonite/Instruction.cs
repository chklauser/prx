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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Prexonite.Commands.Core.Operators;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite
{
    //Behaves like a union. Not all fields are used by all instructions
    /// <summary>
    ///     Represents a single Prexonite VM instruction
    /// </summary>
    //[DebuggerStepThrough]
    public sealed class Instruction : ICloneable
    {
        /// <summary>
        ///     The instructions opcode. Determines the VMs behaviour at runtime.
        /// </summary>
        public OpCode OpCode;

        //Common arguments
        /// <summary>
        ///     One of the instructions operands. Arguments is commonly used to hold the number of arguments take from 
        ///     the stack. The field is also used to store other integer 
        ///     operands like the target of a jump or the number of values to rotate, pop or duplicate from the stack.
        /// </summary>
        public int Arguments;

        /// <summary>
        ///     One of the instructions operands. Id is commonly used to store identifiers but also more 
        ///     general call targets or type expressions.
        /// </summary>
        public string Id;

        /// <summary>
        /// One of the instruction operands. The ModuleName is used for cross-module references 
        /// to functions and variables.
        /// </summary>
        public ModuleName ModuleName;

        /// <summary>
        ///     One of the instructions operands. Statically, GenericArgument is only used by ldc.real 
        ///     to store a boxed double value. The VM however uses this field to cache constant data 
        ///     like evaluated type expressions or resolved call targets.
        /// </summary>
        public object GenericArgument;

        /// <summary>
        ///     The just effect flag prevents certain operations from pushing their result back on the stack.
        ///     This is useful in situations where only the side effect of an operation but not it's return value 
        ///     is interesting, eliminating the need for an additional pop instruction.
        /// </summary>
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // For consistency with the rest of instruction data type
        public bool JustEffect;

        // ReSharper restore FieldCanBeMadeReadOnly.Global

        #region Construction

        #region Low-Level

        /// <summary>
        ///     Creates a new Instruction with a given OpCode. Operands are initialized to default values.
        /// </summary>
        /// <param name = "opCode">The opcode of the instruction.</param>
        /// <remarks>
        ///     See the actual <see cref = "OpCode" />s for details on how to construct valid instruction.
        /// </remarks>
        public Instruction(OpCode opCode)
        {
            OpCode = opCode;
        }

        /// <summary>
        ///     Creates a new Instruction with a given OpCode and an identifier as its operand.
        /// </summary>
        /// <param name = "opCode">The opcode of the instruction.</param>
        /// <param name = "id">The id operand.</param>
        /// <remarks>
        ///     See the actual <see cref = "OpCode" />s for details on how to construct valid instruction.
        /// </remarks>
        public Instruction(OpCode opCode, string id)
            : this(opCode)
        {
            Id = id;
        }

        /// <summary>
        ///     Creates a new Instruction with a given OpCode and an identifier as its operand.
        /// </summary>
        /// <param name = "opCode">The opcode of the instruction.</param>
        /// <param name = "arguments">The arguments operand.</param>
        /// <remarks>
        ///     See the actual <see cref = "OpCode" />s for details on how to construct valid instruction.
        /// </remarks>
        public Instruction(OpCode opCode, int arguments)
            : this(opCode)
        {
            Arguments = arguments;
        }

        /// <summary>
        ///     Creates a new Instruction with a given OpCode and an identifier as its operand.
        /// </summary>
        /// <param name = "opCode">The opcode of the instruction.</param>
        /// <param name = "id">The id operand.</param>
        /// <param name = "arguments">The arguments operand.</param>
        /// <remarks>
        ///     See the actual <see cref = "OpCode" />s for details on how to construct valid instruction.
        /// </remarks>
        public Instruction(OpCode opCode, int arguments, string id)
            : this(opCode)
        {
            Id = id;
            Arguments = arguments;
        }

        /// <summary>
        ///     Creates a new Instruction with a given OpCode and an identifier as its operand.
        /// </summary>
        /// <param name = "opCode">The opcode of the instruction.</param>
        /// <param name = "id">The id operand.</param>
        /// <param name = "arguments">The arguments operand.</param>
        /// <param name = "justEffect">Indicates whether or not the return value is thrown away.</param>
        /// <remarks>
        ///     See the actual <see cref = "OpCode" />s for details on how to construct valid instruction.
        /// </remarks>
        public Instruction(OpCode opCode, int arguments, string id, bool justEffect)
            : this(opCode, arguments, id)
        {
            JustEffect = justEffect;
        }

        /// <summary>
        ///     Retrieves actual index and argument count values from the <see cref = "Arguments" /> field of an instruction.
        /// </summary>
        /// <param name = "index">The address at which to store the actual index.</param>
        /// <param name = "argc">The address at which to store the actual arguments count.</param>
        public void DecodeIndLocIndex(out int index, out int argc)
        {
            if (OpCode != OpCode.indloci)
                throw new ArgumentException("Can only decode indloci instructions.");
            index = (Arguments & ushort.MaxValue);
            argc = ((Arguments & (ushort.MaxValue << 16)) >> 16);
        }

        public PValueKeyValuePair DecodeIndLocIndex()
        {
            int index;
            int argc;
            DecodeIndLocIndex(out index, out argc);
            return new PValueKeyValuePair(index, argc);
        }

        #endregion

        #region High-Level

        #region Constants

        public static Instruction CreateConstant(string str)
        {
            return new Instruction(OpCode.ldc_string, str);
        }

        public static Instruction CreateConstant(bool sw)
        {
            return new Instruction(OpCode.ldc_bool, sw ? 1 : 0);
        }

        public static Instruction CreateConstant(int i)
        {
            return new Instruction(OpCode.ldc_int, i);
        }

        public static Instruction CreateConstant(double r)
        {
            var ins = new Instruction(OpCode.ldc_real)
                {
                    GenericArgument = r
                };
            return ins;
        }

        public static Instruction CreateNull()
        {
            return new Instruction(OpCode.ldc_null);
        }

        #endregion

        #region Variables

        public static Instruction CreateLoadLocal(string id)
        {
            return new Instruction(OpCode.ldloc, id);
        }

        public static Instruction CreateStoreLocal(string id)
        {
            return new Instruction(OpCode.stloc, id);
        }

        public static Instruction CreateLoadGlobal(string id)
        {
            return new Instruction(OpCode.ldglob, id);
        }

        public static Instruction CreateStoreGlobal(string id)
        {
            return new Instruction(OpCode.stglob, id);
        }

        #endregion

        #region Calls

        public static Instruction CreateGetCall(int arguments, string id)
        {
            return CreateGetCall(arguments, id, false);
        }

        public static Instruction CreateGetCall(int arguments, string id, bool justEffect)
        {
            return new Instruction(OpCode.get, arguments, id, justEffect);
        }

        public static Instruction CreateSetCall(int arguments, string id)
        {
            return new Instruction(OpCode.set, arguments, id);
        }

        public static Instruction CreateStaticGetCall(
            int args, string typeId, string memberId, bool justEffect)
        {
            return CreateStaticGetCall(args, typeId + "::" + memberId, justEffect);
        }

        public static Instruction CreateStaticGetCall(int arguments, string typeId, string memberId)
        {
            return CreateStaticGetCall(arguments, typeId, memberId, false);
        }

        public static Instruction CreateStaticGetCall(
            int arguments, string callExpr, bool justEffect)
        {
            return new Instruction(OpCode.sget, arguments, callExpr, justEffect);
        }

        public static Instruction CreateStaticSetCall(int arguments, string typeId, string memberId)
        {
            return CreateStaticSetCall(arguments, typeId + "::" + memberId);
        }

        public static Instruction CreateStaticSetCall(int arguments, string callExpr)
        {
            return new Instruction(OpCode.sset, arguments, callExpr);
        }

        public static Instruction CreateFunctionCall(int arguments, string id, bool justEffect)
        {
            return new Instruction(OpCode.func, arguments, id, justEffect);
        }

        public static Instruction CreateFunctionCall(int arguments, string id)
        {
            return CreateFunctionCall(arguments, id, false);
        }

        public static Instruction CreateCommandCall(int arguments, string id)
        {
            return CreateCommandCall(arguments, id, false);
        }

        public static Instruction CreateCommandCall(int arguments, string id, bool justEffect)
        {
            return new Instruction(OpCode.cmd, arguments, id, justEffect);
        }

        public static Instruction CreateLocalIndirectCall(int arguments, string id, bool justEffect)
        {
            return new Instruction(OpCode.indloc, arguments, id, justEffect);
        }

        public static Instruction CreateLocalIndirectCall(int arguments, string id)
        {
            return CreateLocalIndirectCall(arguments, id, false);
        }

        public static Instruction CreateGlobalIndirectCall(
            int arguments, string id, bool justEffect)
        {
            return new Instruction(OpCode.indglob, arguments, id, justEffect);
        }

        public static Instruction CreateGlobalIndirectCall(int arguments, string id)
        {
            return CreateGlobalIndirectCall(arguments, id, false);
        }

        public static Instruction CreateIndirectCall(int arguments)
        {
            return CreateIndirectCall(arguments, false);
        }

        public static Instruction CreateIndirectCall(int arguments, bool justEffect)
        {
            return new Instruction(OpCode.indarg, arguments, null, justEffect);
        }

        public static Instruction CreateIndLocI(int index, int arguments, bool justEffect)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(
                    "index", index, "index must fit into an unsigned short integer.");
            if (arguments > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(
                    "arguments", arguments, "arguments must fit into an unsigned short integer.");
            var idx = (ushort) index;
            var argc = (ushort) arguments;

            return new Instruction(OpCode.indloci, (argc << 16) | idx, null, justEffect);
        }

        #endregion

        #region Flow-Control

        public static Instruction CreateJump(int address, string label)
        {
            return new Instruction(OpCode.jump, address, label);
        }

        public static Instruction CreateJump(int address)
        {
            return new Instruction(OpCode.jump, address);
        }

        public static Instruction CreateJump(string label)
        {
            return new Instruction(OpCode.jump, -1, label);
        }

        public static Instruction CreateJumpIfTrue(int address, string label)
        {
            return new Instruction(OpCode.jump_t, address, label);
        }

        public static Instruction CreateJumpIfTrue(int address)
        {
            return new Instruction(OpCode.jump_t, address);
        }

        public static Instruction CreateJumpIfTrue(string label)
        {
            return new Instruction(OpCode.jump_t, -1, label);
        }

        public static Instruction CreateJumpIfFalse(int address)
        {
            return new Instruction(OpCode.jump_f, address);
        }

        public static Instruction CreateJumpIfFalse(int address, string label)
        {
            return new Instruction(OpCode.jump_f, address, label);
        }

        public static Instruction CreateJumpIfFalse(string label)
        {
            return new Instruction(OpCode.jump_f, -1, label);
        }

        #endregion

        #region Stack operations

        public static Instruction CreatePop()
        {
            return CreatePop(1);
        }

        internal static Instruction CreatePop(int instructions)
        {
            return new Instruction(OpCode.pop, instructions);
        }

        public static Instruction CreateDuplicate(int copies)
        {
            return new Instruction(OpCode.dup, copies);
        }

        public static Instruction CreateDuplicate()
        {
            return CreateDuplicate(1);
        }

        public static Instruction CreateExchange()
        {
            return CreateRotate(1, 2);
        }

        public static Instruction CreateRotate(int rotations)
        {
            return CreateRotate(rotations, 3);
        }

        public static Instruction CreateRotate(int rotations, int values)
        {
            rotations = (values + rotations)%values;

            if (rotations == 0)
                return new Instruction(OpCode.nop);

            var ins = new Instruction(OpCode.rot, rotations)
                {
                    GenericArgument = values
                };
            return ins;
        }

        #endregion

        #endregion

        #endregion

        #region Classification

        public bool IsJump
        {
            [DebuggerStepThrough]
            get { return OpCode == OpCode.jump || OpCode == OpCode.jump_t || OpCode == OpCode.jump_f; }
        }

        public bool IsUnconditionalJump
        {
            [DebuggerStepThrough]
            get { return OpCode == OpCode.jump; }
        }

        public bool IsConditionalJump
        {
            [DebuggerStepThrough]
            get { return OpCode == OpCode.jump_t || OpCode == OpCode.jump_f; }
        }

        public static OpCode InvertJumpCondition(OpCode jump)
        {
            if (jump == OpCode.jump_t)
                return OpCode.jump_f;
            else if (jump == OpCode.jump_f)
                return OpCode.jump_t;
            else
                throw new ArgumentException("The passed opcode must be a conditional jump.", "jump");
        }

        public bool IsFunctionExit
        {
            [DebuggerStepThrough]
            get
            {
                return OpCode == OpCode.ret_value || OpCode == OpCode.ret_exit ||
                    OpCode == OpCode.ret_continue || OpCode == OpCode.ret_break;
            }
        }

        #endregion

        #region ToString

        /// <summary>
        ///     Returns a human- and machine-readable string representation of the instruction.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     Use the <see cref = "ToString(StringBuilder)" /> overload if you are building up a more complex string.
        /// </remarks>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            ToString(buffer);
            return buffer.ToString();
        }

        /// <summary>
        ///     Writes a human- and machine-readable string representation of the instruction to the supplied <paramref name = "buffer" />.
        /// </summary>
        /// <param name = "buffer">The buffer to write the representation to.</param>
        public void ToString(StringBuilder buffer)
        {
            string escId;
            if (Id != null)
            {
                if (!OperatorCommands.TryGetLiteral(Id, out escId))
                    escId = StringPType.ToIdOrLiteral(Id);
            }
            else
            {
                escId = "\"\"";
            }

            string escModuleName;
            if(ModuleName != null)
            {
                escModuleName = StringPType.ToIdOrLiteral(ModuleName.Id) + "," + ModuleName.Version;
            }
            else
            {
                escModuleName = null;
            }

            switch (OpCode)
            {
                case OpCode.rot:
                    buffer.Append("rot.");
                    buffer.Append(Arguments);
                    buffer.Append(",");
                    buffer.Append(GenericArgument);
                    break;
                case OpCode.incloc:
                    buffer.Append("inc ");
                    buffer.Append(escId);
                    break;
                case OpCode.incloci:
                    buffer.Append("inci ");
                    buffer.Append(Arguments);
                    break;
                case OpCode.indloci:
                    if (JustEffect)
                        buffer.Append('@');
                    int index;
                    int argc;
                    DecodeIndLocIndex(out index, out argc);
                    buffer.Append("indloci.");
                    buffer.Append(argc);
                    buffer.Append(" ");
                    buffer.Append(index);
                    break;
                case OpCode.decloc:
                    buffer.Append("dec ");
                    buffer.Append(escId);
                    break;
                case OpCode.decloci:
                    buffer.Append("deci ");
                    buffer.Append(Arguments);
                    break;
                default:
                    if (JustEffect)
                        buffer.Append("@");
                    buffer.Append(Enum.GetName(typeof (OpCode), OpCode).Replace('_', '.'));
                    switch (OpCode)
                    {
                            //NULL INSTRUCTIONS
                        case OpCode.ldc_null:
                        case OpCode.check_arg:
                        case OpCode.cast_arg:
                        case OpCode.check_null:
                        case OpCode.ldr_app:
                        case OpCode.ldr_eng:
                        case OpCode.ret_value:
                        case OpCode.ret_set:
                        case OpCode.ret_exit:
                        case OpCode.ret_continue:
                        case OpCode.ret_break:
                        case OpCode.@throw:
                        case OpCode.@try:
                        case OpCode.exc:
                            return;
                            //LOAD CONSTANT . REAL
                        case OpCode.ldc_real:
                            buffer.Append(" ");
                            buffer.Append(((double) GenericArgument).ToString());
                            return;
                            //LOAD CONSTANT . BOOL
                        case OpCode.ldc_bool:
                            buffer.Append(Arguments != 0 ? " true" : " false");
                            return;
                            //INTEGER INSTRUCTIONS
                        case OpCode.ldc_int:
                        case OpCode.pop:
                        case OpCode.dup:
                        case OpCode.ldloci:
                        case OpCode.stloci:
                        case OpCode.incloci:
                        case OpCode.decloci:
                        case OpCode.ldr_loci:
                            buffer.Append(" ");
                            buffer.Append(Arguments.ToString());
                            return;
                            //JUMP INSTRUCTIONS
                        case OpCode.jump:
                        case OpCode.jump_t:
                        case OpCode.jump_f:
                        case OpCode.leave:
                            if (Arguments > -1)
                            {
                                buffer.Append(" ");
                                buffer.Append(Arguments.ToString());

#if DEBUG //Save some filespace in the release build.
                                if (Id != null)
                                {
                                    buffer.Append(" /* ");
                                    buffer.Append(Id);
                                    buffer.Append(" */");
                                }
#endif
                            }
                            else
                            {
                                buffer.Append(" ");
                                buffer.Append(escId);
                            }
                            return;
                            //ID INSTRUCTIONS
                        case OpCode.incloc:
                        case OpCode.decloc:
                        case OpCode.ldc_string:
                        case OpCode.ldr_cmd:
                        case OpCode.ldr_loc:
                        case OpCode.ldr_type:
                        case OpCode.ldloc:
                        case OpCode.stloc:
                        case OpCode.check_const:
                        case OpCode.cast_const:
                            buffer.Append(" ");
                            buffer.Append(escId);
                            return;
                            //ID+MODULE  INSTRUCTIONS
                        case OpCode.ldr_func:
                        case OpCode.incglob:
                        case OpCode.decglob:
                        case OpCode.ldr_glob:
                        case OpCode.ldglob:
                        case OpCode.stglob:
                        case OpCode.newclo:
                            buffer.Append(" ");
                            buffer.Append(escId);
                            if (ModuleName != null)
                            {
                                buffer.Append('/');
                                buffer.Append(escModuleName);
                            }
                            return;
                            //ID+ARG INSTRUCTIONS
                        case OpCode.newtype:
                        case OpCode.newobj:
                        case OpCode.get:
                        case OpCode.set:
                        case OpCode.cmd:
                        case OpCode.sget:
                        case OpCode.sset:
                        case OpCode.indloc:
                            buffer.Append(".");
                            buffer.Append(Arguments.ToString());
                            buffer.Append(" ");
                            buffer.Append(escId);
                            return;
                            //ID+ARG+MODULE INSTRUCTIONS
                        case OpCode.func:
                        case OpCode.indglob:
                            buffer.Append(".");
                            buffer.Append(Arguments.ToString());
                            buffer.Append(" ");
                            buffer.Append(escId);
                            if (ModuleName != null)
                            {
                                buffer.Append('/');
                                buffer.Append(escModuleName);
                            }
                            return;
                            //ARG INSTRUCTIONS
                        case OpCode.indarg:
                        case OpCode.newcor:
                        case OpCode.tail:
                            buffer.Append(".");
                            buffer.Append(Arguments.ToString());
                            return;
                            //NOP INSTRUCTION
                        case OpCode.nop:
                            if (!String.IsNullOrEmpty(Id))
                            {
                                buffer.Append("+");
                                buffer.Append(escId);
                            }
#if Verbose
                            buffer.Append(" /*-------------------------------------*/");
#endif
                            break;
                    }
                    break;
            }
        }

        #endregion

        #region Equality

        /// <summary>
        ///     Determines whether the instruction is equal to an object (possibly another instruction).
        /// </summary>
        /// <param name = "obj">Any object (possibly an instruction).</param>
        /// <returns>True if the instruction is equal to the object (possibly another instruction).</returns>
        [DebuggerStepThrough]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (ReferenceEquals(this, obj))
                return true;
            var ins = obj as Instruction;
            if (ins == null)
                return false;
            if (ins.OpCode != OpCode)
                return false;

            switch (OpCode)
            {
                    //NULL INSTRUCTIONS
                case OpCode.nop:
                case OpCode.ldc_null:
                case OpCode.check_arg:
                case OpCode.cast_arg:
                case OpCode.check_null:
                case OpCode.ldr_app:
                case OpCode.ldr_eng:
                case OpCode.ret_value:
                case OpCode.ret_set:
                case OpCode.ret_exit:
                case OpCode.ret_continue:
                case OpCode.ret_break:
                case OpCode.@throw:
                case OpCode.exc:
                case OpCode.@try:
                    return true;
                    //LOAD CONSTANT . REAL
                case OpCode.ldc_real:
                    if (!(ins.GenericArgument is double))
                        return false;
                    return ((double) GenericArgument) == ((double) ins.GenericArgument);
                    //INTEGER INSTRUCTIONS
                case OpCode.ldc_bool:
                case OpCode.ldc_int:
                case OpCode.ldr_loci:
                case OpCode.pop:
                case OpCode.dup:
                case OpCode.ldloci:
                case OpCode.stloci:
                case OpCode.incloci:
                case OpCode.decloci:
                    return Arguments == ins.Arguments;
                case OpCode.indloci: //two short int values encoded in one int.
                    return Arguments == ins.Arguments && JustEffect == ins.JustEffect;
                    //JUMP INSTRUCTIONS
                case OpCode.jump:
                case OpCode.jump_t:
                case OpCode.jump_f:
                case OpCode.leave:
                    if (Arguments > -1 && ins.Arguments > -1)
                        return Arguments == ins.Arguments;
                    else
                        return Engine.StringsAreEqual(Id, ins.Id);
                    //ID INSTRUCTIONS
                case OpCode.incloc:
                case OpCode.decloc:
                case OpCode.ldc_string:
                case OpCode.ldr_cmd:
                case OpCode.ldr_loc:
                case OpCode.ldr_glob:
                case OpCode.ldr_type:
                case OpCode.ldloc:
                case OpCode.stloc:
                case OpCode.check_const:
                case OpCode.cast_const:
                    return Engine.StringsAreEqual(Id, ins.Id);
                    //ID+MODULE INSTRUCTIONS
                case OpCode.incglob:
                case OpCode.decglob:
                case OpCode.ldr_func:
                case OpCode.ldglob:
                case OpCode.stglob:
                case OpCode.newclo:
                    return Engine.StringsAreEqual(Id, ins.Id) && Equals(ModuleName, ins.ModuleName);
                    //ID+ARG INSTRUCTIONS
                case OpCode.newtype:
                case OpCode.newobj:
                case OpCode.get:
                case OpCode.set:
                case OpCode.cmd:
                case OpCode.sget:
                case OpCode.sset:
                case OpCode.indloc:
                    return
                        Arguments == ins.Arguments &&
                            Engine.StringsAreEqual(Id, ins.Id) &&
                                JustEffect == ins.JustEffect;
                    //ID+ARG+MODULE INSTRUCTIONS
                case OpCode.func:
                case OpCode.indglob:
                    return
                        Arguments == ins.Arguments &&
                            Engine.StringsAreEqual(Id, ins.Id) &&
                                JustEffect == ins.JustEffect &&
                                    Equals(ModuleName, ins.ModuleName);
                    //ARG INSTRUCTIONS
                case OpCode.indarg:
                case OpCode.newcor:
                case OpCode.tail:
                    return
                        Arguments == ins.Arguments &&
                            JustEffect == ins.JustEffect;
                case OpCode.rot:
                    return
                        Arguments == ins.Arguments &&
                            (int) GenericArgument == (int) ins.GenericArgument;
                default:
                    throw new PrexoniteException("Invalid opcode " + OpCode);
            }
        }

        /// <summary>
        ///     Determines whether two instructions are equal.
        /// </summary>
        /// <param name = "left">One instruction</param>
        /// <param name = "right">Another instruction</param>
        /// <returns>True if the instructions are equal, false otherwise.</returns>
        [DebuggerStepThrough]
        public static bool operator ==(Instruction left, Instruction right)
        {
            if ((object) left == null)
                return (object) right == null;
            return left.Equals(right);
        }

        /// <summary>
        ///     Determines whether two instructions are not equal.
        /// </summary>
        /// <param name = "left">One instruction</param>
        /// <param name = "right">Another instruction</param>
        /// <returns>True if the instructions are not equal, false otherwise.</returns>
        [DebuggerStepThrough]
        public static bool operator !=(Instruction left, Instruction right)
        {
            if ((object) left == null)
                return (object) right != null;
            return !left.Equals(right);
        }

        #endregion

        #region ICloneable Members

        ///<summary>
        ///    Creates a new object that is a copy of the current instance.
        ///</summary>
        ///<returns>
        ///    A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        ///     Retuns a shallow clone of the instruction.
        /// </summary>
        /// <returns>A shallow clone of the instruction</returns>
        public Instruction Clone()
        {
            return (Instruction) MemberwiseClone();
        }

        #endregion

        public int StackSizeDelta
        {
            get
            {
                switch (OpCode)
                {
                    case OpCode.nop:
                    case OpCode.incloc:
                    case OpCode.incglob:
                    case OpCode.check_const:
                    case OpCode.check_null:
                    case OpCode.cast_const:
                    case OpCode.@try:
                    case OpCode.decloc:
                    case OpCode.decglob:
                    case OpCode.incloci:
                    case OpCode.decloci:
                    case OpCode.neg:
                    case OpCode.ret_exit:
                    case OpCode.ret_break:
                    case OpCode.ret_continue:
                    case OpCode.rot:
                    case OpCode.leave:
                    case OpCode.not:
                    case OpCode.jump:
                        return 0;

                    case OpCode.ldc_real:
                    case OpCode.ldc_bool:
                    case OpCode.ldc_string:
                    case OpCode.ldc_null:
                    case OpCode.ldr_loc:
                    case OpCode.ldr_loci:
                    case OpCode.ldr_glob:
                    case OpCode.ldr_func:
                    case OpCode.ldr_cmd:
                    case OpCode.ldglob:
                    case OpCode.ldr_app:
                    case OpCode.ldloci:
                    case OpCode.ldr_eng:
                    case OpCode.newclo:
                    case OpCode.ldr_type:
                    case OpCode.ldloc:
                    case OpCode.ldc_int:
                    case OpCode.exc:
                        return +1;

                    case OpCode.stglob:
                    case OpCode.stloci:
                    case OpCode.stloc:
                    case OpCode.add:
                    case OpCode.check_arg:
                    case OpCode.cast_arg:
                    case OpCode.sub:
                    case OpCode.mul:
                    case OpCode.div:
                    case OpCode.mod:
                    case OpCode.pow:
                    case OpCode.ceq:
                    case OpCode.ret_value:
                    case OpCode.cne:
                    case OpCode.clt:
                    case OpCode.@throw:
                    case OpCode.cle:
                    case OpCode.cgt:
                    case OpCode.cge:
                    case OpCode.ret_set:
                    case OpCode.or:
                    case OpCode.and:
                    case OpCode.xor:
                    case OpCode.jump_t:
                    case OpCode.jump_f:
                        return -1;

                    case OpCode.set:
                        return -Arguments - 1;

                    case OpCode.get:
                    case OpCode.indarg:
                    case OpCode.tail:
                    case OpCode.newcor:
                        return -Arguments - 1 + (JustEffect ? 0 : 1);

                    case OpCode.indloc:
                    case OpCode.indglob:
                    case OpCode.sget:
                    case OpCode.func:
                    case OpCode.newobj:
                    case OpCode.newtype:
                    case OpCode.cmd:
                        return -Arguments + (JustEffect ? 0 : 1);

                    case OpCode.indloci:
                        int index;
                        int argc;
                        DecodeIndLocIndex(out index, out argc);
                        return -argc + (JustEffect ? 0 : 1);

                    case OpCode.pop:
                    case OpCode.sset:
                        return -Arguments;

                    case OpCode.dup:
                        return Arguments;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    // ReSharper disable InconsistentNaming
    /// <summary>
    ///     The opcodes interpreted by the virtual machine.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public enum OpCode
    {
        /// <summary>
        ///     A non-existant opcode. Will result in exceptions when fed into the virtual machine.
        /// </summary>
        invalid = -1,

        /// <summary>
        ///     No operation. Stack: 0->0. Will be removed in optimization pass.
        /// </summary>
        nop = 0,

        //Loading (13)
        //  - constants
        /// <summary>
        ///     Loads a constant <see cref = "PType.Int" /> onto the stack. Stack: 0->1.
        /// </summary>
        ldc_int, //ldc.int       loads an integer value

        /// <summary>
        ///     Loads a constant <see cref = "PType.Real" /> onto the stack. Stack: 0->1.
        /// </summary>
        ldc_real, //ldc.real      loads a floating point value

        /// <summary>
        ///     Loads a constant <see cref = "PType.Bool" /> onto the stack. Stack: 0->1.
        /// </summary>
        ldc_bool, //ldc.bool      loads a boolean value

        /// <summary>
        ///     Loads a constant <see cref = "PType.String" /> onto the stack. Stack: 0->1.
        /// </summary>
        ldc_string, //ldc.string    loads a string value

        /// <summary>
        ///     Loads the <see cref = "PType.Null" /> value onto the stack. Stack: 0->
        /// </summary>
        ldc_null, //ldc.null      loads a null value

        //  - references
        ldr_loc, //ldr.loc       loads a reference to a local variable by name
        ldr_loci, //ldr.loci        loads a reference to a local variable by index
        ldr_glob, //ldr.glob      loads a reference to a global variable
        ldr_func, //ldr.func      loads a reference to a function
        ldr_cmd, //ldr.cmd      loads a reference to a command
        ldr_app, //ldr.app       loads a reference to the current application
        ldr_eng, //ldr.eng       loads a reference to the current engine
        ldr_type, //ldr.type      loads a reference to a type (from a type expression)
        // Variables (6)
        //  - local
        ldloc, //ldloc         loads the value of a local variable by name
        stloc, //stloc         stores a value into a local variable by name
        ldloci, //ldloci       loads the value of a local variable by index
        stloci, //stloci       stores a value into a local variable by index
        //  - global
        ldglob, //ldglob        loads the value of a global variable
        stglob, //stglob        stores a value into a global variable
        //Construction (4)
        newobj, //newobj        creates a new instance of a specified type
        newtype, //newtype       creates a new type instance
        newclo, //newclo        creates a new closure
        newcor, //newcor        creates a new coroutine
        //Operators (23)
        //  - unary
        incloc, //incloc        unary local increment operator by name
        incglob, //incglob       unary global increment operator
        decloc, //decloc        unary local decrement operator by name
        decglob, //decglob       unary global decrement operator
        incloci, //incloci     unary local increment operator by index
        decloci, //incloci     unary local decrement operator by index
        neg, //neg           unary negation operator
        not, //not           unary logical not operator
        //  - addition
        add, //add           binary addition operator
        sub, //sub           binary subtraction operator
        //  - multiplication
        mul, //mul           binary multiplication operator
        div, //div           binary division operator
        mod, //mod           binary modulus operator
        //  - exponential
        pow, //pow           binary power operator
        //  - comparision
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "ceq")] ceq, //ceq           binary equality operator
        cne, //cne           binary inequality operator
        clt, //clt           binary less-than operator
        cle, //cle           binary less-than-or-equal operator
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "cgt")] cgt, //cgt           binary greater-than operator
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "cge")] cge, //cge           binary greater-than-or-equal operator
        //  - bitwise
        or, //or            binary bitwise or operator
        and, //and           binary bitwise and operator
        xor, //xor           binary bitwise exclusive or operator
        //Type check + cast (5)
        check_const, //check         type check (type as operand)
        check_arg, //check         type check (type on stack)
        check_null, //check.null   check for null
        cast_const, //cast          explicit type cast (type as operand)
        cast_arg, //cast          explicit type cast (type on stack)
        //Object access (4)
        get, //get           performs a get call
        set, //set           performs a set call
        sget, //sget          performs a static get call
        sset, //sset          performs a static set call
        //Calls (3)
        func, //func          performs a function call
        cmd, //cmd           performs a command call
        indarg, //indarg        performs an indirect call on an operand
        tail,
        //tail            performs an indirect call (tries to turn it into an optimized tail call)
        //Indirect calls (3)
        indloc, //indloc        performs an indirect call on a local variable by name
        indloci, //indloci      performs an indriect call on a local variable by index
        indglob, //indglob       performs an indirect call on a global variable
        //Flow control (13)
        jump, //jump          jumps to an address
        jump_t, //jump.t        jumps to an address if a condition is true
        jump_f, //jump.f        jumps to an address if a condition is false
        ret_exit, //ret.exit      exits the function
        ret_value, //ret.value     grabs the return value from the stack and exits the function
        ret_break, //ret.break     exits the function in break mode
        ret_continue, //ret.continue  exits the function in continue mode
        ret_set, //ret.set       grabs the return value from the stack and stores it
        @throw,
        //throw         Throws one argument (string or exception) from the stack as an exception.
        @try, //try            Initializes a try-finally-[catch] block
        leave,
        //leave         jumps depending on whether the context is currently in exception handling mode or not.
        exc, //exc              Pushes the current exception on top of the stack.

        //Stack manipulation (3)
        pop, //pop           Pops x values from the stack
        dup, //dup           duplicates the top value x times

        rot //rot           rotates the x top values y times
    }

    // ReSharper restore InconsistentNaming

    //Total: 77 different operations excluding nop.
}