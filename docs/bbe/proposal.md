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

`pattern_matching enabled;` must occur in the module header, before the first non-meta construct. The existing preflight parser already reads meta assignments until that point, and the full parser therefore sees the switch before it reaches mode-dependent syntax. `PreflightResult` does not currently retain arbitrary meta keys; the implementation must add a separately stored, frozen effective mode rather than repeatedly consulting mutable application metadata. A later assignment to this particular key is a compile-time error rather than a mid-module mode switch.

An application/module may be assembled from more than one source through `Loader` and build-time `add(...)`. The first contributing source that explicitly supplies `pattern_matching`, or the first non-header construct if none does, freezes the application's effective mode. Later contributing sources inherit that mode. Repeating the same explicit value is allowed; supplying a conflicting value is a compile-time error even when it appears at the top of the later source. This prevents transclusion order from creating accidental section-level mode switches. The frozen value must live outside the freely mutable general meta table and must be used by every parser and compiler target contributing to the application.

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

The effective mode is inherited recursively. A function uses its own `pattern_matching` meta value when present; otherwise it inherits the effective mode of its immediately enclosing function; only a top-level function falls back to the frozen module value. This rule applies through arbitrary levels of named nested functions and lambdas. A lambda has no surface override of its own, so it always inherits its immediately enclosing effective mode. Global initialization code uses the module value. Arbitrary section-level switches are deferred: function boundaries already provide a clean parser, symbol, macro, and CIL boundary, whereas a block-level switch would make the meaning of tokens and macro-produced AST depend on a transient parser state.

The scanner must always distinguish the symbolic and word spellings, even in legacy mode:

| Source | Token in the revised scanner |
| --- | --- |
| `&&` | expression conjunction |
| `||` | expression disjunction |
| `!` | expression negation |
| `and` | word `and` |
| `or` | word `or` |
| `not` | word `not` |

With matching disabled, the legacy grammar accepts both token families as the synonyms they are today and produces the existing AST. With matching enabled, only the symbolic family is accepted as boolean-expression operators, while the words are accepted as pattern operators. The restriction applies to operator and statement productions, not to unrelated existing uses of those tokens: metadata forms such as `not key` and `is not key`, assembler/directive syntax, and accepted keyword/member-name positions retain their current meanings. The old `and` statement separator is unavailable in enabled code; a semicolon must be used. Thus all source breakage from separating the operators is behind the gate.

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

### Binding-boolean and pattern grammar

Matching-enabled code replaces the current lazy `OrExpr`/`AndExpr` tier with a separate binding-boolean precedence ladder. The expression tiers outside that point remain unchanged. `NonBbeExpression` denotes the existing `DeltaExpr`-and-tighter grammar, with postfix `is Pattern` in place of legacy `is TypeExpr` and with symbolic `!` factored into `BbeUnary`. A parenthesized primary recursively parses a complete `Expr`, so parentheses deliberately re-enter the full grammar.

The proposed grammar, omitting the unchanged expression tiers and qualified-name details, is:

```text
KeyValuePairExpr ::= BindingBoolean [ ":" KeyValuePairExpr ]

BindingBoolean ::= BbeOr
BbeOr          ::= BbeAnd { "||" BbeAnd }
BbeAnd         ::= BbeUnary { "&&" BbeUnary }
BbeUnary       ::= "!" BbeUnary | BbeOperand
BbeOperand     ::= NonBbeExpression

# NonBbeExpression contains postfix matching:
PostfixMatch   ::= PrefixExpression { "is" Pattern | OtherPostfix }

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
RelationalPattern  ::= ( "==" | "!=" | "<" | "<=" | ">" | ">=" ) RelationalOperand
RelationalOperand  ::= NonBbeExpression
PredicateEscape    ::= "?" "(" Expression ")"
MemberName         ::= Identifier | StringLiteral
```

