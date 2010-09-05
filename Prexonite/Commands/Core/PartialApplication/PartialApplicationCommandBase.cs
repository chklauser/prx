using System;
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
    [CLSCompliant(false)]
    public abstract class PartialApplicationCommandBase : PCommand, ICilExtension
    {
        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            var mappingCandidates = new LinkedList<int>();
            var i = args.Length - 1;
            for (; 0 <= i; i--)
            {
                PValue value;
                if (!args[i].TryConvertTo(sctx, PType.Int, out value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst((int) value.Value);
            }

            //TODO: Improve interpreted runtime by only converting as many arguments as indicated by the mapping
            var mappings = ExtractMappings32(mappingCandidates);

            //Remove mapping args, so we're only left with closed arguments
            var countMappingArgs = _countInt32Required(mappings.Length);
            var closedArgc = args.Length - countMappingArgs;
            var closedArgv = new PValue[closedArgc];
            Array.Copy(args, closedArgv, closedArgc);

            return sctx.CreateNativePValue(CreatePartialApplication(sctx, mappings, closedArgv));
        }

        /// <summary>
        /// Takes the effective arguments mapping and the already provided (closed) arguments and creates the corresponding partial application object.
        /// </summary>
        /// <param name="sctx"></param>
        /// <param name="mappings">Mappings from effective argument position to closed and open arguments. See <see cref="PartialApplicationBase.Mappings"/>.</param>
        /// <param name="closedArguments">Already provided (closed) arguments.</param>
        /// <returns></returns>
        protected abstract IIndirectCall CreatePartialApplication(StackContext sctx, sbyte[] mappings, PValue[] closedArguments);

        private ConstructorInfo _partialApplicationConstructor;

        /// <summary>
        /// <para>Returns the constructor overload to use for CIL compilation. Must have exactly the following signature:</para>
        /// <code>theConstructor(sbyte[] mappings, PValue[] closedArguments)</code>
        /// </summary>
        protected ConstructorInfo PartialApplicationConstructor 
        { 
            get
            {
                return _partialApplicationConstructor
                       ??
                       (_partialApplicationConstructor =
                        PartialCallRepresentationType.GetConstructor(new[] {typeof (sbyte[]), typeof (PValue[])}));
            }
        }

        /// <summary>
        /// The class that represents the partial call at runtime (implements <see cref="IIndirectCall"/>). Its <code>(sbyte[], PValue[])</code> constructor will be called by the CIL implementation.
        /// </summary>
        protected abstract Type PartialCallRepresentationType { get; }

        /// <summary>
        /// Calculates how many Int32 arguments are needed to encode <paramref name="countMapppings"/> mappings, including the number that indicates how many mappings there are
        /// </summary>
        /// <param name="countMapppings">The number of mappings to encode (without the number of mappings itself)</param>
        /// <returns>The number of Int32 values needed to encode the mappings.</returns>
        private static int _countInt32Required(int countMapppings)
        {
            return ( countMapppings + 1 + 3)/4;
        }

        private static readonly sbyte[] _noMapping = new sbyte[0];

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
        //TODO: use int[]
        public static sbyte[] ExtractMappings32(LinkedList<int> rawInput)
        {
            if (rawInput.Count == 0)
                return _noMapping;

            //Extract count
            var int32 = default(ExplicitInt32);
            int32.Int = rawInput.Last.Value;
            int count = int32.Byte3;
            var numArgs = _countInt32Required(count);
            if (numArgs > rawInput.Count)
                return null;

            //remove integers that do not belong to the mapping
            while (numArgs < rawInput.Count)
                rawInput.RemoveFirst();

            //Extract mappings
            var result = new sbyte[count];
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
        //TODO: use int[]
        public static sbyte[] ExtractMappings32(int[] rawInput)
        {
            if (rawInput.Length == 0)
                return _noMapping;

            //Extract count
            var int32 = default(ExplicitInt32);
            int32.Int = rawInput[rawInput.Length-1];
            int count = int32.Byte3;
            var numArgs = _countInt32Required(count);
            if (numArgs > rawInput.Length)
                return null;

            //remove integers that do not belong to the mapping
            var startIndex = rawInput.Length-numArgs;

            //Extract mappings
            var result = new sbyte[count];
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

        private static readonly MethodInfo _extractMappings32Method =
            typeof (PartialApplicationCommandBase).GetMethod("ExtractMappings32",new[]{typeof(Int32[])});

        /// <summary>
        /// Encodes a list of mappings into an array of 32-bit integers (including a byte indicating the number of mappings)
        /// </summary>
        /// <param name="mappings">The mappings to pack into 32-bit integers</param>
        /// <returns>The list of integers making up the packed mappings.</returns>
        public static int[] PackMappings32(sbyte[] mappings)
        {
            var len32 = _countInt32Required(mappings.Length);
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

        #region Implementation of ICilExtension

        public bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            var mappingCandidates = new LinkedList<int>();
            for (var i = staticArgv.Length - 1; 0 <= i; i--)
            {
                int value;
                if(!staticArgv[i].TryGetInt(out value))
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
            var mappingCandidates = new LinkedList<int>();
            for (var i = staticArgv.Length - 1; 0 <= i; i--)
            {
                int value;
                if (!staticArgv[i].TryGetInt(out value))
                    break; //stop at the first non-integer
                mappingCandidates.AddFirst(value);
            }

            var mappings = ExtractMappings32(mappingCandidates);
            var packed = PackMappings32(mappings); //Might not be 32-bit in the future

            //Emit code for constants that are really closed arguments
            var additionalClosedArgc = staticArgv.Length - packed.Length;
            for (var i = 0; i < additionalClosedArgc; i++)
                staticArgv[i].EmitLoadAsPValue(state);

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
            state.EmitCall(_extractMappings32Method);
            state.ReadArgv(argc);

            state.Il.Emit(OpCodes.Newobj, PartialApplicationConstructor);

            if (ins.JustEffect)
                state.Il.Emit(OpCodes.Pop);
            else
                state.EmitVirtualCall(Compiler.Cil.Compiler.CreateNativePValue);
        }

        #endregion
    }
}
