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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// Makes an <see cref="IAstFactory"/> easier to use from Prexonite Script.
    /// </summary>
    public class AstFactoryBridge : IObject, IIndirectCall
    {
        private readonly IAstFactory _base;

        public AstFactoryBridge(IAstFactory @base)
        {
            _base = @base;
        }

        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            string detailedError;
            return TryDynamicCall(sctx, args, call, id, out result, out detailedError);
        }

        private static bool _require(PValue[] args, ref int index, out PValue rawValue)
        {
            if (index < args.Length)
            {
                rawValue = args[index];
                index++;
                return true;
            }
            else
            {
                rawValue = null;
                index++;
                return false;
            }
        }

        private static bool _require(StackContext sctx, PValue[] args, ref int index, out IEnumerable<AstExpr> argSeq)
        {
            PValue raw;
            if(_require(args, ref index, out raw))
            {
                argSeq = Commands.List.Map._ToEnumerable(sctx, raw)
                    .Select(x => x.ConvertTo<AstExpr>(sctx, true));
                return true;
            }
            else
            {
                argSeq = null;
                return false;
            }
        }

        private static bool _require<T>(StackContext sctx, PValue[] args, ref int index, out T value)
        {
            if (index < args.Length && args[index].TryConvertTo(sctx, false, out value))
            {
                index++;
                return true;
            }
            else
            {
                index++;
                value = default(T);
                return false;
            }
        }

        private static bool _takeOptional<T>(StackContext sctx, PValue[]  args, ref int index, out T value, T defaultValue = default(T))
        {
            if(index < args.Length && args[index].TryConvertTo(sctx, false,out value))
            {
                index++;
                return true;
            }
            else
            {
                // don't increment index here, the argument was not present
                value = defaultValue;
                return false;
            }
        }

        private static bool _takeOptional(PValue[] args, ref int index, out PValue rawValue, PValue defaultValue = null)
        {
            if(index < args.Length)
            {
                rawValue = args[index];
                index++;
                return true;
            }
            else
            {
                rawValue = defaultValue ?? PType.Null;
                return false;
            }
        }

        private static bool _takeOptionalList(StackContext sctx, PValue[] args, ref int  index, out IEnumerable<AstExpr> expressions)
        {
            PValue raw;
            if(_takeOptional(args, ref index, out raw))
            {
                expressions = Commands.List.Map._ToEnumerable(sctx, raw)
                    .Select(x => x.ConvertTo<AstExpr>(sctx, true));
                return true;
            }
            else
            {
                expressions = Enumerable.Empty<AstExpr>();
                return false;
            }
        }

        [PublicAPI]
        protected bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result, out string detailedError)
        {
            ISourcePosition position;
            if (args.Length == 0 || !args[0].TryConvertTo(sctx, false, out position))
            {
                detailedError = "Not enough arguments for creating an AST node. Missing source position.";
                result = null;
                return false;
            }

            detailedError = null;
            result = null;
            AstNode node = null;
            var i = 1; // the argument index
            switch (id.ToLowerInvariant())
            {
                #region Type expressions
                case "constanttypeexpression":
                    throw new PrexoniteException("Cannot construct \"ConstantTypeExpression\", did you mean \"ConstantType\"?");
                case "constanttype":
                    {
                        string typeExpression;
                        if (!_require(sctx, args, ref i, out typeExpression))
                        {
                            detailedError =
                                "ConstantTypeExpression(position, typeExpression~String), typeExpression is missing.";
                            return false;
                        }
                        node = _base.ConstantType(position, typeExpression);
                        break;
                    }
                case "dynamictypeexpression":
                    throw new PrexoniteException("Cannot construct \"DynamicTypeExpression\", did you mean \"DynamicType\"?");
                case "dynamictype":
                    {
                        string typeId;
                        IEnumerable<AstExpr> targs;
                        if (!_require(sctx, args, ref i, out typeId))
                        {
                            detailedError =
                                "DynamicTypeExpression(position, typeId~String, arguments~List), typeId is missing";
                            return false;
                        }
                        if (!_require(sctx, args, ref i, out targs))
                        {
                            detailedError =
                                 "DynamicTypeExpression(position, typeId~String, arguments~List), arguments is missing";
                            return false;
                        }
                        node = _base.DynamicType(position, typeId, targs);
                        break;
                    }
                #endregion

                #region Value expressions

                case "binaryoperation":
                    {
                        AstExpr left, right;
                        BinaryOperator op;
                        const string sig = "BinaryOperation(position, left~AstExpr, op~BinaryOperator, right~AstExpr)";
                        if (!_require(sctx, args, ref i, out left))
                        {
                            detailedError = sig + ", missing left expr.";
                            return false;
                        }
                        if (!_require(sctx, args, ref i, out op))
                        {
                            detailedError = sig + ", missing operator.";
                            return false;
                        }
                        if (!_require(sctx, args, ref i, out right))
                        {
                            detailedError = sig + ", missing right expr.";
                            return false;
                        }

                        node = _base.BinaryOperation(position, left, op, right);
                        break;
                    }
                case "unaryoperation":
                    {
                        AstExpr  operand;
                        UnaryOperator op;
                        const string sig = "UnaryOperation(position, op~UnaryOperator, operand~AstExpr)";
                        if (!_require(sctx, args, ref i, out op))
                        {
                            detailedError = sig + ", missing operator.";
                            return false;
                        }
                        if (!_require(sctx, args, ref i, out operand))
                        {
                            detailedError = sig + ", missing operand expr.";
                            return false;
                        }
                        node = _base.UnaryOperation(position, op, operand);
                        break;
                    }
                case "coalescence":
                    {
                        IEnumerable<AstExpr> targs;
                        if (!_require(sctx, args, ref i, out targs))
                        {
                            detailedError = "Coalescence(position, args~List), args is missing.";
                            return false;
                        }
                        node = _base.Coalescence(position, targs);
                        break;
                    }

                case "conditionalexpression":
                    {
                        AstExpr condition, thenExpr, elseExpr;
                        bool isNegative;
                        const string sig =
                            "ConditionalExpression(position, condition~AstExpr, thenExpr~AstExpr, elseExpr~AstExpr, isNegative = false)";
                        if(!_require(sctx, args, ref i,out condition))
                        {
                            detailedError = sig + ", condition is missing.";
                            return false;
                        }
                        if(!_require(sctx, args, ref i, out thenExpr))
                        {
                            detailedError = sig + ", thenExpr is missing.";
                            return false;
                        }
                        if(!_require(sctx,args, ref i, out elseExpr))
                        {
                            detailedError = sig + ", elseExpr is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out isNegative);

                        node = _base.ConditionalExpression(position, condition, thenExpr, elseExpr, isNegative);
                        break;
                    }
                case "constant":
                    {
                        PValue raw;
                        if(!_require(args, ref i, out raw))
                        {
                            detailedError = "Constant(position, const) const is missing.";
                            return false;
                        }
                        node = _base.Constant(position, raw.Value);
                        break;
                    }
                case "createclosure":
                    {
                        EntityRef.Function funcRef;
                        if(!_require(sctx,args,ref i, out funcRef))
                        {
                            detailedError = "CreateClosure(position, funcRef~EntityRef.Function), funcRef is missing.";
                            return false;
                        }

                        node = _base.CreateClosure(position, funcRef);
                        break;
                    }
                case "createcoroutine":
                    {
                        AstExpr expr;
                        if(!_require(sctx, args, ref i, out expr))
                        {
                            detailedError =
                                "CreateCoroutine(position, generatorExpr~AstExpr), generatorExpr is missing.";
                            return false;
                        }

                        node = _base.CreateCoroutine(position, expr);
                        break;
                    }

                case "keyvaluepair":
                    {
                        AstExpr key, value;
                        const string sig = "KeyValuePair(position, key~AstExpr, value~AstExpr)";
                        if(!_require(sctx, args, ref i, out key))
                        {
                            detailedError = sig + ", key is missing";
                            return false;
                        }
                        if(!_require(sctx,args, ref i, out value))
                        {
                            detailedError = sig + ", value is missing";
                            return false;
                        }

                        node = _base.KeyValuePair(position, key, value);
                        break;
                    }
                case "listliteral":
                    {
                        IEnumerable<AstExpr> elems;
                        const string sig = "ListLiteral(position, elements~List<AstExpr>)";
                        if(!_require(sctx, args, ref i, out elems))
                        {
                            detailedError = sig + ", elements missing";
                            return false;
                        }

                        node = _base.ListLiteral(position, elems);
                        break;
                    }
                case "hashliteral":
                    {
                        IEnumerable<AstExpr> elems;
                        const string sig = "HashLiteral(position, elements~List<AstExpr>)";
                        if (!_require(sctx, args, ref i, out elems))
                        {
                            detailedError = sig + ", elements missing";
                            return false;
                        }

                        node = _base.HashLiteral(position, elems);
                        break;
                    }
                case "logicaland":
                    {
                        IEnumerable<AstExpr> elems;
                        const string sig = "LogicalAnd(position, clauses~List<AstExpr>)";
                        if (!_require(sctx, args, ref i, out elems))
                        {
                            detailedError = sig + ", clauses missing";
                            return false;
                        }

                        node = _base.LogicalAnd(position, elems);
                        break;
                    }
                case "logicalor":
                    {
                        IEnumerable<AstExpr> elems;
                        const string sig = "LogicalOr(position, clauses~List<AstExpr>)";
                        if (!_require(sctx, args, ref i, out elems))
                        {
                            detailedError = sig + ", clauses missing";
                            return false;
                        }

                        node = _base.LogicalOr(position, elems);
                        break;
                    }

                case "null":
                    {
                        node = _base.Null(position);
                        break;
                    }

                case "objectcreation":
                    throw new PrexoniteException("Cannot construct \"ObjectCreation\". Did you mean \"CreateObject\"?");
                case "createobject":
                    {
                        AstTypeExpr typeExpr;
                        const string sig = "CreateObject(position, typeExpr~AstTypeExpr)";
                        if(!_require(sctx, args, ref i, out typeExpr))
                        {
                            detailedError = sig + ", typeExpr is missing";
                            return false;
                        }

                        var obj = _base.CreateObject(position, typeExpr);
                        IEnumerable<AstExpr> argumentList;
                        if (_takeOptionalList(sctx, args, ref i, out argumentList))
                            obj.Arguments.AddRange(argumentList);
                        break;
                    }
                case "typecheck":
                    {
                        AstTypeExpr typeExpr;
                        AstExpr operand;
                        const string sig = "Typecheck(position, operand~AstExpr, type~AstTypeExpr)";
                        if(!_require(sctx, args, ref i, out operand))
                        {
                            detailedError = sig + ", operand is missing.";
                            return false;
                        }
                        if(!_require(sctx, args, ref i, out typeExpr))
                        {
                            detailedError = sig + ", typeExpr is missing.";
                            return false;
                        }

                        node = _base.Typecheck(position, operand, typeExpr);
                        break;
                    }
                case "typecast":
                    {
                        AstTypeExpr typeExpr;
                        AstExpr operand;
                        const string sig = "Typecast(position, operand~AstExpr, type~AstTypeExpr)";
                        if (!_require(sctx, args, ref i, out operand))
                        {
                            detailedError = sig + ", operand is missing.";
                            return false;
                        }
                        if (!_require(sctx, args, ref i, out typeExpr))
                        {
                            detailedError = sig + ", typeExpr is missing.";
                            return false;
                        }

                        node = _base.Typecast(position, operand, typeExpr);
                        break;
                    }
                case "reference":
                    {
                        EntityRef entityRef;
                        const string sig = "Reference(position, entityRef~EntityRef)";
                        if(!_require(sctx, args, ref i, out entityRef))
                        {
                            detailedError = sig + ", entityRef is missing.";
                            return false;
                        }
                        node = _base.Reference(position, entityRef);
                        break;
                    }
                case "memberaccess":
                    {
                        AstExpr receiver;
                        string memberId;
                        PCall nodeCall;
                        const string sig = "MemberAccess(position, receiver~AstExpr, memberId~String, call~PCall, args~List<AstExpr> = [])";
                        if (!_require(sctx, args, ref i, out receiver))
                        {
                            detailedError = sig + ", receiver is missing.";
                            return false;
                        }
                        if(!_require(sctx,args, ref i, out memberId))
                        {
                            detailedError = sig + ", memberId is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out nodeCall);
                        var complex = _base.MemberAccess(position, receiver, memberId, nodeCall);
                        node = _takeOptionalArguments(sctx, args, i, complex);
                        break;
                    }
                case "staticmemberaccess":
                    {
                        AstTypeExpr typeExpr;
                        string memberId;
                        PCall nodeCall;
                        const string sig = "StaticMemberAccess(position, typeExpr~AstTypeExpr, memberId~String, call~PCall, args~List<AstExpr> = [])";
                        if (!_require(sctx, args, ref i, out typeExpr))
                        {
                            detailedError = sig + ", typeExpr is missing.";
                            return false;
                        }
                        if (!_require(sctx, args, ref i, out memberId))
                        {
                            detailedError = sig + ", memberId is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out nodeCall);
                        var complex = _base.StaticMemberAccess(position, typeExpr, memberId, nodeCall);
                        node = _takeOptionalArguments(sctx, args, i, complex);
                        break;
                    }
                case "indirectcall":
                    {
                        AstExpr receiver;
                        PCall nodeCall;
                        const string sig = "IndirectCall(position, receiver~AstExpr, call~PCall, args~List<AstExpr> = [])";
                        if (!_require(sctx, args, ref i, out receiver))
                        {
                            detailedError = sig + ", receiver is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out nodeCall);
                        var complex = _base.IndirectCall(position, receiver, nodeCall);
                        node = _takeOptionalArguments(sctx, args, i, complex);
                        break;
                    }
                case "expand":
                    {
                        PCall nodeCall;
                        EntityRef entity;
                        const string sig = "Expand(position, entity~EntityRef, call~PCall)";
                        if (!_require(sctx, args, ref i, out entity))
                        {
                            detailedError = sig + ", entity is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out nodeCall);
                        var complex = _base.Expand(position, entity, nodeCall);
                        node = _takeOptionalArguments(sctx, args, i, complex);
                        break;
                    }
                case "placeholder":
                    {
                        int? index;
                        PValue raw;
                        _takeOptional(args, ref i, out raw, PType.Null);
                        if (raw.IsNull)
                            index = null;
                        else
                        {
                            PValue intValue;
                            if (raw.TryConvertTo(sctx, PType.Int, true, out intValue))
                                index = (int) intValue.Value;
                            else
                                index = null;
                        }

                        node = _base.Placeholder(position, index);
                        break;
                    }

                case "exprfor":
                    {
                        PValue symbolPV;
                        const string sig = "ExprFor(position, symbol~Symbol)";
                        if (_require(args, ref i, out symbolPV))
                        {
                            detailedError = sig + ", symbol is missing.";
                            return false;
                        }

                        // Parse symbol parameter
                        Symbol symbol;
                        if (symbolPV.TryConvertTo(sctx, out symbol))
                        {
                            // nothing to do in this case
                        }
                        else if (symbolPV.IsNull)
                        {
                            symbol = null;
                        }
                        else
                        {
                            detailedError = sig + ", symbol is expected to be a " + typeof(Symbol).Name + ".";
                            return false;
                        }

                        node = _base.ExprFor(position, symbol);
                        break;
                    }

                    #endregion

                #region Statements/Blocks

                case "block":
                    {
                        node = _base.Block(position);
                        break;
                    }
                case "condition":
                    {
                        AstExpr condition;
                        bool isNegative;
                        const string sig = "Condition(position, condition~AstExpr, isNegative = false)";
                        if(!_require(sctx, args, ref i, out condition))
                        {
                            detailedError = sig + ", condition is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out isNegative);

                        node = _base.Condition(position, condition, isNegative);
                        break;
                    }
                case "whileloop":
                    {
                        AstExpr condition;
                        bool isPostcondition, isNegative;

                        const string sig =
                            "WhileLoop(position, condition~AstExpr, isPostcondition = false, isNegative = false)";

                        if(!_require(sctx, args, ref i, out condition))
                        {
                            detailedError = sig + ", condition is missing.";
                            return false;
                        }
                        _takeOptional(sctx, args, ref i, out isPostcondition);
                        _takeOptional(sctx, args, ref i, out isNegative);

                        node = _base.WhileLoop(position, isPostcondition, isNegative);
                        break;
                    }
                case "forloop":
                    {
                        node = _base.ForLoop(position);
                        break;
                    }
                case "foreachloop":
                    {
                        AstGetSet element;
                        AstExpr sequence;
                        const string sig = "ForeachLoop(position, element~AstGetSet, sequence~AstExpr)";
                        if(!_require(sctx, args, ref i, out element))
                        {
                            detailedError = sig + ", element is missing.";
                            return false;
                        }
                        if(!_require( sctx, args, ref i, out sequence))
                        {
                            detailedError = sig + ", sequence is missing.";
                            return false;
                        }
                        node = _base.ForeachLoop(position);
                        break;
                    }
                case "return":
                    {
                        AstExpr expression;
                        ReturnVariant returnVariant;
                        _takeOptional(sctx, args, ref i, out expression);
                        _takeOptional(sctx, args, ref i, out returnVariant);
                        node = _base.Return(position, expression, returnVariant);
                        break;
                    }
                case "throw":
                    {
                        AstExpr exception;
                        const string sig = "Throw(position, exception~AstExpr)";
                        if(!_require(sctx, args, ref i, out exception))
                        {
                            detailedError = sig + ", exception is missing.";
                            return false;
                        }

                        node = _base.Throw(position, exception);
                        break;
                    }
                case "trycatchfinally":
                    {
                        node = _base.TryCatchFinally(position);
                        break;
                    }
                case "using":
                    {
                        node = _base.Using(position);
                        break;
                    }

                #endregion
            }

            result = sctx.CreateNativePValue(node);
            return node != null;
        }

        private static AstGetSet _takeOptionalArguments(StackContext sctx, PValue[] args, int i, AstGetSet complex)
        {
            IEnumerable<AstExpr> nodeArgs;
            if (_takeOptionalList(sctx, args, ref i, out nodeArgs))
                complex.Arguments.AddRange(nodeArgs);
            return complex;
        }

        #endregion

        #region Implementation of IIndirectCall

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            PValue result;
            if (args.Length == 0)
                throw new PrexoniteException("AstFactory.() requires at least one argument, the name of the node type to create.");
            string detailedError;
            if (TryDynamicCall(sctx, args.Skip(1).ToArray(), PCall.Get, args[0].CallToString(sctx), out result, out detailedError))
                return result;
            else
            {
                if (detailedError == null)
                    detailedError = "illegal call";
                _throwInvalidCall(args, "{0} Original call: AstFactory.({1})", detailedError);
                return PType.Null;
            }
        }

        private static void _throwInvalidCall(IEnumerable<PValue> args, string errorFormat, string detailedError)
        {
            throw new PrexoniteException(string.Format(errorFormat, detailedError,
                                                       args.Select(x => x.Type.ToString()).ToListString()));
        }

        #endregion
    }
}