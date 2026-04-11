// Helper for instrumenting existing test suites with V2 parser validation.
// Called from VMTestsBase and CompilerTestBase after each compilation.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Prexonite.Compiler.ParserV2;
using Prexonite.Compiler.ParserV2.Ast;
using PrxLexer = Prexonite.Compiler.ParserV2.Lexing.Lexer;

namespace PrexoniteTests.Tests.ParserV2;

public static class V2ParseCheck
{
    /// <summary>
    /// Parse the given source with the V2 parser and assert no errors.
    /// Warnings (e.g., `this` keyword) are allowed.
    /// </summary>
    /// <param name="input">The Prexonite Script source code.</param>
    /// <param name="expectedV2Errors">
    /// If non-null, the V2 parser is expected to produce at least this many errors
    /// (for known unsupported constructs). The check is inverted: errors are EXPECTED.
    /// If null (default), zero errors are expected.
    /// </param>
    public static void AssertV2ParseSucceeds(string input, int? expectedV2Errors = null)
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
            // Parser crash = always a failure
            Assert.Fail($"V2 parser threw an exception: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        var errors = cu.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (expectedV2Errors.HasValue)
        {
            // Errors are expected (known unsupported constructs)
            Assert.That(errors.Count, Is.GreaterThanOrEqualTo(expectedV2Errors.Value),
                $"V2 parser: expected at least {expectedV2Errors.Value} error(s) but got {errors.Count}");
        }
        else
        {
            if (errors.Count > 0)
            {
                var summary = string.Join("\n", errors.Take(5).Select(d => $"  {d.Span}: {d.Message}"));
                Assert.Fail($"V2 parser produced {errors.Count} error(s):\n{summary}");
            }
        }
    }
}
