# Pattern matching performance

This document records the performance baseline and subsequent optimization results for pattern
matching. The benchmark source lives in `Prexonite.Benchmarks/PatternMatchingCilBenchmarks.cs`.
It compiles its Prexonite functions with `FunctionLinking.FullyStatic` and invokes the generated CIL
delegates directly. Each delegate invocation performs 256 matches; BenchmarkDotNet reports results
normalized to one match.

Run just this suite with:

```shell
dotnet run -c Release --project Prexonite.Benchmarks -- \
  --filter '*PatternMatchingCilBenchmarks*'
```

## Baseline

Measured on 2026-08-01 before the pattern emitter optimizations, at parent change `47b6a7eb`:

- AMD Ryzen 9 7950X3D, 32 logical / 16 physical cores
- .NET SDK 10.0.301, .NET 10.0.9 x64 RyuJIT
- Concurrent workstation GC
- BenchmarkDotNet `NoiseResistant` job, three launches, at least 15 iterations

| Case | Mean | Allocated per match |
| --- | ---: | ---: |
| Literal miss | 49.47 ns | 257 B |
| Literal hit | 59.66 ns | 321 B |
| Binding plus guard, hit | 63.65 ns | 321 B |
| Three-element list pattern, hit | 118.78 ns | 745 B |
| Complex `match`, late miss | 1,502.06 ns | 5,137 B |
| Complex `match`, first-arm hit | 1,629.01 ns | 5,849 B |

The scalar cases cover likely hot-path patterns. The list case adds extraction and three nested
patterns. The complex cases combine a statically linked script view, property and list extraction,
a guard, multiple match arms, and either an early hit or a late fallback.

## Optimized results

Measured with the same configuration after the emitter optimizations. The scalar values come from
one full six-case run. The list and complex values are final targeted re-runs after the affected
optimizations; each targeted run retained the same three-launch job.

| Case | Mean | Time change | Allocated per match | Allocation change |
| --- | ---: | ---: | ---: | ---: |
| Literal miss | 48.86 ns | -1.2% | 257 B | unchanged |
| Literal hit | 59.39 ns | -0.5% | 321 B | unchanged |
| Binding plus guard, hit | 62.56 ns | -1.7% | 321 B | unchanged |
| Three-element list pattern, hit | 88.20 ns | -25.7% | 537 B | -27.9% |
| Complex `match`, late miss | 1,475 ns | -1.8% | 5,161 B | +0.5% |
| Complex `match`, first-arm hit | 1,553 ns | -4.7% | 5,665 B | -3.1% |

The emitter now:

- emits simple relational, wildcard, binding, and binding-plus-guard tests directly against the
  subject expression, without a subject temporary;
- relies on definite-binding flow and the VM/CIL local initialization guarantee instead of writing
  `null` to hidden capture locals;
- emits nested binders and relational patterns directly from their extractor when the extracted
  value does not need to be reused;
- omits the pure `pattern\output\at`/`pattern\list\slice` helper call for wildcard outputs,
  while still invoking user views and property probes; and
- keeps the `match` result local.

The last point is deliberate. An A/B run of direct evaluation-stack joining made the complex hit
slower and increased allocation by roughly 40--64 B per match. Restoring the result local improved
both complex paths, so the two extra bytecode instructions are retained for generated-CIL quality.
