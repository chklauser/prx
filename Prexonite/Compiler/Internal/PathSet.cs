using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Prexonite.Compiler.Internal;

/// <summary>
/// A set of file system paths. Tries to normalize paths (on a best-effort basis).
/// </summary>
/// <remarks>
/// This class is aware of <see cref="ResourceSpec"/>.</remarks>
sealed class PathSet : ICollection<string>
{
    readonly HashSet<string> _paths = new();
    public IEnumerator<string> GetEnumerator()
    {
        return _paths.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_paths).GetEnumerator();
    }

    public void Add(string path) => _paths.Add(canonical(path));

    public void Clear()
    {
        _paths.Clear();
    }

    public bool Contains(string path) => _paths.Contains(canonical(path));

    public void CopyTo(string[] array, int arrayIndex)
    {
        _paths.CopyTo(array, arrayIndex);
    }

    public bool Remove(string path) => _paths.Remove(canonical(path));

    public int Count => _paths.Count;

    bool ICollection<string>.IsReadOnly => ((ICollection<string>)_paths).IsReadOnly;

    static string canonical(string path)
    {
        if (!path.StartsWith(ResourceSpec.Prefix))
        {
            path = Path.GetFullPath(path);
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsWindows())
            {
                // PRX-58 This generalization is of course not correct, but there currently is no convenient API
                // to get a canonical path.
                path = path.ToUpperInvariant();
            }
        }

        return path;
    }
}