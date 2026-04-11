// Helper for instrumenting existing test suites with V2 parser validation.
// Called from VMTestsBase and CompilerTestBase after each compilation.

using System;
using System.Linq;
using NUnit.Framework;
using Prexonite.Compiler.ParserV2.Ast;
using PrxLexer = Prexonite.Compiler.ParserV2.Lexing.Lexer;

namespace PrexoniteTests.Tests.ParserV2;

public static class V2ParseCheck
{
    /// <summary>
    /// Parse the given source with the V2 parser and assert no errors.
    /// Warnings (e.g., <c>this</c> keyword) are allowed.
    /// </summary>
    public static void AssertV2ParseSucceeds(string input)
    {
        CompilationUnit cu;
        try
        {
            var lexer = PrxLexer.ForString(input, "<v2-check>");
            var parser = new Prexonite.Compiler.ParserV2.Parser(lexer);
            cu = parser.ParseFile();
        }
        catch (Exception ex)
        {
            Assert.Fail($"V2 parser threw an exception: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        var errors = cu.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (errors.Count > 0)
        {
            var summary = string.Join("\n", errors.Take(5).Select(d => $"  {d.Span}: {d.Message}"));
            Assert.Fail($"V2 parser produced {errors.Count} error(s):\n{summary}");
        }
    }
}
