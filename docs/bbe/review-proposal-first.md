# First adversarial review of the pattern-matching proposal

## Verdict

The proposal covers almost every requested feature and its dual-outcome binding model is a promising way to support the high-value negated guard-clause case. It is not yet implementation-ready. Two blockers prevent the surface language and compiler integration from having a unique implementation: the grammar does not delimit binding-boolean expressions correctly, and capture flow is computed before macros that are explicitly allowed to change that flow have expanded.

There are also major unresolved choices around captures used in general value expressions, compatibility with the existing built-in `is_*` macro ABI, duplicate acquisition after a flow intersection, application-wide gate stability across transcluded sources, `for` conditions, and writes to capture locals that are later normalized.

This review treats `docs/bbe/prompt.md` as authoritative, then checks `docs/bbe/proposal.md` against the current grammar/compiler/runtime/tests and the local paper. It does not object to deliberate exclusions merely because the paper includes more features.

## Prioritized summary

| Priority | ID | Severity | Finding |
| --- | --- | --- | --- |
| 1 | B1 | Blocker | The proposed grammar is incomplete, and relational operands greedily consume expression-level `&&`/`||`; one of the proposal's main examples does not have the claimed parse. |
| 2 | B2 | Blocker | Parser-time binding/symbol resolution is incompatible with emission-time macros or post-parse hooks adding, removing, or moving captures. Recomputing flow annotations cannot repair already-resolved continuations. |
| 3 | M1 | Major | The scope and lifetime of captures from an `is` expression used as an ordinary value are unspecified. |
| 4 | M2 | Major | `is_String(subject)` is not the current built-in type-check ABI; current built-ins are macros that require a hidden type-argument arity before the subject. |
| 5 | M3 | Major | “Repeated acquisition on a reachable path” contradicts the environment-based rule and slot clearing after `or` intersections. |
| 6 | M4 | Major | An application/module gate can change while one application is assembled from multiple source files, effectively creating the section-level mode switching that the proposal defers. |
| 7 | M5 | Major | Binding-flow scope for existing `for` conditions is omitted. |
| 8 | M6 | Major | Edge normalization can observably undo user writes through ordinary capture locals or references, contrary to the effects claim. |
| 9 | m1–m5 | Minor | Nested/lambda mode inheritance, contextual `match`, property helper details, view ergonomics, and test coverage need tightening. |

## Requirements coverage

| Requirement | Assessment |
| --- | --- |
| Disabled-by-default module/application gate; optional finer overrides | Partially covered. Header syntax and function overrides are proposed, but application-wide stability across additional loaded sources and exact nested/lambda inheritance are unresolved. |
| Repurpose `is`, ideally preserving `x is String` | Partially covered. The name-shift intent is right, but the proposed unary-call account is incompatible with the current built-in macro ABI. |
| Dynamic-language binding/symbol flow | Conceptually covered by `(T, F)` environments, but ordinary value-expression roots and post-expansion validation remain unspecified. |
| User-extensible predicates and views through name shifting | Covered at the basic named-function level. Applied `view_` heads and bare `is_` heads require no static types or declaration metadata. Configurable/first-class views are deliberately limited. |
| CIL plan, including exception regions | Covered credibly. The existing CIL SEH machinery supports the proposed ordinary-jump lowering; helper contracts still need specification and parity tests. |
| Separate expression `&& || !` from pattern `and or not` | Covered behind the gate. |
| No Section 3 backtracking; preserve `return`/`break`/`continue` | Covered for ordinary branches. `for`-condition binding scope remains missing, but no new control target is required. |
| Optional concrete `match` syntax | Covered, subject to the contextual-token and BBE grammar issues. |
| Implicit binders | Covered with `var` and `==` disambiguators. |
| Partial-application predicates | Covered via the acceptable explicit `?(...)` spelling. |
| Bindings from negated patterns without divergence analysis | Strongly covered by false-continuation environments and post-`if` promotion. |
| Tolerant active-pattern arity | Covered: missing outputs become `null`, extra outputs are ignored, and zero outputs can succeed. |
| Do not repeat the paper's obsolete C# claims | Covered explicitly. |
| Deprecate/replace the macro pattern library | Covered with a gated migration path rather than an unguarded removal. |

