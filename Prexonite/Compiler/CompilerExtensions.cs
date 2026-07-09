

using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler;

public static class CompilerExtensions
{
    extension(AstNode node)
    {
        [ContractAnnotation("=>true,indirectCallNode:notnull,referenceNode:notnull;=>false,indirectCallNode:canbenull,referenceNode:canbenull")]
        public bool TryMatchCall([NotNullWhen(true)] out AstIndirectCall? indirectCallNode,
            [NotNullWhen(true)] out AstReference? referenceNode)
        {
            EntityRef? dummy;
            return node.TryMatchCall(out indirectCallNode, out referenceNode, out dummy);
        }

        [ContractAnnotation("=>true,indirectCallNode:notnull,entityRef:notnull;=>false,indirectCallNode:canbenull,entityRef:canbenull")]
        public bool TryMatchCall(
            [NotNullWhen(true)] out AstIndirectCall? indirectCallNode,
            [NotNullWhen(true)] out EntityRef? entityRef
        )
        {
            AstReference? dummy;
            return node.TryMatchCall(out indirectCallNode, out dummy, out entityRef);
        }

        [ContractAnnotation("=>true,entityRef:notnull;=>false,entityRef:canbenull")]
        public bool TryMatchCall([NotNullWhen(true)] out EntityRef? entityRef)
        {
            AstIndirectCall? dummy1;
            AstReference? dummy2;
            return node.TryMatchCall(out dummy1, out dummy2, out entityRef);
        }

        [ContractAnnotation("=>true,referenceNode:notnull,entityRef:notnull;=>false,referenceNode:canbenull,entityRef:canbenull")]
        public bool TryMatchCall(
            [NotNullWhen(true)] out AstReference? referenceNode, [NotNullWhen(true)] out EntityRef? entityRef)
        {
            AstIndirectCall? dummy;
            return node.TryMatchCall(out dummy, out referenceNode, out entityRef);
        }

        [ContractAnnotation("=>true,indirectCallNode:notnull,referenceNode:notnull,entityRef:notnull;=>false,indirectCallNode:canbenull,referenceNode:canbenull,entityRef:canbenull")]
        public bool TryMatchCall([NotNullWhen(true)] out AstIndirectCall? indirectCallNode,
            [NotNullWhen(true)] out AstReference? referenceNode, [NotNullWhen(true)] out EntityRef? entityRef)
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

        [ContractAnnotation("=>true,indirectCallNode:notnull;=>false,indirectCallNode:canbenull")]
        public bool IsCommandCall(
            string commandAlias,
            [NotNullWhen(true)] out AstIndirectCall? indirectCallNode
        )
        {
            return node.TryMatchCall(out indirectCallNode, out EntityRef? entityRef) && entityRef.TryGetCommand(out var cmd) &&
                Engine.StringsAreEqual(cmd.Id, commandAlias);
        }
    }
}