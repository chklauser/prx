using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PxCoco.Msbuild
{
    public static class Platform
    {
#if NETFRAMEWORK
    private static string _platformGetRelativePath(string relativeTo, string path)
    {
        var fromAttr = getPathAttribute(relativeTo);
        var toAttr = getPathAttribute(path);

        var relativePath = new StringBuilder(260); // MAX_PATH
        if(PathRelativePathTo(
               relativePath,
               relativeTo,
               fromAttr,
               path,
               toAttr) == 0)
        {
            throw new ArgumentException("Paths must have a common prefix");
        }
        return relativePath.ToString();
    }

    private static int getPathAttribute(string path)
    {
        var di = new DirectoryInfo(path);
        if (di.Exists)
        {
            return FILE_ATTRIBUTE_DIRECTORY;
        }

        var fi = new FileInfo(path);
        if(fi.Exists)
        {
            return FILE_ATTRIBUTE_NORMAL;
        }

        throw new FileNotFoundException();
    }

    private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
    private const int FILE_ATTRIBUTE_NORMAL = 0x80;

    [DllImport("shlwapi.dll", SetLastError = true)]
    private static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);

#else
        private static string _platformGetRelativePath(string relativeTo, string path)
        {
            return Path.GetRelativePath(relativeTo, path);
        }
#endif
        public static string GetRelativePath(string relativeTo, string path) => _platformGetRelativePath(relativeTo, path);
    }
}