## Blockers

### B1. The grammar has no executable BBE boundary and misparses relational patterns

**Evidence.** `docs/bbe/proposal.md:85-111` defines `Pattern`, but references an undefined `BindingBoolean` nonterminal. `RelationalPattern` is defined as an operator followed by a full `Expression` (`proposal.md:109`), while `proposal.md:142` says that operand stops only at a pattern `and`, `or`, `when`, or a surrounding delimiter. Expression-level `&&` and `||` are not in that stop set.

As a result, the proposal's own example at `proposal.md:74`,

```pxs
value is [a, b] && a is > 0 && b is > a
```

does not follow from the grammar. On reaching `a is >`, the full `Expression` operand can consume `0 && b is > a`. The intended outer BBE conjunction is therefore swallowed by the relational operand. The current grammar confirms why this matters: the full expression nonterminal includes lazy disjunction and conjunction (`Prexonite/Compiler/Grammar/Parser.Expression.atg:56-71`), and current `is` is parsed in the postfix layer (`Parser.Expression.atg:229-258`). Splitting word and symbolic tokens does not by itself introduce the needed boundary.

The same omission leaves precedence and grouping undefined for guards, nested `is` tests, ordinary boolean terms, conditional expressions, and match-arm termination. Saying “approximately its current postfix precedence” (`proposal.md:68`) is not sufficient for the mutually recursive expression/pattern grammar.

**Impact.** There is no unique parser or AST for central examples. An implementer could reasonably produce incompatible parses, and Coco/R conflicts or manual lookahead would encode accidental semantics.

**Correction.** Specify the complete mutually recursive grammar, including a BBE root and all precedence levels. In particular:

- make `BindingBoolean` concrete, with symbolic `!`, `&&`, and `||` levels;
- define an expression atom/operand that cannot consume the enclosing BBE operators;
- parse a relational pattern operand at a precedence tighter than `&&`/`||`, while allowing a parenthesized full expression when a boolean operand is genuinely intended;
- state the exact terminators for guards in view/list/property subpatterns and match arms;
- add parse-shape tests for every example in `proposal.md:70-79`, especially `a is > 0 && ...`, plus parenthesized counterexamples.

### B2. Capture-changing macros cannot work with immediate resolution

**Evidence.** The proposal retains immediate parser resolution (`proposal.md:365-380`), parses dependent operands and branches through capture overlays (`proposal.md:376`), and then says macro-created pattern nodes may declare captures or cause annotations to be recomputed after arbitrary mutation (`proposal.md:400`). These operations occur at incompatible times in the current compiler.

An `if` condition is completely parsed before its branch blocks are constructed and parsed (`Prexonite/Compiler/Grammar/Parser.Statement.atg:373-393`). Symbol uses in those blocks are resolved while AST nodes are constructed. By contrast, `AstExpand` invokes the macro from `DoEmitCode` and immediately emits the returned node (`Prexonite/Compiler/AST/AstExpand.cs:30-45`). Compiler hooks likewise run after parsing, immediately before AST emission (`Prexonite/Compiler/Grammar/Parser.GlobalScope.atg:727-732`; `Prexonite/Compiler/Loader.cs:839-867`). Current AST nodes are broadly mutable, and there is no mutation/version protocol capable of reliably invalidating a flow annotation.

If a macro or hook adds a binder named `x`, removes one, or changes which routes acquire it, recomputing `(T, F)` before emission is too late: references in the right operand, `if`/`else` blocks, and following statements have already resolved either to an ambient symbol, a capture slot, an error symbol, or an unresolved/null AST. Flow recomputation cannot retroactively change those AST references or lexical stores.

This is not a problem for a capture-neutral built-in such as the existing `is_String` macro, but the proposal and test matrix explicitly promise general construction/inspection of enabled pattern AST (`proposal.md:398-400`, `498`).

**Impact.** Macro-produced or macro-mutated captures can silently bind the wrong symbol, fail to scope, or leave stale slots/annotations. The stated macro API cannot be implemented soundly atop the proposed parser-only context.

