# Code-first adversarial review of the pattern-matching proposal

## Verdict

The proposal has a strong semantic core, especially its two-outcome binding-flow analysis, name-shifted views, tolerant view arity, and ordinary-bytecode lowering. It is not yet implementation-ready. Two decisions block a coherent implementation, and six major corrections are needed before the implementation sequence is reliable.

This review uses **blocker** for an unresolved point that changes the parser/symbol architecture or the callable contract of a core required feature; **major** for a material semantic, compatibility, or CIL error; and **minor** for a specification gap that can be settled locally.

## Review method and independence

I read [`prompt.md`](prompt.md) completely first. Before opening, searching, or diffing [`proposal.md`](proposal.md), I studied these parts of the implementation:

- expression, statement, global-scope, helper, scanner, and lexer grammar;
- parser-time name resolution, symbol stores, lexical blocks, condition/loop construction, and unresolved-symbol handling;
- logical/conditional AST nodes, returns, loop jumps, try/catch/finally lowering, bytecode execution, and CIL structured-exception handling;
- macro expansion, macro sessions, AST factories, and the built-in shifted type commands;
- the existing `psr/pattern` macro library and its tests;
- preflight parsing and module construction; and
- dynamic member calls and the existing interpreted/CIL test suites.

I also studied the paper's surface language and flow/lowering rules, particularly sections 2.1–2.3 and 4.1. Only after that independent code study did I read the committed proposal completely and perform this comparison. I did not modify the proposal or any implementation file.

## Blockers

### B1. The lexical boundary and lifetime of captures in an arbitrary value expression are undefined

The proposal says that `is` is an ordinary boolean-valued expression ([`proposal.md`](proposal.md), line 83), and the existing grammar places `is` in the general postfix expression grammar ([`Parser.Expression.atg`](../../Prexonite/Compiler/Grammar/Parser.Expression.atg), lines 229–263). It therefore permits a binding-producing `is` wherever an expression is permitted, not only in the consumers whose scopes the proposal defines.

The proposal specifies promotion for `if`/`unless`, special rules for `while`/`until` and conditional expressions, and operand flow through `&&`/`||` ([`proposal.md`](proposal.md), lines 309–336). It does not specify the capture root, visibility, or lifetime in cases such as:

```pxs
var matched = value is pair(a, b);
consume(value is pair(a), a);
return value is pair(a);
for (; value is pair(a); step()) consume(a);
(value is pair(a)) == true && consume(a);
```

This is architectural, not just editorial. Names are resolved while the AST is constructed ([`Parser.Code.cs`](../../Prexonite/Compiler/Parser.Code.cs), lines 923–956), so the parser must know whether each later expression child sees `a`. The proposal's statement that the right operand of `&&`/`||` receives an environment “regardless of the expression's eventual consumer” does not identify which containing node owns the registry, which expression nodes are capture barriers, or whether a capture is promoted after an ordinary value use.

The prompt specifically calls out the mismatch between parse-time resolution and expression-dependent scope ([`prompt.md`](prompt.md), lines 5–8), so this cannot safely be left to implementation convention.

**Required correction:** define a binding-expression root and a capture-barrier table for every expression-child relationship. A conservative initial rule would be:

1. Each complete `Expr` has a private capture registry.
2. Only explicitly binding-aware child edges (`&&`, `||`, `!`, pattern combinators, and guards) transport overlays inside that root.
3. An expression consumer explicitly chooses whether and how to export the root's `T`/`F` environments: `if`, `unless`, precondition loops, and conditional expressions do; assignment, return, call arguments, ordinary operators, and standalone expression statements do not.
4. `for` conditions must either receive a specified loop rule or reject acquisition in the first version.
5. The proposal must state whether grouping and other wrapper AST nodes preserve a root or create a barrier.

Any different rule is viable, but it must be complete enough that parser-time name resolution has one answer for every expression edge.

### B2. The proposed `is_String(subject)` call is incompatible with the existing built-in type-predicate ABI

The proposal says a bare predicate is selected by shifted name alone and lowers `String` to `is_String(subject)` ([`proposal.md`](proposal.md), lines 179–192). Existing built-in `is_*` commands do not accept that call shape. `BuiltInTypeCommandBase` requires its first invocation argument to be a constant type-argument arity, then consumes the subject after those type arguments ([`BuiltInTypeCommandBase.cs`](../../Prexonite/Compiler/Macro/Commands/BuiltInTypeCommandBase.cs), lines 73–118; [`CheckIsBuiltIn.cs`](../../Prexonite/Compiler/Macro/Commands/CheckIsBuiltIn.cs), lines 14–23). The current type grammar correspondingly inserts that arity before type arguments and before the subject ([`Parser.Expression.atg`](../../Prexonite/Compiler/Grammar/Parser.Expression.atg), lines 508–588).

