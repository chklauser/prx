using System;
using System.Collections.Generic;
using System.Text;
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

        private readonly SymbolTable<PValue> _state;

        public Continuation(FunctionContext fctx)
            : base(fctx.Implementation, _getSharedVariables(fctx))
        {
            _entryOffset = fctx.Pointer; //Pointer must already be incremented
            _state = new SymbolTable<PValue>(fctx.LocalVariables.Count);
            foreach (KeyValuePair<string, PVariable> variable in fctx.LocalVariables)
                _state[variable.Key] = variable.Value.Value;
        }

        private static PVariable[] _getSharedVariables(FunctionContext fctx)
        {
            MetaEntry[] sharedNames = fctx.Implementation.Meta[PFunction.SharedNamesKey].List;
            PVariable[] sharedVariables = new PVariable[sharedNames.Length];
            for (int i = 0; i < sharedNames.Length; i++)
            {
                string name = sharedNames[i].Text;
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

            FunctionContext fctx = CreateFunctionContext(sctx, args);

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

            FunctionContext fctx = base.CreateFunctionContext(sctx, args);

            //restore state
            foreach (KeyValuePair<string, PValue> variable in _state)
                fctx.LocalVariables[variable.Key].Value = variable.Value;

            //insert the value returned by the called function
            fctx.Push(returnValue);

            return fctx;
        } 
 
    }
}
