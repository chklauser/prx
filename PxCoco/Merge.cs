using System;
using System.IO;

namespace at.jku.ssw.Coco
{
    public partial class Coco
    {      
        public static void Merge(string targetPath, string[] sourcePaths, Action<string> logMessage)
        {
            const int bufferSize = 512;
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            //Use a target stream
            using (FileStream ts = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter sw = new StreamWriter(ts))
            {
                sw.WriteLine("//-- GENERATED BY PxCoco -merge --//");
                sw.WriteLine("//-- make sure to modify the source files instead of this one! --//");
                foreach (string sourcePath in sourcePaths)
                {
                    if (!File.Exists(sourcePath))
                    {
                        Console.WriteLine("Source file " + sourcePath + " does not exist.");
                        continue;
                    }
                    string sourceName = Path.GetFileName(sourcePath);
                    logMessage.Invoke(String.Format("Adding {0} to {1}.", sourceName, Path.GetFileName(targetPath)));
                    //Use a source stream
                    using (FileStream ss = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        sw.WriteLine("\n#file:" + Path.GetFullPath(sourcePath) + "#");
                        sw.Flush();
                        while((bytesRead = ss.Read(buffer, 0, bufferSize)) > 0)
                            ts.Write(buffer, 0, bytesRead);                        
                    }
                    
                }
                //Switch line processing back to normal, in case the user wants to add stuff here
                sw.WriteLine("#file:default#");
                sw.Flush();
            }
        }

        public static void MergeCommandLine(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: PxCoco -merge {grammarParts.atg} output.atg");
                return;
            }

            //Add source paths
            string[] sourcePaths = new string[args.Length - 2];
            for (int i = 1; i < args.Length - 1; i++)
                sourcePaths[i - 1] = args[i];

            //Add target path
            string targetPath = args[args.Length - 1];

            Merge(targetPath, sourcePaths, delegate(string text) { Console.WriteLine(text); });
        }
    }
}
