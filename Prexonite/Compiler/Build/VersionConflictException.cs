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
using System;
using Prexonite.Modular;

namespace Prexonite.Compiler.Build
{
    internal sealed class VersionConflictException : Exception
    {
        private readonly ModuleName _existingModule;
        private readonly ModuleName _newModule;
        private readonly ModuleName _offendingModule;

        public VersionConflictException(ModuleName existingModule, ModuleName newModule, ModuleName offendingModule)
        {
            _existingModule = existingModule;
            _newModule = newModule;
            _offendingModule = offendingModule;
            Data["ExistingModule"] = existingModule;
            Data["NewModule"] = newModule;
            Data["OffendingModule"] = offendingModule;
        }

        public ModuleName ExistingModule
        {
            get { return _existingModule; }
        }

        public ModuleName NewModule
        {
            get { return _newModule; }
        }

        public ModuleName OffendingModule
        {
            get { return _offendingModule; }
        }

        public override string Message
        {
            get
            {
                return
                    String.Format(
                        "Version conflict detected in dependencies of module {0}, concerning module {1}. Existing version {2}, new version {3}.",
                        OffendingModule, ExistingModule.Id, ExistingModule.Version, NewModule.Version);
            }
        }
    }
}