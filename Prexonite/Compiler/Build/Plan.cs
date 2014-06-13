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
using System.Diagnostics;
using System.Text;
using System.Threading;
using Prexonite.Compiler.Build.Internal;

namespace Prexonite.Compiler.Build
{
    public static class Plan
    {
        public static readonly TraceSource Trace = new TraceSource("Prexonite.Compiler.Build");

        public static IPlan CreateDefault()
        {
            return new IncrementalPlan();
        }

        public static ISelfAssemblingPlan CreateSelfAssembling()
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return CreateSelfAssembling(StandardLibraryPreference.Default);
        }

        public static ISelfAssemblingPlan CreateSelfAssembling(StandardLibraryPreference stdPreference)
        {
            var plan = new SelfAssemblingPlan();
            if (stdPreference == StandardLibraryPreference.Default)
            {
                var stdlib = plan.AssembleAsync(Source.FromString(Properties.Resources.sys),
                    CancellationToken.None).Result;
                plan.StandardLibrary.Add(stdlib.Name); 
            }
            return plan;
        }
    }

    public enum StandardLibraryPreference
    {
        Default = 0,
        None
    }
}