// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#region Namespace Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Prexonite;
using Prexonite.Commands;
using Prexonite.Compiler;
using Prexonite.Compiler.Build;
using Prexonite.Types;
using Prx.Benchmarking;

#endregion

namespace Prx;

internal static class Program
{
    public const string PrxScriptFileName = "prx_main.pxs";

    public static string GetPrxPath()
    {
        return AppContext.BaseDirectory;
    }

    public static void Main(string[] args)
    {
        Console.CancelKeyPress += delegate { Environment.Exit(1); };
        var prexoniteConsole = new PrexoniteConsole(true);

        //Let the exceptions surface so they can more easily be debugged
        try
        {
            var engine = new Engine();
            engine.RegisterAssembly(Assembly.GetExecutingAssembly());

            //Load application
            var app = _loadApplication(engine, prexoniteConsole);

            //Run the applications main function.
            if (app != null) //errors have already been reported
                _runApplication(engine, app, args);
        }
        // ReSharper disable once RedundantCatchClause
        catch (Exception ex)
        {
#if DEBUG
                _dummyUsageOf(ex);
                throw;
#else
            Console.WriteLine(ex);
#endif
        }
        finally
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine(Prexonite.Properties.Resources.Program_DebugExit);
                Console.ReadLine();
            }
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private static void _dummyUsageOf(object any)
    {
    }

    private static void _runApplication(Engine engine, Application app, IEnumerable<string> args)
    {
        app.Run(engine, args.Select(engine.CreateNativePValue).ToArray());
    }

    [CanBeNull]
    private static Application _loadApplication(Engine engine, PrexoniteConsole prexoniteConsole)
    {
        var plan = Plan.CreateSelfAssembling();
        var opts = new LoaderOptions(engine, null);
        if (plan.Options != null)
        {
            opts.InheritFrom(plan.Options);
        }
        plan.Options = opts;

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
                    return (double)timer.ElapsedMilliseconds;
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
                delegate { return (double)timer.ElapsedMilliseconds; }));

        #endregion

        #region Stack Manipulation commands

        engine.Commands.AddHostCommand
        (
            @"__replace_call",
            delegate(StackContext sctx, PValue[] cargs)
            {
                cargs ??= Array.Empty<PValue>();
                if (sctx == null)
                    throw new ArgumentNullException(nameof(sctx));

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
                                (string)carg.Value, out f))
                            throw new PrexoniteException
                            (
                                "Cannot replace call to " + carg +
                                " because no such function exists.");

                        rctx = f.CreateFunctionContext(e, rargs);
                        break;
                    case PType.BuiltIn.Object:
                        var clrType = ((ObjectPType)carg.Type).ClrType;
                        if (clrType == typeof(PFunction))
                        {
                            f = (PFunction)carg.Value;
                            rctx = f.CreateFunctionContext(e, rargs);
                        }
                        else if (clrType == typeof(Closure) &&
                                 clrType != typeof(Continuation))
                        {
                            var c = (Closure)carg.Value;
                            rctx = c.CreateFunctionContext(sctx, rargs);
                        }
                        else if (clrType == typeof(FunctionContext))
                        {
                            rctx = (FunctionContext)carg.Value;
                        }
                        break;
                }
                if (rctx == null)
                    throw new PrexoniteException("Cannot replace a context based on " +
                        carg);

                var node = e.Stack.Last;
                do
                {
                    if (node?.Value is FunctionContext ectx)
                    {
                        if (ReferenceEquals(ectx.Implementation, rctx.Implementation))
                        {
                            node.Value = rctx;
                            break;
                        }
                    }
                } while ((node = node?.Previous) != null);

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
                    throw new ArgumentNullException(nameof(sctx));
                if (cargs == null)
                    cargs = Array.Empty<PValue>();

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

        var prxPath = GetPrxPath();
            
        #region Self-assembling build plan reference

        engine.Commands.AddHostCommand(@"host\self_assembling_build_plan", (sctx, _) => sctx.CreateNativePValue(plan));
        engine.Commands.AddHostCommand(@"host\prx_path", (sctx, args) => sctx.CreateNativePValue(prxPath));

        #endregion

        (Application Application, ITarget Target)? result;
        try
        {
            var entryDesc = plan.AssembleAsync(Source.FromEmbeddedResource(Assembly.GetAssembly(typeof(Program))!, PrxScriptFileName)).Result;
            result = plan.Load(entryDesc.Name);
        }
        catch (BuildFailureException e)
        {
            _reportErrors(e.Messages);
            Console.WriteLine(e);
#if DEBUG
                throw;
#else
            result = null;
#endif
        }
        catch (BuildException e)
        {
            if(e.RelatedTarget != null)
                _reportErrors(e.RelatedTarget.BuildMessages);

            Console.WriteLine(e);
#if DEBUG
                throw;
#else
            result = null;
#endif
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
#if DEBUG
                throw;
#else
            result = null;
#endif
        }

        if (result == null)
        {
            return null;
        }

        if (_reportErrors(result.Value.Target.Messages)) 
            return null;
            
        var app = result.Value.Application;
        app.Meta["Version"] = Assembly.GetExecutingAssembly().GetName()!.Version!.ToString();
        return app;

    }

    private static bool _reportErrors(IEnumerable<Message> messages)
    {
        var originalColor = Console.ForegroundColor;
        var msgBySev = messages.ToGroupedDictionary<MessageSeverity, Message, List<Message>>(m => m.Severity);
#if DEBUG
            _reportWarnings(msgBySev, originalColor);
#endif
        if (msgBySev.TryGetValue(MessageSeverity.Error, out var errors) && errors.Count > 0)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Errors during compilation detected. Aborting.");
                foreach (var err in errors)
                    Console.WriteLine(err);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private static void _reportWarnings(IDictionary<MessageSeverity, List<Message>> messages, ConsoleColor originalColor)
    {
        try
        {
            Console.ForegroundColor = originalColor;
            if (messages.TryGetValue(MessageSeverity.Info, out var messageCategory))
                foreach (var message in messageCategory)
                    Console.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.Yellow;
            if (messages.TryGetValue(MessageSeverity.Warning, out messageCategory))
                foreach (var warning in messages[MessageSeverity.Warning])
                    Console.WriteLine(warning);
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }
}