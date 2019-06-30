using System;

namespace at.jku.ssw.Coco
{
    public class GeneratorOptions
    {
        public GeneratorOptions(Action<string> writeMessage, Action<string> writeError)
        {
            WriteMessage = writeMessage;
            WriteError = writeError;
        }

        public string SrcName { get; set; }

        /// <summary>
        /// <list type="table">
        ///     <item>
        ///         <term>F</term>
        ///         <description>list first/follow sets</description>
        ///     </item>
        ///     <item>
        ///         <term>G</term>
        ///         <description>print syntax graph</description>
        ///     </item>
        ///     <item>
        ///         <term>I</term>
        ///         <description>trace computation of first sets</description>
        ///     </item>
        ///     <item>
        ///         <term>J</term>
        ///         <description>list ANY and SYNC sets</description>
        ///     </item>
        ///     <item>
        ///         <term>P</term>
        ///         <description>print statistics</description>
        ///     </item>
        ///     <item>
        ///         <term>S</term>
        ///         <description>list symbol table</description>
        ///     </item>
        ///     <item>
        ///         <term>X</term>
        ///         <description>list cross reference table</description>
        ///     </item>
        /// </list>
        /// </summary>
        public string DirectDebugTrace { get; set; }
        public string FrameDirectoryPath { get; set; }
        public string Namespace { get; set; }
        public bool DirectDebug { get; set; }

        /// <summary>
        /// Optional. The path to use as the root for relative file names in <c>#line</c> directives.
        /// </summary>
        public string RelativePathRoot { get; set; }
        public Action<string> WriteMessage { get; set; }
        public Action<string> WriteError { get; set; }
    }
}