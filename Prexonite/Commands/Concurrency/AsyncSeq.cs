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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Prexonite.Commands.List;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency;

public class AsyncSeq : CoroutineCommand, ICilCompilerAware
{
    #region Singleton pattern

    AsyncSeq()
    {
    }

    public static AsyncSeq Instance { get; } = new();

    #endregion

    #region Overrides of PCommand

    #endregion

    #region Overrides of CoroutineCommand

    protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier,
        PValue[] args)
    {
        return CoroutineRunStatically(sctxCarrier, args);
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        MessageId = "Coroutine")]
    protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier,
        PValue[] args)
    {
        if (sctxCarrier == null)
            throw new ArgumentNullException(nameof(sctxCarrier));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        if (args.Length < 1)
            throw new PrexoniteException(
                "async_seq requires one parameter: The sequence to be computed.");

        return new ChannelEnumerable(sctxCarrier, args[0]);
    }

    //Contains all the logic for spawing the background worker
    class ChannelEnumerable : IEnumerable<PValue>
    {
        readonly ContextCarrier _sctxCarrier;
        readonly PValue _arg;


        public ChannelEnumerable(ContextCarrier sctxCarrier, PValue arg)
        {
            _sctxCarrier = sctxCarrier;
            _arg = arg;
        }

        #region Implementation of IEnumerable

        public IEnumerator<PValue> GetEnumerator()
        {
            var peek = new Channel();
            var data = new Channel();
            var rset = new Channel();
            var disp = new Channel();

            #region Producer

            Func<PValue> producer =
                () =>
                {
                    using var e =
                        Map._ToEnumerable(_sctxCarrier.StackContext, _arg).GetEnumerator
                            ();
                    var doCont = true;
                    var doDisp = false;

                    cont:
                    if (e.MoveNext())
                    {
                        peek.Send(true);
                        data.Send(e.Current);
                    }
                    else
                    {
                        peek.Send(false);
                        //doCont = false;
                        goto shutDown;
                    }

                    wait:
                    Select.RunStatically(
                        _sctxCarrier.StackContext, new[]
                        {
                            new KeyValuePair<Channel, PValue>
                            (
                                rset, pfunc(
                                    (s, a) =>
                                    {
                                        doCont = true;
                                        try
                                        {
                                            e.Reset();
                                        }
                                        catch (Exception exc)
                                        {
                                            rset.Send(
                                                PType.Object.CreatePValue(exc));
                                        }
                                        rset.Send(PType.Null);
                                        return PType.Null;
                                    })),
                            new KeyValuePair<Channel, PValue>
                            (
                                disp, pfunc(
                                    (s, a) =>
                                    {
                                        doCont = false;
                                        doDisp = true;
                                        return PType.Null;
                                    })),
                            new KeyValuePair<Channel, PValue>
                                (null, PType.Null),
                        }, false);

                    //We loop until the dispose command is explicitly given. 
                    //  -> This way, a reset command can be issued after
                    //  the complete enumeration has been computed
                    //  without the enumerator being disposed of prematurely
                    if (doCont)
                        goto cont;
                    else if (! doDisp)
                        goto wait;

                    //Ignored (part of CallAsync interface)
                    shutDown:
                    return PType.Null;
                };

            #endregion

            return new ChannelEnumerator(peek, data, rset, disp, producer, _sctxCarrier);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    //The channel enumerator is a proxy that translates method calls into messages
    //  to our producer
    class ChannelEnumerator : IEnumerator<PValue>
    {
        public ChannelEnumerator(Channel peek,
            Channel data,
            Channel rset,
            Channel disp,
            Func<PValue> produce,
            ContextCarrier sctxCarrier)
        {
            _peek = peek;
            _sctxCarrier = sctxCarrier;
            _produce = produce;
            _data = data;
            _rset = rset;
            _disp = disp;

            //The producer runs on a separate thread and communicates
            //  with this thread via 4 channels, one for each method
        }

        readonly Channel _peek;
        readonly Channel _data;
        readonly Channel _rset;
        readonly Channel _disp;
        readonly Func<PValue> _produce;
        readonly ContextCarrier _sctxCarrier;

        #region Implementation of IDisposable

        public void Dispose()
        {
            _disp.Send(PType.Null);
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            if (Current == null)
            {
                //The producer runs on a separate thread and communicates
                //  with this thread via 4 channels, one for each method
                CallAsync.RunAsync(_sctxCarrier.StackContext, _produce);
                Current = PType.Null;
            }

            if ((bool) _peek.Receive().Value)
            {
                Current = _data.Receive();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            _rset.Send(PType.Null);
            var p = _rset.Receive();
            if (!p.IsNull)
            {
                if (p.Value is Exception exc)
                    throw exc;
                else
                    throw new PrexoniteException("Cannot reset async enumerator: " + p.Value);
            }
        }

        public PValue Current { get; private set; }

        object IEnumerator.Current => Current;

        #endregion
    }

    //Makes working with select from managed code easier
    class PFunc : IIndirectCall
    {
        readonly Func<StackContext, PValue[], PValue> _f;

        public PFunc(Func<StackContext, PValue[], PValue> f)
        {
            _f = f;
        }

        #region Implementation of IIndirectCall

        public PValue IndirectCall(StackContext sctx, PValue[] args)
        {
            return _f(sctx, args);
        }

        #endregion

        public static implicit operator PValue(PFunc f)
        {
            return PType.Object.CreatePValue(f);
        }
    }

    static PValue pfunc(Func<StackContext, PValue[], PValue> f)
    {
        return new PFunc(f);
    }

    public static PValue RunStatically(StackContext sctx, PValue[] args)
    {
        var carrier = new ContextCarrier();
        var corctx = new CoroutineContext(sctx, CoroutineRunStatically(carrier, args));
        carrier.StackContext = corctx;
        return sctx.CreateNativePValue(new Coroutine(corctx));
    }

    #endregion

    #region Implementation of ICilCompilerAware

    CompilationFlags ICilCompilerAware.CheckQualification(Instruction ins)
    {
        return CompilationFlags.PrefersRunStatically;
    }

    void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
    {
        throw new NotSupportedException();
    }

    #endregion
}