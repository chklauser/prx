using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text;

using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler.Cil;
using Prexonite.Types;

namespace Prx.Commands.Timer
{
    public static class SharedTimer
    {
        private static readonly Stopwatch _stopwatch = new Stopwatch();

        public static Stopwatch Stopwatch
        {
            get
            {
                return _stopwatch;
            }
        }

        public class StartCommand : PCommand, ICilCompilerAware
        {
            #region Singleton pattern

            private static readonly StartCommand _instance = new StartCommand();

            public static StartCommand Instance
            {
                get
                {
                    return _instance;
                }
            }

            private StartCommand()
            {
            }

            #endregion

            /// <summary>
            /// A flag indicating whether the command acts like a pure function.
            /// </summary>
            /// <remarks>Pure commands can be applied at compile time.</remarks>
            public override bool IsPure
            {
                get
                {
                    return false;
                }
            }

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
            public static PValue RunStatically(StackContext sctx, PValue[] args)
            {
                _stopwatch.Start();
                return PType.Null;
            }

            #region ICilCompilerAware Members

            /// <summary>
            /// Asses qualification and preferences for a certain instruction.
            /// </summary>
            /// <param name="ins">The instruction that is about to be compiled.</param>
            /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
            public CompilationFlags CheckQualification(Instruction ins)
            {
                return CompilationFlags.PreferCustomImplementation;
            }

            /// <summary>
            /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
            /// </summary>
            /// <param name="state">The compiler state.</param>
            /// <param name="ins">The instruction to compile.</param>
            public void ImplementInCil(CompilerState state, Instruction ins)
            {
                state.EmitIgnoreArguments(ins.Arguments);

                state.EmitCall(typeof(SharedTimer).GetProperty("Stopwatch",typeof(Stopwatch)).GetGetMethod());
                state.EmitCall(typeof(Stopwatch).GetMethod("Start",new Type[] {}));
                state.EmitLoadPValueNull();
            }

            #endregion
        }

        public class StopCommand : PCommand, ICilCompilerAware
        {
            #region Singleton pattern

            private static readonly StopCommand _instance = new StopCommand();

            public static StopCommand Instance
            {
                get
                {
                    return _instance;
                }
            }

            private StopCommand()
            {
            }

            #endregion

            /// <summary>
            /// A flag indicating whether the command acts like a pure function.
            /// </summary>
            /// <remarks>Pure commands can be applied at compile time.</remarks>
            public override bool IsPure
            {
                get
                {
                    return false;
                }
            }

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
            public static PValue RunStatically(StackContext sctx, PValue[] args)
            {
                _stopwatch.Stop();
                return PType.Null;
            }

            #region ICilCompilerAware Members

            /// <summary>
            /// Asses qualification and preferences for a certain instruction.
            /// </summary>
            /// <param name="ins">The instruction that is about to be compiled.</param>
            /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
            public CompilationFlags CheckQualification(Instruction ins)
            {
                return CompilationFlags.PreferCustomImplementation;
            }

            /// <summary>
            /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
            /// </summary>
            /// <param name="state">The compiler state.</param>
            /// <param name="ins">The instruction to compile.</param>
            public void ImplementInCil(CompilerState state, Instruction ins)
            {
                state.EmitIgnoreArguments(ins.Arguments);

                state.EmitCall(typeof(SharedTimer).GetProperty("Stopwatch",typeof(Stopwatch)).GetGetMethod());
                state.EmitCall(typeof(Stopwatch).GetMethod("Stop",new Type[] {}));
                state.EmitLoadPValueNull();
            }

            #endregion
        }

        public class ResetCommand : PCommand, ICilCompilerAware
        {
            #region Singleton pattern

            private static readonly ResetCommand _instance = new ResetCommand();

            public static ResetCommand Instance
            {
                get
                {
                    return _instance;
                }
            }

            private ResetCommand()
            {
            }

            #endregion

            /// <summary>
            /// A flag indicating whether the command acts like a pure function.
            /// </summary>
            /// <remarks>Pure commands can be applied at compile time.</remarks>
            public override bool IsPure
            {
                get
                {
                    return false;
                }
            }

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
            public static PValue RunStatically(StackContext sctx, PValue[] args)
            {
                _stopwatch.Reset();
                return PType.Null;
            }

            #region ICilCompilerAware Members

            /// <summary>
            /// Asses qualification and preferences for a certain instruction.
            /// </summary>
            /// <param name="ins">The instruction that is about to be compiled.</param>
            /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
            public CompilationFlags CheckQualification(Instruction ins)
            {
                return CompilationFlags.PreferCustomImplementation;
            }

            /// <summary>
            /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
            /// </summary>
            /// <param name="state">The compiler state.</param>
            /// <param name="ins">The instruction to compile.</param>
            public void ImplementInCil(CompilerState state, Instruction ins)
            {
                state.EmitIgnoreArguments(ins.Arguments);

                state.EmitCall(typeof(SharedTimer).GetProperty("Stopwatch", typeof(Stopwatch)).GetGetMethod());
                state.EmitCall(typeof(Stopwatch).GetMethod("Reset", new Type[] { }));
                state.EmitLoadPValueNull();
            }

            #endregion
        }

        public class ElapsedCommand : PCommand, ICilCompilerAware
        {
            #region Singleton pattern

            private static readonly ElapsedCommand _instance = new ElapsedCommand();

            public static ElapsedCommand Instance
            {
                get
                {
                    return _instance;
                }
            }

            private ElapsedCommand()
            {
            }

            #endregion

            /// <summary>
            /// A flag indicating whether the command acts like a pure function.
            /// </summary>
            /// <remarks>Pure commands can be applied at compile time.</remarks>
            public override bool IsPure
            {
                get
                {
                    return false;
                }
            }

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
            public static PValue RunStatically(StackContext sctx, PValue[] args)
            {
                
                return (double)_stopwatch.ElapsedMilliseconds;
            }

            #region ICilCompilerAware Members

            /// <summary>
            /// Asses qualification and preferences for a certain instruction.
            /// </summary>
            /// <param name="ins">The instruction that is about to be compiled.</param>
            /// <returns>A set of <see cref="CompilationFlags"/>.</returns>
            public CompilationFlags CheckQualification(Instruction ins)
            {
                return CompilationFlags.PreferCustomImplementation;
            }

            /// <summary>
            /// Provides a custom compiler routine for emitting CIL byte code for a specific instruction.
            /// </summary>
            /// <param name="state">The compiler state.</param>
            /// <param name="ins">The instruction to compile.</param>
            public void ImplementInCil(CompilerState state, Instruction ins)
            {
                state.EmitIgnoreArguments(ins.Arguments);

                state.EmitCall(typeof(SharedTimer).GetProperty("Stopwatch", typeof(Stopwatch)).GetGetMethod());
                state.EmitCall(typeof(Stopwatch).GetMethod("ElapsedMilliseconds", new Type[] { }));
                state.Il.Emit(OpCodes.Conv_R8);
                state.EmitWrapReal();
            }

            #endregion
        }
    }
}
