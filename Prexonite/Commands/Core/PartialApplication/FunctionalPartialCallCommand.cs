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
using System.Reflection;
using System.Reflection.Emit;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core.PartialApplication
{
    public class FunctionalPartialCallCommand : PCommand, ICilExtension
    {
        #region Singleton pattern

        private static readonly FunctionalPartialCallCommand _instance =
            new FunctionalPartialCallCommand();

        public static FunctionalPartialCallCommand Instance
        {
            get { return _instance; }
        }

        private FunctionalPartialCallCommand()
        {
        }

        public const string Alias = @"pa\fun\call";

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args.Length < 1)
                return PType.Null;

            var closed = new PValue[args.Length - 1];
            Array.Copy(args, 1, closed, 0, args.Length - 1);
            return sctx.CreateNativePValue(new FunctionalPartialCall(args[0], closed));
        }

        bool ICilExtension.ValidateArguments(CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            return true;
        }

        private ConstructorInfo _functionPartialCallCtorCache;

        private ConstructorInfo _functionPartialCallCtor
        {
            get
            {
                return _functionPartialCallCtorCache ??
                    (_functionPartialCallCtorCache =
                        typeof (FunctionalPartialCall).GetConstructor(new[]
                            {typeof (PValue), typeof (PValue[])}));
            }
        }

        void ICilExtension.Implement(CompilerState state, Instruction ins,
            CompileTimeValue[] staticArgv, int dynamicArgc)
        {
            //the call subject is not part of argv
            var argc = staticArgv.Length + dynamicArgc - 1;

            if (argc == 0)
            {
                //there is no subject, just load null
                state.EmitLoadNullAsPValue();
                return;
            }

            //We don't actually need static arguments, just emit the corresponding opcodes
            foreach (var compileTimeValue in staticArgv)
                compileTimeValue.EmitLoadAsPValue(state);

            //pack arguments (including static ones) into the argv array, but exclude subject (the first argument)
            state.FillArgv(argc);
            state.ReadArgv(argc);

            //call constructor of FunctionalPartialCall
            state.Il.Emit(OpCodes.Newobj, _functionPartialCallCtor);

            //wrap in PValue
            if (ins.JustEffect)
            {
                state.Il.Emit(OpCodes.Pop);
            }
            else
            {
                state.EmitStoreTemp(0);
                state.EmitLoadLocal(state.SctxLocal);
                state.EmitLoadTemp(0);
                state.EmitVirtualCall(Compiler.Cil.Compiler.CreateNativePValue);
            }
        }
    }
}