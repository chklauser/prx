using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Commands
{
    public abstract class StackAwareCommand : PCommand, IStackAware
    {
        public abstract StackContext CreateStackContext(StackContext sctx, PValue[] args);

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            StackContext rctx = CreateStackContext(sctx, args);
            return sctx.ParentEngine.Process(rctx);
        } 

    }
}
