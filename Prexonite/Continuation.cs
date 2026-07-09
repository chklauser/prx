

namespace Prexonite;

public class Continuation : Closure
{
    public int EntryOffset { get; }

    public SymbolTable<PValue> State { get; }

    public PValue[] Stack { get; }

    public Continuation(FunctionContext fctx)
        : base(fctx.Implementation, _getSharedVariables(fctx))
    {
        EntryOffset = fctx.Pointer; //Pointer must already be incremented
        State = new(fctx.LocalVariables.Count);
        foreach (var variable in fctx.LocalVariables)
            State[variable.Key] = variable.Value.Value;
        var stack = new PValue[fctx.StackSize];
        for (var i = 0; i < stack.Length; i++)
            stack[i] = fctx.Pop();
        Stack = stack;
        _populateStack(fctx);
    }

    void _populateStack(FunctionContext fctx)
    {
        for (var i = Stack.Length - 1; i >= 0; i--)
        {
            fctx.Push(Stack[i]);
        }
    }

    static PVariable[] _getSharedVariables(FunctionContext fctx)
    {
        var metaTable = fctx.Implementation.Meta;
        if (!(metaTable.TryGetValue(PFunction.SharedNamesKey, out var entry) && entry.IsList))
        {
            return [];
        }
        var sharedNames = entry.List;
        var sharedVariables = new PVariable[sharedNames.Length];
        for (var i = 0; i < sharedNames.Length; i++)
        {
            var name = sharedNames[i].Text;
            sharedVariables[i] = fctx.LocalVariables[name] ?? throw new PrexoniteException("Continuation references non-existent shared variable '" + name + "'.");
        }
        return sharedVariables;
    }

    public override PValue IndirectCall(StackContext sctx, params ReadOnlySpan<PValue> args)
    {
        if (sctx == null)
            throw new ArgumentNullException(nameof(sctx));

        var fctx = CreateFunctionContext(sctx, args.ToArray());

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
        fctx.Pointer = EntryOffset;

        _populateStack(fctx);

        foreach (var variable in State)
        {
            var v = fctx.LocalVariables[variable.Key];
            if (v == null)
            {
                throw new PrexoniteException("Continuation references non-existent local variable '" + variable.Key + "'.");
            }
            v.Value = variable.Value;
        }

        //insert the value returned by the called function
        fctx.Push(returnValue);

        return fctx;
    }

    public override string ToString()
    {
        return "Continuation(" + Function.Id + ")";
    }
}