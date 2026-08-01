# Adversarial-review triage

This triage consolidates [`review-proposal-first.md`](review-proposal-first.md) and [`review-code-first.md`](review-code-first.md). Both reviews used the committed proposal as their immutable baseline. The second review formed its model of the implementation before reading the proposal.

The reviews agree that the dual true/false flow model, tolerant named views, non-backtracking semantics, property-message dispatch, and ordinary-bytecode lowering are sound foundations. The expression grammar, capture boundaries, macro contract, hidden/public slot model, and recursive mode inheritance have since been decided.

## Applied local corrections

The following findings had a factual or locally determined resolution and have been corrected directly in [`proposal.md`](proposal.md):

| Finding | Resolution |
| --- | --- |
| Proposal-first M2 / code-first B2: built-in `is_*` invocation ABI | Predicate selection remains name-shifted, but built-in/type-capable heads retain `BuiltInTypeCommandBase`'s arity-prefix protocol and existing `PrexoniteTypeExpr`/`AstTypecheck` construction. Ordinary user predicates remain unary. |
| Proposal-first M3: duplicate acquisition after an intersection | The intended rule is flow-validity, not historical syntactic path freshness. A slot may be reacquired after an intersection made it invalid and normalized it. |
| Proposal-first M4 / code-first N2: multi-source gate stability | The effective application/module mode is frozen across all `Loader`/build-added sources and stored outside freely mutable general metadata. Conflicting later headers are errors. |
| Code-first M4: non-operator uses of `not` | Enabled mode restricts word operators only in expression/statement operator productions. Existing metadata and directive uses remain valid. |
| Proposal-first m2, contextual `match` and member names | `match` is recognized only by balanced-subject-plus-brace lookahead; otherwise it is an ordinary identifier. Member names accept identifiers and compile-time string literals. |
| Proposal-first m3 plus the follow-up property-pattern requirement | Property patterns are specified as observable zero-argument `PCall.Get` message sends through an always-registered `pattern\member\probe` engine command. The `[handled, value]` ABI distinguishes missing from handled-null. Its first CIL implementation calls `RunStatically` directly; a later custom implementation removes the temporary List allocation. |
| Code-first M2 versus proposal-first N4: CIL exception handling | The general bytecode/CIL strategy remains sound, but abrupt exits from a `finally` body are explicitly identified as the existing `ReturnFromFinally` SEH deficiency. Matching does not silently claim to repair it. |

## Accepted design decisions

The following recommendations were accepted and are now incorporated into [`proposal.md`](proposal.md):

- **D1, BBE grammar:** matching-enabled expressions use a separate symbolic `!`/`&&`/`||` ladder. Relational-pattern operands stop before enclosing `&&`/`||`; a full boolean value operand requires parentheses.
- **D2, capture roots and barriers:** each complete expression owns a private registry; only enumerated binding-aware edges transport captures; only enumerated control consumers export them. Acquisitions in either `for` condition form are rejected initially, and loop child symbol stores must be delayed or rebuilt.
- **D3, macro captures:** a macro's outward capture signature is frozen synchronously. Late expansion may use predeclared exported captures internally and may allocate hygienic expansion-private captures, but its projected true/false signature must match exactly or compilation fails.
- **D4, capture mutability:** matching uses hidden, read-only/non-addressable acquisition slots. Exporting consumers copy valid values into ordinary mutable public locals and initialize invalid routes to `null`; normalization never overwrites user locals.
- **D5, recursive mode inheritance:** effective mode is `own override > immediately enclosing effective mode > frozen module mode`, recursively through arbitrary levels of named functions and lambdas.

## Macro example and diagnostic

The proposal includes the concrete `is_generated_order` schema-macro example: an emission-time macro adds `{OrderId: order_id}`, but `order_id` in the following `&&` operand was already resolved before expansion. It now also specifies that `{OrderId: var order_id} and {LegacyOrderId: == order_id}` is legal inside an expansion. `order_id` may use a predeclared exported handle or a hygienic private handle that is projected away at the macro boundary. Missing, additional, differently routed, or physically mismatched exported captures are compile-time errors with declared-versus-actual signature diagnostics.

## Deferred or rejected findings

- Proposal-first m4, configured/first-class views: valid ergonomic limitation, already a deliberate exclusion. The named `view_` contract meets the hard extensibility requirement; reserve new syntax only when a concrete configured-view design exists.
- A new pattern bytecode region or foreach-style CIL hint remains unnecessary for correctness. The initial property helper needs no opcode, but it must be statically callable and the plan now includes a later allocation-free custom CIL implementation.
- The existing assignment/deconstructor macro library can remain available in disabled mode while being deprecated; neither review found an unguarded compatibility need to remove it immediately.
- Full test additions for parse shape, value-context barriers, multi-source gating, loops, capture mutation, property messages, recursive mode inheritance, contextual words, and macro-signature validation are now enumerated in the proposal.

## Additional risk worth retaining in the design notes

Implicit binding deliberately turns an unresolved bare pattern head into an irrefutable binder. Consequently, a misspelled predicate such as `value is Strnig` can succeed and bind `Strnig`. The proposal now includes an optional unused-implicit-binder warning. Explicit `var name` suppresses it, preserving a concise intentional escape without capitalization or metadata special cases.
