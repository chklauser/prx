﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public abstract class PartialApplicationCommandBase: PCommand
    {
        private static readonly int[] _noMapping = new int[0];

        protected static readonly MethodInfo ExtractMappings32Method =
            typeof (PartialApplicationCommandBase).GetMethod("ExtractMappings32",new[]{typeof(Int32[])});

        public override bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        /// Calculates how many Int32 arguments are needed to encode <paramref name="countMapppings"/> mappings, including the number that indicates how many mappings there are
        /// </summary>
        /// <param name="countMapppings">The number of mappings to encode (without the number of mappings itself)</param>
        /// <returns>The number of Int32 values needed to encode the mappings.</returns>
        protected static int CountInt32Required(int countMapppings)
        {
            return ( countMapppings + 1 + 3)/4;
        }

        /// <summary>
        /// Little-Endian 32-bit union
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct ExplicitInt32
        {
            [FieldOffset(0)]
            public Int32 Int;

            // overlapped bytes (little-endian)
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            [FieldOffset(0)]
            public SByte Byte0;

            [FieldOffset(1)]
            public SByte Byte1;

            [FieldOffset(2)]
            public SByte Byte2;

            [FieldOffset(3)]
            public SByte Byte3;
            // ReSharper restore FieldCanBeMadeReadOnly.Local
        }

        /// <summary>
        /// Extracts the mappings from a list of 32-bit integers.
        /// </summary>
        /// <param name="rawInput">The raw list of 32-bit integers extracted from the tail of the command arguments. Includes the byte that indicates the number of mappings.</param>
        /// <returns>An array of mappings</returns>
        public static int[] ExtractMappings32(LinkedList<int> rawInput)
        {
            if (rawInput.Count == 0)
                return _noMapping;

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
        /// Extracts the mappings from a list of 32-bit integers.
        /// </summary>
        /// <param name="rawInput">The raw list of 32-bit integers extracted from the tail of the command arguments. Includes the byte that indicates the number of mappings.</param>
        /// <returns>An array of mappings</returns>
// ReSharper disable UnusedMember.Global
        //used via CIL compilation
        public static int[] ExtractMappings32(int[] rawInput)
// ReSharper restore UnusedMember.Global
        {
            if (rawInput.Length == 0)
                return _noMapping;

            //Extract count
            var int32 = default(ExplicitInt32);
            int32.Int = rawInput[rawInput.Length-1];
            int count = int32.Byte3;
            var numArgs = CountInt32Required(count);
            if (numArgs > rawInput.Length)
                return null;

            //remove integers that do not belong to the mapping
            var startIndex = rawInput.Length-numArgs;

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
        /// Encodes a list of mappings into an array of 32-bit integers (including a byte indicating the number of mappings)
        /// </summary>
        /// <param name="mappings">The mappings to pack into 32-bit integers</param>
        /// <returns>The list of integers making up the packed mappings.</returns>
        public static int[] PackMappings32(int[] mappings)
        {
            var len32 = CountInt32Required(mappings.Length);
            var packed = new int[len32];
            var mappingIndex = 0;
            var packedIndex = 0;
            while(mappingIndex < mappings.Length)
            {
                //byte 0
                packed[packedIndex] |= (mappings[mappingIndex] << 0 * 8) & (0xFF << 0 * 8);
                mappingIndex++;

                //byte 1
                if(mappings.Length <= mappingIndex )
                    break;
                packed[packedIndex] |= (mappings[mappingIndex] << 1 * 8) & (0xFF << 1 * 8);
                mappingIndex++;

                //byte 2
                if (mappings.Length <= mappingIndex)
                    break;
                packed[packedIndex] |= (mappings[mappingIndex] << 2 * 8) & (0xFF << 2 * 8);
                mappingIndex++;

                //byte 3
                if (mappings.Length <= mappingIndex)
                    break;
                packed[packedIndex] |= (mappings[mappingIndex] << 3 * 8) & (0xFF << 3 * 8);
                mappingIndex++;

                packedIndex++;
            }

            System.Diagnostics.Debug.Assert(mappingIndex == mappings.Length, "Not all mappings were encoded");

            //pack total number of mappings as the last byte
            packed[packed.Length-1] |= (mappings.Length << 3*8) & (0xFF << 3*8);

            return packed;
        }
    }

    public abstract class PartialApplicationCommandBase<TParam> : PartialApplicationCommandBase, ICilExtension
    {
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            var arguments = new ArraySegment<PValue>(args);
            var parameter = FilterRuntimeArguments(sctx, ref arguments);

            var mappingCandidates = new LinkedList<int>();
            for (var i = (arguments.Offset + arguments.Count) - 1; arguments.Offset <= i; i--)
            {
                PValue value;
                if (!arguments.Array[i].TryConvertTo(sctx, PType.Int, out value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst((int) value.Value);
            }

            //TODO: Improve interpreted runtime by only converting as many arguments as indicated by the mapping
            var mappings = ExtractMappings32(mappingCandidates);

            //Remove mapping args, so we're only left with closed arguments
            var countMappingArgs = CountInt32Required(mappings.Length);
            var closedArgc = arguments.Count - countMappingArgs;
            var closedArgv = new PValue[closedArgc];
            Array.Copy(arguments.Array, arguments.Offset, closedArgv, 0, closedArgc);

            return sctx.CreateNativePValue(CreatePartialApplication(sctx, mappings, closedArgv, parameter));
        }

        /// <summary>
        /// Takes the effective arguments mapping and the already provided (closed) arguments and creates the corresponding partial application object.
        /// </summary>
        /// <param name="sctx"></param>
        /// <param name="mappings">Mappings from effective argument position to closed and open arguments. See <see cref="PartialApplicationBase.Mappings"/>.</param>
        /// <param name="closedArguments">Already provided (closed) arguments.</param>
        /// <returns></returns>
        protected abstract IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, TParam parameter);

        protected virtual TParam FilterRuntimeArguments(StackContext sctx, ref ArraySegment<PValue> arguments)
        {

            return default(TParam);
        }

        /// <summary>
        /// <para>Returns the constructor overload to use for CIL compilation. Must have exactly the following signature:</para>
        /// <code>theConstructor(sbyte[] mappings, PValue[] closedArguments)</code>
        /// </summary>
        protected ConstructorInfo PartialApplicationConstructor(TParam parameter)
        { 
            return GetPartialCallRepresentationType(parameter).GetConstructor(new[] {typeof (int[]), typeof (PValue[])});
        }

        /// <summary>
        /// The class that represents the partial call at runtime (implements <see cref="IIndirectCall"/>). Its <code>(sbyte[], PValue[])</code> constructor will be called by the CIL implementation.
        /// </summary>
        protected abstract Type GetPartialCallRepresentationType(TParam parameter);

        #region Implementation of ICilExtension

        public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            var staticArguments = new ArraySegment<CompileTimeValue>(staticArgv);
            TParam dummyParameter;
            if(!FilterCompileTimeArguments(ref staticArguments, out dummyParameter))
                return false;

            var mappingCandidates = new LinkedList<int>();
            for (var i = staticArguments.Offset + staticArguments.Count - 1; staticArguments.Offset <= i; i--)
            {
                int value;
                if(!staticArguments.Array[i].TryGetInt(out value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst(value);
            }

            var mappings = ExtractMappings32(mappingCandidates);
            foreach (var t in mappings)
                if (t == 0)
                    return false;

            return true;
        }

        public void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            var staticArguments = new ArraySegment<CompileTimeValue>(staticArgv);
            TParam parameter;
            if(!FilterCompileTimeArguments(ref staticArguments, out parameter))
                throw new PrexoniteException("Internal CIL compiler error. Tried to implement invalid CIL extension call.");

            var mappingCandidates = new LinkedList<int>();
            for (var i = staticArguments.Offset + staticArguments.Count - 1; staticArguments.Offset <= i; i--)
            {
                int value;
                if (!staticArguments.Array[i].TryGetInt(out value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst(value);
            }

            var mappings = ExtractMappings32(mappingCandidates);
            var packed = PackMappings32(mappings); //Might not be 32-bit in the future

            //Emit code for constants that are really closed arguments
            var additionalClosedArgc = staticArguments.Count - packed.Length;
            for (var i = staticArguments.Offset; i < additionalClosedArgc - staticArguments.Offset; i++)
                staticArguments.Array[i].EmitLoadAsPValue(state);

            //Save closed arguments 
            var argc = dynamicArgc + additionalClosedArgc;
            state.FillArgv(argc);

            if (!ins.JustEffect)
                state.EmitLoadLocal(state.SctxLocal);

            //Create array for packed mappings
            state.EmitLdcI4(packed.Length);
            state.Il.Emit(OpCodes.Newarr, typeof(int));

            //Populate packed mappings array
            for(var i = 0; i < packed.Length; i++)
            {
                state.Il.Emit(OpCodes.Dup); //make sure array is still on top of the stack afterwards
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

        protected virtual void EmitConstructorCall(CompilerState state, TParam parameter)
        {
            state.Il.Emit(OpCodes.Newobj, PartialApplicationConstructor(parameter));
        }

        protected virtual bool FilterCompileTimeArguments(ref ArraySegment<CompileTimeValue> staticArgv, out TParam parameter)
        {
            parameter = default(TParam);
            return true;
        }

        #endregion

        
    }
}