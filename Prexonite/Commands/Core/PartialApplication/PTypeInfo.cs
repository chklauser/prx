﻿// Prexonite
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

namespace Prexonite.Commands.Core.PartialApplication;

/// <summary>
///     Holds information about a PType at compile- and at run-time. Used in <see cref = "PartialWithPTypeCommandBase{T}" />.
/// </summary>
public class PTypeInfo
{
    /// <summary>
    ///     The runtime instance of the PType.
    /// </summary>
    public PType? Type;

    /// <summary>
    ///     The compile time constant PType expression.
    /// </summary>
    public required string? Expr;
}

public abstract record RuntimePTypeInfo<TSelf> : IRuntimePTypeInfo<TSelf>
    where TSelf : IRuntimePTypeInfo<TSelf>, new()
{
    public PType Type { get; init; } = null!;
    public static TSelf Create(PType type) => new() { Type = type };
}

public record RuntimePTypeInfo : RuntimePTypeInfo<RuntimePTypeInfo>;

public abstract record CompileTimePTypeInfo<TSelf> : ICompileTimePType<TSelf>
    where TSelf : ICompileTimePType<TSelf>, new()
{
    public string Expr { get; init; } = null!;
    public static TSelf Create(string expr) => new() { Expr = expr };
}

public record CompileTimePTypeInfo : CompileTimePTypeInfo<CompileTimePTypeInfo>;

[SuppressMessage("ReSharper", "TypeParameterCanBeVariant", Justification = "Self")]
public interface IRuntimePTypeInfo<TSelf>
    where TSelf : IRuntimePTypeInfo<TSelf>
{
    PType Type { get; init; }
    static abstract TSelf Create(PType type);
}

[SuppressMessage("ReSharper", "TypeParameterCanBeVariant", Justification = "Self")]
public interface ICompileTimePType<TSelf> 
where TSelf : ICompileTimePType<TSelf>
{
    string Expr { get; init; }
    static abstract TSelf Create(string expr);
}