using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Configuration;
using NWheels.Core.Configuration;
using NWheels.Core.Conventions;

namespace NWheels.Core.UnitTests
{
    [TestFixture]
    public class ConfigurationSectionConventionTests : NUnitEmittedTypesTestBase
    {
        [Test]
        public void CanCreateConfigurationObject()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();

            //-- Act

            var obj = factory.CreateService<ITestSectionOne>();

            //-- Assert

            Assert.That(obj, Is.InstanceOf<ConfigurationSectionBase>());
            Assert.That(obj.IntValue, Is.EqualTo(0));
            Assert.That(obj.StringValue, Is.Null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void SectionConfigPathContainsXmlName()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();

            //-- Act

            var obj = (ConfigurationSectionBase)factory.CreateService<ITestSectionOne>();

            //-- Assert

            Assert.That(obj.GetXmlName(), Is.EqualTo("One"));
            Assert.That(obj.ConfigPath, Is.EqualTo("/One"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanSetDefaultValuesThroughAttributes()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();

            //-- Act

            var obj = factory.CreateService<ITestSectionTwo>();

            //-- Assert

            Assert.That(obj.AnotherInt, Is.EqualTo(123));
            Assert.That(obj.AnotherString, Is.EqualTo("ABC"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ConfigurationSectionFactory CreateFactoryUnderTest()
        {
            return new ConfigurationSectionFactory(base.Module, Auto.Of<IConfigurationLogger>(null));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        [ConfigurationSection(XmlName = "One")]
        public interface ITestSectionOne
        {
            int IntValue { get; }
            string StringValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        [ConfigurationSection(XmlName = "Two")]
        public interface ITestSectionTwo
        {
            [DefaultValue(123)]
            int AnotherInt { get; }
            
            [DefaultValue("ABC")]
            string AnotherString { get; }
            
            [DefaultValue("00:10:30")]
            TimeSpan TimeSpanValue { get; }
            
            [DefaultValue("2014-12-11")]
            DateTime DateTimeValue1 { get; }

            [DefaultValue("2014-12-11 19:00:01")]
            DateTime DateTimeValue2 { get; }

            [DefaultValue(DayOfWeek.Friday)]
            DayOfWeek EnumValue { get; }

            [DefaultValue("06DCC4E7-862A-4C99-A225-0814289B62B9")]
            Guid GuidValue { get; }

            [DefaultValue(typeof(System.IO.MemoryStream))]
            Type TypeValue1 { get; }
            
            [DefaultValue("System.IO.FileStream")]
            Type TypeValue2 { get; }
        }
    }
}
