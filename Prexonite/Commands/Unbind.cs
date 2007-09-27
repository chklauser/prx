using System;
using System.Collections.Generic;
using System.Text;
using Prexonite;
using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Unbinds a variable from closures using it.
    /// </summary>
    /// <example><code>function main()
    /// {
    ///     var n = 15;
    ///     function f1()
    ///     {
    ///         while(n > 4)
    ///             println(n--);
    ///     }
    ///     
    ///     f1();
    ///     println(n); //"4"
    /// 
    ///     n = 13;
    ///     unbind(->n);
    ///     f1();
    ///     println(n); //"13"
    /// }</code>After the call to unbind, $n does no longer 
    /// refer to the same variable as the closure but 
    /// still represents the same value.</example>
    /// <remarks><para>What unbind does, is to copy the contents of the 
    /// supplied variable to a new memory location and associate 
    /// all references <b>inside the calling function</b> with this 
    /// new memory location. Should a function create two closures before 
    /// calling unbind on a shared variable, those two closures will still 
    /// use the same memory location. Only the references in the calling 
    /// function change.</para>
    /// <para>Note that the value of the variable remains untouched. 
    /// The <see cref="PValue"/> object reference is just copied to 
    /// the new memory location.</para></remarks>
    public class Unbind : PCommand
    {

        /// <summary>
        /// Executes the unbind command on each of the arguments supplied.
        /// </summary>
        /// <param name="sctx">The <see cref="FunctionContext"/> to modify.</param>
        /// <param name="args">A list of local variable names or references.</param>
        /// <returns>Always {~Null}.</returns>
        /// <remarks>Each of the supplied arguments is processed individually.</remarks>
        /// <exception cref="ArgumentNullException">args is null</exception>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                throw new ArgumentNullException("args"); 
            foreach (PValue arg in args)
                Run(sctx, arg);
            return PType.Null.CreatePValue();
        }

        /// <summary>
        /// Executes the unbind command on a <see cref="PValue"/> argument.
        ///  The argument must either be the variable's name as a string or a 
        /// reference to the <see cref="PVariable"/> object.
        /// </summary>
        /// <param name="sctx">The <see cref="FunctionContext"/> to modify.</param>
        /// <param name="arg">A variable reference or name.</param>
        /// <returns>Always {~Null}</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sctx"/> is null</exception>
        /// <exception cref="ArgumentNullException"><paramref name="arg"/> is null</exception>
        /// <exception cref="PrexoniteException"><paramref name="arg"/> contains null</exception>
        /// <exception cref="PrexoniteException"><paramref name="sctx"/> is not a <see cref="FunctionContext"/></exception>
        public PValue Run(StackContext sctx, PValue arg)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx"); 

            if (arg == null)
                throw new ArgumentNullException("arg");

            if (arg.IsNull)
                throw new PrexoniteException("The unbind command cannot process Null.");

            FunctionContext fctx = sctx as FunctionContext;
            if (fctx == null)
                throw new PrexoniteException(
                    "The unbind command can only work on function contexts.");

            string id;

            if(arg.Type is ObjectPType && arg.Value is PVariable)
            {
                id = null;
                //Variable reference
                foreach (KeyValuePair<string, PVariable> pair in fctx.LocalVariables)
                {
                    if (ReferenceEquals(pair.Value, arg.Value))
                    {
                        id = pair.Key;
                        break;
                    }
                }
            }
            else
            {
                id = arg.CallToString(fctx);
            }

            PVariable existing;
            if(id != null && fctx.LocalVariables.TryGetValue(id, out existing))
            {
                PVariable unbound = new PVariable();
                unbound.Value = existing.Value;
                fctx.LocalVariables[id] = unbound;
            }

            return PType.Null.CreatePValue();
        }
    }
}
