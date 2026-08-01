# Binding boolean expressions and extensible pattern matching

Status: design proposal for an experimental, disabled-by-default language mode.

This proposal adapts Charguéraud and Lefki's [Binding Boolean Expressions and Extended Pattern Matching](bbe-and-extended-matching.pdf) to Prexonite Script. It deliberately keeps the paper's most useful idea—boolean control flow can transport bindings—while replacing its static type environments with Prexonite's symbol environments and extending the model to bindings that are available when a condition is false.

The proposed core is:

- `value is pattern` is a boolean expression that may bind names.
- `&&`, `||`, and `!` are expression-level, short-circuiting boolean operators and transport bindings between their operands.
- `and`, `or`, and `not` combine patterns.
- Bare pattern names use the existing `is_` name shift for predicates. Applied pattern names use a new `view_` shift for active patterns/views.
- Bindings are implicit when an unqualified bare name has no `is_` interpretation. `var name` is the unambiguous spelling of a binding, and `== expression` is the unambiguous spelling of a value/pinned pattern.
- Matching is compiled to ordinary locals, calls, and jumps. It does not require a runtime pattern object, exceptions, or backtracking.
- A condition is analysed for bindings on both its true and false continuations. This directly supports guard-clause code that consumes bindings obtained from a negated condition.

No unguarded source-language break is required by this design. Scanner and AST internals will change, but legacy grammar remains available when the feature is disabled.

## Feature gate and compatibility

The module/application meta key is `pattern_matching`. It defaults to `disabled`.

```pxs
pattern_matching enabled;

name example;
```

`pattern_matching enabled;` must occur in the module header, before the first non-meta construct. The existing preflight parser already reads meta assignments until that point, so dependency discovery can determine the mode without compiling the module. A later assignment to this particular key is a compile-time error rather than a mid-module mode switch.

Function-level overrides are practical and are included in the proposal:

```pxs
pattern_matching enabled;

function legacy(x) [pattern_matching disabled]
{
    return old_code(x);
}

function modern(x) [pattern_matching enabled]
{
    return x is String;
}
```

The effective mode for a function is its own `pattern_matching` meta value when present, otherwise the module value. This also applies to nested functions. Global initialization code uses the module value. Arbitrary section-level switches are deferred: function boundaries already provide a clean parser, symbol, macro, and CIL boundary, whereas a block-level switch would make the meaning of tokens and macro-produced AST depend on a transient parser state.

The scanner must always distinguish the symbolic and word spellings, even in legacy mode:

| Source | Token in the revised scanner |
| --- | --- |
| `&&` | expression conjunction |
| `||` | expression disjunction |
| `!` | expression negation |
| `and` | word `and` |
| `or` | word `or` |
| `not` | word `not` |

With matching disabled, the legacy grammar accepts both token families as the synonyms they are today and produces the existing AST. With matching enabled, only the symbolic family is accepted as boolean-expression operators, while the words are accepted only by the pattern grammar. The old `and` statement separator is also unavailable in enabled code; a semicolon must be used. Thus all source breakage from separating the operators is behind the gate.

`match` and `when`, proposed later in this document, are contextual words in matching mode rather than newly reserved keywords. Identifiers with those names remain valid in legacy code.

## Surface language

### Binding boolean expressions

`is` remains at approximately its current postfix precedence, but its right-hand side is a pattern rather than a `TypeExpr` in enabled code.

```pxs
if (value is String)                         // shifted predicate: is_String(value)
    println("a string");

if (value is [a, b] && a is > 0 && b is > a)
    println("increasing positive pair: $a, $b");

if (value is pair(a, b) && a == b)
    println("equal pair");
```

The second example intentionally uses `&&` between boolean expressions and pattern syntax only to the right of `is`. `a is > 0` is valid, although ordinary `a > 0` is shorter once `a` is in scope.

An `is` expression always produces an ordinary Prexonite boolean when used as a value. Pattern bindings do not change the runtime representation of booleans.

### Pattern grammar

The proposed grammar, omitting qualified-name details, is:

