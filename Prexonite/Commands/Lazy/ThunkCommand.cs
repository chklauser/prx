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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Lazy;

public class ThunkCommand : PCommand, ICilCompilerAware
{
    #region Singleton

    ThunkCommand()
    {
    }

    public static ThunkCommand Instance { get; } = new();

    #endregion

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    // ReSharper disable MemberCanBePrivate.Global
    // Part of CIL compiler infrastructure.
    public static PValue RunStatically(StackContext sctx, PValue[] args)

    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));
        if (args == null || args.Length == 0 || args[0] == null)
            throw new PrexoniteException("The thunk command requires an expression.");

        var expr = args[0];
        var parameters = args.Skip(1).Select(_EnforceThunk).ToArray();

        return PType.Object.CreatePValue(Thunk.NewExpression(expr, parameters));
    }

    // ReSharper restore MemberCanBePrivate.Global

    internal static PValue _EnforceThunk(PValue value)
    {
        if (value.Type.Equals(PType.Object[typeof (Thunk)]))
            return value;
        else
            return PType.Object.CreatePValue(Thunk.NewValue(value));
    }


    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException("The command " + GetType().Name +
            " does not support CIL compilation via ICilCompilerAware.");
    }
}

public class Thunk : IIndirectCall, IObject
{
    struct BlackHole
    {
        readonly bool _isActive;
        readonly int _threadId;
        readonly ManualResetEvent _evaluationDone;

        BlackHole(int threadId)
        {
            _isActive = true;
            _threadId = threadId;
            _evaluationDone = new ManualResetEvent(false);
        }

        public static BlackHole Active(int threadId)
        {
            return new(threadId);
        }

        public BlackHole Inactivate()
        {
            _evaluationDone?.Set();
            return _inactive();
        }

        static BlackHole _inactive()
        {
            return new();
        }

        public bool Trap()
        {
            if (_isActive)
            {
                if (_threadId == Thread.CurrentThread.ManagedThreadId)
                {
                    throw new PrexoniteException("Thunk is already being evaluated!");
                }
                else
                {
                    _evaluationDone.WaitOne(Timeout.Infinite, true);
                    return true;
                }
            }
            return false;
        }
    }

    BlackHole _blackHole;

    PValue _expr;
    PValue[] _parameters;
    PValue _value;
    Exception _exception;

    #region Construction

    Thunk(PValue expr, PValue[] parameters)
    {
        _expr = expr ?? throw new ArgumentNullException(nameof(expr));
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    Thunk(PValue value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static Thunk NewValue(PValue value)
    {
        return new(value);
    }

    public static Thunk NewExpression(PValue expr, PValue[] parameters)
    {
        return new(expr, parameters);
    }

    #endregion

    #region Interface

    public PValue Force(StackContext sctx)
    {
        return ((IIndirectCall) this).IndirectCall(sctx, Array.Empty<PValue>());
    }

    public bool IsEvaluated => _value != null;

    public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
        out PValue result)
    {
        result = null;
        switch (id.ToUpperInvariant())
        {
            case "FORCE":
                result = Force(sctx);
                break;
            case "EVALUATED":
            case "ISEVALUATED":
                result = IsEvaluated;
                break;
        }

        return result != null;
    }

    #endregion

    IEnumerable<bool> _cooperativeForce(StackContext sctx, Action<PValue> setReturnValue)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        while (true)
        {
            //Check if evaluation resulted in exception
            if (_exception != null)
                throw _exception;

            //Check if value is available
            if (_value != null)
                break;

            //Prevent infinite loops
            if (_blackHole.Trap())
                continue; //If we have been trapped, check exception again

            //Tag thunk as being evaluated
            _blackHole = BlackHole.Active(Thread.CurrentThread.ManagedThreadId);

            Debug.Indent();
            //We need to save stack space here, so try to invoke via IStackAware
            //  Since most expressions are closures, this has a high success rate
            if (_expr.Value is IStackAware stackAware)
            {
                //Exception handler defined in creation of cooperative context
                var exprCtx = stackAware.CreateStackContext(sctx, _parameters);
                sctx.ParentEngine.Stack.AddLast(exprCtx);
                yield return true;
                _value = exprCtx.ReturnValue;
            }
            else
            {
                try
                {
                    _value = _expr.IndirectCall(sctx, _parameters);
                }
                catch (Exception ex)
                {
                    _blackHole = _blackHole.Inactivate();
                    _value = PType.Null;
                    _exception = ex;
                    throw;
                }
            }
            Debug.Unindent();


            if (_value.Value is Thunk t)
            {
                //Assimilate nested thunk
                _blackHole = t._blackHole;
                _expr = t._expr;
                _parameters = t._parameters;
                _value = t._value;
                _exception = t._exception;
                continue;
            }
            else
            {
                //Release expression
                _expr = null;
                _parameters = null;
                _blackHole = _blackHole.Inactivate();
                break;
            }
        }

        setReturnValue(_value);
        yield break;
    }

    PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
    {
        CooperativeContext coopctx = null;
        coopctx = new CooperativeContext(sctx, f => _cooperativeForce(coopctx, f))
        {
            ExceptionHandler = ex =>
            {
                _blackHole = _blackHole.Inactivate();
                _value = PType.Null;
                _exception = ex;
                return false;
            }
        };

        if (sctx is FunctionContext fctx)
        {
            //Turn CLR call into Prexonite stack call
            fctx._UseVirtualMachineStackInstead();
            sctx.ParentEngine.Stack.AddLast(coopctx);
            return PType.Null;
        }
        else
        {
            //Traditional implementation using the managed stack
            return sctx.ParentEngine.Process(coopctx);
        }
    }
}