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

using Prexonite.Compiler;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands.List;

public class CreateEnumerator : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    public static CreateEnumerator Instance { get; } = new();

    CreateEnumerator()
    {
    }

    #endregion

    public const string Alias = Loader.ObjectCreationPrefix + "enumerator";

    #region Overrides of PCommand

    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name = "sctx">The stack context in which to execut the command.</param>
    /// <param name = "args">The arguments to be passed to the command.</param>
    /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    // ReSharper disable MemberCanBePrivate.Global
    public static PValue RunStatically(StackContext sctx, PValue[] args)
        // ReSharper restore MemberCanBePrivate.Global
    {
        if (args.Length < 3)
            throw new PrexoniteException(Alias + " requires three arguments: " + Alias +
                "(fMoveNext, fCurrent, fDispose);");

        return sctx.CreateNativePValue(new EnumeratorProxy(sctx, args[0], args[1], args[2]));
    }

    sealed class EnumeratorProxy(
            StackContext sctx,
            PValue moveNext,
            PValue current,
            PValue dispose
        )
        : PValueEnumerator
    {
        bool _disposed;

        #region Implementation of IDisposable

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                throw new InvalidOperationException("The enumerator has already been disposed.");
            try
            {
                dispose.IndirectCall(sctx, Array.Empty<PValue>());
            }
            finally
            {
                _disposed = true;
            }
        }

        #endregion

        #region Implementation of IEnumerator

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        ///     true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref = "T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        /// <filterpriority>2</filterpriority>
        public override bool MoveNext()
        {
            return Runtime.ExtractBool(moveNext.IndirectCall(sctx, Array.Empty<PValue>()),
                sctx);
        }

        #endregion

        #region Implementation of IEnumerator<out PValue>

        /// <summary>
        ///     Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        ///     The element in the collection at the current position of the enumerator.
        /// </returns>
        public override PValue Current => current.IndirectCall(sctx, Array.Empty<PValue>());

        #endregion
    }

    #endregion

    #region Implementation of ICilCompilerAware

    /// <summary>
    ///     Asses qualification and preferences for a certain instruction.
    /// </summary>
    /// <param name = "ins">The instruction that is about to be compiled.</param>
    /// <returns>A set of <see cref = "CompilationFlags" />.</returns>
    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    /// <summary>
    ///     Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
    /// </summary>
    /// <param name = "state">The compiler state.</param>
    /// <param name = "ins">The instruction to compile.</param>
    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}