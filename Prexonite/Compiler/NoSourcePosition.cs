using JetBrains.Annotations;

namespace Prexonite.Compiler
{
    [PublicAPI]
    public sealed class NoSourcePosition : ISourcePosition
    {
        #region Singleton

        private static NoSourcePosition _instance;
        public static ISourcePosition Instance
        {
            get { return _instance ?? (_instance = new NoSourcePosition()); }
        }

        #endregion

        public const string MissingFileName = "-";

        #region Implementation of ISourcePosition

        public string File
        {
            get { return MissingFileName; }
        }

        public int Line
        {
            get { return -1; }
        }

        public int Column
        {
            get { return -1; }
        }

        #endregion
    }
}