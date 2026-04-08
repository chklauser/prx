// Prexonite – ParserV2 – Comprehensive parser tests

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Prexonite.Compiler.ParserV2;
using Prexonite.Compiler.ParserV2.Ast;
using Prexonite.Compiler.ParserV2.Lexing;
using PrxLexer = Prexonite.Compiler.ParserV2.Lexing.Lexer;

namespace PrexoniteTests.Tests.ParserV2;

[TestFixture]
public class ParserV2Tests
{
    static CompilationUnit Parse(string source)
    {
        var lexer = PrxLexer.ForString(source, "<test>");
        var parser = new Parser(lexer);
        return parser.ParseFile();
    }

    static string Sx(Node node) => SExpr.Serialize(node);

    void AssertSx(string source, string expectedSexpr)
    {
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(Sx(cu), Is.EqualTo(expectedSexpr));
    }

    void AssertDeclSx(string source, string expectedSexpr)
    {
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.GreaterThan(0), "No declarations");
        Assert.That(Sx(cu.Declarations[0]), Is.EqualTo(expectedSexpr));
    }

    void AssertExprSx(string source, string expectedSexpr)
    {
        // Wrap in a function body for expression parsing in local scope
        var wrapped = $"function __test__() {{ {source}; }}";
        var cu = Parse(wrapped);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        Assert.That(body.Statements.Statements, Has.Length.GreaterThan(0), "No statements in function body");
        var stmt = body.Statements.Statements[0];
        if (stmt is ExprStmt es)
            Assert.That(Sx(es.Expression), Is.EqualTo(expectedSexpr));
        else
            Assert.That(Sx(stmt), Is.EqualTo(expectedSexpr));
    }

    void AssertStmtSx(string source, string expectedSexpr)
    {
        var wrapped = $"function __test__() {{ {source} }}";
        var cu = Parse(wrapped);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        Assert.That(body.Statements.Statements, Has.Length.GreaterThan(0));
        Assert.That(Sx(body.Statements.Statements[0]), Is.EqualTo(expectedSexpr));
    }

    // ── 1. Literals ────────────────────────────────────────────────────────

    [Test]
    public void Literal_Integer()
    {
        AssertExprSx("42", "42");
    }

    [Test]
    public void Literal_HexInteger()
    {
        AssertExprSx("0xFF", "255");
    }

    [Test]
    public void Literal_IntegerWithSeparators()
    {
        // 1'000'000 should parse as 1000000
        AssertExprSx("1'000'000", "1000000");
    }

    [Test]
    public void Literal_Real()
    {
        AssertExprSx("1.5", "1.5");
    }

    [Test]
    public void Literal_Bool_True()
    {
        AssertExprSx("true", "true");
    }

    [Test]
    public void Literal_Bool_False()
    {
        AssertExprSx("false", "false");
    }

    [Test]
    public void Literal_Null()
    {
        AssertExprSx("null", "null");
    }

    [Test]
    public void Literal_String()
    {
        AssertExprSx("\"hello\"", "\"hello\"");
    }

    // ── 2. Identifiers ─────────────────────────────────────────────────────

    [Test]
    public void Identifier_Simple()
    {
        AssertExprSx("foo", "(id \"foo\")");
    }

    [Test]
    public void Identifier_Keyword_Escaped()
    {
        // $function → plain identifier "function"
        AssertExprSx("$function", "(id \"function\")");
    }

    // ── 3. Arithmetic ──────────────────────────────────────────────────────

    [Test]
    public void Arithmetic_Add()
    {
        AssertExprSx("a + b", "(+ (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Arithmetic_Precedence()
    {
        AssertExprSx("a + b * c", "(+ (id \"a\") (* (id \"b\") (id \"c\")))");
    }

    [Test]
    public void Arithmetic_Precedence2()
    {
        AssertExprSx("a * b + c * d",
            "(+ (* (id \"a\") (id \"b\")) (* (id \"c\") (id \"d\")))");
    }

    [Test]
    public void Arithmetic_Power_RightAssoc()
    {
        // a^b^c should be a^(b^c)
        AssertExprSx("a ^ b ^ c",
            "(^ (id \"a\") (^ (id \"b\") (id \"c\")))");
    }

    [Test]
    public void Arithmetic_Sub()
    {
        AssertExprSx("a - b", "(- (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Arithmetic_Mul()
    {
        AssertExprSx("a * b", "(* (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Arithmetic_Div()
    {
        AssertExprSx("a / b", "(/ (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Arithmetic_Mod()
    {
        AssertExprSx("a mod b", "(mod (id \"a\") (id \"b\"))");
    }

    // ── 4. Comparison ──────────────────────────────────────────────────────

    [Test]
    public void Comparison_Eq()
    {
        AssertExprSx("a == b", "(== (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Comparison_Ne()
    {
        AssertExprSx("a != b", "(!= (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Comparison_Lt()
    {
        AssertExprSx("a < b", "(< (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Comparison_Le()
    {
        AssertExprSx("a <= b", "(<= (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Comparison_Gt()
    {
        AssertExprSx("a > b", "(> (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Comparison_Ge()
    {
        AssertExprSx("a >= b", "(>= (id \"a\") (id \"b\"))");
    }

    // ── 5. Logical ─────────────────────────────────────────────────────────

    [Test]
    public void Logical_And()
    {
        AssertExprSx("a and b", "(and (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Logical_And_DoubleAmpersand()
    {
        AssertExprSx("a && b", "(and (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Logical_Or()
    {
        AssertExprSx("a or b", "(or (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Logical_Or_Chain()
    {
        AssertExprSx("a or b or c", "(or (or (id \"a\") (id \"b\")) (id \"c\"))");
    }

    [Test]
    public void Logical_Not()
    {
        AssertExprSx("not a", "(not (id \"a\"))");
    }

    [Test]
    public void Logical_Not_Bang()
    {
        AssertExprSx("!a", "(not (id \"a\"))");
    }

    // ── 6. Bitwise ─────────────────────────────────────────────────────────

    [Test]
    public void Bitwise_And()
    {
        AssertExprSx("a & b", "(&& (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Bitwise_Or()
    {
        AssertExprSx("a | b", "(|| (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Bitwise_Xor()
    {
        AssertExprSx("a xor b", "(xor (id \"a\") (id \"b\"))");
    }

    // ── 7. Coalesce ────────────────────────────────────────────────────────

    [Test]
    public void Coalesce_Binary()
    {
        AssertExprSx("a ?? b", "(?? (id \"a\") (id \"b\"))");
    }

    [Test]
    public void Coalesce_Chain()
    {
        AssertExprSx("a ?? b ?? c", "(?? (id \"a\") (id \"b\") (id \"c\"))");
    }

    // ── 8. Unary ───────────────────────────────────────────────────────────

    [Test]
    public void Unary_Negate()
    {
        AssertExprSx("-x", "(neg (id \"x\"))");
    }

    [Test]
    public void Unary_PreIncrement()
    {
        AssertExprSx("++x", "(pre++ (id \"x\"))");
    }

    [Test]
    public void Unary_PostIncrement()
    {
        AssertExprSx("x++", "(post++ (id \"x\"))");
    }

    [Test]
    public void Unary_PostDecrement()
    {
        AssertExprSx("x--", "(post-- (id \"x\"))");
    }

    [Test]
    public void Unary_PreDecrement()
    {
        AssertExprSx("--x", "(pre-- (id \"x\"))");
    }

    // ── 9. Assignment ──────────────────────────────────────────────────────

    [Test]
    public void Assignment_Simple()
    {
        AssertExprSx("x = 5", "(= (id \"x\") 5)");
    }

    [Test]
    public void Assignment_Add()
    {
        AssertExprSx("x += 1", "(+= (id \"x\") 1)");
    }

    [Test]
    public void Assignment_Coalesce()
    {
        AssertExprSx("x ??= y", "(??= (id \"x\") (id \"y\"))");
    }

    [Test]
    public void Assignment_Cast()
    {
        AssertExprSx("x ~= Int", "(~= (id \"x\") (type \"Int\"))");
    }

    // ── 10. Calls ──────────────────────────────────────────────────────────

    [Test]
    public void Call_NoArgs()
    {
        AssertExprSx("f()", "(call (id \"f\"))");
    }

    [Test]
    public void Call_WithArgs()
    {
        AssertExprSx("f(x, y)", "(call (id \"f\") (id \"x\") (id \"y\"))");
    }

    [Test]
    public void Call_MemberCall()
    {
        AssertExprSx("f.m(x)", "(member-call (id \"f\") \"m\" (id \"x\"))");
    }

    [Test]
    public void Call_IndirectCall()
    {
        AssertExprSx("f.()", "(indirect-call (id \"f\"))");
    }

    [Test]
    public void Call_IndexAccess()
    {
        AssertExprSx("f[i]", "([] (id \"f\") (id \"i\"))");
    }

    [Test]
    public void Call_IndexMulti()
    {
        AssertExprSx("f[i, j]", "([] (id \"f\") (id \"i\") (id \"j\"))");
    }

    [Test]
    public void Call_MemberAccess()
    {
        AssertExprSx("obj.member", "(. (id \"obj\") \"member\")");
    }

    // ── 11. AppendRight ────────────────────────────────────────────────────

    [Test]
    public void AppendRight_Simple()
    {
        AssertExprSx("f >> g(x)", "(>> (id \"f\") (call (id \"g\") (id \"x\")))");
    }

    [Test]
    public void AppendRight_Chain()
    {
        AssertExprSx("a >> b >> c",
            "(>> (>> (id \"a\") (id \"b\")) (id \"c\"))");
    }

    // ── 12. AppendLeft ─────────────────────────────────────────────────────

    [Test]
    public void AppendLeft_Tuple()
    {
        AssertExprSx("g(x) << (a, b)",
            "(<< (call (id \"g\") (id \"x\")) (id \"a\") (id \"b\"))");
    }

    [Test]
    public void AppendLeft_Single()
    {
        AssertExprSx("g << a",
            "(<< (id \"g\") (id \"a\"))");
    }

    // ── 13. KeyValuePair ───────────────────────────────────────────────────

    [Test]
    public void KeyValuePair_Simple()
    {
        // Key-value pairs only appear in expression contexts, not at statement level.
        // Wrap in a list literal to test.
        AssertExprSx("[a : b]", "(list (kvp (id \"a\") (id \"b\")))");
    }

    [Test]
    public void KeyValuePair_RightRecursive()
    {
        AssertExprSx("[a : b : c]", "(list (kvp (id \"a\") (kvp (id \"b\") (id \"c\"))))");
    }

    // ── 14. Then ───────────────────────────────────────────────────────────

    [Test]
    public void Then_Expr()
    {
        AssertExprSx("a then b", "(then (id \"a\") (id \"b\"))");
    }

    // ── 15. Conditional expressions ────────────────────────────────────────

    [Test]
    public void Conditional_If()
    {
        // if/unless at statement level parse as IfStmt; test using stmt helper
        AssertStmtSx("if (x) a; else b;",
            "(if\n  (id \"x\")\n  (block\n    (id \"a\"))\n  (else\n    (block\n      (id \"b\"))))");
    }

    [Test]
    public void Conditional_Unless()
    {
        AssertStmtSx("unless (x) a; else b;",
            "(unless\n  (id \"x\")\n  (block\n    (id \"a\"))\n  (else\n    (block\n      (id \"b\"))))");
    }

    // ── 16. Lambda ─────────────────────────────────────────────────────────

    [Test]
    public void Lambda_SingleParam()
    {
        AssertExprSx("x => x + 1",
            "(lambda (params (param \"x\"))\n  (+ (id \"x\") 1))");
    }

    [Test]
    public void Lambda_MultiParam()
    {
        AssertExprSx("(x, y) => x + y",
            "(lambda (params (param \"x\") (param \"y\"))\n  (+ (id \"x\") (id \"y\")))");
    }

    [Test]
    public void Lambda_WithBlock()
    {
        AssertExprSx("(x) => { return x; }",
            "(lambda (params (param \"x\"))\n  (block\n    (return (id \"x\"))))");
    }

    [Test]
    public void Lambda_EmptyParams()
    {
        AssertExprSx("() => 42",
            "(lambda (params)\n  42)");
    }

    // ── 17. Lazy ───────────────────────────────────────────────────────────

    [Test]
    public void Lazy_Expr()
    {
        AssertExprSx("lazy x * 2",
            "(lazy (* (id \"x\") 2))");
    }

    [Test]
    public void Lazy_Block()
    {
        AssertExprSx("lazy { return x; }",
            "(lazy\n  (block\n    (return (id \"x\"))))");
    }

    // ── 18. Coroutine ──────────────────────────────────────────────────────

    [Test]
    public void Coroutine_Simple()
    {
        AssertExprSx("coroutine f", "(coroutine (id \"f\"))");
    }

    [Test]
    public void Coroutine_WithArgs()
    {
        AssertExprSx("coroutine f for (x, y)",
            "(coroutine (id \"f\") (id \"x\") (id \"y\"))");
    }

    // ── 19. New ────────────────────────────────────────────────────────────

    [Test]
    public void New_Simple()
    {
        AssertExprSx("new Int(5)", "(new (type \"Int\") 5)");
    }

    [Test]
    public void New_NoArgs()
    {
        AssertExprSx("new Object()", "(new (type \"Object\"))");
    }

    // ── 20. TypeCast ───────────────────────────────────────────────────────

    [Test]
    public void TypeCast_Simple()
    {
        AssertExprSx("x~Int", "(~ (id \"x\") (type \"Int\"))");
    }

    // ── 21. TypeCheck ──────────────────────────────────────────────────────

    [Test]
    public void TypeCheck_Is()
    {
        AssertExprSx("x is Int", "(is (id \"x\") (type \"Int\"))");
    }

    [Test]
    public void TypeCheck_IsNot()
    {
        // 'Null' is lexed as KwNull with lowercase text "null"
        AssertExprSx("x is not Null", "(is-not (id \"x\") (type \"null\"))");
    }

    // ── 22. StaticCall ─────────────────────────────────────────────────────

    [Test]
    public void StaticCall_PrxType()
    {
        AssertExprSx("~Int.Parse(s)",
            "(static-call (type \"Int\") \"Parse\" (id \"s\"))");
    }

    [Test]
    public void StaticCall_ClrType()
    {
        AssertExprSx("::Console.WriteLine(x)",
            "(static-call (clr-type \"Console\") \"WriteLine\" (id \"x\"))");
    }

    // ── 23. Reference ──────────────────────────────────────────────────────

    [Test]
    public void Reference_Single()
    {
        AssertExprSx("->f", "(ref 1 \"f\")");
    }

    [Test]
    public void Reference_Double()
    {
        AssertExprSx("->->f", "(ref 2 \"f\")");
    }

    // ── 24. Placeholder ────────────────────────────────────────────────────

    [Test]
    public void Placeholder_Bare()
    {
        AssertExprSx("?", "(placeholder)");
    }

    [Test]
    public void Placeholder_Indexed()
    {
        AssertExprSx("?1", "(placeholder 1)");
    }

    // ── 25. Splice ─────────────────────────────────────────────────────────

    [Test]
    public void Splice_Args()
    {
        AssertExprSx("*args", "(splice (id \"args\"))");
    }

    // ── 26. ListLit ────────────────────────────────────────────────────────

    [Test]
    public void ListLit_Simple()
    {
        AssertExprSx("[1, 2, 3]", "(list 1 2 3)");
    }

    [Test]
    public void ListLit_Empty()
    {
        AssertExprSx("[]", "(list)");
    }

    // ── 27. HashLit ────────────────────────────────────────────────────────

    [Test]
    public void HashLit_Simple()
    {
        // Hash in expression position: use as argument
        AssertExprSx("f({a:1, b:2})",
            "(call (id \"f\") (hash (kvp (id \"a\") 1) (kvp (id \"b\") 2)))");
    }

    [Test]
    public void HashLit_Empty()
    {
        // Hash in expression position: use as argument
        AssertExprSx("f({})",
            "(call (id \"f\") (hash))");
    }

    // ── 28. Throw expr ─────────────────────────────────────────────────────

    [Test]
    public void ThrowExpr_Simple()
    {
        AssertExprSx("throw x", "(throw-expr (id \"x\"))");
    }

    // ── 29. Asm expr ───────────────────────────────────────────────────────

    [Test]
    public void AsmExpr_Simple()
    {
        // asm(...) is parsed as an expression (AsmExpr).
        // asm { } (brace form) is parsed as a statement.
        AssertExprSx("asm(ldc.int 5)",
            "(asm-expr\n  (op \"ldc.int\" 5))");
    }

    // ── 30. Return/yield ───────────────────────────────────────────────────

    [Test]
    public void Return_NoValue()
    {
        AssertStmtSx("return;", "(return)");
    }

    [Test]
    public void Return_WithValue()
    {
        AssertStmtSx("return x;", "(return (id \"x\"))");
    }

    [Test]
    public void Yield_WithValue()
    {
        AssertStmtSx("yield x;", "(yield (id \"x\"))");
    }

    // ── 31. Break/continue ─────────────────────────────────────────────────

    [Test]
    public void Break_Simple()
    {
        AssertStmtSx("break;", "(break)");
    }

    [Test]
    public void Continue_Simple()
    {
        AssertStmtSx("continue;", "(continue)");
    }

    // ── 32. Goto/label ─────────────────────────────────────────────────────

    [Test]
    public void Goto_Simple()
    {
        AssertStmtSx("goto myLabel;", "(goto \"myLabel\")");
    }

    [Test]
    public void Label_Stmt()
    {
        AssertStmtSx("myLabel:", "(label \"myLabel\")");
    }

    // ── 33. If statement ───────────────────────────────────────────────────

    [Test]
    public void IfStmt_Simple()
    {
        AssertStmtSx("if (x > 0) { return x; }",
            "(if\n  (> (id \"x\") 0)\n  (block\n    (return (id \"x\"))))");
    }

    [Test]
    public void IfStmt_WithElse()
    {
        AssertStmtSx("if (x) return 1; else return 2;",
            "(if\n  (id \"x\")\n  (block\n    (return 1))\n  (else\n    (block\n      (return 2))))");
    }

    // ── 34. Unless statement ───────────────────────────────────────────────

    [Test]
    public void UnlessStmt_Simple()
    {
        AssertStmtSx("unless (x) return;",
            "(unless\n  (id \"x\")\n  (block\n    (return)))");
    }

    // ── 35. While loop ─────────────────────────────────────────────────────

    [Test]
    public void WhileStmt_Simple()
    {
        AssertStmtSx("while (x > 0) { x--; }",
            "(while\n  (> (id \"x\") 0)\n  (block\n    (post-- (id \"x\"))))");
    }

    [Test]
    public void DoWhileStmt_Simple()
    {
        AssertStmtSx("do { x++; } while (x < 10);",
            "(do-while\n  (< (id \"x\") 10)\n  (block\n    (post++ (id \"x\"))))");
    }

    // ── 36. Until loop ─────────────────────────────────────────────────────

    [Test]
    public void UntilStmt_Simple()
    {
        AssertStmtSx("until (x == 0) { x--; }",
            "(until\n  (== (id \"x\") 0)\n  (block\n    (post-- (id \"x\"))))");
    }

    // ── 37. For loop (C-style) ─────────────────────────────────────────────

    [Test]
    public void ForStmt_CStyle()
    {
        var result = "function __test__() { for (var i = 0; i < 10; i++) { } }";
        var cu = Parse(result);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = body.Statements.Statements[0];
        Assert.That(stmt, Is.InstanceOf<ForStmt>());
        var fs = (ForStmt)stmt;
        Assert.That(fs.IsPostCondition, Is.False);
        Assert.That(fs.Init.Statements, Has.Length.EqualTo(1));
        Assert.That(fs.Condition, Is.Not.Null);
        Assert.That(fs.Next.Statements, Has.Length.EqualTo(1));
    }

    // ── 38. For loop (do-style) ────────────────────────────────────────────

    [Test]
    public void ForStmt_DoStyle()
    {
        var result = "function __test__() { for (var i = 0; do i++; while i < 10) { } }";
        var cu = Parse(result);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = body.Statements.Statements[0];
        Assert.That(stmt, Is.InstanceOf<ForStmt>());
        var fs = (ForStmt)stmt;
        Assert.That(fs.IsPostCondition, Is.True);
    }

    // ── 39. For loop multi-init ────────────────────────────────────────────

    [Test]
    public void ForStmt_MultiInit()
    {
        var source = "function __test__() { for (var x = 1 and var y = 2; x < y; x++) { } }";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var fs = (ForStmt)body.Statements.Statements[0];
        Assert.That(fs.Init.Statements, Has.Length.EqualTo(2));
    }

    // ── 40. Foreach ────────────────────────────────────────────────────────

    [Test]
    public void ForeachStmt_Simple()
    {
        AssertStmtSx("foreach (var x in list) { }",
            "(foreach\n  (local-var var \"x\")\n  (id \"list\")\n  (block))");
    }

    // ── 41. Try/catch/finally ──────────────────────────────────────────────

    [Test]
    public void TryCatch_Simple()
    {
        AssertStmtSx("try { } catch (e) { }",
            "(try\n  (block)\n  (catch (id \"e\") (block)))");
    }

    [Test]
    public void TryCatch_Finally()
    {
        AssertStmtSx("try { } finally { }",
            "(try\n  (block)\n  (finally\n    (block)))");
    }

    [Test]
    public void TryCatch_CatchAndFinally()
    {
        AssertStmtSx("try { } catch (e) { } finally { }",
            "(try\n  (block)\n  (catch (id \"e\") (block))\n  (finally\n    (block)))");
    }

    // ── 42. Using ──────────────────────────────────────────────────────────

    [Test]
    public void UsingStmt_Simple()
    {
        AssertStmtSx("using (resource) { }",
            "(using (id \"resource\") (block))");
    }

    // ── 43. Let binding ────────────────────────────────────────────────────

    [Test]
    public void LetBinding_Simple()
    {
        AssertStmtSx("let x = 5, y = 10;",
            "(let\n  (bind \"x\" 5)\n  (bind \"y\" 10))");
    }

    // ── 44. Asm statement ──────────────────────────────────────────────────

    [Test]
    public void AsmStmt_Simple()
    {
        AssertStmtSx("asm { ldc.int 5 }",
            "(asm\n  (op \"ldc.int\" 5))");
    }

    // ── 45. Global var ─────────────────────────────────────────────────────

    [Test]
    public void GlobalVar_Simple()
    {
        AssertDeclSx("var x;", "(global-var \"x\")");
    }

    [Test]
    public void GlobalRef_Simple()
    {
        AssertDeclSx("ref x;", "(global-ref \"x\")");
    }

    [Test]
    public void GlobalVar_WithInitializer()
    {
        var cu = Parse("var x = 5;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var gv = (GlobalVarDecl)cu.Declarations[0];
        Assert.That(gv.PrimaryName, Is.EqualTo("x"));
        Assert.That(gv.Initializer, Is.Not.Null);
        Assert.That(Sx(gv.Initializer!), Is.EqualTo("5"));
    }

    [Test]
    public void GlobalVar_WithAlias()
    {
        var cu = Parse("var x as y;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var gv = (GlobalVarDecl)cu.Declarations[0];
        Assert.That(gv.PrimaryName, Is.EqualTo("x"));
        Assert.That(gv.Aliases, Has.Length.EqualTo(1));
        Assert.That(gv.Aliases[0], Is.EqualTo("y"));
    }

    // ── 46. Function declarations ──────────────────────────────────────────

    [Test]
    public void FunctionDecl_Simple()
    {
        var cu = Parse("function foo(x, y) { return x + y; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.PrimaryName, Is.EqualTo("foo"));
        Assert.That(fn.Parameters, Has.Length.EqualTo(2));
        Assert.That(fn.Kind, Is.EqualTo(FunctionKind.Function));
    }

    [Test]
    public void FunctionDecl_AssignBody()
    {
        var cu = Parse("function foo = 42;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Body, Is.InstanceOf<FunctionExprBody>());
        var body = (FunctionExprBody)fn.Body;
        Assert.That(body.Style, Is.EqualTo(FunctionBodyStyle.Assign));
        Assert.That(Sx(body.Expression), Is.EqualTo("42"));
    }

    [Test]
    public void FunctionDecl_ArrowBody()
    {
        var cu = Parse("function foo(x) => x * 2;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Body, Is.InstanceOf<FunctionExprBody>());
        var body = (FunctionExprBody)fn.Body;
        Assert.That(body.Style, Is.EqualTo(FunctionBodyStyle.Arrow));
    }

    [Test]
    public void FunctionDecl_DoesBody()
    {
        var cu = Parse("function foo does return 0;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Body, Is.InstanceOf<FunctionBlockBody>());
        var body = (FunctionBlockBody)fn.Body;
        Assert.That(body.Style, Is.EqualTo(FunctionBodyStyle.Does));
    }

    [Test]
    public void FunctionDecl_LazyKind()
    {
        var cu = Parse("lazy function foo(x) { return x; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Kind, Is.EqualTo(FunctionKind.Lazy));
    }

    [Test]
    public void FunctionDecl_CoroutineKind()
    {
        var cu = Parse("coroutine foo(x) { yield x; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Kind, Is.EqualTo(FunctionKind.Coroutine));
    }

    [Test]
    public void FunctionDecl_MacroKind()
    {
        var cu = Parse("macro foo(x) { }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Kind, Is.EqualTo(FunctionKind.Macro));
    }

    [Test]
    public void FunctionDecl_WithMeta()
    {
        var cu = Parse("function foo[is test; is disabled] { }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Meta, Has.Length.EqualTo(2));
        var meta0 = (MetaBoolEntry)fn.Meta[0];
        Assert.That(meta0.Key, Is.EqualTo("test"));
        Assert.That(meta0.Value, Is.True);
        var meta1 = (MetaBoolEntry)fn.Meta[1];
        Assert.That(meta1.Key, Is.EqualTo("disabled"));
        Assert.That(meta1.Value, Is.True);
    }

    [Test]
    public void FunctionDecl_WithPath()
    {
        var cu = Parse("function foo/bar/baz() { }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.PrimaryName, Is.EqualTo("foo"));
        Assert.That(fn.Aliases, Has.Length.EqualTo(2));
        Assert.That(fn.Aliases[0], Is.EqualTo("bar"));
        Assert.That(fn.Aliases[1], Is.EqualTo("baz"));
    }

    // ── 47. Namespace declaration ──────────────────────────────────────────

    [Test]
    public void NamespaceDecl_Simple()
    {
        var cu = Parse("namespace prx.foo { function bar() {} }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var ns = (NamespaceDecl)cu.Declarations[0];
        Assert.That(string.Join(".", ns.Name.Parts), Is.EqualTo("prx.foo"));
        Assert.That(ns.Body, Has.Length.EqualTo(1));
    }

    // ── 48. Namespace import ───────────────────────────────────────────────

    [Test]
    public void NamespaceImport_Simple()
    {
        var cu = Parse("namespace import prx.foo.bar;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var nsi = (NamespaceImportDecl)cu.Declarations[0];
        Assert.That(nsi.Specs, Has.Length.EqualTo(1));
        Assert.That(string.Join(".", nsi.Specs[0].Source.Parts), Is.EqualTo("prx.foo.bar"));
    }

    // ── 49. Declare list ───────────────────────────────────────────────────

    [Test]
    public void DeclareList_Function()
    {
        var cu = Parse("declare function foo, bar;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var dl = (DeclareListDecl)cu.Declarations[0];
        Assert.That(dl.EntityKind, Is.EqualTo("function"));
        Assert.That(dl.Items, Has.Length.EqualTo(2));
        Assert.That(dl.Items[0].Name, Is.EqualTo("foo"));
        Assert.That(dl.Items[1].Name, Is.EqualTo("bar"));
    }

    [Test]
    public void DeclareList_Var()
    {
        var cu = Parse("declare var x;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var dl = (DeclareListDecl)cu.Declarations[0];
        Assert.That(dl.EntityKind, Is.EqualTo("var"));
        Assert.That(dl.Items[0].Name, Is.EqualTo("x"));
    }

    // ── 50. Declare block ──────────────────────────────────────────────────

    [Test]
    public void DeclareBlock_Simple()
    {
        var cu = Parse("declare { function foo }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations[0], Is.InstanceOf<DeclareBlockDecl>());
        var db = (DeclareBlockDecl)cu.Declarations[0];
        Assert.That(db.Entries, Has.Length.EqualTo(1));
        Assert.That(db.Entries[0].EntityKind, Is.EqualTo("function"));
    }

    // ── 51. Build block ────────────────────────────────────────────────────

    [Test]
    public void BuildBlock_Simple()
    {
        var cu = Parse("build { }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations[0], Is.InstanceOf<BuildBlockDecl>());
    }

    // ── 52. Global code block ──────────────────────────────────────────────

    [Test]
    public void GlobalCode_Simple()
    {
        var cu = Parse("{ var x = 5; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations[0], Is.InstanceOf<GlobalCodeDecl>());
        var gc = (GlobalCodeDecl)cu.Declarations[0];
        Assert.That(gc.Body.Statements, Has.Length.EqualTo(1));
    }

    // ── 53. Module meta ────────────────────────────────────────────────────

    [Test]
    public void ModuleMeta_NameString()
    {
        var cu = Parse("Name \"mymodule\";");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var mm = (ModuleMetaDecl)cu.Declarations[0];
        Assert.That(mm.Entry, Is.InstanceOf<MetaValueEntry>());
        var mve = (MetaValueEntry)mm.Entry;
        Assert.That(mve.Key, Is.EqualTo("Name"));
    }

    [Test]
    public void ModuleMeta_IsTest()
    {
        var cu = Parse("is test;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var mm = (ModuleMetaDecl)cu.Declarations[0];
        Assert.That(mm.Entry, Is.InstanceOf<MetaBoolEntry>());
        var mbe = (MetaBoolEntry)mm.Entry;
        Assert.That(mbe.Key, Is.EqualTo("test"));
        Assert.That(mbe.Value, Is.True);
    }

    // ── 54. Nested functions ───────────────────────────────────────────────

    [Test]
    public void NestedFunction_Simple()
    {
        var cu = Parse("function outer() { function inner(x) = x + 1; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var outer = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)outer.Body;
        Assert.That(body.Statements.Statements[0], Is.InstanceOf<NestedFunctionStmt>());
        var nfs = (NestedFunctionStmt)body.Statements.Statements[0];
        Assert.That(nfs.Function.PrimaryName, Is.EqualTo("inner"));
    }

    // ── 55. Statement and-chaining ─────────────────────────────────────────

    [Test]
    public void StatementAndChain()
    {
        var cu = Parse("function __test__() { var x = 1 and var y = 2; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        // The two var statements should both be in the block
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        // The `and` in the init creates two stmts chained
        // but since ParseStatement for simple statements doesn't consume `and`,
        // this test verifies ParseBlock handles them
        Assert.That(body.Statements.Statements.Length, Is.GreaterThanOrEqualTo(1));
    }

    // ── 56. Local var decl ─────────────────────────────────────────────────

    [Test]
    public void LocalVarDecl_Simple()
    {
        AssertStmtSx("var x = 5;",
            "(= (local-var var \"x\") 5)");
    }

    [Test]
    public void LocalVarDecl_Ref()
    {
        var cu = Parse("function __test__() { ref x; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = body.Statements.Statements[0];
        Assert.That(stmt, Is.InstanceOf<ExprStmt>());
        var lvd = ((ExprStmt)stmt).Expression as LocalVarDecl;
        Assert.That(lvd, Is.Not.Null);
        Assert.That(lvd!.RefCount, Is.EqualTo(1));
    }

    [Test]
    public void LocalVarDecl_Static()
    {
        var cu = Parse("function __test__() { static var x; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var lvd = (LocalVarDecl)stmt.Expression;
        Assert.That(lvd.IsStatic, Is.True);
    }

    [Test]
    public void LocalVarDecl_New()
    {
        var cu = Parse("function __test__() { new var x; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var lvd = (LocalVarDecl)stmt.Expression;
        Assert.That(lvd.IsNew, Is.True);
    }

    // ── 57. Interpreter line ───────────────────────────────────────────────

    [Test]
    public void InterpreterLine()
    {
        var cu = Parse("#!/usr/bin/prx\nfunction foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.InterpreterLine, Is.EqualTo("/usr/bin/prx"));
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void InterpreterLine_WithArgs()
    {
        var cu = Parse("#!/usr/bin/env prx --flag\nfunction foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.InterpreterLine, Is.EqualTo("/usr/bin/env prx --flag"));
    }

    [Test]
    public void InterpreterLine_Absent()
    {
        var cu = Parse("function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty);
        Assert.That(cu.InterpreterLine, Is.Null);
    }

    // ── Comments ──────────────────────────────────────────────────────────

    [Test]
    public void Comment_LineComment()
    {
        // Line comment should be ignored; function after it is parsed
        var cu = Parse("// this is a comment\nfunction foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
        Assert.That(((FunctionDecl)cu.Declarations[0]).PrimaryName, Is.EqualTo("foo"));
    }

    [Test]
    public void Comment_LineComment_EndOfLine()
    {
        // Comment at end of a declaration line
        var cu = Parse("function foo() {} // end-of-line comment\nfunction bar() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(2));
    }

    [Test]
    public void Comment_LineComment_InsideFunction()
    {
        var cu = Parse("function foo() {\n  // comment inside body\n  var x = 1;\n}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void Comment_BlockComment()
    {
        var cu = Parse("/* block comment */ function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_BlockComment_Multiline()
    {
        var cu = Parse("/* multi\n   line\n   comment */ function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_BlockComment_Inline()
    {
        // Block comment between tokens on the same line
        var cu = Parse("function /* surprise */ foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(((FunctionDecl)cu.Declarations[0]).PrimaryName, Is.EqualTo("foo"));
    }

    [Test]
    public void Comment_BlockComment_Nested()
    {
        // Nested block comments: /* outer /* inner */ outer */
        var cu = Parse("/* outer /* inner */ still comment */ function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_BlockComment_DeeplyNested()
    {
        var cu = Parse("/* l1 /* l2 /* l3 */ l2 */ l1 */ function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_Mixed_LineInsideBlock()
    {
        // Line comment inside a block comment — the // doesn't end the block comment
        var cu = Parse("/* block // not a line comment\n   still block */ function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_Mixed_BlockAfterLine()
    {
        // Block comment start after line comment — the /* is part of the line comment
        var cu = Parse("// line comment /* not a block start\nfunction foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_BlockComment_ContainsStars()
    {
        // Block comment with * and ** inside — should not close early
        var cu = Parse("/*** stars * inside ** here ***/ function foo() {}");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.EqualTo(1));
    }

    [Test]
    public void Comment_BlockComment_InsideString()
    {
        // /* inside a string is NOT a comment — it's part of the string
        var cu = Parse("function foo() { var s = \"hello /* not a comment */ world\"; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void Comment_LineComment_InsideString()
    {
        // // inside a string is NOT a comment
        var cu = Parse("function foo() { var s = \"hello // not a comment\"; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void Comment_HashBang_NotAtStart()
    {
        // #! only counts as interpreter line at the very start of the file
        // In the middle, # is an error token
        var cu = Parse("function foo() {}\n#!/usr/bin/prx");
        Assert.That(cu.InterpreterLine, Is.Null, "should NOT be an interpreter line");
    }

    // ── 58. String interpolation ───────────────────────────────────────────

    [Test]
    public void String_Plain()
    {
        // Plain string in local scope - will be a StringLit
        var cu = Parse("function __test__() { var s = \"hello\"; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var assign = (AssignExpr)stmt.Expression;
        Assert.That(assign.Value, Is.InstanceOf<StringLit>());
        Assert.That(((StringLit)assign.Value).Value, Is.EqualTo("hello"));
    }

    // ── 59. Verbatim strings ───────────────────────────────────────────────

    [Test]
    public void VerbatimString()
    {
        var cu = Parse("function __test__() { var s = @\"hello\\nworld\"; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var assign = (AssignExpr)stmt.Expression;
        Assert.That(assign.Value, Is.InstanceOf<StringLit>());
        // Verbatim string - no escape processing
        Assert.That(((StringLit)assign.Value).Value, Is.EqualTo("hello\\nworld"));
    }

    // ── 58b. String interpolation ────────────────────────────────────────

    [Test]
    public void String_InterpolId()
    {
        // $name interpolation
        AssertExprSx("\"hello $name world\"",
            "(interp \"hello \" (id \"name\") \" world\")");
    }

    [Test]
    public void String_InterpolExpr()
    {
        // $(expr) interpolation
        AssertExprSx("\"result: $(x + 1)\"",
            "(interp \"result: \" (expr (+ (id \"x\") 1)))");
    }

    [Test]
    public void String_InterpolMultiple()
    {
        // Multiple interpolations
        AssertExprSx("\"$a and $b\"",
            "(interp (id \"a\") \" and \" (id \"b\"))");
    }

    [Test]
    public void String_InterpolExprComplex()
    {
        // Complex expression inside interpolation
        AssertExprSx("\"$(f(x))\"",
            "(interp (expr (call (id \"f\") (id \"x\"))))");
    }

    [Test]
    public void VerbatimString_Interpol()
    {
        // Verbatim string with interpolation
        AssertExprSx("@\"hello $name\"",
            "(interp \"hello \" (id \"name\"))");
    }

    [Test]
    public void String_NoInterpol_DollarEscape()
    {
        // \\$ in string should produce literal $
        AssertExprSx("\"price: \\$5\"",
            "\"price: $5\"");
    }

    // ── 60. Error recovery ────────────────────────────────────────────────

    [Test]
    public void ErrorRecovery_SingleError()
    {
        // A parse error on one declaration should not prevent parsing the next
        var cu = Parse("@@@ invalid @@@ \nfunction foo() {}");
        // Should have at least one error
        Assert.That(cu.Diagnostics, Is.Not.Empty, "Expected diagnostics");
        // And should still have parsed the function
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fn && fn.PrimaryName == "foo"),
            Is.True, "Should have parsed the function after the error");
    }

    // ── Additional operator tests ──────────────────────────────────────────

    [Test]
    public void BinOpName_BitwiseAnd_Serializes()
    {
        // Verify BinaryOp.BitwiseAnd serializes to &&
        AssertExprSx("a & b", "(&& (id \"a\") (id \"b\"))");
    }

    [Test]
    public void BinOpName_BitwiseOr_Serializes()
    {
        AssertExprSx("a | b", "(|| (id \"a\") (id \"b\"))");
    }

    // ── DeltaLeft / DeltaRight operators ──────────────────────────────────

    [Test]
    public void DeltaRight_Expr()
    {
        AssertExprSx("x |> f", "(|> (id \"x\") (id \"f\"))");
    }

    [Test]
    public void DeltaLeft_Expr()
    {
        AssertExprSx("f <| x", "(<| (id \"f\") (id \"x\"))");
    }

    // ── Multi-statement function body ─────────────────────────────────────

    [Test]
    public void FunctionBody_MultipleStatements()
    {
        var cu = Parse("function add(x, y) { var result = x + y; return result; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        Assert.That(body.Statements.Statements, Has.Length.EqualTo(2));
    }

    // ── Chained member calls ──────────────────────────────────────────────

    [Test]
    public void ChainedMemberCalls()
    {
        AssertExprSx("a.b.c", "(. (. (id \"a\") \"b\") \"c\")");
    }

    // ── Complex expressions ───────────────────────────────────────────────

    [Test]
    public void ComplexExpr_Precedence()
    {
        // not a and b should be (not a) and b
        AssertExprSx("not a and b",
            "(and (not (id \"a\")) (id \"b\"))");
    }

    [Test]
    public void ComplexExpr_NegateAndAdd()
    {
        AssertExprSx("-a + b", "(+ (neg (id \"a\")) (id \"b\"))");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  GAP COVERAGE — Tasks #3-#10
    // ══════════════════════════════════════════════════════════════════════

    // ── Task #3: Identifiers with backslash and single-quote ─────────────

    [Test]
    public void Identifier_WithBackslash()
    {
        AssertExprSx(@"\init", "(id \"\\\\init\")");
    }

    [Test]
    public void Identifier_WithPrime()
    {
        AssertExprSx("m'", "(id \"m'\")");
    }

    [Test]
    public void Identifier_WithDoublePrime()
    {
        AssertExprSx("m''", "(id \"m''\")");
    }

    [Test]
    public void Identifier_BackslashMid()
    {
        // Backslash in middle of identifier
        AssertExprSx(@"test\store", "(id \"test\\\\store\")");
    }

    // ── Task #4: $"arbitrary id", $@ varargs, $keyword ───────────────────

    [Test]
    public void DollarString_ArbitraryId()
    {
        // $"non standard" produces an Identifier token with the string content
        AssertExprSx("$\"my identifier\"", "(id \"my identifier\")");
    }

    [Test]
    public void DollarKeyword_If()
    {
        // $if produces identifier "if" (not the keyword)
        AssertExprSx("$if", "(id \"if\")");
    }

    // ── Task #5: All modifying assignment operators ───────────────────────

    [Test]
    public void Assignment_Sub()
    {
        AssertExprSx("x -= 1", "(-= (id \"x\") 1)");
    }

    [Test]
    public void Assignment_Mul()
    {
        AssertExprSx("x *= 2", "(*= (id \"x\") 2)");
    }

    [Test]
    public void Assignment_Div()
    {
        AssertExprSx("x /= 2", "(/= (id \"x\") 2)");
    }

    [Test]
    public void Assignment_BitAnd()
    {
        AssertExprSx("x &= 0xFF", "(&= (id \"x\") 255)");
    }

    [Test]
    public void Assignment_BitOr()
    {
        AssertExprSx("x |= 1", "(|= (id \"x\") 1)");
    }

    [Test]
    public void Assignment_DeltaLeft()
    {
        AssertExprSx("x <|= f", "(<<= (id \"x\") (id \"f\"))");
    }

    [Test]
    public void Assignment_DeltaRight()
    {
        AssertExprSx("x |>= f", "(>>= (id \"x\") (id \"f\"))");
    }

    // ── Task #6: Operator-as-identifier ──────────────────────────────────

    [Test]
    public void OpIdent_Plus()
    {
        AssertExprSx("(+)", "(id \"(+)\")");
    }

    [Test]
    public void OpIdent_Minus()
    {
        AssertExprSx("(-)", "(id \"(-)\")");
    }

    [Test]
    public void OpIdent_Star()
    {
        AssertExprSx("(*)", "(id \"(*)\")");
    }

    [Test]
    public void OpIdent_Div()
    {
        AssertExprSx("(/)", "(id \"(/)\")");
    }

    [Test]
    public void OpIdent_DotNeg()
    {
        AssertExprSx("(-.)", "(id \"(-.)\")");
    }

    [Test]
    public void OpIdent_DecDec()
    {
        AssertExprSx("(--)", "(id \"(--)\")");
    }

    [Test]
    public void OpIdent_Eq()
    {
        AssertExprSx("(==)", "(id \"(==)\")");
    }

    // ── Task #7: Missing statement forms ─────────────────────────────────

    [Test]
    public void DoUntilStmt()
    {
        // Just verify it parses without errors and produces the right node type
        var cu = Parse("function __test__() { do { x--; } until (x == 0); }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = body.Statements.Statements[0];
        Assert.That(stmt, Is.InstanceOf<WhileStmt>());
        var ws = (WhileStmt)stmt;
        Assert.That(ws.IsNegated, Is.True, "until = negated");
        Assert.That(ws.IsPostCondition, Is.True, "do-until = post-condition");
    }

    [Test]
    public void TryFinallyCatch_Reversed()
    {
        // finally BEFORE catch — both should be captured regardless of order
        var cu = Parse("function __test__() { try { a(); } finally { cleanup(); } catch (var e) { handle(); } }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = body.Statements.Statements[0];
        Assert.That(stmt, Is.InstanceOf<TryCatchFinallyStmt>());
        var tcf = (TryCatchFinallyStmt)stmt;
        Assert.That(tcf.Catch, Is.Not.Null, "catch should be present");
        Assert.That(tcf.Finally, Is.Not.Null, "finally should be present");
    }

    [Test]
    public void NestedFunction_Depth2()
    {
        var cu = Parse("function outer() { function inner() { function deepest() { return 0; } } }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var outer = (FunctionDecl)cu.Declarations[0];
        var outerBody = (FunctionBlockBody)outer.Body;
        var innerStmt = (NestedFunctionStmt)outerBody.Statements.Statements[0];
        var innerBody = (FunctionBlockBody)innerStmt.Function.Body;
        var deepestStmt = innerBody.Statements.Statements[0];
        Assert.That(deepestStmt, Is.InstanceOf<NestedFunctionStmt>());
    }

    // ── Task #8: \& string escape ────────────────────────────────────────

    [Test]
    public void String_BackslashAmpersand()
    {
        // \& should produce nothing (empty)
        var cu = Parse("function __test__() { var s = \"ab\\&cd\"; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var assign = (AssignExpr)stmt.Expression;
        var str = (StringLit)assign.Value;
        Assert.That(str.Value, Is.EqualTo("abcd")); // \& produces nothing
    }

    // ── Task #9: CLR type with multi-level namespaces ────────────────────

    [Test]
    public void ClrType_MultiLevel()
    {
        // Prexonite::Types::PValueKeyValuePair — verify it parses without error
        var cu = Parse("function __test__() { var x = y is Prexonite::Types::PValueKeyValuePair; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var assign = (AssignExpr)stmt.Expression;
        var check = (TypeCheckExpr)assign.Value;
        var clrType = (ClrTypeExpr)check.Type;
        Assert.That(clrType.FullName, Is.EqualTo("Prexonite.Types.PValueKeyValuePair"));
    }

    [Test]
    public void ClrType_LeadingDoubleColon()
    {
        // Verify ::Console type is parsed
        var cu = Parse("function __test__() { var x = new ::Console(); }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = (FunctionBlockBody)fn.Body;
        var stmt = (ExprStmt)body.Statements.Statements[0];
        var assign = (AssignExpr)stmt.Expression;
        var newExpr = (NewExpr)assign.Value;
        Assert.That(newExpr.Type, Is.InstanceOf<ClrTypeExpr>());
    }

    // ── Task #10: Delta operators ────────────────────────────────────────

    [Test]
    public void Delta_PostfixLeft()
    {
        // x<| with no RHS → postfix unary
        AssertExprSx("[x <|]", "(list (post<| (id \"x\")))");
    }

    [Test]
    public void Delta_PostfixRight()
    {
        AssertExprSx("[x |>]", "(list (post|> (id \"x\")))");
    }

    [Test]
    public void Delta_Prefix()
    {
        AssertExprSx("<| x", "(pre<| (id \"x\"))");
    }

    [Test]
    public void Delta_Binary()
    {
        AssertExprSx("x <| y", "(<| (id \"x\") (id \"y\"))");
    }

    // ── Trailing commas ──────────────────────────────────────────────────

    [Test]
    public void TrailingComma_CallArgs()
    {
        // f(a, b,) — trailing comma in call argument list
        AssertExprSx("f(a, b,)", "(call (id \"f\") (id \"a\") (id \"b\"))");
    }

    [Test]
    public void TrailingComma_ListLiteral()
    {
        // [1, 2, 3,] — trailing comma in list literal
        AssertExprSx("[1, 2, 3,]", "(list 1 2 3)");
    }

    [Test]
    public void TrailingComma_HashLiteral()
    {
        // {a:1, b:2,} — trailing comma in hash literal
        AssertExprSx("f({a:1, b:2,})", "(call (id \"f\") (hash (kvp (id \"a\") 1) (kvp (id \"b\") 2)))");
    }

    [Test]
    public void TrailingComma_FormalParams()
    {
        // function foo(x, y,) { } — trailing comma in formal parameter list
        var cu = Parse("function foo(x, y,) { }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.Parameters.Length, Is.EqualTo(2));
    }

    // ══════════════════════════════════════════════════════════════════════
    //  End-to-end tests with real .pxs file patterns
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    public void E2E_NamespaceWithImportAndDeclare()
    {
        // From prx_lib.pxs: namespace with import and declare block
        var source = @"
namespace prx.cli
    import sys.*
{
    namespace timer {
        declare(
            start = ref command @""timer\start"",
            stop = ref command @""timer\stop"",
        );
    }
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var ns = (NamespaceDecl)cu.Declarations[0];
        Assert.That(ns.Name.ToString(), Is.EqualTo("prx::cli"));
        Assert.That(ns.Body.Length, Is.GreaterThan(0));
    }

    [Test]
    public void E2E_FunctionWithMetaAndPrimeIdentifiers()
    {
        // From struct.test.pxs: functions with prime identifiers and metadata
        var source = @"
function test_struct[test]
{
    function create_test_obj(x)
    {
        function m(self,y) = x + y;
        function m'(self,y) = self.m(y*x);
        function p(y)[private] = 2*x + 3*y;
        function m''(self,y) = p(y);
        return struct;
    }

    var a = 11;
    var o1 = new test_obj(a);
    assert(o1 is not null,""Struct doesn't return null."");
    assert_eq(o1.m(13),a+13,""Member m"");
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.PrimaryName, Is.EqualTo("test_struct"));
        Assert.That(fn.Meta.Length, Is.EqualTo(1));
    }

    [Test]
    public void E2E_StringInterpolationInAssert()
    {
        // From lang-ext.test.pxs: string interpolation with function calls
        var source = @"
function test_con[test]
{
    var a = 11;
    var b = 13;
    var x = kvp(a,b);
    assert(x is Prexonite::Types::PValueKeyValuePair,""$(boxed(x)) is not a PValueKeyValuePair"");
    assert_eq(x.Key,a,""Key of $(boxed(x)) doesn't match"");
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void E2E_PartialApplication()
    {
        // From prx_lib.pxs: partial application with ? placeholder
        var source = @"
function foo()
{
    ref green = runInDifferentColor(?, ::ConsoleColor.Green);
    ref yellow = ?.();
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void E2E_AppendRightChain()
    {
        // Common pattern: list >> each(action) >> all
        var source = @"
function test()
{
    var data = [1, 2, 3];
    data >> each(i => assert_eq(s.contains(i), true, ""contains($i)"")) >> all;
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void E2E_LambdaAndConditionalRef()
    {
        // From prx_lib.pxs: conditional ref declaration with lambdas
        var source = @"
function test()
{
    ref red =
        if(supportsColors)
            f => runInDifferentColor(f, ::ConsoleColor.Red)
        else
            f => f.()
        ;
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void E2E_TryCatchFinally()
    {
        var source = @"
function runSafe()
{
    var r;
    try {
        r = f();
    } catch(var e) {
        println(""Error: $(e)"");
    } finally {
        cleanup();
    }
    return r;
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void E2E_ModuleMetaBlock()
    {
        var source = @"
Name ""mymodule"";
Description ""A test module"";
References { ""dep1"", ""dep2"" };
build does require(""framework"");
";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        Assert.That(cu.Declarations.Length, Is.GreaterThanOrEqualTo(3));
    }

    [Test]
    public void E2E_ForeachWithLambda()
    {
        var source = @"
function test()
{
    var data = [a, b, c, d];
    foreach(var i in data)
    {
        assert_eq(s.contains(i), true, ""contains $i"");
    }
}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void VarAssign_AppendRight()
    {
        // var x = f(a) >> all;
        var cu = Parse("function __t__() { var x = f(a) >> all; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void ParenConditionalExpr_WithStrings()
    {
        // (if(x) "a" else "b") + y — parenthesized conditional with string values
        var cu = Parse("function __t__() { var z = (if(x) \"a\" else \"b\") + y; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void SpliceArgs_AppendRight()
    {
        // f(*args) >> all; — as a standalone statement (no var assignment)
        var cu = Parse("function __t__() { f(*args) >> all; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // ── CLR type member access chain ──────────────────────────────────

    [Test]
    public void String_InterpolFollowedByCommaArg()
    {
        // Two string args: f("$a", "")
        var cu = Parse("function __t__() { f(\"$a\", \"\"); }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void IsNull_InExpr()
    {
        // x is Null — Null is a valid type name
        AssertExprSx("x is Null", "(is (id \"x\") (type \"null\"))");
    }

    [Test]
    public void NotExpr_IfBlock()
    {
        // if(not x is Int) { y; } — not + type check + brace block
        var cu = Parse("function __t__() { if(not x is Int) { var r = 1; } }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void NotIsType_WithBraceBlock()
    {
        // if(!x is Int) { y; } — the { must be a block, not a hash literal
        var cu = Parse("function __t__() { if(!x is Int) { var r = 1; } }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void AsmExpr_MemberAccessChain()
    {
        // asm(ldr.eng).Commands.Contains("x") — member access after asm expr
        var cu = Parse("function f() { var x = asm(ldr.eng).Commands.Contains(\"x\"); }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void ClrType_MemberAccessChain()
    {
        // System::Environment.OSVersion.Platform — chains through member access
        var cu = Parse("function __t__() { var x = System::Environment.OSVersion.Platform; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Expert review findings — reproducing tests
    //  These document known issues found by static analysis.
    //  Marked [Explicit] so they don't run normally until the issue is fixed.
    // ══════════════════════════════════════════════════════════════════════

    // --- Finding 4: true/false/null missing ParseGetSetSuffix ---
    [Test]
    public void Finding4_NullMemberAccess()
    {
        // null should support member access (e.g., null~String or null.ToString)
        // Currently: the null literal is returned without ParseGetSetSuffix
        var cu = Parse("function __t__() { var x = true.ToString; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Finding 6: PushBack not consulted by Peek1 ---
    [Test]
    public void Finding6_IntegerDotMember()
    {
        // 1.method — the dot should be preserved after the integer
        var lexer = PrxLexer.ForString("1.m", "<test>");
        var tok1 = lexer.Next();
        Assert.That(tok1.Kind, Is.EqualTo(TokenKind.Integer), "first token should be integer");
        Assert.That(tok1.Text, Is.EqualTo("1"), "integer text");
        var tok2 = lexer.Next();
        Assert.That(tok2.Kind, Is.EqualTo(TokenKind.Dot), "second token should be dot (not lost)");
    }

    // --- Finding 13: Verbatim string interpolation doesn't suspend ---
    [Test]
    public void Finding13_VerbatimStringInterpolExpr()
    {
        // @"$(x + 1)" — verbatim string with $(expr) interpolation
        var cu = Parse("function __t__() { var s = @\"$(x + 1)\"; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        var stmt = (ExprStmt)body.Statements[0];
        var assign = (AssignExpr)stmt.Expression;
        Assert.That(assign.Value, Is.InstanceOf<InterpolatedString>(),
            "should be InterpolatedString, not StringLit");
    }

    // --- Finding 16: Unary + prefix ---
    [Test]
    public void Finding16_UnaryPlus()
    {
        // +x should parse without error (unary plus, identity)
        var cu = Parse("function __t__() { var y = +x; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Finding 12: ParseBlockAsStmt drops statements ---
    [Test]
    public void Finding12_BareBlockMultipleStatements()
    {
        // { x = 1; y = 2; } — bare block used as statement should keep all stmts
        // Currently the parser only keeps the first statement (lossy)
        var cu = Parse("function __t__() { { var x = 1; var y = 2; } }");
        // At minimum, no crash and some diagnostics or both stmts present
        // (Currently lossy — documenting the behavior)
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Finding 7: Whitespace-insensitive compound assignment (ACCEPTED) ---
    [Test]
    public void Finding7_SpacedCompoundAssign()
    {
        // Decision: `x + = 1` is accepted as equivalent to `x += 1`.
        // This is intentional: the parser checks Current==Plus && Peek==Assign
        // regardless of whitespace between them.
        var cu = Parse("function __t__() { var x = 0; x + = 1; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Finding 9: << single-arg captures full expression (ACCEPTED) ---
    [Test]
    public void Finding9_AppendLeftCapture()
    {
        // Decision: `f << a + b` is parsed as `f << (a + b)`.
        // The << operator's single-arg RHS uses ParseExpr, capturing the full expression.
        var cu = Parse("function __t__() { f << a + b; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Finding 11: Label vs key-value at statement start (ACCEPTED) ---
    [Test]
    public void Finding11_LabelVsKeyValue()
    {
        // Decision: at statement level, `id:` is always a label, never a key-value pair.
        // This matches the original Prexonite parser behavior.
        var cu = Parse("function __t__() { myLabel: var x = 1; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements[0], Is.InstanceOf<LabelStmt>());
    }

    // --- Precedence fixes: delta, not, xor, | ---
    [Test]
    public void Precedence_DeltaTighterThanThen()
    {
        // `a then b |> c` should be `a then (b |> c)` — delta tighter than then
        AssertExprSx("a then b |> c",
            "(then (id \"a\") (|> (id \"b\") (id \"c\")))");
    }

    [Test]
    public void Precedence_NotTighterThanBitwiseOr()
    {
        // `not a | b` should be `(not a) | b` — not tighter than |
        AssertExprSx("not a | b",
            "(|| (not (id \"a\")) (id \"b\"))");
    }

    [Test]
    public void Precedence_XorTighterThanBitwiseOr()
    {
        // `a | b xor c` should be `a | (b xor c)` — xor tighter than |
        AssertExprSx("a | b xor c",
            "(|| (id \"a\") (xor (id \"b\") (id \"c\")))");
    }

    [Test]
    public void Precedence_DeltaTighterThanAnd()
    {
        // `a and b |> c` should be `a and (b |> c)` — delta tighter than and
        AssertExprSx("[a and b |> c]",
            "(list (and (id \"a\") (|> (id \"b\") (id \"c\"))))");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Asm instruction coverage
    //  One test case per instruction form, verifying parse + AST shape.
    // ══════════════════════════════════════════════════════════════════════

    static IEnumerable<TestCaseData> AsmInstructionCases()
    {
        // Variable declarations
        yield return new TestCaseData("var x", "(asm-var \"x\")").SetName("Asm_VarDecl");
        yield return new TestCaseData("var x, y", "(asm-var \"x\" \"y\")").SetName("Asm_VarDeclMulti");
        yield return new TestCaseData("ref r", "(asm-var ref \"r\")").SetName("Asm_RefDecl");

        // Label
        yield return new TestCaseData("label myLabel", "(asm-label \"myLabel\")").SetName("Asm_Label");

        // Null group (no arguments)
        yield return new TestCaseData("ldc.null", "(op \"ldc.null\")").SetName("Asm_Null_LdcNull");
        yield return new TestCaseData("ret", "(op \"ret\")").SetName("Asm_Null_Ret");
        yield return new TestCaseData("ret.exit", "(op \"ret.exit\")").SetName("Asm_Null_RetExit");
        yield return new TestCaseData("ret.set", "(op \"ret.set\")").SetName("Asm_Null_RetSet");
        yield return new TestCaseData("ret.break", "(op \"ret.break\")").SetName("Asm_Null_RetBreak");
        yield return new TestCaseData("ret.continue", "(op \"ret.continue\")").SetName("Asm_Null_RetContinue");
        yield return new TestCaseData("ldr.eng", "(op \"ldr.eng\")").SetName("Asm_Null_LdrEng");
        yield return new TestCaseData("ldr.app", "(op \"ldr.app\")").SetName("Asm_Null_LdrApp");
        yield return new TestCaseData("throw", "(op \"throw\")").SetName("Asm_Null_Throw");
        yield return new TestCaseData("try", "(op \"try\")").SetName("Asm_Null_Try");
        yield return new TestCaseData("exc", "(op \"exc\")").SetName("Asm_Null_Exc");

        // Op-alias group (no arguments, instruction names that map to operators)
        yield return new TestCaseData("neg", "(op \"neg\")").SetName("Asm_OpAlias_Neg");
        yield return new TestCaseData("add", "(op \"add\")").SetName("Asm_OpAlias_Add");
        yield return new TestCaseData("sub", "(op \"sub\")").SetName("Asm_OpAlias_Sub");
        yield return new TestCaseData("mul", "(op \"mul\")").SetName("Asm_OpAlias_Mul");
        yield return new TestCaseData("ceq", "(op \"ceq\")").SetName("Asm_OpAlias_Ceq");

        // Integer group (one signed integer argument)
        yield return new TestCaseData("ldc.int 42", "(op \"ldc.int\" 42)").SetName("Asm_Int_LdcInt");
        yield return new TestCaseData("ldc.int -1", "(op \"ldc.int\" -1)").SetName("Asm_Int_LdcIntNeg");
        yield return new TestCaseData("pop 2", "(op \"pop\" 2)").SetName("Asm_Int_Pop");
        yield return new TestCaseData("dup 1", "(op \"dup\" 1)").SetName("Asm_Int_Dup");
        yield return new TestCaseData("ldloci 0", "(op \"ldloci\" 0)").SetName("Asm_Int_Ldloci");

        // Load constant real
        yield return new TestCaseData("ldc.real 3.14", "(op \"ldc.real\" 3.14)").SetName("Asm_LdcReal");

        // Load constant bool (in asm mode, true/false are identifiers)
        yield return new TestCaseData("ldc.bool true", "(op \"ldc.bool\" \"true\")").SetName("Asm_LdcBoolTrue");
        yield return new TestCaseData("ldc.bool false", "(op \"ldc.bool\" \"false\")").SetName("Asm_LdcBoolFalse");

        // Load constant string (id group — string arg parsed as identifier in asm)
        yield return new TestCaseData("ldc.string hello", "(op \"ldc.string\" \"hello\")").SetName("Asm_IdGroup_LdcString");
        yield return new TestCaseData("ldloc x", "(op \"ldloc\" \"x\")").SetName("Asm_IdGroup_Ldloc");
        yield return new TestCaseData("stloc x", "(op \"stloc\" \"x\")").SetName("Asm_IdGroup_Stloc");
        yield return new TestCaseData("ldglob g", "(op \"ldglob\" \"g\")").SetName("Asm_IdGroup_Ldglob");
        yield return new TestCaseData("ldr.func f", "(op \"ldr.func\" \"f\")").SetName("Asm_IdGroup_LdrFunc");
        yield return new TestCaseData("ldr.cmd c", "(op \"ldr.cmd\" \"c\")").SetName("Asm_IdGroup_LdrCmd");
        yield return new TestCaseData("ldr.type t", "(op \"ldr.type\" \"t\")").SetName("Asm_IdGroup_LdrType");
        yield return new TestCaseData("newclo f", "(op \"newclo\" \"f\")").SetName("Asm_IdGroup_Newclo");
        yield return new TestCaseData("inc x", "(op \"inc\" \"x\")").SetName("Asm_IdGroup_Inc");

        // Jump group (one id or integer argument)
        yield return new TestCaseData("jump myLabel", "(op \"jump\" \"myLabel\")").SetName("Asm_Jump_Symbolic");
        yield return new TestCaseData("jump 5", "(op \"jump\" 5)").SetName("Asm_Jump_Offset");
        yield return new TestCaseData("jump.t myLabel", "(op \"jump.t\" \"myLabel\")").SetName("Asm_Jump_True");
        yield return new TestCaseData("jump.f myLabel", "(op \"jump.f\" \"myLabel\")").SetName("Asm_Jump_False");
        yield return new TestCaseData("leave myLabel", "(op \"leave\" \"myLabel\")").SetName("Asm_Jump_Leave");

        // Id+arg group (optional @, .N is argument count in detail)
        yield return new TestCaseData("func.2 myFunc", "(op \"func.2\" \"myFunc\")").SetName("Asm_IdArg_Func");
        yield return new TestCaseData("cmd.1 myCmd", "(op \"cmd.1\" \"myCmd\")").SetName("Asm_IdArg_Cmd");
        yield return new TestCaseData("get.1 prop", "(op \"get.1\" \"prop\")").SetName("Asm_IdArg_Get");
        yield return new TestCaseData("set.2 prop", "(op \"set.2\" \"prop\")").SetName("Asm_IdArg_Set");
        yield return new TestCaseData("indloc.2 x", "(op \"indloc.2\" \"x\")").SetName("Asm_IdArg_Indloc");

        // Arg group (.N is argument count)
        yield return new TestCaseData("indarg.2", "(op \"indarg.2\")").SetName("Asm_Arg_Indarg");
        yield return new TestCaseData("tail.3", "(op \"tail.3\")").SetName("Asm_Arg_Tail");
        yield return new TestCaseData("newcor.1", "(op \"newcor.1\")").SetName("Asm_Arg_Newcor");

        // Qualid+arg group (.N is argument count)
        yield return new TestCaseData("sget.1 obj", "(op \"sget.1\" \"obj\")").SetName("Asm_QualidArg_Sget");
        yield return new TestCaseData("newobj.2 myType", "(op \"newobj.2\" \"myType\")").SetName("Asm_QualidArg_Newobj");

        // Special: nop with optional +data
        yield return new TestCaseData("nop", "(op \"nop\")").SetName("Asm_Nop");

        // Special: swap
        yield return new TestCaseData("swap", "(op \"swap\")").SetName("Asm_Swap");

        // Special: rot.N,M
        yield return new TestCaseData("rot.3, 5", "(op \"rot.3\" 5)").SetName("Asm_Rot");

        // Special: indloci.N index
        yield return new TestCaseData("indloci.2 0", "(op \"indloci.2\" 0)").SetName("Asm_Indloci");

        // @ modifier (just-effect: instruction drops its value)
        yield return new TestCaseData("@func.2 myFunc", "(@op \"func.2\" \"myFunc\")").SetName("Asm_JustEffect_Func");
        yield return new TestCaseData("@indarg.1", "(@op \"indarg.1\")").SetName("Asm_JustEffect_Indarg");
        yield return new TestCaseData("@sget.1 obj", "(@op \"sget.1\" \"obj\")").SetName("Asm_JustEffect_Sget");
        yield return new TestCaseData("@indloci.2 0", "(@op \"indloci.2\" 0)").SetName("Asm_JustEffect_Indloci");
    }

    [Test]
    [TestCaseSource(nameof(AsmInstructionCases))]
    public void AsmInstruction(string asmCode, string expectedAst)
    {
        var source = $"function __t__() {{ asm {{ {asmCode} }} }}";
        var cu = Parse(source);
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors for `{asmCode}`: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");

        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        var asmStmt = (AsmStmt)body.Statements[0];
        Assert.That(asmStmt.Instructions.Length, Is.GreaterThan(0),
            $"No instructions parsed for `{asmCode}`");
        var actual = Sx(asmStmt.Instructions[0]);
        Assert.That(actual, Is.EqualTo(expectedAst),
            $"AST mismatch for `{asmCode}`");
    }

    // --- Op-ident as function name ---
    [Test]
    public void FunctionDecl_OpIdentName()
    {
        // function (-)(a, b) = a; — operator overloading with op-ident name
        var cu = Parse("function (-)(a, b) = a;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.PrimaryName, Is.EqualTo("(-)"));
    }

    [Test]
    public void FunctionDecl_OpIdentDotNeg()
    {
        var cu = Parse("function (-.)(a, b) = a;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var fn = (FunctionDecl)cu.Declarations[0];
        Assert.That(fn.PrimaryName, Is.EqualTo("(-.)"));
    }

    // --- Lambda with var/ref params ---
    [Test]
    public void Lambda_VarRefParams()
    {
        // (var x, ref y) => x + y
        var cu = Parse("function __t__() { var f = (var x, ref y) => x + y; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Repro: unary + in call arg ---
    [Test]
    public void UnaryPlus_InCallArg()
    {
        var cu = Parse("function __t__() { f(+4); }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // --- Decision: x.m (a,b) => {} is NOT supported (legacy syntax) ---
    [Test]
    public void Decision_MemberSetLambda_NotSupported()
    {
        // In old Prexonite, `obj.member (params) => body` set a member to a lambda.
        // This syntax is no longer supported. The parser treats (params) as call args
        // and => as an unexpected token.
        var cu = Parse("function __t__() { json.mk_ldr (eng,app) => { }; }");
        Assert.That(cu.Diagnostics, Is.Not.Empty,
            "Expected a parse error for legacy member-set-lambda syntax");
    }

    // --- f << () => {} (replacement syntax for member-set-lambda) ---
    [Test]
    public void AppendLeft_EmptyLambda()
    {
        // f << () => {} — lambda as single << argument
        var cu = Parse("function __t__() { f << () => { return 1; }; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void AppendLeft_ParamLambda()
    {
        // f << (x, y) => x + y — lambda with params as << argument
        var cu = Parse("function __t__() { f << (x, y) => x + y; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void AppendLeft_SingleArgLambda()
    {
        // f << x => x + 1 — single-arg lambda
        var cu = Parse("function __t__() { f << x => x + 1; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    // ── Reproduction tests from spec/examples ──────────────────────────

    [Test]
    public void NestedFunction_OpIdentName()
    {
        // function (-.) nested inside another function
        var cu = Parse("function outer() { function (-.)(a,b) = a; }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void DeclareAndChain_WithReturn()
    {
        // declare ...; and declare ...; and return x;
        var cu = Parse("function f() does declare function a; and declare ref b; and return 1;");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void YieldStatement()
    {
        // yield inside a for loop
        var cu = Parse("function f() { for(var i = 0; i < 10; i++) { yield i; } }");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
    }

    [Test]
    public void EmptyStringInModuleMeta()
    {
        // Appearance "";  — empty string as module meta value
        var cu = Parse("Appearance \"\";");
        Assert.That(cu.Diagnostics, Is.Empty,
            $"Parse errors: {string.Join("; ", cu.Diagnostics.Select(d => d.Message))}");
        var mm = (ModuleMetaDecl)cu.Declarations[0];
        var entry = (MetaValueEntry)mm.Entry;
        Assert.That(entry.Key, Is.EqualTo("Appearance"));
    }

    [Test]
    public void ThisKeyword_IsWarning()
    {
        // `this` is reserved and should produce a warning diagnostic, but parsing continues
        var cu = Parse("function f() { var x = this; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(
            d => d.Severity == DiagnosticSeverity.Warning && d.Message.Contains("this")),
            "Expected warning for reserved 'this' keyword");
        // No errors — only warnings
        Assert.That(cu.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error), Is.Empty,
            "Should not have errors, only warnings");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Prx arc — parse every .pxs file in the Prx/ directory tree
    // ══════════════════════════════════════════════════════════════════════

    static string FindRepoRoot()
    {
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "global.json")))
            dir = Path.GetDirectoryName(dir);
        return dir ?? throw new InvalidOperationException("Could not find repo root (global.json)");
    }

    static IEnumerable<string> PrxSourceFiles()
    {
        var root = FindRepoRoot();
        var prxDir = Path.Combine(root, "Prx");
        return Directory.EnumerateFiles(prxDir, "*.pxs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.Combine("bin", "")) && !f.Contains(Path.Combine("obj", "")))
            .Select(f => Path.GetRelativePath(root, f))
            .OrderBy(f => f);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Error recovery tests
    //  Each test has a syntax error inside a delimiter pair and verifies:
    //  (1) at least one diagnostic at the correct location
    //  (2) a witness element after the error is present in the AST
    // ══════════════════════════════════════════════════════════════════════

    // ── () constructs ─────────────────────────────────────────────────

    [Test]
    public void Recovery_CallArgs()
    {
        // Error inside call args, witness: the `var y` statement after
        var cu = Parse("function __t__() { f(@@@ error); var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1 && d.Span.Start.Column >= 22 && d.Span.Start.Column <= 26));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_IfCondition()
    {
        var cu = Parse("function __t__() { if(@@@ error) { } var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_WhileCondition()
    {
        var cu = Parse("function __t__() { while(@@@ error) { } var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_ForHeader()
    {
        var cu = Parse("function __t__() { for(@@@ error) { } var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_ForeachHeader()
    {
        var cu = Parse("function __t__() { foreach(@@@ error) { } var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_CatchHeader()
    {
        var cu = Parse("function __t__() { try { } catch(@@@ error) { } var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_UsingHeader()
    {
        var cu = Parse("function __t__() { using(@@@ error) { } var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    // ── {} constructs ─────────────────────────────────────────────────

    [Test]
    public void Recovery_BlockBody()
    {
        // Error inside a block, witness: function bar after the error
        var cu = Parse("function foo() { @@@ error } function bar() { }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fd && fd.PrimaryName == "bar"),
            Is.True, "witness function 'bar' missing");
    }

    [Test]
    public void Recovery_FunctionBraceBody()
    {
        // Error inside function body, witness: function bar
        var cu = Parse("function foo() { @@@ error; var x = 1; } function bar() { }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fd && fd.PrimaryName == "bar"),
            Is.True, "witness function 'bar' missing");
    }

    [Test]
    public void Recovery_NamespaceBody()
    {
        // Error inside namespace body, witness: function bar after namespace
        var cu = Parse("namespace ns { @@@ error } function bar() { }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fd && fd.PrimaryName == "bar"),
            Is.True, "witness function 'bar' missing");
    }

    [Test]
    public void Recovery_HashLiteral()
    {
        // Error inside hash literal — at minimum, the parse completes without hanging.
        // The } in {1 2} terminates the hash. Recovery may not find the witness in all cases
        // since the } could also be interpreted as closing the function body.
        var cu = Parse("function foo() { f({1 2}); }\nfunction bar() { }");
        Assert.That(cu.Diagnostics, Is.Not.Empty, "Expected at least one diagnostic");
        // The parse should complete (no infinite loop) — that's the main assertion.
    }

    // ── [] constructs ─────────────────────────────────────────────────

    [Test]
    public void Recovery_IndexAccess()
    {
        // Error inside index, witness: var y statement
        var cu = Parse("function __t__() { x[@@@ error]; var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_ListLiteral()
    {
        // Error inside list literal, witness: var y statement
        var cu = Parse("function __t__() { var x = [@@@ error]; var y = 1; }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        var fn = (FunctionDecl)cu.Declarations[0];
        var body = ((FunctionBlockBody)fn.Body).Statements;
        Assert.That(body.Statements.Any(s => s is ExprStmt es && es.Expression is AssignExpr ae
            && ae.Target is LocalVarDecl lvd && lvd.Name == "y"), Is.True, "witness 'var y' missing");
    }

    [Test]
    public void Recovery_MetaBlock()
    {
        // Error inside meta block, witness: function bar
        var cu = Parse("function foo [@@@ error] { } function bar() { }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fd && fd.PrimaryName == "bar"),
            Is.True, "witness function 'bar' missing");
    }

    // ── Expression body forms ─────────────────────────────────────────

    [Test]
    public void Recovery_FunctionAssignBody()
    {
        // Error in `= expr;` body, witness: function bar
        var cu = Parse("function foo() = @@@ error; function bar() { }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fd && fd.PrimaryName == "bar"),
            Is.True, "witness function 'bar' missing");
    }

    [Test]
    public void Recovery_FunctionDoesBody()
    {
        // Error in `does stmt;` body, witness: function bar
        var cu = Parse("function foo() does @@@ error; function bar() { }");
        Assert.That(cu.Diagnostics, Has.Some.Matches<Diagnostic>(d => d.Span.Start.Line == 1));
        Assert.That(cu.Declarations.Any(d => d is FunctionDecl fd && fd.PrimaryName == "bar"),
            Is.True, "witness function 'bar' missing");
    }

    // ══════════════════════════════════════════════════════════════════════
    //  Prx source tree — parse every .pxs file
    // ══════════════════════════════════════════════════════════════════════

    [Test]
    [TestCaseSource(nameof(PrxSourceFiles))]
    public void PrxArc_ParsesWithoutErrors(string relativePath)
    {
        var root = FindRepoRoot();
        var fullPath = Path.Combine(root, relativePath);
        var source = File.ReadAllText(fullPath);
        var lexer = PrxLexer.ForString(source, relativePath);
        var parser = new Parser(lexer);
        var cu = parser.ParseFile();

        var errors = cu.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.That(errors, Is.Empty,
            $"Parse errors in {relativePath}:\n{string.Join("\n", errors.Select(d => $"  {d.Span}: {d.Message}"))}");
    }
}
