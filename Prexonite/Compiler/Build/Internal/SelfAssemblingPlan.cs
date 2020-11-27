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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Prexonite.Internal;
using Prexonite.Modular;
using Prexonite.Properties;

#nullable enable

namespace Prexonite.Compiler.Build.Internal
{
    public class SelfAssemblingPlan : IncrementalPlan, ISelfAssemblingPlan
    {
        private static readonly TraceSource _trace = Plan.Trace;

        [NotNull]
        private readonly AdHocTaskCache<string, PreflightResult> _preflightCache = new AdHocTaskCache<string, PreflightResult>();

        [NotNull]
        private readonly AdHocTaskCache<string, ITargetDescription> _targetCreationCache = new AdHocTaskCache<string, ITargetDescription>();

        public IList<string> SearchPaths { get; } = new ThreadSafeList<string>();

        public Encoding Encoding { get; set; }

        /// <summary>
        /// <para>
        ///     Assembles a plan around the supplied <paramref name="source"/> text. Automatically resolves and parses any references against modules
        ///     stored as *.pxs files in the file system using <see cref="SearchPaths"/>. Does not yet trigger compilation, but might take a while
        ///     if the dependency graph is very deep or if there are a lot of candidates lying around.
        /// </para>
        /// <para>
        ///     All dependencies (including transitive ones) must either already be defined in the plan or located somewhere in the file system.
        /// </para>
        /// <para>
        ///     You should usually prefer this method over <see cref="RegisterModule"/>. Whenever this method succeeds, the build plan has enough information 
        ///     to attempt building the resulting description. With <see cref="RegisterModule"/>, you might get a target description with unsatisfied dependencies.
        /// </para>
        /// </summary>
        /// <param name="source">The source text to assemble (parts) of a build plan from.</param>
        /// <param name="token"></param>
        /// <returns>The build target description of the supplied source text.</returns>
        public Task<ITargetDescription> AssembleAsync(ISource source, CancellationToken token)
        {
            return _assembleAsync(source, token, SelfAssemblyMode.RecurseIntoFileSystem);
        }

        [PublicAPI]
        public async Task<ITargetDescription> ResolveAndAssembleAsync(string refSpec, CancellationToken token)
        {
            var resolvedRefSpec = await _resolveRefSpec(_parseRefSpec(new MetaEntry(refSpec)), token, SelfAssemblyMode.RecurseIntoFileSystem);
            if (resolvedRefSpec.ErrorMessage != null)
            {
                return _wrapErrorInTargetDescription(resolvedRefSpec.ErrorMessage, resolvedRefSpec.ModuleName);
            }

            return TargetDescriptions[resolvedRefSpec!.ModuleName];
        }

        /// <summary>
        /// <para>Offers a module in source form to the self-assembling build plan.</para>
        /// <para>Unlike <see cref="AssembleAsync"/>, this method does <em>not</em> search the file system for dependencies. It simply takes note of 
        /// them, expecting the user of the build plan to make sure that all dependencies are met in the end.</para>
        /// </summary>
        /// <param name="source">The source text to read. Must be a module.</param>
        /// <param name="token"></param>
        /// <returns>A description of the supplied module. Its dependencies might not be satisfied at this point.</returns>
        public Task<ITargetDescription> RegisterModule(ISource source, CancellationToken token)
        {
            return _assembleAsync(source, token, SelfAssemblyMode.RegisterOnly);
        }

        private async Task<ITargetDescription> _assembleAsync(ISource source, CancellationToken token, SelfAssemblyMode mode)
        {
            token.ThrowIfCancellationRequested();

            // Technically _orderPreflight would be better, but that only works with a resolved path as the key
            var primaryPreflight = await _performPreflight(new RefSpec {Source = source, ResolvedPath = _getPath(source)}, token);

            if (primaryPreflight.ErrorMessage != null)
            {
                var message = primaryPreflight.ErrorMessage;
                var moduleName = primaryPreflight.ModuleName;
                return _wrapErrorInTargetDescription(message, moduleName);
            }
            else
            {
                token.ThrowIfCancellationRequested();
                return await _performCreateTargetDescription(primaryPreflight, source, token, mode);
            }
        }

