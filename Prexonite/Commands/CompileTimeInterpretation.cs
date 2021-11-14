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

namespace Prexonite.Commands;

/// <summary>
///     The different interpretations of a compile-time value.
/// </summary>
public enum CompileTimeInterpretation
{
    /// <summary>
    ///     A <code>null</code>-literal. Obtained from <code>ldc.null</code>. Null is the default interpretation.
    /// </summary>
    Null = 0,

    /// <summary>
    ///     A string literal. Obtained from <code>ldc.string</code>. Represented as <see cref = "string" />.
    /// </summary>
    String,

    /// <summary>
    ///     An integer literal. Obtained from <code>ldc.int</code>. Represented as <see cref = "int" />.
    /// </summary>
    Int,

    /// <summary>
    ///     A boolean literal. Obtained from <code>ldc.bool</code>. Represented as <see cref = "bool" />.
    /// </summary>
    Bool,

    /// <summary>
    ///     A local variable reference literal. Obtained from <code>ldr.loc</code> and <code>ldr.loci</code>. Represented as an <see
    ///      cref = "EntityRef" />.
    /// </summary>
    LocalVariableReference,

    /// <summary>
    ///     A global variable reference literal. Obtained from <code>ldr.glob</code>. Represented as an <see cref = "EntityRef" />.
    /// </summary>
    GlobalVariableReference,

    /// <summary>
    ///     A function reference literal. Obtained from <code>ldr.func</code>. Represented as an <see cref = "EntityRef" />.
    /// </summary>
    FunctionReference,

    /// <summary>
    ///     A command reference literal. Obtained from <code>ldr.cmd</code>. Represented as an <see cref = "EntityRef" />.
    /// </summary>
    CommandReference
}