Thus the binding-boolean precedence, from tightest to loosest, is the existing non-BBE operators and postfix matching, symbolic `!`, symbolic `&&`, then symbolic `||`. Pattern precedence remains primary/`as`, word `not`, word `and`, word `or`, then `when`. The two ladders meet only through postfix `is Pattern`, a `when` guard, predicate escapes, and explicit parentheses.

Most importantly, an unparenthesized relational-pattern operand cannot consume an enclosing `&&` or `||`:

```pxs
a is > 0 && b is > a        // (a is > 0) && (b is > a)
a is == (flag && enabled)   // compare a with the full parenthesized BBE
```

Pattern delimiters such as comma, `)`, `]`, and `}` and the match-arm `->` also terminate the applicable production. A `when` guard consumes `BindingBoolean`; a full lower-precedence expression inside a guard can be parenthesized. Parse-shape tests, rather than arbitrary stop-token lookahead, are required at every boundary.

A string-literal member name is compile-time syntax for sending that exact message and permits keywords or punctuation that cannot be written as an identifier. It is not evaluated as an expression.

Parentheses can always make pattern grouping explicit. In particular:

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

The relational operand is a `NonBbeExpression`: it supports the existing non-lazy expression operators but stops before an enclosing symbolic `&&` or `||`. Parentheses are required when the value being compared is itself a full binding-boolean expression.

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

Acquiring a binding whose slot is already valid on the current continuation is an error:

```pxs
value is pair(x, x)            // error: x would be acquired twice
value is pair(x, == x)         // intended nonlinear/equality pattern
value is pair(x, y) and > 0    // x and y flow into the right pattern
```

The same logical name may be acquired in mutually exclusive `or` alternatives. Both alternatives use the same compiler-allocated capture slot:

```pxs
value is some(x) or fallback(x)
```

An intersection can also make a slot invalid and therefore eligible for later reacquisition. For example, `(left(x, y) or right(x)) and again(y)` is legal: `y` is not valid after the `or`, its slot is normalized, and `again(y)` reacquires it. This flow-valid rule, rather than syntactic occurrence along a historical route, defines duplicate acquisition.

String and symbol lookup remain case-insensitive according to the existing language rules.

### Predicates and partial application

A bare pattern name uses the existing `is_` shift. Resolution checks only for a symbol with the shifted name; it does not inspect function metadata or require the definition to be available. Invocation is a separate concern: ordinary user predicates use a unary subject call, while the existing built-in type macros retain their established hidden-arity ABI.

```pxs
declare function is_even;

function is_even(x) = x mod 2 == 0;

if (n is even)
    println("even");
```

This is the same selection mechanism that makes the existing built-in type checks work, but not literally the same call shape as an ordinary predicate. The current compiler resolves `String` in a type-check position through the expanding `is_String` macro. `BuiltInTypeCommandBase` requires a constant type-argument arity first, followed by those type arguments and then the subject; a zero-argument `String` check is therefore invoked in the equivalent of `is_String(0, subject)`, not `is_String(subject)`. Enabled parsing must reuse the existing `PrexoniteTypeExpr`/`AstTypecheck` construction for a resolved built-in or type-argument-capable head, including this arity prefix. Ordinary shifted user predicates receive only the subject. Consequently, `x is String` retains its current runtime meaning, and Prexonite type arguments and CLR-qualified type syntax retain their current lowering. Selecting predicate versus binder remains name-only; recognizing the legacy type-argument invocation ABI is the one entity/metadata-aware compatibility exception. A forward-declared custom predicate cannot use `<...>` unless its declaration exposes the existing capability metadata when parsed.

If `is_x` exists but the programmer wants a binding named `x`, `var x` forces binding. If no `is_x` exists, bare `x` is an implicit binding even when an ordinary, unshifted symbol named `x` exists. A comparison against an existing value is written `== x`. These rules make the parse depend only on symbol names already available to the compiler and avoid metadata or static-type queries.

#### Optional unused implicit-binder diagnostic

