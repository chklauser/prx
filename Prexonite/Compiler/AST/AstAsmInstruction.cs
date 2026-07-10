namespace Prexonite.Compiler.Ast;

//[System.Diagnostics.DebuggerNonUserCode()]
public class AstAsmInstruction : AstNode
{
    public Instruction Instruction;

    public AstAsmInstruction(string file, int line, int column, Instruction instruction)
        : base(file, line, column)
    {
        Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
    }

    internal AstAsmInstruction(Parser p, Instruction instruction)
        : this(p.scanner.File, p.t.line, p.t.col, instruction) { }

    protected override void DoEmitCode(CompilerTarget target, StackSemantics stackSemantics)
    {
        //Jumps need special treatment for label resolution

        if (Instruction.Arguments == -1)
        {
            switch (Instruction.OpCode)
            {
                case OpCode.jump:
                    target.EmitJump(Position, jumpTargetLabel);
                    break;
                case OpCode.jump_t:
                    target.EmitJumpIfTrue(Position, jumpTargetLabel);
                    break;
                case OpCode.jump_f:
                    target.EmitJumpIfFalse(Position, jumpTargetLabel);
                    break;
                case OpCode.leave:
                    target.EmitLeave(Position, jumpTargetLabel);
                    break;
                default:
                    goto emitNormally;
            }
        }
        else
            goto emitNormally;

        return;
        emitNormally:
        target.Emit(Position, Instruction);
    }

    string jumpTargetLabel =>
        Instruction is { Id: { } id }
            ? id
            : throw new PrexoniteException("Invalid instruction. Missing jump target identifier.");

    public override string ToString()
    {
        return Instruction.ToString();
    }
}
