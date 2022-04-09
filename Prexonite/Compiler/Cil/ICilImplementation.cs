#nullable enable
using System.Reflection;

namespace Prexonite.Compiler.Cil;

public interface ICilImplementation
{
    MethodInfo Declaration { get; }
    CilFunction Implementation { get; }
}