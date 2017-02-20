using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a collection of <see cref="DataError"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public sealed class DataErrorCollection : Collection<DataError>
    {
        /// <summary>
        /// Gets all the errors of this collection.
        /// </summary>
        /// <value>
        /// All the errors (severity <see cref="IssueSeverity.ModelError"/> or <see cref="IssueSeverity.ValidationError"/>)
        /// of this collection.
        /// </value>
        public IEnumerable<DataError> Errors => Items.Where(x => x.Severity != IssueSeverity.Warning);

        /// <summary>
        /// Gets all the warnings of this collection.
        /// </summary>
        /// <value>
        /// All the warnings (severity <see cref="IssueSeverity.Warning"/>) of this collection.
        /// </value>
        public IEnumerable<DataError> Warnings => Items.Where(x => x.Severity == IssueSeverity.Warning);

        /// <summary>
        /// Indicates whether this collection contains any error.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if any issue in this collection is a <see cref="IssueSeverity.ModelError"/> or
        /// <see cref="IssueSeverity.ValidationError"/>; <see langword="false"/> if collection is empty or it contains
        /// only <see cref="IssueSeverity.Warning"/>.
        /// </value>
        public bool HasErrors => Errors.Any();

        internal void AddWarning(ValueDescriptor item, string message)
        {
            Add(new DataError(IssueSeverity.Warning, item, message));
        }

        internal void AddModelError(ValueDescriptor item, string message)
        {
            Add(new DataError(IssueSeverity.ModelError, item, message));
        }

        internal void AddModelErrors(IEnumerable<ModelError> modelErrors)
        {
            foreach (var modelError in modelErrors)
                Add(new DataError(modelError));
        }

        internal void AddValidationError(ValueDescriptor item, string message)
        {
            Add(new DataError(IssueSeverity.ValidationError, item, message));
        }

        private string DebuggerDisplay
        {
            get
            {
                int warnings = Items.Count(x => x.Severity == IssueSeverity.Warning);
                return $"Errors: {Count - warnings}, warnings: {warnings}";
            }
                    
        }
    }
}
