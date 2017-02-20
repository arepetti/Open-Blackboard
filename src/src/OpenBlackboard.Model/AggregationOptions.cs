using System;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents the options for aggregation algorithm implemented by <see cref="DataSetAggregator"/>.
    /// </summary>
    [Flags]
    public enum AggregationOptions
    {
        /// <summary>
        /// Default options, all values are aggregated and break in case of errors.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Exclude <see langword="null"/> values when aggregation mode is <see cref="AggregationMode.Count"/>.
        /// </summary>
        ExcludeNullValuesFromCount = 1,

        /// <summary>
        /// Continue aggregation even if there is an error aggregating one value.
        /// </summary>
        IgnoreAggregationErrors = 2,
    }
}
