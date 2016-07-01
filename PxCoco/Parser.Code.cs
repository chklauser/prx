using System.IO;
using System.Text;
using System;

namespace at.jku.ssw.Coco
{
    public partial class Parser
    {        
        private bool isPragma()
        {
            return la.kind == _filechg;
        }
        
        public string GetVirtualFile()
        {
            return errors.GetVirtualFile();
        }

        public void ChangeVirtualFile(string newFile)
        {
            if (newFile == null)
            {
                errors.lineOffset = 0;
                //errors.WriteMessage("File(null)" + errors.lineOffset.ToString());
            }
            else
            {
                errors.lineOffset = -t.line+1;
                //errors.WriteMessage("File(" + System.IO.Path.GetFileNameWithoutExtension(newFile) + ")"  + errors.lineOffset.ToString());
                //errors.WriteMessage(String.Format("Token(val=\"{0}\", line={1}, col={2}, kind={3}", la.val, la.line, la.col, la.kind));
            }

            

            errors.virtualFile = newFile;
        }

        public int GetVirtualLine(int physicalLine)
        {
            return errors.GetVirtualLine(physicalLine);
        }

        public int GetVirtualLine()
        {
            return GetVirtualLine(la.line);
        }
    }

    public partial class Errors
    {
        public string virtualFile = null;
        public string realFile;

        public string GetVirtualFile()
        {
            return virtualFile != null ? virtualFile : realFile;
        }

        public int lineOffset = 0;

        public int GetVirtualLine(int physicalLine)
        {
            return physicalLine + lineOffset;
        }
    }
}