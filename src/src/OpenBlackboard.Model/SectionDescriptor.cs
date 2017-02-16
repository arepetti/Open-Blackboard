using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a <em>section</em> inside a <see cref="Protocol"/>.
    /// </summary>
    /// <remarks>
    /// A section of a protocol is a top-level grouping for homogeneous values and
    /// it used probably used only for classification purposes. For all the expressions
    /// (for example <see cref="ValueDescriptor.DefaultValueExpression"/>) all values
    /// shall be available.
    /// </remarks>
    public sealed class SectionDescriptor : NamedItemDescriptor
    {
        /// <summary>
        /// Gets the collection of all the values which are direct children of this section.
        /// </summary>
        /// <value>
        /// The collection that contains all the direct children of this section. Each value
        /// may contain other children, use <see cref="SectionDescriptorCollection.VisitAllValues"/>
        /// to enumerate all children from every section.
        /// </value>
        public ValueDescriptorCollection Values { get; } = new ValueDescriptorCollection();

        internal IEnumerable<ValueDescriptor> VisitAllValues()
        {
            Debug.Assert(Values != null);
            Debug.Assert(Values.All(x => x != null));

            return Values.SelectMany(x => x.VisitAllValues());
        }
    }
}
