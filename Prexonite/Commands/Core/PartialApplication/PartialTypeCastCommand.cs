using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialTypecastCommand : PartialWithPTypeCommandBase<PTypeInfo>
    {
        private static readonly PartialTypecastCommand _instance = new PartialTypecastCommand();
        private PartialTypecastCommand()
        {
        }
        public static PartialTypecastCommand Instance
        {
            get { return _instance; }
        }

        private ConstructorInfo _partialTypeCastCtor;

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, PTypeInfo parameter)
        {
            return new PartialTypecast(mappings, closedArguments, parameter.Type);
        }

        protected override ConstructorInfo GetConstructorCtor(PTypeInfo parameter)
        {
            return _partialTypeCastCtor ??
                   (_partialTypeCastCtor =
                    typeof (PartialTypecast).GetConstructor(new[] {typeof (int[]), typeof (PValue[]), typeof (PType)}));
        }

        protected override Type GetPartialCallRepresentationType(PTypeInfo parameter)
        {
            return typeof(PartialTypecast);
        }

        protected override string PartialApplicationKind
        {
            get { return "Partial type cast"; }
        }
    }
}
