using System;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents the preferred aggregation mode to summarize multiple
    /// values into a single measure.
    /// </summary>
    /// <remarks>
    /// Some modes are limited to a specified type of value (for example numbers) but
    /// if specified an aggregation expression then its result value may be used. Note that
    /// returning wrong type will be detected only at run-time.
    /// </remarks>
    public enum AggregationMode
    {
        /// <summary>
        /// This value cannot be aggregated into a single measure (for example a string).
        /// </summary>
        None,

        /// <summary>
        /// To aggregate this value the arithmetic mean should be used. This mode can be be
        /// applied only to <see cref="TypeOfValue.Double"/>.  Missing (<em>null</em>) instances
        /// aren't averaged.
        /// </summary>
        Average,

        /// <summary>
        /// To aggregate this value the sum of each value should be used. This mode cannot be
        /// used for <see cref="TypeOfValue.String"/>. For <see cref="TypeOfValue.Boolean"/>
        /// it counts the number of <em>true</em>. Missing (<em>null</em>) instances aren't summed
        /// for any type.
        /// </summary>
        Sum,

        /// <summary>
        /// Aggregation of this value is the number of non empty instances regardless their value. If you need
        /// to count non empty strings then you should use <see cref="AggregationMode.Sum"/> with a transformation
        /// expression to return 1 when string is not empty.
        /// </summary>
        Count,
    }
}
