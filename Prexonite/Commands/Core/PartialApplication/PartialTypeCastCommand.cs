using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialTypeCastCommand : PartialWithPTypeCommandBase<PTypeInfo>
    {
        private static readonly PartialTypeCastCommand _instance = new PartialTypeCastCommand();
        private PartialTypeCastCommand()
        {
        }
        public static PartialTypeCastCommand Instance
        {
            get { return _instance; }
        }

        private ConstructorInfo _partialTypeCastCtor;

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, PTypeInfo parameter)
        {
            return new PartialTypeCast(mappings, closedArguments, parameter.Type);
        }

        protected override ConstructorInfo GetConstructorCtor(PTypeInfo pTypeInfo)
        {
            return _partialTypeCastCtor ??
                   (_partialTypeCastCtor =
                    typeof (PartialTypeCast).GetConstructor(new[] {typeof (int[]), typeof (PValue[]), typeof (PType)}));
        }

        protected override Type GetPartialCallRepresentationType(PTypeInfo parameter)
        {
            return typeof(PartialTypeCast);
        }

        protected override string PartialApplicationKind
        {
            get { return "Partial type cast"; }
        }
    }
}
