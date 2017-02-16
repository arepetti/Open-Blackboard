using System;
using System.ComponentModel;
using System.Diagnostics;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents any item which has a user-friendly name and a description.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public abstract class NamedItemDescriptor
    {
        /// <summary>
        /// Gets/sets the user-friendly name of this item.
        /// </summary>
        /// <value>
        /// The user-friendly name of this item. Text may be localized (if required)
        /// and it is used only to present this item to end-users. This value may be
        /// an empty string if item won't ever be presented to end-user. Default
        /// value is <see cref="String.Empty"/>.
        /// </value>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets/sets the user-friendly short name of this item.
        /// </summary>
        /// <value>
        /// The user-friendly short name of this item. Text may be localized (if required)
        /// and it is used only to present this item to end-users when there are space restrictions
        /// and a name shorter than <see cref="Name"/> is preferable. This value may be
        /// an empty string, in that case value from <see cref="Name"/> should be used. Default
        /// value is <see cref="String.Empty"/>.
        /// </value>
        [DefaultValue("")]
        public string ShortName { get; set; } = "";

        /// <summary>
        /// Gets/sets a description of this item.
        /// </summary>
        /// <value>
        /// User-friendly description of this item. Text may be localized (if required)
        /// and, if available, it is used only to describe the nature of this item to end-users.
        /// Default value is <see cref="String.Empty"/>.
        /// </value>
        [DefaultValue("")]
        public string Description { get; set; } = "";
    }
}
