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
using System.Diagnostics;

namespace Prx.Benchmarking;

[DebuggerStepThrough]
public sealed class Measurement
{
    public Measurement(BenchmarkEntry parentEntry, long rawMilliseconds,
        long overheadMilliseconds)
    {
        if (parentEntry == null)
            throw new ArgumentNullException(nameof(parentEntry));
        Entry = parentEntry;
        RawMilliseconds = rawMilliseconds;
        OverheadMilliseconds = overheadMilliseconds;
        Entry.Measurements.Add(this);
    }

    public BenchmarkEntry Entry { get; }

    public double RawSeconds => RawMilliseconds/1000.0;

    public double PassMilliseconds => RawMilliseconds/(double) Entry.Parent.Iterations;

    public double PassMicroseconds => PassMilliseconds*1000;

    public long ClearedMilliseconds => RawMilliseconds - OverheadMilliseconds;

    public long ClearedMicroseconds => checked(ClearedMilliseconds*1000);

    public double ClearedPassMilliseconds => ClearedMilliseconds/(double) Entry.Parent.Iterations;

    public double ClearedPassMicroseconds => ClearedPassMilliseconds*1000;

    public long OverheadMilliseconds { get; }

    public long RawMilliseconds { get; }
}