```text
Pattern       ::= PatternOr [ "when" BindingBoolean ]
PatternOr     ::= PatternAnd { "or" PatternAnd }
PatternAnd    ::= PatternUnary { "and" PatternUnary }
PatternUnary  ::= "not" PatternUnary | PatternAlias
PatternAlias  ::= PatternPrimary [ "as" Binder ]

PatternPrimary ::=
      "_"
    | Binder
    | Literal
    | RelationalPattern
    | PredicateEscape
    | PatternName
    | PatternName "(" [ Pattern { "," Pattern } ] ")"
    | "[" [ "*" Pattern | Pattern { "," Pattern } [ "," "*" Pattern ] ] "]"
    | "{" [ MemberName ":" Pattern { "," MemberName ":" Pattern } ] "}"
    | "(" Pattern ")"

Binder             ::= "var" Identifier | ImplicitBindingIdentifier
RelationalPattern  ::= ( "==" | "!=" | "<" | "<=" | ">" | ">=" ) Expression
PredicateEscape    ::= "?" "(" Expression ")"
```

The precedence, tightest first, is primary/`as`, prefix `not`, `and`, `or`, then `when`. Parentheses can always make the intended grouping explicit. In particular:

```pxs
x is not (String and > "")
x is (not String) as non_string
x is not (String as string_value)
```

The last two have different binding flow. `non_string` is bound when the negated pattern succeeds. `string_value` is bound when the negated pattern fails, because the inner `String as string_value` then succeeded.

The identifier following `as` is always a binder (an optional `var` is accepted for emphasis); it is never subjected to `is_` lookup. Existing Prexonite type arguments are accepted on shifted predicate heads, so a form such as `value is Object<"System.String">` retains the current type-expression argument grammar. Type arguments are not part of a `view_` head.

### Basic patterns

The initial pattern set is:

| Pattern | Meaning |
| --- | --- |
| `_` | Always succeeds and binds nothing. |
| `var x` | Always succeeds and binds the subject to a new local `x`. |
| `x` | A shifted predicate if `is_x` is in scope; otherwise an implicit binding. |
| `null`, `true`, `false`, numbers, strings | Compare the subject with the literal using ordinary Prexonite equality. |
| `== e`, `!= e`, `< e`, `<= e`, `> e`, `>= e` | Invoke the corresponding ordinary dynamic operator on the subject and `e`. |
| `p as x` | Match `p`, then also bind its subject as `x`. |
| `not p` | Invert success and failure, retaining bindings for the continuation on which they are valid. |
| `p and q` | Match both patterns against the same subject, left to right. Bindings from successful `p` are in scope in `q`. |
| `p or q` | Try `q` only when `p` fails. Only bindings valid on every successful route are available after the disjunction. |
| `p when b` | Match `p`, then evaluate binding boolean expression `b`; bindings from `p` are in scope in `b`. |

The relational operand is a normal expression. It ends at the surrounding pattern delimiter or a pattern `and`, `or`, or `when`; parentheses are recommended for a complicated operand.

Guards do not require parentheses around the entire binding boolean expression. In ordinary pattern contexts, the surrounding pattern syntax already provides an unambiguous boundary:

```pxs
if (x is pat(var y when (y mod 2) == 0))
    consume(y);
```

The optional `match` syntax uses `->`, not lambda's `=>`, to delimit an arm. `->` is already a distinct scanner token and is not an infix expression operator. After the guard parser has consumed a complete expression, it therefore remains as lookahead for the match-arm production. Lambda lookahead continues to own `=>` without ambiguity:

```pxs
null when null_allowed -> true
transform when accepts((x) => x + 1) -> result
_ -> x => x + 1
```

The first line parses `null_allowed` as the guard and `true` as the arm expression. The second contains a lambda inside its guard. The third has no guard and produces a lambda as the arm value. A function reference at the end of a guard is also syntactically distinguishable because it requires two thin arrows: `p when ->predicate -> result`.

Repeated acquisition of the same binding on one reachable path is an error:

```pxs
value is pair(x, x)            // error: x would be acquired twice
value is pair(x, == x)         // intended nonlinear/equality pattern
value is pair(x, y) and > 0    // x and y flow into the right pattern
```

The same logical name may be acquired in mutually exclusive `or` alternatives. Both alternatives use the same compiler-allocated capture slot:

```pxs
value is some(x) or fallback(x)
```

