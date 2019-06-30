using System;
using System.Diagnostics.CodeAnalysis;
using at.jku.ssw.Coco;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PxCoco.Msbuild
{
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class PxCoco : Task
    {
        [Required]
        public ITaskItem Grammar { get; set; }

        [Output]
        public ITaskItem[] OutputFiles { get; private set; }

        public string Namespace { get; set; } = null;

        public string FramesDirectory { get; set; } = null;

        public bool DirectDebug { get; set; } = false;

        public string RelativePathRoot { get; set; }

        public override bool Execute()
        {
            if (Grammar == null)
            {
                Log.LogError("PxCoco requires a grammar file.");
                return false;
            }

            if (!System.IO.File.Exists(Grammar.ItemSpec))
            {
                Log.LogError("The grammar file {0} does not exist", Grammar.ItemSpec);
                return false;
            }

            OutputFiles = new ITaskItem[2];
            OutputFiles[0] = new TaskItem("Parser.cs");
            OutputFiles[1] = new TaskItem("Scanner.cs");

            return Coco.Generate(//Continue here: extract line number and column to forward them to the IDE
                new GeneratorOptions(text => Log.LogMessageFromText(text, MessageImportance.High), _msBuildError)
                {
                    SrcName = Grammar.ItemSpec,
                    DirectDebugTrace = null, 
                    DirectDebug = DirectDebug, 
                    FrameDirectoryPath = FramesDirectory, 
                    Namespace = Namespace,
                    RelativePathRoot = RelativePathRoot
                });
        }

        private void _msBuildError(string ex)
        {
            int idxComma, idxClosing;
            if (ex.StartsWith("(") && (idxComma = ex.IndexOf(',')) > 0 && (idxClosing = ex.IndexOf(')')) > 0 && idxClosing > idxComma)
            {
                var line = int.Parse(ex.Substring(1, idxComma - 1));
                var column = int.Parse(ex.Substring(idxComma + 1, idxClosing - idxComma - 1));
                var idxEndFile = ex.IndexOf("::", StringComparison.InvariantCulture);
                var errorFile = ex.Substring(idxClosing + 1, idxEndFile - idxClosing - 2);
                var errorMessage = ex.Substring(idxEndFile + 2);
                Log.LogMessageFromText(
                    $"line={line}\ncolumn={column}\nerrorFile={errorFile}\nerrorMessage={errorMessage}", MessageImportance.High);
                Log.LogError("grammar", "", "Coco/R", errorFile, line, column, line, column, errorMessage);
            }
            else
                Log.LogError(ex);
        }
    }
}
