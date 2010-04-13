using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class Call_CC : StackAwareCommand, ICilCompilerAware
    {
        private Call_CC()
        {
        }

        private static readonly Call_CC _instance = new Call_CC();

        public static Call_CC Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn, ref continuation);
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Returns null if no callable object is passed.
        /// </para>
        /// <para>
        ///     Uses the <see cref="IIndirectCall"/> interface.
        /// </para>
        /// <para>
        ///     Wrap Lists in other lists, if you want to pass them without being unfolded: 
        /// <code>
        /// function main()
        /// {   var myList = [1, 2, 3];
        ///     var f = xs => xs.Count;
        ///     print( call(f, [ myList ]) );
        /// }
        /// 
        /// //Prints "3"
        /// </code>
        /// </para>
        /// </remarks>
        /// <seealso cref="IIndirectCall"/>
        /// <param name="sctx">The stack context in which to call the callable argument.</param>
        /// <param name="args">A list of the form [ ref f, arg1, arg2, arg3, ..., argn].<br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by <see cref="IIndirectCall.IndirectCall"/> or PValue Null if no callable object has been passed.</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null || args[0].IsNull)
                return PType.Null.CreatePValue();

            PValue callable;
            List<PValue> iargs;

            create_current_continuation(sctx, args, out callable, out iargs);

            //continue with execution of the callee
            return callable.IndirectCall(sctx, iargs.ToArray());
        }

        public override StackContext CreateStackContext(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null || args[0].IsNull)
                return new NullContext(sctx);

            PValue callable;
            List<PValue> iargs;

            create_current_continuation(sctx, args, out callable, out iargs);

            return Call.CreateStackContext(sctx, callable, iargs.ToArray());
        }

        private static void create_current_continuation(StackContext sctx, PValue[] args, out PValue callable, out List<PValue> iargs)
        {
            iargs = Call.FlattenArguments(sctx, args, 1);

            callable = args[0];

            var fctx = sctx as FunctionContext;
            if (fctx == null)
                throw new PrexoniteException(@"Call\cc can only be called from within functions.");

            //Insert the continuation as the first argument
            iargs.Insert(0, sctx.CreateNativePValue(new Continuation(fctx)));
        }

        #region ICilCompilerAware Members

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.IsIncompatible;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}