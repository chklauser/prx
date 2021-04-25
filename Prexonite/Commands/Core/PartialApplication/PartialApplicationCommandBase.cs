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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public abstract class PartialApplicationCommandBase : PCommand
    {
        protected static readonly MethodInfo ExtractMappings32Method =
            typeof (PartialApplicationCommandBase).GetMethod("ExtractMappings32",
                new[] {typeof (int[])});

        /// <summary>
        ///     Calculates how many Int32 arguments are needed to encode <paramref name = "countMapppings" /> mappings, including the number that indicates how many mappings there are
        /// </summary>
        /// <param name = "countMapppings">The number of mappings to encode (without the number of mappings itself)</param>
        /// <returns>The number of Int32 values needed to encode the mappings.</returns>
        protected static int CountInt32Required(int countMapppings)
        {
            return (countMapppings + 1 + 3)/4;
        }

        /// <summary>
        ///     Little-Endian 32-bit union
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct ExplicitInt32
        {
            [FieldOffset(0)] public int Int;

            // overlapped bytes (little-endian)
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            [FieldOffset(0)] public sbyte Byte0;

            [FieldOffset(1)] public sbyte Byte1;

            [FieldOffset(2)] public sbyte Byte2;

            [FieldOffset(3)] public sbyte Byte3;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
        }

        /// <summary>
        ///     Extracts the mappings from a list of 32-bit integers.
        /// </summary>
        /// <param name = "rawInput">The raw list of 32-bit integers extracted from the tail of the command arguments. Includes the byte that indicates the number of mappings.</param>
        /// <returns>An array of mappings</returns>
        public static int[] ExtractMappings32(LinkedList<int> rawInput)
        {
            if (rawInput.Count == 0)
                return Array.Empty<int>();

            //Extract count
            var int32 = default(ExplicitInt32);
            int32.Int = rawInput.Last.Value;
            int count = int32.Byte3;
            var numArgs = CountInt32Required(count);
            if (numArgs > rawInput.Count)
                return null;

            //remove integers that do not belong to the mapping
            while (numArgs < rawInput.Count)
                rawInput.RemoveFirst();

            //Extract mappings
            var result = new int[count];
            var index = 0;
            foreach (var raw in rawInput)
            {
                int32.Int = raw;

                if (index < count)
                    result[index++] = int32.Byte0;

                if (index < count)
                    result[index++] = int32.Byte1;

                if (index < count)
                    result[index++] = int32.Byte2;

                if (index < count)
                    result[index++] = int32.Byte3;
            }

            return result;
        }

        /// <summary>
        ///     Extracts the mappings from a list of 32-bit integers.
        /// </summary>
        /// <param name = "rawInput">The raw list of 32-bit integers extracted from the tail of the command arguments. Includes the byte that indicates the number of mappings.</param>
        /// <returns>An array of mappings</returns>
        // ReSharper disable UnusedMember.Global
        //used via CIL compilation
        public static int[] ExtractMappings32(int[] rawInput)
            // ReSharper restore UnusedMember.Global
        {
            if (rawInput.Length == 0)
                return Array.Empty<int>();

            //Extract count
            var int32 = default(ExplicitInt32);
            int32.Int = rawInput[^1];
            int count = int32.Byte3;
            var numArgs = CountInt32Required(count);
            if (numArgs > rawInput.Length)
                return null;

            //remove integers that do not belong to the mapping
            var startIndex = rawInput.Length - numArgs;

            //Extract mappings
            var result = new int[count];
            var index = 0;
            for (var i = startIndex; i < rawInput.Length; i++)
            {
                var raw = rawInput[i];
                int32.Int = raw;

                if (index < count)
                    result[index++] = int32.Byte0;

                if (index < count)
                    result[index++] = int32.Byte1;

                if (index < count)
                    result[index++] = int32.Byte2;

                if (index < count)
                    result[index++] = int32.Byte3;
            }

            return result;
        }

        /// <summary>
        ///     Encodes a list of mappings into an array of 32-bit integers (including a byte indicating the number of mappings)
        /// </summary>
        /// <param name = "mappings">The mappings to pack into 32-bit integers</param>
        /// <returns>The list of integers making up the packed mappings.</returns>
        public static int[] PackMappings32(int[] mappings)
        {
            var len32 = CountInt32Required(mappings.Length);
            var packed = new int[len32];
            var mappingIndex = 0;
            var packedIndex = 0;
            while (mappingIndex < mappings.Length)
            {
                //byte 0
                packed[packedIndex] |= (mappings[mappingIndex] << 0*8) & (0xFF << 0*8);
                mappingIndex++;

                //byte 1
                if (mappings.Length <= mappingIndex)
                    break;
                packed[packedIndex] |= (mappings[mappingIndex] << 1*8) & (0xFF << 1*8);
                mappingIndex++;

                //byte 2
                if (mappings.Length <= mappingIndex)
                    break;
                packed[packedIndex] |= (mappings[mappingIndex] << 2*8) & (0xFF << 2*8);
                mappingIndex++;

                //byte 3
                if (mappings.Length <= mappingIndex)
                    break;
                packed[packedIndex] |= (mappings[mappingIndex] << 3*8) & (0xFF << 3*8);
                mappingIndex++;

                packedIndex++;
            }

            System.Diagnostics.Debug.Assert(mappingIndex == mappings.Length,
                "Not all mappings were encoded");

            //pack total number of mappings as the last byte
            packed[^1] |= (mappings.Length << 3*8) & (0xFF << 3*8);

            return packed;
        }
    }

    public abstract class PartialApplicationCommandBase<TParam> : PartialApplicationCommandBase,
                                                                  ICilExtension
    {
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            var arguments = new ArraySegment<PValue>(args);
            var parameter = FilterRuntimeArguments(sctx, ref arguments);

            var mappingCandidates = new LinkedList<int>();
            for (var i = arguments.Offset + arguments.Count - 1; arguments.Offset <= i; i--)
            {
                if (!arguments.Array[i].TryConvertTo(sctx, PType.Int, out var value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst((int) value.Value);
            }

            //TODO: (Ticket #105) Improve interpreted runtime by only converting as many arguments as indicated by the mapping
            var mappings = ExtractMappings32(mappingCandidates);

            //Remove mapping args, so we're only left with closed arguments
            var countMappingArgs = CountInt32Required(mappings.Length);
            var closedArgc = arguments.Count - countMappingArgs;
            var closedArgv = new PValue[closedArgc];
            Array.Copy(arguments.Array, arguments.Offset, closedArgv, 0, closedArgc);

            return
                sctx.CreateNativePValue(CreatePartialApplication(sctx, mappings, closedArgv,
                    parameter));
        }

        /// <summary>
        ///     Takes the effective arguments mapping and the already provided (closed) arguments and creates the corresponding partial application object.
        /// </summary>
        /// <param name = "sctx"></param>
        /// <param name = "mappings">Mappings from effective argument position to closed and open arguments. See <see
        ///      cref = "PartialApplicationBase.Mappings" />.</param>
        /// <param name = "closedArguments">Already provided (closed) arguments.</param>
        /// <param name = "parameter">The custom parameter extracted by <see cref = "FilterRuntimeArguments" />.</param>
        /// <returns>The object that represents the partial application. The application is completed when calling that object indirectly.</returns>
        protected abstract IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings,
            PValue[] closedArguments, TParam parameter);

        /// <summary>
        ///     <para>Extracts a custom parameter from the arguments supplied to the constructor at runtime.</para>
        ///     <para>This method is only invoked when the calling function is interpreted. In CIL-compiled functions, the parameter is extracted by <see
        ///      cref = "FilterCompileTimeArguments" />.</para>
        /// </summary>
        /// <param name = "sctx">The stack context that issued this constructor call.</param>
        /// <param name = "arguments">The arguments as provided by the interpreter. Remove any arguments used to build the parameter (the return value) from <paramref
        ///      name = "arguments" /> by adapting the array segment.</param>
        /// <returns>A custom parameter. Will be passed to <see cref = "CreatePartialApplication" /> untouched.</returns>
        protected virtual TParam FilterRuntimeArguments(StackContext sctx,
            ref ArraySegment<PValue> arguments)
        {
            return default;
        }

        /// <summary>
        ///     <para>Returns the constructor overload to use for CIL compilation. Must have exactly the following signature:</para>
        ///     <code>theConstructor(sbyte[] mappings, PValue[] closedArguments)</code>
        /// </summary>
        protected virtual ConstructorInfo GetConstructorCtor(TParam parameter)
        {
            return
                GetPartialCallRepresentationType(parameter).GetConstructor(new[]
                    {typeof (int[]), typeof (PValue[])});
        }

        /// <summary>
        ///     The class that represents the partial call at runtime (implements <see cref = "IIndirectCall" />). Its <code>(sbyte[], PValue[])</code> constructor will be called by the CIL implementation.
        /// </summary>
        protected abstract Type GetPartialCallRepresentationType(TParam parameter);

        #region Implementation of ICilExtension

        public virtual bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            var staticArguments = new ArraySegment<CompileTimeValue>(staticArgv);
            if (!FilterCompileTimeArguments(ref staticArguments, out var dummyParameter))
                return false;

            var mappingCandidates = new LinkedList<int>();
            for (var i = staticArguments.Offset + staticArguments.Count - 1;
                 staticArguments.Offset <= i;
                 i--)
            {
                if (!staticArguments.Array[i].TryGetInt(out var value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst(value);
            }

            var mappings = ExtractMappings32(mappingCandidates);
            return !mappings.Contains(0);
        }

        public virtual void Implement(CompilerState state, Instruction ins,
            CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            var staticArguments = new ArraySegment<CompileTimeValue>(staticArgv);
            if (!FilterCompileTimeArguments(ref staticArguments, out var parameter))
                throw new PrexoniteException(
                    "Internal CIL compiler error. Tried to implement invalid CIL extension call.");

            var mappingCandidates = new LinkedList<int>();
            for (var i = staticArguments.Offset + staticArguments.Count - 1;
                 staticArguments.Offset <= i;
                 i--)
            {
                if (!staticArguments.Array[i].TryGetInt(out var value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst(value);
            }

            var mappings = ExtractMappings32(mappingCandidates);
            var packed = PackMappings32(mappings); //Might not be 32-bit in the future

            //Emit code for constants that are really closed arguments
            var additionalClosedArgc = staticArguments.Count - packed.Length;
            for (var i = staticArguments.Offset;
                 i < additionalClosedArgc - staticArguments.Offset;
                 i++)
                staticArguments.Array[i].EmitLoadAsPValue(state);

            //Save closed arguments 
            var argc = dynamicArgc + additionalClosedArgc;
            state.FillArgv(argc);

            if (!ins.JustEffect)
                state.EmitLoadLocal(state.SctxLocal);

            //Create array for packed mappings
            state.EmitLdcI4(packed.Length);
            state.Il.Emit(OpCodes.Newarr, typeof (int));

            //Populate packed mappings array
            for (var i = 0; i < packed.Length; i++)
            {
                state.Il.Emit(OpCodes.Dup);
                //make sure array is still on top of the stack afterwards
                state.EmitLdcI4(i);
                state.EmitLdcI4(packed[i]);
                state.Il.Emit(OpCodes.Stelem_I4);
            }

            //-> packed mappings array is still on top of the stack
            state.EmitCall(ExtractMappings32Method);
            state.ReadArgv(argc);

            EmitConstructorCall(state, parameter);

            if (ins.JustEffect)
                state.Il.Emit(OpCodes.Pop);
            else
                state.EmitVirtualCall(Compiler.Cil.Compiler.CreateNativePValue);
        }

        /// <summary>
        ///     <para>Emits the call to the partial application constructor, along with any custom parameters.</para>
        ///     <para>When this method is called the parameter mappings (<see cref = "Int32" />[]) and the closed arguments (<see
        ///     cref = "PValue" />[]) are already on the stack, in that order.</para>
        /// </summary>
        /// <param name = "state">The compiler state to compile to.</param>
        /// <param name = "parameter">The custom parameter as returned by <see cref = "FilterCompileTimeArguments" />.</param>
        protected virtual void EmitConstructorCall(CompilerState state, TParam parameter)
        {
            state.Il.Emit(OpCodes.Newobj, GetConstructorCtor(parameter));
        }

        /// <summary>
        ///     <para>Performs additional compatibility checks on static arguments and extracts a custom parameter from the static arguments read from byte code during CIL-compilation.</para>
        ///     <para>This method is only invoked when the constructor call is being compiled to CIL. In interpreted functions, the parameter is extracted by <see
        ///      cref = "FilterRuntimeArguments" />.</para>
        /// </summary>
        /// <param name = "staticArgv">The static arguments as read from byte code. Remove any arguments used to build the <paramref
        ///      name = "parameter" /> from <paramref name = "staticArgv" /> by adapting the array segment.</param>
        /// <param name = "parameter">The custom parameter. Will be passed to <see cref = "EmitConstructorCall" /> untouched. If the method returns false, the value of this out parameter is undefined.</param>
        /// <returns>True if the static arguments are compatible with the partial application command; false otehrwise</returns>
        protected virtual bool FilterCompileTimeArguments(
            ref ArraySegment<CompileTimeValue> staticArgv, out TParam parameter)
        {
            parameter = default;
            return true;
        }

        #endregion
    }
}