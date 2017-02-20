using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class ModelValidationScenario
    {
        [Fact]
        public void GivenValidationMessageWithoutExpression_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            var value = protocol[ProtocolFactory.WeightFieldId];

            value.WarningMessage = "test"; // This will result in a warning because of missing WarningIfExpression
            value.ValidationMessage = "test"; // This will result in a warning because of missing ValidIfExpression


            Assert.Equal(2, protocol.ValidateModel().Count(x => x.Severity == IssueSeverity.Warning));
        }

        [Fact]
        public void GivenPreferredAggregationWithAggregationExpression_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            var value = protocol[ProtocolFactory.WeightFieldId];

            value.PreferredAggregation = AggregationMode.Sum;
            value.AggregationExpression = "sum(values)";


            Assert.Equal(1, protocol.ValidateModel().Count(x => x.Severity == IssueSeverity.Warning));
        }

        [Fact]
        public void GivenStringFieldAggregatedWithAverage_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            var value = new ValueDescriptor { Reference = "Name", Name = "Name" };
            value.Type = TypeOfValue.String;
            value.PreferredAggregation = AggregationMode.Average;
            protocol.Sections.First().Values.Add(value);

            // Strings cannot be averaged
            Assert.Equal(1, protocol.ValidateModel().Count());

            // ...unless we supply a custom transformation
            value.TransformationForAggregationExpression = "length(this)"; // Code is not really parsed during model validation...
            Assert.Equal(0, protocol.ValidateModel().Count());
        }

        [Fact]
        public void GivenListOfAvailableValuesForCalculatedField_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            // Calculated values cannot have a list of available values
            var value = new ValueDescriptor { Reference = "Test", Name = "Test" };
            value.CalculatedValueExpression = "[Weight] / [Height]";
            value.AvailableValues.Add(new ListItem { Name = "1", Value = "1" });
            protocol.Sections.First().Values.Add(value);

            Assert.Equal(1, protocol.ValidateModel().Count());
        }

        [Fact]
        public void GivenCalculatedValueWithDefault_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            // Calculated values cannot have a default value
            var value = new ValueDescriptor { Reference = "Test", Name = "Test" };
            value.CalculatedValueExpression = "[Weight] / [Height]";
            value.DefaultValueExpression = "10";
            protocol.Sections.First().Values.Add(value);

            Assert.Equal(1, protocol.ValidateModel().Count());
        }

        [Fact]
        public void GivenEditableValueWithoutReferenceId_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            // Calculated values cannot have a default value
            var value = new ValueDescriptor { Reference = "", Name = "Test" };
            protocol.Sections.First().Values.Add(value);

            Assert.Equal(1, protocol.ValidateModel().Count());
        }

        [Fact]
        public void GivenValuesWithDuplicatedReferenceId_ThenAddIssue()
        {
            var protocol = ProtocolFactory.CreateTest();
            Assert.Empty(protocol.ValidateModel());

            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = ProtocolFactory.WeightFieldId, Name = "Test 1" });
            protocol.Sections.First().Values.Add(new ValueDescriptor { Reference = ProtocolFactory.HeightFieldId, Name = "Test 2" });

            Assert.Equal(2, protocol.ValidateModel().Count());
        }
    }
}
