// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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