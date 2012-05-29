using System.Diagnostics;
using Prexonite.Modular;

namespace Prexonite.Compiler.Symbolic
{
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
                _description = string.Format("Top-level declaration from module {0}.", moduleName);
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
        }
    }
}