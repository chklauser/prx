using System.Collections;

namespace Prexonite;

public abstract class PFunctionTable : ICollection<PFunction>
{
    /// <summary>
    /// Indicates whether a function with the specified id exists in the table.
    /// </summary>
    /// <param name="id">The id to search the table for.</param>
    /// <returns>True if the table contains a function with the supplied id; false otherwise.</returns>
    public abstract bool Contains(string id);

    /// <summary>
    /// Attempts to retrieve a function with the specified id.
    /// </summary>
    /// <param name="id">The id of the function to find.</param>
    /// <param name="func">The function with a matching id.</param>
    /// <returns>True if a function was found; false otherwise.</returns>
    public abstract bool TryGetValue(string id, [NotNullWhen(true)] out PFunction? func);

    /// <summary>
    /// Looks up indvidual functions in the table.
    /// </summary>
    /// <param name="id">The id of the function to look up</param>
    /// <returns>The function with a matching id, or null if there is no such function in the table.</returns>
    public abstract PFunction? this[string id] { get; }

    /// <summary>
    /// Writes a serial representation of this module into the <see cref="TextWriter"/> provided.
    /// </summary>
    /// <param name="writer">The text writer to write the representation to.</param>
    public abstract void Store(TextWriter writer);

    /// <summary>
    /// Adds a function to the table. Throws an exception if the slot is occupied.
    /// </summary>
    /// <param name="item">The function to add to the table.</param>
    /// <exception cref="ArgumentException">Function with the same id already exists in the table.</exception>
    /// <exception cref="ArgumentNullException">item is null.</exception>
    public abstract void Add(PFunction item);

    /// <summary>
    /// Adds or overrides a function in the table.
    /// </summary>
    /// <param name="item">The function to add to the table.</param>
    /// <exception cref="ArgumentNullException">item is null.</exception>
    public abstract void AddOverride(PFunction item);

    /// <summary>
    /// Removes all functions from the table.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Determines whether the table contains a specific function.
    /// </summary>
    /// <param name="item">The function to search the table for.</param>
    /// <returns>True if the table contains the supplied function, false otherwise</returns>
    /// <exception cref="ArgumentNullException">item</exception>
    public abstract bool Contains(PFunction? item);

    /// <summary>
    /// Copies the contents of the function table to the supplied array, starting at the specified index.
    /// </summary>
    /// <param name="array">The array to copy the function to.</param>
    /// <param name="arrayIndex">The index at which to put the first function.</param>
    /// <remarks>The order in which functions are written to the array is not specified.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is negative or not
    /// all functions fit into the remaining portion of the array.</exception>
    public abstract void CopyTo(PFunction[] array, int arrayIndex);

    /// <summary>
    /// Determines the number of functions stored in the function table.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Determines whether the function table is read only.
    /// </summary>
    public abstract bool IsReadOnly { get; }

    /// <summary>
    /// Removes a specific function from the table. Has no effect if a different function occupies the same slot.
    /// </summary>
    /// <param name="item">The function to remove.</param>
    /// <returns>True if the function was removed; false otherwise</returns>
    /// <remarks>This function returning false can mean that a) the table contains no function
    /// in that slot or b) that the function in that slot is different from the supplied function.</remarks>
    /// <exception cref="ArgumentNullException">item</exception>
    public abstract bool Remove(PFunction? item);

    /// <summary>
    /// Removes the function in the specified slot.
    /// </summary>
    /// <param name="id">The id of the function to remove.</param>
    /// <returns>True if a function was removed. False otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
    public abstract bool Remove(string id);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public abstract IEnumerator<PFunction> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
