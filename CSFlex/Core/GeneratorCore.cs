/*
 * Modern in-memory entry point for C# Flex 1.4.
 * Copyright (C) 1998-2005 Gerwin Klein and Jonathan Gilbert.
 * Copyright (C) 2026 Prexonite contributors.
 * Distributed under GNU GPL version 2; see ../COPYRIGHT.
 */

using System.Collections.Immutable;
using System.Text;

namespace CSFlex;

internal sealed record GenerationResult(
    string Source,
    ImmutableArray<GenerationDiagnostic> Diagnostics);

internal static class GeneratorCore
{
    internal const string Version = "1.4";
    static readonly object Gate = new();

    internal static GenerationResult Generate(string specification, string sourcePath)
    {
        lock (Gate)
        {
            Options.setDefaults();
            Options.emit_csharp = true;
            Options.verbose = false;
            Options.progress = false;
            Skeleton.readNested();
            Out.resetCounters();

            try
            {
                using var reader = new StringReader(specification);
                var scanner = new LexScan(reader);
                var inputFile = new File(sourcePath);
                scanner.setFile(inputFile);
                var parser = new LexParse(scanner);
                var nfa = (NFA)parser.parse().value;
                Out.checkErrors();

                var dfa = nfa.getDFA();
                dfa.checkActions(scanner, parser);
                dfa.minimize();

                var output = new StringBuilder();
                using (var writer = new StringWriter(
                           output,
                           System.Globalization.CultureInfo.InvariantCulture))
                {
                    var emitter = new Emitter(inputFile, parser, dfa, writer);
                    emitter.emit();
                }

                return new GenerationResult(
                    output.ToString(),
                    Out.Diagnostics.ToImmutableArray());
            }
            catch (ScannerException exception)
            {
                Out.error(exception.file, exception.message, exception.line, exception.column);
            }
            catch (MacroException exception)
            {
                Out.error(exception.Message);
            }
            catch (GeneratorException)
            {
            }
            catch (Exception exception)
            {
                Out.error(exception.Message);
            }

            return new GenerationResult(null, Out.Diagnostics.ToImmutableArray());
        }
    }
}
