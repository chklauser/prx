using System;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prexonite.Commands.Core
{
    public class CompileToCil : PCommand, ICilCompilerAware
    {
        #region Singleton

        private static readonly CompileToCil _instance = new CompileToCil();

        private CompileToCil()
        {
        }

        public static CompileToCil Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        public override bool IsPure
        {
            get
            {
                return false;
            }
        }

        #region ICilCompilerAware Members

        public CompilationFlags CheckQualification(Instruction ins)
        {
            return CompilationFlags.PreferRunStatically;
        }

        public void ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            return RunStatically(sctx, args);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="sctx">The stack context in which to execut the command.</param>
        /// <param name="args">The arguments to be passed to the command.</param>
        /// <returns>The value returned by the command. Must not be null. (But possibly {null~Null})</returns>
        /// <remarks>
        ///     <para>
        ///         This variation is independant of the executing engine and can take advantage from static binding in CIL compilation.
        ///     </para>
        /// </remarks>
        public static PValue RunStatically(StackContext sctx, PValue[] args)
        {
            if(sctx == null)
                throw new ArgumentNullException("sctx");
            if(args == null)
                args = new PValue[0];

            if(args.Length == 0)
            {
                Compiler.Cil.Compiler.Compile(sctx.ParentApplication, sctx.ParentEngine);
            }
            else
            {
                //Compile individual functions to CIL
                foreach(PValue arg in args)
                {
                    PType T = arg.Type;
                    PFunction func;
                    switch(T.ToBuiltIn())
                    {
                        case PType.BuiltIn.String:
                            if(!sctx.ParentApplication.Functions.TryGetValue((string) arg.Value, out func))
                                continue;
                            break;
                        case PType.BuiltIn.Object:
                            func = arg.Value as PFunction;
                            if(func == null)
                                goto default;
                            else
                                break;
                        default:
                            if(!arg.TryConvertTo(sctx, out func))
                                continue;
                            break;
                    }

                    Compiler.Cil.Compiler.TryCompile(func, sctx.ParentEngine);
                }
            }

            return PType.Null;
        }
    }
}