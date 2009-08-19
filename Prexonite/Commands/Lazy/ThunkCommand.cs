using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;
using Action=Prexonite.Compiler.Cil.Action;

namespace Prexonite.Commands.Lazy
{
    public class ThunkCommand : PCommand, ICilCompilerAware
    {

        #region Singleton

        private ThunkCommand()
        {
        }

        private static readonly ThunkCommand _instance = new ThunkCommand();

        public static ThunkCommand Instance
        {
            get { return _instance; }
        }

        #endregion 

        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

// ReSharper disable MemberCanBePrivate.Global
// Part of CIL compiler infrastructure.
        public static PValue RunStatically(StackContext sctx, PValue[] args)

        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null)
                throw new PrexoniteException("The thunk command requires an expression.");

            var expr = args[0];
            var parameters = args.Skip(1).Select<PValue, PValue>(_enforceThunk).ToArray();
             
            return PType.Object.CreatePValue(Thunk.NewExpression(expr, parameters));
        }
// ReSharper restore MemberCanBePrivate.Global

        private static PValue _enforceThunk(PValue value)
        {
            if (value.Type.Equals(PType.Object[typeof (Thunk)]))
                return value;
            else
                return PType.Object.CreatePValue(Thunk.NewValue(value));
        }


        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }
    }

    public class Thunk : IIndirectCall, IObject
    {
        private bool _isBlackHole;

        private PValue _expr;
        private PValue[] _parameters;
        private PValue _value;
        private Exception _exception;

        #region Construction

        private Thunk(PValue expr, PValue[] parameters)
        {
            if (expr == null)
                throw new ArgumentNullException("expr");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            _expr = expr;
            _parameters = parameters;
        }

        private Thunk(PValue value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            _value = value;
        }

        public static Thunk NewValue(PValue value)
        {
            return new Thunk(value);
        }

        public static Thunk NewExpression(PValue expr, PValue[] parameters)
        {
            return new Thunk(expr, parameters);
        }

        #endregion

        #region Interface

        public PValue Force(StackContext sctx, PValue[] args)
        {
            return ((IIndirectCall) this).IndirectCall(sctx, args);
        }

        public bool IsEvaluated
        {
            get { return _value != null; }
        }

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;
            switch (id.ToUpperInvariant())
            {
                case "FORCE":
                    result = Force(sctx, args);
                    break;
                case "EVALUATED":
                case "ISEVALUATED":
                    result = IsEvaluated;
                    break;
            }

            return result != null;
        }


        #endregion

        private IEnumerable<bool> _cooperativeForce(StackContext sctx, PValue[] args, Action<PValue> setReturnValue)
        {
            while (true)
            {
                //Check if evaluation resulted in exception
                if (_exception != null)
                    throw _exception;

                //Check if value is available
                if (_value != null)
                    break;

                //Prevent infinite loops
                if (_isBlackHole)
                    throw new PrexoniteException("Thunk is already being evaluated!");

                Debug.WriteLine("Force " + _expr, "thunk");

                //Tag thunk as being evaluated
                _isBlackHole = true;

                Debug.Indent();
                //We need to save stack space here, so try to invoke via IStackAware
                //  Since most expressions are closures, this has a high success rate
                var stackAware = _expr.Value as IStackAware;
                if (stackAware != null)
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
                        _isBlackHole = false;
                        _value = PType.Null;
                        _exception = ex;
                        throw;
                    }
                    
                }
                Debug.Unindent();


                var t = _value.Value as Thunk;
                if (t != null)
                {
                    //Assimilate nested thunk
                    _isBlackHole = t._isBlackHole;
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
                    _isBlackHole = false;
                    break;
                }

            }

            setReturnValue(_value);
            yield break;
        }

        PValue IIndirectCall.IndirectCall(StackContext sctx, PValue[] args)
        {
            var coopctx =
                new CooperativeContext(sctx, f => _cooperativeForce(sctx, args, f))
                    {
                        ExceptionHandler = ex =>
                                               {
                                                   _isBlackHole = false;
                                                   _value = PType.Null;
                                                   _exception = ex;
                                                   return false;
                                               }
                    };

            var fctx = sctx as FunctionContext;
            if (fctx != null)
            {
                //Turn CLR call into Prexonite stack call
                fctx.UseVirtualStackInstead();
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
}
