using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class PopulateDataSetScenario
    {
        [Fact]
        public void GivenModelWithErrors_ThenTheyAreDetectedWhenBuildingDataSet()
        {
            var protocol = ProtocolFactory.CreateTest();

            // Duplicated IDs are not allowed...
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "Value" });
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "Value" });

            // We expect the dataset to validate its input model, create an issue and be inusable...
            var dataset = new DataSet(protocol);
            Assert.Equal(1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenDataSetWithMissingData_ThenTheyAreDetectedWhenCalculating()
        {
            var protocol = ProtocolFactory.CreateTest();
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "a" });
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "b", CalculatedValueExpression = "a * 2" });

            var dataset = new DataSet(protocol);
            dataset.Calculate();

            // Value for "a" is required to calculate "b" but it has not been provided
            Assert.Equal(1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenModelWithMultipleErrors_ThenTheyAreAllDetectedWhenCalculating()
        {
            var protocol = ProtocolFactory.CreateTest();

            // Here we're trying to detect an error on EnabledIfExpression. CalculatedValueExpression is invalid but it won't be evaluated
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "Value1", CalculatedValueExpression = "invalid", EnabledIfExpression = "invalid" });

            // This will add an error for an invalid DefaultValueExpression
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "Value2", DefaultValueExpression = "invalid" });

            // This will add an error for an invalid CalculatedValueExpression
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "Value3", CalculatedValueExpression = "invalid", EnabledIfExpression = "true" });

            var dataset = new DataSet(protocol);
            dataset.Calculate();

            // Note that the same expression may cause more than one issue if it is evaluated
            // multiple times (especially EnabledIfExpression because it's used to filter out the list
            // of descriptors). Errors in model are a development thing that won't happen in production
            // then it's easier to leave this as-is than changing design di handle this corner case.
            Assert.True(dataset.Issues.Count > 3);
        }

        [Fact]
        public void GivenInvalidArgumentsForNewValue_ThenExceptionIsThrown()
        {
            var protocol = ProtocolFactory.CreateTest();
            var dataset = new DataSet(protocol);

            Assert.Throws<ArgumentException>(() => dataset.AddValue(new ValueDescriptor { Reference = "a" }, 1));
            Assert.Throws<ArgumentNullException>(() => dataset.AddValue((ValueDescriptor)null, 1));
            Assert.Throws<ArgumentNullException>(() => dataset.AddValue((string)null, 1));
        }

        [Fact]
        public void WhenStoreValidValue_ThenItCanBeRead()
        {
            var protocol = ProtocolFactory.CreateTest();
            var a = new ValueDescriptor { Reference = "a" };
            var b = new ValueDescriptor { Reference = "b" };

            protocol.Sections.First().Values.Add(a);
            protocol.Sections.First().Values.Add(b);

            var dataset = new DataSet(protocol);
            dataset.AddValue(a, 1.0);
            dataset.AddValue("b", 2.0);
            dataset.Calculate();

            Assert.Equal(1.0, Convert.ToDouble(dataset["a"].Value));
            Assert.Equal(2.0, Convert.ToDouble(dataset["b"].Value));
        }

        [Fact]
        public void WhenValuesHaveCalculatedOrLiteralDefaults_ThenTheyAreCalculatedAndCanBeRead()
        {
            var protocol = ProtocolFactory.CreateTest();
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "Literal", DefaultValueExpression = "2" });
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = "LiteralExpression", DefaultValueExpression = "2 + 2" });

            var dataset = new DataSet(protocol);
            dataset.Calculate();

            Assert.Equal(2.0, Convert.ToDouble(dataset["Literal"].Value));
            Assert.Equal(4.0, Convert.ToDouble(dataset["LiteralExpression"].Value));
        }

        [Fact]
        public void WhenCalculatedValueDependsOnValuesWithDefault_ThenItIsCalculatedCorrectly()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", DefaultValueExpression = "2" });
            section.Values.Add(new ValueDescriptor { Reference = "b", DefaultValueExpression = "2 + 2" });
            section.Values.Add(new ValueDescriptor { Reference = "c", CalculatedValueExpression = "a + b" });

            var dataset = new DataSet(protocol);
            dataset.Calculate();

            Assert.Equal(2.0, Convert.ToDouble(dataset["a"].Value));
            Assert.Equal(4.0, Convert.ToDouble(dataset["b"].Value));
            Assert.Equal(6.0, Convert.ToDouble(dataset["c"].Value));
        }

        [Fact]
        public void WhenCalculatedValueDependsOnValuesWithDefaultAndExternalValues_ThenItIsCalculatedCorrectly()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a" });
            section.Values.Add(new ValueDescriptor { Reference = "b", DefaultValueExpression = "2 + 2" });
            section.Values.Add(new ValueDescriptor { Reference = "c", CalculatedValueExpression = "a + b" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 2.0);
            dataset.Calculate();

            Assert.Equal(2.0, Convert.ToDouble(dataset["a"].Value));
            Assert.Equal(4.0, Convert.ToDouble(dataset["b"].Value));
            Assert.Equal(6.0, Convert.ToDouble(dataset["c"].Value));
        }

        [Fact]
        public void GivenExternalValue_WhenAlert_ThenAddWarning()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", WarningIfExpression = "this > 5" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 6);
            dataset.Calculate();

            Assert.Equal(1, dataset.Issues.Count(x => x.Item.Reference == "a" && x.Severity == IssueSeverity.Warning));
            Assert.Equal(1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenExternalValue_WhenInvalid_ThenAddError()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", ValidIfExpression = "this > 5" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 1);
            dataset.Calculate();

            Assert.Equal(1, dataset.Issues.Count(x => x.Item.Reference == "a" && x.Severity == IssueSeverity.ValidationError));
            Assert.Equal(1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenCalculatedValueWithValidation_ThenValidationIsApplied()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", ValidIfExpression = "this < 5" });
            section.Values.Add(new ValueDescriptor { Reference = "b", CalculatedValueExpression = "a * 2", ValidIfExpression = "this < 6" });

            // 4 is valid for a but it will fail validation for b. Another test will ensure that validation for
            // b is not even evaluated if validation for a isn't passed.
            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 4);
            dataset.Calculate();

            Assert.Equal(1, dataset.Issues.Count(x => x.Item.Reference == "b" && x.Severity == IssueSeverity.ValidationError));
            Assert.Equal(1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenInvalidExternalValuesAndDependantCalculatedValue_ThenDependantValueIsNotCalculated()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", ValidIfExpression = "this < 5" });
            section.Values.Add(new ValueDescriptor { Reference = "b", CalculatedValueExpression = "a * 2", ValidIfExpression = "this < 10" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 5);
            dataset.Calculate();

            Assert.Equal(1, dataset.Issues.Count(x => x.Item.Reference == "a" && x.Severity == IssueSeverity.ValidationError));
            Assert.Equal(1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenValue_WhenAlertedAndInvalid_ThenThereAreIssuesForBoth()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", WarningIfExpression = "this > 3", ValidIfExpression = "this < 5" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 6);
            dataset.Calculate();

            Assert.Equal(1, dataset.Issues.Count(x => x.Severity == IssueSeverity.ValidationError));
            Assert.Equal(1, dataset.Issues.Count(x => x.Severity == IssueSeverity.Warning));
            Assert.Equal(2, dataset.Issues.Count);
        }

        [Theory]
        [InlineData(5, 0), InlineData(5, 6), InlineData(1, -12), InlineData(-1, -2)]
        public void GivenValidationWithDependencies_ThenItIsCalculatedCorrectly(int a, int b)
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a" });
            section.Values.Add(new ValueDescriptor { Reference = "b", ValidIfExpression = "this < a" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", a);
            dataset.AddValue("b", b);
            dataset.Calculate();

            Assert.Equal(b < a ? 0 : 1, dataset.Issues.Count);
        }

        [Fact]
        public void GivenExpressionWithNullCoalescingForUnknownValue_ThenItIsNullInsteadOfThrowing()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", ValidIfExpression = "isnull([invalid?])" });

            var dataset = new DataSet(protocol);
            dataset.Calculate();

            Assert.Equal(0, dataset.Issues.Count);
        }

        [Fact]
        public void GivenExpressionWithThisKeyword_ThenItIsSynonimOfCurrentValueId()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", ValidIfExpression = "this == 1 and this == a" });
            section.Values.Add(new ValueDescriptor { Reference = "b", ValidIfExpression = "this == 2 and this == b" });

            var dataset = new DataSet(protocol);
            dataset.AddValue("a", 1);
            dataset.AddValue("b", 2);
            dataset.Calculate();

            Assert.Equal(0, dataset.Issues.Count);
        }

        [Fact]
        public void GivenExpressionWithRequired_ThenItIsTrueIfValueExists()
        {
            var protocol = ProtocolFactory.CreateTest();
            var section = protocol.Sections.First();
            section.Values.Add(new ValueDescriptor { Reference = "a", ValidIfExpression = "required" });

            var dataset = new DataSet(protocol);
            dataset.Calculate();
            Assert.Equal(1, dataset.Issues.Count);

            dataset = new DataSet(protocol);
            dataset.AddValue("a", 1);
            dataset.Calculate();
            Assert.Equal(0, dataset.Issues.Count);
        }
    }
}
