using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Prexonite.Compiler.Build
{
    public class BuildFailureException : BuildException
    {
        private readonly List<Message> _messages = new List<Message>();

        public List<Message> Messages
        {
            get { return _messages; }
        }

        private static String _makeErrorMessage(IEnumerable<Message> messages, string messageFormat)
        {
            var e = 0;
            foreach (var message in messages)
            {
                if(message.Severity == MessageSeverity.Error)
                    e++;
            }
            return String.Format(messageFormat, e, e == 1 ? "error" : "errors", e == 1 ? "was" : "were");
        }

        public BuildFailureException(ITargetDescription target, string messageFormat, IEnumerable<Message> messages) : base(_makeErrorMessage(messages, messageFormat),target)
        {
            _messages.AddRange(messages);
        }

        public BuildFailureException(ITargetDescription target, string messageFormat, IEnumerable<Message> messages, Exception inner)
            : base(_makeErrorMessage(messages, messageFormat), target,inner)
        {
            _messages.AddRange(messages);
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.AppendLine(base.ToString());
            b.Append(":: Prexonite messages:");
            foreach (var message in Messages)
            {
                b.AppendLine();
                b.Append(message);
            }
            return b.ToString();
        }
    }
}
