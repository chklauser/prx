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
using System.Collections.Concurrent;
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

namespace Prexonite.Compiler.Build.Internal
{
    public class SelfAssemblingPlan : IncrementalPlan, ISelfAssemblingPlan
    {
        private static readonly TraceSource Trace = Plan.Trace;

        private readonly IList<string> _searchPaths = new List<string>();

        [NotNull]
        private readonly AdHocTaskCache<String, PreflightResult> _preflightCache = new AdHocTaskCache<String, PreflightResult>();

        [NotNull]
        private readonly AdHocTaskCache<String, ITargetDescription> _targetCreationCache = new AdHocTaskCache<String, ITargetDescription>();

        private readonly SemaphoreSlim _searchPathsLock = new SemaphoreSlim(1);

        public IList<string> SearchPaths
        {
            get { return _searchPaths; }
        }

        public SemaphoreSlim SearchPathsLock
        {
            get { return _searchPathsLock; }
        }

        public Encoding Encoding { get; set; }

        public async Task<ITargetDescription> AssembleAsync(ISource source, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var primaryPreflight = await _performPreflight(new RefSpec { Source = source }, token);

            if (primaryPreflight.ErrorMessage != null)
            {
                return CreateDescription(primaryPreflight.ModuleName, Source.FromString(""), NoSourcePosition.MissingFileName,
                    Enumerable.Empty<ModuleName>(),
                    new[]
                        {
                            Message.Error(primaryPreflight.ErrorMessage, NoSourcePosition.Instance,
                                MessageClasses.SelfAssembly)
                        });
            }
            else
            {
                token.ThrowIfCancellationRequested();
                return await _performCreateTargetDescription(primaryPreflight, source, token);
            }
        }


        [NotNull]
        private Task<PreflightResult> _orderPreflight(RefSpec refSpec, CancellationToken token)
        {
            if (refSpec.ResolvedPath == null)
                throw new ArgumentException(Resources.SelfAssemblingPlan_RefSepcMustHaveResolvedPathForPreflightOrder,
                    "refSpec");

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
            Trace.TraceEvent(TraceEventType.Information, 0, "Preflight parsing of {0} requested.", refSpec);

            // Make sure refSpec has a re-usable source (will have to support both preflight and actual compilation)
            var source = refSpec.Source;
            if (source == null)
                throw new ArgumentException(Resources.SelfAssemblingPlan_RefSpecMustHaveSource, "refSpec");
            if (source.IsSingleUse)
                source = await source.CacheInMemoryAsync();

            token.ThrowIfCancellationRequested();

            // Perform preflight parse
            var eng = new Engine { ExecutionProhibited = true };
            var app = new Application();
            var ldr =
                new Loader(new LoaderOptions(eng, app)
                               {
                                   // Important: Have preflight flag set
                                   PreflightModeEnabled = false,
                                   ReconstructSymbols = false,
                                   RegisterCommands = false,
                                   StoreSourceInformation = false,
                               });

            TextReader sourceReader;
            if (!source.TryOpen(out sourceReader))
            {
                var errorResult = new PreflightResult
                                      {
                                          ErrorMessage =
                                              "Failed to open " + refSpec + " for preflight parsing."
                                      };
                return errorResult;
            }
            ldr.LoadFromReader(sourceReader, refSpec.ResolvedPath != null ? refSpec.ResolvedPath.ToString() : null);

            // Extract preflight information
            ModuleName theModuleName;
            if (!ModuleName.TryParse(app.Meta[Module.NameKey], out theModuleName))
                theModuleName = app.Module.Name;

            var result = new PreflightResult { ModuleName = theModuleName };
            result.References.AddRange(
                app.Meta[Module.ReferencesKey].List.Where(entry => !entry.Equals(new MetaEntry("")))
                    .Select(_parseRefSpec));
            Trace.TraceEvent(TraceEventType.Verbose, 0, "Preflight parsing of {0} finished.", refSpec);
            return result;
        }

        [NotNull]
        private static readonly Regex _fileReferencePattern = new Regex(@"^(\.|/|:)");

