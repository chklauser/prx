/*
 * C# Flex 1.4
 * Copyright (C) 2004-2005 Jonathan Gilbert <logic@deltaq.org>
 * Derived from JFlex 1.4, Copyright (C) 1998-2004 Gerwin Klein.
 *
 * Modified in July 2026 by the Prexonite contributors: replaced the
 * WinForms/console output layer with an in-memory diagnostic collector.
 * Distributed under GNU GPL version 2; see ../COPYRIGHT.
 */

namespace CSFlex;

internal enum GenerationDiagnosticSeverity
{
    Warning,
    Error
}

internal sealed record GenerationDiagnostic(
    GenerationDiagnosticSeverity Severity,
    string Message,
    string Path = null,
    int Line = -1,
    int Column = -1);

public sealed class Out
{
    public static readonly string NL = Environment.NewLine;

    static readonly List<GenerationDiagnostic> diagnostics = new();
    static int errors;

    internal static IReadOnlyList<GenerationDiagnostic> Diagnostics => diagnostics;

    public static void resetCounters()
    {
        diagnostics.Clear();
        errors = 0;
    }

    public static void checkErrors()
    {
        if (errors > 0)
            throw new GeneratorException();
    }

    public static void error(string message) => AddError(message);
    public static void error(ErrorMessages message) => AddError(ErrorMessages.get(message));
    public static void error(ErrorMessages message, string data) =>
        AddError(ErrorMessages.get(message, data));
    public static void error(ErrorMessages message, File file) =>
        AddError(ErrorMessages.get(message), file);
    public static void error(File file, ErrorMessages message, int line, int column) =>
        AddError(ErrorMessages.get(message), file, line, column);

    public static void warning(string message) => AddWarning(message);
    public static void warning(ErrorMessages message, int line) =>
        AddWarning(ErrorMessages.get(message), null, line, -1);
    public static void warning(File file, ErrorMessages message, int line, int column) =>
        AddWarning(ErrorMessages.get(message), file, line, column);

    static void AddError(string message, File file = null, int line = -1, int column = -1)
    {
        errors++;
        diagnostics.Add(new GenerationDiagnostic(
            GenerationDiagnosticSeverity.Error, message, file?.ToString(), line, column));
    }

    static void AddWarning(string message, File file = null, int line = -1, int column = -1) =>
        diagnostics.Add(new GenerationDiagnostic(
            GenerationDiagnosticSeverity.Warning, message, file?.ToString(), line, column));

    public static void debug(string message) { }
    public static void dump(string message) { }
    public static void print(string message) { }
    public static void println(string message) { }
    public static void println(ErrorMessages message, string data) { }
    public static void println(ErrorMessages message, int data) { }
    public static void time(string message) { }
    public static void time(ErrorMessages message, Timer timer) { }
}
