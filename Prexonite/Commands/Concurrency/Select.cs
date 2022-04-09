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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Prexonite.Commands.Core;
using Prexonite.Commands.List;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency;

public class Select : PCommand, ICilCompilerAware
{
    #region Singleton pattern

    private Select()
    {
    }

    public static Select Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

    public override PValue Run(StackContext sctx, PValue[] args)
    {
        return RunStatically(sctx, args);
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        bool performSubCall;
        if (args.Length > 0 && args[0].Type.ToBuiltIn() == PType.BuiltIn.Bool)
            performSubCall = (bool) args[0].Value;
        else
            performSubCall = false;

        var rawCases = new List<PValue>();
        foreach (var arg in args.Skip(performSubCall ? 1 : 0))
        {
            var set = Map._ToEnumerable(sctx, arg);
            if (set == null)
                continue;
            else
                rawCases.AddRange(set);
        }

        var appCases =
            rawCases.Select(c => _isApplicable(sctx, c)).Where(x => x != null).Select(_extract).
                ToArray();

        return RunStatically(sctx, appCases, performSubCall);
    }

    public static PValue RunStatically(StackContext sctx,
        KeyValuePair<Channel, PValue>[] appCases, bool performSubCall)
    {
        //Check if there data is already available (i.e. if the select can be processed non-blocking)
        foreach (var kvp in appCases)
        {
            var chan = kvp.Key;
            var handler = kvp.Value;

            if (chan != null)
            {
                if (chan.TryReceive(out var datum))
                {
                    return _invokeHandler(sctx, handler, datum, performSubCall);
                }
            }
            else
            {
                return _invokeHandler(sctx, handler, null, performSubCall);
            }
        }

        //We have to wait for one of the channels to become active (there are no default handlers)
        _split(appCases, out var channels, out var handlers);
        var flags = channels.Select(c => c.DataAvailable).ToArray();

        while (true)
        {
            var selected = WaitHandle.WaitAny(flags);
            if (channels[selected].TryReceive(out var datum))
            {
                return _invokeHandler(sctx, handlers[selected], datum, performSubCall);
            }
            //else: someone ninja'd the damn thing before we could get to it, continue waiting
        }
    }

    private static PValue _invokeHandler(StackContext sctx, PValue handler, PValue datum,
        bool performSubCall)
    {
        var handlerArgv = datum != null ? new[] {datum} : Array.Empty<PValue>();
        return performSubCall
            ? CallSubPerform.RunStatically(sctx, handler, handlerArgv,
                useIndirectCallAsFallback: true)
            : handler.IndirectCall(sctx, handlerArgv);
    }

    private static readonly PType _chanType = PType.Object[typeof (Channel)];

    private static PValue _isApplicable(StackContext sctx, PValue selectCase)
    {
        if (selectCase.Type == PValueKeyValuePair.ObjectType)
        {
            var kvp = (PValueKeyValuePair) selectCase.Value;
            var key = kvp.Key;
            if (key.Type == _chanType)
                return selectCase;
            else
            {
                if (key.Type.ToBuiltIn() == PType.BuiltIn.Bool)
                    if ((bool) key.Value)
                        return kvp.Value;
                    else
                        return null;
                else if (key.Value == null)
                    return null;
                else if (Runtime.ExtractBool(key.IndirectCall(sctx, Array.Empty<PValue>()),
                             sctx))
                    return kvp.Value;
                else
                    return null;
            }
        }
        else
        {
            return selectCase;
        }
    }

    private static KeyValuePair<Channel, PValue> _extract(PValue c)
    {
        if (c.Type == PValueKeyValuePair.ObjectType)
        {
            var kvp = (PValueKeyValuePair) c.Value;

            if (kvp.Value.Type == PValueKeyValuePair.ObjectType)
            {
                kvp = (PValueKeyValuePair) kvp.Value.Value;
            }

            var key = kvp.Key;

            if (key.Type == _chanType)
                return new KeyValuePair<Channel, PValue>((Channel) kvp.Key.Value, kvp.Value);
            else if (key.Value == null)
                return new KeyValuePair<Channel, PValue>(null, kvp.Value);
            else
                throw new PrexoniteException(
                    "Invalid select clause. Syntax: select( [channel:handler] ) or select( [cond:channel:handler] ). Offending value " +
                    c.Value);
        }
        else if (c.Type == _chanType)
        {
            throw new PrexoniteException(
                "Missing handler in select clause. Syntax: select( [channel: handler] ) or select( [cond:channel:handler] )");
        }
        else
        {
            //A default handler or a handler that just doesn't have input (but possibly a condition)
            return new KeyValuePair<Channel, PValue>(null, c);
        }
    }

    private static void _split(IEnumerable<KeyValuePair<Channel, PValue>> cases,
        out Channel[] channels, out PValue[] handlers)
    {
        var chanCases = cases.Where(kvp => kvp.Key != null).ToArray();
        var count = chanCases.Length;
        channels = new Channel[count];
        handlers = new PValue[count];
        for (var i = 0; i < chanCases.Length; i++)
        {
            channels[i] = chanCases[i].Key;
            handlers[i] = chanCases[i].Value;
        }
    }

    #endregion

    #region Implementation of ICilCompilerAware

    public CompilationFlags CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    public void ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException("The command " + GetType().Name +
            " does not support CIL compilation via ICilCompilerAware.");
    }

    #endregion
}