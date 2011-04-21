using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialConstructionCommand : PartialWithPTypeCommandBase<PTypeInfo>
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

        #endregion

        #region Overrides of PartialApplicationCommandBase<TypeInfo>

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings, PValue[] closedArguments, PTypeInfo parameter)
        {
            return new PartialConstruction(mappings, closedArguments, parameter.Type);
        }

        protected override ConstructorInfo GetConstructorCtor(PTypeInfo parameter)
        {
            return (_ptypeConstructCtor
                    ??
                    (_ptypeConstructCtor =
                     GetPartialCallRepresentationType(parameter).GetConstructor(
                         new[] { typeof(int[]), typeof(PValue[]), typeof(PType) })));
        }

        protected override Type GetPartialCallRepresentationType(PTypeInfo parameter)
        {
            return typeof(PartialConstruction);
        }

        protected override string PartialApplicationKind
        {
            get { return "Partial Construction"; }
        }

        #endregion
    }
}
