// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    public static class BuildExtensions
    {
        public static ITarget Build(this IPlan plan, ModuleName name)
        {
            return plan.BuildAsync(name).Result;
        }

        public static Task<ITarget> BuildAsync(this IPlan plan, ModuleName name)
        {
            return BuildAsync(plan, name, CancellationToken.None);
        }

        public static Task<ITarget> BuildAsync(this IPlan plan, ModuleName name, CancellationToken token)
        {
            var buildTasks = plan.BuildAsync(name.Singleton(), token);
            Debug.Assert(buildTasks.Count == 1,"Expected build task dictionary for a single module to only contain a single task.");
            return buildTasks[name];
        }

        public static Tuple<Application,ITarget> Load(this IPlan plan, ModuleName name)
        {
            var loadTask = plan.LoadAsync(name, CancellationToken.None);
            return loadTask.Result;
        }

        public static Application LoadApplication(this IPlan plan, ModuleName name)
        {
            var desc = plan.TargetDescriptions[name];
            var t = plan.LoadAsync(name, CancellationToken.None).Result;
            t.Item2.ThrowIfFailed(desc);
            return t.Item1;
        }

        public static Task<Application> LoadAsync(this IPlan plan, ModuleName name)
        {
            return plan.LoadAsync(name, CancellationToken.None).ContinueWith(tt =>
                {
                    var result = tt.Result;
                    result.Item2.ThrowIfFailed(plan.TargetDescriptions[name]);
                    return result.Item1;
                });
        }
    }
}
