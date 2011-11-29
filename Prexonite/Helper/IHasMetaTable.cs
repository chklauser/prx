namespace Prexonite
{
    /// <summary>
    ///     An object that is associated with meta information.
    /// </summary>
    public interface IHasMetaTable
    {
        /// <summary>
        ///     Returns a reference to the meta table associated with the object.
        /// </summary>
        MetaTable Meta { get; }
    }
}