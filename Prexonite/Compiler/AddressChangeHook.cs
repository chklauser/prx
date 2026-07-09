

using System.Diagnostics;

namespace Prexonite.Compiler;

[DebuggerStepThrough]
public class AddressChangeHook
{
    public AddressChangeHook(int instructionIndex, Action<int> reaction)
    {
        if (instructionIndex < 0)
            throw new ArgumentOutOfRangeException
            (nameof(instructionIndex), instructionIndex,
                "The instruction index must be valid (i.e. not negative).");

        React = reaction ?? throw new ArgumentNullException(nameof(reaction));
        InstructionIndex = instructionIndex;
    }

    public Action<int> React { get; private set; }

    public int InstructionIndex { get; set; }
}