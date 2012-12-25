using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler.Internal
{
    public static class CompilerExtensions
    {
        [ContractAnnotation("=>true,indirectCallNode:notnull,referenceNode:notnull;=>false,indirectCallNode:canbenull,referenceNode:canbenull")]
        public static bool TryMatchCall(this AstNode node, out AstIndirectCall indirectCallNode,
                                        out AstReference referenceNode)
        {
            EntityRef entityRef;
            return TryMatchCall(node, out indirectCallNode, out referenceNode, out entityRef);
        }

        [ContractAnnotation("=>true,indirectCallNode:notnull,entityRef:notnull;=>false,indirectCallNode:canbenull,entityRef:canbenull")]
        public static bool TryMatchCall(this AstNode node, out AstIndirectCall indirectCallNode, out EntityRef entityRef)
        {
            AstReference referenceNode;
            return TryMatchCall(node, out indirectCallNode, out referenceNode, out entityRef);
        }

        [ContractAnnotation("=>true,entityRef:notnull;=>false,entityRef:canbenull")]
        public static bool TryMatchCall(this AstNode node, out EntityRef entityRef)
        {
            AstReference referenceNode;
            AstIndirectCall indirectCallNode;
            return TryMatchCall(node, out indirectCallNode, out referenceNode, out entityRef);
        }

        [ContractAnnotation("=>true,referenceNode:notnull,entityRef:notnull;=>false,referenceNode:canbenull,entityRef:canbenull")]
        public static bool TryMatchCall(this AstNode node,
                                        out AstReference referenceNode, out EntityRef entityRef)
        {
            AstIndirectCall indirectCallNode;
            return TryMatchCall(node, out indirectCallNode, out referenceNode, out entityRef);
        }

        [ContractAnnotation("=>true,indirectCallNode:notnull,referenceNode:notnull,entityRef:notnull;=>false,indirectCallNode:canbenull,referenceNode:canbenull,entityRef:canbenull")]
        public static bool TryMatchCall(this AstNode node, out AstIndirectCall indirectCallNode,
                                        out AstReference referenceNode, out EntityRef entityRef)
        {
            if ((indirectCallNode = node as AstIndirectCall) != null
                && (referenceNode = indirectCallNode.Subject as AstReference) != null)
            {
                entityRef = referenceNode.Entity;
                return true;
            }
            else
            {
                referenceNode = null;
                entityRef = null;
                return false;
            }
        }
    }
}
