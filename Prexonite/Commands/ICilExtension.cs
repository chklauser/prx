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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    ///     <para>An interface implemented by elements that require special treatment for CIL compilation.</para>
    ///     <para>CIL extensions have access to compile-time constant arguments and can provide customized CIL code as their implementation.</para>
    ///     <para>The implementation of this interface does not affect execution under the Prexonite VM.</para>
    ///     <para>Currently only commands are checked for CIL extensions.</para>
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil")]
    public interface ICilExtension
    {
        /// <summary>
        ///     Checks whether the static arguments and number of dynamic arguments are valid for the CIL extension. 
        /// 
        ///     <para>Returning false means that the CIL extension cannot provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler will fall back to  <see
        ///       cref = "ICilCompilerAware" /> and finally the built-in mechanisms.</para>
        ///     <para>Returning true means that the CIL extension can provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler may subsequently call <see
        ///      cref = "Implement" /> with the same set of arguments.</para>
        /// </summary>
        /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        /// <returns>true if the extension can provide a CIL implementation for the set of arguments; false otherwise</returns>
        bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc);

        /// <summary>
        ///     Implements the CIL extension in CIL for the supplied arguments. The CIL compiler guarantees to always first call <see
        ///      cref = "ValidateArguments" /> in order to establish whether the extension can actually implement a particular call.
        ///     Thus, this method does not have to verify <paramref name = "staticArgv" /> and <paramref name = "dynamicArgc" />.
        /// </summary>
        /// <param name = "state">The CIL compiler state. This object is used to emit instructions.</param>
        /// <param name = "ins">The instruction that "calls" the CIL extension. Usually a command call.</param>
        /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        void Implement(CompilerState state, Instruction ins, CompileTimeValue[] staticArgv,
            int dynamicArgc);
    }

    /// <summary>
    ///     The different interpretations of a compile-time value.
    /// </summary>
    public enum CompileTimeInterpretation
    {
        /// <summary>
        ///     A <code>null</code>-literal. Obtained from <code>ldc.null</code>. Null is the default interpretation.
        /// </summary>
        Null = 0,

        /// <summary>
        ///     A string literal. Obtained from <code>ldc.string</code>. Represented as <see cref = "string" />.
        /// </summary>
        String,

        /// <summary>
        ///     An integer literal. Obtained from <code>ldc.int</code>. Represented as <see cref = "int" />.
        /// </summary>
        Int,

        /// <summary>
        ///     A boolean literal. Obtained from <code>ldc.bool</code>. Represented as <see cref = "bool" />.
        /// </summary>
        Bool,

        /// <summary>
        ///     A local variable reference literal. Obtained from <code>ldr.loc</code> and <code>ldr.loci</code>. Represented as <see
        ///      cref = "string" />, the name of the local variable.
        /// </summary>
        LocalVariableReference,

        /// <summary>
        ///     A global variable reference literal. Obtained from <code>ldr.glob</code>. Represented as <see cref = "string" />, the name of the global variable.
        /// </summary>
        GlobalVariableReference,

        /// <summary>
        ///     A function reference literal. Obtained from <code>ldr.func</code>. Represented as <see cref = "string" />, the name of the function.
        /// </summary>
        FunctionReference,

        /// <summary>
        ///     A command reference literal. Obtained from <code>ldr.cmd</code>. Represented as <see cref = "string" />, the name of the command.
        /// </summary>
        CommandReference
    }

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
            SymbolInterpretations symKind;
            switch (Interpretation)
            {
                case CompileTimeInterpretation.LocalVariableReference:
                    symKind = SymbolInterpretations.LocalObjectVariable;
                    break;
                case CompileTimeInterpretation.GlobalVariableReference:
                    symKind = SymbolInterpretations.GlobalObjectVariable;
                    break;
                case CompileTimeInterpretation.FunctionReference:
                    symKind = SymbolInterpretations.Function;
                    break;
                case CompileTimeInterpretation.CommandReference:
                    symKind = SymbolInterpretations.Command;
                    break;
                default:
                    entry = null;
                    return false;
            }

            var id = Value as string;
            if (id != null)
            {
                entry = new SymbolEntry(symKind, id);
                return true;
            }
            else
            {
                entry = null;
                return false;
            }
        }

        public bool TryGetReference(StackContext sctx, out PValue result)
        {
            result = null;
            string id;
            switch (Interpretation)
            {
                case CompileTimeInterpretation.GlobalVariableReference:
                    id = (string) Value;
                    PVariable gvar;
                    if (sctx.ParentApplication.Variables.TryGetValue(id, out gvar))
                        result = sctx.CreateNativePValue(gvar);
                    break;
                case CompileTimeInterpretation.FunctionReference:
                    id = (string) Value;
                    PFunction func;
                    if (sctx.ParentApplication.Functions.TryGetValue(id, out func))
                        result = sctx.CreateNativePValue(func);
                    break;
                case CompileTimeInterpretation.CommandReference:
                    id = (string) Value;
                    PCommand cmd;
                    if (sctx.ParentEngine.Commands.TryGetValue(id, out cmd))
                        result = sctx.CreateNativePValue(cmd);
                    break;
            }

            return result != null;
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
        /// <param name = "commandAlias">If the conversion succeeds, <paramref name = "commandAlias" /> is set to the converted command reference. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetCommandReference(out string commandAlias)
        {
            commandAlias = null;
            return Interpretation == CompileTimeInterpretation.CommandReference &&
                (commandAlias = Value as string) != null;
        }

        /// <summary>
        ///     <para>Tries to extract a function reference from the compile time value. This will only work if the value is actually a function reference.</para>
        /// </summary>
        /// <param name = "functionId">If the conversion succeeds, <paramref name = "functionId" /> is set to the converted function reference. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetFunctionReference(out string functionId)
        {
            functionId = null;
            return Interpretation == CompileTimeInterpretation.FunctionReference &&
                (functionId = Value as string) != null;
        }

        /// <summary>
        ///     <para>Tries to extract a local variable reference from the compile time value. This will only work if the value is actually a local variable reference.</para>
        /// </summary>
        /// <param name = "localVariableId">If the conversion succeeds, <paramref name = "localVariableId" /> is set to the converted local variable reference. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetLocalVariableReference(out string localVariableId)
        {
            localVariableId = null;
            return Interpretation == CompileTimeInterpretation.LocalVariableReference &&
                (localVariableId = Value as string) != null;
        }

        /// <summary>
        ///     <para>Tries to extract a global variable reference from the compile time value. This will only work if the value is actually a global variable reference.</para>
        /// </summary>
        /// <param name = "globalVariableId">If the conversion succeeds, <paramref name = "globalVariableId" /> is set to the converted global variable reference. Otherwise its value is undefined.</param>
        /// <returns>True if the conversion succeeded; false otherwise</returns>
        public bool TryGetGlobalVariableReference(out string globalVariableId)
        {
            globalVariableId = null;
            return Interpretation == CompileTimeInterpretation.GlobalVariableReference &&
                (globalVariableId = Value as string) != null;
        }

        #endregion

        #region Parsing

        /// <summary>
        ///     Tries to parse the supplied instruction into a compile time value.
        /// </summary>
        /// <param name = "instruction">The instruction to parse.</param>
        /// <param name = "localVariableMapping">The local variable mapping to apply when confronted with index-instructions (e.g., <code>ldloci</code>)</param>
        /// <param name = "compileTimeValue">The parsed compile time value, if parsing was successful; undefined otherwise</param>
        /// <returns>True if parsing was successful; false otherwise</returns>
        public static bool TryParse(Instruction instruction,
            IDictionary<int, string> localVariableMapping, out CompileTimeValue compileTimeValue)
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
                    compileTimeValue.Value = instruction.Id;
                    return true;
                case OpCode.ldr_loci:
                    string id;
                    if (!localVariableMapping.TryGetValue(argc, out id) || id == null)
                        goto default;
                    compileTimeValue.Interpretation =
                        CompileTimeInterpretation.LocalVariableReference;
                    compileTimeValue.Value = id;
                    return true;
                case OpCode.ldr_glob:
                    compileTimeValue.Interpretation =
                        CompileTimeInterpretation.GlobalVariableReference;
                    compileTimeValue.Value = instruction.Id;
                    return true;
                case OpCode.ldr_func:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.FunctionReference;
                    compileTimeValue.Value = instruction.Id;
                    return true;
                case OpCode.ldr_cmd:
                    compileTimeValue.Interpretation = CompileTimeInterpretation.CommandReference;
                    compileTimeValue.Value = instruction.Id;
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
        /// <returns>the compile time values in the order they appear in the code (the "right" order).</returns>
        public static CompileTimeValue[] ParseSequenceReverse(IList<Instruction> instructions,
            IDictionary<int, string> localVariableMapping, int offset)
        {
            var compileTimeValues = new LinkedList<CompileTimeValue>();
            CompileTimeValue compileTimeValue;
            while (0 <= offset &&
                TryParse(instructions[offset--], localVariableMapping, out compileTimeValue))
                compileTimeValues.AddFirst(compileTimeValue);
            return compileTimeValues.ToArray();
        }

        #endregion

        public void EmitLoadAsPValue(CompilerState state)
        {
            string id;
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
                    if (!TryGetLocalVariableReference(out id))
                        goto default;
                    state.EmitLoadLocalRefAsPValue(id);
                    break;
                case CompileTimeInterpretation.GlobalVariableReference:
                    if (!TryGetGlobalVariableReference(out id))
                        goto default;
                    state.EmitLoadGlobalRefAsPValue(id);
                    break;
                case CompileTimeInterpretation.FunctionReference:
                    if (!TryGetFunctionReference(out id))
                        goto default;
                    state.EmitLoadFuncRefAsPValue(id);
                    break;
                case CompileTimeInterpretation.CommandReference:
                    if (!TryGetCommandReference(out id))
                        goto default;
                    state.EmitLoadCmdRefAsPValue(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}