using System;
using System.Diagnostics;

namespace Prexonite.Compiler
{
    [DebuggerStepThrough]
    public class AddressChangeHook
    {
        public AddressChangeHook(int instructionIndex, Action<int> reaction)
        {
            if (reaction == null)
                throw new ArgumentNullException("reaction");
            if (instructionIndex < 0)
                throw new ArgumentOutOfRangeException
                    ("instructionIndex", instructionIndex, "The instruction index must be valid (i.e. not negative).");

            React = reaction;
            InstructionIndex = instructionIndex;
        }

        public Action<int> React { get; private set; }

        public int InstructionIndex { get; set; }
    }
}