        private ITargetDescription _wrapErrorInTargetDescription(string message, ModuleName? moduleName)
        {
            var errorMessage = Message.Error(message, NoSourcePosition.Instance,
                MessageClasses.SelfAssembly);

            if (moduleName != null)
            {
                return CreateDescription(moduleName, Source.FromString(""),
                    NoSourcePosition.MissingFileName,
                    Enumerable.Empty<ModuleName>(),
                    new[] {errorMessage});
            }
            else
            {
                throw new BuildFailureException(null, "There {2} {0} {1} while trying to determine dependencies.",
                    new[] {errorMessage});
            }
        }

        private enum SelfAssemblyMode
        {
            RecurseIntoFileSystem = 0,
            RegisterOnly
        }

        private readonly HashSet<ModuleName> _standardLibrary = new HashSet<ModuleName>();
        public ISet<ModuleName> StandardLibrary => _standardLibrary;


        [NotNull]
        private Task<PreflightResult> _orderPreflight(RefSpec refSpec, CancellationToken token)
        {
            if (refSpec.ResolvedPath == null)
                throw new ArgumentException(Resources.SelfAssemblingPlan_RefSepcMustHaveResolvedPathForPreflightOrder,
                    nameof(refSpec));

            return _preflightCache.GetOrAdd(refSpec.ResolvedPath.FullName,
                async (path, actualToken) =>
                {
                    refSpec.Source = new FileSource(refSpec.ResolvedPath, Encoding);
                    await Task.Yield(); // Need to yield at this point to keep
                    // the critical section of the cache short
                    return await _performPreflight(refSpec, actualToken);
                }, token);
        }

        [NotNull]
        private async Task<PreflightResult> _performPreflight(RefSpec refSpec, CancellationToken token)
        // requires refSpec.Source != null
        // ensures result != null
        {
            if (refSpec.ErrorMessage != null)
                return new PreflightResult { ErrorMessage = refSpec.ErrorMessage };

            token.ThrowIfCancellationRequested();
            _trace.TraceEvent(TraceEventType.Information, 0, "Preflight parsing of {0} requested.", refSpec);

            // Make sure refSpec has a re-usable source (will have to support both preflight and actual compilation)
            var source = refSpec.Source;
            if (source == null)
                throw new ArgumentException(Resources.SelfAssemblingPlan_RefSpecMustHaveSource, nameof(refSpec));
            var reportedPath = _getPath(source);
            if (source.IsSingleUse)
                source = await source.CacheInMemoryAsync();

            token.ThrowIfCancellationRequested();

            // Perform preflight parse
            var eng = _createPreflightEngine();
            var app = new Application();
            var ldr =
                new Loader(new LoaderOptions(eng, app)
                               {
                                   // Important: Have preflight flag set
                                   PreflightModeEnabled = true,
                                   ReconstructSymbols = false,
                                   RegisterCommands = false,
                                   StoreSourceInformation = false,
                               });

            if (!source.TryOpen(out var sourceReader))
            {
                var errorResult = new PreflightResult
                                      {
                                          ErrorMessage =
                                              "Failed to open " + refSpec + " for preflight parsing."
                                      };
                return errorResult;
            }
            ldr.LoadFromReader(sourceReader, refSpec.ResolvedPath?.ToString());

            // Extract preflight information
            if (!ModuleName.TryParse(app.Meta[Module.NameKey], out var theModuleName))
                theModuleName = app.Module.Name;

            var result = new PreflightResult
            {
                ModuleName = theModuleName,
                SuppressStandardLibrary =
                    app.Meta.TryGetValue(Module.NoStandardLibraryKey, out var noStdLibEntry) && noStdLibEntry.Switch,
                Path = reportedPath
            };

            result.References.AddRange(
                app.Meta[Module.ReferencesKey].List.Where(entry => !entry.Equals(new MetaEntry("")))
                    .Select(_parseRefSpec));
            _trace.TraceEvent(TraceEventType.Verbose, 0, "Preflight parsing of {0} finished.", refSpec);
            return result;
        }

