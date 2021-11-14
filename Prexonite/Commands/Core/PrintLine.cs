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
using System.IO;

namespace Prexonite.Commands.Core;

/// <summary>
///     Implementation of <c>println</c>
/// </summary>
public class DynamicPrintLine : PCommand
{
    private readonly TextWriter _writer;

    /// <summary>
    ///     Creates a new <c>println</c> command, that prints to the supplied <see cref = "TextWriter" />.
    /// </summary>
    /// <param name = "writer">The TextWriter to write to.</param>
    public DynamicPrintLine(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    ///     Creates a new <c>println</c> command that prints to <see cref = "Console.Out" />.
    /// </summary>
    public DynamicPrintLine()
    {
        _writer = Console.Out;
    }

    /// <summary>
    ///     A flag indicating whether the command acts like a pure function.
    /// </summary>
    /// <remarks>
    ///     Pure commands can be applied at compile time.
    /// </remarks>
    [Obsolete]
    public override bool IsPure => false;

    /// <summary>
    ///     Prints all arguments and appends a NewLine.
    /// </summary>
    /// <param name = "sctx">The context in which to convert the arguments to strings.</param>
    /// <param name = "args">The list of arguments to print.</param>
    /// <returns></returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        var s = Concat.ConcatenateString(sctx, args);

        _writer.WriteLine(s);

        return s;
    }
}