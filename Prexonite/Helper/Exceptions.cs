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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Prexonite.Compiler;

namespace Prexonite
{
    namespace Compiler
    {
        /// <summary>
        /// Thrown when the compiler detected an invalid state. 
        /// You must consider both the <see cref="Loader"/> and the <see cref="Application"/> to be corrupt.
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
            /// Creates a new instance of <see cref="FatalCompilerException"/>.
            /// </summary>
            public FatalCompilerException()
            {
            }

            /// <summary>
            /// Creates a new instance of <see cref="FatalCompilerException"/> with a custom message.
            /// </summary>
            /// <param name="message">The message to report.</param>
            public FatalCompilerException(string message)
                : base(message)
            {
            }

            /// <summary>
            /// Creates a new instance of <see cref="FatalCompilerException"/> with a custom message and and an inner exception.
            /// </summary>
            /// <param name="message">The message to report.</param>
            /// <param name="inner">The exception that caused this exception.</param>
            public FatalCompilerException(string message, Exception inner)
                : base(message, inner)
            {
            }

            /// <summary>
            /// Creates an in-memory instance from an already existing but serialized instance of <see cref="FatalCompilerException"/>.
            /// </summary>
            /// <param name="info">The <see cref="SerializationInfo"/>.</param>
            /// <param name="context">The <see cref="StreamingContext"/> of this particular instance.</param>
            protected FatalCompilerException(
                SerializationInfo info,
                StreamingContext context)
                : base(info, context)
            {
            }
        }
    }

    /// <summary>
    /// The generic base class of all exceptions rised by the Prexonite library.
    /// </summary>
    /// <remarks>Even exceptional errors during compilation are reported as <see cref="PrexoniteException"/> if they are not considerer 'fatal'.</remarks>
    [Serializable]
    public class PrexoniteException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="PrexoniteException"/>.
        /// </summary>
        public PrexoniteException()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="PrexoniteException"/> with a custom message.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public PrexoniteException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="PrexoniteException"/> with a custom message as well as an inner exception.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PrexoniteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an in-memory instance from an already existing but serialized instance of <see cref="PrexoniteException"/>.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/>.</param>
        /// <param name="context">The <see cref="StreamingContext"/> of this particular instance.</param>
        public PrexoniteException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// A wrapper around exceptions caused by the code executed by the virtual machine.
    /// It provides a stack trace of Prexonite <see cref="StackContext"/>s.
    /// </summary>
    /// <remarks>Use an overload of the static method CreateRuntimeException to create an instance with a stack trace.</remarks>
    [Serializable]
    public class PrexoniteRuntimeException : PrexoniteException
    {
        /// <summary>
        /// Creates a new instance of PrexoniteRuntimeException without a stack trace and a custom message.
        /// </summary>
        /// <param name="message">A custom message</param>
        protected PrexoniteRuntimeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of PrexoniteRuntimeException without a stack trace and a custom message.
        /// </summary>
        /// <param name="message">A custom message</param>
        /// <param name="innerException">The inner exception</param>
        protected PrexoniteRuntimeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Provides access to the stack trace recorded when creating the exception.
        /// This field can be null if the instance has beeen created without a stack trace.
        /// </summary>
        public string PrexoniteStackTrace
        {
            get { return _prexoniteStackTrace; }
        }

        private readonly string _prexoniteStackTrace;

        private PrexoniteRuntimeException(
            string message, Exception innerException, string prexoniteStackTrace)
            : base(message, innerException)
        {
            _prexoniteStackTrace = prexoniteStackTrace;
        }

        /// <summary>
        /// Creates a new instance of <see cref="PrexoniteRuntimeException"/> with a stack trace based on <paramref name="esctx"/>.
        /// </summary>
        /// <param name="esctx">The stack context that caused the exception.</param>
        /// <param name="message">The custom message to be displayed.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns>An instance of <see cref="PrexoniteRuntimeException"/> with a stack trace.</returns>
        public static PrexoniteRuntimeException CreateRuntimeException(
            StackContext esctx, string message, Exception innerException)
        {
            if (esctx == null)
                throw new ArgumentNullException("esctx");
            if (message == null)
                message = "An error occured at runtime.";

            var builder = new StringBuilder();
            var stack = new List<StackContext>(esctx.ParentEngine.Stack);
            for (var i = stack.Count - 1; i >= 0; i--)
            {
                var sctx = stack[i];
                var fctx = sctx as FunctionContext;
                builder.Append("   at ");
                if (fctx == null)
                {
                    builder.Append(sctx.ToString());
                }
                else
                {
                    var func = fctx.Implementation;
                    var code = func.Code;
                    var pointer = fctx.Pointer - 1;

                    builder.Append("function ");

                    builder.Append(func.Meta.GetDefault(PFunction.LogicalIdKey, func.Id).Text);

                    if (pointer < code.Count)
                    {
                        builder.Append(" around instruction ");

                        builder.Append(pointer);
                        builder.Append(": ");
                        builder.Append(code[pointer]);

                        var sm = SourceMapping.Load(fctx.Implementation);
                        ISourcePosition pos;
                        if(sm.TryGetValue(pointer, out pos))
                        {
                            builder.AppendFormat(" (in {0}, on line {1}, col {2})", pos.File, pos.Line, pos.Column);
                        }
                    }
                }
                builder.Append("\n");
            }

            return new PrexoniteRuntimeException(message, innerException, builder.ToString());
        }

        /// <summary>
        /// Creates a new instance of <see cref="PrexoniteRuntimeException"/> with a stack trace based on <paramref name="sctx"/>.
        /// </summary>
        /// <param name="sctx">The stack context that caused the exception.</param>
        /// <param name="message">The custom message to be displayed.</param>
        /// <returns>An instance of <see cref="PrexoniteRuntimeException"/> with a stack trace.</returns>
        public static PrexoniteRuntimeException CreateRuntimeException(
            StackContext sctx, string message)
        {
            return CreateRuntimeException(sctx, message, null);
        }

        /// <summary>
        /// Creates a new instance of <see cref="PrexoniteRuntimeException"/> with a stack trace based on <paramref name="sctx"/>.
        /// </summary>
        /// <param name="sctx">The stack context that caused the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns>An instance of <see cref="PrexoniteRuntimeException"/> with a stack trace.</returns>
        public static PrexoniteRuntimeException CreateRuntimeException(
            StackContext sctx, Exception innerException)
        {
            return CreateRuntimeException(sctx, innerException.Message, innerException);
        }

        /// <summary>
        /// Returns the standard string representation of <see cref="PrexoniteException"/> with the stack trace appended.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(base.ToString(), "\n:: Prexonite Stack:\n", _prexoniteStackTrace);
        }

        /// <summary>
        /// Tries to unwrap the inner exception of the supplied <see cref="PrexoniteRuntimeException"/> so
        /// the inner exception of the returned <see cref="PrexoniteRuntimeException"/> contains relevant information.
        /// </summary>
        /// <remarks>The method replaces <see cref="PrexoniteRuntimeException"/>s as well as 
        /// <see cref="TargetInvocationException"/>s with their inner exceptions (unless they are null). 
        /// In some cases, a new <see cref="PrexoniteRuntimeException"/> instance is created.</remarks>
        /// <param name="pExc">Any <see cref="PrexoniteRuntimeException"/>.</param>
        /// <returns>An instance of <see cref="PrexoniteRuntimeException"/> with a relevant inner exception.</returns>
        public static PrexoniteRuntimeException UnpackException(PrexoniteRuntimeException pExc)
        {
            if (pExc == null)
                throw new ArgumentNullException("pExc");

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

            PrexoniteRuntimeException lowestRuntimeException;
            Exception lowestException;

            if (pExc.InnerException == null)
                return pExc;

            _unpack(pExc, out lowestException, out lowestRuntimeException);

            //Check if something changed
            if (ReferenceEquals(lowestException, pExc.InnerException) && ReferenceEquals(lowestRuntimeException, pExc))
                return pExc;

            return new PrexoniteRuntimeException(lowestException.Message, lowestException, lowestRuntimeException._prexoniteStackTrace);
        }

        private static void _unpack(PrexoniteRuntimeException originalException, out Exception lowestException, out PrexoniteRuntimeException lowestRuntimeException)
        {
            Exception exc = originalException;
            TargetInvocationException targetInvocationExc = null;
            lowestRuntimeException = originalException;

            while (
                    (exc is PrexoniteRuntimeException || (targetInvocationExc = exc as TargetInvocationException) != null)
                    && exc.InnerException != null)
            {
                if (targetInvocationExc == null)
                    lowestRuntimeException = (PrexoniteRuntimeException) exc;
                exc = exc.InnerException;
            }

            lowestRuntimeException = (exc as PrexoniteRuntimeException) ?? lowestRuntimeException;
            lowestException = exc;
        }

        public PrexoniteRuntimeException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
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

    [Serializable]
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