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
using Prexonite.Modular;
using Prexonite.Types;

namespace Prexonite.Compiler.Macro.Commands;

public class CallSub : MacroCommand
{
    public const string Alias = @"call\sub";

    #region Singleton pattern

    public static CallSub Instance { get; } = new();

    CallSub() : base(Alias)
    {
    }

    #endregion

    #region Overrides of MacroCommand

    protected override void DoExpand(MacroContext context)
    {
        var perform =
            context.Factory.Call(context.Invocation.Position, EntityRef.Command.Create(Engine.CallSubPerformAlias),
                PCall.Get, context.Invocation.Arguments.ToArray());
        var interpret = context.Factory.Expand(context.Invocation.Position,
            EntityRef.MacroCommand.Create(CallSubInterpret.Alias), context.Invocation.Call);
            
        interpret.Arguments.Add(perform);

        context.Block.Expression = interpret;
    }

    #endregion
}