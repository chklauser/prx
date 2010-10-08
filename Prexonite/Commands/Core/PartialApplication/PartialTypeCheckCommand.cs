using System;
using System.Reflection;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialTypeCheckCommand : PartialWithPTypeCommandBase<PTypeInfo>
    {
        private static readonly PartialTypeCheckCommand _instance = new PartialTypeCheckCommand();
        private PartialTypeCheckCommand()
        {
        }
        public static PartialTypeCheckCommand Instance
        {
            get { return _instance; }
        }

        private ConstructorInfo _partialTypeCheckCtor;

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, PTypeInfo parameter)
        {
            return new PartialTypeCheck(mappings, closedArguments, parameter.Type);
        }

        protected override ConstructorInfo GetConstructorCtor(PTypeInfo pTypeInfo)
        {
            return _partialTypeCheckCtor ??
                   (_partialTypeCheckCtor =
                    typeof (PartialTypeCheck).GetConstructor(new[] {typeof (int[]), typeof (PValue[]), typeof (PType)}));
        }

        protected override Type GetPartialCallRepresentationType(PTypeInfo parameter)
        {
            return typeof(PartialTypeCheck);
        }

        protected override string PartialApplicationKind
        {
            get { return "Partial type check"; }
        }
    }
}