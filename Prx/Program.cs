/*
 * Prx, a standalone command line interface to the Prexonite scripting engine.
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Types;
using Prx.Properties;

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
                FileStream fsmain =
                    new FileStream(
                        GetPrxPath() + @"\src\" + path, FileMode.Create, FileAccess.Write))
                fsmain.Write(buffer, 0, buffer.Length);
        }

        private static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate { Environment.Exit(1); };
            PrexoniteConsole pc = new PrexoniteConsole(true);

#if !DEBUG //Let the exceptions surface so they can more easily be debugged
            try
            {
#endif
            Engine engine = new Engine();
            engine.RegisterAssembly(Assembly.GetExecutingAssembly());

            #region Stopwatch commands

            //prx.exe provides these three additional commands for high speed access to a stopwatch from your script code
            Stopwatch timer = new Stopwatch();
            engine.Commands.AddHostCommand(
                @"timer\start",
                new DelegatePCommand(
                    delegate
                    {
                        timer.Start();
                        return null;
                    }));

            engine.Commands.AddHostCommand(
                @"timer\stop",
                new DelegatePCommand(
                    delegate
                    {
                        timer.Stop();
                        return (double) timer.ElapsedMilliseconds;
                    }));

            engine.Commands.AddHostCommand(
                @"timer\reset",
                new DelegatePCommand(
                    delegate
                    {
                        timer.Reset();
                        return null;
                    }));

            engine.Commands.AddHostCommand(
                @"timer\elapsed",
                new DelegatePCommand(
                    delegate { return (double) timer.ElapsedMilliseconds; }));

            #endregion

            #region Stack Manipulation commands

            engine.Commands.AddHostCommand(
                @"__replace_call",
                delegate(StackContext sctx, PValue[] cargs)
                {
                    if (cargs == null)
                        cargs = new PValue[] {};
                    if (sctx == null)
                        throw new ArgumentNullException("sctx");

                    Engine e = sctx.ParentEngine;

                    if (cargs.Length < 1)
                        throw new PrexoniteException(
                            "__replace_call requires the context or function to be replaced.");

                    PValue carg = cargs[0];
                    PValue[] rargs = new PValue[cargs.Length - 1];
                    Array.Copy(cargs, 1, rargs, 0, rargs.Length);

                    FunctionContext rctx = null;
                    PFunction f;
                    switch(carg.Type.ToBuiltIn())
                    {
                        case PType.BuiltIn.String:
                            if (
                                !sctx.ParentApplication.Functions.TryGetValue(
                                     (string)carg.Value, out f))
                                throw new PrexoniteException(
                                    "Cannot replace call to " + carg +
                                    " because no such function exists.");

                            rctx = f.CreateFunctionContext(e, rargs);
                            break;
                        case PType.BuiltIn.Object:
                            Type clrType = ((ObjectPType) carg.Type).ClrType;
                            if (clrType == typeof(PFunction))
                            {
                                f = (PFunction) carg.Value;
                                rctx = f.CreateFunctionContext(e, rargs);
                            }
                            else if(clrType == typeof(Closure) && clrType != typeof(Continuation))
                            {
                                Closure c = (Closure)carg.Value;
                                rctx = c.CreateFunctionContext(sctx, rargs);
                            }
                            else if(clrType == typeof(FunctionContext))
                            {
                                rctx = (FunctionContext)carg.Value;
                            }
                            break;
                    }
                    if(rctx == null)
                        throw new PrexoniteException("Cannot replace a context based on " + carg);

                    LinkedListNode<StackContext> node = e.Stack.Last;
                    do
                    {
                        FunctionContext ectx = node.Value as FunctionContext;

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

            engine.Commands.AddHostCommand("__console", pc);

            #endregion

            #region Create benchmark command

            engine.Commands.AddHostCommand("createBenchmark",
                delegate(StackContext sctx, PValue[] cargs)
                {
                    if (sctx == null)
                        throw new ArgumentNullException("sctx");
                    if (cargs == null)
                        cargs = new PValue[]{};

                    Engine teng;
                    int tit;

                    if(cargs.Length >= 2)
                    {
                        teng = cargs[0].ConvertTo<Engine>(sctx);
                        tit = cargs[1].ConvertTo<int>(sctx);
                    }
                    else if(cargs.Length >= 1)
                    {
                        teng = sctx.ParentEngine;
                        tit = cargs[0].ConvertTo<int>(sctx);
                    }
                    else
                    {
                        return  sctx.CreateNativePValue(new Benchmarking.Benchmark(sctx.ParentEngine));
                    }

                    return sctx.CreateNativePValue(new Benchmarking.Benchmark(teng, tit));
                });

            #endregion

            //Create an empty application
            Application app = new Application("prx");
            //Create a loader for that application and...
            Loader ldr = new Loader(engine, app);
            //load the main script file.
            string entryPath = GetPrxPath() + @"\src\prx_main.pxs";
            bool deleteSrc = false;
            if (!File.Exists(entryPath))
            {
                if (!Directory.Exists("src"))
                {
                    DirectoryInfo di = Directory.CreateDirectory(GetPrxPath() + @"\src");
                    di.Attributes = di.Attributes | FileAttributes.Hidden;
                    deleteSrc = true;
                }

                //Unpack source
                writeFile(Resources.prx_main, "prx_main.pxs");
                writeFile(Resources.prx_lib, "prx_lib.pxs");
                writeFile(Resources.prx_interactive, "prx_interactive.pxs");
            }

#if !DEBUG
            try
            {
#endif
            ldr.LoadFromFile(entryPath);
#if !DEBUG
            }
            catch(Exception exc)
            {
                _reportErrors(ldr);
                Console.WriteLine(exc);
                return;
            }
#endif

            if (deleteSrc)
                Directory.Delete(GetPrxPath() + @"\src", true);

            //Report errors
            _reportErrors(ldr);

            //Run the applications main function.
            app.Run(engine, new[] {engine.CreateNativePValue(args)});
#if !DEBUG
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
#endif
        }

        private static void _reportErrors(Loader ldr)
        {
            if (ldr.ErrorCount > 0)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errors during compilation detected. Aborting.");
                foreach (string err in ldr.Errors)
                    Console.WriteLine(err);

                Console.ForegroundColor = originalColor;
                Environment.Exit(1);
                return;
            }
        }
    }
}