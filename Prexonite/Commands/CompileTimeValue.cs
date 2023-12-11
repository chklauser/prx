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

using JetBrains.Annotations;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Modular;

namespace Prexonite.Commands;

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
    public object? Value;

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
    public bool TryGetSymbolEntry([NotNullWhen(true)] out SymbolEntry? entry)
    {
        if (Value is EntityRef er)
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

    [PublicAPI]
    public bool TryGetValue(StackContext sctx, [NotNullWhen(true)] out PValue? result)
    {
        EntityRef? er;
        if (IsReference && (er = Value as EntityRef) != null && er._TryLookup(sctx, out result))
            return true;
        else
            return TryGetConstant(out result);
    }

    public bool TryGetConstant([NotNullWhen(true)] out PValue? result)
    {
        result = null;
        switch (Interpretation)
        {
            case CompileTimeInterpretation.Null:
                result = PType.Null;
                break;
            case CompileTimeInterpretation.String:
                result = (string) Value!;
                break;
            case CompileTimeInterpretation.Int:
                TryGetInt(out var value);
                result = value;
                break;
            case CompileTimeInterpretation.Bool:
                TryGetBool(out var flag);
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
    public bool TryGetString([NotNullWhen(true)] out string? value)
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
    public bool TryGetBool(out bool value)
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
    public bool TryGetCommandReference([NotNullWhen(true)] out EntityRef.Command? command)
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
    public bool TryGetFunctionReference([NotNullWhen(true)] out EntityRef.Function? func)
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
    public bool TryGetLocalVariableReference([NotNullWhen(true)] out EntityRef.Variable.Local? localVariable)
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
    public bool TryGetGlobalVariableReference([NotNullWhen(true)] out EntityRef.Variable.Global? globalVariable)
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
        switch (instruction)
        {
            case { OpCode: OpCode.ldc_int }:
                compileTimeValue.Interpretation = CompileTimeInterpretation.Int;
                compileTimeValue.Value = (int?) argc;
                return true;
            case { OpCode: OpCode.ldc_real }:
                compileTimeValue = default;
                return false;
            case { OpCode: OpCode.ldc_bool }:
                compileTimeValue.Interpretation = CompileTimeInterpretation.Bool;
                compileTimeValue.Value = argc != 0;
                return true;
            case { OpCode: OpCode.ldc_string }:
                compileTimeValue.Interpretation = CompileTimeInterpretation.String;
                compileTimeValue.Value = instruction.Id;
                return true;
            case { OpCode: OpCode.ldc_null }:
                compileTimeValue.Interpretation = CompileTimeInterpretation.Null;
                compileTimeValue.Value = null;
                return true;
            case { OpCode: OpCode.ldr_loc, Id: {} varId }:
                compileTimeValue.Interpretation =
                    CompileTimeInterpretation.LocalVariableReference;
                compileTimeValue.Value = cache[EntityRef.Variable.Local.Create(varId)];
                return true;
            case { OpCode: OpCode.ldr_loci }:
                if (!localVariableMapping.TryGetValue(argc, out var id))
                    goto default;
                compileTimeValue.Interpretation =
                    CompileTimeInterpretation.LocalVariableReference;
                compileTimeValue.Value = cache[EntityRef.Variable.Local.Create(id)];
                return true;
            case { OpCode: OpCode.ldr_glob, Id: {} varId }:
                compileTimeValue.Interpretation =
                    CompileTimeInterpretation.GlobalVariableReference;
                compileTimeValue.Value = cache[EntityRef.Variable.Global.Create(varId, instruction.ModuleName ?? internalModule)];
                return true;
            case { OpCode: OpCode.ldr_func, Id: {} funcId }:
                compileTimeValue.Interpretation = CompileTimeInterpretation.FunctionReference;
                compileTimeValue.Value = cache[EntityRef.Function.Create(funcId, instruction.ModuleName ?? internalModule)];
                return true;
            case { OpCode: OpCode.ldr_cmd, Id: {} cmdId }:
                compileTimeValue.Interpretation = CompileTimeInterpretation.CommandReference;
                compileTimeValue.Value = cache[EntityRef.Command.Create(cmdId)];
                return true;
            case { OpCode: OpCode.ldr_app }:
                compileTimeValue = default;
                return false;
            case { OpCode: OpCode.ldr_eng }:
                compileTimeValue = default;
                return false;
            case { OpCode: OpCode.ldr_type }:
                compileTimeValue = default;
                return false;
            default:
                compileTimeValue = default;
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
        while (0 <= offset &&
               TryParse(instructions[offset--], localVariableMapping, cache, internalModule, out var compileTimeValue))
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
                if (!TryGetString(out var stringValue))
                    goto default;
                state.EmitLoadStringAsPValue(stringValue);
                break;
            case CompileTimeInterpretation.Int:
                if (!TryGetInt(out var intValue))
                    goto default;
                state.EmitLoadIntAsPValue(intValue);
                break;
            case CompileTimeInterpretation.Bool:
                if (!TryGetBool(out var boolValue))
                    goto default;
                state.EmitLoadBoolAsPValue(boolValue);
                break;
            case CompileTimeInterpretation.LocalVariableReference:
                if (!TryGetLocalVariableReference(out var localVariable))
                    goto default;
                state.EmitLoadLocalRefAsPValue(localVariable);
                break;
            case CompileTimeInterpretation.GlobalVariableReference:
                if (!TryGetGlobalVariableReference(out var globalVariable))
                    goto default;
                state.EmitLoadGlobalRefAsPValue(globalVariable);
                break;
            case CompileTimeInterpretation.FunctionReference:
                if (!TryGetFunctionReference(out var func))
                    goto default;
                state.EmitLoadFuncRefAsPValue(func);
                break;
            case CompileTimeInterpretation.CommandReference:
                if (!TryGetCommandReference(out var command))
                    goto default;
                state.EmitLoadCmdRefAsPValue(command);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}