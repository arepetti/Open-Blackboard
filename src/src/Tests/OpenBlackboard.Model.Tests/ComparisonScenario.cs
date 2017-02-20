using System;
using Xunit;

namespace OpenBlackboard.Model.Tests
{
    public class ComparisonScenario
    {
        [Fact]
        public void GivenTwoValueDescriptorWithSameId_ThenTheyHaveSameHashCode()
        {
            Assert.Equal(new ValueDescriptor { Reference = "a" }.GetHashCode(), new ValueDescriptor { Reference = "a" }.GetHashCode());
        }

        [Fact]
        public void GivenTwoValueDescriptorWithSameId_ThenTheyAlwaysCompareEqual()
        {
            Assert.Equal(new ValueDescriptor { Reference = "a" }, new ValueDescriptor { Reference = "a" });
        }

        [Fact]
        public void GivenTwoValueDescriptorWithDifferentId_ThenTheyAreNotEqual()
        {
            Assert.NotEqual(new ValueDescriptor { Reference = "a" }, new ValueDescriptor { Reference = "b" });
        }

        [Fact]
        public void GivenValueDescriptor_ThenItCanBeComparedWihtOthers()
        {
            var value1 = new ValueDescriptor { Reference = "a" };
            var value2 = new ValueDescriptor { Reference = "a" };
            var value3 = new ValueDescriptor { Reference = "a" };

            Assert.Equal(value1, value2);
            Assert.Equal(value2, value3);
            Assert.Equal(value3, value1);

            Assert.True(value1.Equals(value1));
            Assert.True(value1.Equals(value2));
            Assert.True(value1.Equals((object)value2));
            Assert.False(value1.Equals(null));
            Assert.False(value1.Equals(new object()));

            // ValueDescriptor is a mutable object, I prefer to do not
            // override == and != ooperators and keep original behavior (reference comparison)
            Assert.True(value1 != value2);
            Assert.False(value1 == null);
        }
    }
}
