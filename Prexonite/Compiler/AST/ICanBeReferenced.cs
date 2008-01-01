using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Ast
{
    interface ICanBeReferenced
    {
        ICollection<IAstExpression> Arguments{ get; }
        bool TryToReference(out AstGetSet reference);
    }
}