Implicit binding is concise but makes a misspelled predicate such as `value is Strnig` an irrefutable binding instead of an unknown-name error. An optional `warn_unused_pattern_bindings` compiler/module/function diagnostic mitigates this without adding capitalization rules or weakening name-only extensibility.

When enabled, the compiler warns if a logical binding introduced only through the implicit spelling has no source-level read on any flow-valid continuation or exported consumer scope. Reads in a later subpattern, `when` guard, `&&`/`||` operand, selected branch, match arm, or after an exporting `if` all count. The diagnostic is computed only after the enclosing lexical block/function has been parsed, so a guard-clause binding used after the `if` is not reported prematurely. Alternative acquisitions sharing one logical slot produce at most one diagnostic for that binding root.

The explicit spelling `var name` suppresses this warning and documents intentional acquisition; `_` remains the spelling for an intentionally discarded value. The diagnostic is advisory, disabled by default during the experiment, and does not change flow or emitted code. It may become enabled by default after experience with existing DSLs and macro-generated source.

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

Property patterns send ordinary zero-argument get messages and are case-insensitive in the same way as normal member access:

```pxs
arg is {Policy: {} as policy}
person is {Name: String as name, Age: >= 18}
```

`{}` succeeds for every non-null subject. A non-empty property pattern first requires a non-null subject, then sends each named member message and matches its result from left to right. A message is sent exactly once for each written member occurrence. If it is not handled, the pattern mismatches. If it is handled with a `null` result, that value can still be matched or bound as null. Sending a message may run arbitrary user code; its effects remain observable even if this or a later subpattern fails. Exceptions thrown while handling the message propagate. The matcher catches neither user exceptions nor general dynamic-call failures, and it performs no reflection-only preflight that could suppress the send.

The initial implementation uses an always-registered engine command with the stable internal id `pattern\member\probe`. It accepts exactly `(subject, memberName)`, converts the compiler-supplied member name to a string, and calls:

```csharp
subject.TryDynamicCall(sctx, ReadOnlySpan<PValue>.Empty,
    PCall.Get, memberName, out var value)
```

The command returns the two-element List `[handled, value]`; on an unhandled message it returns `[false, null]`, and on a handled message it returns `[true, value]`. Thus a handled null remains distinct from a missing member. "Missing" means precisely that this zero-argument `PCall.Get` returns `false`, including when no compatible zero-argument getter/overload handles the message. If the call returns `true`, any effects have already happened. If it throws, the helper does not intercept the exception.

The pattern emitter stores the command result in a temporary, branches to mismatch when element zero is false, and otherwise matches the member subpattern against element one. The command is an engine command rather than a compiler/build command so persisted bytecode can execute without compiler services. Interpreted bytecode uses an ordinary `cmd` call.

The initial helper must already provide a statically callable CIL implementation:

```csharp
public static PValue RunStatically(StackContext sctx, PValue[] args)
```

Its instance `Run` delegates to the same implementation. The command implements `ICilCompilerAware`; `CheckQualification` returns `CompilationFlags.PrefersRunStatically`, allowing `CompilerState.EmitEarlyBoundCommandCall` to emit a direct call to `RunStatically` instead of `Runtime.CallCommand`. Thus the first CIL version avoids command-table lookup and indirect dispatch without a new opcode. A later custom `ICilCompilerAware` implementation may call a lower-level runtime probe and branch on its handled flag directly, eliminating the temporary two-element List allocation while preserving the command ABI and observable message-send semantics. The helper must not be implemented using a broad `try`/`catch`.

List and property forms are built-in syntax because they are common and readable, but their lowering uses the same gate/extract/subpattern operations as active views. Hash/key patterns and arbitrary enumerable list patterns are deferred; users can define views for those domains.

## Binding flow without static types

The paper assigns a map of names to types to the successful result of a binding boolean expression. Prexonite instead needs a map of logical names to already-resolved local-variable symbols, and it needs that map for both outcomes.

For an ambient capture environment `E`, analysis of a pattern or binding boolean expression produces:

