using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace OpenBlackboard.Model
{
    /// <summary>
    /// Creates a new <see cref="DataSet"/> from the aggregation of all the values
    /// in two or more existing datasets.
    /// </summary>
    public sealed class DataSetAggregator
    {
        /// <summary>
        /// Creates a new instance of <see cref="DataSetAggregator"/>.
        /// </summary>
        /// <param name="protocol">Protocol for wich the aggregation is calculated.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="protocol"/> is <see langword="null"/>.
        /// </exception>
        public DataSetAggregator(ProtocolDescriptor protocol)
        {
            if (protocol == null)
                throw new ArgumentNullException(nameof(protocol));

            _protocol = protocol;
            _accumulator = new DataSetValueAccumulator();
            Options = new AggregationOptions();
        }

        /// <summary>
        /// Gets/sets the options to customize how aggregation works
        /// </summary>
        /// <value>
        /// The options to customize how aggregation works.
        /// Default value is <see cref="AggregationOptions.Default"/>.
        /// </value>
        public AggregationOptions Options
        {
            get;
            set;
        }

        /// <summary>
        /// Clears the aggregated values.
        /// </summary>
        /// <remarks>
        /// Usually this function is not needed unless you reuse the same aggregator to create multiple
        /// aggregated datasets.
        /// </remarks>
        public void Clear()
        {
            Debug.Assert(_accumulator != null);

            _accumulator.Clear();
            _culture = null;
        }

        /// <summary>
        /// Accumulate the values of specified datasets.
        /// </summary>
        /// <param name="datasets">List of datasets to accumulate.</param>
        /// <returns>A reference to this object.</returns>
        /// <remarks>
        /// If datasets has not been <em>calculated</em> and validated (with a call to <see cref="DataSet.Calculate"/>)
        /// or they have been modified after last calculation then they're calculated again. This may add issues
        /// to <see cref="DataSet.Issues"/> if their state is not valid. To perform any aggregation all the involved
        /// datasets must belong to the same protocol, must have the same culture and must be valid (warnings are
        /// allowed and they're not copied into generated dataset which, however, may generate its own sets of errors
        /// and warnings. 
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="datasets"/> or any of its elements is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If datasets belongs to different protocols or they do not belong to the protocol
        /// specified in constructor <see cref="DataSetAggregator.DataSetAggregator(ProtocolDescriptor)"/>.
        /// <br/>-or-<br/>
        /// If these datasets (or previously aggregated ones) use different <see cref="DataSet.Culture"/>.
        /// <br/>-or-<br/>
        /// If any dataset has a validation error or a model error.
        /// </exception>
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

        /// <summary>
        /// Creates a new aggregated dataset.
        /// </summary>
        /// <returns>
        /// A new dataset created aggregating values from datasets previously accumualated with
        /// <see cref="Accumulate(DataSet[])"/>. Generated dataset has also its own calculated values and
        /// validation rules.
        /// </returns>
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

        private readonly ProtocolDescriptor _protocol;
        private readonly DataSetValueAccumulator _accumulator;
        private CultureInfo _culture;

        private object Transform(DataSet dataset, ValueDescriptor descriptor, object value)
        {
            if (String.IsNullOrWhiteSpace(descriptor.TransformationForAggregationExpression))
                return value;

            var evaluator = new ExpressionEvaluator(dataset);
            evaluator.AddConstant(ExpressionEvaluator.IdentifierValue, value);
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
                evaluator.AddConstant(ExpressionEvaluator.IdentifierValues, values);

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