**Correction.** Choose and document one of these coherent models:

1. Keep immediate resolution, but freeze capture topology after parsing. Macros/hooks may inspect and transform capture-neutral internals, but may not add/remove/rename/reassociate exporting binders. Factory-created binding nodes must carry a capture signature known at the syntactic invocation before dependent source is parsed. Validate that post-parse transforms preserve that signature.
2. Permit general capture-changing macros, but introduce a post-expansion binding-flow and name-resolution phase. Dependent uses must remain symbolic until that phase; parser overlays alone are then not the architecture.

“Invalidate and recompute the annotation” is insufficient unless the proposal also explains how all dependent symbol uses are re-resolved. Add adversarial tests in which macros add, remove, rename, and move binders across `and`/`or`/`not`.

## Major issues

### M1. Captures from a general value-valued `is` expression have no defined lexical contract

**Evidence.** `proposal.md:83` says an `is` expression always produces an ordinary boolean “when used as a value,” and `proposal.md:340` says all bindings/slots are known after parsing. The scope rules, however, define promotion only for statement conditions (`proposal.md:307-336`) and explicitly special-case conditional expressions and loops. They do not define what happens in examples such as:

```pxs
var matched = value is pair(a, b);
consume(a);

consume(value is pair(c, d));

(value is pair(e, f)) == true && consume(e);
```

Nor is it stated whether a binding `is` nested inside an otherwise “ordinary boolean expression” is traversed by `Flow`; the rule at `proposal.md:290` simply assigns an ordinary boolean expression `(E, E)`.

The paper avoids this ambiguity by giving BBEs their own syntactic category and using them in designated control-flow positions (paper §2.1, Figure 1). Prexonite can choose a broader design, but it must specify the boundary at which captures stop exporting.

**Impact.** Parser scope, slot allocation, post-expression visibility, and edge clearing differ depending on an unstated notion of the “root binding-boolean AST.” Macros cannot know which factory result is legal, and users cannot predict whether a bare pattern name declares a usable local or merely performs an inaccessible store.

**Correction.** Define binding-capable expression consumers and export boundaries. A conservative rule would let bindings flow only within the BBE's own `&&`/`||`/`!` continuations and into explicitly enumerated consumers (`if`/`unless`, loop body, conditional branch, guard, match arm); an `is` used elsewhere still returns a boolean but does not promote captures beyond itself. Alternatively, extend flow analysis compositionally across every expression form. In either case, specify nested calls/equality/assignment/return, standalone expression statements, and whether inaccessible binders are allowed or diagnosed.

### M2. The promised `String` compatibility does not match the current shifted macro ABI

**Evidence.** `proposal.md:71`, `179-190` describes a bare shifted predicate as a unary call such as `is_String(value)`, then also says current type arguments and the same `AstTypecheck` lowering are retained. The current implementation does not call built-in type-check heads with only the subject.

`PrexoniteTypeExpr` detects whether the resolved head supports type arguments and, if it does, prepends the type-argument count before the type arguments (`Prexonite/Compiler/Grammar/Parser.Expression.atg:508-588`). `BuiltInTypeCommandBase` requires that arity integer as argument zero and separates type arguments from operation arguments (`Prexonite/Compiler/Macro/Commands/BuiltInTypeCommandBase.cs:73-119`). `CheckIsBuiltIn` then takes the first operation argument as the subject and constructs `AstTypecheck` (`Prexonite/Compiler/Macro/Commands/CheckIsBuiltIn.cs:14-23`). The standard `is_String` symbol is an expanding macro command, not an ordinary unary function (`Prexonite/prxlib/prx.prim.pxs:132-142`). A literal `is_String(subject)` expansion treats `subject` as the arity and has no operation argument.

The current capability check can also consult function metadata (`BuiltInTypeCommandBase.cs:23-71`), whereas the proposal repeatedly says resolution requires only a symbol name. Name-only lookup is enough to choose predicate versus binder, but it is not enough to reproduce the optional type-argument ABI exactly.

**Impact.** The headline compatibility example can fail at macro expansion, and `Object<...>`/CLR-qualified checks have no unambiguous lowering. A forward-declared custom `is_x` and a built-in type head need different argument construction.

