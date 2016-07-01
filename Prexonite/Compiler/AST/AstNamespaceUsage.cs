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
using JetBrains.Annotations;
using Prexonite.Compiler.Symbolic;
using Prexonite.Properties;
using Prexonite.Types;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// A reference to a namespace. Used temporarily while resolving qualified names.
    /// </summary>
    /// <remarks>
    /// <para>Does not have a meaningful translation into executable code, 
    /// but is available for diagnostic and meta programming purposes</para>
    /// <para>
    /// Naked namespace usages (without a dot following them) can be used as arguments to macros.
    /// Such macros need to be prepared to handle namespace references.
    /// </para>
    /// <para>Trying to generate code for a naked namespace usage always fails.</para>
    /// </remarks>
    public class AstNamespaceUsage : AstGetSetImplBase
    {
        [CanBeNull]
        private QualifiedId? _referencePath;

        [NotNull]
        private readonly Namespace _namespace;

        public AstNamespaceUsage(ISourcePosition position, PCall call, [NotNull] Namespace @namespace) : base(position, call)
        {
            if (@namespace == null)
                throw new ArgumentNullException(nameof(@namespace));
            _namespace = @namespace;
        }

        /// <summary>
        /// Represents the path that was used to access this namespace usage. Might be null.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property optionally assigned by the parser when a namespace is accessed via a qualified path.
        /// It only encompasses a qualified id as it appears at the usage site. 
        /// 
        /// </para>
        /// <para>
        /// The reference path does not include prefixes imported into the current scope. 
        /// Consequently, it needs to be evaluated in the same context/scope that produced it,
        /// otherwise it is meaningless. 
        /// </para>
        /// </remarks>
        [CanBeNull]
        public QualifiedId? ReferencePath
        {
            get { return _referencePath; }
            set
            {
                if(!ReferenceEquals(_referencePath,null))
                    throw new InvalidOperationException("Can only assign namespace usage reference path once.");
                _referencePath = value;
            }
        }

        public Namespace Namespace
        {
            get { return _namespace; }
        }

        protected override void EmitGetCode(CompilerTarget target, StackSemantics stackSemantics)
        {
            target.Loader.ReportMessage(Message.Error(Resources.Parser_ExpectedEntityFoundNamespace, Position, MessageClasses.ExpectedEntityFoundNamespace));
            if(stackSemantics == StackSemantics.Value)
                target.EmitNull(Position);
        }

        protected override void EmitSetCode(CompilerTarget target)
        {
            EmitGetCode(target, StackSemantics.Effect);
        }

        public override AstGetSet GetCopy()
        {
            var copy = new AstNamespaceUsage(Position, Call, Namespace);
            if (ReferencePath != null)
                copy.ReferencePath = ReferencePath;
            return copy;
        }
    }
}