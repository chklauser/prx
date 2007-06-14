/*
 * Prexonite, a scripting engine (Scripting Language -> Bytecode -> Virtual Machine)
 *  Copyright (C) 2007  Christian "SealedSun" Klauser
 *  E-mail  sealedsun a.t gmail d.ot com
 *  Web     http://www.sealedsun.ch/
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  Please contact me (sealedsun a.t gmail do.t com) if you need a different license.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Prexonite
{
    namespace Compiler
    {
        [Serializable]
        public class FatalCompilerException : PrexoniteException
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //
            public FatalCompilerException()
            {
            }

            public FatalCompilerException(string message)
                : base(message)
            {
            }

            public FatalCompilerException(string message, Exception inner)
                : base(message, inner)
            {
            }

            protected FatalCompilerException(
                SerializationInfo info,
                StreamingContext context)
                : base(info, context)
            {
            }
        }
    }

    [Serializable]
    public class PrexoniteException : Exception
    {
        public PrexoniteException()
        {
        }

        public PrexoniteException(string message)
            : base(message)
        {
        }

        public PrexoniteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PrexoniteException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class PrexoniteRuntimeException : PrexoniteException
    {
        public PrexoniteRuntimeException(string message)
            : base(message)
        {
        }

        public PrexoniteRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string PrexoniteStackTrace
        {
            get { return _prexoniteStackTrace; }
        }
        private string _prexoniteStackTrace;

        private PrexoniteRuntimeException(string message, Exception innerException, string prexoniteStackTrace)
            : base (message, innerException)
        {
            _prexoniteStackTrace = prexoniteStackTrace;   
        }

        public static PrexoniteRuntimeException CreateRuntimeException(StackContext esctx, string message, Exception innerException)
        {
            if (esctx == null)
                throw new ArgumentNullException("sctx");
            if (message == null)
                message = "An error occured at runtime.";

            StringBuilder builder = new StringBuilder();
            List<StackContext> stack = new List<StackContext>(esctx.ParentEngine.Stack);
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                StackContext sctx = stack[i];
                builder.Append("   at ");
                builder.Append(sctx.Implementation);
                builder.Append("\n");
            }

            return new PrexoniteRuntimeException(message,innerException,builder.ToString());
        }

        public static PrexoniteRuntimeException CreateRuntimeException(StackContext sctx, string message)
        {
            return CreateRuntimeException(sctx, message, null);
        }

        public static PrexoniteRuntimeException CreateRuntimeException(StackContext sctx, Exception innerException)
        {
            return CreateRuntimeException(sctx, innerException.Message, innerException);
        }

        public override string ToString()
        {
            return String.Concat(base.ToString() , "\n:: Prexonite Stack:\n" , _prexoniteStackTrace);
        }

        public static PrexoniteRuntimeException UnpackException(PrexoniteRuntimeException pexc)
        {
            if (pexc == null)
                throw new ArgumentNullException("exc"); 
            Exception exc = pexc;
            PrexoniteRuntimeException lpexc = pexc;
            Exception lexc = pexc;
            while (
                (exc is PrexoniteRuntimeException || exc is System.Reflection.TargetInvocationException) &&
                exc.InnerException != null)
            {
                PrexoniteRuntimeException ipexc = exc as PrexoniteRuntimeException;
                if (ipexc != null)
                    lpexc = ipexc;
                lexc = exc;
                exc = exc.InnerException;
            }

            if(ReferenceEquals(exc,pexc))
                return pexc; //No unpacking needed

            if (ReferenceEquals(lexc, lpexc))
                return lpexc; //Use lowest prexonite runtime exception
            else 
                //Construct new runtime exception
                return new PrexoniteRuntimeException(exc.Message, exc, lpexc._prexoniteStackTrace);
        }

        public PrexoniteRuntimeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable()]
    public class InvalidCallException : PrexoniteException
    {
        public InvalidCallException(string message)
            : base(message)
        {
        }

        public InvalidCallException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidCallException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable()]
    public class InvalidConversionException : PrexoniteException
    {
        public InvalidConversionException(string message)
            : base(message)
        {
        }

        public InvalidConversionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public InvalidConversionException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class SymbolNotFoundException : PrexoniteException
    {
        public SymbolNotFoundException()
        {
        }

        public SymbolNotFoundException(string message)
            : base(message)
        {
        }

        public SymbolNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SymbolNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class ExecutionProhibitedException : PrexoniteException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
        public ExecutionProhibitedException()
        {
        }

        public ExecutionProhibitedException(string message)
            : base(message)
        {
        }

        public ExecutionProhibitedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ExecutionProhibitedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}