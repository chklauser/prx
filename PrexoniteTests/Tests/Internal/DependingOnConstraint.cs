// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
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
using System.Linq;
using NUnit.Framework.Constraints;
using Prexonite.Compiler.Build;
using Prexonite.Modular;

namespace PrexoniteTests.Tests.Internal
{
    public class DependingOnConstraint : Constraint
    {
        private readonly ModuleName _dependency;

        public DependingOnConstraint(ModuleName dependency) : base(dependency)
        {
            _dependency = dependency;
        }

        public DependingOnConstraint(string name) : base(name)
        {
            if (ModuleName.TryParse(name, out var moduleName))
                _dependency = moduleName;
            else
                throw new ArgumentException($"The string {name} is not a valid module name.");
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            var actualValue = actual;
            return new  ConstraintResult(this, actualValue, _matches(actualValue));
        }

        private bool _matches(object actualValue)
        {
            return actualValue is ITargetDescription desc && desc.Dependencies.Any(n => n.Equals(_dependency));
        }
    }
}