**Correction.** Reuse the existing type-head construction protocol for resolved built-in/type-capable heads, including the hidden arity, while using a unary subject call for ordinary shifted predicates. State how capability is recognized when metadata is unavailable and what a forward-declared predicate may do with `<...>`. Alternatively, introduce a uniform new unary predicate ABI and compatibility shims for all standard `is_*` macros. Add bytecode and runtime parity tests for every primitive head, aliases/imports, `Object<...>`, and CLR syntax.

### M3. Duplicate acquisition is specified in two incompatible ways

**Evidence.** `proposal.md:161-173` says repeated acquisition of the same binding “on one reachable path” is an error, while mutually exclusive alternatives may share a slot. The formal rule at `proposal.md:301`, however, errors only when a name is already *valid* in the current flow environment. Slot normalization discards captures absent from an intersected environment (`proposal.md:357`).

Consider the shape:

```pxs
value is (left(x, y) or right(x)) and again(y)
```

The successful environment of the `or` contains `x` but not `y`, so the rule at line 301 allows `again(y)`. Yet a concrete route through `left(x, y)` acquires `y` twice, which line 161 appears to reject. This is the exact class of conflict for which the paper imposes the stronger `pv`-disjointness condition (paper §2.2, Figure 5 and the discussion on printed pp. 1:9–1:10). The proposal can safely be more expressive because it has real slots and explicit clearing, but it must say so.

**Impact.** Implementations and diagnostics will disagree on legal programs. Alias binders and guards have the same ambiguity after a flow intersection.

**Correction.** Prefer the already implied flow-valid rule: acquisition is an error only while the same slot is valid on that continuation; an intersection/normalization point discards the previous acquisition and permits a later overwrite. Replace “one reachable path” with that definition and add the example above. If the intended rule is instead the paper's syntactic-path freshness rule, reject such patterns using a separate `pv` analysis and remove the claim that environment validity alone detects duplicates.

### M4. The application-level gate is not stable across multi-source assembly

**Evidence.** `proposal.md:21-29` places the mode in module/application metadata and rejects only a late assignment within a module header; `proposal.md:47` says global initialization uses the module value and rejects arbitrary section switches. A `Loader`, however, can load many readers/files into the same `Application` (`Prexonite/Compiler/Loader.cs:641-650`, `668-733`, `748-772`), including files added by build scripts. Each load reparses metadata into the same application. Initialization AST/code is emitted incrementally into the same initialization function (`Loader.cs:827-867`). The repository uses this mechanism, for example `Prx/psr/_2/psr/pattern.pxs:18` adds another source file.

Thus a transcluded source can omit, repeat, or reverse `pattern_matching` after earlier functions or initialization fragments were parsed. Reading the current application meta at each function/global fragment creates mixed-mode sections despite the proposal deferring section-level switches. A later `disabled` can also change the metadata used to decide whether an already mixed initialization function is CIL-eligible.

**Impact.** Parse meaning depends on load order; a module can accidentally contain both grammars; diagnostics and CIL gating become nondeterministic across otherwise normal `build does add(...)` usage.

**Correction.** Define and enforce a frozen effective mode per application/module assembly. The first explicit header (or a default frozen at the first non-header construct) should establish it; every later contributing source must inherit it or issue an error on a conflicting explicit assignment. Record the frozen value outside freely mutable general metadata, and define how preflight/build-added sources communicate it. Test root-plus-transclusion combinations in both orders and mixed global initialization.

### M5. Existing `for` conditions are missing from binding-flow scope rules

**Evidence.** `proposal.md:336` defines `while`/`until`, post-conditions, and conditional-expression scope, but never defines `for`. The current language has binding-bearing condition positions in both forms of `for`; the parser stores a condition and has distinct initialize, next-iteration, and body scopes (`Prexonite/Compiler/Grammar/Parser.Statement.atg:418-451`). Since enabled `is` remains a general expression, it can occur there even if the proposal does not mention it.

