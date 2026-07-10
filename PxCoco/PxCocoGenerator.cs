using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace PxCoco.Generators;

[Generator]
public sealed class PxCocoGenerator : IIncrementalGenerator
{
    const string GrammarMetadata = "build_metadata.AdditionalFiles.PxCocoGrammar";
    const string OrderMetadata = "build_metadata.AdditionalFiles.PxCocoOrder";

    static readonly DiagnosticDescriptor InvalidInput = new(
        "PRXCOCO001",
        "Invalid PxCoco input",
        "{0}",
        "PxCoco",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor InvalidGrammar = new(
        "PRXCOCO002",
        "Invalid PxCoco grammar",
        "{0}",
        "PxCoco",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    static readonly DiagnosticDescriptor GenerationFailure = new(
        "PRXCOCO003",
        "PxCoco generation failed",
        "{0}",
        "PxCoco",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (pair, cancellationToken) => ReadFile(pair.Left, pair.Right, cancellationToken))
            .Where(static file => file is not null)
            .Collect();

        context.RegisterSourceOutput(files, Generate);
    }

    static GrammarFile ReadFile(
        AdditionalText file,
        AnalyzerConfigOptionsProvider optionsProvider,
        CancellationToken cancellationToken)
    {
        var options = optionsProvider.GetOptions(file);
        if (!options.TryGetValue(GrammarMetadata, out var grammar) || string.IsNullOrWhiteSpace(grammar))
            return null;

        var order = 0;
        if (options.TryGetValue(OrderMetadata, out var orderText) &&
            !int.TryParse(orderText, NumberStyles.Integer, CultureInfo.InvariantCulture, out order))
            order = int.MinValue;

        var text = file.GetText(cancellationToken);
        return text is null ? null : new GrammarFile(file.Path, grammar, order, text);
    }

    static void Generate(SourceProductionContext context, ImmutableArray<GrammarFile> files)
    {
        GenerateGrammar(
            context,
            files,
            grammarName: "Prexonite",
            targetNamespace: "Prexonite.Compiler",
            generateScanner: false,
            parserHintName: "Prexonite.Compiler.Parser.g.cs",
            scannerHintName: null);

        GenerateGrammar(
            context,
            files,
            grammarName: "PTypeExpression",
            targetNamespace: "Prexonite.Internal",
            generateScanner: true,
            parserHintName: "Prexonite.Internal.Parser.g.cs",
            scannerHintName: "Prexonite.Internal.Scanner.g.cs");
    }

    static void GenerateGrammar(
        SourceProductionContext context,
        ImmutableArray<GrammarFile> allFiles,
        string grammarName,
        string targetNamespace,
        bool generateScanner,
        string parserHintName,
        string scannerHintName)
    {
        var files = allFiles
            .Where(file => string.Equals(file.Grammar, grammarName, StringComparison.Ordinal))
            .OrderBy(file => file.Order)
            .ThenBy(file => file.Path, StringComparer.Ordinal)
            .ToArray();

        if (files.Length == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidInput,
                Location.None,
                $"No AdditionalFiles were supplied for the '{grammarName}' grammar."));
            return;
        }

        if (files.Any(file => file.Order == int.MinValue))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidInput,
                Location.None,
                $"Grammar '{grammarName}' has a file with a non-integer PxCocoOrder."));
            return;
        }

        try
        {
            var errors = new List<string>();
            var grammar = files.Length == 1 ? files[0].Text.ToString() : Merge(files);
            var result = at.jku.ssw.Coco.Coco.Generate(new at.jku.ssw.Coco.GeneratorOptions(
                writeMessage: static _ => { },
                writeError: errors.Add)
            {
                Grammar = grammar,
                SourceName = files.Length == 1 ? files[0].Path : grammarName + ".atg",
                ParserFrame = ReadResource("PxCoco.Parser.frame"),
                ParserFrameName = "../../Tools/Parser.frame",
                ScannerFrame = ReadResource("PxCoco.Scanner.frame"),
                Namespace = targetNamespace,
                GenerateScanner = generateScanner
            });

            foreach (var error in errors)
                ReportGrammarError(context, files, error);

            if (!result.Success || result.Parser is null)
                return;

            context.AddSource(
                parserHintName,
                SourceText.From("#nullable enable\n" + result.Parser, Encoding.UTF8));
            if (generateScanner)
            {
                if (result.Scanner is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        GenerationFailure,
                        Location.None,
                        $"Grammar '{grammarName}' did not produce a scanner."));
                    return;
                }

                context.AddSource(scannerHintName, SourceText.From(result.Scanner, Encoding.UTF8));
            }
        }
        catch (Exception exception)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                GenerationFailure,
                Location.None,
                $"Grammar '{grammarName}': {exception.Message}"));
        }
    }

    static string Merge(IEnumerable<GrammarFile> files)
    {
        var result = new StringBuilder();
        result.AppendLine("//-- GENERATED INTERNALLY BY PxCoco --//");
        foreach (var file in files)
        {
            result.AppendLine();
            result.Append("#file:").Append(file.Path).AppendLine("#");
            result.Append(file.Text.ToString());
        }
        result.AppendLine("#file:default#");
        return result.ToString();
    }

    static string ReadResource(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource '{name}' is missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    static void ReportGrammarError(
        SourceProductionContext context,
        IReadOnlyCollection<GrammarFile> files,
        string error)
    {
        if (!TryParseError(error, out var path, out var line, out var column, out var message))
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidGrammar, Location.None, error));
            return;
        }

        var file = files.FirstOrDefault(candidate => PathsEqual(candidate.Path, path));
        var location = file is null ? Location.None : CreateLocation(file, line, column);
        context.ReportDiagnostic(Diagnostic.Create(InvalidGrammar, location, message));
    }

    static bool TryParseError(
        string error,
        out string path,
        out int line,
        out int column,
        out string message)
    {
        path = "";
        line = 0;
        column = 0;
        message = error;
        if (string.IsNullOrEmpty(error) || error[0] != '(')
            return false;

        var comma = error.IndexOf(',');
        var closing = error.IndexOf(')');
        var separator = error.IndexOf("::", StringComparison.Ordinal);
        if (comma < 2 || closing < comma || separator < closing)
            return false;

        if (!int.TryParse(error.AsSpan(1, comma - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out line) ||
            !int.TryParse(error.AsSpan(comma + 1, closing - comma - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out column))
            return false;

        path = error.Substring(closing + 1, separator - closing - 1);
        message = error[(separator + 2)..];
        return true;
    }

    static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }
    }

    static Location CreateLocation(GrammarFile file, int line, int column)
    {
        var lineIndex = Math.Clamp(line - 1, 0, file.Text.Lines.Count - 1);
        var textLine = file.Text.Lines[lineIndex];
        var columnIndex = Math.Clamp(column - 1, 0, textLine.Span.Length);
        var position = textLine.Start + columnIndex;
        var point = new LinePosition(lineIndex, columnIndex);
        return Location.Create(
            file.Path,
            new TextSpan(position, 0),
            new LinePositionSpan(point, point));
    }

    sealed record GrammarFile(string Path, string Grammar, int Order, SourceText Text);
}
