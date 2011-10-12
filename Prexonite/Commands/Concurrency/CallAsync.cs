using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Prexonite.Commands.Core;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Compiler.Macro.Commands;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency
{
    public class CallAsync : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private CallAsync()
        {
        }

        private static readonly CallAsync _instance = new CallAsync();

        public static CallAsync Instance
        {
            get { return _instance; }
        }

        #endregion

        public const string Alias = @"call\async\perform";

        #region Overrides of PCommand

        [Obsolete]
        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null || args.Length == 0 || args[0] == null)
                return PType.Null.CreatePValue();

            var iargs = Call.FlattenArguments(sctx, args, 1);

            var retChan = new Channel();
            var T = new Thread(() =>
                                   {
                                       PValue result;
                                       try
                                       {
                                           result = args[0].IndirectCall(sctx, iargs.ToArray());
                                       }
                                       catch (Exception ex)
                                       {
                                           result = sctx.CreateNativePValue(ex);
                                       }
                                       retChan.Send(result);
                                   })
            {
                IsBackground = true
            };
            T.Start();
            return PType.Object.CreatePValue(retChan);
        }

        public static Channel RunAsync(StackContext sctx, Func<PValue> comp)
        {
            var retChan = new Channel();
            var T = new Thread(() => retChan.Send(comp()))
            {
                IsBackground = true
            };
            T.Start();
            return retChan;
        }

        #endregion

        #region Implementation of ICilCompilerAware

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException("The command " + GetType().Name + " does not support CIL compilation via ICilCompilerAware.");
        }

        #endregion

        #region Partial application via call\star

        private readonly PartialCallWrapper _partial = new PartialCallWrapper(
            Engine.Call_AsyncAlias, Alias, SymbolInterpretations.Command);

        public PartialCallWrapper Partial
        {
            [DebuggerStepThrough]
            get { return _partial; }
        }

        #endregion
    }
}