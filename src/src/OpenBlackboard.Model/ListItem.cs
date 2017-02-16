using System;
using System.Diagnostics;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a single possible value for a <see cref="ValueDescriptor"/>.
    /// </summary>
    [DebuggerDisplay("{Name} ({Value})")]
    public sealed class ListItem
    {
        /// <summary>
        /// Gets/sets the display name of this list item.
        /// </summary>
        /// <value>
        /// The display name of this item which is what end-user should see
        /// when selecting/reviewing the value. Default value is <see cref="String.Empty"/>.
        /// If this property is a null or empty string then end-user should see the value
        /// of <see cref="Value"/>.
        /// </value>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets the value associated with this list item.
        /// </summary>
        /// <value>
        /// The value associated with this list item. It is the final value assigned
        /// to the <see cref="OpenBlackboard.Model.DataSetValue"/> described by <see cref="ValueDescriptor"/> with a non-empty
        /// list of available values <see cref="ValueDescriptor.AvailableValues"/>.
        /// If the type of value is anything else than a string then it must be specified
        /// using invariant culture shall be parsed before assigning the content to
        /// <see cref="OpenBlackboard.Model.DataSetValue"/>.
        /// </value>
        public string Value { get; set; } = "";
    }
}
