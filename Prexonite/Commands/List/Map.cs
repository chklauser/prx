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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.List
{
    /// <summary>
    ///     Implementation of the map function. Applies a supplied function (#1) to every 
    ///     value in the supplied list (#2) and returns a list with the result values.
    /// </summary>
    /// <remarks>
    ///     <code>function map(ref f, var lst)
    ///         {
    ///         var nlst = [];
    ///         foreach(var x in lst)
    ///         nlst[] = f(x);
    ///         return nlst;
    ///         }</code>
    /// </remarks>
    public class Map : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton

        private Map()
        {
        }

        private static readonly Map _instance = new Map();

        public static Map Instance
        {
            get { return _instance; }
        }

        #endregion

        /// <summary>
        ///     Tries to turn a generic PValue object into an <see cref = "IEnumerable{PValue}" /> if possible. Returns null if <paramref
        ///      name = "psource" /> cannot be enumerated over.
        /// </summary>
        /// <param name = "sctx"></param>
        /// <param name = "psource"></param>
        /// <returns></returns>
        internal static IEnumerable<PValue> _ToEnumerable(StackContext sctx, PValue psource)
        {
            switch (psource.Type.ToBuiltIn())
            {
                case PType.BuiltIn.List:
                    return (IEnumerable<PValue>) psource.Value;
                case PType.BuiltIn.Object:
                    var clrType = ((ObjectPType) psource.Type).ClrType;
                    if (typeof (IEnumerable<PValue>).IsAssignableFrom(clrType))
                        goto case PType.BuiltIn.List;
                    else if (typeof (IEnumerable).IsAssignableFrom(clrType))
                        return _wrapNonGenericIEnumerable(sctx, (IEnumerable) psource.Value);

                    break;
            }
            IEnumerable<PValue> set;
            IEnumerable nset;
            if (psource.TryConvertTo(sctx, true, out set))
                return set;
            else if (psource.TryConvertTo(sctx, true, out nset))
                return _wrapNonGenericIEnumerable(sctx, nset);
            else
                return _wrapDynamicIEnumerable(sctx, psource);
        }

        private static IEnumerable<PValue> _wrapDynamicIEnumerable(StackContext sctx, PValue psource)
        {
            var pvEnumerator =
                psource.DynamicCall(sctx, Runtime.EmptyPValueArray, PCall.Get, "GetEnumerator").
                    ConvertTo(sctx, typeof (IEnumerator));
            var enumerator = (IEnumerator) pvEnumerator.Value;
            PValueEnumerator pvEnum;
            try
            {
                if ((pvEnum = enumerator as PValueEnumerator) != null)
                {
                    while (pvEnum.MoveNext())
                        yield return pvEnum.Current;
                }
                else
                {
                    while (enumerator.MoveNext())
                        yield return sctx.CreateNativePValue(enumerator.Current);
                }
            }
            finally
            {
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Coroutine")]
        protected static IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
            IIndirectCall f, IEnumerable<PValue> source)
        {
            var sctx = sctxCarrier.StackContext;

            foreach (var x in source)
                yield return f != null ? f.IndirectCall(sctx, new[] {x}) : x;
        }

        /// <summary>
        ///     Executes the map command.
        /// </summary>
        /// <param name = "sctxCarrier">The stack context in which to call the supplied function.</param>
        /// <param name = "args">The list of arguments to be passed to the command.</param>
        /// <returns>A coroutine that maps the.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Coroutine")]
        protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier,
            PValue[] args)
        {
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");
            if (args == null)
                throw new ArgumentNullException("args");

            var sctx = sctxCarrier.StackContext;

            //Get f
            IIndirectCall f;
            if (args.Length < 1)
                f = null;
            else
                f = args[0];

            //Get the source
            IEnumerable<PValue> source;
            if (args.Length == 2)
            {
                var psource = args[1];
                source = _ToEnumerable(sctx, psource) ?? new[] {psource};
            }
            else
            {
                var lstsource = new List<PValue>();
                for (var i = 1; i < args.Length; i++)
                {
                    var multiple = _ToEnumerable(sctx, args[i]);
                    if (multiple != null)
                        lstsource.AddRange(multiple);
                    else
                        lstsource.Add(args[i]);
                }
                source = lstsource;
            }

            //Note: need to forward element because this method must remain lazy.
            foreach (var value in CoroutineRun(sctxCarrier, f, source))
            {
                yield return value;
            }
        }

        private static IEnumerable<PValue> _wrapNonGenericIEnumerable(StackContext sctx,
            IEnumerable nonGeneric)
        {
            foreach (var obj in nonGeneric)
                yield return sctx.CreateNativePValue(obj);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            var carrier = new ContextCarrier();
            var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
            carrier.StackContext = corctx;
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        /// <summary>
        ///     A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>
        ///     Pure commands can be applied at compile time.
        /// </remarks>
        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        #region ICilCompilerAware Members

        /// <summary>
        ///     Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name = "ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        /// <summary>
        ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name = "state">The compiler state.</param>
        /// <param name = "ins">The instruction to compile.</param>
        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion

        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
            PValue[] args)
        {
            return CoroutineRunStatically(sctxCarrier, args);
        }
    }
}