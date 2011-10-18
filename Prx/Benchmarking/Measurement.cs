// Prexonite
// 
// Copyright (c) 2011, Christian Klauser
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

namespace Prx.Benchmarking
{
    [DebuggerStepThrough]
    public sealed class Measurement
    {
        public Measurement(BenchmarkEntry parentEntry, long rawMilliseconds,
            long overheadMilliseconds)
        {
            if (parentEntry == null)
                throw new ArgumentNullException("parentEntry");
            _entry = parentEntry;
            _rawMilliseconds = rawMilliseconds;
            _overheadMilliseconds = overheadMilliseconds;
            _entry.Measurements.Add(this);
        }

        public BenchmarkEntry Entry
        {
            get { return _entry; }
        }

        private readonly BenchmarkEntry _entry;
        private readonly long _overheadMilliseconds;
        private readonly long _rawMilliseconds;

        public double RawSeconds
        {
            get { return _rawMilliseconds/1000.0; }
        }

        public double PassMilliseconds
        {
            get { return _rawMilliseconds/(double) Entry.Parent.Iterations; }
        }

        public double PassMicroseconds
        {
            get { return checked(PassMilliseconds*1000); }
        }

        public long ClearedMilliseconds
        {
            get { return (_rawMilliseconds - _overheadMilliseconds); }
        }

        public long ClearedMicroseconds
        {
            get { return checked(ClearedMilliseconds*1000); }
        }

        public double ClearedPassMilliseconds
        {
            get { return ClearedMilliseconds/(double) Entry.Parent.Iterations; }
        }

        public double ClearedPassMicroseconds
        {
            get { return checked(ClearedPassMilliseconds*1000); }
        }

        public long OverheadMilliseconds
        {
            get { return _overheadMilliseconds; }
        }

        public long RawMilliseconds
        {
            get { return _rawMilliseconds; }
        }
    }
}