        private Engine _createPreflightEngine()
        {
            var compilationEngine = LeaseBuildEngine();
            try
            {
                // We cannot modify a shared engine from the pool, but we can clone one (cloning is far cheaper than
                // instantiating a new one).
                return new Engine(compilationEngine) {ExecutionProhibited = true};
            }
            finally
            {
                ReturnBuildEngine(compilationEngine);
            }
        }

        private static FileInfo? _getPath([NotNull] ISource source)
        {
            return source is FileSource fileSource ? fileSource.File : null;
        }

        [NotNull]
        private static readonly Regex _fileReferencePattern = new Regex(@"^(\.|/|:)");

        private static RefSpec _parseRefSpec(MetaEntry entry)
        {
            string? text = null;
            if (entry.IsText && _fileReferencePattern.IsMatch(text = entry.Text))
            {
                // This is a file path reference specification
                return new RefSpec { RawPath = text };
            }
            else if (ModuleName.TryParse(entry, out var moduleName))
            {
                // This is a module name reference specification
                return new RefSpec { ModuleName = moduleName };
            }
            else
            {
                // This is an invalid reference specification
                return new RefSpec
                           {
                               RawPath = text ?? entry.Text,
                               ErrorMessage = "The reference specification is neither a path nor a module name."
                           };
            }
        }

        [NotNull]
        private async Task<RefSpec> _resolveRefSpec([NotNull] RefSpec refSpec, CancellationToken token, SelfAssemblyMode mode)
        // requires refSpec.ModuleName != null || refSpec.Source != null || refSpec.rawPath != null || refSpec.ResolvedPath != null
        // ensures result == refSpec && (TargetDescriptions.Contains(result) || refSpec.ErrorMessage != null)
        {
            if (!refSpec.IsValid)
                return refSpec;

            var pathCandidateCount = 0;
            IEnumerator<FileInfo>? candidateSequence = null;
            var expectedModuleName = refSpec.ModuleName;
            try
            {
                while (refSpec.ModuleName == null || !TargetDescriptions.Contains(refSpec.ModuleName))
                {
                    candidateSequence ??= _pathCandidates(refSpec).GetEnumerator();

                    if (!candidateSequence.MoveNext())
                    {
                        var msg =
                            $"Failed to find a file that matches the reference specification {refSpec}. " +
                            $"{pathCandidateCount} path(s) searched.";
                        _trace.TraceEvent(TraceEventType.Error, 0, msg);
                        refSpec.ErrorMessage = msg;
                        break;
                    }

                    var candidate = candidateSequence.Current;
                    pathCandidateCount++;

                    refSpec.ResolvedPath = candidate;
                    var result = await _orderPreflight(refSpec, token);

                    if (!result.IsValid)
                    {
                        _trace.TraceEvent(TraceEventType.Verbose, 0,
                            "Rejected {0} as a candidate for {1} because there were errors during preflight: {2}",
                            candidate, refSpec, result.ErrorMessage);
                    }
                    else if (result.ModuleName == null)
                    {
                        _trace.TraceEvent(TraceEventType.Information, 0,
                            "Rejected {0} as a candidate for {1} because its module name could not be inferred.",
                            candidate, refSpec);
                    }
                    else if (expectedModuleName != null
                        && !Engine.StringsAreEqual(result.ModuleName.Id, expectedModuleName.Id))
                    {
                        _trace.TraceEvent(TraceEventType.Warning, 0,
                            "Rejected {0} as a candidate for {1} because the module name in the file ({2}) doesn't match the module name expected by the reference.",
                            candidate, refSpec, result.ModuleName.Id);
                    }
                    else
                    {
                        refSpec.ModuleName = result.ModuleName;
                        _trace.TraceEvent(TraceEventType.Information, 0, "Accepted match {0} after preflight, ordering corresponding description.", result.ModuleName);
                        await _orderTargetDescription(result, candidate, token, mode);
                    }
                }
            }
            finally
            {
                candidateSequence?.Dispose();
            }

            Debug.Assert(!refSpec.IsValid || TargetDescriptions.Contains(refSpec.ModuleName));
            return refSpec;
        }

