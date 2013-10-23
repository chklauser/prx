using JetBrains.Annotations;

namespace Prexonite.Compiler
{
    [PublicAPI]
    public interface IMessageSink
    {
        /// <summary>
        /// Reports a compiler message (error, warning, info).
        /// </summary>
        /// <param name="message">The message to be reported.</param>
        /// <remarks>
        /// <para>Issuing an error message does not automatically abort execution of the macro.</para>
        /// <para>Messages should always have a message class. Especially warings and infos (that way, the user can filter undesired warnings/infos)</para>
        /// </remarks>
        [PublicAPI]
        void ReportMessage([NotNull] Message message);
    }
}