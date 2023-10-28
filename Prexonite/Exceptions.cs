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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Prexonite.Compiler;

namespace Prexonite
{
    namespace Compiler
    {
        /// <summary>
        ///     Thrown when the compiler detected an invalid state. 
        ///     You must consider both the <see cref = "Loader" /> and the <see cref = "Application" /> to be corrupt.
        /// </summary>
        [Serializable]
        public class FatalCompilerException : PrexoniteException
        {
            //
            // For guidelines regarding the creation of new exception types, see
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
            // and
            //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
            //

            /// <summary>
            ///     Creates a new instance of <see cref = "FatalCompilerException" />.
            /// </summary>
            public FatalCompilerException()
            {
            }

            /// <summary>
            ///     Creates a new instance of <see cref = "FatalCompilerException" /> with a custom message.
            /// </summary>
            /// <param name = "message">The message to report.</param>
            public FatalCompilerException(string message)
                : base(message)
            {
            }

            /// <summary>
            ///     Creates a new instance of <see cref = "FatalCompilerException" /> with a custom message and and an inner exception.
            /// </summary>
            /// <param name = "message">The message to report.</param>
            /// <param name = "inner">The exception that caused this exception.</param>
            public FatalCompilerException(string message, Exception inner)
                : base(message, inner)
            {
            }

            /// <summary>
            ///     Creates an in-memory instance from an already existing but serialized instance of <see cref = "FatalCompilerException" />.
            /// </summary>
            /// <param name = "info">The <see cref = "SerializationInfo" />.</param>
            /// <param name = "context">The <see cref = "StreamingContext" /> of this particular instance.</param>
            protected FatalCompilerException(
                SerializationInfo info,
                StreamingContext context)
                : base(info, context)
            {
            }
        }
    }

    /// <summary>
    ///     The generic base class of all exceptions raised by the Prexonite library.
    /// </summary>
    /// <remarks>
    ///     Even exceptional errors during compilation are reported as <see cref = "PrexoniteException" /> if they are not considered 'fatal'.
    /// </remarks>
    [Serializable]
    public class PrexoniteException : Exception
    {
        /// <summary>
        ///     Creates a new instance of <see cref = "PrexoniteException" />.
        /// </summary>
        public PrexoniteException()
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref = "PrexoniteException" /> with a custom message.
        /// </summary>
        /// <param name = "message">The custom message.</param>
        public PrexoniteException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref = "PrexoniteException" /> with a custom message as well as an inner exception.
        /// </summary>
        /// <param name = "message">The custom message.</param>
        /// <param name = "innerException">The inner exception.</param>
        public PrexoniteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Creates an in-memory instance from an already existing but serialized instance of <see cref = "PrexoniteException" />.
        /// </summary>
        /// <param name = "info">The <see cref = "SerializationInfo" />.</param>
        /// <param name = "context">The <see cref = "StreamingContext" /> of this particular instance.</param>
        protected PrexoniteException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    ///     A wrapper around exceptions caused by the code executed by the virtual machine.
    ///     It provides a stack trace of Prexonite <see cref = "StackContext" />s.
    /// </summary>
    /// <remarks>
    ///     Use an overload of the static method CreateRuntimeException to create an instance with a stack trace.
    /// </remarks>
    [Serializable]
    public class PrexoniteRuntimeException : PrexoniteException
    {
        /// <summary>
        ///     Creates a new instance of PrexoniteRuntimeException without a stack trace and a custom message.
        /// </summary>
        /// <param name = "message">A custom message</param>
        protected PrexoniteRuntimeException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Creates a new instance of PrexoniteRuntimeException without a stack trace and a custom message.
        /// </summary>
        /// <param name = "message">A custom message</param>
        /// <param name = "innerException">The inner exception</param>
        protected PrexoniteRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Provides access to the stack trace recorded when creating the exception.
        ///     This field can be null if the instance has been created without a stack trace.
        /// </summary>
        public string PrexoniteStackTrace { get; }

        PrexoniteRuntimeException(
            string message, Exception innerException, string prexoniteStackTrace)
            : base(message, innerException)
        {
            PrexoniteStackTrace = prexoniteStackTrace;
        }

        /// <summary>
        ///     Creates a new instance of <see cref = "PrexoniteRuntimeException" /> with a stack trace based on <paramref
        ///      name = "esctx" />.
        /// </summary>
        /// <param name = "esctx">The stack context that caused the exception.</param>
        /// <param name = "message">The custom message to be displayed.</param>
        /// <param name = "innerException">The inner exception.</param>
        /// <returns>An instance of <see cref = "PrexoniteRuntimeException" /> with a stack trace.</returns>
        public static PrexoniteRuntimeException CreateRuntimeException(
            StackContext esctx, string message, Exception innerException)
        {
            if (esctx == null)
                throw new ArgumentNullException(nameof(esctx));
            message ??= "An error occured at runtime.";

            var builder = new StringBuilder();
            var stack = new List<StackContext>(esctx.ParentEngine.Stack);
            for (var i = stack.Count - 1; i >= 0; i--)
            {
                var sctx = stack[i];
                var fctx = sctx as FunctionContext;
                builder.Append("   at ");
                if (fctx == null)
                {
                    builder.Append(sctx);
                }
                else
                {
                    var func = fctx.Implementation;
                    var code = func.Code;
                    var pointer = fctx.Pointer - 1;

                    builder.Append("function ");

                    builder.Append(func.Meta.GetDefault(PFunction.LogicalIdKey, func.Id).Text);

                    builder.Append(" module ");
                    builder.Append(func.ParentApplication.Module.Name);

                    if (0 <= pointer && pointer < code.Count)
                    {
                        builder.Append(" around instruction ");

                        builder.Append(pointer);
                        builder.Append(": ");
                        builder.Append(code[pointer]);

                        var sm = SourceMapping.Load(fctx.Implementation);
                        if (sm.TryGetValue(pointer, out var pos))
                        {
                            builder.AppendFormat(" (in {0}, on line {1}, col {2})", pos.File,
                                pos.Line, pos.Column);
                        }
                    }
                }
                builder.Append("\n");
            }

            return new PrexoniteRuntimeException(message, innerException, builder.ToString());
        }

