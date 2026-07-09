

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