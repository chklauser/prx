using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Compiler.Cil;

namespace Prexonite.Commands
{
    /// <summary>
    /// <para>An interface implemented by commands that require special treatment for CIL compilation.</para>
    /// <para>Extension commands have access to compile-time constant arguments and can provide customized CIL code as their implementation.</para>
    /// <para>The implementation of this interface does not affect execution under the Prexonite VM.</para>
    /// </summary>
    public interface IExtensionCommand
    {
        /// <summary>
        /// Checks whether the static arguments and number of dynamic arguments are valid for the extension command. 
        /// 
        /// <para>Returning false means that the extension command cannot provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler will fall back to  <see cref="ICilCompilerAware"/> and finally ordinary command handling.</para>
        /// <para>Returning true means that the extension command can provide a CIL implementation for the set of arguments at hand. In that case the CIL compiler may subsequently call <see cref="Implement"/> with the same set of arguments.</para>
        /// </summary>
        /// <param name="staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name="dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        /// <returns>true if the command can provide a CIL implementation for the set of arguments; false otherwise</returns>
        bool ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc);

        /// <summary>
        /// Implements the extension command in CIL for the supplied arguments. The CIL compiler guarantees to always first call <see cref="ValidateArguments"/> in order to establish whether the command can actually implement a particular call.
        /// Thus, this method does not have to verify <paramref name="staticArgv"/> and <paramref name="dynamicArgc"/>.
        /// </summary>
        /// <param name="state">The CIL compiler state. This object is used to emit instructions.</param>
        /// <param name="staticArgv">The suffix of compile-time constant arguments, starting after the last dynamic (not compile-time constant) argument. An empty array means that there were no compile-time constant arguments at the end.</param>
        /// <param name="dynamicArgc">The number of dynamic arguments preceding the supplied static arguments. The total number of arguments is determined by <code>(staticArgv.Length + dynamicArgc)</code></param>
        void Implement(CompilerState state, CompileTimeValue[] staticArgv, int dynamicArgc);

    }

    /// <summary>
    /// The different interpretations of a compile-time value.
    /// </summary>
    public enum CompileTimeInterpretation
    {
        /// <summary>
        /// A <code>null</code>-literal. Obtained from <code>ldc.null</code>. Null is the default interpretation.
        /// </summary>
        Null = 0,

        /// <summary>
        /// A string literal. Obtained from <code>ldc.string</code>. Represented as <see cref="string"/>.
        /// </summary>
        String,

        /// <summary>
        /// An integer literal. Obtained from <code>ldc.int</code>. Represented as <see cref="int"/>.
        /// </summary>
        Int,

        /// <summary>
        /// A boolean literal. Obtained from <code>ldc.bool</code>. Represented as <see cref="bool"/>.
        /// </summary>
        Bool,

        /// <summary>
        /// A local variable reference literal. Obtained from <code>ldr.loc</code> and <code>ldr.loci</code>. Represented as <see cref="string"/>, the name of the local variable.
        /// </summary>
        LocalVariableReference,

        /// <summary>
        /// A global variable reference literal. Obtained from <code>ldr.glob</code>. Represented as <see cref="string"/>, the name of the global variable.
        /// </summary>
        GlobalVariableReference,

        /// <summary>
        /// A function reference literal. Obtained from <code>ldr.func</code>. Represented as <see cref="string"/>, the name of the function.
        /// </summary>
        FunctionReference,

        /// <summary>
        /// A command reference literal. Obtained from <code>ldr.cmd</code>. Represented as <see cref="string"/>, the name of the command.
        /// </summary>
        CommandReference
    }

    /// <summary>
    /// Represents a compile time value, as read from an instruction.
    /// The default value is a valid representation of <code>null</code>.
    /// </summary>
    public struct CompileTimeValue
    {
        /// <summary>
        /// Indicates how to interpret the compile-time value stored in <see cref="Value"/>.
        /// </summary>
        public CompileTimeInterpretation Interpretation;

        /// <summary>
        /// The compile-time value. Interpret according to <see cref="Interpretation"/>.
        /// </summary>
        public Object Value;
    }
}
