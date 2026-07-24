# Object member dispatch benchmarks

This project compares the original uncached `ObjectPType` resolver with the interpreter's cached
`CallSite<T>` path. Both implementations run side by side against the same Prexonite build:

- `Uncached*` calls `PValue.DynamicCall` or `ObjectPType.StaticCall`, which still perform member
  discovery, filtering, and overload ranking on every invocation. This is also the path currently
  used by CIL-compiled member calls.
- `Cached*` invokes a pre-warmed `DynamicObjectMemberCallSite` or
  `StaticObjectMemberCallSite`. It reuses the resolved candidate plan while preserving Prexonite's
  conversions and reflection invocation.

The suite deliberately separates the easy steady-state case from costly cache behavior:

- `Wide cache hit` validates all 16 argument guards on every successful hit.
- `Instance polymorphic` alternates the explicit-conversion lock, exercising two stable rules.
- `Late guard miss` changes only argument 16, so a failed rule pays for 15 successful guard checks
  before the call site updates to its other pre-warmed rule.
- `Rebind: receiver tag` and `Rebind: argument tag` supply a fresh `ObjectPType` reference on every
  invocation. Setup verifies that every cached invocation really creates a new binding; these are
  intentional deoptimization/unstable-shape cases rather than cache hits.

Run the benchmarks from the repository root in Release mode:

```bash
dotnet build -c Release -m:1 Prexonite.Benchmarks/Prexonite.Benchmarks.csproj
dotnet run -c Release --no-build --project Prexonite.Benchmarks -- --filter '*ObjectMemberCallBenchmarks*'
```

The default configuration is intended for a desktop that may experience unrelated load:

- three separate benchmark-process launches;
- BenchmarkDotNet's adaptive pilot and warmup stages;
- 15–100 measurement iterations, continuing until the 99.9% confidence-interval error is at most
  2% of the mean (or the maximum iteration count is reached);
- upper-outlier removal for interruptions caused by other processes;
- mean, median, standard deviation, 95th percentile, and baseline ratio (BenchmarkDotNet adds ratio
  variability when it is statistically meaningful);
- `MemoryDiagnoser` allocation and GC statistics.

Results and raw measurements are written to `BenchmarkDotNet.Artifacts`. Check BenchmarkDotNet's
warnings, the error and `RatioSD` columns, and the generated distribution data before trusting a
small difference. Avoid comparing results from different power modes or while the machine is under
sustained load.
