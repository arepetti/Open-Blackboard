using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class ValueAggregationScenario
    {
        [Fact]
        public void WithCountAsAggregation_GivenDoubleValues()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor { Reference = "a", PreferredAggregation = AggregationMode.Count });

            var dataset1 = new DataSet(protocol);
            dataset1.AddValue("a", 10);

            var dataset2 = new DataSet(protocol);
            dataset2.AddValue("a", 5);

            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Accumulate(dataset1, dataset2).Calculate();

            Assert.Equal(2, result["a"].Value);
        }

        [Fact]
        public void WithCountAsAggregation_GivenOneNullValues_ThenItCanBeExcludedFromCount()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor { Reference = "a", PreferredAggregation = AggregationMode.Count });

            var dataset1 = new DataSet(protocol);
            dataset1.AddValue("a", 10);

            var dataset2 = new DataSet(protocol);
            dataset2.AddValue("a", null);

            // Null value is included in count
            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Accumulate(dataset1, dataset2).Calculate();
            Assert.Equal(2, result["a"].Value);

            // Null value is excluded from count
            aggregator.Clear();
            aggregator.Options = AggregationOptions.Default | AggregationOptions.ExcludeNullValuesFromCount;
            result = aggregator.Accumulate(dataset1, dataset2).Calculate();
            Assert.Equal(1, result["a"].Value);
        }

        [Fact]
        public void WithSumAsAggregation_GivenTwoDoubleValues()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor { Reference = "a", PreferredAggregation = AggregationMode.Sum });

            var dataset1 = new DataSet(protocol);
            dataset1.AddValue("a", 10);

            var dataset2 = new DataSet(protocol);
            dataset2.AddValue("a", 5);

            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Accumulate(dataset1, dataset2).Calculate();
            Assert.Equal(15.0, result["a"].Value);
        }

        [Fact]
        public void WithSumAsAggregation_ThenNullValuesEqualToZero()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor { Reference = "a", PreferredAggregation = AggregationMode.Sum });

            var dataset1 = new DataSet(protocol);
            dataset1.AddValue("a", 10);

            var dataset2 = new DataSet(protocol);
            dataset2.AddValue("a", null);

            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Accumulate(dataset1, dataset2).Calculate();
            Assert.Equal(10.0, result["a"].Value);
        }

        [Fact]
        public void WithAveragesAggregation_ThenNullsAreIgnored()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor { Reference = "a", PreferredAggregation = AggregationMode.Average });

            var dataset1 = new DataSet(protocol);
            dataset1.AddValue("a", 10);

            var dataset2 = new DataSet(protocol);
            dataset2.AddValue("a", 20);

            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Accumulate(dataset1, dataset2).Calculate();
            Assert.Equal(15.0, result["a"].Value);
        }

        [Fact]
        public void WithAveragesAggregation_ThenEmptyDatasetsAreZero()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor { Reference = "a", PreferredAggregation = AggregationMode.Average });

            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Calculate();
            Assert.Equal(null, result["a"].Value);
        }

        [Fact]
        public void WithNoAggregation_ThenCustomAggregationFunctionCanBeUsed()
        {
            var protocol = new ProtocolDescriptor { Reference = "Aggregation test" };
            var section = protocol.Sections.Add("Test");
            section.Values.Add(new ValueDescriptor
            {
                Reference = "a",
                PreferredAggregation = AggregationMode.None,
                AggregationExpression = @"sequence
                                          (
                                            let('mean', average(values)),
                                            let('n', count(values) - 1),
                                            sqrt(sum(values, pow(value - mean, 2)) / n)
                                          )"
            });

            var dataset1 = new DataSet(protocol);
            dataset1.AddValue("a", 10);

            var dataset2 = new DataSet(protocol);
            dataset2.AddValue("a", 20);

            var aggregator = new DataSetAggregator(protocol);
            var result = aggregator.Accumulate(dataset1, dataset2).Calculate();
            if (result.Issues.HasErrors)
                throw new InvalidOperationException(String.Join("\n", result.Issues.Select(x => x.Message)));

            Assert.Equal(7.07, (double)result["a"].Value, 2);
        }
    }
}
