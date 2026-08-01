We are exploring an experimental change to the language. Pattern matching, but not just any form of pattern matching: the frontier of PL research. "Functional
  Pearl: Binding Boolean Expressions and
  Extended Pattern Matching
  ARTHUR CHARGUÉRAUD and YANNI LEFKI" (paper included in docs/bbe/bbe-and-extended-matching.pdf (not checked in). The goal is to have a pattern matching
  implementation that is somewhere between what C# 10 offers as the floor and the BBE paper as the aspirational goal. A hard requirement is that matching must be
  user-extensible (that user code can define define "views" and predicates). This endeavour will not be trivial because the compiler architecture expects symbol
  resolution to be done by the time the AST is constructed, while the description in the BBE paper heavily relies on modifying what variables are in scope in which
  sub-expression.

  In this first step, your task is to study the existing language implementation, study the paper and
  then propose a concrete change/extension to the language in the new document docs/bbe/proposal.md

  1. This language change is expected to be breaking. The goal is that we can gate the breakage
     behind a toggle (application/module-level meta switch): `pattern_matching enabled;` (disabled by default for now).
     That way, new code can opt in while old code gets compiled with the previous rules.
     It is possible that the work requires minor breaking changes that cannot be handled by the flag. Run unguarded breaking changes by me before implementing.
  1.1 If easily doable, we might allow `pattern_matching` opt-in/opt-out for individual functions or sections of code. If not, it's also acceptable if
  `pattern_matching` is required to appear at the top of a module for discovery during pre-flight (ahead of module compilation)
  1.2 The compiler representation of the code (AST, consumed by macros) is fair game for breaking changes. Ideally code with `pattern_matching disabled`
     would not see a change in the AST but that's a very very low prio goal. For `pattern_matching enabled` we may change the AST however we like.
  1.3 There is an existing pattern matching implementation using macros. That would be completely deprecated/replaced by this work.
  2. Syntax wise, I want to repurpose the existing keyword `is` to initiate pattern matching. When the flag is enabled `x is String` is no longer a
     type check, but a pattern matching expression. Ideally, and I really mean as an ideal, as an aspiration, we can arrange it so that the
     pattern matching interpretation just so happens to have the same meaning (true if x has runtime type string, false otherwise).
  3. Prexonite Script is not statically typed. It knows which names are defined and what they mean (variable, by-ref variable, function, etc.)
     but that's it. This language extension won't change that. This means that the whole typing section of the paper needs to be re-contextualized for our work. It
  is still relevant in the sense that the typing rules define what symbols/bindings are in scope, what gets shadowed and how the little
     typing information we have (how many de-references should happen) flows through an expression.
  4. For extensibility, we follow a python-inspired "name-shifting" scheme.
  5. Try to minimize special cases, prefer extensible solutions. For example: today, if we see `x is even`, we look for a symbol in scope called `is_even` and if it
  exists, we transform the expression into `is_even(x)`. These should continue to work and users should be able to define custom predicates and views with just
  function name conventions. (Reasoning: function metadata is not guaranteed to be available when a function name is in scope; the function symbol could be a forward
  declaration. This means that the name alone has to carry the information whether a function implements a certain contract/pattern).
  6. Compatibility with CIL compilation is required. This can be tricky with the constraints on try-catch blocks (or it could be a non-issue).
     And we need a plan on how the extension impacts CIL compilation. This could be trvial (only re-use existing constructs), minimal (add some intrinsic/built-in
  commands with custom CIL compilation hints) or extensive (encoding and detection of a compiled pattern, like how we handle foreach).
     The initial implementation round will not involve CIL, but we do need a plan.
  7. We need two sets of boolean operators. The paper uses pattern level & | and ¬, and binding boolean epxression level `and`, `or` and `not`.
     This is awkward for Prexonite because, today, `and` and `&&` are synonyms. This is probably going to be the primary breaking change. With `pattern_matching
  enabled;` we follow C#'s lead. `and` `or` `not` become the operators used in patterns. `&&` `||` `!` are used in expressions. Yes, this is exactly
     the other way around from the paper's presentation. Take extra care not to confuse the two.
  8. Section 3 "Backtracking in Pattern Matching" is out of scope. It's a very cool feature but I doubt it would see much use.
     The existing unlabeled `break` `return` `continue` constructs need to be supported though (which can be seen as a special case of Section 3)
  9. I am undecided whether I actually want `switch` or `match` as new expressions. Switch seems dubious. We already have if-then-else-if-then-.. chains that the
  user can write without (visually) increasing nesting. Without labelled arms to jump to, I don't see value. If you do, feel free to propose it. Match seems more
  useful because it cuts down on repetition. Draft a concrete syntax that fits in well with the reest of the language for `match` as an optional addon to the
  propsal.
  9.1 It might of course make sense to introduce Switch as an AST node. Compiler representation under `pattern_matching enabled;` is fair game.
  10. Implicit binding declarations. C# requires that users write `var x` to bind any value. If possible, I'd like to avoid that. That is, I'd like to write
      patterns as `xs is [a, b]` not `xs is [var a, var b]`. This is not a hard requirement and it might come with too many tradeoffs.
  11. Predicate patterns would ideally support our existing partial application syntax. `xs is [(? > 0)]`. This might, again, be hard to pull off
      because we have to decide how to interpret the expression in parentheses before we know whether it is a partial application.
      I am absolutely willing to drop this or to require an additional annotation, say a `?` that indicates that the remainder of the expression
      is a predicate => `xs is [?(? > 0)]`. Other suggestions welcome
  12. One thing that the paper seems to ignore but that turns out to be extremely common in production code is binding from negated patterns. In C#:
      ```
      if(arg is not { Policy: {} policy }) {
        return;
      }
      Console.WriteLine(policy);
      ```
      or Rust:
      ```
      let Some(policy) = arg else {
        return;
      }
      ```
      I would want support for code patterns like this. We don't need to check whether the branches diverge (or C#'s definite assignment).
      This could be easy to implement (define the variable in the outer scope and shadow it with an error symbol in the inner scope)
      or very hard (because it undermines central assumptions of the paper).
      This feature is worth a lot to me. If necessary, do spend considerable time on it.
  13. Prexonite, in general, has the JavaScript-inspired philosophy that arguments and formal parameters don't need to match. Whatever is missing is
      assumed to be `null`/some appropriate default. This same philosophy should extend to patterns. E.g., if we have an active pattern `p0` that can bind
      two values but it is used as `x is p0(y)`, then that's absolutely fine. The same goes for when an active pattern (at runtime) doesn't return
      enough values. E.g., if `x is p0(y, z)` is written, but `p0` only returns one value to bind against, then `z` is bound to `null` instead.
  14. Note about the paper's claims about C#. The author seems to operate on very outdated knowledge of C#'s pattern matching capabilities, claiming
      that in `exp1 is pat1 && exp2 is pat2`, bound variables from pat1 didn't flow into pat2. That is completely false. C# absolutely supports something like `exp1
  is { Policy: {} policy } && policy is { Active: true }`. Similarly, don't be confused by earlier versions of C# not supporting binding from negative patterns
  (CS8780). This is no longer true in modern C#. The example listed in point 12 is absolutely legal and compiles.
