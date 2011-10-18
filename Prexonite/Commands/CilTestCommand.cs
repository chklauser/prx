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

using System.Collections.Generic;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands
{
    internal class CilTestCommand
    {
        /* function foo(a) [share c]
         * {
         *    var b;
         *    var d;
         *    return ->d;
         * }
        */

        public static void Test1(StackContext sctx, PValue[] args, PVariable[] sharedVariables,
            out PValue result)
        {
            var a = args[0];
            var b = PType.Null.CreatePValue();
            var c = sharedVariables[0];
            var d = new PVariable();
            d.Value = PType.Null;

            var argv = new[] {a, b};

            a = new PValue(true, PType.Bool);
            b = sctx.CreateNativePValue(sctx.ParentApplication.Functions["funcid"]);

            a.DynamicCall(sctx, null, PCall.Get, "memid");

            result = sctx.CreateNativePValue(d);
        }

        public static void Test2(PValue[] args, PValue b, PValue c, PValue d) //, out PValue result)
        {
            var length = args.Length;
            var a = 0 < length ? args[0] : PType.Null.CreatePValue();
        }

        public static void Test3(StackContext sctx, List<PValue> lst)
        {
            foreach (var a in lst)
                Runtime.ExtractBool(a, sctx);
        }
    }
}