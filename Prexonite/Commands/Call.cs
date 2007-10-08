using System;
using System.Collections.Generic;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Returns null if no callable object is passed.
    /// </para>
    /// <para>
    ///     Uses the <see cref="IIndirectCall"/> interface.
    /// </para>
    /// </remarks>
    /// <seealso cref="IIndirectCall"/>
    public class Call : PCommand
    {
        /// <summary>
        /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
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
            if (args == null || args.Length == 0 || args[0] == null)
                return PType.Null.CreatePValue();

            List<PValue> iargs = new List<PValue>();
            for (int i = 1; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> folded = MapAll._ToEnumerable(arg);
                if (folded == null)
                    iargs.Add(arg);
                else
                    iargs.AddRange(folded);
            }

            return args[0].IndirectCall(sctx, iargs.ToArray());
        }

        /// <summary>
        /// Implementation of (ref f, arg1, arg2, arg3, ..., argn) => f(arg1, arg2, arg3, ..., argn);
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Returns PValue null if no callable object is passed.
        /// </para>
        /// <para>
        ///     Uses the <see cref="IIndirectCall"/> interface.
        /// </para>
        /// </remarks>
        /// <seealso cref="IIndirectCall"/>
        /// <param name="sctx">The stack context in which to call <paramref name="callable"/>.</param>
        /// <param name="callable">The <see cref="IIndirectCall"/> implementation to call.</param>
        /// <param name="args">The array of arguments to pass to <see cref="IIndirectCall.IndirectCall"/>. <br />
        /// Lists and coroutines are expanded.</param>
        /// <returns>The result returned by <see cref="IIndirectCall.IndirectCall"/> or PValue null if <paramref name="callable"/> is null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sctx"/> is null.</exception>
        public PValue Run(StackContext sctx, IIndirectCall callable, params PValue[] args)
        {
            if (callable == null)
                return PType.Null.CreatePValue();
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                args = new PValue[] {};

            List<PValue> iargs = new List<PValue>();
            for (int i = 0; i < args.Length; i++)
            {
                PValue arg = args[i];
                IEnumerable<PValue> folded = MapAll._ToEnumerable(arg);
                if (folded == null)
                    iargs.Add(arg);
                else
                    iargs.AddRange(folded);
            }

            return callable.IndirectCall(sctx, iargs.ToArray());
        }
    }
}