using System;
using System.Runtime.Serialization;

namespace Prexonite.Commands.Concurrency
{
    /// <summary>
    /// Indented to convey that the operation that was attempted is not supported. Usually there is a mechanism that allows you to avoid exceptions of this type.
    /// </summary>
    [Serializable]
    public class NotSupportedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NotSupportedException()
        {
        }

        public NotSupportedException(string message) : base(message)
        {
        }

        public NotSupportedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected NotSupportedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}