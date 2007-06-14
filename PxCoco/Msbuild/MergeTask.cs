using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;

namespace PxCoco.Msbuild
{
    public class Merge : Microsoft.Build.Utilities.Task
    {

        private ITaskItem _outputFile;
        
        [Output, Required]
        public ITaskItem OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        private ITaskItem[] _inputFiles;

        [Required]
        public ITaskItem[] InputFiles
        {
            get { return _inputFiles; }
            set { _inputFiles = value; }
        }

        public override bool Execute()
        {
            if (_outputFile == null)
            {
                Log.LogError("No output file provided for PxCoco/R -merge");
                return false;
            }

            if (_inputFiles == null || _inputFiles.Length <= 0)
            {
                Log.LogError("No input files defined for PxCoco/R -merge");
                return false;
            }

            string[] inputPaths = new string[_inputFiles.Length];
            for (int i = 0; i < _inputFiles.Length; i++)
                inputPaths[i] = _inputFiles[i].ItemSpec;

            try
            {
                at.jku.ssw.Coco.Coco.Merge(
                    _outputFile.ItemSpec, 
                    inputPaths,
                    delegate(string text) 
                        { Log.LogMessageFromText(text, MessageImportance.Normal); }
                    );
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            return true;
        }
    }
}
