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
using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Prexonite.Properties;

namespace Prexonite.Compiler.Build.Internal
{
    internal class StreamSource : ISource, IDisposable
    {
        private readonly Encoding _encoding;
        private readonly bool _forceSingleUse;
        private Stream _stream;

        public StreamSource([NotNull] Stream stream, [NotNull] Encoding encoding, bool forceSingleUse)
        {
            if ((object) stream == null)
                throw new ArgumentNullException("stream");
            if(!stream.CanRead)
                throw new ArgumentException(Resources.Exception_StreamSource_CannotUseWriteOnlyStream, "stream");
            if ((object) encoding == null)
                throw new ArgumentNullException("encoding");

            _stream = stream;
            _encoding = encoding;
            _forceSingleUse = forceSingleUse;
        }

        #region Implementation of ISource

        public bool CanOpen
        {
            get
            {
                var stream = _stream;
                return stream != null && stream.CanRead;
            }
        }

        public bool IsSingleUse
        {
            get { return _forceSingleUse || _stream == null || !_stream.CanSeek; }
        }

        public bool TryOpen(out TextReader reader)
        {
            object streamObject = _stream;
            if(_stream == null)
            {
                reader = null;
                return false;
            }
            else
            {
                lock (streamObject)
                {
                    if(_stream == null)
                    {
                        reader = null;
                        return false;
                    }
                    else
                    {
                        reader = new StreamReader(_stream,_encoding);
                        if (IsSingleUse)
                            _stream = null;
                        return true;
                    }
                }
            }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            var d = _stream;
            if(d != null)
            {
                lock (d)
                {
                    if (_stream != null)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }
                }
            }
        }

        #endregion
    }
}