        private Task<ITargetDescription> _orderTargetDescription(PreflightResult result, FileInfo candidate, CancellationToken token, SelfAssemblyMode mode)
        {
            if (candidate == null)
                throw new ArgumentNullException(nameof(candidate));
            return _targetCreationCache.GetOrAdd(candidate.FullName,
                async (key, actualToken) =>
                {
                    var src = Source.FromFile(candidate, Encoding);
                    await Task.Yield();
                    return await _performCreateTargetDescription(result, src, actualToken, mode);
                }, token);
        }
        
        private async Task<ITargetDescription> _performCreateTargetDescription(PreflightResult result, ISource source, CancellationToken token, SelfAssemblyMode mode)
        {
            Debug.Assert(result.IsValid, "TargetDescription ordered despite the preflight result (or its dependencies) containing errors.", "PreflightResult {0} is not valid.", result.RenderDebugState());

            RefSpec[] refSpecs;
            switch (mode)
            {
                case SelfAssemblyMode.RecurseIntoFileSystem:
                    var refSpecResolveTasks =
                        result.References
                        .Select(r => _resolveRefSpec(r, token, mode))
                        .ToArray();
                    await Task.WhenAll(refSpecResolveTasks);
                    refSpecs = refSpecResolveTasks.Select(t => t.Result).ToArray();
                    break;
                case SelfAssemblyMode.RegisterOnly:
                    refSpecs = result.References.Select(_forbidFileRefSpec).ToArray();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, Resources.SelfAssemblingPlan_performCreateTargetDescription_mode);
            }
            
            var buildMessages = refSpecs.Where(t => !t.IsValid).Select(
                    s =>
                    {
                        Debug.Assert(!s.IsValid);
                        // ReSharper disable PossibleNullReferenceException,AssignNullToNotNullAttribute
                        var refPosition = new SourcePosition(
                            s.ResolvedPath != null ? s.ResolvedPath.ToString() 
                          : result.Path != null    ? result.Path.ToString() 
                          : NoSourcePosition.MissingFileName, 0, 0);
                        return Message.Error(s.ErrorMessage, refPosition, MessageClasses.SelfAssembly);
                        // ReSharper restore PossibleNullReferenceException,AssignNullToNotNullAttribute
                    });

            // Assemble dependencies, including standard library (unless suppressed)
            var deps = refSpecs.Where(r => r.ModuleName != null).Select(r => r.ModuleName!);
            if (!result.SuppressStandardLibrary)
                deps = deps.Append(StandardLibrary);

            var reportedFileName = 
                result.Path != null ? result.Path.ToString() 
                : result.ModuleName != null ? result.ModuleName.Id + ".pxs" 
                : null;

            // Typically, duplicate requests are caught much earlier (based on full file paths)
            // But if the user of this self assembling build plan manually adds descriptions
            // that can also be found on the file system, that conflict can in some situations
            // not be detected until full preflight is done.
            // This GetOrAdd is our last line of defense against that scenario and race conditions
            // around targets in general (e.g., when symbolic links or duplicate files are involved)
            return TargetDescriptions.GetOrAdd(result.ModuleName,
                mn => CreateDescription(mn, source, reportedFileName, deps, buildMessages));
        }

        private RefSpec _forbidFileRefSpec(RefSpec refSpec)
        {
            if (refSpec.ModuleName == null)
                Interlocked.CompareExchange(ref refSpec.ErrorMessage, 
                    Resources.SelfAssemblingPlan__forbidFileRefSpec_notallowed, null);
            return refSpec;
        }

