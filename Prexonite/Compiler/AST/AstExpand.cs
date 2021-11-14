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
using System;
using Prexonite.Compiler.Macro;
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast;

public class AstExpand : AstGetSetImplBase, IAstPartiallyApplicable
{
    public AstExpand(ISourcePosition position, EntityRef entity, PCall call) : base(position, call)
    {
        Entity = entity;
    }

    public EntityRef Entity { get; }

    protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        throw new NotSupportedException("Macro expansion requires a different mechanism. Use AstGetSet.EmitCode instead.");
    }

    protected override void EmitSetCode(CompilerTarget target)
    {
        throw new NotSupportedException("Macro expansion requires a different mechanism. Use AstGetSet.EmitCode instead.");
    }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //instantiate macro for the current target
        MacroSession session = null;

        try
        {
            //Acquire current macro session
            session = target.AcquireMacroSession();

            //Expand macro
            var justEffect = stackSemantics == StackSemantics.Effect;
            var node = session.ExpandMacro(this, justEffect);

            //Emit generated code
            node.EmitCode(target, stackSemantics);
        }
        finally
        {
            if (session != null)
                target.ReleaseMacroSession(session);
        }
    }

    void IAstPartiallyApplicable.DoEmitPartialApplicationCode(CompilerTarget target)
    {
        // This may fail if the macro implementation does not support partial application.
        DoEmitCode(target, StackSemantics.Value);
    }

    public override bool TryOptimize(CompilerTarget target, out AstExpr expr)
    {
        //Do not optimize the macros arguments! They should be passed to the macro in their original form.
        //  the macro should decide whether or not to apply AST-optimization to the arguments or not.
        expr = null;
        return false;
    }

    public override AstGetSet GetCopy()
    {
        var copy = new AstExpand(Position, Entity, Call);
        CopyBaseMembers(copy);
        return copy;
    }

    public override string ToString()
    {
        return
            $"expand {(Enum.GetName(typeof(PCall), Call) ?? "-").ToLowerInvariant()}: {Entity}({ArgumentsToString()})";
    }
}