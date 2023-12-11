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

using Prexonite.Compiler.Build.Internal;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build;

public class IncrementalPlan : ManualPlan
{
    readonly TaskMap<ModuleName,ITarget> _taskMap = new();

    protected override TaskMap<ModuleName, ITarget> CreateTaskMap()
    {
        return _taskMap;
    }

    protected override Task<ITarget> BuildTargetAsync(Task<IBuildEnvironment> buildEnvironment, ITargetDescription description, Dictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
    {
        return
            base.BuildTargetAsync(buildEnvironment, description, dependencies, token).ContinueWith(
                t =>
                {
                    var actualTarget = t.Result;
                    if (actualTarget is ProvidedTarget)
                        return actualTarget;
                            
                    var providedTarget = new ProvidedTarget(description, actualTarget);
                    TargetDescriptions.Replace(description, providedTarget);
                    return providedTarget;
                }, token,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
    }
}