String and symbol lookup remain case-insensitive according to the existing language rules.

### Predicates and partial application

A bare pattern name uses the existing `is_` shift. Resolution checks only for a symbol with the shifted name; it does not inspect function metadata or require the definition to be available.

```pxs
declare function is_even;

function is_even(x) = x mod 2 == 0;

if (n is even)
    println("even");
```

This is the same mechanism that makes the existing built-in type checks work. The current compiler resolves `String` in a type-check position through `is_String`. Under this proposal, `String` is a shifted predicate pattern and lowers to `is_String(subject)`. Consequently, `x is String` retains its current runtime meaning without a dedicated `String` pattern. Prexonite type arguments and CLR-qualified type syntax remain supported by constructing the same existing type-check AST used today; CLR syntax is the one built-in special case to ordinary name shifting.

If `is_x` exists but the programmer wants a binding named `x`, `var x` forces binding. If no `is_x` exists, bare `x` is an implicit binding even when an ordinary, unshifted symbol named `x` exists. A comparison against an existing value is written `== x`. These rules make the parse depend only on symbol names already available to the compiler and avoid metadata or static-type queries.

Arbitrary predicate expressions use an explicit `?` marker:

```pxs
xs is [?(? > 0), ?(->is_acceptable), var last]
```

The expression inside `?(...)` is evaluated when the pattern reaches it, must produce a callable value, and is called with the pattern subject. Its result is interpreted exactly as an ordinary condition. `?(? > 0)` therefore reuses the existing partial-application syntax without requiring the pattern parser to guess whether ordinary parentheses contain a grouped pattern or a partial application. The shorter `(? > 0)` is not part of the initial grammar; it may be added later as an unambiguous convenience after parser experience.

The direct relational spelling is preferred for the common case:

```pxs
xs is [> 0, > 0]
```

### Views/active patterns

An applied pattern name uses the new `view_` shift on the last component of the name:

```pxs
declare function view_pair;

function view_pair(value)
{
    if (value is not Pair)
        return null;
    return [value.Left, value.Right];
}

if (value is pair(left, right))
    consume(left, right);
```

For `ns.pair(left, right)`, lookup is for `ns.view_pair`, just as the existing type-expression lookup shifts the final component in a namespace. `pair(...)` is a compile-time error when `view_pair` is not in scope; it is not reinterpreted as an ordinary function call. A forward declaration is sufficient.

The runtime view contract is intentionally small and arity-tolerant:

- The compiler calls `view_name(subject)` once.
- A `null` result means mismatch.
- A List result supplies its elements as the subjects of the subpatterns.
- Any other non-null result is treated as a one-element result.
- A missing element is supplied as `null`; extra elements are ignored.
- An empty list is a successful zero-output view.
- Subpatterns are evaluated left to right and may use bindings from preceding subpatterns.

Thus both declared and runtime arity mismatches follow normal Prexonite calling philosophy:

```pxs
value is p0(y)       // a second result, if any, is ignored
value is p0(y, z)    // z is matched against null if p0 returns only one result
```

A view that wants to successfully extract a single `null` returns `[null]`; raw `null` is reserved for mismatch. Predicate and view exceptions propagate normally. Failure of a later subpattern does not undo side effects performed by the view or earlier predicates; matching has strict, observable left-to-right evaluation, as in the paper's state-threaded semantics.

Views can capture configuration through an ordinary local or nested `view_` function. An arbitrary first-class view escape is not included initially; `?(...)` already covers arbitrary predicates, while the name convention keeps view arity and intent evident to the parser.

### List and property patterns

List patterns operate on Prexonite List values. Without a rest pattern they require exactly the written number of elements. A final `*p` accepts the remainder as a newly allocated List and matches it against `p`.

```pxs
xs is []
xs is [first, second]
xs is [head, *tail]
```

Indexing and slicing are performed by compiler-internal helpers that return `null` for a missing element rather than throwing. This rule does not weaken exact-length matching; it exists so views and list internals share the same tolerant element-access primitive.

Property patterns use ordinary dynamic member lookup and are case-insensitive in the same way as normal member access:

```pxs
arg is {Policy: {} as policy}
person is {Name: String as name, Age: >= 18}
```