```text
Flow(node, E) = (T, F)
```

`T` is the capture environment available if the node succeeds/evaluates true. `F` is the environment available if it fails/evaluates false. Either outcome may be unreachable, written `bottom`. The analysis also retains a registry of every capture slot mentioned by the whole expression.

The environments carry symbols, not values or types. A symbol therefore retains all existing information about local/global/function identity and dereferencing. During matching, a newly acquired binding is stored in a hidden compiler-owned, auto-dereferenced acquisition slot. Its source name resolves through a read-only, non-addressable capture symbol while the BBE is still being evaluated. It cannot be assigned, incremented, or passed by reference there. A binding-aware consumer copies valid hidden values into ordinary user-visible locals at its continuation boundary. By-reference acquisition is not included initially.

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

### Binding-expression roots and barriers

Every complete matching-enabled `Expr` owns a private capture registry. Parentheses are transparent and preserve the same root. Only explicitly binding-aware child edges transport capture environments inside that root:

- the right operand of symbolic `&&` receives the left operand's true environment;
- the right operand of symbolic `||` receives the left operand's false environment;
- symbolic `!` swaps environments;
- pattern `and`, `or`, `not`, later view/list/property subpatterns, and `when` use the pattern-flow rules above; and
- a successful `match` arm transports its pattern environment into that arm expression.

All other expression-child relationships are capture barriers. Assignment, return, call arguments, separate arguments of the same call, ordinary unary/binary operators, collection elements, and expression statements neither receive a sibling's captures nor export captures to following source. An `is` below such a barrier still evaluates and returns an ordinary boolean, and its private bindings may still be used inside its own pattern and guards, but their names are not promoted outside it.

For example:

```pxs
var matched = value is pair(a, b);      // boolean value; a and b do not escape
consume(value is pair(c), c);           // error: call arguments are barriers
(value is pair(d)) == true && use(d);   // error: equality is a barrier
value is pair(e) && use(e);             // valid binding-aware edge
```

A consumer must explicitly opt into exporting a root environment. The initial exporting consumers are `if`/`unless`, precondition `while`/`until`, and the two branches of a conditional expression. `match` exports only into the selected arm expression. Plain value consumers never export. This table is closed rather than inferred from an AST node eventually using jump emission, so parser-time symbol resolution always has one answer.

Both existing `for` condition forms may contain capture-free pattern tests, but the initial implementation rejects a condition that acquires or exports a binding. A precondition `for` could later expose successful captures to both its body and next-iteration block. A post-condition `for` needs a separate rule because its first next-iteration execution textually and dynamically precedes its first condition evaluation. Deferring both avoids silently choosing different lexical and first-iteration meanings.

The parser must delay construction of, or deliberately replace, a loop child `SymbolStore` before parsing a body that receives condition captures. Existing `AstLoop` objects create their body scopes before parsing the condition, so merely adding names to the parent store afterwards is insufficient.

### Branch and guard-clause scope

An `if`/`unless` condition is parsed before its branch scopes are constructed, which is exactly the point at which its `T`, `F`, and capture registry are known.

- The `if` branch sees the condition's `T` environment.
- The `else` branch sees its `F` environment.
- `unless` swaps those branch environments.
- Every captured name not valid in a branch is installed in that branch as an error-bearing `MessageSymbol`. It does not silently fall back to an outer symbol with the same name.
- Ordinary public locals for exported capture names are declared in the enclosing lexical block after the condition. They are consequently in scope after the whole `if` statement.

On the edge into a branch, every valid hidden acquisition slot is copied into its public local and every invalid public local is initialized to `null`. The branch overlay resolves valid names to those ordinary locals and invalid names to error symbols. User code in the branch may assign to, increment, or reference a valid public local normally. Those writes are not part of matcher normalization and remain observable after the branch.

Prexonite does not perform definite-assignment analysis. After an `if`, an exported capture that was not obtained on the executed route therefore exists with value `null`; one obtained on that route has its copied value, including any subsequent branch mutation. Hidden acquisition slots remain inaccessible outside matching and may be cleared freely.

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

