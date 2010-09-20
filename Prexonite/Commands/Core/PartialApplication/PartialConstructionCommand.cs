using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialConstructionCommand : PartialApplicationCommandBase<PartialConstructionCommand.PTypeInfo>
    {

        #region Singleton pattern

        private PartialConstructionCommand()
        {
        }

        private static readonly PartialConstructionCommand _instance = new PartialConstructionCommand();
        private ConstructorInfo _ptypeConstructCtor;

        public static PartialConstructionCommand Instance
        {
            get { return _instance; }
        }

        internal ConstructorInfo _PTypeConstructCtor
        {
            get
            {
                return (_ptypeConstructCtor
                        ??
                        (_ptypeConstructCtor =
                         typeof (PartialConstruction).GetConstructor(
                             new[] {typeof (int[]), typeof (PValue[]), typeof (PType)})));
            }
        }

        #endregion 

        public struct PTypeInfo
        {
            public PType Type;
            public string Expr;
        }

        protected override PTypeInfo FilterRuntimeArguments(StackContext sctx, ref ArraySegment<PValue> arguments)
        {
            if (arguments.Count < 1)
                throw new PrexoniteException("Partial construction requires at least one argument.");

            var raw = arguments.Array[arguments.Offset + arguments.Count - 1];
            PType ptype;
            if(!(raw.Type is ObjectPType && (object)(ptype = raw.Value as PType) != null))
            {
                var ptypeExpr = raw.CallToString(sctx);
                ptype = sctx.ConstructPType(ptypeExpr);
            }

            arguments = new ArraySegment<PValue>(arguments.Array, arguments.Offset, arguments.Count - 1);
            return new PTypeInfo {Type = ptype};
        }

        protected override bool FilterCompileTimeArguments(ref ArraySegment<CompileTimeValue> staticArgv, out PTypeInfo parameter)
        {
            parameter = default(PTypeInfo);
            if (staticArgv.Count < 1)
                return false;

            var raw = staticArgv.Array[staticArgv.Offset + staticArgv.Count - 1];
            string ptypeExpr;
            if (!raw.TryGetString(out ptypeExpr))
                return false;

            parameter.Expr = ptypeExpr;
            staticArgv = new ArraySegment<CompileTimeValue>(staticArgv.Array, staticArgv.Offset, staticArgv.Count - 1);
            return true;
        }

        protected override void EmitConstructorCall(CompilerState state, PTypeInfo parameter)
        {
            state.EmitLoadLocal(state.SctxLocal);
            state.Il.Emit(OpCodes.Ldstr,parameter.Expr);
            state.EmitCall(Runtime.ConstructPTypeMethod);
            state.Il.Emit(OpCodes.Newobj,_PTypeConstructCtor);
        }

        #region Overrides of PartialApplicationCommandBase<TypeInfo>

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, PTypeInfo parameter)
        {
            return new PartialConstruction(mappings, closedArguments, parameter.Type);
        }

        protected override Type GetPartialCallRepresentationType(PTypeInfo parameter)
        {
            return typeof (PartialConstruction);
        }

        #endregion
    }
}