        private RefSpec _parseRefSpec(MetaEntry entry)
        {
            ModuleName moduleName;
            string text = null;
            if (entry.IsText && _fileReferencePattern.IsMatch(text = entry.Text))
            {
                // This is a file path reference specification
                return new RefSpec { RawPath = text };
            }
            else if (ModuleName.TryParse(entry, out moduleName))
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
        private async Task<RefSpec> _resolveRefSpec([NotNull] RefSpec refSpec, CancellationToken token)
        // requires refSpec.ModuleName != null || refSpec.Source != null || refSpec.rawPath != null || refSpec.ResolvedPath != null
        // ensures result == refSpec && (TargetDescriptions.Contains(result) || refSpec.ErrorMessage != null)
        {
            if (refSpec.ErrorMessage != null)
                return refSpec;

            var pathCandidateCount = 0;
            IEnumerator<FileInfo> candidateSequence = null;
            var expectedModuleName = refSpec.ModuleName;
            try
            {
                while (refSpec.ModuleName == null || !TargetDescriptions.Contains(refSpec.ModuleName))
                {
                    if (candidateSequence == null)
                        candidateSequence = _pathCandidates(refSpec,token).GetEnumerator();

                    if (!candidateSequence.MoveNext())
                    {
                        var msg =
                            String.Format(
                                "Failed to find a file that matches the reference specification {0}. {1} path(s) searched.",
                                refSpec, pathCandidateCount);
                        Trace.TraceEvent(TraceEventType.Error, 0, msg);
                        refSpec.ErrorMessage = msg;
                        break;
                    }

                    var candidate = candidateSequence.Current;
                    pathCandidateCount++;

                    refSpec.ResolvedPath = candidate;
                    var result = await _orderPreflight(refSpec, token);

                    if (result.ErrorMessage != null)
                    {
                        Trace.TraceEvent(TraceEventType.Verbose, 0,
                            "Rejected {0} as a candidate for {1} because there were errors during preflight: {2}",
                            candidate, refSpec, result.ErrorMessage);
                    }
                    else if (result.ModuleName == null)
                    {
                        Trace.TraceEvent(TraceEventType.Information, 0,
                            "Rejected {0} as a candidate for {1} because its module name could not be inferred.",
                            candidate, refSpec);
                    }
                    else if (expectedModuleName != null 
                        && !Engine.StringsAreEqual(result.ModuleName.Id, expectedModuleName.Id))
                    {
                        Trace.TraceEvent(TraceEventType.Warning, 0,
                            "Rejected {0} as a candidate for {1} because the module name in the file ({2}) doesn't match the module name expected by the reference.",
                            candidate, refSpec, result.ModuleName.Id);
                    }
                    else
                    {
                        refSpec.ModuleName = result.ModuleName;
                        Trace.TraceEvent(TraceEventType.Information, 0, "Accepted match {0} after preflight, ordering corresponding description.",result.ModuleName);
                        await _orderTargetDescription(result, candidate,token);
                    }
                }
            }
            finally
            {
                if (candidateSequence != null)
                    candidateSequence.Dispose();
            }

            Debug.Assert(refSpec.ErrorMessage != null || TargetDescriptions.Contains(refSpec.ModuleName));
            return refSpec;
        }

        private Task<ITargetDescription> _orderTargetDescription(PreflightResult result, FileInfo candidate, CancellationToken token)
        {
            if (candidate == null)
                throw new ArgumentNullException("candidate");
            return _targetCreationCache.GetOrAdd(candidate.FullName,
                async (key, actualToken) =>
                          {
                              var src = Source.FromFile(candidate, Encoding);
                              await Task.Yield();
                              await _addToSearchPaths(candidate,actualToken);
                              return await _performCreateTargetDescription(result, src, actualToken);
                          }, token);
        }

        private async Task<ITargetDescription> _performCreateTargetDescription(PreflightResult result, ISource source, CancellationToken token)
        {
            Debug.Assert(result.ErrorMessage == null, "TargetDescription ordered despite the preflight result containing errors.");
            Debug.Assert(result.References.All(r => r.ErrorMessage == null), "TargetDescription ordered despite the preflight result containing errors.");

            var refSpecResolveTasks =
                result.References
                .Select(r => _resolveRefSpec(r, token))
                .ToArray();
            await Task.WhenAll(refSpecResolveTasks);
            var refSpecs = refSpecResolveTasks.Select(t => t.Result).ToArray();
            var buildMessages = refSpecs.Where(t => t.ErrorMessage != null).Select(
                    s =>
                    {
                        Debug.Assert(s.ResolvedPath != null);
                        Debug.Assert(s.ErrorMessage != null);
                        // ReSharper disable PossibleNullReferenceException,AssignNullToNotNullAttribute
                        var refPosition = new SourcePosition(s.ResolvedPath.ToString(), 0, 0);
                        return Message.Error(s.ErrorMessage, refPosition, MessageClasses.SelfAssembly);
                        // ReSharper restore PossibleNullReferenceException,AssignNullToNotNullAttribute
                    });
            var deps = refSpecs.Where(r => r.ModuleName != null).Select(r => r.ModuleName);
            
            var reportedFileName = result.Path != null ? result.Path.ToString() : null;
                
            var desc = CreateDescription(result.ModuleName, source,reportedFileName, deps,buildMessages);
            TargetDescriptions.Add(desc);
            return desc;
        }

        private IEnumerable<FileInfo> _pathCandidates(RefSpec refSepc, CancellationToken token)
        {
            var resolvedPath = refSepc.ResolvedPath;
            if (resolvedPath != null)
                yield return resolvedPath;

            var prefixes = _copySearchPaths(token).Result;
            var rawPath = refSepc.RawPath;

            // Prefer a path, if one was provided.
            if (rawPath != null)
            {
                if (Path.IsPathRooted(rawPath))
                {
                    // Path is absolute, just try it
                    yield return _safelyCreateFileInfo(rawPath);
                }
                else
                {
                    // Path is relative. We won't try the process working directory unless
                    //  explicitly instructed to (by adding '.' to the search paths)
                    foreach (var candidate in _combineWithSearchPaths(prefixes, rawPath))
                        yield return candidate;
                }
            }

            var moduleName = refSepc.ModuleName;
            if (moduleName != null)
            {

                var splitPrefix = moduleName.Id;
                while (true) // while there are '.' in the module name...
                {
                    // ... try each search path in turn ...
                    foreach (var candidate in _combineWithSearchPaths(prefixes, splitPrefix + ".pxs"))
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

        private static IEnumerable<FileInfo> _combineWithSearchPaths(string[] prefixes, string rawPath)
        {
            return prefixes.Select(prefix => _safelyCreateFileInfo(Path.Combine(prefix, rawPath)));
        }

        private static FileInfo _safelyCreateFileInfo(string path)
        {
            FileInfo candidate;
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
                    Trace.TraceEvent(TraceEventType.Error, 0,
                        "Error while handling file path \"{0}\". Treating file as non-existent instead of reporting exception: ",
                        path, ex);
                }
                else
                {
                    throw;
                }
            }
            return candidate;
        }

        private async Task<string[]> _copySearchPaths(CancellationToken token)
        {
            string[] prefixes;

            await SearchPathsLock.WaitAsync(token);
            try
            {
                prefixes = SearchPaths.ToArray();
            }
            finally
            {
                SearchPathsLock.Release();
            }
            return prefixes;
        }

        private async Task _addToSearchPaths(FileInfo resolvedPath, CancellationToken token)
        {
            await SearchPathsLock.WaitAsync(token);
            try
            {
                if (!SearchPaths.Contains(resolvedPath.DirectoryName))
                {
                    SearchPaths.Add(resolvedPath.DirectoryName);
                }
            }
            finally
            {
                SearchPathsLock.Release();
            }
        }

        internal SelfAssemblingPlan()
        {
            Encoding = Encoding.UTF8;
        }
    }

    internal class PreflightResult
    {
        [CanBeNull]
        public volatile ModuleName ModuleName;

        [NotNull]
        public readonly List<RefSpec> References = new List<RefSpec>();

        [CanBeNull]
        public volatile string ErrorMessage;

        public FileInfo Path;

        public bool IsValid
        {
            get { return ErrorMessage == null && References.All(x => x.IsValid); }
        }
    }

    internal class RefSpec
    {
        [CanBeNull]
        public volatile ModuleName ModuleName;

        [CanBeNull]
        public volatile string RawPath;

        [CanBeNull]
        public volatile FileInfo ResolvedPath;

        [CanBeNull]
        public volatile ISource Source;

        [CanBeNull]
        public volatile string ErrorMessage;

        public bool IsValid
        {
            get { return ErrorMessage == null; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("reference ");
            if (ModuleName != null)
                sb.Append(ModuleName);
            if (ResolvedPath != null)
                sb.AppendFormat("@{0}", ResolvedPath);
            else if (RawPath != null)
                sb.AppendFormat("~@{0}", RawPath);

            if (Source != null)
                sb.Append(" with source");

            if (ErrorMessage != null)
                sb.AppendFormat(" error: {0}", ErrorMessage);

            return sb.ToString();
        }
    }
}