No divergence check is needed. If the branch falls through instead of returning, `policy` is still in scope afterwards and is `null` on that route. If a route obtained and then modified `policy` in user branch code, that modification remains. This is the deliberate dynamic-language counterpart of C#'s lexical scope plus definite-assignment check and Rust's `let ... else` guard idiom.

The same rule supports `break` and `continue` guard clauses inside loops. These constructs remain bound to their existing nearest lexical loop/function; matching introduces no new labeled control target.

Bindings in a precondition `while`/`until` are copied into body-scoped ordinary locals only on the outcome that enters the body (`T` for `while`, `F` for `until`) and do not leak after the loop. Each condition evaluation clears its hidden registry before evaluating any condition effect, then recopies successful values on body entry. A post-condition cannot make a binding available to the body that textually precedes it. Conditional-expression branches receive branch-scoped ordinary copies of their corresponding environments, but captures are not promoted beyond the conditional expression. Inside a binding boolean expression, the `&&`/`||` right operand receives the appropriate read-only hidden capture environment regardless of the expression's eventual consumer.

## Runtime semantics and lowering

Matching must not construct a dictionary of bindings at runtime. All binding names and slots are known after parsing, even though value types are not.

The root binding-boolean AST is emitted in continuation-passing form:

```text
Emit(node, ambient-environment, true-label, false-label)
```

The emitter uses ordinary Prexonite temporaries for subjects and view results, hidden acquisition slots during matching, and ordinary public locals only at exporting consumer boundaries. The important cases are:

1. On entry to a root evaluation, clear every hidden slot before evaluating any predicate, view, getter, or other user effect.
2. Evaluate the left side of `is` once into a temporary.
3. A binder stores that temporary in its registered hidden slot and jumps to success.
4. A predicate emits a normal call followed by the existing conditional jump.
5. `and`/`&&` emit the right operand only on the left success edge.
6. `or`/`||` emit the right operand only on the left failure edge.
7. `not`/`!` exchange the target labels.
8. A view call is evaluated once. `null` jumps to failure; otherwise its outputs are extracted and subpatterns are emitted left to right.
9. Before following an internal edge with environment `E`, hidden slots in the root registry but absent from `E` are assigned `null`. This prevents partial captures from a failed alternative from reaching a later matcher continuation.
10. At an exporting consumer edge, copy valid hidden slots into the corresponding public locals and assign `null` to invalid public locals before entering user code.

When the expression is needed as a value, the emitter converges the labels and pushes `true` or `false`, exactly as `AstLogicalAnd` and `AstLogicalOr` do today. When consumed by `if`, `while`, or another binding boolean node, it emits direct jumps and avoids materializing the boolean.

Effects are never rolled back. Hidden-slot normalization cannot overwrite user state because in-progress capture symbols are read-only and non-addressable; ordinary public locals are copied only at consumer boundaries and are never normalized afterwards. Calls, property getters, view effects, and mutations in selected user branches remain observable. `or` tries its right side after a left mismatch, but there is no continuation backtracking: once a pattern succeeds and a branch is selected, `return`, `break`, `continue`, a thrown exception, or an ordinary result does not resume another pattern.

## Parser, symbols, and AST

### Why immediate resolution can be retained

The current parser resolves a symbolic usage while constructing its AST node. `AstBlock` snapshots/derives a `SymbolStore`, and `AstCondition` creates the branch blocks only after parsing the condition. Replacing this with a later general name-resolution pass would be invasive and is unnecessary.

Introduce a parser-only `BindingParseContext` containing:

- the ambient symbol view;
- a case-insensitive registry from logical capture names to hidden acquisition slots and read-only/non-addressable capture symbols;
- the capture environment valid at the current parse point;
- helpers that derive a temporary `SymbolStore` overlay for a continuation.

