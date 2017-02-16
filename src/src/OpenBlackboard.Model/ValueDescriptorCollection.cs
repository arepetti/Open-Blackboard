using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents a collection of <see cref="ValueDescriptor"/>.
    /// </summary>
    public sealed class ValueDescriptorCollection : Collection<ValueDescriptor>
    {
        internal IEnumerable<ValueDescriptor> VisitAllValues()
        {
            Debug.Assert(Items.All(x => x != null));

            return Items.SelectMany(x => x.VisitAllValues());
        }
    }
}
