using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Prexonite.Types;

namespace Prexonite.Concurrency
{
    public class Channel : IObject
    {
        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id, out PValue result)
        {
            result = null;
            switch (id.ToUpperInvariant())
            {
                case "RECEIVE":
                    result = Receive();
                    break;
                case "SEND":
                    Send((args.Length > 0 ? args[0] : null) ?? PType.Null);
                    result = PType.Null;
                    break;
                case "TOSTRING":
                    result = ToString();
                    break;
                case "TRYRECEIVE":
                    PValue datum;
                    var refVar = (args.Length > 0 ? args[0] : null) ?? PType.Null;
                    if (TryReceive(out datum))
                    {
                        refVar.IndirectCall(sctx, new[] {datum});
                        result = true;
                    }
                    else
                    {
                        refVar.IndirectCall(sctx, new PValue[] {PType.Null});
                        result = false;
                    }
                    break;
                default:
                    return false;
            }

            return result != null;
        }

        #endregion

        #region State

        private PValue _datum;

        #endregion

        #region Synchornization

        private readonly object _syncRoot = new object();
        private readonly ManualResetEvent _dataAvailable = new ManualResetEvent(false);
        private readonly ManualResetEvent _channelEmpty = new ManualResetEvent(true);

        public WaitHandle DataAvailable
        {
            [DebuggerStepThrough]
            get { return _dataAvailable; }
        }

        #endregion

        public void Send(PValue obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            lock (_syncRoot)
            {
                while (_datum != null)
                {
                    try
                    {
                        Monitor.Exit(_syncRoot);
                        _channelEmpty.WaitOne(Timeout.Infinite, true); //Wait for the channelEmpty event (lock released)
                    }
                    finally
                    {
                        Monitor.Enter(_syncRoot);
                    }
                }

                //We have the lock and _datum == null
                //  -> Update state and signals
                _datum = obj;
                _channelEmpty.Reset();
                _dataAvailable.Set();
            }
        }

        public PValue Receive()
        {
            lock (_syncRoot)
            {
                while (_datum == null)
                {
                    try
                    {
                        Monitor.Exit(_syncRoot);
                        _dataAvailable.WaitOne(Timeout.Infinite, true); //Wait for the dataAvailable event (lock released)
                    }
                    finally
                    {
                        Monitor.Enter(_syncRoot);
                    }
                }

                //We have the lock and _datum != null
                var d = _datum;
                _datum = null;
                _dataAvailable.Reset();
                _channelEmpty.Set();
                return d;
            }
        }

        public bool TryReceive(out PValue datum)
        {
            lock (_syncRoot)
            {
                if (_datum == null)
                {
                    datum = null;
                    return false;
                }
                else
                {
                    datum = Receive();
                    return true;
                }
            }
        }
    }
}