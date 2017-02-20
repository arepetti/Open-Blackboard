using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Infrastructure. This class supports value conversions and should not be used directly.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ValueConversions
    {
        /// <summary>
        /// Infrastructure. Converts a value into the proper type required by a <see cref="ValueDescriptor"/>
        /// and then to its representation as <see cref="double"/> number.
        /// </summary>
        /// <param name="descriptor">Descriptor for which the object to convert represents the value.</param>
        /// <param name="culture">Culture used to perform conversion, if required.</param>
        /// <param name="value">
        /// Value to convert to the exact type required by <paramref name="descriptor"/> and then to
        /// its numeric representation. 
        /// </param>
        /// <returns>
        /// The numeric representation of <paramref name="value"/> after it has been converted to the
        /// type required by <paramref name="descriptor"/>. If type cannot be converted to a number (for example because
        /// required type is a string) then it returns <see langword="null"/>.
        /// </returns>
        /// <exception cref="FormatException">
        /// <paramref name="value"/> cannot be converted to the type required by <paramref name="descriptor"/>
        /// using the specified culture.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> cannot be converted to the type required by <paramref name="descriptor"/>
        /// using the specified culture.
        /// <br/>-or-<br/>
        /// <paramref name="value"/> is a <see cref="bool"/> and required type is a <see cref="double"/>. This
        /// conversion is possible but it's probably unwanted then it's not allowed implictly.
        /// </exception>
        /// <exception cref="OverflowException">
        /// <paramref name="value"/> cannot be converted to the type required by <paramref name="descriptor"/>
        /// because it's too big or to small to be represented as a <see cref="double"/> value.
        /// </exception>
        public static double? ToNumber(ValueDescriptor descriptor, CultureInfo culture, object value)
        {
            Debug.Assert(descriptor != null);
            Debug.Assert(culture != null);

            if (descriptor.Type == TypeOfValue.String)
                return null;

            if (descriptor.Type == TypeOfValue.Boolean)
                return ToBoolean(culture, value) ? 1 : 0;

            if (value is bool)
                throw new InvalidCastException("Required type is System.Double, cannot automatically convert from System.Boolean values.");

            // Null for doubles are simply ignored, it's not safe to assume 0 here
            if (value == null)
                return null;

            return (double)Convert.ChangeType(value, typeof(double), culture);
        }

        private static bool ToBoolean(CultureInfo culture, object value)
        {
            if (value == null)
                return false;

            // This should be the most common code-path
            if (value is bool)
                return (bool)value;

            // Special case for strings, especially useful because of literal values
            if (value is string)
            {
                var valueAsTetxt = Convert.ToString(value, culture);

                if (String.Equals(valueAsTetxt, Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (String.Equals(valueAsTetxt, Boolean.FalseString, StringComparison.OrdinalIgnoreCase))
                    return false;

                // Not a known literal? Go with Double...
            }

            // First conversion is directly to boolean, if it doesn't work for this type then we try also
            // converting to double (it shouldn't happen with primitive types, only with custom types)
            try
            {
                return (bool)Convert.ChangeType(value, typeof(bool), culture);
            }
            catch (FormatException)
            {
                return (double)Convert.ChangeType(value, typeof(double), culture) != 0;
            }
        }
    }
}
