// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using JetBrains.Annotations;
using Prexonite.Compiler.Ast;
using Prexonite.Modular;

namespace Prexonite.Compiler;

public static class CompilerExtensions
{
    [ContractAnnotation("=>true,indirectCallNode:notnull,referenceNode:notnull;=>false,indirectCallNode:canbenull,referenceNode:canbenull")]
    public static bool TryMatchCall(this AstNode node, out AstIndirectCall indirectCallNode,
        out AstReference referenceNode)
    {
        return TryMatchCall(node, out indirectCallNode, out referenceNode, out _);
    }

    [ContractAnnotation("=>true,indirectCallNode:notnull,entityRef:notnull;=>false,indirectCallNode:canbenull,entityRef:canbenull")]
    public static bool TryMatchCall(this AstNode node, out AstIndirectCall indirectCallNode, out EntityRef entityRef)
    {
        return TryMatchCall(node, out indirectCallNode, out _, out entityRef);
    }

    [ContractAnnotation("=>true,entityRef:notnull;=>false,entityRef:canbenull")]
    public static bool TryMatchCall(this AstNode node, out EntityRef entityRef)
    {
        return TryMatchCall(node, out _, out _, out entityRef);
    }

    [ContractAnnotation("=>true,referenceNode:notnull,entityRef:notnull;=>false,referenceNode:canbenull,entityRef:canbenull")]
    public static bool TryMatchCall(this AstNode node,
        out AstReference referenceNode, out EntityRef entityRef)
    {
        return TryMatchCall(node, out _, out referenceNode, out entityRef);
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

    [ContractAnnotation("=>true,indirectCallNode:notnull;=>false,indirectCallNode:canbenull")]
    public static bool IsCommandCall(this AstNode node, string commandAlias, out AstIndirectCall indirectCallNode)
    {
        return node.TryMatchCall(out indirectCallNode, out EntityRef entityRef) && entityRef.TryGetCommand(out var cmd) &&
            Engine.StringsAreEqual(cmd.Id, commandAlias);
    }
}