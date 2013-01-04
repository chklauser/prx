using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler
{
    public static class AstFactoryExtensions
    {
        public static AstGetSet Call(this IAstFactory factory, ISourcePosition position, EntityRef entity,
                                     PCall call = PCall.Get, params AstExpr[] arguments)
        {
            var c = factory.IndirectCall(position, factory.Reference(position, entity), call);
            c.Arguments.AddRange(arguments);
            return c;
        }
    }
}
