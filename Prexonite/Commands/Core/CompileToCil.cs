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

        public static bool AlreadyCompiledStatically
        {
            get { return _alreadyCompiledStatically; }
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

        private static bool _alreadyCompiledStatically = false;


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
                args = new PValue[] {};

            FunctionLinking linking = FunctionLinking.FullyStatic;
            switch(args.Length)
            {
                case 0:
                    if(args.Length == 0)
                    {
                        if (AlreadyCompiledStatically)
                            throw new PrexoniteException(
                                string.Format("You should only use static compilation once per process. Use {0}(true)" + 
                                " to force recompilation (warning: memory leak!). Should your program recompile dynamically, " + 
                                "use {1}(false) for disposable implementations.", Engine.CompileToCilAlias, Engine.CompileToCilAlias));
                        else
                            _alreadyCompiledStatically = true;
                    }
                    Compiler.Cil.Compiler.Compile(sctx.ParentApplication, sctx.ParentEngine,linking);
                    break;
                case 1:
                    PValue arg0 = args[0];

                    if (arg0 == null || arg0.IsNull)
                        goto case 0;
                    if (arg0.Type == PType.Bool)
                    {
                        if (!(bool)arg0.Value)
                            linking = FunctionLinking.FullyIsolated;
                        else
                            linking = FunctionLinking.FullyStatic;
                        goto case 0;
                    }
                    else if (arg0.Type == typeof(FunctionLinking))
                    {
                        linking = (FunctionLinking)arg0.Value;
                        goto case 0;
                    }
                    else
                    {
                        goto default;
                    }
                default:
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

                        Compiler.Cil.Compiler.TryCompile(func, sctx.ParentEngine,FunctionLinking.FullyIsolated);
                    }
                    break;
            }

            return PType.Null;
        }

    }
}