using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Configuration;
using NWheels.Testing;
using System.Xml.Linq;
using NWheels.Configuration.Core;

namespace NWheels.Core.UnitTests.Configuration
{
    [TestFixture]
    public class XmlConfigurationLoaderTest : NUnitEmittedTypesTestBase
    {
        [TestCase("dev", "dev", true)]
        [TestCase("DeV", "dEv", true)]
        [TestCase("Dev", "UAT", false)]
        [TestCase("Dev", "DEV, UAT", true)]
        [TestCase("Dev", "UAT, DEV", true)]
        [TestCase("Dev", "PROD, UAT, DEV", true)]
        [TestCase("Dev", "PROD, UAT", false)]
        [TestCase("dev", "!dev", false)]
        [TestCase("Dev", "!UAT", true)]
        [TestCase("Dev", "!DEV, UAT", false)]
        [TestCase("Dev", "!UAT, DEV", false)]
        [TestCase("Dev", "!PROD, UAT, DEV", false)]
        [TestCase("Dev", "!PROD, UAT", true)]
        [TestCase("dev", null, true)]
        [TestCase("dev", "", true)]
        public void TestMatchScopeExpression(string value, string expression, bool shouldMatch)
        {
            var actualMatch = XmlConfigurationLoader.Match(value, expression);
            Assert.AreEqual(shouldMatch, actualMatch, "ShouldMatch");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("dev", "backend", "1-DEV", "2-BACKEND", "3-BACKEND", "4-DEV")]
        [TestCase("dev", "webApp", "1-DEV", "2-NOT-BACKEND", "3-NOT-BACKEND", "4-DEV")]
        [TestCase("prod", "backend", "1-PROD", "2-BACKEND", "3-BACKEND", "4-PROD")]
        [TestCase("prod", "webApp", "1-PROD", "2-NOT-BACKEND", "3-NOT-BACKEND", "4-PROD")]
        [TestCase("uat", "webApp", "1-DEFAULT", "2-NOT-BACKEND", "3-NOT-BACKEND", "4-DEFAULT")]
        public void TestApplyMatchingIfScopes(
            string environmentType, string nodeName, string expectedFirst, string expectedSecond, string expectedThird, string expectedFourth)
        {
            //-- Arrange

            var framework = new TestFramework(base.Module);
            framework.NodeConfiguration.EnvironmentType = environmentType;
            framework.NodeConfiguration.NodeName = nodeName;

            var one = framework.ConfigSection<ISectionOne>();
            var two = framework.ConfigSection<ISectionTwo>();

            var xml =
                @"<CONFIGURATION>
                    <IF environment-type='dev' comment='only for dev environments'>
                        <One first='1-DEV' />
                        <Two fourth='4-DEV' />
                    </IF>
                    <IF environment-type='prod' comment='only for prod environments'>
                        <One first='1-PROD' />
                        <Two fourth='4-PROD' />
                    </IF>
                    <IF node='BACKEND' comment='only for backend node instances'>
                        <One second='2-BACKEND' />
                        <Two third='3-BACKEND' />
                    </IF>
                    <IF node='!BACKEND' comment='for all but backend node instances'>
                        <One second='2-NOT-BACKEND' />
                        <Two third='3-NOT-BACKEND' />
                    </IF>
                </CONFIGURATION>";

            var loader = new XmlConfigurationLoader(
                framework,
                framework.LoggerAuto<IConfigurationLogger>(),
                new IConfigurationSection[] { one, two });

            //-- Act

            loader.LoadConfigurationDocument(XDocument.Parse(xml));

            //-- Assert

            Assert.That(one.First, Is.EqualTo(expectedFirst));
            Assert.That(one.Second, Is.EqualTo(expectedSecond));
            Assert.That(two.Third, Is.EqualTo(expectedThird));
            Assert.That(two.Fourth, Is.EqualTo(expectedFourth));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "One")]
        public interface ISectionOne : IConfigurationSection
        {
            [DefaultValue("1-DEFAULT")]
            string First { get; }
            [DefaultValue("2-DEFAULT")]
            string Second { get; }
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "Two")]
        public interface ISectionTwo : IConfigurationSection
        {
            [DefaultValue("3-DEFAULT")]
            string Third { get; }
            [DefaultValue("4-DEFAULT")]
            string Fourth { get; }
        }
    }
}
