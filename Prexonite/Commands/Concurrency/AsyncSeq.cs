using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prexonite.Commands.List;
using Prexonite.Compiler.Cil;
using Prexonite.Concurrency;
using Prexonite.Types;

namespace Prexonite.Commands.Concurrency
{
    public class AsyncSeq : CoroutineCommand, ICilCompilerAware
    {

        #region Singleton pattern

        private AsyncSeq()
        {
        }

        private static readonly AsyncSeq _instance = new AsyncSeq();

        public static AsyncSeq Instance
        {
            get { return _instance; }
        }

        #endregion 

        #region Overrides of PCommand

        public override bool IsPure
        {
            get { return false; }
        }

        #endregion

        #region Overrides of CoroutineCommand

        protected override IEnumerable<PValue> CoroutineRun(ContextCarrier sctxCarrier, PValue[] args)
        {
            return CoroutineRunStatically(sctxCarrier, args);
        }

        protected static IEnumerable<PValue> CoroutineRunStatically(ContextCarrier sctxCarrier, PValue[] args)
        {
            if (sctxCarrier == null)
                throw new ArgumentNullException("sctxCarrier");
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.Length < 1)
                throw new PrexoniteException("async_seq requires one parameter: The sequence to be computed.");

            var sctx = sctxCarrier.StackContext;

            //Store all necessary information in enumerable
            var seq = Map._ToEnumerable(sctx, args[0]);

            return new ChannelEnumerable(sctx, seq);
        }

        //Contains all the logic for spawing the background worker
        private class ChannelEnumerable : IEnumerable<PValue>
        {
            private readonly StackContext _sctx;
            private readonly IEnumerable<PValue> _seq;

            public ChannelEnumerable(StackContext sctx, IEnumerable<PValue> seq)
            {
                _sctx = sctx;
                _seq = seq;
            }

            #region Implementation of IEnumerable

            public IEnumerator<PValue> GetEnumerator()
            {
                var peek = new Channel();
                var data = new Channel();
                var rset = new Channel();
                var disp = new Channel();

                #region Producer
                //The producer runs on a separate thread and communicates
                //  with this thread via 4 channels, one for each method
                CallAsync.RunAsync(_sctx, () =>
                {
                    using (var e = _seq.GetEnumerator())
                    {
                        var doCont = true;
                        var doDisp = false;

                        cont:
                        if(e.MoveNext())
                        {
                            peek.Send(true);
                            data.Send(e.Current);
                        }
                        else
                        {
                            peek.Send(false);
                            doCont = false;
                        }

                        wait:
                        Select.RunStatically(
                            _sctx, new[]
                            {
                                new KeyValuePair<Channel, PValue>
                                    (rset,pfunc((s,a) =>
                                    {
                                        doCont = true;
                                        try
                                        {
                                            e.Reset();
                                        }
                                        catch (Exception exc)
                                        {
                                            rset.Send(PType.Object.CreatePValue(exc));
                                        }
                                        rset.Send(PType.Null);
                                        return PType.Null;
                                    })),
                                new KeyValuePair<Channel, PValue>
                                    (disp,pfunc((s,a) =>
                                    {
                                        doCont = false;
                                        doDisp = true;
                                        return PType.Null;
                                    })),
                                new KeyValuePair<Channel, PValue>
                                    (null,PType.Null), 
                            });

                        //We loop until the dispose command is explicitly given. 
                        //  -> This way, a reset command can be issued after
                        //  the complete enumeration has been computed
                        //  without the enumerator being disposed of prematurely
                        if(doCont)
                            goto cont;
                        else if(! doDisp)
                            goto wait;
                    }//end using (disposes enumerator)

                    //Ignored (part of CallAsync interface)
                    return PType.Null;
                });

                #endregion

                return new ChannelEnumerator(peek, data, rset, disp);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        //The channel enumerator is a proxy that translates method calls into messages
        //  to our producer
        private class ChannelEnumerator : IEnumerator<PValue>
        {

            public ChannelEnumerator(Channel peek, Channel data, Channel rset, Channel disp)
            {
                _peek = peek;
                _data = data;
                _rset = rset;
                _disp = disp;
            }

            private readonly Channel _peek;
            private readonly Channel _data;
            private readonly Channel _rset;
            private readonly Channel _disp;
            private PValue _current;

            #region Implementation of IDisposable

            public void Dispose()
            {
                _disp.Send(PType.Null);
            }

            #endregion

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                if((bool)_peek.Receive().Value)
                {
                    _current = _data.Receive();
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
                if(!p.IsNull)
                {
                    var exc = p.Value as Exception;
                    if (exc != null)
                        throw exc;
                    else
                        throw new PrexoniteException("Cannot reset async enumerator: " + p.Value);
                }
            }

            public PValue Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            #endregion
        }

        //Makes working with select from managed code easier
        private class PFunc : IIndirectCall
        {
            private readonly Func<StackContext, PValue[], PValue> _f;

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

        private static PValue pfunc(Func<StackContext, PValue[], PValue> f)
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
            return CompilationFlags.PreferRunStatically;
        }

        void ICilCompilerAware.ImplementInCil(CompilerState state, Instruction ins)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