        /// <summary>
        ///     Creates a new instance of <see cref = "PrexoniteRuntimeException" /> with a stack trace based on <paramref
        ///      name = "sctx" />.
        /// </summary>
        /// <param name = "sctx">The stack context that caused the exception.</param>
        /// <param name = "message">The custom message to be displayed.</param>
        /// <returns>An instance of <see cref = "PrexoniteRuntimeException" /> with a stack trace.</returns>
        public static PrexoniteRuntimeException CreateRuntimeException(
            StackContext sctx, string message)
        {
            return CreateRuntimeException(sctx, message, null);
        }

        /// <summary>
        ///     Creates a new instance of <see cref = "PrexoniteRuntimeException" /> with a stack trace based on <paramref
        ///      name = "sctx" />.
        /// </summary>
        /// <param name = "sctx">The stack context that caused the exception.</param>
        /// <param name = "innerException">The inner exception.</param>
        /// <returns>An instance of <see cref = "PrexoniteRuntimeException" /> with a stack trace.</returns>
        public static PrexoniteRuntimeException CreateRuntimeException(
            StackContext sctx, Exception innerException)
        {
            return CreateRuntimeException(sctx, innerException.Message, innerException);
        }

        /// <summary>
        ///     Returns the standard string representation of <see cref = "PrexoniteException" /> with the stack trace appended.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(base.ToString(), "\n:: Prexonite Stack:\n", PrexoniteStackTrace);
        }

        /// <summary>
        ///     Tries to unwrap the inner exception of the supplied <see cref = "PrexoniteRuntimeException" /> so
        ///     the inner exception of the returned <see cref = "PrexoniteRuntimeException" /> contains relevant information.
        /// </summary>
        /// <remarks>
        ///     The method replaces <see cref = "PrexoniteRuntimeException" />s as well as 
        ///     <see cref = "TargetInvocationException" />s with their inner exceptions (unless they are null). 
        ///     In some cases, a new <see cref = "PrexoniteRuntimeException" /> instance is created.
        /// </remarks>
        /// <param name = "pExc">Any <see cref = "PrexoniteRuntimeException" />.</param>
        /// <returns>An instance of <see cref = "PrexoniteRuntimeException" /> with a relevant inner exception.</returns>
        public static PrexoniteRuntimeException UnpackException(PrexoniteRuntimeException pExc)
        {
            if (pExc == null)
                throw new ArgumentNullException(nameof(pExc));

            //Exception exc = pExc;
            //Exception lastExc = null;

            //while (
            //    (exc is PrexoniteRuntimeException || exc is TargetInvocationException) &&
            //    exc.InnerException != null)
            //{
            //    var ipexc = exc as PrexoniteRuntimeException;
            //    if (ipexc != null)
            //        lastpExc = ipexc;
            //    lastExc = exc;
            //    exc = exc.InnerException;
            //}

            //if (ReferenceEquals(exc, pExc))
            //    return pExc; //No unpacking needed

            //if (ReferenceEquals(lastExc, pExc))
            //    return pExc; //Use lowest prexonite runtime exception
            //else 
            //    //Construct new runtime exception
            //    return
            //        new PrexoniteRuntimeException(exc.Message, exc, pExc._prexoniteStackTrace);

            if (pExc.InnerException == null)
                return pExc;

            _unpack(pExc, out var lowestException, out var lowestRuntimeException);

            //Check if something changed
            if (ReferenceEquals(lowestException, pExc.InnerException) &&
                ReferenceEquals(lowestRuntimeException, pExc))
                return pExc;

            return new PrexoniteRuntimeException(lowestException.Message, lowestException,
                lowestRuntimeException.PrexoniteStackTrace);
        }

        static void _unpack(PrexoniteRuntimeException originalException,
            out Exception lowestException, out PrexoniteRuntimeException lowestRuntimeException)
        {
            Exception exc = originalException;
            TargetInvocationException targetInvocationExc = null;
            lowestRuntimeException = originalException;

            while (
                (exc is PrexoniteRuntimeException ||
                    (targetInvocationExc = exc as TargetInvocationException) != null)
                        && exc.InnerException != null)
            {
                if (targetInvocationExc == null)
                    lowestRuntimeException = (PrexoniteRuntimeException) exc;
                exc = exc.InnerException;
            }

            lowestRuntimeException = exc as PrexoniteRuntimeException ?? lowestRuntimeException;
            lowestException = exc;
        }

        protected PrexoniteRuntimeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class InvalidCallException : PrexoniteException
    {
        public InvalidCallException()
        {
        }

        public InvalidCallException(string message)
            : base(message)
        {
        }

        public InvalidCallException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidCallException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class InvalidConversionException : PrexoniteException
    {
        public InvalidConversionException()
        {
        }

        public InvalidConversionException(string message)
            : base(message)
        {
        }

        public InvalidConversionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidConversionException(
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