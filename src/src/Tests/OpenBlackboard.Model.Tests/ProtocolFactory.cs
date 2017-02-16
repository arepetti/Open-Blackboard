using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    static class ProtocolFactory
    {
        public const string BmiProtocolReference = "BMI";
        public const string BmiProtocolName = "Body Mass Index Protocol (test)";
        public const string PhysicalDataSectionName = "Physical Data";
        public const string WeightFieldId = "Weight";
        public const string WeightFieldName = "Weight (kg)";
        public const string HeightFieldId = "Height";
        public const string HeightFieldName = "Height (cm)";
        public const string GenderFieldId = "Gender";
        public const string GenderFieldName = "Gender at birth";

        public static ProtocolDescriptor CreateTest()
        {
            // This simple protocol consists of one default section
            // with all requried fields. Note that NCalc expression syntax
            // isn't enforced by ProtocolDescriptor itself.
            var section = new SectionDescriptor
            {
                Name = PhysicalDataSectionName
            };

            // ...with a numeric weight field
            section.Values.Add(new ValueDescriptor
            {
                Reference = WeightFieldId,
                Name = WeightFieldName,
            });

            // ...with a numeri height field
            section.Values.Add(new ValueDescriptor
            {
                Reference = HeightFieldId,
                Name = HeightFieldName,
            });

            // ...and a gender at birth list field
            var genderField = new ValueDescriptor
            {
                Reference = GenderFieldId,
                Name = GenderFieldName,
                DefaultValueExpression = "0",
        };

            genderField.AvailableValues.Add(new ListItem { Name = "Male", Value = "0" });
            genderField.AvailableValues.Add(new ListItem { Name = "Female", Value = "1" });
            section.Values.Add(genderField);

            // Just create and return this simple protocol, caller will decorate those fields
            // according to what must be tested.
            var protocol = new ProtocolDescriptor
            {
                Reference = BmiProtocolReference,
                Name = BmiProtocolName
            };

            protocol.Sections.Add(section);

            return protocol;
        }

        public static void CheckConsistency(ProtocolDescriptor protocol)
        {
            Assert.NotNull(protocol);
            Assert.Equal(protocol.Sections.Count, 1);
            Assert.Equal(protocol.Sections.VisitAllValues().Count(), 3);
            Assert.Empty(protocol.ValidateModel());

            Assert.Equal(BmiProtocolReference, protocol.Reference);
            Assert.Equal(BmiProtocolName, protocol.Name);

            Assert.Equal(PhysicalDataSectionName, protocol.Sections.Single().Name);

            Assert.Equal(WeightFieldName, protocol[WeightFieldId].Name);
            Assert.Equal(HeightFieldName, protocol[HeightFieldId].Name);
            Assert.Equal(GenderFieldName, protocol[GenderFieldId].Name);
        }
    }
}