As written, `is_String(subject)` makes the subject occupy the arity slot and macro expansion fails. Type arguments add a second contradiction: the proposal says lookup needs only a name and supports forward declarations, but the current parser determines type-argument support by resolving the macro/function entity and inspecting its type or metadata ([`BuiltInTypeCommandBase.cs`](../../Prexonite/Compiler/Macro/Commands/BuiltInTypeCommandBase.cs), lines 23–71). This directly jeopardizes the requested `x is String` compatibility ([`prompt.md`](prompt.md), lines 22–24) and the required name-only extensibility contract (lines 29–33).

**Required correction:** distinguish symbol selection from invocation ABI. Name shifting can remain metadata-free, but the proposal must choose and document one of these implementation contracts:

- preserve the current `TypeExpr` construction for legacy built-in type predicates, including the arity prefix, while ordinary user predicates receive only `subject`; explicitly acknowledge that type-argument capability is the legacy entity/metadata-aware exception; or
- introduce a uniform new predicate adapter/command ABI and migrate all built-in `is_*` symbols to it.

The first option is much smaller and preserves existing partial and CLR type-check behavior, but it is not the literal direct call currently specified.

## Major findings

### M1. Late macro expansion cannot repair or recompute parse-time capture scope

The proposal permits arbitrary pattern-AST mutation to invalidate and recompute flow annotations, then says lowering occurs after macros have had their normal expansion opportunity ([`proposal.md`](proposal.md), lines 400–402). In the current compiler, normal macro expansion occurs inside `AstExpand.DoEmitCode`; the expansion result is immediately emitted ([`AstExpand.cs`](../../Prexonite/Compiler/AST/AstExpand.cs), lines 30–50). It is not a whole-tree phase before dependent source expressions and branch bodies are parsed and resolved.

Moreover, child blocks snapshot a derived symbol store, and replacing a parent block's store explicitly does not affect existing nested scopes ([`AstBlock.cs`](../../Prexonite/Compiler/AST/AstBlock.cs), lines 25–51). Recomputing `T`/`F` sets after a macro changes the capture interface cannot rebind the already-constructed `AstReference`s in the right operand, branch, or loop body.

**Required correction:** freeze every source-visible capture declaration and physical slot before dependent source is parsed. Macro APIs may inspect and rewrite pattern internals only if they preserve that capture interface. A factory-created pattern that exports bindings must declare its complete capture signature synchronously during parsing. Supporting a late macro that adds/removes outward bindings would require a genuine pre-resolution macro phase plus name re-resolution, which contradicts the proposal's stated architecture and should be deferred.

### M2. The CIL/finally claim exceeds what `StructuredExceptionHandling` supports

The proposal says existing structured-exception handling already chooses the needed `br`, `leave`, or finally behavior for `return`, `break`, and `continue`, and requires parity tests inside exception regions ([`proposal.md`](proposal.md), lines 434–452 and 497–499). That is only partly true.

Leaving a `try` or `catch` is supported with `leave`, but a jump from a `finally` to outside its region is valid only for the block's natural `EndTry`; otherwise it is classified as invalid ([`StructuredExceptionHandling.cs`](../../Prexonite/Compiler/Cil/StructuredExceptionHandling.cs), lines 376–406). The existing `ReturnFromFinally` test explicitly expects a structured-exception-handling deficiency ([`CilCompilerTests.cs`](../../PrexoniteTests/Tests/CilCompilerTests.cs), lines 647–672).

Pattern-internal branches that stay within a `finally` are likely fine. An abrupt `return`, `break`, or `continue` executed from that region is not made safe merely because the pattern lowered to ordinary jumps.

**Required correction:** split the parity promise into (a) pattern matching inside each region, (b) abrupt exits from `try`/`catch`, and (c) abrupt exits from `finally`. Mark (c) as an existing CIL limitation unless this feature also undertakes a nontrivial pending-control-flow lowering for finally blocks. Do not make full CIL parity contingent on a test the current compiler deliberately fails without adding that work to the implementation plan.

### M3. Nested-function mode inheritance contradicts the required test matrix

The effective-mode rule says a function uses its own override or the **module** value, including nested functions ([`proposal.md`](proposal.md), line 47). The test matrix instead requires “nested-function inheritance” (line 488). These differ whenever an outer function overrides the module and a nested function has no override.

Current nested/lambda creation does not provide a general meta-inheritance mechanism; for example, lambdas explicitly copy only selected metadata such as the import key ([`Parser.Expression.atg`](../../Prexonite/Compiler/Grammar/Parser.Expression.atg), lines 418–424). The implementation therefore needs an intentional rule, not an assumed current behavior.

