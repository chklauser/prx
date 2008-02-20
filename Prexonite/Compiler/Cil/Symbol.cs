using System;
using System.Diagnostics;
using System.Reflection.Emit;
using cop = System.Reflection.Emit.OpCodes;

namespace Prexonite.Compiler.Cil
{
    public class Symbol
    {
        private SymbolKind _kind;

        private LocalBuilder _local;

        [DebuggerStepThrough]
        public Symbol(SymbolKind kind)
        {
            _kind = kind;
        }

        public SymbolKind Kind
        {
            [DebuggerStepThrough]
            get
            {
                return _kind;
            }
            [DebuggerStepThrough]
            set
            {
                _kind = value;
            }
        }

        public LocalBuilder Local
        {
            [DebuggerStepThrough]
            get
            {
                return _local;
            }
            [DebuggerStepThrough]
            set
            {
                _local = value;
            }
        }

        public void EmitLoad(CompilerState state)
        {
            switch(Kind)
            {
                case SymbolKind.Local:
                    state.EmitLoadLocal(Local.LocalIndex);
                    break;
                case SymbolKind.LocalRef:
                    state.EmitLoadLocal(Local.LocalIndex);
                    state.Il.EmitCall(cop.Call, Compiler.GetValueMethod, null);
                    break;
            }
        }
    }

    public enum SymbolKind
    {
        Local,
        LocalRef,
        LocalEnum
    }
}