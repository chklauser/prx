using System;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class Not : PCommand
    {
        public static Not Instance { get; } = new();

        private Not()
        {
        }

        [Obsolete("IsPure mechanism was abandoned in v1.2. Use ICilExtension to perform constant folding instead.")]
        public override bool IsPure => true;

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            foreach (var arg in args)
            {
                var b = arg.Type.ToBuiltIn() != PType.BuiltIn.Bool
                             ? (bool) arg.ConvertTo(sctx, PType.Bool, true).Value
                             : (bool) arg.Value;
                if(b)
                    return false;
            }

            return true;
        }
    }
}
