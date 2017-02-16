using System;
using System.Diagnostics;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Holds a submitted value with the reference to its <see cref="ValueDescriptor"/>.
    /// </summary>
    public sealed class DataSetValue
    {
        internal DataSetValue(ValueDescriptor descriptor, object value)
        {
            Debug.Assert(descriptor != null);

            Descriptor = descriptor;
            Value = value;
        }

        /// <summary>
        /// Gets/sets the descriptor of this value.
        /// </summary>
        /// <value>
        /// The descriptor of the field for which this object contains the effective value.
        /// </value>
        public ValueDescriptor Descriptor { get; }

        /// <summary>
        /// Gets/sets the value of this field.
        /// </summary>
        /// <value>
        /// This value may be in any format (a primitive data type that matches underlying required type or
        /// its textual representation). If a conversion is required then it is performed according to
        /// <see cref="DataSet.Culture"/>.
        /// </value>
        public object Value { get; set; }
    }
}
