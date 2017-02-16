using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a collection of <see cref="SectionDescriptor"/>.
    /// </summary>
    public sealed class SectionDescriptorCollection : Collection<SectionDescriptor>
    {
        /// <summary>
        /// Enumerate through all the <see cref="ValueDescriptor"/> of all the
        /// <see cref="SectionDescriptor"/> of this collection.
        /// </summary>
        /// <returns>
        /// All the <c>ValueDescriptor</c> in the hierarchy contained in all the
        /// <c>SectionDescriptor</c> of this collection.
        /// </returns>
        public IEnumerable<ValueDescriptor> VisitAllValues()
        {
            Debug.Assert(Items.All(x => x != null));

            return Items.SelectMany(x => x.VisitAllValues());
        }
    }
}
