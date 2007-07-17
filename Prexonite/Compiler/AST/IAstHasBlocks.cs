using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Ast
{
    public interface IAstHasBlocks
    {
        AstBlock[] Blocks
        {
            get;
        }
    }
}
