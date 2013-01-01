using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prexonite.Compiler;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class CreateSourcePosition : PCommand, ICilCompilerAware
    {
        #region Singleton

        private static readonly CreateSourcePosition _instance = new CreateSourcePosition();

        public static CreateSourcePosition Instance { get { return _instance; } }

        private CreateSourcePosition()
        {
            
        }

        public const string Alias = "create_source_position";

        #endregion

        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if (args.Length == 0)
            {
                return sctx.CreateNativePValue(NoSourcePosition.Instance);
            }

            string file = args[0].CallToString(sctx);
            int? line, column;

            PValue box;
            if (args.Length >= 2 && args[1].TryConvertTo(sctx, IntPType.Instance,true,out box))
            {
                line = (int) box.Value;
            }
            else
            {
                line = null;
            }

            if (args.Length >= 3 && args[2].TryConvertTo(sctx, IntPType.Instance, true, out box))
            {
                column = (int) box.Value;
            }
            else
            {
                column = null;
            }

            return sctx.CreateNativePValue(new SourcePosition(file, line ?? -1, column ?? -1));
        }

        public CompilationFlags CheckQualification(Instruction ins)
        {
           return CompilationFlags.PrefersRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException("The command " + Alias + " does not provide a custom CIL implementation.");
        }
    }
}
