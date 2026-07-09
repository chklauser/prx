

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
    public override PValue Run(StackContext sctx, ReadOnlySpan<PValue> args)
    {
        return RunStatically(sctx, args.ToArray());
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
                dispose.IndirectCall(sctx);
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
            return Runtime.ExtractBool(moveNext.IndirectCall(sctx),
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
        public override PValue Current => current.IndirectCall(sctx);

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