When the pattern parser consumes a binder, it allocates or reuses its registered slot immediately. Before parsing a dependent subexpression—the right side of `&&`, `||`, pattern `and`/`or`, a later view subpattern, or a `when` guard—it installs an overlay containing the flow-valid capture symbols. Registered captures not valid on that continuation are overlaid with error-bearing `MessageSymbol`s. Ordinary names continue to resolve through the ambient store.

Predicate and view heads are also shifted and resolved immediately. Their AST contains the resolved call/expand node, so macros and forward declarations behave like existing shifted type checks. No unresolved pattern name survives into emission.

After an exporting condition is parsed, ordinary public locals can be declared in the already-known enclosing block. The appropriate valid/error overlays are then used to construct `IfBlock` and `ElseBlock`, with edge prologues copying hidden values or `null` into those locals. Non-exporting value contexts discard the private registry after their expression. Loop child stores must be delayed or rebuilt as described above. This is a local extension to parsing order, not deferred symbol resolution.

### Frozen macro signatures and expansion-private captures

A plausible schema macro demonstrates the conflict. Suppose `is_generated_order` is an expanding shifted predicate macro. At emission time it reads a compile-time schema and returns the pattern AST equivalent of:

```pxs
subject is {OrderId: order_id}
```

User code then writes:

```pxs
if (packet is generated_order && order_id > 0)
    consume(order_id);
```

With the current architecture, `is_generated_order` remains an `AstExpand` until `DoEmitCode`. The parser must resolve `order_id` in the `&&` operand and branch long before the macro has revealed that binder. Recomputing flow after expansion cannot replace those already-built references. The reverse is equally unsafe: if the parser assumes the macro captures `order_id` but a schema version omits it or moves it below `not`, the pre-resolved references point at a slot that is unavailable on the route where they are used.

This is not limited to schema generation. A compiler hook that adds a property binder, renames a binder, or reassociates it across `or`/`not` changes the outward `(T, F)` capture signature after dependent source has resolved.

The adopted first implementation freezes a macro's *outward* capture signature during parsing. A macro or factory that exports captures must synchronously declare each logical name, its physical `PatternCapture` handle, and whether it is available on the macro boundary's true and false continuations. Dependent source resolves against that declaration. Late expansion may rewrite the internal implementation only while preserving the declared signature. An existing or forward-declared macro that provides no synchronous signature is capture-neutral at its boundary, but may still allocate and use expansion-private captures internally.

This does **not** prohibit a macro from binding and using captures inside its own expansion. For example, the expansion may contain:

```pxs
{OrderId: var order_id} and {LegacyOrderId: == order_id}
```

The left property subpattern acquires `order_id`; pattern `and` threads its hidden slot into the right subpattern; the pinned relational pattern reads the same capture. The expansion is flow-analysed normally, so reversing the two operands or putting the acquisition on a route that does not reach the read is an ordinary unavailable-capture error.

Macro captures have two explicit classes:

- **Exported captures** use the predeclared `PatternCapture` handles. They may be read inside the expansion and must reach the macro boundary with exactly the promised true/false availability. In the earlier `generated_order && order_id > 0` example, `order_id` must be exported.
- **Expansion-private captures** receive hygienic compiler-generated handles and hidden slots. They may be acquired and read anywhere permitted by the expansion's internal flow, but the macro boundary projects them out of both outward environments and clears/discards them. They cannot collide with ambient names or be referenced by dependent source. In the contrived two-property example, `order_id` may be private when it is needed only for the internal consistency check.

The macro construction API must traffic in `PatternCapture` handles rather than reconnecting binders and reads by case-insensitive strings after the fact. It should provide separate operations for looking up a predeclared exported handle and allocating a hygienic private handle. A source-like macro builder may offer names for convenience, but resolves them within a macro-local binding environment before returning the AST.

After expansion and before emission, the compiler flow-validates the expanded pattern, projects away private captures, and compares the result with the frozen signature. Any of the following is a compile-time error, not merely a warning:

