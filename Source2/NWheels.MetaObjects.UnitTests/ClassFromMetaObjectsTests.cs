using FluentAssertions;
using System;
using Xunit;

namespace NWheels.MetaObjects.UnitTests
{
    public class ClassFromMetaObjectsTests
    {
        [Fact]
        public void TestMC()
        {
            var m = new ClassFromMetaObjects();
            var result = m.MC();
            result.Should().Be("MMMKKKCCC");
        }

        [Fact]
        public void TestMK()
        {
            var m = new ClassFromMetaObjects();
            var result = m.MK();
            result.Should().Be("MMMKKK");
        }
    }
}
