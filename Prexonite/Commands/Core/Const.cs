using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class Const : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private static readonly Const _instance = new Const();

        public static Const Instance
        {
            get { return _instance; }
        }

        private Const()
        {
        }

        #endregion

        public const string Alias = "const";

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            PValue constant;
            if (args.Length < 1)
                constant = PType.Null;
            else
                constant = args[0];

            return CreateConstFunction(constant, sctx);
        }

        private class Impl : IIndirectCall
        {
            private readonly PValue _value;

            public Impl(PValue value)
            {
                _value = value;
            }

            public PValue IndirectCall(StackContext sctx, PValue[] args)
            {
                return _value;
            }
        }

        private MethodInfo _createConstFunctionInfoCache;
        private MethodInfo _createConstFunction
        {
            get
            {
                return _createConstFunctionInfoCache ??
                       (_createConstFunctionInfoCache =
                        typeof (Const).GetMethod("CreateConstFunction",
                                                 new[] {typeof (StackContext), typeof (PValue)}));
            }
        }
        public static PValue CreateConstFunction(PValue constant, StackContext sctx)
        {
            return sctx.CreateNativePValue(new Impl(constant));
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PrefersCustomImplementation;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            var argc = ins.Arguments;
            if(argc > 1)
                state.EmitIgnoreArguments(argc-1);

            state.EmitLoadLocal(state.SctxLocal);
            if (argc == 0)
                state.EmitLoadNullAsPValue();
            
            state.EmitCall(_createConstFunction);
        }
    }
}
