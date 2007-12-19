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

#if Verbose
using System.Text;
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Prexonite.Commands;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite
{
    public class FunctionContext : StackContext
    {
        #region Creation

        public FunctionContext(
            Engine parentEngine,
            PFunction implementation,
            PValue[] args,
            PVariable[] sharedVariables)
            : this(parentEngine, implementation, args, sharedVariables, false)
        {
        }

        internal FunctionContext(
            Engine parentEngine,
            PFunction implementation,
            PValue[] args,
            PVariable[] sharedVariables,
            bool suppressInitialization)
        {
            if (parentEngine == null)
                throw new ArgumentNullException("parentEngine");
            if (implementation == null)
                throw new ArgumentNullException("implementation");
            if (sharedVariables == null)
                sharedVariables = new PVariable[] {};
            if (args == null)
                args = new PValue[] {};

            if (!suppressInitialization)
                implementation.ParentApplication.EnsureInitialization(parentEngine, implementation);

            _parentEngine = parentEngine;
            _implementation = implementation;
            _bindArguments(args);
            ReturnMode = ReturnModes.Exit;
            if (_implementation.Meta.ContainsKey(PFunction.SharedNamesKey))
            {
                MetaEntry[] sharedNames = _implementation.Meta[PFunction.SharedNamesKey].List;
                if (sharedNames.Length > sharedVariables.Length)
                    throw new ArgumentException(
                        "The function " + _implementation.Id + " requires " +
                        sharedNames.Length + " variables to be shared.");
                for (int i = 0; i < sharedNames.Length; i++)
                {
                    if (sharedVariables[i] == null)
                        throw new ArgumentNullException(
                            "sharedVariables",
                            "One of the elements passed in sharedVariables is null.");

                    if (_localVariables.ContainsKey(sharedNames[i]))
                        continue; //Arguments are redeclarations.
                    _localVariables.Add(sharedNames[i], sharedVariables[i]);
                }
            }

            //Populate fast access array
            _localVariableArray = new PVariable[_localVariables.Count];
            foreach (KeyValuePair<string, int> mapping in _implementation.LocalVariableMapping)
                _localVariableArray[mapping.Value] = _localVariables[mapping.Key];
        }

        public FunctionContext(Engine parentEngine, PFunction implementation, PValue[] args)
            : this(parentEngine, implementation, args, null)
        {
        }

        public FunctionContext(Engine parentEngine, PFunction implementation)
            : this(parentEngine, implementation, null)
        {
        }

        private void _bindArguments(PValue[] args)
        {
            //Create args variable
            string argVId = PFunction.ArgumentListId;
            //Make sure the variable does not override any parameter or existing variable
            while (_implementation.Parameters.Contains(argVId))
                argVId = "\\" + argVId;

            if (_implementation.Variables.Contains(argVId))
            {
                PVariable argsV = new PVariable();
                argsV.Value = _parentEngine.CreateNativePValue(args);
                _localVariables.Add(argVId, argsV);
            }

            //Create actual parameter variables
            int i = 0;
            foreach (string arg in _implementation.Parameters)
            {
                PVariable pvar = new PVariable();
                if (i < args.Length)
                    pvar.Value = args[i++];
                //Ensure it is a PValue
                if (pvar.Value == null)
                    pvar.Value = PType.Null.CreatePValue();

                _localVariables.Add(arg, pvar);
            }

            //Create local variables
            foreach (string local in _implementation.Variables)
                if (!_localVariables.ContainsKey(local))
                    _localVariables.Add(local, new PVariable());
        }

        #endregion

        #region Interface

        private PValue _returnValue = null;

        public override PValue ReturnValue
        {
            [NoDebug]
            get { return _returnValue ?? NullPType.CreateValue(); }
            //Returns PValue(null) instead of just null.
        }

        private Engine _parentEngine;

        public override Engine ParentEngine
        {
            [NoDebug]
            get { return _parentEngine; }
        }

        private PFunction _implementation;

        public PFunction Implementation
        {
            [NoDebug]
            get { return _implementation; }
        }

        public override Application ParentApplication
        {
            get
            {
                return _implementation.ParentApplication;
            }
        }

        public override SymbolCollection ImportedNamespaces
        {
            get
            {
                return _implementation.ImportedNamespaces;
            }
        }

        public override string ToString()
        {
            return "context of " + _implementation;
        }

        #endregion

        #region Local variables

        private SymbolTable<PVariable> _localVariables = new SymbolTable<PVariable>();
        private PVariable[] _localVariableArray;

        public SymbolTable<PVariable> LocalVariables
        {
            [NoDebug]
            get { return _localVariables; }
        }

        public void ReplaceLocalVariable(string name, PVariable newVariable)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (newVariable == null)
                throw new ArgumentNullException("newVariable");

            if (_implementation.LocalVariableMapping.ContainsKey(name))
                _localVariableArray[_implementation.LocalVariableMapping[name]] = newVariable;
            _localVariables[name] = newVariable;
        }

        #endregion

        #region Virtual Machine

        private int _pointer = 0;

        public int Pointer
        {
            [NoDebug]
            get { return _pointer; }
            [NoDebug]
            set { _pointer = value; }
        }

        private Stack<PValue> _stack = new Stack<PValue>();

        [NoDebug]
        private void push(PValue val)
        {
            _stack.Push(val ?? NullPType.CreateValue());
        }

        [NoDebug]
        private PValue pop()
        {
            return _stack.Pop() ?? NullPType.CreateValue();
        }

        [NoDebug]
        private PValue peek()
        {
            return _stack.Peek() ?? NullPType.CreateValue();
        }

        private void throwInvalidStackException(int argc)
        {
            throw new PrexoniteInvalidStackException(
                "Code expects " + argc +
                " values on the stack but finds " + _stack.Count);
        }

        [NoDebug]
        private void fillArgs(int argc, out PValue[] argv)
        {
            argv = new PValue[argc];
            if (_stack.Count < argc)
                throwInvalidStackException(argc);
            for (int i = argc - 1; i >= 0; i--)
                argv[i] = pop();
        }

        private StackContext _lastContext = null;
        private bool _lastJustEffectFlag = false;
#if Verbose
        internal static string toDebug(PValue val)
        {
            if (val == null)
                return "@null";
            switch(val.Type.ToBuiltIn())
            {
                case PType.BuiltIn.Int:
                case PType.BuiltIn.Real:
                case PType.BuiltIn.Bool:
                    return val.Value.ToString();
                case PType.BuiltIn.String:            
                    return "\"" + StringPType.Escape(val.Value as string) + "\"";
                case PType.BuiltIn.Null:            
                    return NullPType.Literal;
                case PType.BuiltIn.Object:
                    return "{" + val.Value + "}";
                case PType.BuiltIn.List:
                    StringBuilder buffer = new StringBuilder("List(");
                    List<PValue> lst = val.Value as List<PValue>;
                    for(int i = 0; i < lst.Count-1; i++)
                    {
                        buffer.Append(toDebug(lst[i]));
                        buffer.Append(",");
                    }
                    if (lst.Count > 0)
                    {
                        buffer.Append(toDebug(lst[lst.Count-1]));
                    }
                    buffer.Append(")");
                    return buffer.ToString();
                default:
                    return "#" + val +"#";
            }
        }
#endif

        protected override bool PerformNextCylce()
        {
            bool needToReturn = false;
            List<Instruction> codeBase = _implementation.Code;
            int codeLength = codeBase.Count;
            while (!needToReturn)
            {
                if (_pointer >= codeLength)
                {
                    ReturnMode = ReturnModes.Exit;
                    return false;
                }

                //Get return value
                if (_lastContext != null)
                {
                    if (!_lastJustEffectFlag)
                        push(_lastContext.ReturnValue ?? PType.Null.CreatePValue());
                    _lastContext = null;
                }

                Instruction ins = codeBase[_pointer++];

#if Verbose
            Console.Write("/* " + (_pointer-1).ToString().PadLeft(4, '0') + " */ " + ins);
            PValue val;
#endif

                int argc = ins.Arguments;
                bool justEffect = ins.JustEffect;
                PValue[] argv;
                string id = ins.Id;
                PValue left;
                PValue right;
                PType t = ins.GenericArgument as PType;
                PVariable pvar;
                PFunction func;
                //used by static calls
                int idx;
                string methodId;
                MemberInfo member;

                #region OPCODE HANDLING

                switch (ins.OpCode)
                {
                        #region NOP

                        //NOP
                    case OpCode.nop:
                        //Do nothing
                        break;

                        #endregion

                        #region LOAD

                        #region LOAD CONSTANT

                        //LOAD CONSTANT
                    case OpCode.ldc_int:
                        push(argc);
                        break;
                    case OpCode.ldc_real:
                        push((double) ins.GenericArgument);
                        break;
                    case OpCode.ldc_bool:
                        push(argc != 0);
                        break;
                    case OpCode.ldc_string:
                        push(id);
                        break;
                    case OpCode.ldc_null:
                        push(PType.Null.CreatePValue());
                        break;

                        #endregion LOAD CONSTANT

                        #region LOAD REFERENCE

                        //LOAD REFERENCE
                    case OpCode.ldr_loc:
                        if (_localVariables.ContainsKey(id))
                            push(CreateNativePValue(_localVariables[id]));
                        else
                            throw new PrexoniteException(
                                string.Format(
                                    "Cannot load reference to local variable {0} in function {1}.",
                                    id,
                                    _implementation.Id));
                        break;
                    case OpCode.ldr_loci:
                        push(CreateNativePValue(_localVariableArray[argc]));
                        break;
                    case OpCode.ldr_glob:
                        if (ParentApplication.Variables.ContainsKey(id))
                            push(CreateNativePValue(ParentApplication.Variables[id]));
                        else
                            throw new PrexoniteException(
                                string.Format(
                                    "Cannot load reference to global variable {0} in application {1}.",
                                    id,
                                    ParentApplication.Id));
                        break;
                    case OpCode.ldr_func:
                        if (ParentApplication.Functions.Contains(id))
                            push(CreateNativePValue(ParentApplication.Functions[id]));
                        else
                            throw new PrexoniteException(
                                string.Format(
                                    "Cannot load reference to function {0} in application {1}.",
                                    id,
                                    ParentApplication.Id));
                        break;
                    case OpCode.ldr_cmd:
                        if (ParentEngine.Commands.Contains(id))
                            push(CreateNativePValue(ParentEngine.Commands[id]));
                        else
                            throw new PrexoniteException(
                                string.Format(
                                    "Cannot load reference to command {0}.",
                                    id));
                        break;
                    case OpCode.ldr_app:
                        push(CreateNativePValue(ParentApplication));
                        break;
                    case OpCode.ldr_eng:
                        push(CreateNativePValue(ParentEngine));
                        break;
                    case OpCode.ldr_type:
                        if (t == null)
                        {
                            t = ConstructPType(id);
                            ins.GenericArgument = t;
                        }
                        push(CreateNativePValue(t));
                        break;

                        #endregion //LOAD REFERENCE

                        #endregion //LOAD

                        #region VARIABLES

                        #region LOCAL

                        //LOAD LOCAL VARIABLE
                    case OpCode.ldloc:
                        pvar = _localVariables[id];
                        if (pvar == null)
                            throw new PrexoniteException(
                                "The local variable " + id + " in function " + _implementation.Id +
                                " does not exist.");
#if Verbose
                    val = pvar.Value;
                    Console.Write("=" + toDebug(val));
                    push(val);
#else
                        push(pvar.Value);
#endif
                        break;
                    case OpCode.stloc:
                        pvar = _localVariables[id];
                        if (pvar == null)
                            throw new PrexoniteException(
                                "The local variable " + id + " in function " + _implementation.Id +
                                " does not exist.");

                        pvar.Value = pop();
                        break;

                    case OpCode.ldloci:
                        push(_localVariableArray[argc].Value);
                        break;

                    case OpCode.stloci:
                        _localVariableArray[argc].Value = pop();
                        break;

                        #endregion

                        #region GLOBAL

                        //LOAD GLOBAL VARIABLE
                    case OpCode.ldglob:
                        Application app = ParentApplication;
                        pvar = app.Variables[id];
                        if (pvar == null)
                            throw new PrexoniteException(
                                "The global variable " + id + " does not exist.");
                        app.EnsureInitialization(ParentEngine, pvar);
#if Verbose
                    val = pvar.Value;
                    Console.Write("=" + toDebug(val));
                    push(val);
#else
                        push(pvar.Value);
#endif
                        break;
                    case OpCode.stglob:
                        pvar = ParentApplication.Variables[id];
                        if (pvar == null)
                            throw new PrexoniteException(
                                "The global variable " + id + " does not exist.");
                        pvar.Value = pop();
                        break;

                        #endregion

                        #endregion

                        #region CONSTRUCTION

                        //CONSTRUCTION
                    case OpCode.newobj:
                        if (t == null)
                        {
                            t = ConstructPType(id);
                            ins.GenericArgument = t;
                        }
                        fillArgs(argc, out argv);
                        push(t.Construct(this, argv));
                        break;
                    case OpCode.newtype:
                        //assemble type expression
                        fillArgs(argc, out argv);
                        push(CreateNativePValue(ParentEngine.CreatePType(this, id, argv)));
                        break;

                    case OpCode.newclo:
                        string[] vars = ins.GenericArgument as string[];
                        func = ParentApplication.Functions[id];
                        if (vars == null)
                        {
                            MetaEntry[] entries;
                            if (func.Meta.ContainsKey(PFunction.SharedNamesKey))
                                entries = func.Meta[PFunction.SharedNamesKey].List;
                            else
                                entries = new MetaEntry[] {};
                            vars = new string[entries.Length];
                            for (int i = 0; i < entries.Length; i++)
                                vars[i] = entries[i].Text;
                            ins.GenericArgument = vars;
                        }
                        PVariable[] pvars = new PVariable[vars.Length];
                        for (int i = 0; i < pvars.Length; i++)
                            pvars[i] = _localVariables[vars[i]];
                        push(CreateNativePValue(new Closure(func, pvars)));
                        break;

                    case OpCode.newcor:
                        fillArgs(argc, out argv);

                        PValue routine = pop();
                        object routineobj = routine.Value;
                        IStackAware routinesa = routineobj as IStackAware;
                        StackContext corctx;
                        if (routineobj == null)
                        {
                            push(PType.Null.CreatePValue());
                        }
                        else
                        {
                            if (routinesa != null)
                                corctx = routinesa.CreateStackContext(ParentEngine, argv);
                            else
                                corctx = (StackContext)
                                         routine.DynamicCall(
                                             this,
                                             new PValue[]
                                                 {
                                                     PType.Object.CreatePValue(ParentEngine),
                                                     PType.Object.CreatePValue(argv)
                                                 },
                                             PCall.Get,
                                             "CreateStackContext").Value;

                            push(
                                PType.Object[typeof(Coroutine)].CreatePValue(new Coroutine(corctx)));
                        }
                        break;

                        #endregion

                        #region OPERATORS

                        #region UNARY

                        //UNARY OPERATORS
                    case OpCode.incloc:
                        pvar = _localVariables[id];
doIncrement:            pvar.Value = pvar.Value.Increment(this);
#if Verbose
                    Console.Write("=" + toDebug(pvar.Value));
#endif
                        break;

                    case OpCode.incloci:
                        pvar = _localVariableArray[argc];
                        goto doIncrement;

                    case OpCode.incglob:
                        pvar = ParentApplication.Variables[id];
                        pvar.Value = pvar.Value.Increment(this);
#if Verbose
                    Console.Write("=" + toDebug(pvar.Value));
#endif
                        break;
                    case OpCode.decloc:
                        pvar = _localVariables[id];
doDecrement:            pvar.Value = pvar.Value.Decrement(this);
#if Verbose
                    Console.Write("=" + toDebug(pvar.Value));
#endif
                        break;
                    case OpCode.decloci:
                        pvar = _localVariableArray[argc];
                        goto doDecrement;

                    case OpCode.decglob:
                        pvar = ParentApplication.Variables[id];
                        pvar.Value = pvar.Value.Decrement(this);
#if Verbose
                    Console.Write("=" + toDebug(pvar.Value));
#endif
                        break;
                    case OpCode.neg:
                        push(pop().UnaryNegation(this));
                        break;
                    case OpCode.not:
                        push(pop().LogicalNot(this));
                        break;

                        #endregion

                        #region BINARY

                        //BINARY OPERATORS

                        #region ADDITION

                        //ADDITION
                    case OpCode.add:
                        right = pop();
                        push(pop().Addition(this, right));
                        break;
                    case OpCode.sub:
                        right = pop();
                        push(pop().Subtraction(this, right));
                        break;

                        #endregion

                        #region MULTIPLICATION

                        //MULTIPLICATION
                    case OpCode.mul:
                        right = pop();
                        push(pop().Multiply(this, right));
                        break;
                    case OpCode.div:
                        right = pop();
                        push(pop().Division(this, right));
                        break;
                    case OpCode.mod:
                        right = pop();
                        push(pop().Modulus(this, right));
                        break;

                        #endregion

                        #region EXPONENTIAL

                        //EXPONENTIAL
                    case OpCode.pow:
                        right = pop();
                        left = pop();
                        PValue rleft,
                               rright;
                        if (
                            !(left.TryConvertTo(this, PType.Real, out rleft) &&
                              right.TryConvertTo(this, PType.Real, out rright)))
                            throw new PrexoniteException(
                                "The arguments supplied to the power operator are invalid (cannot be converted to Real).");
                        push(
                            Math.Pow(Convert.ToDouble(rleft.Value), Convert.ToDouble(rright.Value)));
                        break;

                        #endregion EXPONENTIAL

                        #region COMPARISION

                        //COMPARISION
                    case OpCode.ceq:
                        right = pop();
                        push(pop().Equality(this, right));
                        break;
                    case OpCode.cne:
                        right = pop();
                        push(pop().Inequality(this, right));
                        break;
                    case OpCode.clt:
                        right = pop();
                        push(pop().LessThan(this, right));
                        break;
                    case OpCode.cle:
                        right = pop();
                        push(pop().LessThanOrEqual(this, right));
                        break;
                    case OpCode.cgt:
                        right = pop();
                        push(pop().GreaterThan(this, right));
                        break;
                    case OpCode.cge:
                        right = pop();
                        push(pop().GreaterThanOrEqual(this, right));
                        break;

                        #endregion

                        #region BITWISE

                        //BITWISE
                    case OpCode.or:
                        right = pop();
                        push(pop().BitwiseOr(this, right));
                        break;
                    case OpCode.and:
                        right = pop();
                        push(pop().BitwiseAnd(this, right));
                        break;
                    case OpCode.xor:
                        right = pop();
                        push(pop().ExclusiveOr(this, right));
                        break;

                        #endregion

                        #endregion //OPERATORS

                        #endregion

                        #region TYPE OPERATIONS

                        #region TYPE CHECK

                        //TYPE CHECK
                    case OpCode.check_const:
                        if (t == null)
                        {
                            t = ConstructPType(id);
                            ins.GenericArgument = t;
                        }
                        goto check_type; //common code
                    case OpCode.check_arg:
                        t = (PType) pop().Value;
                        check_type:
                        ;
                        push(pop().Type.Equals(t));
                        break;
                    case OpCode.check_null:
                        push(pop().IsNull);
                        break;

                        #endregion

                        #region TYPE CAST

                    case OpCode.cast_const:
                        if (t == null)
                        {
                            t = ConstructPType(id);
                            ins.GenericArgument = t;
                        }
                        goto cast_type; //common code
                    case OpCode.cast_arg:
                        t = (PType) pop().Value;
                        cast_type:
                        ;
                        push(pop().ConvertTo(this, t, true));
                        break;

                        #endregion

                        #endregion

                        #region OBJECT CALLS

                        #region DYNAMIC

                    case OpCode.get:
                        fillArgs(argc, out argv);
                        left = pop();
                        right = left.DynamicCall(this, argv, PCall.Get, id);
                        if (!justEffect)
                            push(right);
                        needToReturn = true;
                        break;
                    case OpCode.set:
                        fillArgs(argc, out argv);
                        left = pop();
                        left.DynamicCall(this, argv, PCall.Set, id);
                        needToReturn = true;
                        break;

                        #endregion

                        #region STATIC

                    case OpCode.sget:
                        fillArgs(argc, out argv);
                        idx = id.LastIndexOf("::");
                        if (idx < 0)
                            throw new PrexoniteException(
                                "Invalid sget instruction. Does not specify a method.");
                        needToReturn = true;
                        methodId = id.Substring(idx + 2);
                        member = ins.GenericArgument as MemberInfo;
                        if (member != null)
                            goto callByMemberGet;
                        else if (t != null)
                            goto callByTypeGet;
                        else
                        {
                            //Full resolve
                            string typeExpr = id.Substring(0, idx);
                            t = ConstructPType(typeExpr);
                            ins.GenericArgument = t;
                            if (t is ObjectPType)
                            {
                                //Try to get a member info
                                ObjectPType objT = (ObjectPType) t;
                                right = objT.StaticCall(this, argv, PCall.Get, methodId, out member);
                                if (!justEffect)
                                    push(right);
                                if (member != null)
                                    ins.GenericArgument = member;
                                break;
                            }
                            else
                                goto callByTypeGet;
                        }

                        callByTypeGet:
                        ;
                        right = t.StaticCall(this, argv, PCall.Get, methodId);
                        if (!justEffect)
                            push(right);
                        break;
                        callByMemberGet:
                        ;
                        right = ObjectPType._execute(this, member, argv, PCall.Get, methodId, null);
                        if (!justEffect)
                            push(right);
                        break;

                    case OpCode.sset:
                        fillArgs(argc, out argv);
                        idx = id.LastIndexOf("::");
                        if (idx < 0)
                            throw new PrexoniteException(
                                "Invalid sget instruction. Does not specify a method.");
                        needToReturn = true;
                        methodId = id.Substring(idx + 2);
                        member = ins.GenericArgument as MemberInfo;
                        if (member != null)
                            goto callByMemberSet;
                        else if (t != null)
                            goto callByTypeSet;
                        else
                        {
                            //Full resolve
                            string typeExpr = id.Substring(0, idx);
                            t = ConstructPType(typeExpr);
                            ins.GenericArgument = t;
                            if (t is ObjectPType)
                            {
                                //Try to get a member info
                                ObjectPType objT = (ObjectPType) t;
                                objT.StaticCall(this, argv, PCall.Set, methodId, out member);
                                if (member != null)
                                    ins.GenericArgument = member;
                                break;
                            }
                            else
                                goto callByTypeSet;
                        }

                        callByTypeSet:
                        ;
                        t.StaticCall(this, argv, PCall.Set, methodId);
                        break;
                        callByMemberSet:
                        ;
                        ObjectPType._execute(this, member, argv, PCall.Set, methodId, null);
                        break;

                        #endregion

                        #endregion

                        #region INDIRECT CALLS

                    case OpCode.indloc:
                        fillArgs(argc, out argv);
                        needToReturn = true;
                        left = _localVariables[id].Value;

#if Verbose
                    Console.Write("  " + toDebug(left) + "(");
                    foreach (PValue arg in argv)
                        Console.Write(toDebug(arg) + ", ");
                    Console.WriteLine(")");
#endif

                        //Perform indirect call
doIndloc:               if (justEffect)
                            left.IndirectCall(this, argv);
                        else
                            push(left.IndirectCall(this, argv));
                        break;
                    case OpCode.indloci:
                        idx = argc & ushort.MaxValue;
                        argc = (argc & (ushort.MaxValue << 16)) >> 16;
                        fillArgs(argc, out argv);
                        left = _localVariableArray[idx].Value;
                        goto doIndloc;
                    case OpCode.indglob:
                        fillArgs(argc, out argv);
                        needToReturn = true;
                        app = ParentApplication;
                        pvar = app.Variables[id];
                        app.EnsureInitialization(ParentEngine, pvar);
                        left = pvar.Value;

#if Verbose
                    Console.Write("  " + toDebug(left) + "(");
                    foreach (PValue arg in argv)
                        Console.Write(toDebug(arg) + ", ");
                    Console.WriteLine(")");
#endif

                        //Perform indirect call
                        if (justEffect)
                            left.IndirectCall(this, argv);
                        else
                            push(left.IndirectCall(this, argv));
                        break;

                    case OpCode.indarg:
                        fillArgs(argc, out argv);
                        needToReturn = true;
                        left = pop();
                        if (justEffect)
                            left.IndirectCall(this, argv);
                        else
                            push(left.IndirectCall(this, argv));
                        break;

                        #endregion

                        #region ENGINE CALLS

                    case OpCode.func:
                        fillArgs(argc, out argv);
                        if (ParentEngine.CacheFunctions)
                        {
                            func = (ins.GenericArgument as PFunction) ??
                                   ParentApplication.Functions[id];
                            ins.GenericArgument = func;
                        }
                        else
                        {
                            func = ParentApplication.Functions[id];
                        }
                        if (func == null)
                            throw PrexoniteRuntimeException.CreateRuntimeException(
                                this, "No function with the physical name " + id + " exists.");
                        _lastContext =
                            new FunctionContext(
                                ParentEngine, func, argv);

                        _lastJustEffectFlag = justEffect;
                        ParentEngine.Stack.AddLast(_lastContext);
#if Verbose
                    Console.Write("\n#PSH: " + id + "(");
                    foreach (PValue arg in argv)
                        Console.Write(toDebug(arg) + ", ");
                    Console.WriteLine(")");
#endif
                        return true;
                        //Force the engine to keep this context on the stack for another cycle
                    case OpCode.cmd:
                        fillArgs(argc, out argv);
                        needToReturn = true;
                        PCommand cmd;
                        if (ParentEngine.CacheCommands)
                        {
                            cmd = (ins.GenericArgument as PCommand) ?? ParentEngine.Commands[id];
                            ins.GenericArgument = cmd;
                        }
                        else
                        {
                            cmd = ParentEngine.Commands[id];
                        }
                        if (cmd == null)
                            throw new PrexoniteException("Cannot find command " + id + "!");
                        if (justEffect)
                            cmd.Run(this, argv);
                        else
                        {
#if Verbose
                            val = cmd.Run(this, argv);
                            Console.Write(" =" + toDebug(val));
                            push(val);
#else
                            push(cmd.Run(this, argv));
#endif
                        }
                        break;

                        #endregion

                        #region FLOW CONTROL

                        //FLOW CONTROL

                        #region JUMPS

                    case OpCode.jump:
                        _pointer = argc;
                        break;
                    case OpCode.jump_t:
                        left = pop();
                        if (!(left.Value is bool))
                            left = left.ConvertTo(this, PType.Bool);
                        if ((bool) left.Value)
                        {
                            _pointer = argc;
#if Verbose
                        Console.Write(" -> jump");
#endif
                        }
                        break;
                    case OpCode.jump_f:
                        left = pop();
                        if (!(left.Value is bool))
                            left = left.ConvertTo(this, PType.Bool);
                        if (!(bool) left.Value)
                        {
#if Verbose
                        Console.Write(" -> jump");
#endif
                            _pointer = argc;
                        }
                        break;

                        #endregion

                        #region RETURNS

                    case OpCode.ret_exit:
                        ReturnMode = ReturnModes.Exit;
#if Verbose
                    Console.WriteLine();
#endif
                        return false;
                    case OpCode.ret_value:
                        _returnValue = pop();
                        ReturnMode = ReturnModes.Exit;
#if Verbose
                    Console.WriteLine("=" + toDebug(_returnValue));
#endif
                        return false;
                    case OpCode.ret_break:
                        ReturnMode = ReturnModes.Break;
#if Verbose
                    Console.WriteLine();
#endif
                        return false;
                    case OpCode.ret_continue:
                        ReturnMode = ReturnModes.Continue;
#if Verbose
                    Console.WriteLine();
#endif
                        return false;
                    case OpCode.ret_set:
                        _returnValue = pop();
#if Verbose
                    Console.WriteLine("=" + toDebug(_returnValue));
#endif
                        break;

                        #endregion

                        #region THROW

                    case OpCode.@throw:
                        left = pop();
                        t = left.Type;
                        PrexoniteRuntimeException prexc;
                        if (t is StringPType)
                            prexc =
                                PrexoniteRuntimeException.CreateRuntimeException(
                                    this, (string) left.Value);
                        else if (t is ObjectPType && left.Value is Exception)
                            prexc =
                                PrexoniteRuntimeException.CreateRuntimeException(
                                    this,
                                    ((Exception) left.Value).Message,
                                    (Exception) left.Value);
                        else
                            prexc =
                                PrexoniteRuntimeException.CreateRuntimeException(
                                    this, left.CallToString(this));
#if Verbose
                    Console.WriteLine();
#endif
                        throw prexc;

                        #endregion

                        #region LEAVE

                    case OpCode.@try:
                        _isHandlingException.Push(false);
                        break;

                    case OpCode.leave:
                        if (!_isHandlingException.Pop())
                        {
                            _pointer = argc;
#if Verbose
                        Console.Write(" => Skip catch block.");
#endif
                        }
#if Verbose
                        else
                        {
                            Console.Write(" => execute catch({0}:{1})", 
                                _currentException.GetType().Name, _currentException.Message);
                        }
#endif
                        break;

                        #endregion

                        #region EXCEPTION

                    case OpCode.exc:
                        push(CreateNativePValue(_currentException));
                        break;

                        #endregion

                        #region TAIL

                    case OpCode.tail:
                        break;

                        #endregion

                        #endregion

                        #region STACK MANIPULATION

                        //STACK MANIPULATION
                    case OpCode.pop:
                        if (_stack.Count < argc)
                            throwInvalidStackException(argc);
                        for (int i = 0; i < argc; i++)
                            pop(); //pop to nirvana
                        break;
                    case OpCode.dup:
                        left = peek();
                        for (int i = 0; i < argc; i++)
                            push(left);
                        break;
                    case OpCode.rot:
                        int values = (int) ins.GenericArgument;
                        int rotations = argc;
                        PValue[] target = new PValue[values];
                        for (int i = 0; i < values; i++)
                            target[(i + rotations)%values] = pop();
                        for (int i = 0; i < values; i++)
                            push(target[i]);
                        break;

                        #endregion
                }

                #endregion

                //Next instruction
#if Verbose
            Console.Write("\n");
#endif
                if (_pointer >= codeLength)
                    return false;
            }

            return _pointer < codeLength;
        }

        #region Exception Handling

        private Exception _currentException = null;

        private Stack<bool> _isHandlingException = new Stack<bool>();

        /// <summary>
        /// Indicates whether the function context is currently handling an exception or not.
        /// </summary>
        /// <value>True, if the function is currently handling an exception.<br />
        /// False, if the function runs normally.</value>
        public bool IsHandlingException
        {
            get { return _isHandlingException.Peek(); }
        }

        public override bool TryHandleException(Exception exc)
        {
            //Pointer has already been incremented.
            int address = _pointer - 1;

            TryCatchFinallyBlock block =
                TryCatchFinallyBlock.Closest(address, _implementation.TryCatchFinallyBlocks);

            if (block == null) //No try-catch-finally block handles exceptions at the given address.
                return false;

            _currentException = exc;

            if (block.HasFinally)
            {
#if Verbose
                Console.WriteLine("Exception handled by finally-catch. " + block);
#endif
                _isHandlingException.Pop();
                _isHandlingException.Push(true);
                _pointer = block.BeginFinally;
            }
            else if (block.HasCatch)
            {
#if Verbose
                Console.WriteLine("Exception handled by catch." + block);
#endif
                _pointer = block.BeginCatch;
            }

            return true;
        }

        #endregion

        #endregion Virtual Machine
    }

    [Serializable]
    [NoDebug]
    public class PrexoniteInvalidStackException : PrexoniteException
    {
        public PrexoniteInvalidStackException()
        {
        }

        public PrexoniteInvalidStackException(string message)
            : base(message)
        {
        }

        public PrexoniteInvalidStackException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PrexoniteInvalidStackException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}