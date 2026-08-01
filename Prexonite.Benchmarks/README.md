# Object member dispatch benchmarks

This project compares the original uncached `ObjectPType` resolver with the cached `CallSite<T>`
paths used by the interpreter and CIL compiler. Both implementations run side by side against the
same Prexonite build:

- `Uncached*` calls `PValue.DynamicCall` or `ObjectPType.StaticCall`, which still perform member
  discovery, filtering, and overload ranking on every invocation. This is the path CIL-compiled
  member calls used before callsite emission was added.
- `Cached*` invokes a pre-warmed `DynamicObjectMemberCallSite` or
  `StaticObjectMemberCallSite`. It reuses the resolved candidate plan while preserving Prexonite's
  conversions and reflection invocation.
- `CilObjectMemberCallBenchmarks` invokes paired generated functions directly. Its `Uncached*`
  functions are compiled with the diagnostic callsite opt-out, so the generated-function frame,
  argument handling, and return handling are identical on both sides.

The suite deliberately separates the easy steady-state case from costly cache behavior:

- `Wide cache hit` validates all 16 argument guards on every successful hit.
- `Instance polymorphic` alternates the explicit-conversion lock, exercising two stable rules.
- `Late guard miss` changes only argument 16, so a failed rule pays for 15 successful guard checks
  before the call site updates to its other pre-warmed rule.
- `Rebind: receiver tag` and `Rebind: argument tag` supply a fresh `ObjectPType` reference on every
  invocation. The sites are saturated during setup, so these measure the sticky megamorphic
  fallback rather than an ever-growing polymorphic rule chain.
- `CIL megamorphic retained hit` saturates a site and then repeatedly uses its newest retained
  binding. `CIL megamorphic five-shape cycle` rotates through five shapes, so four hit retained
  bindings while the evicted shape is resolved afresh on every cycle.

Run the benchmarks from the repository root in Release mode:

```bash
dotnet build -c Release -m:1 Prexonite.Benchmarks/Prexonite.Benchmarks.csproj
dotnet run -c Release --no-build --project Prexonite.Benchmarks -- --filter '*Prexonite.Benchmarks.ObjectMemberCallBenchmarks.*'
```

Use `--filter '*CilObjectMemberCallBenchmarks*'` to run only the CIL-generated function suite.
Use `--filter '*PatternMatchingCilBenchmarks*'` to run only the fully statically linked pattern
matching suite. Each benchmark invokes a generated CIL function that performs 256 matches; the
reported time and allocation columns are normalized to one match.

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
