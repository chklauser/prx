using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Compiler
{
    public static class MacroAliases
    {
        public const string ContextAlias = "context";

        public static IEnumerable<string> Aliases()
        {
            yield return ContextAlias;
        }
    }
}