using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
    [DebuggerDisplay("SymbolOrigin({Description},{File},{Line},{Column})")]
    public abstract class SymbolOrigin : ISourcePosition
    {
        public abstract string Description { get; }
        public abstract string File { get; }
        public abstract int Line { get; }
        public abstract int Column { get; }

        public sealed class ModuleTopLevel : SymbolOrigin
        {
            private readonly ModuleName _moduleName;
            private readonly ISourcePosition _position;
            private readonly string _description;

            [DebuggerStepThrough]
            public ModuleTopLevel(ModuleName moduleName, ISourcePosition position)
            {
                _moduleName = moduleName;
                _position = position;
                _description = string.Format("top-level declaration in module {0}.", moduleName);
            }

            public ModuleName ModuleName
            {
                [DebuggerStepThrough]
                get { return _moduleName; }
            }

            public override string File
            {
                [DebuggerStepThrough]
                get { return _position.File; }
            }

            public override int Line
            {
                [DebuggerStepThrough]
                get { return _position.Line; }
            }

            public override int Column
            {
                [DebuggerStepThrough]
                get { return _position.Column; }
            }

            public override string Description
            {
                [DebuggerStepThrough]
                get { return _description; }
            }

            public override string ToString()
            {
                return Description;
            }

            private bool _equals(ModuleTopLevel other)
            {
                return Equals(_moduleName, other._moduleName);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is ModuleTopLevel && _equals((ModuleTopLevel) obj);
            }

            public override int GetHashCode()
            {
                return (_moduleName != null ? _moduleName.GetHashCode() : 0);
            }
        }
    }
}