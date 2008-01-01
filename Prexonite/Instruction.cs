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
using System.Text;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    //Behaves like a union. Not all fields are used by all instructions
    /// <summary>
    /// Represents a single Prexonite VM instruction
    /// </summary>
    //[NoDebug]
    public class Instruction
    {
        /// <summary>
        /// The instructions opcode. Determines the VMs behaviour at runtime.
        /// </summary>
        public OpCode OpCode;

        //Common arguments
        /// <summary>
        /// One of the instructions operands. Arguments is commonly used to hold the number of arguments take from 
        /// the stack. The field is also used to store other integer 
        /// operands like the target of a jump or the number of values to rotate, pop or duplicate from the stack.
        /// </summary>
        public int Arguments = 0;

        /// <summary>
        /// One of the instructions operands. Id is commonly used to store identifiers but also more 
        /// general call targets or type expressions.
        /// </summary>
        public string Id = null;

        /// <summary>
        /// One of the instructions operands. Statically, GenericArgument is only used by ldc.real 
        /// to store a boxed double value. The VM however uses this field to cache constant data 
        /// like evaluated type expressions or resolved call targets.
        /// </summary>
        public object GenericArgument = null;

        /// <summary>
        /// The just effect flag prevents certain operations from pushing their result back on the stack.
        /// This is useful in situations where only the side effect of an operation but not it's return value 
        /// is interesting, eliminating the need for an additional pop instruction.
        /// </summary>
        public bool JustEffect = false;

        #region Construction

        #region Low-Level

        /// <summary>
        /// Creates a new Instruction with a given OpCode. Operands are initialized to default values.
        /// </summary>
        /// <param name="opCode">The opcode of the instruction.</param>
        public Instruction(OpCode opCode)
        {
            OpCode = opCode;
        }

        /// <summary>
        /// Creates a new Instruction with a given OpCode and an identifier as its operand.
        /// </summary>
        /// <param name="opCode">The opcode of the instruction.</param>
        /// <param name="id">The id operand.</param>
        public Instruction(OpCode opCode, string id)
            : this(opCode)
        {
            Id = id;
        }

        public Instruction(OpCode opCode, int arguments)
            : this(opCode)
        {
            Arguments = arguments;
        }

        public Instruction(OpCode opCode, int arguments, string id)
            : this(opCode)
        {
            Id = id;
            Arguments = arguments;
        }

        public Instruction(OpCode opCode, int arguments, string id, bool justEffect)
            : this(opCode, arguments, id)
        {
            JustEffect = justEffect;
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
            Instruction ins = new Instruction(OpCode.ldc_real);
            ins.GenericArgument = r;
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

        #region Operators

        public static Instruction CreateAddition()
        {
            return new Instruction(OpCode.add);
        }

        public static Instruction CreateSubtraction()
        {
            return new Instruction(OpCode.sub);
        }

        public static Instruction CreateMultiplication()
        {
            return new Instruction(OpCode.mul);
        }

        public static Instruction CreateDivision()
        {
            return new Instruction(OpCode.div);
        }

        public static Instruction CreateModulus()
        {
            return new Instruction(OpCode.mod);
        }

        public static Instruction CreatePower()
        {
            return new Instruction(OpCode.pow);
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

        public static Instruction CreateIndLocI(int index, int arguments)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(
                    "index", index, "index must fit into an unsigned short integer.");
            if (arguments > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(
                    "arguments", arguments, "arguments must fit into an unsigned short integer.");
            ushort idx = (ushort)index;
            ushort argc = (ushort)arguments;

            return new Instruction(Prexonite.OpCode.indloci, (argc << 16) | idx);
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

            if(rotations == 0)
                return new Instruction(Prexonite.OpCode.nop);

            Instruction ins = new Instruction(OpCode.rot, rotations);
            ins.GenericArgument = values;
            return ins;
        }

        #endregion

        #endregion

        #endregion

        #region Classification

        public bool IsJump
        {
            [NoDebug]
            get { return OpCode == OpCode.jump || OpCode == OpCode.jump_t || OpCode == OpCode.jump_f; }
        }

        public bool IsUnconditionalJump
        {
            [NoDebug]
            get { return OpCode == OpCode.jump; }
        }

        public bool IsConditionalJump
        {
            [NoDebug]
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

        #endregion

        #region ToString

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            ToString(buffer);
            return buffer.ToString();
        }

        public void ToString(StringBuilder buffer)
        {
            string escId;
            if (Id != null)
            {
                escId = StringPType.ToIdOrLiteral(Id);
            }
            else
                escId = "\"\"";
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
                    ushort index = (ushort) (Arguments & ushort.MaxValue);
                    ushort argc = (ushort) ((Arguments & (ushort.MaxValue << 16)) >> 16);
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
                    buffer.Append(Enum.GetName(typeof(OpCode), OpCode).Replace('_', '.'));
                    switch (OpCode)
                    {
                            //NULL INSTRUCTIONS
                        case OpCode.ldc_null:
                        case OpCode.neg:
                        case OpCode.not:
                        case OpCode.add:
                        case OpCode.sub:
                        case OpCode.mul:
                        case OpCode.div:
                        case OpCode.mod:
                        case OpCode.pow:
                        case OpCode.ceq:
                        case OpCode.cne:
                        case OpCode.cgt:
                        case OpCode.cge:
                        case OpCode.clt:
                        case OpCode.cle:
                        case OpCode.or:
                        case OpCode.and:
                        case OpCode.xor:
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
                        case OpCode.incglob:
                        case OpCode.decloc:
                        case OpCode.decglob:
                        case OpCode.ldc_string:
                        case OpCode.ldr_func:
                        case OpCode.ldr_cmd:
                        case OpCode.ldr_loc:
                        case OpCode.ldr_glob:
                        case OpCode.ldr_type:
                        case OpCode.ldloc:
                        case OpCode.stloc:
                        case OpCode.ldglob:
                        case OpCode.stglob:
                        case OpCode.check_const:
                        case OpCode.cast_const:
                        case OpCode.newclo:
                            buffer.Append(" ");
                            buffer.Append(escId);
                            return;
                            //ID+ARG INSTRUCTIONS
                        case OpCode.newtype:
                        case OpCode.newobj:
                        case OpCode.get:
                        case OpCode.set:
                        case OpCode.cmd:
                        case OpCode.sget:
                        case OpCode.sset:
                        case OpCode.func:
                        case OpCode.indloc:
                        case OpCode.indglob:
                            buffer.Append(".");
                            buffer.Append(Arguments.ToString());
                            buffer.Append(" ");
                            buffer.Append(escId);
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
        /// Determines whether the instruction is equal to an object (possibly another instruction).
        /// </summary>
        /// <param name="obj">Any object (possibly an instruction).</param>
        /// <returns>True if the instruction is equal to the object (possibly another instruction).</returns>
        [NoDebug]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if(ReferenceEquals(this, obj))
                return true;
            if (obj is Instruction)
            {
                Instruction ins = obj as Instruction;
                if (ins.OpCode != OpCode)
                    return false;

                switch (OpCode)
                {
                        //NULL INSTRUCTIONS
                    case OpCode.nop:
                    case OpCode.ldc_null:
                    case OpCode.neg:
                    case OpCode.not:
                    case OpCode.add:
                    case OpCode.sub:
                    case OpCode.mul:
                    case OpCode.div:
                    case OpCode.mod:
                    case OpCode.pow:
                    case OpCode.ceq:
                    case OpCode.cne:
                    case OpCode.cgt:
                    case OpCode.cge:
                    case OpCode.clt:
                    case OpCode.cle:
                    case OpCode.or:
                    case OpCode.and:
                    case OpCode.xor:
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
                    case OpCode.indloci: //two short int values encoded in one int.
                        return Arguments == ins.Arguments;
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
                    case OpCode.incglob:
                    case OpCode.decloc:
                    case OpCode.decglob:
                    case OpCode.ldc_string:
                    case OpCode.ldr_func:
                    case OpCode.ldr_cmd:
                    case OpCode.ldr_loc:
                    case OpCode.ldr_glob:
                    case OpCode.ldr_type:
                    case OpCode.ldloc:
                    case OpCode.stloc:
                    case OpCode.ldglob:
                    case OpCode.stglob:
                    case OpCode.check_const:
                    case OpCode.cast_const:
                    case OpCode.newclo:
                        return Engine.StringsAreEqual(Id, ins.Id);
                        //ID+ARG INSTRUCTIONS
                    case OpCode.newtype:
                    case OpCode.newobj:
                    case OpCode.get:
                    case OpCode.set:
                    case OpCode.cmd:
                    case OpCode.sget:
                    case OpCode.sset:
                    case OpCode.func:
                    case OpCode.indglob:
                    case OpCode.indloc:
                        return
                            Arguments == ins.Arguments &&
                            Engine.StringsAreEqual(Id, ins.Id) &&
                            JustEffect == ins.JustEffect;
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
            else
                return base.Equals(obj);
        }

        /// <summary>
        /// Determines whether two instructions are equal.
        /// </summary>
        /// <param name="left">One instruction</param>
        /// <param name="right">Another instruction</param>
        /// <returns>True if the instructions are equal, false otherwise.</returns>
        [NoDebug]
        public static bool operator ==(Instruction left, Instruction right)
        {
            if ((object) left == null)
                return (object) right == null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two instructions are not equal.
        /// </summary>
        /// <param name="left">One instruction</param>
        /// <param name="right">Another instruction</param>
        /// <returns>True if the instructions are not equal, false otherwise.</returns>
        [NoDebug]
        public static bool operator !=(Instruction left, Instruction right)
        {
            if ((object)left == null)
                return (object)right != null;
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a hash code based on <see cref="OpCode"/>, <see cref="Arguments"/> and <see cref="Id"/>.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return (int)OpCode ^ Arguments ^ (Id == null ? 0 : Id.GetHashCode());
        }

        #endregion
    }

    /// <summary>
    /// The opcodes interpreted by the virtual machine.
    /// </summary>
    public enum OpCode
    {
        invalid = -1,
        nop = 0,
        //Loading (13)
        //  - constants
        ldc_int, //ldc.int       loads an integer value
        ldc_real, //ldc.real      loads a floating point value
        ldc_bool, //ldc.bool      loads a boolean value
        ldc_string, //ldc.string    loads a string value
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
        ceq, //ceq           binary equality operator
        cne, //cne           binary inequality operator
        clt, //clt           binary less-than operator
        cle, //cle           binary less-than-or-equal operator
        cgt, //cgt           binary greater-than operator
        cge, //cge           binary greater-than-or-equal operator
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
        tail, //tail            performs an indirect call (tries to turn it into an optimized tail call)
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

    //Total: 77 different operations excluding nop.
}