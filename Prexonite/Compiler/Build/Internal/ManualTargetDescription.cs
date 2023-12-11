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
using Prexonite.Modular;

namespace Prexonite.Compiler.Build.Internal;

[DebuggerDisplay("ManualTargetDescription({Name} from {_fileName})")]
class ManualTargetDescription : ITargetDescription
{
    readonly ISource _source;
    /// <summary>
    /// The file name for symbols derived from the supplied reader. Can be null.
    /// </summary>
    readonly string? _fileName;
    readonly DependencySet _dependencies;

    readonly List<Message>? _buildMessages;

    internal ManualTargetDescription(ModuleName moduleName, ISource source, string? fileName, IEnumerable<ModuleName> dependencies, IEnumerable<Message>? buildMessages = null)
    {
        if (moduleName == null)
            throw new ArgumentNullException(nameof(moduleName));
        if (dependencies == null)
            throw new ArgumentNullException(nameof(dependencies));
        Name = moduleName;
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _fileName = fileName;
        _dependencies = new(moduleName);
        _dependencies.AddRange(dependencies);
        if (buildMessages != null)
            _buildMessages = new(buildMessages);
    }

    public IReadOnlyCollection<ModuleName> Dependencies => _dependencies;

    public ModuleName Name { get; }

    public IReadOnlyList<Message> BuildMessages => (IReadOnlyList<Message>?)_buildMessages ?? DefaultModuleTarget.NoMessages;

    public Task<ITarget> BuildAsync(IBuildEnvironment build, IDictionary<ModuleName, Task<ITarget>> dependencies, CancellationToken token)
    {
        return Task.Factory.StartNew(
            () =>
            {
                var ldr = build.CreateLoader(new(null, null)
                {
                    EnforceDeterministicCodeOrder = true,
                });

                var aggregateMessages = dependencies.Values
                    .SelectMany(t => t.Result.Messages);
                var aggregateExceptions = dependencies.Values
                    .Select(t => t.Result.Exception)
                    .OfType<Exception>();
                if (dependencies.Values.All(t => t.Result.IsSuccessful))
                {
                    try
                    {
                        if (!_source.TryOpen(out var reader))
                            throw new BuildFailureException(this,
                                $"The source for target {Name} could not be opened.",
                                Enumerable.Empty<Message>());
                        using (reader)
                        {
                            token.ThrowIfCancellationRequested();
                            Plan.Trace.TraceEvent(TraceEventType.Information, 0, "Building {0}.", this);
                            // Hand compilation off to loader. 
                            // If the description is backed by a file, allow inclusion of 
                            // other files via relative paths.
                            FileInfo? fsContext;
                            if (_fileName != null && (fsContext = new(_fileName)) is {DirectoryName: not null})
                            {
                                ldr.LoadPaths.Push(fsContext.DirectoryName);
                            }
                            else
                            {
                                fsContext = null;
                            }
                            ldr.LoadFromReader(reader, _fileName);
                            if (ldr.ErrorCount > 0 || ldr.Warnings.Count > 0)
                            {
                                Plan.Trace.TraceEvent(TraceEventType.Error, 0, "Build of {0} completed with {1} error(s) and {2} warning(s).", 
                                    this, ldr.ErrorCount, ldr.Warnings.Count);
                            }
                            foreach (var msg in ldr.Infos.Append(ldr.Warnings).Append(ldr.Errors).OrderBy(m => m))
                            {
                                var evType = msg.Severity switch
                                {
                                    MessageSeverity.Error => TraceEventType.Error,
                                    MessageSeverity.Warning => TraceEventType.Warning,
                                    MessageSeverity.Info => TraceEventType.Information,
                                    _ => throw new ArgumentOutOfRangeException(),
                                };
                                Plan.Trace.TraceEvent(evType, 0, "({0}) {1}", this, msg);
                            }

                            if (fsContext != null)
                            {
                                ldr.LoadPaths.Pop();
                            }

                            Plan.Trace.TraceEvent(TraceEventType.Verbose, 0, "Done with building {0}, wrapping result in target.", this);
                        }

                        // ReSharper disable PossibleMultipleEnumeration
                        return DefaultModuleTarget._FromLoader(ldr, aggregateExceptions.ToArray(), aggregateMessages);
                    }
                    catch (Exception e)
                    {
                        Plan.Trace.TraceEvent(TraceEventType.Error, 0, "Exception while building {0}, constructing failure result. Exception: {1}", this, e);
                        return DefaultModuleTarget._FromLoader(ldr, Extensions.Append(aggregateExceptions, e).ToArray(),
                            aggregateMessages);
                        // ReSharper restore PossibleMultipleEnumeration
                    }
                }
                else
                {
                    Plan.Trace.TraceEvent(TraceEventType.Error, 0,
                        "Not all dependencies of {0} were built successfully. Waiting for other dependencies to finish and then return a failed target. Failed dependencies: {1}",
                        this, dependencies.Where(d => !d.Value.Result.IsSuccessful).Select(d => d.Key).ToEnumerationString());
                    Task.WaitAll(dependencies.Values.ToArray<Task>());
                    return DefaultModuleTarget._FromLoader(ldr, aggregateExceptions.ToArray(), aggregateMessages);
                }
            }, token);
    }

    public override string ToString()
    {
        return $"{{{Name} located in {_fileName}}}";
    }
}