**Impact.** It is unclear whether condition captures are visible in the body, next-iteration clause, both, or neither, and when their slots are reset. `continue` makes the ordering observable. This is directly adjacent to the hard requirement that existing unlabeled `break`/`continue` remain supported (`prompt.md:42-43`).

**Correction.** Define flow for each current `for` form based on actual evaluation order. If that is too much for the first implementation, explicitly reject exporting binders in `for` conditions while continuing to allow capture-free predicate patterns. Add `continue` and false-exit tests for both pre- and post-condition forms.

### M6. Capture-slot clearing can roll back user-visible capture mutations

**Evidence.** Captures are ordinary auto-dereferenced locals (`proposal.md:286`), all slots are promoted after an `if` and default to/are reset to `null` (`proposal.md:315-317`), and every outgoing edge clears slots absent from its environment (`proposal.md:357`). Yet `proposal.md:361` says normalization “does not undo mutations.” Ordinary Prexonite expressions can assign to locals and take references to them, including from a `when` guard.

For example, a route can acquire `x`, assign to `x` in a guard, then fail; if the overall failure environment intersects with an earlier gate-failure route that never acquired `x`, normalization clears `x`. The assignment is observable after the condition because the slot is deliberately promoted, and it is also observable through an escaped reference to `x`. Clearing it is necessarily a mutation of user-observable state.

**Impact.** The effects claim is false for the very locals introduced by matching. Users may see assignments or reference-visible values replaced with `null` at control-flow merges.

**Correction.** State this exception explicitly, or prevent it. Robust options are to make acquisition bindings non-assignable/non-addressable within a BBE, or to use hidden immutable acquisition slots and copy only flow-valid values into ordinary exposed locals at continuation boundaries. Add tests for assignment, increment, and reference escape from a guard that later fails.

## Minor issues

### m1. Nested-function and lambda mode inheritance needs an explicit hierarchy

`proposal.md:47` says a function uses its own meta value or the module value, “also” for nested functions, while the test matrix asks for “nested-function inheritance” (`proposal.md:488`). That could mean direct module fallback or lexical inheritance from an overridden enclosing function. The difference is visible when an opted-in function in a disabled module contains a nested function or lambda. Lambdas are anonymous `PFunction`s created and compiled immediately and currently inherit only import/parent metadata (`Prexonite/Compiler/Grammar/Parser.Expression.atg:418-459`); they have no surface metadata block.

Specify `explicit function override > enclosing effective function mode > module mode` if lexical inheritance is intended, or explicitly document that nested functions/lambdas revert to the module setting. A parser mode stack is clearer than consulting mutable metadata ad hoc. Test lambdas as well as named nested functions in both override directions.

### m2. Contextual `match`/`when` and member-name syntax are not fully specified

The formal grammar quotes `match` and `when` as terminals (`proposal.md:90`, `423-425`), while `proposal.md:62` says they remain contextual identifiers. The current parser supports contextual words with semantic lookahead, but the proposal does not state the recognition rule or the fate of an enabled-mode call shaped like `match(x) { ... }`. `MemberName` is also referenced but undefined (`proposal.md:105`), leaving keyword, quoted, qualified, and punctuation-containing members unclear.

Define the semantic predicates and identifier escape rules, and define `MemberName` (ideally identifier or string literal). Add legacy and enabled tests for variables/functions named `match` and `when`.

### m3. The property/member helper contract is too abstract for interoperability and CIL parity

`proposal.md:259-270` requires tolerant output access and an explicit presence/value probe but gives no command IDs, return representation, or exact dispatch API. `PValue.TryDynamicCall` already distinguishes “handled with a null value” from “not handled” (`Prexonite/PValue.cs:127-160`), so the helper is feasible, but the proposal should say it uses this non-throwing presence result and which fallback/overload failures count as “missing.” It should also state whether helpers are always registered when loading persisted matching bytecode and how CIL-disabled registration modes behave.

Specify a stable internal ABI and test `IObject`, CLR properties/fields, structures, null-valued members, unreadable/indexed properties, missing overloads, and throwing getters under both interpreters.

### m4. Named-only views meet the hard floor but significantly limit reusable view composition

