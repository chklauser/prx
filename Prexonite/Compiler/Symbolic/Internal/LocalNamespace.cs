using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Prexonite.Compiler.Symbolic.Internal
{
    /// <summary>
    /// A module-local namespace declaration. 
    /// Inherits symbols from namespaces with the same name exported from referenced modules.
    /// </summary>
    /// <remarks>
    /// To create a new instance, use <see cref="ModuleLevelView.CreateLocalNamespace"/> on any of 
    /// the module level view associated with the module that the namespace is supposed to be local to.
    /// </remarks>
    internal abstract class LocalNamespace : Namespace
    {
        #region Physical namespace

        /// <summary>
        /// The prefix to use for physical names. Can only be set once.
        /// </summary>
        /// <remarks>
        /// Use <see cref="DerivePhysicalName"/> to generate physical names based on this prefix.
        /// </remarks>
        [CanBeNull]
        public abstract string Prefix { get; set; }

        /// <summary>
        /// Uses the <see cref="Prefix"/> to derive a physical name for a given logical name. 
        /// </summary>
        /// <param name="logicalName">The logical name to derive a physical name from.</param>
        /// <returns>A physical name, related to the logical name provided.</returns>
        /// <exception cref="InvalidOperationException"><see cref="Prefix"/> has not been assigned yet.</exception>
        [NotNull]
        public string DerivePhysicalName([NotNull] String logicalName)
        {
            if (logicalName == null)
                throw new ArgumentNullException("logicalName");
            if (Prefix == null)
                throw new InvalidOperationException(
                    "Cannot derive physical name before a perfix has been assigned to the namespace.");

            return Prefix + "\\" + logicalName;
        }

        #endregion
        
        #region Exports (logical namespace)

        [NotNull]
        public abstract IEnumerable<KeyValuePair<string, Symbol>> Exports { get; }

        /// <summary>
        /// Adds a set of declarations to the exports of this namespace. Will replace conflicting symbols instead of merging with them.
        /// </summary>
        /// <param name="exportScope">The set of symbols to export.</param>
        public abstract void DeclareExports([NotNull] IEnumerable<KeyValuePair<string, Symbol>> exportScope);

        #endregion

    }
}