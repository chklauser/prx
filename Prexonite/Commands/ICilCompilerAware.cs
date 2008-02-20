using System;
using System.Collections.Generic;
using System.Text;

namespace Prexonite.Commands
{
    public interface ICilCompilerAware
    {
        CompilationFlags CheckQualification(Instruction ins);

        void ImplementInCil(Compiler.Cil.CompilerState state, Instruction ins);
    }
}
