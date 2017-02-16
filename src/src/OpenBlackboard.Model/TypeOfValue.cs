using System;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents the type of content that a <see cref="ValueDescriptor"/> may describe.
    /// </summary>
    public enum TypeOfValue
    {
        /// <summary>
        /// Value is a string.
        /// </summary>
        String,

        /// <summary>
        /// Value is at least a double precision number.
        /// </summary>
        Double,

        /// <summary>
        /// Value is a boolean value.
        /// </summary>
        Boolean,
    }
}
