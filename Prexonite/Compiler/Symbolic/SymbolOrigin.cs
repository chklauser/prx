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
    [DebuggerDisplay("SymbolOrigin({Description},{File},{Line},{Column})")]
    public abstract class SymbolOrigin
    {
        public abstract string Description { get; }

        public abstract ISourcePosition Position { get; }
        public override string ToString()
        {
            return String.Format("{0} in {1}", Description, Position);
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
                {
                    var mergedScope = x as MergedScope;
                    return mergedScope != null ? mergedScope._origins : x.Singleton();
                }).ToArray());
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
                    return String.Format("merged scope of {0}", _origins.Select(x => x.Description).ToEnumerationString());
                }
            }

            public override ISourcePosition Position
            {
                get { return _origins[0].Position; }
            }
        }

        public sealed class NamespaceImport : SymbolOrigin
        {
            private readonly QualifiedId _namespaceId;

            [NotNull]
            private readonly ISourcePosition _position;

            public NamespaceImport(QualifiedId namespaceId, [NotNull] ISourcePosition position)
            {
                if (position == null) 
                    throw new ArgumentNullException(nameof(position));
                _namespaceId = namespaceId;
                _position = position;
            }

            public override string Description
            {
                get { return String.Format("import out of namespace {0}",NamespaceId); }
            }

            public override ISourcePosition Position
            {
                get { return _position; }
            }

            public QualifiedId NamespaceId
            {
                get { return _namespaceId; }
            }

            private bool _equals(NamespaceImport other)
            {
                return _namespaceId.Equals(other._namespaceId);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is NamespaceImport && _equals((NamespaceImport)obj);
            }

            public override int GetHashCode()
            {
                return _namespaceId.GetHashCode();
            }
        }

        public sealed class ModuleTopLevel : SymbolOrigin
        {
            [NotNull]
            private readonly ModuleName _moduleName;

            [NotNull]
            private readonly ISourcePosition _position;

            [NotNull]
            private readonly string _description;

            [DebuggerStepThrough]
            public ModuleTopLevel([NotNull] ModuleName moduleName, [NotNull] ISourcePosition position)
            {
                if (moduleName == null)
                    throw new ArgumentNullException(nameof(moduleName));
                if (position == null)
                    throw new ArgumentNullException(nameof(position));
                
                _moduleName = moduleName;
                _position = position;
                _description = string.Format("top-level declaration in module {0}", moduleName);
            }

            public ModuleName ModuleName
            {
                [DebuggerStepThrough]
                get { return _moduleName; }
            }

            public override ISourcePosition Position
            {
                get { return _position; }
            }

            public override string Description
            {
                [DebuggerStepThrough]
                get { return _description; }
            }

            public override string ToString()
            {
                return Description;
            }

            private bool _equals(ModuleTopLevel other)
            {
                return Equals(_moduleName, other._moduleName);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is ModuleTopLevel && _equals((ModuleTopLevel) obj);
            }

            public override int GetHashCode()
            {
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                return (_moduleName != null ? _moduleName.GetHashCode() : 0);
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            }
        }

        public sealed class NamespaceDeclarationScope : SymbolOrigin
        {
            [NotNull]
            private readonly ISourcePosition _position;
            private readonly QualifiedId _namespacePath;

            public QualifiedId NamespacePath
            {
                get { return _namespacePath; }
            }

            public NamespaceDeclarationScope([NotNull] ISourcePosition position, QualifiedId namespacePath)
            {
                if (position == null) throw new ArgumentNullException(nameof(position));
                _position = position;
                _namespacePath = namespacePath;
            }

            public override string Description
            {
                get { return string.Format("private declaration in namespace {0}.",_namespacePath); }
            }

            public override ISourcePosition Position
            {
                get { return _position; }
            }
        }
    }
}