**Required correction:** use `own override ?? enclosing function's effective mode ?? module mode`, or rename the test to “nested functions fall back to module mode.” Lexical inheritance is the less surprising choice and makes an opt-in outer function internally consistent.

### M4. “Word operators only in pattern grammar” breaks existing enabled-mode meta syntax

Splitting symbolic and word tokens is necessary because the lexer currently maps `&&`/`||` to the same `and`/`or` tokens used by the word spellings ([`Prexonite.lex`](../../Prexonite/Compiler/Prexonite.lex), lines 193–194, together with the scanner keyword table). However, the proposal's enabled-mode rule says `and`, `or`, and `not` words are accepted only by pattern grammar ([`proposal.md`](proposal.md), lines 49–60).

`not` is also existing global/function metadata syntax, including `not key` and `is not key` ([`Parser.GlobalScope.atg`](../../Prexonite/Compiler/Grammar/Parser.GlobalScope.atg), lines 48–64). A literal application of the proposed restriction would reject unrelated metadata as soon as pattern mode is enabled, violating the claim that all source breakage is the intended boolean/statement-separator break behind the gate.

**Required correction:** restrict word tokens only as operators in enabled **local expression and statement** productions. Preserve their non-operator grammatical uses, including metadata, assembler/directive grammar, and any qualified/member identifier positions already accepted. Add enabled-mode compatibility tests for those contexts.

### M5. Existing loop construction invalidates the proposal's assumption that condition captures can simply be installed for the body

The parser creates an `AstWhileLoop` before parsing its condition and pushes the already-created body scope only afterwards ([`Parser.Statement.atg`](../../Prexonite/Compiler/Grammar/Parser.Statement.atg), lines 396–415). `AstLoop` constructs that body immediately from a snapshot of the parent symbol store ([`AstLoop.cs`](../../Prexonite/Compiler/AST/AstLoop.cs), lines 3–24), and parent-store replacement does not update child scopes ([`AstBlock.cs`](../../Prexonite/Compiler/AST/AstBlock.cs), lines 45–51).

That differs from `if`, where the condition is parsed before `AstCondition` and its branch blocks are constructed ([`Parser.Statement.atg`](../../Prexonite/Compiler/Grammar/Parser.Statement.atg), lines 373–393). The proposal recognizes the `if` ordering but then promises loop-body flow without specifying this different construction path ([`proposal.md`](proposal.md), lines 309 and 336).

**Required correction:** delay loop body-scope creation until the condition's capture registry is known, or deliberately replace/derive the body symbol store with the correct valid/error overlay before parsing the body. Specify `for` initializer, condition, next-iteration, body, and post-condition loop edges separately; their current scopes and textual order are not equivalent to `while`.

### M6. Edge normalization does not implement per-evaluation loop reinitialization

The proposal says each `while` condition evaluation reinitializes its capture registry ([`proposal.md`](proposal.md), line 336), but the lowering only clears slots absent from an outgoing environment immediately before following an edge (lines 348–357). On a successful iteration, captures valid on the body edge remain populated when control returns to the next condition evaluation.

Usually the next binder overwrites them before direct use, but stale values remain observable through a closure or callback created in the preceding body and invoked by an earlier side effect in the next condition. This is especially relevant in a dynamic, effectful language where predicates and views are arbitrary calls.

**Required correction:** emit a clear of the root registry on entry to every repeated condition evaluation, before any condition operand, predicate, getter, or view can run. Keep outgoing-edge normalization as well, because it clears partial captures on failed alternatives and controls values visible after the consumer.

## Minor findings

### N1. The grammar is not yet concrete enough to implement without inventing language rules

The proposal labels its grammar as omitting qualified-name details ([`proposal.md`](proposal.md), lines 85–111), but the omitted pieces decide real parses: `PatternName`, `MemberName`, CLR-qualified heads, type arguments, contextual `match`/`when`, escaped keyword/member names, and where an unparenthesized relational operand stops. The existing `TypeExpr` grammar uses semantic predicates and shifted namespace lookup for these cases ([`Parser.Expression.atg`](../../Prexonite/Compiler/Grammar/Parser.Expression.atg), lines 476–588).

**Correction:** add parser-level productions or explicit lookahead/semantic-predicate rules, particularly for `match(...)` versus an ordinary call named `match`, `when` as a delimiter versus an identifier, and `<...>` type arguments versus relational pattern syntax. This can remain an appendix, but it should precede parser work.

### N2. The gate's freeze point across the build pipeline needs one precise ownership rule

The existing preflight grammar aborts at the first non-meta/global-variable construct ([`Parser.GlobalScope.atg`](../../Prexonite/Compiler/Grammar/Parser.GlobalScope.atg), lines 20–45), which makes a leading switch feasible. The proposal does not yet say which object owns the frozen mode when a module is assembled from multiple sources/build fragments, how duplicate leading assignments are handled, or how a later assignment is diagnosed when ordinary meta assignments currently remain legal during the full parse.