The applied head receives only the subject (`proposal.md:228-247`); all parenthesized items are output subpatterns. Therefore a reusable configured view cannot receive a regex, comparer, key, or other ordinary argument. The suggested nested `view_` function works but requires generating a fresh declaration/name at every configuration site. This is a deliberate exclusion (`proposal.md:510`), not a blocker, but the extensibility section should acknowledge concrete ergonomic consequences and reserve syntax for later configured/first-class view heads so today's grammar does not preclude them.

### m5. The required test matrix misses the design's highest-risk boundaries

`proposal.md:483-499` is broad, but should additionally require:

- parse-shape tests at the relational-pattern/`&&` boundary;
- value-context and nested-expression capture scope;
- post-parse macro/hook capture mutation (or tests proving it is rejected);
- conflicting feature headers across transcluded source files and mixed initialization;
- lambdas under function-level overrides;
- both `for` condition forms with `continue`;
- writes and escaped references to capture locals before a cleared edge;
- contextual `match`/`when` identifier compatibility.

## Verified non-issues

### N1. The dual true/false flow equations support the requested negated guard clause

Swapping `T` and `F` for pattern/expression negation, installing unavailable captures as error symbols in the taken branch, and promoting slots after `if` (`proposal.md:288-332`) directly models the high-priority requirement in `prompt.md:55-71`. It does not require divergence analysis. The explicit null-on-fallthrough behavior is a defensible dynamic-language choice, provided M6 is resolved.

### N2. The active-view result contract satisfies arity tolerance

The contract at `proposal.md:228-245` correctly covers compile-time and runtime arity mismatch: missing elements are matched as `null`, excess elements are ignored, raw `null` is mismatch, `[null]` represents successful null extraction, and `[]` represents zero-output success. This meets `prompt.md:72-75` without function metadata.

### N3. Separating word and symbolic boolean tokens can be gated without changing disabled AST shape

The current lexer maps `&&`/`and`, `||`/`or`, and `!`/`not` to the same tokens (`Prexonite/Compiler/Prexonite.lex:193-194`, `218`; `Prexonite/Compiler/Grammar/Scanner.Coco.atg:90`, `107-108`), and `and` is also the current statement separator (`Parser.Statement.atg:7-16`). Splitting scanner tokens globally while having the disabled grammar construct the old nodes, as proposed at `proposal.md:49-60`, is a reasonable gated compatibility plan. Scanner/token internals are allowed to break.

### N4. The bytecode-level CIL strategy is fundamentally sound

The CIL compiler consumes `PFunction.Code`, and current bytecode already has `check_const`, `check_null`, local operations, calls, and conditional/unconditional jumps. More importantly, `StructuredExceptionHandlingCompiler.EmitJump` assesses `jump`, `jump_t`, `jump_f`, `leave`, and return variants against source/target exception regions, emitting `br`, `leave`, or `endfinally` as needed (`Prexonite/Compiler/Cil/StructuredExceptionHandling.cs:20-87`, `117-193`). Pattern lowering does not inherently require exceptions or a foreach-style region hint. The parity cases proposed at `proposal.md:452` are exactly the right risks. This does not remove the need to specify the helper ABI in m3.

### N5. Deprecating rather than immediately deleting the old macro library is compatible with the gate

`Prx/psr/impl/pattern.pxs` does implement assignment/deconstruction macros, including `kvp` (`pattern.pxs:61-122`, `186-212`). Keeping it loadable in disabled code while making enabled matching independent of its descriptors (`proposal.md:454-468`) satisfies the requested replacement direction without introducing an unnecessary unguarded source break.

## Recommended revision order

1. Resolve B1 by publishing a complete, testable grammar and precedence table.
2. Resolve B2 by freezing capture topology after parse or adopting a real post-expansion resolution phase.
3. Define BBE root/export semantics (M1), then restate the duplicate-acquisition rule (M3); these determine the flow implementation.
4. Correct the standard type-head ABI (M2) and freeze the application-wide gate across all contributing sources (M4).
5. Fill in existing-control-flow coverage (`for`, M5) and capture mutation semantics (M6).
6. Tighten contextual syntax/helper ABIs and expand the test matrix before implementation begins.
