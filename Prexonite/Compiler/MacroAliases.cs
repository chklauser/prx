using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prexonite.Compiler
{
    public static class MacroAliases
    {
        public const string NewLocalVariableAlias = "newvar";
        public const string LoaderAlias = "loader";
        public const string TargetAlias = "target";
        public const string LocalsAlias = "locals";
        public const string CallTypeAlias = "callType";
        public const string JustEffectAlias = "justEffect";
        public const string MacroInvocationAlias = "macroInvocation";

        public static IEnumerable<string> Aliases()
        {
            yield return NewLocalVariableAlias;
            yield return LoaderAlias;
            yield return TargetAlias;
            yield return LocalsAlias;
            yield return CallTypeAlias;
            yield return JustEffectAlias;
            yield return MacroInvocationAlias;
        }
    }
}