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

using System.Reflection;
using System.Text;
using Prexonite.Compiler.Build.Internal;

namespace Prexonite.Compiler.Build;

public static class Source
{
    public static ISource FromReader(TextReader reader)
    {
        return new ReaderSource(reader);
    }

    public static ISource FromString(string source)
    {
        return new StringSource(source);
    }

    public static ISource FromStream(Stream stream, Encoding encoding)
    {
        return FromStream(stream, encoding, true);
    }

    public static ISource FromStream(Stream stream, Encoding encoding, bool forceSingleUse)
    {
        return new StreamSource(stream, encoding, forceSingleUse);
    }

    public static ISource FromFile(FileInfo file, Encoding encoding)
    {
        return new FileSource(file, encoding);
    }

    public static ISource FromFile(string path, Encoding encoding)
    {
        return FromFile(new FileInfo(path), encoding);
    }

    public static ISource FromBytes(byte[] data, Encoding encoding)
    {
        return FromStream(new MemoryStream(data, false), encoding, false);
    }

    public static ISource FromEmbeddedPrexoniteResource(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new EmbeddedResourceSource(Assembly.GetExecutingAssembly(), "Prexonite." + name);
    }
    
    public static ISource FromEmbeddedResource(Assembly assembly, string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new EmbeddedResourceSource(assembly, name);
    }

    public static async Task<ISource> CacheInMemoryAsync(this ISource source)
    {
        if(!source.TryOpen(out var reader))
            throw new InvalidOperationException("Unable to open source " + source + " for reading.");
        var contents = await reader.ReadToEndAsync();
        return FromString(contents);
    }
}