`{}` succeeds for every non-null subject. A non-empty property pattern first requires a non-null subject, then retrieves and matches each named member from left to right. A missing member is a mismatch. A member whose value is `null` was still retrieved successfully and can be matched or bound as null. Exceptions thrown by an actual getter propagate; the matcher catches neither user exceptions nor general dynamic-call failures.

The compiler should implement the "member present" probe as a reserved internal command or runtime helper that uses the existing dynamic-call protocol and returns an explicit success/value pair. It must not lower property matching to a broad `try`/`catch`.

List and property forms are built-in syntax because they are common and readable, but their lowering uses the same gate/extract/subpattern operations as active views. Hash/key patterns and arbitrary enumerable list patterns are deferred; users can define views for those domains.

## Binding flow without static types

The paper assigns a map of names to types to the successful result of a binding boolean expression. Prexonite instead needs a map of logical names to already-resolved local-variable symbols, and it needs that map for both outcomes.

For an ambient capture environment `E`, analysis of a pattern or binding boolean expression produces:

```text
Flow(node, E) = (T, F)
```

`T` is the capture environment available if the node succeeds/evaluates true. `F` is the environment available if it fails/evaluates false. Either outcome may be unreachable, written `bottom`. The analysis also retains a registry of every capture slot mentioned by the whole expression.

The environments carry symbols, not values or types. A symbol therefore retains all existing information about local/global/function identity and dereferencing. A newly acquired pattern binding is an ordinary, auto-dereferenced local symbol. By-reference acquisition is not included initially.

The core rules are:

- An ordinary boolean expression has `(E, E)`. The compiler need not prove constants unreachable.
- A binding pattern `x` has `(E + x, bottom)` because acquisition cannot fail.
- `_` has `(E, bottom)`.
- A literal, relational, or predicate test has `(E, E)`.
- `not A` and `!A` swap `T` and `F`.
- For `A && B`, analyse `B` under `T(A)`. The true environment is `T(B)`. The false environment is the intersection of the reachable `F(A)` and `F(B)` routes.
- For `A || B`, analyse `B` under `F(A)`. The false environment is `F(B)`. The true environment is the intersection of the reachable `T(A)` and `T(B)` routes.
- Pattern `and` and `or` use the same equations, with both operands matching the same subject.
- A view/list/property gate has a failure route with `E`; its successful extraction route analyses subpatterns sequentially. All reachable failure routes participate in the intersection.
- `p when b` sequences `p` and `b` like conjunction.

Intersection means that a name is present only when the same capture slot is valid on every reachable route. `bottom` routes are ignored. Adding a name already valid on the same route is the repeated-acquisition error described earlier. A binder in an alternative route reuses the expression's registered slot for that logical name.

This dual-outcome formulation is a conservative extension of the paper's success-only binding map. When no construct exports a false-path binding, its true-path rules reduce to the paper's `and`-threads-bindings, `or`-intersects-bindings, and `not`-exports-nothing behavior. It also permits useful compositions that the success-only formulation cannot express: the right side of `A || B` may use a binding guaranteed by the failure of `A`, commonly because `A` contains a negated pattern.

This design does not rely on the paper's historical claims about limitations in C# pattern-variable flow. Modern C#'s ability to carry pattern bindings through `&&` and to use a binding after a negated guard clause is treated as baseline evidence for the feature, not as a missing capability to contrast against.

### Branch and guard-clause scope

An `if`/`unless` condition is parsed before its branch scopes are constructed, which is exactly the point at which its `T`, `F`, and capture registry are known.

- The `if` branch sees the condition's `T` environment.
- The `else` branch sees its `F` environment.
- `unless` swaps those branch environments.
- Every captured name not valid in a branch is installed in that branch as an error-bearing `MessageSymbol`. It does not silently fall back to an outer symbol with the same name.
- All capture slots are declared in the enclosing lexical block after the condition. They are consequently in scope after the whole `if` statement.

Prexonite does not perform definite-assignment analysis. The runtime rule is therefore that capture slots start as `null`, and every matching control-flow edge clears slots not valid on that edge back to `null`. After an `if`, a capture that was not obtained on the executed route has the value `null`.

This makes the desired guard clause direct:

```pxs
if (arg is not {Policy: {} as policy})
{
    // policy is intentionally an error symbol here: this route did not obtain it.
    return;
}

// The false result of `not` means the inner property pattern succeeded.
consume(policy);
```

No divergence check is needed. If the branch falls through instead of returning, `policy` is still in scope afterwards and is `null` on that route. This is the deliberate dynamic-language counterpart of C#'s lexical scope plus definite-assignment check and Rust's `let ... else` guard idiom.

The same rule supports `break` and `continue` guard clauses inside loops. These constructs remain bound to their existing nearest lexical loop/function; matching introduces no new labeled control target.

Bindings in a `while` condition are available only in the outcome that enters the body (`T` for `while`, `F` for `until`) and do not leak after the loop. Each condition evaluation reinitializes its capture registry. A post-condition cannot make a binding available to the body that textually precedes it. Conditional-expression branches receive their corresponding environments, but captures are not promoted beyond the conditional expression. Inside any binding boolean expression, the `&&`/`||` right operand receives the appropriate environment regardless of the expression's eventual consumer.

## Runtime semantics and lowering

Matching must not construct a dictionary of bindings at runtime. All binding names and slots are known after parsing, even though value types are not.

The root binding-boolean AST is emitted in continuation-passing form:

```text
Emit(node, ambient-environment, true-label, false-label)
```

The emitter uses ordinary Prexonite temporaries for subjects, view results, and capture slots. The important cases are:

1. Evaluate the left side of `is` once into a temporary.
2. A binder stores that temporary in its registered capture slot and jumps to success.
3. A predicate emits a normal call followed by the existing conditional jump.
4. `and`/`&&` emit the right operand only on the left success edge.
5. `or`/`||` emit the right operand only on the left failure edge.
6. `not`/`!` exchange the target labels.
7. A view call is evaluated once. `null` jumps to failure; otherwise its outputs are extracted and subpatterns are emitted left to right.
8. Before following an edge with environment `E`, slots in the root registry but absent from `E` are assigned `null`. This prevents partial captures from a failed alternative from becoming observable after the condition.

When the expression is needed as a value, the emitter converges the labels and pushes `true` or `false`, exactly as `AstLogicalAnd` and `AstLogicalOr` do today. When consumed by `if`, `while`, or another binding boolean node, it emits direct jumps and avoids materializing the boolean.

Effects are never rolled back. Slot normalization only governs compiler-created bindings; it does not undo mutations, calls, property getters, or view effects. `or` tries its right side after a left mismatch, but there is no continuation backtracking: once a pattern succeeds and a branch is selected, `return`, `break`, `continue`, a thrown exception, or an ordinary result does not resume another pattern.

## Parser, symbols, and AST

### Why immediate resolution can be retained

The current parser resolves a symbolic usage while constructing its AST node. `AstBlock` snapshots/derives a `SymbolStore`, and `AstCondition` creates the branch blocks only after parsing the condition. Replacing this with a later general name-resolution pass would be invasive and is unnecessary.

Introduce a parser-only `BindingParseContext` containing:

- the ambient symbol view;
- a case-insensitive registry from logical capture names to generated local slots/symbols;
- the capture environment valid at the current parse point;
- helpers that derive a temporary `SymbolStore` overlay for a continuation.

When the pattern parser consumes a binder, it allocates or reuses its registered slot immediately. Before parsing a dependent subexpression—the right side of `&&`, `||`, pattern `and`/`or`, a later view subpattern, or a `when` guard—it installs an overlay containing the flow-valid capture symbols. Registered captures not valid on that continuation are overlaid with error-bearing `MessageSymbol`s. Ordinary names continue to resolve through the ambient store.

Predicate and view heads are also shifted and resolved immediately. Their AST contains the resolved call/expand node, so macros and forward declarations behave like existing shifted type checks. No unresolved pattern name survives into emission.

After a condition is parsed, its capture symbols can be installed into the already-known enclosing block and the appropriate valid/error overlays used to construct `IfBlock` and `ElseBlock`. This is a local extension to parsing order, not deferred symbol resolution.

### Proposed AST shape

Enabled code should preserve patterns as first-class compiler AST until bytecode emission. A reasonable shape is:

- `AstBindingBoolean` (abstract `AstExpr`, jump-aware)
  - `AstPatternTest`
  - `AstBindingAnd`
  - `AstBindingOr`
  - `AstBindingNot`
  - a wrapper for an ordinary boolean `AstExpr`
- `AstPattern` (compiler object, not a runtime `AstExpr`)
  - bind, wildcard, literal/relational, predicate
  - and, or, not, alias, guard
  - view, list, and property
- `BindingFlow` and `PatternCapture` compiler-only annotations

`AstPatternTest` remains an `AstExpr` because `is` produces a value. A small `IAstJumpCondition` interface can generalize the direct-label facility currently hard-coded around `AstLazyLogical`; `AstCondition`, `AstWhileLoop`, legacy lazy logical nodes, and new binding boolean nodes can all use it.

The AST factory and macro bridge need constructors and inspection APIs for the new nodes. This is an allowed compiler-AST break in enabled mode. Disabled code should continue to construct `AstTypecheck`, `AstLogicalAnd`, and `AstLogicalOr` as it does now. Macro-generated pattern nodes must supply explicit capture declarations or use factory helpers that perform the same flow validation; arbitrary AST mutation after flow analysis should invalidate and recompute the annotation rather than trusting stale binding sets.

The new AST is lowered during normal `DoEmitCode`, after macro expansion has had its usual opportunity to transform nodes. It emits only existing bytecode plus a small number of reserved internal helper calls for tolerant output access and member probing.

## Optional `match` expression

No surface `switch` is proposed. Without the paper's labeled `next`/backtracking construct, it offers little beyond the existing visually flat `if`/`else if` chain. An internal branch-oriented AST is useful, but it need not force another surface statement into the language.

An optional `match` expression does remove repeated subjects and is worth adding after the core `is` implementation is stable:

```pxs
var description = match (value)
{
    {Policy: {} as policy} when policy.Active -> policy.Name;
    [a, b]                                   -> "$a:$b";
    String as text                           -> text;
    _                                        -> null;
};
```

Proposed grammar:

```text
MatchExpression ::= "match" "(" Expression ")"
                    "{" MatchArm { ";" MatchArm } [ ";" ] "}"
MatchArm        ::= Pattern "->" Expression
```

The subject is evaluated once. Arms are tested from top to bottom with strict effects. Each arm's successful environment scopes only over its right-hand expression; arm bindings neither leak into later arms nor escape the `match`. If no arm matches, the expression produces `null`, consistent with the language's default-oriented dynamic semantics. `_` supplies an explicit catch-all when a different value or effect is desired.

This lowers to a subject temporary, one failure label per arm, a result temporary, and an end label. A dedicated `AstMatchExpression` is still desirable for macros, diagnostics, and later optimization even though its bytecode is ordinary branching. There is no `next`; an arm that starts executing has been selected permanently.

A statement/block-arm form is deferred. Existing `if` chains remain the form for branches containing multiple statements or `return`/`break`/`continue`. This keeps the optional addition small and avoids inventing a new value-producing block syntax.

## CIL compilation plan

The initial implementation should target interpreted bytecode only, but the proposed lowering is intentionally CIL-compatible without a second pattern compiler.

The CIL compiler consumes `PFunction.Code`, not the source AST. By the time it runs, matching consists of:

- local loads and stores;
- existing `check_null`, `check_const`, equality/operator calls, and dynamic calls;
- `jump`, `jump_t`, and `jump_f`;
- ordinary return/break/continue instructions;
- calls to reserved pattern helper commands.

No pattern-generated `try`/`catch` is required. All pattern branches emitted within a source `try` region remain within that region. A `return`, `break`, or `continue` from a branch is already represented by the existing constructs, and `StructuredExceptionHandling` already chooses `br`, `leave`, or the required finally handling from source and target instruction addresses. Exceptions from predicates, views, or getters propagate through the source program's normal exception regions.

The first CIL-compatible implementation can compile helper commands through the ordinary command-call path. If profiling justifies it, tolerant view-output access, List shape/slice checks, and member probes can implement `ICilCompilerAware` for direct IL. This is a local optimization and does not change pattern bytecode.

