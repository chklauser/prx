// Prexonite
// 
// Copyright (c) 2013, Christian Klauser
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
using System.Diagnostics;
using System.Threading;
using Prexonite.Types;

namespace Prexonite.Concurrency
{
    public class Channel : IObject, IDisposable
    {
        #region Implementation of IObject

        public bool TryDynamicCall(StackContext sctx, PValue[] args, PCall call, string id,
            out PValue result)
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
                        _channelEmpty.WaitOne(Timeout.Infinite, true);
                        //Wait for the channelEmpty event (lock released)
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
                        _dataAvailable.WaitOne(Timeout.Infinite, true);
                        //Wait for the dataAvailable event (lock released)
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

        #region Implementation of IDisposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            if (_channelEmpty != null)
                _channelEmpty.Dispose();
            if (_dataAvailable != null)
                _dataAvailable.Dispose();
            _disposed = true;
        }

        #endregion
    }
}