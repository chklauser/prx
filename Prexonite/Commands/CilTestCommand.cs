using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands
{
    class CilTestCommand
    {

        /* function foo(a) [share c]
         * {
         *    var b;
         *    var d;
         *    return ->d;
         * }
        */
        public static void Test1(StackContext sctx, PValue[] args, PVariable[] sharedVariables, out PValue result)
        {
            PValue a = args[0];
            PValue b = PType.Null.CreatePValue();
            PVariable c = sharedVariables[0];
            PVariable d = new PVariable();
            d.Value = PType.Null;

            PValue[] argv = new PValue[] {a,b};

            a = new PValue(true,PType.Bool);
            b = sctx.CreateNativePValue(sctx.ParentApplication.Functions["funcid"]);

            a.DynamicCall(sctx, null, PCall.Get, "memid");

            result = sctx.CreateNativePValue(d);
        }

        public static void Test2(PValue[] args, PValue b, PValue c, PValue d)//, out PValue result)
        {
            Int32 length = args.Length;
            PValue a = 0 < length ? args[0] : PType.Null.CreatePValue();
        }

        public static void Test3(StackContext sctx, List<PValue> lst)
        {
            foreach(PValue a in lst)
                Compiler.Cil.Runtime.ExtractBool(a, sctx);
        }

    }
}
