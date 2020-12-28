#nullable enable

using System;
using System.Diagnostics;

namespace Prexonite.Types
{
    /// <summary>
    ///     Associates a literal with a class. Only interpreted on classes inheriting from <see cref = "Prexonite.Types.PType" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [DebuggerStepThrough]
    public class PTypeLiteralAttribute : Attribute
    {
        /// <summary>
        ///     The literal this attribute represents.
        /// </summary>
        public string Literal { get; }

        /// <summary>
        ///     Creates a new instance of the PTypeLiteral attribute.
        /// </summary>
        /// <param name = "literal">The literal to associate with this type.</param>
        public PTypeLiteralAttribute(string literal)
        {
            Literal = literal;
        }
    }
}