using System;
using Prexonite.Properties;

namespace Prexonite.Compiler.Symbolic.Internal
{
    public class ReplaceCoreNilHandler : TransformHandler<Symbol>
    {
        #region Singleton Pattern

        private static readonly ReplaceCoreNilHandler _instance = new ReplaceCoreNilHandler();

        public static ReplaceCoreNilHandler Instance
        {
            get { return _instance; }
        }

        private ReplaceCoreNilHandler()
        {
        }

        #endregion

        public override Symbol HandleNil(NilSymbol self, Symbol argument)
        {
            return argument;
        }
    }
}