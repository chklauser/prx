using Prexonite.Compiler.Cil;

namespace Prexonite.Commands;

/// <summary>
///     <para>An interface implemented by elements that require special treatment for CIL compilation.</para>
///     <para>CIL extensions have access to compile-time constant arguments and can provide customized CIL code as their implementation.</para>
///     <para>The implementation of this interface does not affect execution under the Prexonite VM.</para>
///     <para>Currently only commands are checked for CIL extensions.</para>
/// </summary>
[SuppressMessage(
    "Microsoft.Naming",
    "CA1704:IdentifiersShouldBeSpelledCorrectly",
    MessageId = "Cil"
)]
public interface ICilExtension
{
    /// <summary>
    ///     Checks whether the static arguments and number of dynamic arguments are valid for the CIL extension.
    ///
    ///     <para>Returning false means that the CIL extension cannot provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler will fall back to  <see
    ///       cref = "ICilCompilerAware" /> and finally the built-in mechanisms.</para>
    ///     <para>Returning true means that the CIL extension can provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler may subsequently call <see
    ///      cref = "Implement" /> with the same set of arguments.</para>
    /// </summary>
    /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
    /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
    /// <returns>true if the extension can provide a CIL implementation for the set of arguments; false otherwise</returns>
    bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc);

    /// <summary>
    ///     Implements the CIL extension in CIL for the supplied arguments. The CIL compiler guarantees to always first call <see
    ///      cref = "ValidateArguments" /> in order to establish whether the extension can actually implement a particular call.
    ///     Thus, this method does not have to verify <paramref name = "staticArgv" /> and <paramref name = "dynamicArgc" />.
    /// </summary>
    /// <param name = "state">The CIL compiler state. This object is used to emit instructions.</param>
    /// <param name = "ins">The instruction that "calls" the CIL extension. Usually a command call.</param>
    /// <param name = "staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
    /// <param name = "dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
    void Implement(
        CompilerState state,
        Instruction ins,
        CompileTimeValue[] staticArgv,
        int dynamicArgc
    );
}
