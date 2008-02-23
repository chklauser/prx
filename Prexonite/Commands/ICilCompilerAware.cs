using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Commands
{

    /// <summary>
    /// Provides a way to communicate incompatibilities and custom implementations to the CIL compiler.
    /// </summary>
    public interface ICilCompilerAware
    {

        /// <summary>
        /// Asses qualification and preferences for a certain instruction.
        /// </summary>
        /// <param name="ins">The instruction that is about to be compiled.</param>
        /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
        CompilationFlags CheckQualification(Instruction ins);

        /// <summary>
        /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
        /// </summary>
        /// <param name="state">The compiler state.</param>
        /// <param name="ins">The instruction to compile.</param>
        void ImplementInCil(Compiler.Cil.CompilerState state, Instruction ins);
    }
}