- an exported capture is missing or has different true/false availability;
- an undeclared capture escapes the macro boundary;
- the expansion binds a different physical handle for a declared name;
- a private capture is referenced outside the expansion; or
- an internal capture is read on a route where it is unavailable.

The diagnostic should identify the macro invocation, show the declared and actual outward signatures, and point to the offending generated binder/use when source information is available. A warning would be insufficient because dependent AST references have already been resolved under the frozen contract and emitting code after a mismatch would be unsound.

General post-parse changes to the outward signature remain out of scope. Supporting them later requires a genuine expansion-before-resolution phase and symbolic dependent names; recomputing `BindingFlow` alone is not enough.

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

The AST factory and macro bridge need constructors and inspection APIs for the new nodes, exported-signature declarations, exported-capture lookup, and hygienic private-capture allocation. This is an allowed compiler-AST break in enabled mode. Disabled code should continue to construct `AstTypecheck`, `AstLogicalAnd`, and `AstLogicalOr` as it does now. Macro expansion is rejected unless its projected flow matches the frozen outward signature; merely invalidating and recomputing `BindingFlow` after dependent source has resolved is not sufficient.

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

In matching-enabled expression grammar, contextual `match` is recognized only when lookahead sees `match`, a balanced parenthesized subject, and then `{`. Otherwise the token remains an ordinary identifier, so `match(value)` continues to be a normal call. This predicate looks only for the balanced closing parenthesis and following brace; it does not try to parse or classify lambdas inside the subject. Within a pattern, contextual `when` is recognized as the guard delimiter after a complete `PatternOr`; in all other identifier positions it remains available as a name.

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

No pattern-generated `try`/`catch` is required. All pattern-internal branches emitted within one source exception region remain within that region. Exits from `try` and `catch` reuse the existing `StructuredExceptionHandling` choice of `br` or `leave`, and exceptions from predicates, views, or getters propagate through the source program's normal regions.

This proposal must not overstate the existing CIL support for abrupt exits *from a `finally` body*. `StructuredExceptionHandling` can translate the block's natural exit to `endfinally`, but classifies another jump from `finally` to the outside as invalid; `CilCompilerTests.ReturnFromFinally` deliberately expects an SEH deficiency. Matching inside a `finally` is therefore CIL-compatible when its own branches remain in the region, but a matching guard whose selected source branch executes `return`, `break`, or `continue` from that `finally` retains the language's existing CIL limitation. Closing that gap would require a separate pending-control-transfer lowering that records the requested return/jump and dispatches it outside the exception region; pattern lowering does not solve it merely by using ordinary jumps.

Reserved pattern helpers must offer a statically callable implementation wherever their contract permits it. In particular, `pattern\member\probe` implements `ICilCompilerAware` with `PrefersRunStatically` in the first CIL-compatible version, so compiled patterns call `RunStatically` directly rather than going through `Runtime.CallCommand`. Tolerant view-output access and List shape/slice helpers should follow the same model.

The next optimization tier is a custom CIL implementation for the member probe that calls a runtime `TryDynamicCall` adapter directly and exposes the handled flag/value without allocating the command ABI's two-element List. This remains a local optimization: persisted/interpreted bytecode keeps the stable command call, and both paths must send the same single zero-argument `PCall.Get` message with identical side effects and exception behavior.

A foreach-style CIL hint or an encoded compiled-pattern region is not proposed initially. Such a hint would only be useful for recognizing and optimizing a multi-instruction pattern as a unit; it is unnecessary for correctness and would couple the CIL compiler to source-pattern structure. Reconsider it only after bytecode-level optimization measurements.

The CIL parity suite must separately cover patterns whose internal branches stay inside `try`, `catch`, and `finally`; guard clauses whose branches leave `try` or `catch`; and the known-deficiency behavior for abrupt exits from `finally`. If full parity for the last category is made part of the pattern-matching milestone, the pending-control-transfer work must be an explicit prerequisite rather than an incidental pattern task.

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

