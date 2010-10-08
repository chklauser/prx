using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class Not : PCommand
    {
        private static readonly Not _instance = new Not();
        public static Not Instance
        {
            get { return _instance; }
        }

        private Not()
        {
        }

        public override bool IsPure
        {
            get { return true; }
        }

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