A foreach-style CIL hint or an encoded compiled-pattern region is not proposed initially. Such a hint would only be useful for recognizing and optimizing a multi-instruction pattern as a unit; it is unnecessary for correctness and would couple the CIL compiler to source-pattern structure. Reconsider it only after bytecode-level optimization measurements.

The CIL parity suite must explicitly cover patterns inside `try`, `catch`, and `finally`, and guard clauses whose branches `return`, `break`, or `continue`, because these are the cases most likely to expose illegal cross-region jumps.

## Existing macro pattern library

`Prx/psr/impl/pattern.pxs` currently implements constructor/deconstructor assignment macros such as:

```pxs
kvp(var x, var y) = value;
```

That facility should be deprecated in favour of `is` plus shifted views:

```pxs
value is kvp(x, y)
```

The legacy library remains loadable while matching is disabled so existing programs continue to compile. Enabled mode does not translate its assignment syntax into new patterns, and the new compiler does not depend on its descriptor or macro-expansion protocol. Standard migration views such as `view_kvp` may be supplied as ordinary functions. The old APIs can be marked obsolete during the experiment and removed only in a separately announced compatibility release.

## Implementation sequence

1. Split word and symbolic boolean tokens. Add module/function effective-mode plumbing and legacy grammar compatibility tests.
2. Add binding parse contexts, capture allocation, dual-outcome flow, and jump-aware AST interfaces. Implement bind/wildcard/literal, shifted predicate, `and`/`or`/`not`, and `&&`/`||`/`!`.
3. Integrate branch overlays and post-`if` promotion, including error symbols and slot normalization. Add `while`/`until` and conditional-expression scopes.
4. Add aliases, relational patterns, `when`, and `?(...)` predicates.
5. Add shifted views and the tolerant output helper, followed by List and property patterns.
6. Expose the AST through factories and macro bridges, document the old macro library as deprecated, and add migration views.
7. Optionally add `AstMatchExpression` and the contextual `match` grammar.
8. Run the same semantic suite interpreted and CIL-compiled; add CIL intrinsics only if needed for performance.

The first implementation round can stop after step 5 and explicitly reject CIL compilation of matching-enabled functions until the parity work in step 8 is complete. The bytecode format itself need not be provisional.

## Required test matrix

At minimum, the implementation needs tests for:

- disabled modules preserving `is` type checks, word/symbol boolean synonyms, statement `and`, partial type checks, and the current AST;
- module enable, function opt-in, function opt-out, nested-function inheritance, and rejection of a late module switch;
- `x is String` parity with the legacy type check, including Prexonite type arguments and CLR-qualified types;
- implicit binders, forced `var` binders, forced `==` value patterns, shadowing, case-insensitive duplicates, and by-reference symbols used from guards;
- every true/false flow equation for `&&`, `||`, `!`, pattern `and`, `or`, `not`, and `when`;
- same-name bindings in exclusive alternatives and repeated-acquisition errors on a single path;
- the negative guard-clause example, unavailable-branch diagnostics, and null after an unbound fallthrough route;
- subject/view/predicate evaluation exactly once, left-to-right effects, and exception propagation;
- view results with zero, too few, exact, and excess outputs, including a successfully extracted null;
- exact and rest List patterns, empty property patterns, missing members, null-valued members, and throwing getters;
- `return`, `break`, and `continue` from branches, including inside source exception regions;
- macro inspection/construction of the enabled AST and unchanged disabled AST nodes;
- interpreted/CIL parity for the entire enabled suite when CIL support is switched on.

## Deliberate exclusions

- No `next`, labeled match, or continuation backtracking from Section 3 of the paper.
- No exhaustiveness or redundancy checking.
- No static type refinement; `String as x` does not make later member access statically typed.
- No rollback of user-visible effects on mismatch.
- No assignment/`let` pattern syntax in the first implementation. The existing macro deconstructor is deprecated rather than adopted as the core semantics.
- No surface `switch` for now.
- No arbitrary section-level feature toggles.
- No arbitrary first-class view head in the initial grammar; named shifted views and arbitrary predicate expressions cover the primary extensibility cases.

These exclusions keep the implementation centred on binding-aware control flow, user-extensible predicates/views, and predictable lowering—the parts that add capabilities not already expressible cleanly with current Prexonite constructs.
