using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PxCoco.Msbuild
{
    public class PxCoco : Microsoft.Build.Utilities.Task
    {

        private ITaskItem _grammar;
        [Required]
        public ITaskItem Grammar
        {
            set { _grammar = value; }
        }

        private ITaskItem[] _outputFiles;
        [Output]
        public ITaskItem[] OutputFiles
        {
            get { return _outputFiles; }
        }

        private string _namespace = null;
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        private string _framesDirectory = null;
        public string FramesDirectory
        {
            get { return _framesDirectory; }
            set { _framesDirectory = value; }
        }

        private bool _directDebug = false;

        public bool DirectDebug
        {
            get { return _directDebug; }
            set { _directDebug = value; }
        }

        public override bool Execute()
        {
            if (_grammar == null)
            {
                Log.LogError("PxCoco requires a grammar file.");
                return false;
            }

            if (!System.IO.File.Exists(_grammar.ItemSpec))
            {
                Log.LogError("The grammar file {0} does not exist", _grammar.ItemSpec);
                return false;
            }

            _outputFiles = new ITaskItem[2];
            _outputFiles[0] = new TaskItem("Parser.cs");
            _outputFiles[1] = new TaskItem("Scanner.cs");

            return at.jku.ssw.Coco.Coco.Generate(
                _grammar.ItemSpec,
                null,
                _framesDirectory,
                _namespace,
                _directDebug,
                delegate(string text) { Log.LogMessageFromText(text, MessageImportance.High); },
                //Continue here: extract line number and column to forward them to the IDE
                delegate(string ex)
                {
                    int line, column, idxComma = -1, idxClosing = -1, idxEndFile;
                    string errorFile;
                    string errorMessage;
                    if (ex.StartsWith("(") && (idxComma = ex.IndexOf(',')) > 0 && (idxClosing = ex.IndexOf(')')) > 0 && idxClosing > idxComma)
                    {
                        line = Int32.Parse(ex.Substring(1, idxComma-1));
                        column = Int32.Parse(ex.Substring(idxComma + 1, idxClosing - idxComma - 1));
                        idxEndFile = ex.IndexOf("::");
                        errorFile = ex.Substring(idxClosing+1, idxEndFile - idxClosing - 2);
                        errorMessage = ex.Substring(idxEndFile + 2); // + 1 because the separator is "::" and + 1 to start from the nect char
                        Log.LogMessageFromText(String.Format("line={0}\ncolumn={1}\nerrorFile={2}\nerrorMessage={3}",line, column, errorFile, errorMessage), MessageImportance.High);
                        Log.LogError("grammar", "", "Coco/R", errorFile, line, column, line, column, errorMessage);
                    }
                    else
                        Log.LogError(ex);
                });
        }
    }
}
