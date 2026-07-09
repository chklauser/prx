
#region Namespace Imports

using System.Diagnostics;
using System.Reflection.Emit;

#endregion

namespace Prexonite.Compiler.Cil;

public class CilSymbol
{
    [DebuggerStepThrough]
    public CilSymbol(SymbolKind kind)
    {
        Kind = kind;
    }

    public SymbolKind Kind { get; set; }

    public LocalBuilder? Local { get; set; }

    public void EmitLoad(CompilerState state)
    {
        if (Local == null)
        {
            throw new PrexoniteException("Internal error: CilSymbol is not bound to a variable.");
        }
        
        switch (Kind)
        {
            case SymbolKind.Local:
                state.EmitLoadLocal(Local.LocalIndex);
                break;
            case SymbolKind.LocalRef:
                state.EmitLoadLocal(Local.LocalIndex);
                state.Il.EmitCall(OpCodes.Call, Compiler.GetValueMethod, null);
                break;
            default:
                throw new PrexoniteException("Internal error: cannot emit load for enumeration variable.");
        }
    }
}

public enum SymbolKind
{
    Local,
    LocalRef,
    LocalEnum,
}