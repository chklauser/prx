

using System.Text;

namespace Prexonite.Compiler.Build;

public class BuildFailureException : BuildException
{
    public List<Message> Messages { get; } = new();

    static string _makeErrorMessage(IEnumerable<Message> messages, string messageFormat)
    {
        var e = 0;
        foreach (var message in messages)
        {
            if(message.Severity == MessageSeverity.Error)
                e++;
        }
        return string.Format(messageFormat, e, e == 1 ? "error" : "errors", e == 1 ? "was" : "were");
    }

    public BuildFailureException(ITargetDescription? target, string messageFormat, IEnumerable<Message> messages) : base(_makeErrorMessage(messages, messageFormat),target)
    {
        Messages.AddRange(messages);
    }

    public BuildFailureException(ITargetDescription? target, string messageFormat, IEnumerable<Message> messages, Exception inner)
        : base(_makeErrorMessage(messages, messageFormat), target,inner)
    {
        Messages.AddRange(messages);
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