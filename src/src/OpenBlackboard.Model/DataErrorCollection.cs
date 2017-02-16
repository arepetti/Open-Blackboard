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
