using System;

namespace at.jku.ssw.Coco
{
    public sealed class GeneratorOptions
    {
        public GeneratorOptions(Action<string> writeMessage, Action<string> writeError)
        {
            WriteMessage = writeMessage ?? throw new ArgumentNullException(nameof(writeMessage));
            WriteError = writeError ?? throw new ArgumentNullException(nameof(writeError));
        }

        public string Grammar { get; set; }
        public string SourceName { get; set; }
        public string ParserFrame { get; set; }
        public string ParserFrameName { get; set; }
        public string ScannerFrame { get; set; }
        public string Namespace { get; set; }
        public bool GenerateScanner { get; set; } = true;
        public bool DirectDebug { get; set; }
        public string DirectDebugTrace { get; set; }
        public Action<string> WriteMessage { get; }
        public Action<string> WriteError { get; }
    }

    public sealed class GenerationResult
    {
        public GenerationResult(bool success, string parser, string scanner, string trace)
        {
            Success = success;
            Parser = parser;
            Scanner = scanner;
            Trace = trace;
        }

        public bool Success { get; }
        public string Parser { get; }
        public string Scanner { get; }
        public string Trace { get; }
    }
}
