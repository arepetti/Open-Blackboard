using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Represents the options for aggregation algorithm implemented by <see cref="DataSetAggregator"/>.
    /// </summary>
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

    public sealed class DataSetAggregator
    {
        public DataSetAggregator(ProtocolDescriptor protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            _protocol = protocol;
            _accumulator = new DataSetValueAccumulator();
            Options = new AggregationOptions();
        }

        public AggregationOptions Options
        {
            get;
            set;
        }

        public void Clear()
        {
            Debug.Assert(_accumulator != null);

            _accumulator.Clear();
        }

        public DataSetAggregator Accumulate(params DataSet[] datasets)
        {
            Debug.Assert(_accumulator != null);

            if (datasets == null || datasets.Any(x => x == null))
                throw new ArgumentNullException(nameof(datasets), "Cannot pass a null array or any null argument.");

            if (datasets.Any(x => !String.Equals(x.Protocol.Reference, _protocol.Reference, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("All accumulated values must belong to the same protocol.");

            foreach (var dataset in datasets)
            {
                if (_culture == null)
                    _culture = dataset.Culture;
                else if (_culture != dataset.Culture)
                    throw new ArgumentException("All accumulated values must use the same culture for conversions.");

                if (dataset.IsChangedAfterLastCalculation)
                    dataset.Calculate();

                if (dataset.Issues.HasErrors)
                    throw new ArgumentException("You cannot aggregate a dataset with model or validation issues.");

                _accumulator.AddRange(dataset.Values.Values);
            }

            return this;
        }

        public DataSet Calculate()
        {
            Debug.Assert(_protocol != null);
            Debug.Assert(_accumulator != null);

            var result = new DataSet(_protocol);

            foreach (var descriptor in _protocol.Sections.VisitAllValues())
            {
                var values = _accumulator[descriptor]
                    .Select(x => Transform(result, descriptor, x))
                    .ToArray();

                if (!Options.HasFlag(AggregationOptions.IgnoreAggregationErrors) && result.Issues.HasErrors)
                    return result;

                result.AddValue(descriptor, Aggregate(result, descriptor, Filter(descriptor, values)));
            }

            result.Calculate();

            return result;
        }

        private const string IdentifierCurrentValue = "value";
        private const string IdentifierAllValues = "values";

        private readonly ProtocolDescriptor _protocol;
        private readonly DataSetValueAccumulator _accumulator;
        private CultureInfo _culture;

        private object Transform(DataSet dataset, ValueDescriptor descriptor, object value)
        {
            if (String.IsNullOrWhiteSpace(descriptor.TransformationForAggregationExpression))
                return value;

            var evaluator = new ExpressionEvaluator(dataset);
            evaluator.AddConstant(IdentifierCurrentValue, value);
            evaluator.AddConstant(descriptor.Reference, value);

            // If there is an error in this expression then we accumulate a null value instead of untransformed
            // one because there are less chances it will cause more errors later. If option AggregationOptions.IgnoreAggregationErrors
            // is specified then Issues collection of the result dataset will contain this error for each accumulated value.
            return evaluator.Evaluate(() => descriptor.TransformationForAggregationExpression);
        }

        private IEnumerable<object> Filter(ValueDescriptor descriptor, IEnumerable<object> values)
        {
            if (descriptor.PreferredAggregation == AggregationMode.Count && Options.HasFlag(AggregationOptions.ExcludeNullValuesFromCount))
                return values.Where(x => x != null);

            return values;
        }

        private object Aggregate(DataSet dataset, ValueDescriptor descriptor, IEnumerable<object> values)
        {
            if (!String.IsNullOrWhiteSpace(descriptor.AggregationExpression))
            {
                var evaluator = new ExpressionEvaluator(dataset);
                evaluator.AddConstant(IdentifierAllValues, values);

                return evaluator.Evaluate(() => descriptor.AggregationExpression);
            }

            // TODO: AggregationMode.None may be checked at the very beginning, no need to create a
            // list of values if there is nothing to do with them...
            if (descriptor.PreferredAggregation == AggregationMode.None)
                return null;

            if (descriptor.PreferredAggregation == AggregationMode.Count)
                return values.Count();

            var numbers = values
                    .Select(x => ValueConversions.ToNumber(descriptor, _culture, x))
                    .Where(x => x.HasValue);

            // Sum of an empty set is zero but average is null
            if (descriptor.PreferredAggregation == AggregationMode.Sum)
                return numbers.Sum();

            if (descriptor.PreferredAggregation == AggregationMode.Average)
                return numbers.Any() ? (object)(numbers.Sum() / numbers.Count()) : null;

            throw new NotSupportedException();
        }
    }
}
