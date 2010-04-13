using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency
{
    public class Chan : PCommand, ICilCompilerAware
    {
        #region Singleton pattern

        private Chan()
        {
        }

        private static readonly Chan _instance = new Chan();

        public static Chan Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Overrides of PCommand

        public override bool IsPure
        {
            get { return false; }
        }

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return PType.Object.CreatePValue(new Channel());
        }

        #endregion

        #region Implementation of ICilCompilerAware

        CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferCustomImplementation;
        }

        private static readonly ConstructorInfo _channelCtor = typeof (Channel).GetConstructor(new Type[] {});
        private static readonly ConstructorInfo _newPValue = typeof (PValue).GetConstructor(new[] {typeof (object), typeof (PType)});

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            state.EmitIgnoreArguments(ins.Arguments);
            state.Il.Emit(OpCodes.Newobj, _channelCtor);
            PType.PrexoniteObjectTypeProxy.ImplementInCil(state, typeof (Channel));
            state.Il.Emit(OpCodes.Newobj, _newPValue);
        }

        #endregion
    }
}