        private IEnumerable<FileInfo> _pathCandidates(RefSpec refSpec)
        {
            var resolvedPath = refSpec.ResolvedPath;
            if (resolvedPath != null)
                yield return resolvedPath;

            var rawPath = refSpec.RawPath;

            // Prefer a path, if one was provided.
            if (rawPath != null)
            {
                if (Path.IsPathRooted(rawPath))
                {
                    // Path is absolute, just try it
                    if (_safelyCreateFileInfo(rawPath) is {} absolutePath)
                    {
                        yield return absolutePath;
                    }
                }
                else
                {
                    // Path is relative. We won't try the process working directory unless
                    //  explicitly instructed to (by adding '.' to the search paths)
                    foreach (var candidate in _combineWithSearchPaths(SearchPaths, rawPath))
                        yield return candidate;
                }
            }

            var moduleName = refSpec.ModuleName;
            if (moduleName != null)
            {

                var splitPrefix = moduleName.Id;
                while (true) // while there are '.' in the module name...
                {
                    // ... try each search path in turn ...
                    foreach (var candidate in _combineWithSearchPaths(SearchPaths, splitPrefix + ".pxs"))
                        yield return candidate;

                    // ... convert the last '.' to a '/' (or platform equivalent) ...
                    var dotIndex = splitPrefix.LastIndexOf('.');
                    // Check if there is a '.' that is not the first (=0) or last character of the module name
                    if (1 <= dotIndex && dotIndex <= splitPrefix.Length - 2)
                    {
                        splitPrefix = Path.Combine(
                            splitPrefix.Substring(0, dotIndex),
                            splitPrefix.Substring(dotIndex + 1));
                    }
                    else
                    {
                        // ... or abort if all '.' have been converted ...
                        break;
                    }
                }
            }
        }

        private static IEnumerable<FileInfo> _combineWithSearchPaths(IEnumerable<string> prefixes, string rawPath)
        {
            return prefixes.SelectMaybe(prefix => _safelyCreateFileInfo(Path.Combine(prefix, rawPath)));
        }

        private static FileInfo? _safelyCreateFileInfo(string path)
        {
            FileInfo? candidate;
            try
            {
                candidate = new FileInfo(path);
                // Note: We DON'T check existence of this candidate here
                // Instead we will order a Preflight of this path, which
                // will neatly cache the information whether the file exists in the first place
            }
            catch (Exception ex)
            {
                candidate = null;
                if (ex is ArgumentException ||
                    ex is UnauthorizedAccessException ||
                    ex is PathTooLongException ||
                    ex is NotSupportedException)
                {
                    _trace.TraceEvent(TraceEventType.Error, 0,
                        "Error while handling file path \"{0}\". Treating file as non-existent instead of reporting exception: {1}",
                        path, ex);
                }
                else
                {
                    throw;
                }
            }
            return candidate;
        }

        internal SelfAssemblingPlan()
        {
            Encoding = Encoding.UTF8;
        }
    }

    internal class PreflightResult
    {
        public volatile ModuleName? ModuleName;

        [NotNull]
        public readonly List<RefSpec> References = new List<RefSpec>();

        public volatile string? ErrorMessage;

        public FileInfo? Path;

        public volatile bool SuppressStandardLibrary;

        public bool IsValid => ErrorMessage == null && References.All(x => x.IsValid);

        internal string RenderDebugState()
        {
            var sb = new StringBuilder();
            _renderDebugState(sb);
            return sb.ToString();
        }

        private void _renderDebugState(StringBuilder builder)
        {
            builder.Append(ModuleName);
            builder.Append('(');
            if (Path != null)
            {
                builder.AppendFormat("path: \"{0}\" ", Path);
            }
            if (ErrorMessage != null)
            {
                builder.AppendFormat("error: \"{0}\" ", ErrorMessage);
            }

            if (References.Count > 0)
            {
                builder.Append("references: ");
                var commaRequired = false;
                foreach (var reference in References)
                {
                    if (commaRequired)
                    {
                        builder.Append(',');
                    }

                    commaRequired = true;
                    reference.Render(builder);
                }
            }
            builder.Append(')');
        }
    }

    internal class RefSpec
    {
        public volatile ModuleName? ModuleName;

        public volatile string? RawPath;

        public volatile FileInfo? ResolvedPath;

        public volatile ISource? Source;

        public volatile string? ErrorMessage;

        public bool IsValid => ErrorMessage == null;

        public override string ToString()
        {
            var sb = new StringBuilder();
            Render(sb);
            return sb.ToString();
        }

        internal void Render(StringBuilder sb)
        {
            if (ModuleName != null)
                sb.Append(ModuleName);
            if (ResolvedPath != null)
                sb.AppendFormat("@{0}", ResolvedPath);
            else if (RawPath != null)
                sb.AppendFormat("~@{0}", RawPath);
            else
                sb.Append("(defined programmatically)");

            if (Source != null)
                sb.Append(" with source");

            if (ErrorMessage != null)
                sb.AppendFormat(" error: {0}", ErrorMessage);
        }
    }
}