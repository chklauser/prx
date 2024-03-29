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
#region Namespace Imports

using System.Diagnostics;
using System.Reflection.Emit;

#endregion

namespace Prexonite.Compiler.Cil;

public class CilSymbol
{
    [DebuggerStepThrough]
    public CilSymbol(SymbolKind kind)
    {
        Kind = kind;
    }

    public SymbolKind Kind { get; set; }

    public LocalBuilder? Local { get; set; }

    public void EmitLoad(CompilerState state)
    {
        if (Local == null)
        {
            throw new PrexoniteException("Internal error: CilSymbol is not bound to a variable.");
        }
        
        switch (Kind)
        {
            case SymbolKind.Local:
                state.EmitLoadLocal(Local.LocalIndex);
                break;
            case SymbolKind.LocalRef:
                state.EmitLoadLocal(Local.LocalIndex);
                state.Il.EmitCall(OpCodes.Call, Compiler.GetValueMethod, null);
                break;
            default:
                throw new PrexoniteException("Internal error: cannot emit load for enumeration variable.");
        }
    }
}

public enum SymbolKind
{
    Local,
    LocalRef,
    LocalEnum,
}