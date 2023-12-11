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

[assembly:
    SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace",
        Target = "Prexonite.Helper")]
[assembly:
    SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "5#",
        Scope = "member",
        Target =
            "Prexonite.Types.ObjectPType.#DynamicCall(Prexonite.StackContext,Prexonite.PValue,Prexonite.PValue[],Prexonite.Types.PCall,System.String,System.Reflection.MemberInfo&)"
        )]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Cil", Scope = "namespace", Target = "Prexonite.Compiler.Cil")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Ctor", Scope = "member",
        Target = "Prexonite.Compiler.Cil.Compiler.#NewPVariableCtor")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = nameof(Prexonite))]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.Concurrency")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.Core")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.Core.Operators")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace",
        Target = "Prexonite.Commands.Core.PartialApplication")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.Lazy")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.List")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.Math")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Commands.Text")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Compiler")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Compiler.Ast")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Compiler.Cil")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Concurrency")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Helper")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Internal")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "namespace", Target = "Prexonite.Types")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "type", Target = "Prexonite.OperatorNames+Prexonite")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "type", Target = "Prexonite.PrexoniteException")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "type", Target = "Prexonite.PrexoniteInvalidStackException"
        )]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "type", Target = "Prexonite.PrexoniteRuntimeException")]
[assembly:
    SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = nameof(Prexonite), Scope = "type",
        Target = "Prexonite.Types.PType+PrexoniteObjectTypeProxy")]