using System;
using System.Collections.Generic;
using System.Text;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public sealed class Char : PCommand
    {
        private Char()
        {
        }

        private static Char _instance = new Char();

        public static Char Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// A flag indicating whether the command acts like a pure function.
        /// </summary>
        /// <remarks>Pure commands can be applied at compile time.</remarks>
        public override bool IsPure
        {
            get { return false; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (sctx == null)
                throw new ArgumentNullException("sctx");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("Char requires at least one argument.");

            PValue v;
            PValue arg = args[0];
            if(arg.Type == PType.String)
            {
                string s = (string) arg.Value;
                if (s.Length == 0)
                    throw new PrexoniteException("Cannot create char from empty string.");
                else
                    return s[0];
            }
            else if(arg.TryConvertTo(sctx, PType.Char, true, out v))
            {
                return v;
            }
            else if(arg.TryConvertTo(sctx, PType.Int, true, out v))
            {
                return (char) (int) v.Value;
            }
            else
            {
                throw new PrexoniteException("Cannot create char from " + arg);
            }
        }
    }
}
