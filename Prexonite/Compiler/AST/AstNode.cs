// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.ComponentModel;
using System.Diagnostics;
using Prexonite.Types;
using NoDebug = System.Diagnostics.DebuggerNonUserCodeAttribute;

namespace Prexonite.Compiler.Ast
{
    /// <summary>
    /// Indicates how an operation behaves with respect to the Prexonite evaluation stack.
    /// </summary>
    /// <remarks>WARNING: Do not extend this enumeration. Users assume that 
    /// <see cref="Value"/> and <see cref="Effect"/> are its only two members.</remarks>
    public enum StackSemantics
    {

        /// <summary>
        /// Indicates that the operation pushes a single value onto the
        /// evaluation stack.  May also have side effects.
        /// </summary>
        Value,
        /// <summary>
        /// Indicates that the operation does not modify the 
        /// evaluation stack, but may have side effects.
        /// </summary>
        Effect
    }

    [DebuggerStepThrough]
    public abstract class AstNode : IObject, ISourcePosition
    {
        private readonly ISourcePosition _position;

        protected AstNode(string file, int line, int column) : this(new SourcePosition(file, line,column))
        {
        }

        protected AstNode(ISourcePosition position)
        {
            if (position == null)
                throw new ArgumentNullException("position");
            _position = position;
        }

        internal AstNode(Parser p)
            : this(p.scanner.File, p.t.line, p.t.col)
        {
        }

        public ISourcePosition Position
        {
            get { return _position; }
        }

        public string File
        {
            get { return _position.File; }
        }

        public int Line
        {
            get { return _position.Line; }
        }

        public int Column
        {
            get { return _position.Column; }
        }

        protected abstract void DoEmitCode(CompilerTarget target, StackSemantics semantics);

        public void EmitValueCode(CompilerTarget target)
        {
            EmitCode(target, StackSemantics.Value);
        }

        public void EmitEffectCode(CompilerTarget target)
        {
            EmitCode(target, StackSemantics.Effect);
        }

        public void EmitCode(CompilerTarget target, StackSemantics justEffectCode)
        {
            var partiallyApplicabale = this as IAstPartiallyApplicable;
            var isPartialApplication = partiallyApplicabale != null &&
                partiallyApplicabale.CheckForPlaceholders();

            if (justEffectCode == StackSemantics.Effect)
            {
                if (isPartialApplication)
                {
                    //A partial application does not have an effect.
                }
                else
                {
                    DoEmitCode(target, StackSemantics.Effect);
                }
            }
            else
            {
                if (isPartialApplication)
                {
                    partiallyApplicabale.DoEmitPartialApplicationCode(target);
                }
                else
                {
                    DoEmitCode(target, StackSemantics.Value);
                }
            }
        }

        /// <summary>
        ///     Checks the nodes immediate child nodes for instances of <see cref = "AstPlaceholder" />. Must yield the same result as <see
        ///      cref = "IAstPartiallyApplicable.CheckForPlaceholders" />, if implemented in derived types.
        /// </summary>
        /// <returns>True if this node has placeholders; false otherwise</returns>
        public virtual bool CheckForPlaceholders()
        {
            return false;
        }

        internal static AstExpr _GetOptimizedNode(CompilerTarget target, AstExpr expr)
        {
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target cannot be null.");
            if (expr == null)
                throw new ArgumentNullException(
                    "expr", "Expression to be optimized can not be null.");
            AstExpr opt;
            return expr.TryOptimize(target, out opt) ? opt : expr;
        }

        internal static void _OptimizeNode(CompilerTarget target, ref AstExpr expr)
        {
            if (target == null)
                throw new ArgumentNullException("target", "Compiler target cannot be null.");
            if (expr == null)
                throw new ArgumentNullException(
                    "expr", "Expression to be optimized can not be null.");
            expr = _GetOptimizedNode(target, expr);
        }

        #region Implementation of IObject

        public virtual bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
            out PValue result)
        {
            result = null;

            switch (id.ToUpperInvariant())
            {
                case "GETOPTIMIZEDNODE":
                    CompilerTarget target;
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException(
                            "_GetOptimizedNode(CompilerTarget target) requires target.");
                    var expr = this as AstExpr;
                    if (expr == null)
                        throw new PrexoniteException("The node is not an AstExpr.");

                    result = target.Loader.CreateNativePValue(_GetOptimizedNode(target, expr));
                    break;
                case "EMITEFFECTCODE":
                    if (args.Length < 1 || (target = args[0].Value as CompilerTarget) == null)
                        throw new PrexoniteException(
                            "EmitEffectCode(CompilerTarget target) requires target.");
                    EmitEffectCode(target);
                    result = PType.Null;
                    break;
            }

            return result != null;
        }

        #endregion

        internal static SymbolEntry Resolve(Parser parser, string symbolicId)
        {
            SymbolEntry symbolEntry;
            if (!parser.target.Symbols.TryGetValue(symbolicId, out symbolEntry))
            {
                parser.SemErr(string.Format("No implementation defined for operator `{0}`",
                    symbolEntry));
                return new SymbolEntry(SymbolInterpretations.Command, symbolicId, null);
            }
            else
            {
                return symbolEntry;
            }
        }
    }
}