1. Split word and symbolic boolean tokens. Add frozen module mode plus recursively inherited function/lambda effective-mode plumbing and legacy grammar compatibility tests.
2. Add the separate BBE precedence ladder, binding parse contexts, hidden acquisition slots, dual-outcome flow, capture barriers, and jump-aware AST interfaces. Implement bind/wildcard/literal, shifted predicate, `and`/`or`/`not`, and `&&`/`||`/`!`.
3. Integrate consumer-edge copies, branch valid/error overlays, and post-`if` public-local promotion. Add `while`/`until` and conditional-expression scopes, and reject exporting acquisitions in `for` conditions.
4. Add aliases, relational patterns, `when`, and `?(...)` predicates.
5. Add shifted views and the tolerant output helper, followed by List and property patterns. Give reserved helpers stable command ABIs and statically callable implementations from the start.
6. Expose the AST through factories and macro bridges, including frozen outward signatures, exported `PatternCapture` handles, hygienic private handles, expansion-flow projection, and signature-mismatch diagnostics. Document the old macro library as deprecated and add migration views.
7. Optionally add `AstMatchExpression` and the contextual `match` grammar.
8. Run the same semantic suite interpreted and CIL-compiled; verify direct static helper calls, then add and benchmark the allocation-free member-probe CIL implementation.

The first implementation round can stop after step 5 and explicitly reject CIL compilation of matching-enabled functions until the parity work in step 8 is complete. The bytecode format itself need not be provisional.

## Required test matrix

At minimum, the implementation needs tests for:

- disabled modules preserving `is` type checks, word/symbol boolean synonyms, statement `and`, partial type checks, and the current AST;
- module enable, function opt-in, function opt-out, recursive inheritance through arbitrary levels of named nested functions and lambdas, inner overrides in both directions, rejection of a late module switch, same/conflicting headers in transcluded sources, and enabled-mode legacy metadata forms using `not`;
- `x is String` parity with the legacy type check, including the hidden arity prefix, Prexonite type arguments, CLR-qualified types, aliases/imports, and forward-declared ordinary predicates;
- implicit binders, forced `var` binders, forced `==` value patterns, shadowing, case-insensitive duplicates, ambient by-reference symbols used from guards, and the optional unused-implicit-binder diagnostic including `var` suppression;
- parse shape at every BBE/pattern boundary, especially `a is > 0 && b is > a`, its parenthesized boolean-operand counterpart, nested guards, lambdas in guards, and match-arm `->` termination;
- capture barriers for assignment, return, call arguments, ordinary operators, collection elements, and expression statements, plus transparent parentheses and valid `&&`/`||` transport;
- every true/false flow equation for `&&`, `||`, `!`, pattern `and`, `or`, `not`, and `when`;
- same-name bindings in exclusive alternatives, repeated-acquisition errors while a slot is flow-valid, and legal reacquisition after an intersection invalidates it;
- the negative guard-clause example, unavailable-branch diagnostics, null after an unbound fallthrough route, read-only/non-addressable captures during matching, and normally mutable public locals after consumer entry;
- precondition loop capture copying and per-evaluation hidden-slot clearing, post-condition non-export, and rejection of acquisitions in both `for` condition forms;
- subject/view/predicate evaluation exactly once, left-to-right effects, and exception propagation;
- view results with zero, too few, exact, and excess outputs, including a successfully extracted null;
- exact and rest List patterns; empty property patterns; handled and unhandled `IObject`, CLR, and structure messages; missing and null-valued members; indexed/unreadable members; observable getter effects; throwing getters; direct CIL calls to `RunStatically`; and parity of the later allocation-free probe;
- `return`, `break`, and `continue` from branches, separating exits from `try`/`catch` from the existing CIL deficiency for exits from `finally`;
- macro inspection/construction of the enabled AST and unchanged disabled AST nodes; exported captures used internally; expansion-private captures; projected-signature equality; and diagnostics for missing, extra, rerouted, physically mismatched, escaping, and use-before-availability captures;
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
