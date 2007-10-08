using Prexonite.Types;

namespace Prexonite.Commands
{
    /// <summary>
    /// Turns to arguments into a key-value pair
    /// </summary>
    /// <remarks>
    /// Equivalent to:
    /// <code>function pair(key, value) = key: value;</code>
    /// </remarks>
    public class Pair : PCommand
    {
        /// <summary>
        /// Turns to arguments into a key-value pair
        /// </summary>
        /// <param name="args">The arguments to pass to this command. Array must contain 2 elements.</param>
        /// <param name="sctx">Unused.</param>
        /// <remarks>
        /// Equivalent to:
        /// <code>function pair(key, value) = key: value;</code>
        /// </remarks>
        public override PValue Run(StackContext sctx, PValue[] args)
        {
            if (args == null)
                args = new PValue[] {};

            if (args.Length < 2)
                return PType.Null.CreatePValue();
            else
                return PType.Object.CreatePValue(
                    new PValueKeyValuePair(
                        args[0] ?? PType.Null.CreatePValue(),
                        args[1] ?? PType.Null.CreatePValue()
                        ));
        }
    }
}