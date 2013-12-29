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

        /// <summary>
        /// Set of symbol entries exported by this local namespace declaration (the contributions of the current module)
        /// </summary>
        /// <remarks>
        /// <para>Use the <see cref="Namespace"/> interface to list all symbols included in this namespace, not just the ones defined in this module.</para>
        /// </remarks>
        [NotNull]
        public abstract IEnumerable<KeyValuePair<string, Symbol>> Exports { get; }

        /// <summary>
        /// Attempt to retrieve a certain symbol from the set of symbols exported by this module.
        /// </summary>
        /// <param name="id">The name of the symbol to retrieve.</param>
        /// <param name="exported">Will contain the symbol if found (non-null) or null if no symbol with that name is exported by the current module.</param>
        /// <returns>true if the symbol is found among the symbols exported by this module; false otherwise</returns>
        /// <remarks><para>This method can fail to return a symbol that is available via <see cref="ISymbolView{T}.TryGet"/>.</para></remarks>
        [ContractAnnotation("=>true,exported:notnull;=>false,exported:null")]
        public abstract bool TryGetExported(string id, out Symbol exported);

        /// <summary>
        /// Adds a set of declarations to the exports of this namespace. Will replace conflicting symbols instead of merging with them.
        /// </summary>
        /// <param name="exportScope">The set of symbols to export.</param>
        public abstract void DeclareExports([NotNull] IEnumerable<KeyValuePair<string, Symbol>> exportScope);

        #endregion

    }
}