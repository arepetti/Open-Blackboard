using System;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Provides a mechanism to reference an item using an unique ID.
    /// </summary>
    public interface IReferenceable
    {
        /// <summary>
        /// Gets the unique ID associated with this item.
        /// </summary>
        /// <value>
        /// The unique ID associated with this item. There is not a mandatory syntax or convention
        /// for these IDs (they might even be a GUID), each <see cref="Protocol"/> may define its own convention
        /// or mapping. Comparison of this value must be ordinal and case insensitive.
        /// </value>
        string Reference { get; }
    }
}
