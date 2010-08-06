using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Lazy
{
    public class ToSeqCommand : CoroutineCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private ToSeqCommand()
        {
        }

        private static readonly ToSeqCommand _instance = new ToSeqCommand();

        public static ToSeqCommand Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Overrides of PCommand

        public override bool IsPure
        {
            get { return false; }
        }

        #endregion

        #region Overrides of CoroutineCommand

        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
        {
            return CoroutineRunStatically(sctxCarrier, args);
        }

        public static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier getSctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            if (getSctx == null)
                throw new ArgumentNullException("getSctx");

            if (args.Length < 1)
                throw new PrexoniteException("toseq requires one argument.");

            var xsT = args[0];
            PValue xs;

            var sctx = getSctx.StackContext;

            while (!(xs = ForceCommand.Force(sctx, xsT)).IsNull)
            {
                //Accept key value pairs directly
                var kvp = xs.Value as PValueKeyValuePair;
                if (kvp != null)
                {
                    yield return kvp.Key;
                    xsT = kvp.Value;
                }
                    //Late bound
                else
                {
                    var k = xs.DynamicCall(sctx, Runtime.EmptyPValueArray, PCall.Get, "Key");
                    yield return k;
                    xsT = xs.DynamicCall(sctx, Runtime.EmptyPValueArray, PCall.Get, "Value");
                }
            }
        }

        #endregion

        #region Implementation of ICilCompilerAware

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            var carrier = new ContextCarrier();
            var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
            carrier.StackContext = corctx;
            return sctx.CreateNativePValue(new Coroutine(corctx));
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}