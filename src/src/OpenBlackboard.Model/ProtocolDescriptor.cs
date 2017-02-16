using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a protocol (i.e. a set of acquired values).
    /// </summary>
    [DebuggerDisplay("{Reference}")]
    public sealed class ProtocolDescriptor : NamedItemDescriptor, IReferenceable
    {
        /// <summary>
        /// Gets/sets the unique ID of this protocol.
        /// </summary>
        /// <value>
        /// The ID of this protocol that uniquely identifies the protocol from any other.
        /// </value>
        public string Reference { get; set; }

        /// <summary>
        /// Gets the <see cref="ValueDescriptor"/> with the specified reference ID.
        /// </summary>
        /// <param name="reference">ID to search within all the values of this protocol.</param>
        /// <returns>
        /// The <c>ValueDescriptor</c> with the specified ID.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If there is not a <c>ValueDescriptor</c> with the specified ID.
        /// <br/>-or-<br/>
        /// In debug builds if there is more than one <c>ValueDescriptor</c> with specified ID.
        /// </exception>
        public ValueDescriptor this[string reference]
        {
            get
            {
#if DEBUG
                return Sections.VisitAllValues().Single(x => String.Equals(x.Reference, reference, StringComparison.OrdinalIgnoreCase));
#else
                return Sections.VisitAllValues().First(x => String.Equals(x.Reference, reference, StringComparison.OrdinalIgnoreCase));
#endif
            }
        }

        /// <summary>
        /// Gets the list of <see cref="SectionDescriptor"/> of this protocol.
        /// </summary>
        /// <value>
        /// The sections (high level logical groups) into which this protocol is organized.
        /// </value>
        public SectionDescriptorCollection Sections { get; } = new SectionDescriptorCollection();

        /// <summary>
        /// Validate the consistency of this model.
        /// </summary>
        /// <returns>
        /// A list of validation errors of this model.
        /// </returns>
        /// <remarks>
        /// Errors are in the model itself, not in the value that will be submitted. Note that not all errors
        /// may be detected, it's still possible that some errors will cause unpredicted behaviors or run-time errors.
        /// </remarks>
        public IEnumerable<ModelError> ValidateModel()
        {
            Debug.Assert(Sections != null);

            var allValues = Sections.VisitAllValues().ToArray();
            var issuesInValues = allValues.SelectMany(x => x.ValidateModel());

            var valuesWithDuplicatedId = allValues.GroupBy(x => x.Reference, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() > 1);

            if (!valuesWithDuplicatedId.Any())
                return issuesInValues;

            return valuesWithDuplicatedId
                .Select(x => new ModelError(IssueSeverity.ModelError, x.First(), $"Value '{x.Key}': multiple values with same reference ID."))
                .Concat(issuesInValues);
        }
    }
}
