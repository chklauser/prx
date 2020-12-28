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
using JetBrains.Annotations;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("SymbolOrigin({Description},{Position.File},{Position.Line},{Position.Column})")]
    public abstract class SymbolOrigin
    {
        public abstract string Description { get; }

        public abstract ISourcePosition Position { get; }
        public override string ToString()
        {
            return $"{Description} in {Position}";
        }

        public sealed class MergedScope : SymbolOrigin
        {
            public static SymbolOrigin CreateMerged(params SymbolOrigin[] origins)
            {
                if (origins == null)
                    throw new ArgumentNullException(nameof(origins));
                if (origins.Length == 0)
                {
                    throw new ArgumentException("Must have at least one symbol origin.");
                }

                // Flatten merged scopes
                return new MergedScope(origins.SelectMany(x => 
                    x is MergedScope mergedScope ? mergedScope._origins : x.Singleton()).ToArray());
            }

            [NotNull]
            private readonly SymbolOrigin[] _origins;

            private MergedScope([NotNull] SymbolOrigin[] origins)
            {
                if (origins == null)
                    throw new ArgumentNullException(nameof(origins));
                
                if(origins.Length < 2)
                    throw new ArgumentException("Merged scope origin must be composed of at least two origins.");
                _origins = origins;
            }

            public override string Description
            {
                get
                {
                    return $"merged scope of {_origins.Select(x => x.Description).ToEnumerationString()}";
                }
            }

            public override ISourcePosition Position => _origins[0].Position;
        }

        public sealed class NamespaceImport : SymbolOrigin
        {
            private readonly QualifiedId _namespaceId;

            public NamespaceImport(QualifiedId namespaceId, [NotNull] ISourcePosition position)
            {
                _namespaceId = namespaceId;
                Position = position ?? throw new ArgumentNullException(nameof(position));
            }

            public override string Description => $"import from namespace {NamespaceId}";

            [NotNull]
            public override ISourcePosition Position { get; }

            public QualifiedId NamespaceId => _namespaceId;

            private bool _equals(NamespaceImport other)
            {
                return _namespaceId.Equals(other._namespaceId);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is NamespaceImport otherImport && _equals(otherImport);
            }

            public override int GetHashCode()
            {
                return _namespaceId.GetHashCode();
            }
        }

        public sealed class ModuleTopLevel : SymbolOrigin
        {
            [DebuggerStepThrough]
            public ModuleTopLevel([NotNull] ModuleName moduleName, [NotNull] ISourcePosition position)
            {
                if (moduleName == null)
                    throw new ArgumentNullException(nameof(moduleName));

                ModuleName = moduleName;
                Position = position ?? throw new ArgumentNullException(nameof(position));
                Description = $"top-level declaration in module {moduleName}";
            }

            [NotNull]
            public ModuleName ModuleName { [DebuggerStepThrough] get; }

            [NotNull]
            public override ISourcePosition Position { get; }

            [NotNull]
            public override string Description { [DebuggerStepThrough] get; }

            public override string ToString()
            {
                return Description;
            }

            private bool _equals(ModuleTopLevel other)
            {
                return Equals(ModuleName, other.ModuleName);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is ModuleTopLevel otherTopLevel && _equals(otherTopLevel);
            }

            public override int GetHashCode()
            {
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                return (ModuleName != null ? ModuleName.GetHashCode() : 0);
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            }
        }

        public sealed class NamespaceDeclarationScope : SymbolOrigin
        {
            public QualifiedId NamespacePath { get; }

            public NamespaceDeclarationScope([NotNull] ISourcePosition position, QualifiedId namespacePath)
            {
                Position = position ?? throw new ArgumentNullException(nameof(position));
                NamespacePath = namespacePath;
            }

            public override string Description => $"private declaration in namespace {NamespacePath}.";

            [NotNull]
            public override ISourcePosition Position { get; }
        }
    }
}