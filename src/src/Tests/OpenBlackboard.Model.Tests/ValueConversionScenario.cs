using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class ValueConversionScenario
    {
        [Fact]
        public void WithLowLevelConverter_GivenDoubleType_ThenAnyFloatingPointOrIntegerCanBeConvertedToNumber()
        {
            var doubleDescriptor = new ValueDescriptor();

            Assert.Equal(1.0, ValueConversions.ToNumber(doubleDescriptor, CultureInfo.InvariantCulture, 1.0));
            Assert.Equal(1.0, ValueConversions.ToNumber(doubleDescriptor, CultureInfo.InvariantCulture, 1.0f));
            Assert.Equal(1.0, ValueConversions.ToNumber(doubleDescriptor, CultureInfo.InvariantCulture, 1));
        }

        [Fact]
        public void WithLowLevelConverter_GivenDoubleType_ThenValidStringsCanBeConvertedToNumber()
        {
            var doubleDescriptor = new ValueDescriptor();

            Assert.Equal(1.0, ValueConversions.ToNumber(doubleDescriptor, CultureInfo.InvariantCulture, "1.0"));
            Assert.Equal(1.0, ValueConversions.ToNumber(doubleDescriptor, CultureInfo.InvariantCulture, "1e0"));
        }

        [Fact]
        public void WithLowLevelConverter_GivenDoubleType_ImplicitConversionFromBooleanIsNotAllowed()
        {
            Assert.Throws<InvalidCastException>(() => ValueConversions.ToNumber(new ValueDescriptor(), CultureInfo.InvariantCulture, true));
        }

        [Fact]
        public void WithLowLevelConverter_GivenDoubleType_ThenNullIsNull()
        {
            Assert.Equal(null, ValueConversions.ToNumber(new ValueDescriptor(), CultureInfo.InvariantCulture, null));
        }

        [Fact]
        public void WithLowLevelConverter_GivenBooleanType_ThenAnyValueCanBeConvertedToBoolean()
        {
            var boolDescriptor = new ValueDescriptor { Type = TypeOfValue.Boolean };

            Assert.Equal(1.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, 10));
            Assert.Equal(0.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, 0));
            Assert.Equal(0.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, null));
            Assert.Equal(1.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, true));
            Assert.Equal(0.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, false));
            Assert.Equal(1.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, "true"));
            Assert.Equal(1.0, ValueConversions.ToNumber(boolDescriptor, CultureInfo.InvariantCulture, "10"));
        }

        [Fact]
        public void WithLowLevelConverter_GivenStringType_CannotConvertAnyValueToNumber()
        {
            var stringDescriptor = new ValueDescriptor { Type = TypeOfValue.String };

            // Note that if fails even if string is a valid number!
            Assert.Equal(null, ValueConversions.ToNumber(stringDescriptor, CultureInfo.InvariantCulture, "test"));
            Assert.Equal(null, ValueConversions.ToNumber(stringDescriptor, CultureInfo.InvariantCulture, "1.0"));
        }
    }
}
