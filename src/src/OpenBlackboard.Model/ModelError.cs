using System;
using System.Diagnostics;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents an issue with the model described by a <see cref="Protocol"/>.
    /// </summary>
    [DebuggerDisplay("{Severity}: {Message}")]
    public sealed class ModelError
    {
        internal ModelError(IssueSeverity severity, object item, string message)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(message));

            Severity = severity;
            Item = item;
            Message = message;
        }

        /// <summary>
        /// Gets the severity of this issue.
        /// </summary>
        public IssueSeverity Severity { get; }

        /// <summary>
        /// Gets the item that caused this issue.
        /// </summary>
        public object Item { get; }

        /// <summary>
        /// Gets not localized culture invariant error message for this issue.
        /// </summary>
        public string Message { get; }
    }
}
