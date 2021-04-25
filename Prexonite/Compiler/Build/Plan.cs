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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public static class Plan
    {
        public static readonly TraceSource Trace = new("Prexonite.Compiler.Build");

        /// <summary>List of modules included in the Prexonite assembly. Does not include 'sys' itself.</summary>
        private static readonly ISource[] _stdLibModules =
        {
            Source.FromEmbeddedPrexoniteResource("prxlib.prx.prim.pxs"),
            Source.FromEmbeddedPrexoniteResource("prxlib.prx.core.pxs"),
            Source.FromEmbeddedPrexoniteResource("prxlib.sys.pxs"),
            Source.FromEmbeddedPrexoniteResource("prxlib.prx.v1.pxs"),
            Source.FromEmbeddedPrexoniteResource("prxlib.prx.v1.prelude.pxs")
        };

        public static IPlan CreateDefault()
        {
            return new IncrementalPlan();
        }

        public static ISelfAssemblingPlan CreateSelfAssembling()
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return CreateSelfAssembling(StandardLibraryPreference.Default);
        }

        public static async Task<ISelfAssemblingPlan> CreateSelfAssemblingAsync(StandardLibraryPreference stdPreference,
            CancellationToken token)
        {
            var plan = new SelfAssemblingPlan();
            if (stdPreference == StandardLibraryPreference.Default)
            {
                // Provide modules that the standard library may be composed of. No dependency checking.
                await Task.WhenAll(_stdLibModules.Select(s => plan.RegisterModule(s, token)));

                // Describe standard library. Dependencies must be satisfied
                plan.StandardLibrary.Add(new ModuleName("sys", new Version(0, 0)));
            }
            return plan;
        }

        public static ISelfAssemblingPlan CreateSelfAssembling(StandardLibraryPreference stdPreference)
        {
            return CreateSelfAssemblingAsync(stdPreference, CancellationToken.None).Result;
        }
    }

    public enum StandardLibraryPreference
    {
        Default = 0,
        None
    }
}