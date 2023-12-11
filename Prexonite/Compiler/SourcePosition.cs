// Prexonite
// 
// Copyright (c) 2014, Christian Klauser
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, 
//  are permitted provided that the following conditions are met:
// 
//     Redistributions of source code must retain the above copyright notice, 
//          this list of conditions and the following disclaimer.
//     Redistributions in binary form must reproduce the above copyright notice, 
//          this list of conditions and the following disclaimer in the 
//          documentation and/or other materials provided with the distribution.
//     The names of the contributors may be used to endorse or 
//          promote products derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
//  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
//  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING 
//  IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Prexonite.Compiler;

public class SourcePosition : ISourcePosition, IEquatable<SourcePosition>
{
    public SourcePosition(string? file, int line, int column)
    {
        File = file ?? "unknown~";
        Line = line;
        Column = column;
    }

    #region Implementation of ISourcePosition

    /// <summary>
    ///     The source file that declared this object.
    /// </summary>
    public string File { get; }

    /// <summary>
    ///     The line in the source file that declared this object.
    /// </summary>
    public int Line { get; }

    /// <summary>
    ///     The column in the source file that declared this object.
    /// </summary>
    public int Column { get; }

    #endregion

    /// <summary>
    ///     Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <returns>
    ///     true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
    /// </returns>
    /// <param name = "other">An object to compare with this object.</param>
    public bool Equals(SourcePosition? other)
    {
        return !ReferenceEquals(null, other);
    }

    /// <summary>
    ///     Determines whether the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     true if the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />; otherwise, false.
    /// </returns>
    /// <param name = "obj">The <see cref = "T:System.Object" /> to compare with the current <see cref = "T:System.Object" />. </param>
    /// <filterpriority>2</filterpriority>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof (SourcePosition)) return false;
        return Equals((SourcePosition) obj);
    }

    /// <summary>
    ///     Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    ///     A hash code for the current <see cref = "T:System.Object" />.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode()
    {
        return (File ?? string.Empty).GetHashCode() ^ Line ^ (Column + 256);
    }

    public override string ToString()
    {
        return File != null
            ? $"{File} {Line}.{Column}"
            : $"{Line}.{Column}";
    }
}