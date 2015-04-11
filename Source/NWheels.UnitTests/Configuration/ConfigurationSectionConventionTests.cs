using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Hapil.Testing.NUnit;
using NUnit.Framework;
using NWheels.Configuration;
using NWheels.Configuration.Core;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Testing;

namespace NWheels.UnitTests.Configuration
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

            Assert.That(obj, Is.InstanceOf<IConfigurationSection>());
            Assert.That(obj, Is.InstanceOf<IConfigurationObject>());
            Assert.That(obj, Is.InstanceOf<IInternalConfigurationObject>());

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

            var obj = factory.CreateService<ITestSectionOne>().AsConfigurationObject();

            //-- Assert

            Assert.That(obj.GetXmlName(), Is.EqualTo("One"));
            Assert.That(obj.GetConfigPath(), Is.EqualTo("/One"));
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
            Assert.That(obj.TimeSpanValue, Is.EqualTo(TimeSpan.FromMinutes(10).Add(TimeSpan.FromSeconds(30))));
            Assert.That(obj.DateTimeValue1, Is.EqualTo(new DateTime(2014, 12, 11)));
            Assert.That(obj.DateTimeValue2, Is.EqualTo(new DateTime(2014, 12, 11, 19, 00, 01)));
            Assert.That(obj.EnumValue, Is.EqualTo(DayOfWeek.Friday));
            Assert.That(obj.GuidValue, Is.EqualTo(new Guid("06DCC4E7-862A-4C99-A225-0814289B62B9")));
            Assert.That(obj.TypeValue1, Is.EqualTo(typeof(MemoryStream)));
            Assert.That(obj.TypeValue2, Is.EqualTo(typeof(FileStream)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLoadValuesFromXml()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var xml = XElement.Parse("<One intValue='987' stringValue='XYZ' />");

            //-- Act

            var obj = factory.CreateService<ITestSectionOne>();
            (obj as IInternalConfigurationObject).LoadObject(xml);

            //-- Assert

            Assert.That(obj.IntValue, Is.EqualTo(987));
            Assert.That(obj.StringValue, Is.EqualTo("XYZ"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLoadValuesOfAllSupportedTypesFromXml()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var xml = XElement.Parse(
                @"<Two 
                    timeSpanValue='00:00:05.250' 
                    dateTimeValue1='2014-02-22'
                    dateTimeValue2='2014-02-22 15:30:11'
                    enumValue='Monday'
                    guidValue='F3377322-C42E-4DB2-BAF5-07EE7D5647BA'
                    typeValue1='NWheels.IFramework, NWheels' 
                    boolValue1='true'
                    boolValue2='True' />");

            //-- Act

            var obj = factory.CreateService<ITestSectionTwo>();
            (obj as IInternalConfigurationObject).LoadObject(xml);

            //-- Assert

            Assert.That(obj.TimeSpanValue, Is.EqualTo(TimeSpan.FromSeconds(5).Add(TimeSpan.FromMilliseconds(250))));
            Assert.That(obj.DateTimeValue1, Is.EqualTo(new DateTime(2014, 02, 22)));
            Assert.That(obj.DateTimeValue2, Is.EqualTo(new DateTime(2014, 02, 22, 15, 30, 11)));
            Assert.That(obj.EnumValue, Is.EqualTo(DayOfWeek.Monday));
            Assert.That(obj.GuidValue, Is.EqualTo(new Guid("F3377322-C42E-4DB2-BAF5-07EE7D5647BA")));
            Assert.That(obj.TypeValue1, Is.EqualTo(typeof(IFramework)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanOverrideValuesThroughXml()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var xmlOverride1 = XElement.Parse(@"<One intValue='999' />");
            var xmlOverride2 = XElement.Parse(@"<One stringValue='ZZZ' />");

            //-- Act

            var obj = factory.CreateService<ITestSectionOne>();

            var intValueInitial = obj.IntValue;
            var stringValueInitial = obj.StringValue;

            (obj as IInternalConfigurationObject).LoadObject(xmlOverride1);

            var intValueOverride1 = obj.IntValue;
            var stringValueOverride1 = obj.StringValue;

            (obj as IInternalConfigurationObject).LoadObject(xmlOverride2);

            var intValueOverride2 = obj.IntValue;
            var stringValueOverride2 = obj.StringValue;

            //-- Assert

            Assert.That(intValueInitial, Is.EqualTo(0));
            Assert.That(stringValueInitial, Is.Null);

            Assert.That(intValueOverride1, Is.EqualTo(999));
            Assert.That(stringValueOverride1, Is.Null);

            Assert.That(intValueOverride2, Is.EqualTo(999));
            Assert.That(stringValueOverride2, Is.EqualTo("ZZZ"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateSectionWithNestedElements()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();

            //-- Act

            var section = factory.CreateService<ITestSectionThree>();

            //-- Assert

            Assert.That(section.Four, Is.Not.Null);
            Assert.That(section.Five, Is.Not.Null);
            Assert.That(section.Five.IntValue, Is.EqualTo(123));
            Assert.That(section.Five.StringValue, Is.EqualTo("ABC"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateSectionWithNestedElementCollections()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();

            //-- Act

            var section = factory.CreateService<ITestSectionSix>();

            //-- Assert

            Assert.That(section.Sevens, Is.Not.Null);
            Assert.That(section.Sevens.Count, Is.EqualTo(0));
            Assert.That(section.Eights, Is.Not.Null);
            Assert.That(section.Eights.Count, Is.EqualTo(0));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLoadNestedElementsFromXml()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var xml = XElement.Parse(
                @"<Three 
                    dateValue='2014-02-22'
                    timeValue='00:00:05'>
                    <Four
                        intValue='987'
                        stringValue='ZXY' />
                    <Five
                        intValue='654'
                        stringValue='WVU' />
                </Three>"
            );

            //-- Act

            var section = factory.CreateService<ITestSectionThree>();
            (section as IInternalConfigurationObject).LoadObject(xml);

            //-- Assert

            Assert.That(section.DateValue, Is.EqualTo(new DateTime(2014, 02, 22)));
            Assert.That(section.TimeValue, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(section.Four.IntValue, Is.EqualTo(987));
            Assert.That(section.Four.StringValue, Is.EqualTo("ZXY"));
            Assert.That(section.Five.IntValue, Is.EqualTo(654));
            Assert.That(section.Five.StringValue, Is.EqualTo("WVU"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanLoadNestedElementCollectionsFromXml()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var xml = XElement.Parse(
                @"<Six 
                    dateValue='2014-02-22'
                    timeValue='00:00:05'>
                    <Sevens>
                        <Seven intValue='71' stringValue='S1' />
                    </Sevens>
                    <Eights>
                        <Eight intValue='81' stringValue='E1' />
                        <Eight intValue='82' stringValue='E2' />
                    </Eights>
                </Six>"
            );

            //-- Act

            var section = factory.CreateService<ITestSectionSix>();
            (section as IInternalConfigurationObject).LoadObject(xml);

            //-- Assert

            var sevens = section.Sevens.ToArray();
            var eights = section.Eights.ToArray();

            Assert.That(section.DateValue, Is.EqualTo(new DateTime(2014, 02, 22)));
            Assert.That(section.TimeValue, Is.EqualTo(TimeSpan.FromSeconds(5)));

            Assert.That(sevens.Length, Is.EqualTo(1));
            Assert.That(sevens[0].IntValue, Is.EqualTo(71));
            Assert.That(sevens[0].StringValue, Is.EqualTo("S1"));

            Assert.That(eights.Length, Is.EqualTo(2));
            Assert.That(eights[0].IntValue, Is.EqualTo(81));
            Assert.That(eights[0].StringValue, Is.EqualTo("E1"));
            Assert.That(eights[1].IntValue, Is.EqualTo(82));
            Assert.That(eights[1].StringValue, Is.EqualTo("E2"));

            Assert.That(eights[0].GetType(), Is.SameAs(eights[1].GetType()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanCreateSectionWithNamedElementCollection()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();

            //-- Act

            var section = factory.CreateService<ITestSectionNine>();

            //-- Assert

            Assert.That(section.Tens, Is.Not.Null);
            Assert.That(section.Tens.Count, Is.EqualTo(0));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanAddNamedElementsToCollection()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var section = factory.CreateService<ITestSectionNine>();

            //-- Act

            var ten1 = section.Tens.Add("AAA");
            var ten2 = section.Tens.Add("BBB");

            //-- Assert

            Assert.That(section.Tens, Is.Not.Null);
            Assert.That(section.Tens.Count, Is.EqualTo(2));
            Assert.That(section.Tens, Is.EquivalentTo(new[] { ten1, ten2 }));
            Assert.That(ten1.GetType(), Is.SameAs(ten2.GetType()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanGetElementsByName()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var section = factory.CreateService<ITestSectionNine>();

            //-- Act

            var ten1 = section.Tens.Add("AAA");
            var ten2 = section.Tens.Add("BBB");

            //-- Assert

            Assert.That(section.Tens["AAA"], Is.SameAs(ten1));
            Assert.That(section.Tens["BBB"], Is.SameAs(ten2));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void NameLookupIsCaseInsensitive()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var section = factory.CreateService<ITestSectionNine>();

            //-- Act

            var ten1 = section.Tens.Add("AAA");
            var ten2 = section.Tens.Add("BBB");

            //-- Assert

            Assert.That(section.Tens["aaa"], Is.SameAs(ten1));
            Assert.That(section.Tens["bbb"], Is.SameAs(ten2));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(KeyNotFoundException))]
        public void AttemptToGetElementWithNonExistentNameThrows()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var section = factory.CreateService<ITestSectionNine>();

            //-- Act

            section.Tens.Add("AAA");
            section.Tens.Add("BBB");

            //-- Assert

            var ten3 = section.Tens["zzz"];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void AttemptToAddElementWithDuplicateNameThrows()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var section = factory.CreateService<ITestSectionNine>();

            //-- Act

            section.Tens.Add("AAA");
            section.Tens.Add("AAA");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanOverrideNamedElementsInCollectionThroughXml()
        {
            //-- Arrange

            var factory = CreateFactoryUnderTest();
            var xml = XElement.Parse(
                @"<Nine dayOfWeek='Tuesday'>
                    <Tens>
                        <Ten name='BBB' intValue='222' />
                        <Ten name='CCC' intValue='333' />
                    </Tens>
                </Nine>"
            );

            var section = factory.CreateService<ITestSectionNine>();
            
            section.DayOfWeek = DayOfWeek.Friday;

            var ten1 = section.Tens.Add("AAA");
            ten1.IntValue = 11;
            
            var ten2 = section.Tens.Add("BBB");
            ten2.IntValue = 22;

            //-- Act

            section.AsInternalConfigurationObject().LoadObject(xml);
            
            //-- Assert

            Assert.That(section.Tens.Count, Is.EqualTo(3));
            Assert.That(section.Tens["AAA"].IntValue, Is.EqualTo(11));
            Assert.That(section.Tens["BBB"].IntValue, Is.EqualTo(222));
            Assert.That(section.Tens["CCC"].IntValue, Is.EqualTo(333));

            Assert.That(section.DayOfWeek, Is.EqualTo(DayOfWeek.Tuesday));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanRememberOverrideHistory()
        {
            //-- Arrange

            var xml1 = 
                @"<Nine dayOfWeek='Tuesday'>
                    <Tens>
                        <Ten name='BBB' intValue='222' />
                        <Ten name='CCC' intValue='333' />
                    </Tens>
                </Nine>";
            var xml2 =
                @"<Nine timeSpan='12:13:14' dayOfWeek='Wednesday'>
                    <Tens>
                        <Ten name='AAA' intValue='777' />
                        <Ten name='CCC' intValue='999' />
                    </Tens>
                </Nine>";

            var factory = CreateFactoryUnderTest();
            var section = factory.CreateService<ITestSectionNine>();

            //-- Act

            section.Tens.Add("AAA").IntValue = 123;

            using ( ConfigurationSourceInfo.UseSource(ConfigurationSourceLevel.Code, "XML-1") )
            {
                section.AsInternalConfigurationObject().LoadObject(LoadXmlWithLineInfoFromString(xml1));
            }

            using ( ConfigurationSourceInfo.UseSource(ConfigurationSourceLevel.Code, "XML-2") )
            {
                section.AsInternalConfigurationObject().LoadObject(LoadXmlWithLineInfoFromString(xml2));
            }

            var nineHistory = section.AsInternalConfigurationObject().GetOverrideHistory();
            var aaaHistory = section.Tens["AAA"].AsInternalConfigurationObject().GetOverrideHistory();
            var bbbHistory = section.Tens["BBB"].AsInternalConfigurationObject().GetOverrideHistory();
            var cccHistory = section.Tens["CCC"].AsInternalConfigurationObject().GetOverrideHistory();

            //-- Assert

            Assert.That(nineHistory.Select(h => h.Source.Name), Is.EquivalentTo(new[] { "XML-1", "XML-2" }));

            Assert.That(nineHistory["TimeSpan"].Select(h => h.Value), Is.EqualTo(new[] { "11:12:13", "12:13:14" }));
            Assert.That(nineHistory["TimeSpan"].Select(h => h.Source.Name), Is.EqualTo(new[] { "Default", "XML-2" }));

            //TODO: changes done to configuration objects through code are not reflected in the override history
            //Assert.That(aaaHistory.Select(h => h.Source.Name), Is.EquivalentTo(new[] { "Program", "XML-2" }));
            //Assert.That(aaaHistory["IntValue"].Select(h => h.Value), Is.EquivalentTo(new[] { "123", "777" }));
            //Assert.That(aaaHistory["IntValue"].Select(h => h.Source.Name), Is.EquivalentTo(new[] { "Program", "XML-2" }));

            Assert.That(cccHistory.Select(h => h.Source.Name), Is.EquivalentTo(new[] { "XML-1", "XML-2" }));

            Assert.That(cccHistory["IntValue"].Select(h => h.Value), Is.EquivalentTo(new[] { "333", "999" }));
            Assert.That(cccHistory["IntValue"].Select(h => h.Source.Name), Is.EquivalentTo(new[] { "XML-1", "XML-2" }));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private XElement LoadXmlWithLineInfoFromString(string xmlString)
        {
            XDocument xml;

            using ( TextReader reader = new StringReader(xmlString) )
            {
                xml = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }

            return xml.Root;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ConfigurationObjectFactory CreateFactoryUnderTest()
        {
            var framework = new TestFramework(base.Module);
            return new ConfigurationObjectFactory(framework.Components, base.Module, framework.MetadataCache);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "One")]
        public interface ITestSectionOne : IConfigurationSection
        {
            int IntValue { get; }
            string StringValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "Two")]
        public interface ITestSectionTwo : IConfigurationSection
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

            [DefaultValue(false)]
            bool BoolValue1 { get; }

            [DefaultValue(false)]
            bool BoolValue2 { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "Three")]
        public interface ITestSectionThree : IConfigurationSection
        {
            DateTime DateValue { get; }
            TimeSpan TimeValue { get; }
            ITestElementFour Four { get; }
            ITestElementFive Five { get; }
        }
        [ConfigurationElement(XmlName = "Four")]
        public interface ITestElementFour : IConfigurationElement
        {
            int IntValue { get; }
            string StringValue { get; }
        }
        [ConfigurationElement(XmlName = "Five")]
        public interface ITestElementFive : IConfigurationElement
        {
            [DefaultValue(123)]
            int IntValue { get; }
            [DefaultValue("ABC")]
            string StringValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "Three")]
        public interface ITestSectionSix : IConfigurationSection
        {
            DateTime DateValue { get; }
            TimeSpan TimeValue { get; }
            ICollection<ITestElementSeven> Sevens { get; }
            ICollection<ITestElementEight> Eights { get; }
        }
        [ConfigurationElement(XmlName = "Seven")]
        public interface ITestElementSeven : IConfigurationElement
        {
            int IntValue { get; }
            string StringValue { get; }
        }
        [ConfigurationElement(XmlName = "Eight")]
        public interface ITestElementEight : IConfigurationElement
        {
            int IntValue { get; }
            string StringValue { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [ConfigurationSection(XmlName = "Nine")]
        public interface ITestSectionNine : IConfigurationSection
        {
            DayOfWeek DayOfWeek { get; set; }
            
            [PropertyContract.DefaultValue("11:12:13")]
            TimeSpan TimeSpan { get; set; }
            
            INamedObjectCollection<ITestElementTen> Tens { get; }
        }
        [ConfigurationElement(XmlName = "Ten")]
        public interface ITestElementTen : INamedConfigurationElement
        {
            int IntValue { get; set; }
            string StringValue { get; set; }
        }
    }
}