**Correction:** store a frozen module mode in the build/compiler target after preflight, require every parser instance for that module to use it, and diagnose any later write to that key independently of its value. This is a local design addition, not a reason to abandon preflight gating.

## Requirements coverage

| Prompt requirement | Coverage in proposal | Review result |
| --- | --- | --- |
| 1. Disabled-by-default module/application gate | Covered at lines 19–30 | Feasible; define the frozen owner across build fragments (N2). |
| 1.1 Function/section override if practical | Function overrides included | Resolve nested inheritance contradiction (M3). Deferring arbitrary sections is reasonable. |
| 1.2 Enabled AST may break; disabled AST preferably stable | Covered at lines 382–402 | Disabled shape is preserved in intent; late macro recomputation is not implementable as stated (M1). |
| 1.3 Replace/deprecate existing macro patterns | Covered at lines 454–468 | Sound migration boundary. |
| 2. Repurpose `is`; aspire to `String` parity | Covered in intent at lines 68–83 and 177–192 | Existing built-in ABI contradicts the stated lowering (B2). |
| 3. Dynamic-language symbol/binding flow | Dual `T`/`F` environments are detailed | Strong core, but capture roots and arbitrary expression edges are incomplete (B1). |
| 4–5. Name-shifted, metadata-independent predicates/views | `is_` and `view_` contracts are included | Views satisfy the requirement; predicate selection is sound, but legacy type-argument capability is not name-only (B2). |
| 6. CIL compatibility plan | Ordinary-bytecode plan included | Broadly right; finally exits are overclaimed (M2). |
| 7. Separate expression and pattern operators | Covered and correctly oriented | Scanner split is necessary; preserve non-operator word uses (M4). |
| 8. No backtracking; support unlabeled abrupt control | Explicitly covered | Runtime semantics are sound; CIL-from-finally remains an existing gap (M2). |
| 9. Optional concrete `match`; no need for `switch` | Concrete `match` expression proposed | Good scope; contextual parser details remain (N1). |
| 10. Implicit bindings | Covered with `var` and `==` escape hatches | Good resolution rule; its outward scope still depends on B1. |
| 11. Existing partial application | Covered through explicit `?(...)` | Meets the prompt's acceptable annotated form. |
| 12. Bindings from negative patterns | Dual-outcome analysis and guard promotion covered | One of the proposal's strongest parts; needs capture-root completion and loop fixes. |
| 13. Arity tolerance and runtime missing outputs | Covered at lines 228–245 | Meets the requirement, including distinguishing mismatch `null` from successful `[null]`. |
| 14. Do not rely on obsolete C# limitations | Explicitly acknowledged at lines 303–305 | Covered. |

Overall, every requested feature is addressed in intent. The misses are not feature omissions so much as contradictions at the exact seams the prompt identified as risky: parser-time scope, legacy shifted type macros, and CIL exception regions.

## Confirmed non-issues and sound choices

- The two-outcome `(T, F)` flow equations, bottom-route handling, and intersection at disjunction are coherent. They extend the paper in the right direction for negated guard bindings.
- Direct lowering to locals, calls, and jumps is appropriate. Since the CIL compiler consumes bytecode, no foreach-style pattern hint is required for correctness; the exception is the pre-existing abrupt-exit limitation described in M2.
- Splitting word and symbolic operator tokens is required; the current lexer conflates the spellings.
- The property-presence helper is feasible without broad exception handling: [`PValue.TryDynamicCall`](../../Prexonite/PValue.cs) returns a success flag separately from the retrieved value (lines 136–160), so missing and successfully retrieved `null` can be distinguished.
- The strict, non-backtracking view semantics and tolerant missing-output rule fit existing Prexonite calling conventions and prompt requirement 13.
- Keeping the legacy assignment/deconstructor macro library available only in disabled mode is a credible compatibility and migration plan.
- Deferring surface `switch`, labeled `next`, backtracking, exhaustiveness, and first-class arbitrary view heads keeps the first implementation focused without violating the prompt.

## Recommended correction order

1. Specify binding-expression roots, capture barriers, consumer export/promotion, and all loop forms (B1, M5).
2. Correct and test the shifted predicate/type-check ABI, including zero/type-argument calls and forward declarations (B2).
3. Freeze the capture interface before dependent parsing and narrow the macro contract (M1).
4. Define nested effective-mode inheritance and the exact preflight freeze owner (M3, N2).
5. Amend token restrictions to preserve non-operator grammar and make contextual productions parser-ready (M4, N1).
6. Add loop-entry clearing and narrow or extend the CIL/finally promise (M6, M2).

After those corrections, the proposed dual-flow model and lowering strategy form a credible implementation design.
