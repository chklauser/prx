// /*
//  * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
//  *  Copyright (C) 2007  Christian "SealedSun" Klauser
//  *  E-mail  sealedsun a.t gmail d.ot com
//  *  Web     http://www.sealedsun.ch/
//  *
//  *  This program is free software; you can redistribute it and/or modify
//  *  it under the terms of the GNU General Public License as published by
//  *  the Free Software Foundation; either version 2 of the License, or
//  *  (at your option) any later version.
//  *
//  *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
//  * 
//  *  This program is distributed in the hope that it will be useful,
//  *  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  *  GNU General Public License for more details.
//  *
//  *  You should have received a copy of the GNU General Public License along
//  *  with this program; if not, write to the Free Software Foundation, Inc.,
//  *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//  */

#region Namespace Imports

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Commands;
using Prexonite.Types;
using CilException = Prexonite.PrexoniteException;

#endregion

namespace Prexonite.Compiler.Cil
{
    public delegate void Action();

    public static class Compiler
    {
        #region Public interface and LCG Setup

        public static void Compile(Application app, Engine targetEngine)
        {
            Compile(app, targetEngine, FunctionLinking.FullyStatic);
        }

        public static void Compile(Application app, Engine targetEngine, FunctionLinking linking)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            Compile(app.Functions, targetEngine, linking);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, Application app)
        {
            Compile(sctx, app, FunctionLinking.FullyStatic);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, Application app, FunctionLinking linking)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            Compile(app, sctx.ParentEngine, linking);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, List<PValue> lst)
        {
            Compile(sctx, lst, FunctionLinking.FullyStatic);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, List<PValue> lst, bool fullyStatic)
        {
            Compile(sctx, lst, fullyStatic ? FunctionLinking.FullyStatic : FunctionLinking.FullyIsolated);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Compile(StackContext sctx, List<PValue> lst, FunctionLinking linking)
        {
            if (lst == null)
                throw new ArgumentNullException("lst");
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            var functions = new List<PFunction>();
            foreach (var value in lst)
            {
                if (value == null)
                    continue;
                var T = value.Type.ToBuiltIn();
                PFunction func;
                switch (T)
                {
                    case PType.BuiltIn.String:
                        if (!sctx.ParentApplication.Functions.TryGetValue((string) value.Value, out func))
                            continue;
                        break;
                    case PType.BuiltIn.Object:
                        if (!value.TryConvertTo(sctx, false, out func))
                            continue;
                        break;
                    default:
                        continue;
                }
                functions.Add(func);
            }

            Compile(functions, sctx.ParentEngine, linking);
        }

        public static void Compile(IEnumerable<PFunction> functions, Engine targetEngine)
        {
            Compile(functions, targetEngine, FunctionLinking.FullyStatic);
        }

        public static void Compile(IEnumerable<PFunction> functions, Engine targetEngine, FunctionLinking linking)
        {
            CheckQualification(functions, targetEngine);

            var qfuncs = new List<PFunction>();

            //Get a list of qualifying functions
            foreach (var func in functions)
                if (!func.Meta.GetDefault(PFunction.VolatileKey, false))
                    qfuncs.Add(func);

            if (qfuncs.Count == 0)
                return; //No compilation to be done

            var pass = new CompilerPass(linking);

            //Generate method stubs
            foreach (var func in qfuncs)
                pass.DefineImplementationMethod(func.Id);

            //Emit IL
            foreach (var func in qfuncs)
            {
                _compile(func, CompilerPass.GetIlGenerator(pass.Implementations[func.Id]), targetEngine, pass, linking);
            }

            //Enable by name linking and link meta data to CIL implementations
            foreach (var func in qfuncs)
            {
                func.CilImplementation = pass.GetDelegate(func.Id);
                pass.LinkMetadata(func);
            }
        }

        public static bool TryCompile(PFunction func, Engine targetEngine)
        {
            return TryCompile(func, targetEngine, FunctionLinking.FullyStatic);
        }

        public static bool TryCompile(PFunction func, Engine targetEngine, FunctionLinking linking)
        {
            if (CheckQualification(func, targetEngine))
            {
                var pass = new CompilerPass(func.ParentApplication, linking);

                var m = pass.DefineImplementationMethod(func.Id);
                var il = CompilerPass.GetIlGenerator(m);

                _compile(func, il, targetEngine, pass, linking);

                func.CilImplementation = pass.GetDelegate(m);
                pass.LinkMetadata(func);

                return true;
            }
            return false;
        }

        #endregion

        #region Store debug implementation

        public static void StoreDebugImplementation(StackContext sctx)
        {
            StoreDebugImplementation(sctx.ParentApplication, sctx.ParentEngine);
        }

        public static void StoreDebugImplementation(StackContext sctx, Application app)
        {
            StoreDebugImplementation(app, sctx.ParentEngine);
        }

        public static void StoreDebugImplementation(Application app, Engine targetEngine)
        {
            CheckQualification(app.Functions, targetEngine);

            var linking = FunctionLinking.FullyStatic;
            var pass = new CompilerPass(linking);

            var qfuncs = new List<PFunction>();
            foreach (var func in app.Functions)
            {
                if (!func.Meta.GetDefault(PFunction.VolatileKey, false))
                {
                    qfuncs.Add(func);
                    pass.DefineImplementationMethod(func.Id);
                }
            }

            foreach (var func in qfuncs)
            {
                _compile(func, pass.GetIlGenerator(func.Id), targetEngine, pass, linking);
            }

            pass.Type.CreateType();

            pass.Assembly.Save(pass.Assembly.GetName().Name + ".dll");
        }

        public static void StoreDebugImplementation(PFunction func, Engine targetEngine)
        {
            var linking = FunctionLinking.FullyStatic;
            var pass = new CompilerPass(linking);

            var m = pass.DefineImplementationMethod(func.Id);

            var il = CompilerPass.GetIlGenerator(m);

            _compile(func, il, targetEngine, pass, linking);

            pass.Type.CreateType();

            //var sm = tb.DefineMethod("whoop", MethodAttributes.Static | MethodAttributes.Public);

            //ab.SetEntryPoint(sm);
            pass.Assembly.Save(pass.Assembly.GetName().Name + ".dll");
        }

        public static void StoreDebugImplementation(StackContext sctx, PFunction func)
        {
            StoreDebugImplementation(func, sctx.ParentEngine);
        }

        #endregion

        #region Check Qualification

        private static bool CheckQualification(PFunction source, Engine targetEngine)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            lock (source)
            {
                string reason;
                var qualifies = _check(source, targetEngine, out reason);
                _register_check_results(source, qualifies, reason);
                return qualifies;
            }
        }

        private static void _register_check_results(IHasMetaTable source, bool qualifies, string reason)
        {
            if (!qualifies && source.Meta[PFunction.DeficiencyKey].Text == "" && reason != null)
            {
                source.Meta[PFunction.DeficiencyKey] = reason;
            } //else nothing
            if ((!qualifies) || source.Meta.ContainsKey(PFunction.VolatileKey))
            {
                source.Meta[PFunction.VolatileKey] = !qualifies;
            }
        }

        private class TailCallHint
        {
            private readonly int indexOfReference;
            private readonly int indexOfCall;
            private readonly Instruction actualCall;

            internal TailCallHint(int indexOfReference, int indexOfCall, Instruction actualCall)
            {
                this.indexOfReference = indexOfReference;
                this.actualCall = actualCall;
                this.indexOfCall = indexOfCall;
            }


            public int IndexOfReference
            {
                get { return indexOfReference; }
            }

            public int IndexOfCall
            {
                get { return indexOfCall; }
            }

            public Instruction ActualCall
            {
                get { return actualCall; }
            }
        }

        private static IEnumerable<TailCallHint> findTailCalls(PFunction source)
        {
            MetaEntry cilHintEntry;
            if (source.Meta.TryGetValue(Loader.CilHintsKey, out cilHintEntry))
            {
                foreach (var hintEntry in cilHintEntry.List)
                {
                    var hint = hintEntry.List;
                    if (hint.Length < 1)
                        continue;
                    if (hint[0].Text == Loader.TailCallHintKey)
                    {
                        if (hint.Length < Loader.TailCallHintLength)
                            throw new PrexoniteException(source + " has an invalid tail call CIL hint.");
                        var refAddr = int.Parse(hint[Loader.TailCallHintReferenceIndex].Text);
                        var callAddr = int.Parse(hint[Loader.TailCallHintCallIndex].Text);
                        var tailCall = source.Code[callAddr];
                        var actualCall = tailCall.Clone();
                        var type = hint[Loader.TailCallHintTypeIndex].Text;
                        switch (type)
                        {
                            case "cmd":
                                actualCall.OpCode = OpCode.cmd;
                                break;
                            case "func":
                                actualCall.OpCode = OpCode.func;
                                break;
                            default:
                                throw new PrexoniteException
                                    (source + "has a tail call CIL hint with an unknown type " + type);
                        }
                        actualCall.Id = hint[Loader.TailCallHintSymbolIndex].Text;

                        yield return new TailCallHint(refAddr, callAddr, actualCall);
                    }
                }
            }
        }

        private static void CheckQualification(IEnumerable<PFunction> functions, Engine targetEngine)
        {
            //Whole program compatibility analysis
            foreach (var source in functions)
            {
                if (source.Meta[PFunction.VolatileKey].Switch || source.Meta.ContainsKey(PFunction.DynamicKey))
                    continue;

                //Handle command calls via tail
                foreach (var call in findTailCalls(source))
                    if (call.ActualCall.OpCode == OpCode.cmd)
                        handle_possibly_dynamic_command(source, call.ActualCall, targetEngine);

                //Handle 'normal' command calls (cmd instruction)
                foreach (var ins in source.Code)
                    switch (ins.OpCode)
                    {
                        case OpCode.cmd:
                            handle_possibly_dynamic_command(source, ins, targetEngine);
                            break;
                    }
            }

            //Check qualifications
            foreach (var func in functions)
            {
                string reason;
                var qualifies = _check(func, targetEngine, out reason);
                _register_check_results(func, qualifies, reason);
            }
        }

        private static void handle_possibly_dynamic_command(IHasMetaTable source, Instruction ins, Engine targetEngine)
        {
            PCommand cmd;
            ICilCompilerAware aware;
            if (targetEngine.Commands.TryGetValue(ins.Id, out cmd) &&
                (aware = cmd as ICilCompilerAware) != null)
            {
                var flags = aware.CheckQualification(ins);
                if ((flags & CompilationFlags.OperatesOnCaller) == CompilationFlags.OperatesOnCaller)
                    source.Meta[PFunction.DynamicKey] = true;
            }
        }


        private static bool _check(PFunction source, Engine targetEngine, out string reason)
        {
            reason = null;
            if (source == null)
                throw new ArgumentNullException("source");
            if (targetEngine == null)
                throw new ArgumentNullException("targetEngine");
            //Application does not allow cil compilation
            if ((!source.Meta.ContainsKey(PFunction.VolatileKey)) &&
                source.ParentApplication.Meta[PFunction.VolatileKey].Switch)
            {
                reason = "Application does not allow cil compilation";
                return false;
            }
            //Function does not allow cil compilation
            if (source.Meta[PFunction.VolatileKey].Switch)
                return false;

            //Check tail calls for dynamic functions
            foreach (var call in findTailCalls(source))
            {
                PFunction func;
                if (source.ParentApplication.Functions.TryGetValue(call.ActualCall.Id, out func) &&
                    func.Meta[PFunction.DynamicKey].Switch)
                {
                    reason = "Uses dynamic function " + call.ActualCall.Id;
                    return false;
                }
            }

            //Not supported instructions
            for (var address = 0; address < source.Code.Count; address++)
            {
                var ins = source.Code[address];
                switch (ins.OpCode)
                {
                    case OpCode.cmd:
                        //Check for commands that are not compatible.
                        PCommand cmd;
                        ICilCompilerAware aware;
                        if (targetEngine.Commands.TryGetValue(ins.Id, out cmd) &&
                            (aware = cmd as ICilCompilerAware) != null)
                        {
                            var flags = aware.CheckQualification(ins);
                            if (flags == CompilationFlags.IsIncompatible) //Incompatible and no workaround
                            {
                                reason = "Incompatible command " + cmd;
                                return false;
                            }
                        }
                        break;
                    case OpCode.func:
                        //Check for functions that use dynamic features
                        PFunction func;
                        if (source.ParentApplication.Functions.TryGetValue(ins.Id, out func) &&
                            func.Meta[PFunction.DynamicKey].Switch)
                        {
                            reason = "Uses dynamic function " + ins.Id;
                            return false;
                        }
                        break;
                    case OpCode.ret_break:
                    case OpCode.ret_continue:
                    case OpCode.invalid:
                        reason = "Unsupported instruction " + ins;
                        return false;
                    case OpCode.newclo:
                        //Function must already be available
                        if (!source.ParentApplication.Functions.Contains(ins.Id))
                        {
                            reason = "Enclosed function " + ins.Id + " must already be compiled";
                            return false;
                        }
                        break;
                    case OpCode.@try:
                        //must be the first instruction of a try block
                        var isCorrect = false;
                        foreach (var block in source.TryCatchFinallyBlocks)
                        {
                            if (block.BeginTry == address)
                            {
                                isCorrect = true;
                                break;
                            }
                        }
                        if (!isCorrect)
                        {
                            reason = "try instruction is not the first instruction of a guarded block.";
                            return false;
                        }
                        break;
                    case OpCode.exc:
                        //must be the first instruction of a catch block
                        isCorrect = false;
                        foreach (var block in source.TryCatchFinallyBlocks)
                        {
                            if (block.BeginCatch == address)
                            {
                                isCorrect = true;
                                break;
                            }
                        }
                        if (!isCorrect)
                        {
                            reason = "exc instruction is not the first instruction of a catch clause.";
                            return false;
                        }
                        break;
                    case OpCode.leave:
                        //must either be at the end of a finally or a try block without a finally one.
                        //must point to the instruction after the tryfinallycatch
                        isCorrect = false;
                        foreach (var block in source.TryCatchFinallyBlocks)
                        {
                            int lastOfTry;
                            if (block.HasFinally)
                                lastOfTry = -1; //<-- must not be last of try if finally exists
                            else if (block.HasCatch)
                                lastOfTry = block.BeginCatch - 1;
                            else
                                lastOfTry = block.EndTry - 1;

                            int lastOfFinally;
                            if (!block.HasFinally)
                                lastOfFinally = -1;
                            else if (block.HasCatch)
                                lastOfFinally = block.BeginCatch - 1;
                            else
                                lastOfFinally = block.EndTry - 1;

                            if (ins.Arguments == block.EndTry &&
                                (address == lastOfTry || address == lastOfFinally))
                            {
                                isCorrect = true;
                                break;
                            }
                        }
                        if (!isCorrect)
                        {
                            reason =
                                "leave instruction not in the right place (last instruction of regular control flow in try-catch-finally)";
                            return false;
                        }
                        break;
                }
            }

            //Otherwise, qualification passed.
            return true;
        }

        #endregion

        #region Compile Function

        private static void _compile
            (PFunction _source, ILGenerator il, Engine targetEngine, CompilerPass pass, FunctionLinking linking)
        {
            var state = new CompilerState(_source, targetEngine, il, pass, linking);

            //Every cil implementation needs to instantiate a CilFunctionContext and assign PValue.Null to the result.
            emit_cil_implementation_header(state);

            //Reads the functions metadata about parameters, local variables and shared variables.
            //initializes shared variables.
            build_symbol_table(state);

            //CODE ANALYSIS
            //  - determine number of temporary variables
            //  - find variable references (alters the symbol table)
            analysis_and_preparation(state);

            //Create and initialize local variables for parameters
            parse_parameters(state);

            //Shared variables and parameters have already been initialized
            // this method initializes (PValue.Null) the rest.
            _create_and_initialize_remaining_locals(state);

            //Emits IL for the functions Prexonite byte code.
            emit_instructions(state);
        }

        private static void emit_cil_implementation_header(CompilerState state)
        {
            //Create local cil function stack context
            //  CilFunctionContext cfctx = CilFunctionContext.New(sctx, source);
            state.SctxLocal = state.Il.DeclareLocal(typeof (CilFunctionContext));
            state.EmitLoadArg(CompilerState.ParamSctxIndex);
            state.EmitLoadArg(CompilerState.ParamSourceIndex);
            state.Il.EmitCall(OpCodes.Call, CilFunctionContext.NewMethod, null);
            state.EmitStoreLocal(state.SctxLocal.LocalIndex);

            //Initialize result
            //  Result = null;
            state.EmitLoadArg(CompilerState.ParamResultIndex);
            state.EmitLoadPValueNull();
            state.Il.Emit(OpCodes.Stind_Ref);
        }

        private static void build_symbol_table(CompilerState state)
        {
            //Create local ref variables for shared names
            //  and populate them with the contents of the sharedVariables parameter
            if (state.Source.Meta.ContainsKey(PFunction.SharedNamesKey))
            {
                var sharedNames = state.Source.Meta[PFunction.SharedNamesKey].List;
                for (var i = 0; i < sharedNames.Length; i++)
                {
                    if (state.Source.Variables.Contains(sharedNames[i]))
                        continue; //Arguments are redeclarations.
                    var sym = new Symbol(SymbolKind.LocalRef)
                    {
                        Local = state.Il.DeclareLocal(typeof (PVariable))
                    };
                    var id = sharedNames[i].Text;

                    state.EmitLoadArg(CompilerState.ParamSharedVariablesIndex);
                    state.Il.Emit(OpCodes.Ldc_I4, i);
                    state.Il.Emit(OpCodes.Ldelem_Ref);
                    state.EmitStoreLocal(sym.Local.LocalIndex);

                    state.Symbols.Add(id, sym);
                }
            }

            //Create index -> id map
            foreach (var mapping in state.Source.LocalVariableMapping)
                state.IndexMap.Add(mapping.Value, mapping.Key);

            //Add entries for paramters
            foreach (var parameter in state.Source.Parameters)
                if (!state.Symbols.ContainsKey(parameter))
                    state.Symbols.Add(parameter, new Symbol(SymbolKind.Local));

            //Add entries for enumerator variables
            foreach (var hint in state.ForeachHints)
            {
                if (state.Symbols.ContainsKey(hint.EnumVar))
                    throw new PrexoniteException("Invalid foreach hint. Enumerator variable is shared.");
                state.Symbols.Add(hint.EnumVar, new Symbol(SymbolKind.LocalEnum));
            }

            //Add entries for non-shared local variables
            foreach (var variable in state.Source.Variables)
                if (!state.Symbols.ContainsKey(variable))
                    state.Symbols.Add(variable, new Symbol(SymbolKind.Local));
        }

        private static void analysis_and_preparation(CompilerState state)
        {
            var tempMaxOrder = 1; // 
            var needsSharedVariables = false;
            var sourceCode = state.Source.Code;
            foreach (var ins in sourceCode)
            {
                string toConvert;
                switch (ins.OpCode)
                {
                    case OpCode.ldr_loci:
                        //see ldr_loc
                        toConvert = state.IndexMap[ins.Arguments];
                        goto Convert;
                    case OpCode.ldr_loc:
                        toConvert = ins.Id;
                        Convert:

                        //Normal local variables are implemented as CIL locals.
                        // If the function uses variable references, they must be converted to reference variables.
                        state.Symbols[toConvert].Kind = SymbolKind.LocalRef;
                        break;
                    case OpCode.rot:
                        //Determine the maximum number of temporary variables for the implementation of rot[ate]
                        var order = (int) ins.GenericArgument;
                        if (order > tempMaxOrder)
                            tempMaxOrder = order;
                        break;
                    case OpCode.newclo:
                        MetaEntry[] entries;
                        var func = state.Source.ParentApplication.Functions[ins.Id];
                        MetaEntry entry;
                        if (func.Meta.ContainsKey(PFunction.SharedNamesKey) &&
                            (entry = func.Meta[PFunction.SharedNamesKey]).IsList)
                            entries = entry.List;
                        else
                            entries = new MetaEntry[] {};
                        for (var i = 0; i < entries.Length; i++)
                        {
                            var symbolName = entries[i].Text;
                            if (!state.Symbols.ContainsKey(symbolName))
                                throw new PrexoniteException
                                    (func + " does not contain a mapping for the symbol " + symbolName);

                            //In order for variables to be shared, they too, need to be converted to reference locals.
                            state.Symbols[symbolName].Kind = SymbolKind.LocalRef;
                        }

                        //Notify the compiler of the presence of closures with shared variables
                        needsSharedVariables = needsSharedVariables || entries.Length > 0;
                        break;
                }
            }

            //Create temporary variables for rotation
            state.TempLocals = new LocalBuilder[tempMaxOrder];
            for (var i = 0; i < tempMaxOrder; i++)
            {
                var rot_temp = state.Il.DeclareLocal(typeof (PValue));
                state.TempLocals[i] = rot_temp;
            }

            //Create temporary variable for argv and sharedVariables
            state.ArgvLocal = state.Il.DeclareLocal(typeof (PValue[]));
            state.SharedLocal = needsSharedVariables
                                    ? state.Il.DeclareLocal(typeof (PVariable[]))
                                    : null;

            //Create argc local variable and initialize it, if needed
            if (state.Source.Parameters.Count > 0)
            {
                state.ArgcLocal = state.Il.DeclareLocal(typeof (Int32));
                state.EmitLoadArg(CompilerState.ParamArgsIndex);
                state.Il.Emit(OpCodes.Ldlen);
                state.Il.Emit(OpCodes.Conv_I4);
                state.EmitStoreLocal(state.ArgcLocal);
            }
        }

        private static void parse_parameters(CompilerState state)
        {
            for (var i = 0; i < state.Source.Parameters.Count; i++)
            {
                var id = state.Source.Parameters[i];
                var sym = state.Symbols[id];
                LocalBuilder local;

                //Determine whether local variables for parameters have already been created and create them if necessary
                if (sym.Kind == SymbolKind.Local)
                {
                    if (sym.Local == null)
                        local = state.Il.DeclareLocal(typeof (PValue));
                    else
                        local = sym.Local;
                }
                else if (sym.Kind == SymbolKind.LocalRef)
                {
                    if (sym.Local == null)
                    {
                        local = state.Il.DeclareLocal(typeof (PVariable));
                        state.Il.Emit(OpCodes.Newobj, newPVariableCtor);
                        state.EmitStoreLocal(local);
                        //PVariable objects already contain PValue.Null and need not be initialized if no
                        //  argument has been passed.
                    }
                    else
                    {
                        local = sym.Local;
                    }
                }
                else
                {
                    throw new PrexoniteException("Cannot create variable to represent symbol");
                }

                sym.Local = local;

                var hasArg = state.Il.DefineLabel();
                var end = state.Il.DefineLabel();

                if (sym.Kind == SymbolKind.Local) // var = idx < len ? args[idx] : null;
                {
                    //The closure below is only accessed once. The capture is therefore transparent.
                    // ReSharper disable AccessToModifiedClosure
                    state.EmitStorePValue
                        (
                        sym,
                        delegate
                        {
                            //(idx < argc) ? args[idx] : null; 
                            state.EmitLdcI4(i);
                            state.EmitLoadLocal(state.ArgcLocal);
                            state.Il.Emit(OpCodes.Blt_S, hasArg);
                            state.EmitLoadPValueNull();
                            state.Il.Emit(OpCodes.Br_S, end);
                            state.Il.MarkLabel(hasArg);
                            state.EmitLoadArg(CompilerState.ParamArgsIndex);
                            state.EmitLdcI4(i);
                            state.Il.Emit(OpCodes.Ldelem_Ref);
                            state.Il.MarkLabel(end);
                        }
                        );
                    // ReSharper restore AccessToModifiedClosure
                }
                else // if(idx < len) var = args[idx];
                {
                    state.EmitLdcI4(i);
                    state.EmitLoadLocal(state.ArgcLocal);
                    state.Il.Emit(OpCodes.Bge_S, end);

                    //The following closure is only accessed once. The capture is therefore transparent.
                    // ReSharper disable AccessToModifiedClosure
                    state.EmitStorePValue
                        (
                        sym,
                        delegate
                        {
                            state.EmitLoadArg(CompilerState.ParamArgsIndex);
                            state.EmitLdcI4(i);
                            state.Il.Emit(OpCodes.Ldelem_Ref);
                        });
                    // ReSharper restore AccessToModifiedClosure
                    state.Il.MarkLabel(end);
                }
            }
        }

        private static void _create_and_initialize_remaining_locals(CompilerState state)
        {
            var nullLocals = new List<LocalBuilder>();

            //Create remaining local variables and initialize them
            foreach (var pair in state.Symbols)
            {
                var id = pair.Key;
                var sym = pair.Value;
                if (sym.Local != null)
                    continue;

                switch (sym.Kind)
                {
                    case SymbolKind.Local:
                    {
                        sym.Local = state.Il.DeclareLocal(typeof (PValue));
                        var initVal = GetVariableInitialization(state, id, false);
                        switch (initVal)
                        {
                            case VariableInitialization.ArgV:
                                EmitLoadArgV(state);
                                state.EmitStoreLocal(sym.Local);
                                break;
                            case VariableInitialization.Null:
                                nullLocals.Add(sym.Local); //defer assignment
                                break;

                            case VariableInitialization.None:
                            default:
                                break;
                        }
                    }
                        break;
                    case SymbolKind.LocalRef:
                    {
                        sym.Local = state.Il.DeclareLocal(typeof (PVariable));
                        var initVal = GetVariableInitialization(state, id, true);

                        var idx = sym.Local.LocalIndex;

                        state.Il.Emit(OpCodes.Newobj, newPVariableCtor);
                        state.EmitStoreLocal(idx);

                        if (initVal != VariableInitialization.None)
                        {
                            state.EmitLoadLocal(idx);
                            switch (initVal)
                            {
                                case VariableInitialization.ArgV:
                                    EmitLoadArgV(state);
                                    break;
                                case VariableInitialization.Null:
                                    state.EmitLoadPValueNull();
                                    break;

                                default:
                                    break;
                            }
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                    }
                        break;
                    case SymbolKind.LocalEnum:
                    {
                        sym.Local = state.Il.DeclareLocal(typeof (IEnumerator<PValue>));
                        //No initialization needed.
                    }
                        break;
                    default:
                        throw new PrexoniteException("Cannot initialize unknown symbol kind.");
                }
            }

            //Initialize null locals
            var nullCount = nullLocals.Count;
            if (nullCount > 0)
            {
                state.EmitLoadPValueNull();
                for (var i = 0; i < nullCount; i++)
                {
                    var local = nullLocals[i];
                    if (i + 1 != nullCount)
                        state.Il.Emit(OpCodes.Dup);
                    state.EmitStoreLocal(local);
                }
            }
        }

        private static void emit_instructions(CompilerState state)
        {
            //Tables of tail call hint hooks
            var tailReferences = new Dictionary<int, TailCallHint>();
            var tailCalls = new Dictionary<int, TailCallHint>();

            foreach (var hint in findTailCalls(state.Source))
            {
                tailReferences.Add(hint.IndexOfReference, hint);
                tailCalls.Add(hint.IndexOfCall, hint);
            }

            //Tables of foreach call hint hooks
            var foreachCasts = new Dictionary<int, ForeachHint>();
            var foreachGetCurrents = new Dictionary<int, ForeachHint>();
            var foreachMoveNexts = new Dictionary<int, ForeachHint>();
            var foreachDisposes = new Dictionary<int, ForeachHint>();

            foreach (var hint in state.ForeachHints)
            {
                foreachCasts.Add(hint.CastAddress, hint);
                foreachGetCurrents.Add(hint.GetCurrentAddress, hint);
                foreachMoveNexts.Add(hint.MoveNextAddress, hint);
                foreachDisposes.Add(hint.DisposeAddress, hint);
            }

            //Used to prevent duplicate ret OpCodes at the end of the compiled function.
            var lastWasRet = false;

            var sourceCode = state.Source.Code;
            for (var instructionIndex = 0; instructionIndex < sourceCode.Count; instructionIndex++)
            {
                #region Handling for try-finally-catch blocks

                //Handle try-finally-catch blocks
                foreach (var block in state.Source.TryCatchFinallyBlocks)
                {
                    if (instructionIndex == block.BeginTry)
                    {
                        state.TryBlocks.Push(block);
                        if (block.HasFinally)
                            state.Il.BeginExceptionBlock();
                        if (block.HasCatch)
                            state.Il.BeginExceptionBlock();
                    }
                    else if (instructionIndex == block.BeginFinally)
                    {
                        state.Il.BeginFinallyBlock();
                    }
                    else if (instructionIndex == block.BeginCatch)
                    {
                        if (block.HasFinally)
                            state.Il.EndExceptionBlock(); //end finally here
                        state.Il.BeginCatchBlock(typeof (Exception));
                        //parse the exception
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.ParseExceptionMethod, null);
                        //user code will store it in a local variable
                    }
                    else if (instructionIndex == block.EndTry)
                    {
                        if (block.HasFinally || block.HasCatch)
                            state.Il.EndExceptionBlock();
                        state.TryBlocks.Pop();
                    }
                }

                #endregion

                state.MarkInstruction(instructionIndex);

                lastWasRet = false;

                var ins = sourceCode[instructionIndex];

                #region CIL hints

                // **** CIL hints ****
                //  * Tail call *
                TailCallHint tailCallHint;
                ForeachHint hint;
                if (tailReferences.ContainsKey(instructionIndex))
                {
                    //Do not load that reference (yes that means ignoring it)

                    //However, we can use this spot to load the address of the return value already
                    state.EmitLoadArg(CompilerState.ParamResultIndex);
                    //This will save us some rotating
                    continue;
                }
                else if (tailCalls.TryGetValue(instructionIndex, out tailCallHint))
                {
                    //Emit code for the actual call by repacing the 
                    ins = tailCallHint.ActualCall;
                }
                    //  * Foreach *
                else if (foreachCasts.TryGetValue(instructionIndex, out hint))
                {
                    //result of (expr).GetEnumerator on the stack
                    //cast IEnumerator
                    state.EmitLoadLocal(state.SctxLocal);
                    state.Il.EmitCall(OpCodes.Call, Runtime.ExtractEnumeratorMethod, null);
                    instructionIndex++;
                    //stloc enum
                    state.EmitStoreLocal(state.Symbols[hint.EnumVar].Local);
                    continue;
                }
                else if (foreachGetCurrents.TryGetValue(instructionIndex, out hint))
                {
                    //ldloc enum
                    state.EmitLoadLocal(state.Symbols[hint.EnumVar].Local);
                    instructionIndex++;
                    //get.0 Current
                    state.Il.EmitCall(OpCodes.Callvirt, ForeachHint.GetCurrentMethod, null);
                    //result will be stored by user code
                    continue;
                }
                else if (foreachMoveNexts.TryGetValue(instructionIndex, out hint))
                {
                    //ldloc enum
                    state.EmitLoadLocal(state.Symbols[hint.EnumVar].Local);
                    instructionIndex++;
                    //get.0 MoveNext
                    state.Il.EmitCall(OpCodes.Callvirt, ForeachHint.MoveNextMethod, null);
                    instructionIndex++;
                    //jump.t begin
                    var target = sourceCode[instructionIndex].Arguments; //read from user code
                    state.Il.Emit(OpCodes.Brtrue, state.InstructionLabels[target]);
                    continue;
                }
                else if (foreachDisposes.TryGetValue(instructionIndex, out hint))
                {
                    //ldloc enum
                    state.EmitLoadLocal(state.Symbols[hint.EnumVar].Local);
                    instructionIndex++;
                    //@cmd.1 dispose
                    state.Il.EmitCall(OpCodes.Callvirt, ForeachHint.DisposeMethod, null);
                    continue;
                }

                #endregion

                //  * Normal code generation *
                //Decode instruction
                var argc = ins.Arguments;
                var justEffect = ins.JustEffect;
                var id = ins.Id;
                int idx;
                string methodId;
                string typeExpr;

                //Emit code for the instruction
                var primaryTempLocal = state.TempLocals[0];
                switch (ins.OpCode)
                {
                        #region NOP

                        //NOP
                    case OpCode.nop:
                        //Do nothing
                        state.Il.Emit(OpCodes.Nop);
                        break;

                        #endregion

                        #region LOAD

                        #region LOAD CONSTANT

                        //LOAD CONSTANT
                    case OpCode.ldc_int:
                        state.EmitLdcI4(argc);
                        state.EmitWrapInt();
                        break;
                    case OpCode.ldc_real:
                        state.Il.Emit(OpCodes.Ldc_R8, (double) ins.GenericArgument);
                        state.EmitWrapReal();
                        break;
                    case OpCode.ldc_bool:
                        if (argc != 0)
                            state.EmitLdcI4(1);
                        else
                            state.EmitLdcI4(0);
                        state.EmitWrapBool();
                        break;
                    case OpCode.ldc_string:
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.EmitWrapString();
                        break;

                    case OpCode.ldc_null:
                        state.EmitLoadPValueNull();
                        break;

                        #endregion LOAD CONSTANT

                        #region LOAD REFERENCE

                        //LOAD REFERENCE
                    case OpCode.ldr_loc:
                        state.EmitLoadLocal(state.Symbols[id].Local);
                        state.Il.EmitCall(OpCodes.Call, Runtime.WrapPVariableMethod, null);
                        break;
                    case OpCode.ldr_loci:
                        id = state.IndexMap[argc];
                        goto case OpCode.ldr_loc;
                    case OpCode.ldr_glob:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall
                            (
                            OpCodes.Call, Runtime.LoadGlobalVariableReferenceAsPValueMethod, null);
                        break;
                    case OpCode.ldr_func:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall
                            (
                            OpCodes.Call, Runtime.LoadFunctionReferenceMethod, null);
                        break;
                    case OpCode.ldr_cmd:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, Runtime.LoadCommandReferenceMethod, null);
                        break;
                    case OpCode.ldr_app:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall
                            (
                            OpCodes.Call, Runtime.LoadApplicationReferenceMethod, null);
                        break;
                    case OpCode.ldr_eng:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.LoadEngineReferenceMethod, null);
                        break;
                    case OpCode.ldr_type:
                        MakePTypeFromExpr(state, id);
                        break;

                        #endregion //LOAD REFERENCE

                        #endregion //LOAD

                        #region VARIABLES

                        #region LOCAL

                        //LOAD LOCAL VARIABLE
                    case OpCode.ldloc:
                        var sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitLoadLocal(sym.Local.LocalIndex);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitLoadLocal(sym.Local.LocalIndex);
                            state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                        }
                        break;
                    case OpCode.stloc:
                        sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitStoreLocal(sym.Local.LocalIndex);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitStoreLocal(primaryTempLocal.LocalIndex);
                            state.EmitLoadLocal(sym.Local.LocalIndex);
                            state.EmitLoadLocal(primaryTempLocal.LocalIndex);
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                        break;

                    case OpCode.ldloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.ldloc;

                    case OpCode.stloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.stloc;

                        #endregion

                        #region GLOBAL

                        //LOAD GLOBAL VARIABLE
                    case OpCode.ldglob:
                        state.EmitLoadGlobalValue(id);
                        break;
                    case OpCode.stglob:
                        state.EmitStoreLocal(primaryTempLocal.LocalIndex);
                        state.EmitLoadLocal(state.SctxLocal.LocalIndex);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall
                            (
                            OpCodes.Call, Runtime.LoadGlobalVariableReferenceMethod, null);
                        state.EmitLoadLocal(primaryTempLocal.LocalIndex);
                        state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        break;

                        #endregion

                        #endregion

                        #region CONSTRUCTION

                        //CONSTRUCTION
                    case OpCode.newobj:
                        state.EmitNewObj(id, argc);
                        break;
                    case OpCode.newtype:
                        state.fillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.readArgv(argc);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, Runtime.NewTypeMethod, null);
                        break;

                    case OpCode.newclo:
                        //Collect shared variables
                        MetaEntry[] entries;
                        var func = state.Source.ParentApplication.Functions[id];
                        if (func.Meta.ContainsKey(PFunction.SharedNamesKey))
                            entries = func.Meta[PFunction.SharedNamesKey].List;
                        else
                            entries = new MetaEntry[] {};
                        var hasSharedVariables = entries.Length > 0;
                        if (hasSharedVariables)
                        {
                            state.EmitLdcI4(entries.Length);
                            state.Il.Emit(OpCodes.Newarr, typeof (PVariable));
                            state.EmitStoreLocal(state.SharedLocal);
                            for (var i = 0; i < entries.Length; i++)
                            {
                                state.EmitLoadLocal(state.SharedLocal);
                                state.EmitLdcI4(i);
                                state.EmitLoadLocal(state.Symbols[entries[i].Text].Local);
                                state.Il.Emit(OpCodes.Stelem_Ref);
                            }
                        }
                        state.EmitLoadLocal(state.SctxLocal);
                        if (hasSharedVariables)
                            state.EmitLoadLocal(state.SharedLocal);
                        else
                            state.Il.Emit(OpCodes.Ldnull);

                        MethodInfo dummy;
                        if (TryGetStaticallyLinkedFunction(state, id, out dummy))
                        {
                            state.Il.Emit(OpCodes.Ldsfld,state.Pass.FunctionFields[id]);
                            state.Il.EmitCall(OpCodes.Call, Runtime.newClosureMethod_StaticallyBound, null);
                        }
                        else
                        {
                            state.Il.Emit(OpCodes.Ldstr, id);
                            state.Il.EmitCall(OpCodes.Call, Runtime.newClosureMethod_LateBound, null);
                        }
                        break;

                    case OpCode.newcor:
                        state.fillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.readArgv(argc);
                        state.Il.EmitCall(OpCodes.Call, Runtime.NewCoroutineMethod, null);
                        break;

                        #endregion

                        #region OPERATORS

                        #region UNARY

                        //UNARY OPERATORS
                    case OpCode.incloc:
                        sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVIncrementMethod, null);
                            state.EmitStoreLocal(sym.Local);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.Il.Emit(OpCodes.Dup);
                            state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVIncrementMethod, null);
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                        break;

                    case OpCode.incloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.incloc;

                    case OpCode.incglob:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall
                            (
                            OpCodes.Call, Runtime.LoadGlobalVariableReferenceMethod, null);
                        state.Il.Emit(OpCodes.Dup);
                        state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, PVIncrementMethod, null);
                        state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        break;

                    case OpCode.decloc:
                        sym = state.Symbols[id];
                        if (sym.Kind == SymbolKind.Local)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVDecrementMethod, null);
                            state.EmitStoreLocal(sym.Local);
                        }
                        else if (sym.Kind == SymbolKind.LocalRef)
                        {
                            state.EmitLoadLocal(sym.Local);
                            state.Il.Emit(OpCodes.Dup);
                            state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.Il.EmitCall(OpCodes.Call, PVDecrementMethod, null);
                            state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        }
                        break;
                    case OpCode.decloci:
                        id = state.IndexMap[argc];
                        goto case OpCode.decloc;

                    case OpCode.decglob:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall
                            (
                            OpCodes.Call, Runtime.LoadGlobalVariableReferenceMethod, null);
                        state.Il.Emit(OpCodes.Dup);
                        state.Il.EmitCall(OpCodes.Call, GetValueMethod, null);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, PVDecrementMethod, null);
                        state.Il.EmitCall(OpCodes.Call, SetValueMethod, null);
                        break;

                    case OpCode.neg:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, PVUnaryNegationMethod, null);
                        break;
                    case OpCode.not:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, PVLogicalNotMethod, null);
                        break;

                        #endregion

                        #region BINARY

                        //BINARY OPERATORS

                        #region ADDITION

                        //ADDITION
                    case OpCode.add:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVAdditionMethod, null);
                        break;
                    case OpCode.sub:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVSubtractionMethod, null);
                        break;

                        #endregion

                        #region MULTIPLICATION

                        //MULTIPLICATION
                    case OpCode.mul:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVMultiplyMethod, null);
                        break;
                    case OpCode.div:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVDivisionMethod, null);
                        break;
                    case OpCode.mod:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVModulusMethod, null);
                        break;

                        #endregion

                        #region EXPONENTIAL

                        //EXPONENTIAL
                    case OpCode.pow:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.RaiseToPowerMethod, null);
                        break;

                        #endregion EXPONENTIAL

                        #region COMPARISION

                        //COMPARISION
                    case OpCode.ceq:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVEqualityMethod, null);
                        break;
                    case OpCode.cne:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVInequalityMethod, null);
                        break;
                    case OpCode.clt:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVLessThanMethod, null);
                        break;
                    case OpCode.cle:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVLessThanOrEqualMethod, null);
                        break;
                    case OpCode.cgt:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVGreaterThanMethod, null);
                        break;
                    case OpCode.cge:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVGreaterThanOrEqualMethod, null);
                        break;

                        #endregion

                        #region BITWISE

                        //BITWISE
                    case OpCode.or:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVBitwiseOrMethod, null);
                        break;
                    case OpCode.and:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVBitwiseAndMethod, null);
                        break;
                    case OpCode.xor:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.EmitCall(OpCodes.Call, PVExclusiveOrMethod, null);
                        break;

                        #endregion

                        #endregion //OPERATORS

                        #endregion

                        #region TYPE OPERATIONS

                        #region TYPE CHECK

                        //TYPE CHECK
                    case OpCode.check_const:
                        //Stack:
                        //  Obj
                        state.EmitLoadType(id);
                        //Stack:
                        //  Obj
                        //  Type
                        state.EmitCall(Runtime.CheckTypeConstMethod);
                        break;
                    case OpCode.check_arg:
                        //Stack: 
                        //  Obj
                        //  Type
                        state.Il.EmitCall(OpCodes.Call, Runtime.CheckTypeMethod, null);
                        break;

                    case OpCode.check_null:
                        state.Il.EmitCall(OpCodes.Call, PVIsNullMethod, null);
                        state.Il.Emit(OpCodes.Box, typeof (bool));
                        state.Il.EmitCall(OpCodes.Call, GetBoolPType, null);
                        state.Il.Emit(OpCodes.Newobj, NewPValue);
                        break;

                        #endregion

                        #region TYPE CAST

                    case OpCode.cast_const:
                        //Stack:
                        //  Obj
                        state.EmitLoadType(id);
                        //Stack:
                        //  Obj
                        //  Type
                        state.EmitLoadLocal(state.SctxLocal);
                        state.EmitCall(Runtime.CastConstMethod);

                        break;
                    case OpCode.cast_arg:
                        //Stack
                        //  Obj
                        //  Type
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.CastMethod, null);
                        break;

                        #endregion

                        #endregion

                        #region OBJECT CALLS

                        #region DYNAMIC

                    case OpCode.get:
                        state.fillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.readArgv(argc);
                        state.EmitLdcI4((int) PCall.Get);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, PVDynamicCallMethod, null);
                        if (justEffect)
                            state.Il.Emit(OpCodes.Pop);
                        break;

                    case OpCode.set:
                        state.fillArgv(argc);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.readArgv(argc);
                        state.EmitLdcI4((int) PCall.Set);
                        state.Il.Emit(OpCodes.Ldstr, id);
                        state.Il.EmitCall(OpCodes.Call, PVDynamicCallMethod, null);
                        state.Il.Emit(OpCodes.Pop);
                        break;

                        #endregion

                        #region STATIC

                    case OpCode.sget:
                        //Stack:
                        //  arg
                        //   .
                        //   .
                        //   .
                        state.fillArgv(argc);
                        idx = id.LastIndexOf("::");
                        if (idx < 0)
                            throw new PrexoniteException
                                (
                                "Invalid sget instruction. Does not specify a method.");
                        methodId = id.Substring(idx + 2);
                        typeExpr = id.Substring(0, idx);
                        state.EmitLoadType(typeExpr);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.readArgv(argc);
                        state.EmitLdcI4((int) PCall.Get);
                        state.Il.Emit(OpCodes.Ldstr, methodId);
                        state.EmitVirtualCall(Runtime.StaticCallMethod);
                        if (justEffect)
                            state.Il.Emit(OpCodes.Pop);
                        break;

                    case OpCode.sset:
                        state.fillArgv(argc);
                        idx = id.LastIndexOf("::");
                        if (idx < 0)
                            throw new PrexoniteException
                                (
                                "Invalid sset instruction. Does not specify a method.");
                        methodId = id.Substring(idx + 2);
                        typeExpr = id.Substring(0, idx);
                        state.EmitLoadType(typeExpr);
                        state.EmitLoadLocal(state.SctxLocal);
                        state.readArgv(argc);
                        state.EmitLdcI4((int) PCall.Set);
                        state.Il.Emit(OpCodes.Ldstr, methodId);
                        state.EmitVirtualCall(Runtime.StaticCallMethod);
                        state.Il.Emit(OpCodes.Pop);
                        break;

                        #endregion

                        #endregion

                        #region INDIRECT CALLS

                    case OpCode.indloc:
                        sym = state.Symbols[id];
                        state.fillArgv(argc);
                        sym.EmitLoad(state);
                        state.EmitIndirectCall(argc, justEffect);
                        break;

                    case OpCode.indloci:
                        idx = argc & ushort.MaxValue;
                        argc = (argc & (ushort.MaxValue << 16)) >> 16;
                        id = state.IndexMap[idx];
                        goto case OpCode.indloc;

                    case OpCode.indglob:
                        state.fillArgv(argc);
                        state.EmitLoadGlobalValue(id);
                        state.EmitIndirectCall(argc, justEffect);
                        break;

                    case OpCode.indarg:
                        //Stack
                        //  obj
                        //  args
                        state.fillArgv(argc);
                        state.EmitIndirectCall(argc, justEffect);
                        break;

                    case OpCode.tail:
                        //Stack
                        //  obj
                        //  args
                        state.fillArgv(argc);
                        state.EmitIndirectCall(argc, justEffect);
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadArg(CompilerState.ParamResultIndex);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.Emit(OpCodes.Stind_Ref);
                        _emit_ret(state, instructionIndex);
                        lastWasRet = true;
                        break;

                        #endregion

                        #region ENGINE CALLS

                    case OpCode.func:
                        MethodInfo targetMethod;
                        if (TryGetStaticallyLinkedFunction(state, id, out targetMethod))
                        {
                            state.fillArgv(argc);
                            state.Il.Emit(OpCodes.Ldsfld, state.Pass.FunctionFields[id]);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.readArgv(argc);
                            state.Il.Emit(OpCodes.Ldnull);
                            state.Il.Emit(OpCodes.Ldloca_S, state.TempLocals[0]);
                            state.EmitCall(targetMethod);
                            if (!justEffect)
                                state.EmitLoadTemp(0);
                        }
                        else
                        {
                            state.fillArgv(argc);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.readArgv(argc);
                            state.Il.Emit(OpCodes.Ldstr, id);
                            state.Il.EmitCall(OpCodes.Call, Runtime.CallFunctionMethod, null);
                            if (justEffect)
                                state.Il.Emit(OpCodes.Pop);
                        }
                        break;
                    case OpCode.cmd:
                        PCommand cmd;
                        ICilCompilerAware aware = null;
                        CompilationFlags flags;
                        if (
                            state.TargetEngine.Commands.TryGetValue(id, out cmd) &&
                            (aware = cmd as ICilCompilerAware) != null)
                            flags = aware.CheckQualification(ins);
                        else
                            flags = CompilationFlags.IsCompatible;

                        if (
                            (
                                (flags & CompilationFlags.PreferCustomImplementation) ==
                                CompilationFlags.PreferCustomImplementation ||
                                (flags & CompilationFlags.HasCustomWorkaround) == CompilationFlags.HasCustomWorkaround
                            ) && aware != null)
                        {
                            //Let the command handle the call
                            aware.ImplementInCil(state, ins);
                        }
                        else if ((flags & CompilationFlags.PreferRunStatically) == CompilationFlags.PreferRunStatically)
                        {
                            //Emit a static call to $commandType$.RunStatically
                            state.EmitEarlyBoundCommandCall(cmd.GetType(), ins);
                        }
                        else
                        {
                            //Implement via Runtime.CallCommand (call by name)
                            state.fillArgv(argc);
                            state.EmitLoadLocal(state.SctxLocal);
                            state.readArgv(argc);
                            state.Il.Emit(OpCodes.Ldstr, id);
                            state.Il.EmitCall(OpCodes.Call, Runtime.CallCommandMethod, null);
                            if (justEffect)
                                state.Il.Emit(OpCodes.Pop);
                        }
                        break;

                        #endregion

                        #region FLOW CONTROL

                        //FLOW CONTROL

                        #region JUMPS

                    case OpCode.jump:
                        state.Il.Emit
                            (
                            state.MustUseLeave(instructionIndex, ref argc) ? OpCodes.Leave : OpCodes.Br,
                            state.InstructionLabels[argc]);
                        break;
                    case OpCode.jump_t:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.ExtractBoolMethod, null);
                        if (state.MustUseLeave(instructionIndex, ref argc))
                        {
                            var cont = state.Il.DefineLabel();
                            state.Il.Emit(OpCodes.Brfalse_S, cont);
                            state.Il.Emit(OpCodes.Leave, state.InstructionLabels[argc]);
                            state.Il.MarkLabel(cont);
                        }
                        else
                        {
                            state.Il.Emit(OpCodes.Brtrue, state.InstructionLabels[argc]);
                        }
                        break;
                    case OpCode.jump_f:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.ExtractBoolMethod, null);
                        if (state.MustUseLeave(instructionIndex, ref argc))
                        {
                            var cont = state.Il.DefineLabel();
                            state.Il.Emit(OpCodes.Brtrue_S, cont);
                            state.Il.Emit(OpCodes.Leave, state.InstructionLabels[argc]);
                            state.Il.MarkLabel(cont);
                        }
                        else
                        {
                            state.Il.Emit(OpCodes.Brfalse, state.InstructionLabels[argc]);
                        }
                        break;

                        #endregion

                        #region RETURNS

                    case OpCode.ret_exit:
                        _emit_ret(state, instructionIndex);
                        lastWasRet = true;
                        break;

                    case OpCode.ret_value:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadArg(CompilerState.ParamResultIndex);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.Emit(OpCodes.Stind_Ref);
                        _emit_ret(state, instructionIndex);
                        lastWasRet = true;
                        break;

                    case OpCode.ret_break:
                        throw new PrexoniteException
                            (
                            String.Format
                                (
                                "OpCode {0} not implemented in Cil compiler",
                                Enum.GetName(typeof (OpCode), ins.OpCode)));
                    case OpCode.ret_continue:
                        throw new PrexoniteException
                            (
                            String.Format
                                (
                                "OpCode {0} not implemented in Cil compiler",
                                Enum.GetName(typeof (OpCode), ins.OpCode)));
                    case OpCode.ret_set:
                        state.EmitStoreLocal(primaryTempLocal);
                        state.EmitLoadArg(CompilerState.ParamResultIndex);
                        state.EmitLoadLocal(primaryTempLocal);
                        state.Il.Emit(OpCodes.Stind_Ref);
                        break;

                        #endregion

                        #region THROW

                    case OpCode.@throw:
                        state.EmitLoadLocal(state.SctxLocal);
                        state.Il.EmitCall(OpCodes.Call, Runtime.ThrowExceptionMethod, null);
                        break;

                        #endregion

                        #region LEAVE

                    case OpCode.@try:
                        //Is done via analysis of TryCatchFinally objects associated with the funciton
                        break;

                    case OpCode.leave:
                        //is handled by the CLR

                        #endregion

                        #region EXCEPTION

                    case OpCode.exc:
                        //is not implemented via Emit
                        // The exception is stored when the exception block is entered.
                        break;

                        #endregion

                        #endregion

                        #region STACK MANIPULATION

                        //STACK MANIPULATION
                    case OpCode.pop:
                        for (var i = 0; i < argc; i++)
                            state.Il.Emit(OpCodes.Pop);
                        break;
                    case OpCode.dup:
                        for (var i = 0; i < argc; i++)
                            state.Il.Emit(OpCodes.Dup);
                        break;
                    case OpCode.rot:
                        var values = (int) ins.GenericArgument;
                        var rotations = argc;
                        for (var i = 0; i < values; i++)
                            state.EmitStoreLocal
                                (
                                state.TempLocals[(i + rotations)%values].LocalIndex);
                        for (var i = values - 1; i >= 0; i--)
                            state.EmitLoadLocal(state.TempLocals[i].LocalIndex);
                        break;

                        #endregion
                }

                // * Tail call *
                if (tailCallHint != null)
                {
                    //  tail => call + ret.value
                    //  call has already been emitted, ret.value is missing
                    //  also, the address of the return value is already on the stack (emitted instead of the ldr.* instruction)
                    state.Il.Emit(OpCodes.Stind_Ref);
                    _emit_ret(state, instructionIndex);
                    lastWasRet = true;
                }
            }

            //Often instructions refer to a virtual instruction after the last real one.
            foreach (var block in state.TryBlocks)
            {
                if (block.HasCatch || block.HasFinally)
                    state.Il.EndExceptionBlock();
            }

            if (!lastWasRet)
            {
                state.MarkInstruction(sourceCode.Count);
                state.Il.Emit(OpCodes.Ret);
            }
        }

        public static bool TryGetStaticallyLinkedFunction(CompilerState state, string id, out MethodInfo targetMethod)
        {
            targetMethod = null;
            return (state.Linking & FunctionLinking.Static) == FunctionLinking.Static &&
                   state.Pass.Implementations.TryGetValue(id, out targetMethod);
        }

        private static void _emit_ret(CompilerState state, int instructionIndex)
        {
            var max = state.Source.Code.Count;
            var rmax = max;
            if (instructionIndex == max - 1) //last instruction
                state.MarkInstruction(max); //mark ret ("over-last instruction")

            if (state.MustUseLeave(instructionIndex, ref rmax))
                //Cannot return from protected block.
                //Jump to return instruction (guaranteed to be at address $count)
                state.Il.Emit(OpCodes.Leave, state.InstructionLabels[max]);
            else
                //Use conventional jump
                state.Il.Emit(OpCodes.Ret);
        }

        #region IL helper

        public static readonly MethodInfo CreateNativePValue =
            typeof (CilFunctionContext).GetMethod("CreateNativePValue", new[] {typeof (object)});

        private static readonly MethodInfo _GetBoolPType =
            typeof (PType).GetProperty("Bool").GetGetMethod();

        private static readonly MethodInfo _GetIntPType =
            typeof (PType).GetProperty("Int").GetGetMethod();

        private static readonly MethodInfo _GetPTypeListMethod =
            typeof (PType).GetProperty("List").GetGetMethod();

        private static readonly MethodInfo _getPTypeNull =
            typeof (PType).GetProperty("Null").GetGetMethod();

        private static readonly MethodInfo _GetRealPType =
            typeof (PType).GetProperty("Real").GetGetMethod();

        private static readonly MethodInfo _GetStringPType =
            typeof (PType).GetProperty("String").GetGetMethod();

        internal static readonly MethodInfo GetNullPType =
            typeof (PType).GetProperty("Null").GetGetMethod();

        internal static readonly MethodInfo GetObjectProxy =
            typeof (PType).GetProperty("Object").GetGetMethod();

        private static readonly MethodInfo _getValue =
            typeof (PVariable).GetProperty("Value").GetGetMethod();

        private static readonly ConstructorInfo _NewPValue =
            typeof (PValue).GetConstructor(new[] {typeof (object), typeof (PType)});

        private static readonly ConstructorInfo _NewPValueListCtor =
            typeof (List<PValue>).GetConstructor(new[] {typeof (IEnumerable<PValue>)});

        private static readonly ConstructorInfo _newPVariableCtor =
            typeof (PVariable).GetConstructor(new Type[] {});

        private static readonly MethodInfo _nullCreatePValue =
            typeof (NullPType).GetMethod("CreatePValue", new Type[] {});

        private static readonly MethodInfo _PVAdditionMethod =
            typeof (PValue).GetMethod("Addition", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVBitwiseAndMethod =
            typeof (PValue).GetMethod
                (
                "BitwiseAnd", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVBitwiseOrMethod =
            typeof (PValue).GetMethod("BitwiseOr", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVDecrementMethod =
            typeof (PValue).GetMethod("Decrement", new[] {typeof (StackContext)});

        private static readonly MethodInfo _PVDivisionMethod =
            typeof (PValue).GetMethod("Division", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVDynamicCallMethod =
            typeof (PValue).GetMethod("DynamicCall");

        private static readonly MethodInfo _PVEqualityMethod =
            typeof (PValue).GetMethod("Equality", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVExclusiveOrMethod =
            typeof (PValue).GetMethod
                (
                "ExclusiveOr", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVGreaterThanMethod =
            typeof (PValue).GetMethod
                (
                "GreaterThan", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVGreaterThanOrEqualMethod =
            typeof (PValue).GetMethod
                (
                "GreaterThanOrEqual", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVIncrementMethod =
            typeof (PValue).GetMethod("Increment", new[] {typeof (StackContext)});

        private static readonly MethodInfo _PVIndirectCallMethod =
            typeof (PValue).GetMethod("IndirectCall");

        private static readonly MethodInfo _PVInequalityMethod =
            typeof (PValue).GetMethod
                (
                "Inequality", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVIsNullMethod =
            typeof (PValue).GetProperty("IsNull").GetGetMethod();

        private static readonly MethodInfo _PVLessThanMethod =
            typeof (PValue).GetMethod("LessThan", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVLessThanOrEqualMethod =
            typeof (PValue).GetMethod
                (
                "LessThanOrEqual", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVLogicalNotMethod =
            typeof (PValue).GetMethod("LogicalNot", new[] {typeof (StackContext)});

        private static readonly MethodInfo _PVModulusMethod =
            typeof (PValue).GetMethod("Modulus", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVMultiplyMethod =
            typeof (PValue).GetMethod("Multiply", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVSubtractionMethod =
            typeof (PValue).GetMethod
                (
                "Subtraction", new[] {typeof (StackContext), typeof (PValue)});

        private static readonly MethodInfo _PVUnaryNegationMethod =
            typeof (PValue).GetMethod("UnaryNegation", new[] {typeof (StackContext)});

        private static readonly MethodInfo _setValue =
            typeof (PVariable).GetProperty("Value").GetSetMethod();

        private static MethodInfo GetPTypeListMethod
        {
            get { return _GetPTypeListMethod; }
        }

        private static ConstructorInfo NewPValueListCtor
        {
            get { return _NewPValueListCtor; }
        }

        internal static MethodInfo getPTypeNull
        {
            get { return _getPTypeNull; }
        }

        internal static MethodInfo nullCreatePValue
        {
            get { return _nullCreatePValue; }
        }

        private static ConstructorInfo newPVariableCtor
        {
            get { return _newPVariableCtor; }
        }

        public static MethodInfo GetValueMethod
        {
            get { return _getValue; }
        }

        public static MethodInfo SetValueMethod
        {
            get { return _setValue; }
        }

        internal static MethodInfo GetIntPType
        {
            get { return _GetIntPType; }
        }

        internal static MethodInfo GetRealPType
        {
            get { return _GetRealPType; }
        }

        internal static MethodInfo GetBoolPType
        {
            get { return _GetBoolPType; }
        }

        internal static MethodInfo GetStringPType
        {
            get { return _GetStringPType; }
        }

        private static readonly MethodInfo _GetObjectPTypeSelector = typeof (PType).GetProperty("Object").GetGetMethod();

        public static MethodInfo GetObjectPTypeSelector
        {
            get { return _GetObjectPTypeSelector; }
        }

        private static readonly MethodInfo _CreatePValueAsObject = typeof (PType.PrexoniteObjectTypeProxy).GetMethod
            ("CreatePValue", new[] {typeof (object)});

        public static MethodInfo CreatePValueAsObject
        {
            get { return _CreatePValueAsObject; }
        }

        private static readonly ConstructorInfo _NewPValueKeyValuePair =
            typeof (PValueKeyValuePair).GetConstructor(new[] {typeof (PValue), typeof (PValue)});

        public static ConstructorInfo NewPValueKeyValuePair
        {
            get { return _NewPValueKeyValuePair; }
        }

        //private readonly MethodInfo _CreateNativePValue = typeof(StackContext).GetMethod("CreateNativePValue");
        //private MethodInfo CreateNativePValue
        //{
        //    get { return _CreateNativePValue; }
        //}

        internal static ConstructorInfo NewPValue
        {
            get { return _NewPValue; }
        }

        private static MethodInfo PVIncrementMethod
        {
            get { return _PVIncrementMethod; }
        }

        private static MethodInfo PVDecrementMethod
        {
            get { return _PVDecrementMethod; }
        }

        private static MethodInfo PVUnaryNegationMethod
        {
            get { return _PVUnaryNegationMethod; }
        }

        private static MethodInfo PVLogicalNotMethod
        {
            get { return _PVLogicalNotMethod; }
        }

        private static MethodInfo PVAdditionMethod
        {
            get { return _PVAdditionMethod; }
        }

        private static MethodInfo PVSubtractionMethod
        {
            get { return _PVSubtractionMethod; }
        }

        private static MethodInfo PVMultiplyMethod
        {
            get { return _PVMultiplyMethod; }
        }

        private static MethodInfo PVDivisionMethod
        {
            get { return _PVDivisionMethod; }
        }

        private static MethodInfo PVModulusMethod
        {
            get { return _PVModulusMethod; }
        }

        private static MethodInfo PVBitwiseAndMethod
        {
            get { return _PVBitwiseAndMethod; }
        }

        private static MethodInfo PVBitwiseOrMethod
        {
            get { return _PVBitwiseOrMethod; }
        }

        private static MethodInfo PVExclusiveOrMethod
        {
            get { return _PVExclusiveOrMethod; }
        }

        private static MethodInfo PVEqualityMethod
        {
            get { return _PVEqualityMethod; }
        }

        private static MethodInfo PVInequalityMethod
        {
            get { return _PVInequalityMethod; }
        }

        private static MethodInfo PVGreaterThanMethod
        {
            get { return _PVGreaterThanMethod; }
        }

        private static MethodInfo PVLessThanMethod
        {
            get { return _PVLessThanMethod; }
        }

        private static MethodInfo PVGreaterThanOrEqualMethod
        {
            get { return _PVGreaterThanOrEqualMethod; }
        }

        private static MethodInfo PVLessThanOrEqualMethod
        {
            get { return _PVLessThanOrEqualMethod; }
        }

        private static MethodInfo PVIsNullMethod
        {
            get { return _PVIsNullMethod; }
        }

        private static MethodInfo PVDynamicCallMethod
        {
            get { return _PVDynamicCallMethod; }
        }

        internal static MethodInfo PVIndirectCallMethod
        {
            get { return _PVIndirectCallMethod; }
        }

        private enum VariableInitialization
        {
            None,
            Null,
            ArgV
        }

        private static VariableInitialization GetVariableInitialization(CompilerState state, string id, bool isRef)
        {
            if (Engine.StringsAreEqual(id, PFunction.ArgumentListId) &&
                !state.Source.Parameters.Contains(id))
            {
                return VariableInitialization.ArgV;
            }
            else if (!isRef)
            {
                return VariableInitialization.Null;
            }
            else
            {
                return VariableInitialization.None;
            }
        }

        private static void EmitLoadArgV(CompilerState state)
        {
            state.EmitLoadArg(CompilerState.ParamArgsIndex);
            state.Il.Emit(OpCodes.Newobj, NewPValueListCtor);
            state.Il.EmitCall(OpCodes.Call, GetPTypeListMethod, null);
            state.Il.Emit(OpCodes.Newobj, NewPValue);
        }

        public static void MakePTypeFromExpr(CompilerState state, string expr)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.Emit(OpCodes.Ldstr, expr);
            state.Il.EmitCall(OpCodes.Call, Runtime.ConstructPTypeAsPValueMethod, null);
        }

        #endregion //IL Helper

        #endregion
    }
}