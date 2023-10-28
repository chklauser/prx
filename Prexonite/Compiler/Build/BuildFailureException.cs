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
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Compiler.Build;

public class BuildFailureException : BuildException
{
    public List<Message> Messages { get; } = new();

    static string _makeErrorMessage(IEnumerable<Message> messages, string messageFormat)
    {
        var e = 0;
        foreach (var message in messages)
        {
            if(message.Severity == MessageSeverity.Error)
                e++;
        }
        return string.Format(messageFormat, e, e == 1 ? "error" : "errors", e == 1 ? "was" : "were");
    }

    public BuildFailureException(ITargetDescription target, string messageFormat, IEnumerable<Message> messages) : base(_makeErrorMessage(messages, messageFormat),target)
    {
        Messages.AddRange(messages);
    }

    public BuildFailureException(ITargetDescription target, string messageFormat, IEnumerable<Message> messages, Exception inner)
        : base(_makeErrorMessage(messages, messageFormat), target,inner)
    {
        Messages.AddRange(messages);
    }

    public override string ToString()
    {
        var b = new StringBuilder();
        b.AppendLine(base.ToString());
        b.Append(":: Prexonite messages:");
        foreach (var message in Messages)
        {
            b.AppendLine();
            b.Append(message);
        }
        return b.ToString();
    }
}