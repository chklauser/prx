/*
 * CSFlex incremental source-generator wrapper.
 * Copyright (C) 2026 Prexonite contributors.
 *
 * This file is part of the modernized C# Flex port and is distributed under
 * GNU GPL version 2. See COPYRIGHT. Generated scanner code is covered by the
 * input specification's copyright, as described by the upstream exception.
 */

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSFlex.Generators;

[Generator]
public sealed class CSFlexGenerator : IIncrementalGenerator
{
    const string OutputMetadata = "build_metadata.AdditionalFiles.CSFlexOutput";

    static readonly DiagnosticDescriptor InvalidInput = new(
        "PRXCSFLEX001",
        "Invalid CSFlex input",
        "{0}",
        "CSFlex",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor InvalidSpecification = new(
        "PRXCSFLEX002",
        "Invalid CSFlex specification",
        "{0}",
        "CSFlex",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor SpecificationWarning = new(
        "PRXCSFLEX003",
        "CSFlex specification warning",
        "{0}",
        "CSFlex",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor GenerationFailure = new(
        "PRXCSFLEX004",
        "CSFlex generation failed",
        "{0}",
        "CSFlex",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var inputs = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (pair, cancellationToken) =>
                ReadInput(pair.Left, pair.Right, cancellationToken))
            .Where(static input => input is not null);

        context.RegisterSourceOutput(inputs, Generate);
    }

    static ScannerInput ReadInput(
        AdditionalText file,
        AnalyzerConfigOptionsProvider optionsProvider,
        CancellationToken cancellationToken)
    {
        var options = optionsProvider.GetOptions(file);
        if (!options.TryGetValue(OutputMetadata, out var output) ||
            string.IsNullOrWhiteSpace(output))
            return null;

        var text = file.GetText(cancellationToken);
        return text is null ? null : new ScannerInput(file.Path, output, text);
    }

    static void Generate(SourceProductionContext context, ScannerInput input)
    {
        if (!IsValidHintName(input.Output))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidInput,
                Location.None,
                $"'{input.Output}' is not a valid generated-source hint name."));
            return;
        }

        try
        {
            var result = GeneratorCore.Generate(
                input.Text.ToString(),
                Path.GetFileName(input.Path));
            foreach (var diagnostic in result.Diagnostics)
            {
                var descriptor = diagnostic.Severity == GenerationDiagnosticSeverity.Error
                    ? InvalidSpecification
                    : SpecificationWarning;
                var location = CreateLocation(input, diagnostic.Line, diagnostic.Column);
                context.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    location,
                    diagnostic.Message));
            }

            if (result.Source is not null)
            {
                context.AddSource(
                    input.Output,
                    SourceText.From(result.Source, Encoding.UTF8));
            }
            else if (!result.Diagnostics.Any(static diagnostic =>
                         diagnostic.Severity == GenerationDiagnosticSeverity.Error))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    GenerationFailure,
                    Location.None,
                    $"'{input.Path}' did not produce a scanner."));
            }
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GenerationFailure,
                Location.None,
                $"'{input.Path}': {exception.Message}"));
        }
    }

    static bool IsValidHintName(string hintName) =>
        hintName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
        hintName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
        !Path.IsPathRooted(hintName);

    static Location CreateLocation(ScannerInput input, int line, int column)
    {
        if (line < 0 || input.Text.Lines.Count == 0)
            return Location.None;

        var lineIndex = Math.Clamp(line, 0, input.Text.Lines.Count - 1);
        var textLine = input.Text.Lines[lineIndex];
        var columnIndex = column < 0 ? 0 : Math.Clamp(column, 0, textLine.Span.Length);
        var position = textLine.Start + columnIndex;
        var point = new LinePosition(lineIndex, columnIndex);
        return Location.Create(
            input.Path,
            new TextSpan(position, 0),
            new LinePositionSpan(point, point));
    }

    sealed record ScannerInput(string Path, string Output, SourceText Text);
}
