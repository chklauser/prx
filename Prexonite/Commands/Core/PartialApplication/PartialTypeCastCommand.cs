// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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
using System.Reflection;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class PartialTypecastCommand : PartialWithPTypeCommandBase<PTypeInfo>
    {
        private static readonly PartialTypecastCommand _instance = new PartialTypecastCommand();

        private PartialTypecastCommand()
        {
        }

        public static PartialTypecastCommand Instance
        {
            get { return _instance; }
        }

        private ConstructorInfo _partialTypeCastCtor;

        protected override IIndirectCall CreatePartialApplication(StackContext sctx, int[] mappings,
            PValue[] closedArguments, PTypeInfo parameter)
        {
            return new PartialTypecast(mappings, closedArguments, parameter.Type);
        }

        protected override ConstructorInfo GetConstructorCtor(PTypeInfo parameter)
        {
            return _partialTypeCastCtor ??
                (_partialTypeCastCtor =
                    typeof (PartialTypecast).GetConstructor(new[]
                        {typeof (int[]), typeof (PValue[]), typeof (PType)}));
        }

        protected override Type GetPartialCallRepresentationType(PTypeInfo parameter)
        {
            return typeof (PartialTypecast);
        }

        protected override string PartialApplicationKind
        {
            get { return "Partial type cast"; }
        }
    }
}