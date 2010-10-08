using System;
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    /// <summary>
    /// <para>Common base class for partial application commands (constructors) that deal with an additional PType parameter (such as type casts)</para>
    /// <para>This class exists to share implementation. DO NOT use it for classification.</para>
    /// </summary>
    /// <typeparam name="T">The parameter POCO. <see cref="PTypeInfo"/> can be used if no additional information is required.</typeparam>
    public abstract class PartialWithPTypeCommandBase<T> : PartialApplicationCommandBase<T> where T : PTypeInfo,new()
    {
        /// <summary>
        /// The human readable name of this kind of partial application. Used in error messages.
        /// </summary>
        protected abstract string PartialApplicationKind { get; }

        protected override T FilterRuntimeArguments(StackContext sctx, ref ArraySegment<PValue> arguments)
        {
            if (arguments.Count < 1)
            {
                throw new PrexoniteException(string.Format("{0} requires a PType argument (or a PType expression).", PartialApplicationKind));
            }

            var raw = arguments.Array[arguments.Offset + arguments.Count - 1];
            PType ptype;
            //Allow the type to be specified as a type expression (instead of a type instance)
            if(!(raw.Type is ObjectPType && (object)(ptype = raw.Value as PType) != null))
            {
                var ptypeExpr = raw.CallToString(sctx);
                ptype = sctx.ConstructPType(ptypeExpr);
            }

            arguments = new ArraySegment<PValue>(arguments.Array, arguments.Offset, arguments.Count - 1);
            return new T {Type = ptype};
        }

        protected override bool FilterCompileTimeArguments(ref ArraySegment<CompileTimeValue> staticArgv, out T parameter)
        {
            parameter = default(T);
            if (staticArgv.Count < 1)
                return false;

            var raw = staticArgv.Array[staticArgv.Offset + staticArgv.Count - 1];
            string ptypeExpr;
            if (!raw.TryGetString(out ptypeExpr))
                return false;

            parameter = new T {Expr = ptypeExpr};
            staticArgv = new ArraySegment<CompileTimeValue>(staticArgv.Array, staticArgv.Offset, staticArgv.Count - 1);
            return true;
        }

        protected override void EmitConstructorCall(CompilerState state, T parameter)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.Emit(OpCodes.Ldstr,parameter.Expr);
            state.EmitCall(Runtime.ConstructPTypeMethod);
            base.EmitConstructorCall(state, parameter);
        }
    }
}