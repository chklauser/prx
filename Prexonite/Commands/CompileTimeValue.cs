using System;
using System.Collections.Generic;
using System.Linq;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    ///     Represents a compile time value, as read from an instruction.
    ///     The default value is a valid representation of <code>null</code>.
    /// </summary>
    public struct CompileTimeValue
    {
        /// <summary>
        ///     Indicates how to interpret the compile-time value stored in <see cref = "Value" />.
        /// </summary>
        public CompileTimeInterpretation Interpretation;

        /// <summary>
        ///     The compile-time value. Interpret according to <see cref = "Interpretation" />.
        /// </summary>
        public Object Value;

        /// <summary>
        ///     Indicates whether the compile time value is a reference (to a variable, function, command etc.)
        /// </summary>
        public bool IsReference
        {
            get
            {
                switch (Interpretation)
                {
                    case CompileTimeInterpretation.LocalVariableReference:
                    case CompileTimeInterpretation.GlobalVariableReference:
                    case CompileTimeInterpretation.FunctionReference:
                    case CompileTimeInterpretation.CommandReference:
                        return true;
                    default:
                        return false;
                }
            }
        }

        #region Pattern Matching/Unwrapping

        /// <summary>
        ///     Tries to manifest the compile time value as a symbol entry. This will only work for references (command-, function-, variable-).
        /// </summary>
        /// <param name = "entry">If the result is true, returns a symbol entry with the physical id set to the compile time value and the symbol interpretation set according to the interpretation of the compile time value. Otherwise the value is undefined.</param>
        /// <returns>true if the conversion was successful; false otherwise.</returns>
        public bool TryGetSymbolEntry(out SymbolEntry entry)
        {
            var er = Value as EntityRef;
            if (er != null)
            {
                entry = (SymbolEntry) er;
                return true;
            }
            else
            {
                entry = null;
                return false;
            }
        }

        public bool TryGetValue(StackContext sctx, out PValue result)
        {
            EntityRef er;
            if (IsReference && (er = Value as EntityRef) != null && er._TryLookup(sctx, out result))
                return true;
            else
                return TryGetConstant(out result);
        }

        public bool TryGetConstant(out PValue result)
        {
            result = null;
            switch (Interpretation)
            {
                case CompileTimeInterpretation.Null:
                    result = PType.Null;
                    break;
                case CompileTimeInterpretation.String:
                    result = (string) Value;
                    break;
                case CompileTimeInterpretation.Int:
                    int value;
                    TryGetInt(out value);
                    result = value;
                    break;
                case CompileTimeInterpretation.Bool:
                    bool flag;
                    TryGetBool(out flag);
                    result = flag;
                    break;
            }

            return result != null;
        }

        /// <summary>
        ///     <para>Tries to extract a string from the compile time value. This will only work if the value is actually a string.</para>
        ///     <para>References are not treated as strings.</para>
        /// </summary>
        /// <param name = "value">If the conversion succeeds, <paramref name = "value" /> is set to the converted string. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetString(out string value)
        {
            value = null;
            return Interpretation == CompileTimeInterpretation.String &&
                (value = Value as string) != null;
        }

        /// <summary>
        ///     <para>Tries to extract an integer from the compile time value. This will only work if the value is actually an integer.</para>
        /// </summary>
        /// <param name = "value">If the conversion succeeds, <paramref name = "value" /> is set to the converted integer. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetInt(out int value)
        {
            var i = Value as int?;
            value = i.GetValueOrDefault();
            return Interpretation == CompileTimeInterpretation.Int && i != null;
        }

        /// <summary>
        ///     <para>Tries to extract a boolean from the compile time value. This will only work if the value is actually a boolean.</para>
        /// </summary>
        /// <param name = "value">If the conversion succeeds, <paramref name = "value" /> is set to the converted boolean. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetBool(out Boolean value)
        {
            var b = Value as bool?;
            value = b.GetValueOrDefault();
            return Interpretation == CompileTimeInterpretation.Bool && b != null;
        }

        /// <summary>
        ///     <para>Tries to extract a command reference from the compile time value. This will only work if the value is actually a command reference.</para>
        /// </summary>
        /// <param name = "command">If the conversion succeeds, <paramref name = "command" /> is set to the converted command reference. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetCommandReference(out EntityRef.Command command)
        {
            command = null;
            return Interpretation == CompileTimeInterpretation.CommandReference &&
                (command = Value as EntityRef.Command) != null;
        }

        /// <summary>
        ///     <para>Tries to extract a function reference from the compile time value. This will only work if the value is actually a function reference.</para>
        /// </summary>
        /// <param name = "func">If the conversion succeeds, <paramref name = "func" /> is set to the converted function reference. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetFunctionReference(out EntityRef.Function func)
        {
            func = null;
            return Interpretation == CompileTimeInterpretation.FunctionReference &&
                (func = Value as EntityRef.Function) != null;
        }

        /// <summary>
        ///     <para>Tries to extract a local variable reference from the compile time value. This will only work if the value is actually a local variable reference.</para>
        /// </summary>
        /// <param name="localVariable"> </param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetLocalVariableReference(out EntityRef.Variable.Local localVariable)
        {
            localVariable = null;
            return Interpretation == CompileTimeInterpretation.LocalVariableReference &&
                (localVariable = Value as EntityRef.Variable.Local) != null;
        }

        /// <summary>
        ///     <para>Tries to extract a global variable reference from the compile time value. This will only work if the value is actually a global variable reference.</para>
        /// </summary>
        /// <param name="globalVariable"> </param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetGlobalVariableReference(out EntityRef.Variable.Global globalVariable)
        {
            globalVariable = null;
            return Interpretation == CompileTimeInterpretation.GlobalVariableReference &&
                (globalVariable = Value as EntityRef.Variable.Global) != null;
        }

        #endregion

        #region Parsing

        /// <summary>
        ///     Tries to parse the supplied instruction into a compile time value.
        /// </summary>
        /// <param name = "instruction">The instruction to parse.</param>
        /// <param name = "localVariableMapping">The local variable mapping to apply when confronted with index-instructions (e.g., <code>ldloci</code>)</param>
        /// <param name="cache">The cache to use for module names and entity references.</param>
        /// <param name="internalModule"> The module the instruction lives in. Necessary to create absolute entity references.</param>
        /// <param name = "compileTimeValue">The parsed compile time value, if parsing was successful; undefined otherwise</param>
        /// <returns>True if parsing was successful; false otherwise</returns>
        public static bool TryParse(Instruction instruction, IDictionary<int, string> localVariableMapping, CentralCache cache, ModuleName internalModule, out CompileTimeValue compileTimeValue)
        {
            var argc = instruction.Arguments;
            switch (instruction.OpCode)
            {
                case OpCode.ldc_int:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.Int;
                    compileTimeValue.Value = (int?) argc;
                    return true;
                case OpCode.ldc_real:
                    compileTimeValue = default(CompileTimeValue);
                    return false;
                case OpCode.ldc_bool:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.Bool;
                    compileTimeValue.Value = argc != 0;
                    return true;
                case OpCode.ldc_string:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.String;
                    compileTimeValue.Value = instruction.Id;
                    return true;
                case OpCode.ldc_null:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.Null;
                    compileTimeValue.Value = null;
                    return true;
                case OpCode.ldr_loc:
                    compileTimeValue.Interpretation =
                        CompileTimeInterpretation.LocalVariableReference;
                    compileTimeValue.Value = cache[EntityRef.Variable.Local.Create(instruction.Id)];
                    return true;
                case OpCode.ldr_loci:
                    string id;
                    if (!localVariableMapping.TryGetValue(argc, out id) || id == null)
                        goto default;
                    compileTimeValue.Interpretation =
                        CompileTimeInterpretation.LocalVariableReference;
                    compileTimeValue.Value = cache[EntityRef.Variable.Local.Create(id)];
                    return true;
                case OpCode.ldr_glob:
                    compileTimeValue.Interpretation =
                        CompileTimeInterpretation.GlobalVariableReference;
                    compileTimeValue.Value = cache[EntityRef.Variable.Global.Create(instruction.Id, instruction.ModuleName ?? internalModule)];
                    return true;
                case OpCode.ldr_func:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.FunctionReference;
                    compileTimeValue.Value = cache[EntityRef.Function.Create(instruction.Id, instruction.ModuleName ?? internalModule)];
                    return true;
                case OpCode.ldr_cmd:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.CommandReference;
                    compileTimeValue.Value = cache[EntityRef.Command.Create(instruction.Id)];
                    return true;
                case OpCode.ldr_app:
                    compileTimeValue = default(CompileTimeValue);
                    return false;
                case OpCode.ldr_eng:
                    compileTimeValue = default(CompileTimeValue);
                    return false;
                case OpCode.ldr_type:
                    compileTimeValue = default(CompileTimeValue);
                    return false;
                default:
                    compileTimeValue = default(CompileTimeValue);
                    return false;
            }
        }

        /// <summary>
        ///     Parses compile time constants from the specified <paramref name = "offset" /> until it encounters an instruction that is not compile-time-constant. Returns the compile time values in the order they appear in the code (the "right" order).
        /// </summary>
        /// <param name = "instructions">The code to parse from.</param>
        /// <param name = "localVariableMapping">The local variable mapping to apply when confronted with index-instructions (e.g., <code>ldloci</code>)</param>
        /// <param name = "offset">The offset of the first possible compile time constant.</param>
        /// <param name="cache">The cache to use. See <see cref="StackContext"/> or <see cref="Module"/> on how to get one.</param>
        /// <param name="internalModule">The module that contains the instructions.</param>
        /// <returns>the compile time values in the order they appear in the code (the "right" order).</returns>
        public static CompileTimeValue[] ParseSequenceReverse(IList<Instruction> instructions,
            IDictionary<int, string> localVariableMapping, int offset, CentralCache cache, ModuleName internalModule)
        {
            var compileTimeValues = new LinkedList<CompileTimeValue>();
            CompileTimeValue compileTimeValue;
            while (0 <= offset &&
                TryParse(instructions[offset--], localVariableMapping, cache, internalModule, out compileTimeValue))
                compileTimeValues.AddFirst(compileTimeValue);
            return compileTimeValues.ToArray();
        }

        #endregion

        public void EmitLoadAsPValue(CompilerState state)
        {
            switch (Interpretation)
            {
                case CompileTimeInterpretation.Null:
                    state.EmitLoadNullAsPValue();
                    break;
                case CompileTimeInterpretation.String:
                    string stringValue;
                    if (!TryGetString(out stringValue))
                        goto default;
                    state.EmitLoadStringAsPValue(stringValue);
                    break;
                case CompileTimeInterpretation.Int:
                    int intValue;
                    if (!TryGetInt(out intValue))
                        goto default;
                    state.EmitLoadIntAsPValue(intValue);
                    break;
                case CompileTimeInterpretation.Bool:
                    bool boolValue;
                    if (!TryGetBool(out boolValue))
                        goto default;
                    state.EmitLoadBoolAsPValue(boolValue);
                    break;
                case CompileTimeInterpretation.LocalVariableReference:
                    EntityRef.Variable.Local localVariable;
                    if (!TryGetLocalVariableReference(out localVariable))
                        goto default;
                    state.EmitLoadLocalRefAsPValue(localVariable);
                    break;
                case CompileTimeInterpretation.GlobalVariableReference:
                    EntityRef.Variable.Global globalVariable;
                    if (!TryGetGlobalVariableReference(out globalVariable))
                        goto default;
                    state.EmitLoadGlobalRefAsPValue(globalVariable);
                    break;
                case CompileTimeInterpretation.FunctionReference:
                    EntityRef.Function func;
                    if (!TryGetFunctionReference(out func))
                        goto default;
                    state.EmitLoadFuncRefAsPValue(func);
                    break;
                case CompileTimeInterpretation.CommandReference:
                    EntityRef.Command command;
                    if (!TryGetCommandReference(out command))
                        goto default;
                    state.EmitLoadCmdRefAsPValue(command);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}