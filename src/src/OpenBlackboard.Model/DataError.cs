using System;
using System.Diagnostics;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents an issue with the data described by a <see cref="DataSet"/>.
    /// </summary>
    [DebuggerDisplay("{Severity}: {Message}")]
    public sealed class DataError
    {
        internal DataError(IssueSeverity severity, ValueDescriptor item, string message)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(message));

            Severity = severity;
            Item = item;
            Message = message;
        }

        internal DataError(ModelError error)
            : this(error.Severity, error.Item as ValueDescriptor, error.Message)
        {

        }

        /// <summary>
        /// Gets the severity of this issue.
        /// </summary>
        public IssueSeverity Severity { get; }

        /// <summary>
        /// Gets the item that caused this issue.
        /// </summary>
        public ValueDescriptor Item { get; }

        /// <summary>
        /// Gets not localized culture invariant error message for this issue.
        /// </summary>
        public string Message { get; }
    }
}
