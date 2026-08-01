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

## Direct CIL helpers and extraction caching

The next round extended the suite with a loop-only control, a list-shape-only pattern, an isolated
property pattern, and an isolated user view. This separates the fixed invocation/loop cost from the
cost of each pattern facility. The pre-change figures below were measured at parent change
`268dfbe7`; the view was added as a characterization case but its pre-change run was not completed.

| Case | Before | After | Time change | Before allocation | After allocation | Allocation change |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Loop-only control | 25.60 ns | 24.17 ns | -5.6% | 129 B | 129 B | unchanged |
| Three-element list shape | 73.07 ns | 45.71 ns | -37.4% | 433 B | 257 B | -40.6% |
| Three-element list pattern, hit | 94.27 ns | 51.82 ns | -45.0% | 537 B | 257 B | -52.1% |
| Property pattern, hit | 537.04 ns | 482.85 ns | -10.1% | 2,041 B | 1,697 B | -16.9% |
| User view pattern, hit | — | 235.94 ns | — | — | 1,121 B | — |
| Complex `match`, late miss | 1,475 ns | 1,258.71 ns | -14.7% | 5,161 B | 4,185 B | -18.9% |
| Complex `match`, first-arm hit | 1,553 ns | 1,281.96 ns | -17.5% | 5,665 B | 4,337 B | -23.4% |

The CIL compiler now recognizes the internal pattern helpers as extensions when their structural
arguments are compile-time constants. It calls statically callable implementations directly,
without constructing a `PValue[]` argument vector or routing through command dispatch. Property
probes return either the actual member value or a private missing-member sentinel, so the common
property path no longer constructs a two-element `[handled, value]` list and then extracts it twice.

List patterns also emit a CIL hint associating their shape test with their element and rest
extractors. The shape helper unwraps and validates `List<PValue>` once, stores the typed list in a
compiler local, and subsequent extractors index or slice that local directly. Wildcard-only list
patterns omit the hint because they have no extractors to reuse. Nested list patterns receive
independent caches. The cache local is declared lazily, after the CIL compiler has laid out its own
locals; declaring it during hint discovery can perturb the compiler's local layout and produce
invalid CIL.
