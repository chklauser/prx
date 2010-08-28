#region Namespace Imports

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Types;
using Prx.Benchmarking;
using Prx.Properties;

#endregion

namespace Prx
{
    internal static class Program
    {
        public const string PrxScriptFileName = "prx.pxs";

        public static string GetPrxPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static void writeFile(byte[] buffer, string path)
        {
            using (
                var fsmain =
                    new FileStream
                        (
                        GetPrxPath() + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + path,
                        FileMode.Create,
                        FileAccess.Write))
                fsmain.Write(buffer, 0, buffer.Length);
        }

        private static void _test(out ReturnMode mode)
        {
            mode = ReturnMode.Exit;
        }

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate { Environment.Exit(1); };
            var prexoniteConsole = new PrexoniteConsole(true);

#if !DEBUG //Let the exceptions surface so they can more easily be debugged
            try
            {
#endif
                //Create an empty application
                var app = new Application("prx");

                var engine = new Engine();
                engine.RegisterAssembly(Assembly.GetExecutingAssembly());

                //Load application
                if(!_loadApplication(engine, prexoniteConsole, app))
                    return;

                //Run the applications main function.
                _runApplication(engine, app, args);
#if !DEBUG
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
#else
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Exiting Prx.Main normally. Press Enter to exit");
                    Console.ReadLine();
                }
#endif
        }

        private static void _runApplication(Engine engine, Application app, string[] args)
        {
            app.Run
                (engine,
                    new[]
                    {
                        engine.CreateNativePValue(args)
                    });
        }

