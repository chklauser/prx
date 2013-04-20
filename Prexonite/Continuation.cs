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
using System.Linq;
using Prexonite.Types;

namespace Prexonite
{
    public class Continuation : Closure
    {
        public int EntryOffset
        {
            get { return _entryOffset; }
        }

        private readonly int _entryOffset;

        public SymbolTable<PValue> State
        {
            get { return _state; }
        }

        public PValue[] Stack
        {
            get { return _stack; }
        }

        private readonly PValue[] _stack;

        private readonly SymbolTable<PValue> _state;

        public Continuation(FunctionContext fctx)
            : base(fctx.Implementation, _getSharedVariables(fctx))
        {
            _entryOffset = fctx.Pointer; //Pointer must already be incremented
            _state = new SymbolTable<PValue>(fctx.LocalVariables.Count);
            foreach (var variable in fctx.LocalVariables)
                _state[variable.Key] = variable.Value.Value;
            var stack = new PValue[fctx.StackSize];
            for (var i = 0; i < stack.Length; i++)
                stack[i] = fctx.Pop();
            _stack = stack;
            _populateStack(fctx);
        }

        private void _populateStack(FunctionContext fctx)
        {
            for (var i = _stack.Length - 1; i >= 0; i--)
            {
                fctx.Push(_stack[i]);
            }
        }

        private static PVariable[] _getSharedVariables(FunctionContext fctx)
        {
            var metaTable = fctx.Implementation.Meta;
            MetaEntry entry;
            if (!(metaTable.TryGetValue(PFunction.SharedNamesKey, out entry) && entry.IsList))
            {
                return new PVariable[] {};
            }
            var sharedNames = entry.List;
            var sharedVariables = new PVariable[sharedNames.Length];
            for (var i = 0; i < sharedNames.Length; i++)
            {
                var name = sharedNames[i].Text;
                sharedVariables[i] = fctx.LocalVariables[name];
            }
            return sharedVariables;
        }

        public override PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            var fctx = CreateFunctionContext(sctx, args);

            //run the continuation
            return sctx.ParentEngine.Process(fctx);
        }

        public override FunctionContext CreateFunctionContext(StackContext sctx, PValue[] args)
        {
            PValue returnValue;
            if (args.Length < 1)
                returnValue = PType.Null.CreatePValue();
            else
                returnValue = args[0];

            var fctx = base.CreateFunctionContext(sctx, args);

            //restore state
            fctx.Pointer = _entryOffset;

            _populateStack(fctx);

            foreach (var variable in _state)
                fctx.LocalVariables[variable.Key].Value = variable.Value;

            //insert the value returned by the called function
            fctx.Push(returnValue);

            return fctx;
        }

        public override string ToString()
        {
            return "Continuation(" + Function.Id + ")";
        }
    }
}