        private static bool _loadApplication(Engine engine, PrexoniteConsole prexoniteConsole, Application app)
        {
            #region Stopwatch commands

            //prx.exe provides these three additional commands for high speed access to a stopwatch from your script code
            var timer = new Stopwatch();
            engine.Commands.AddHostCommand
                (
                    @"timer\start",
                    new DelegatePCommand
                        (
                        delegate
                        {
                            timer.Start();
                            return null;
                        }));

            engine.Commands.AddHostCommand
                (
                    @"timer\stop",
                    new DelegatePCommand
                        (
                        delegate
                        {
                            timer.Stop();
                            return (double) timer.ElapsedMilliseconds;
                        }));

            engine.Commands.AddHostCommand
                (
                    @"timer\reset",
                    new DelegatePCommand
                        (
                        delegate
                        {
                            timer.Reset();
                            return null;
                        }));

            engine.Commands.AddHostCommand
                (
                    @"timer\elapsed",
                    new DelegatePCommand
                        (
                        delegate { return (double) timer.ElapsedMilliseconds; }));

            #endregion

            #region Stack Manipulation commands

            engine.Commands.AddHostCommand
                (
                    @"__replace_call",
                    delegate(StackContext sctx, PValue[] cargs)
                    {
                        if (cargs == null)
                            cargs = new PValue[]
                            {
                            };
                        if (sctx == null)
                            throw new ArgumentNullException("sctx");

                        var e = sctx.ParentEngine;

                        if (cargs.Length < 1)
                            throw new PrexoniteException
                                (
                                "__replace_call requires the context or function to be replaced.");

                        var carg = cargs[0];
                        var rargs = new PValue[cargs.Length - 1];
                        Array.Copy(cargs, 1, rargs, 0, rargs.Length);

                        FunctionContext rctx = null;
                        PFunction f;
                        switch (carg.Type.ToBuiltIn())
                        {
                            case PType.BuiltIn.String:
                                if (
                                    !sctx.ParentApplication.Functions.TryGetValue
                                         (
                                             (string) carg.Value, out f))
                                    throw new PrexoniteException
                                        (
                                        "Cannot replace call to " + carg +
                                        " because no such function exists.");

                                rctx = f.CreateFunctionContext(e, rargs);
                                break;
                            case PType.BuiltIn.Object:
                                var clrType = ((ObjectPType) carg.Type).ClrType;
                                if (clrType == typeof (PFunction))
                                {
                                    f = (PFunction) carg.Value;
                                    rctx = f.CreateFunctionContext(e, rargs);
                                }
                                else if (clrType == typeof (Closure) && clrType != typeof (Continuation))
                                {
                                    var c = (Closure) carg.Value;
                                    rctx = c.CreateFunctionContext(sctx, rargs);
                                }
                                else if (clrType == typeof (FunctionContext))
                                {
                                    rctx = (FunctionContext) carg.Value;
                                }
                                break;
                        }
                        if (rctx == null)
                            throw new PrexoniteException("Cannot replace a context based on " + carg);

                        var node = e.Stack.Last;
                        do
                        {
                            var ectx = node.Value as FunctionContext;

                            if (ectx != null)
                            {
                                if (ReferenceEquals(ectx.Implementation, rctx.Implementation))
                                {
                                    node.Value = rctx;
                                    break;
                                }
                            }
                        } while ((node = node.Previous) != null);

                        return PType.Null.CreatePValue();
                    });

            #endregion

            #region Prexonite Console constant

            engine.Commands.AddHostCommand("__console", prexoniteConsole);

            #endregion

            #region Create benchmark command

            engine.Commands.AddHostCommand
                ("createBenchmark",
                    delegate(StackContext sctx, PValue[] cargs)
                    {
                        if (sctx == null)
                            throw new ArgumentNullException("sctx");
                        if (cargs == null)
                            cargs = new PValue[]
                            {
                            };

                        Engine teng;
                        int tit;

                        if (cargs.Length >= 2)
                        {
                            teng = cargs[0].ConvertTo<Engine>(sctx);
                            tit = cargs[1].ConvertTo<int>(sctx);
                        }
                        else if (cargs.Length >= 1)
                        {
                            teng = sctx.ParentEngine;
                            tit = cargs[0].ConvertTo<int>(sctx);
                        }
                        else
                        {
                            return sctx.CreateNativePValue(new Benchmark(sctx.ParentEngine));
                        }

                        return sctx.CreateNativePValue(new Benchmark(teng, tit));
                    });

            #endregion

            //Create a loader for that application and...
            var ldr = new Loader(engine, app);
            //load the main script file. 

            //CLI override script in action:
            var deleteSrc = false;
            var entryPath = GetPrxPath() + Path.DirectorySeparatorChar + @"prx.pxs";
            if (!File.Exists(entryPath))
            {
                //Load default CLI app
                entryPath = GetPrxPath() + Path.DirectorySeparatorChar + @"src" + Path.DirectorySeparatorChar + "prx_main.pxs";

                if (!File.Exists(entryPath))
                {
                    if (!Directory.Exists("src"))
                    {
                        var di = Directory.CreateDirectory(GetPrxPath() + Path.DirectorySeparatorChar + @"src");
                        di.Attributes = di.Attributes | FileAttributes.Hidden;
                        deleteSrc = true;
                    }

                    //Unpack source
                    writeFile(Resources.prx_main, "prx_main.pxs");
                    writeFile(Resources.prx_lib, "prx_lib.pxs");
                    writeFile(Resources.prx_interactive, "prx_interactive.pxs");
                }
            }

#if !DEBUG
            try
            {
#endif
                ldr.LoadFromFile(entryPath);
#if !DEBUG
            }
            catch (Exception exc)
            {
                _reportErrors(ldr);
                Console.WriteLine(exc);
                return false;
            }
#endif

            if (deleteSrc)
                Directory.Delete(GetPrxPath() + Path.DirectorySeparatorChar + @"src", true);

            //Report errors
            _reportErrors(ldr);

            return ldr.ErrorCount == 0;
        }

        private static void _reportErrors(Loader ldr)
        {
            if (ldr.ErrorCount > 0)
            {
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errors during compilation detected. Aborting.");
                foreach (var err in ldr.Errors)
                    Console.WriteLine(err);

                Console.ForegroundColor = originalColor;
                Environment.Exit(1);
                return;
            }
        }
    }
}