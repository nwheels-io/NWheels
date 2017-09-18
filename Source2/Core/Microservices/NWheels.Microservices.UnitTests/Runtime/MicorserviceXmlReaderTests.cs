using FluentAssertions;
using NWheels.Microservices;
using NWheels.Microservices.Api;
using System.IO;
using System.Xml.Serialization;
using Xunit;
using System.Xml.Linq;
using NWheels.Kernel.Api.Injection;
using NWheels.Testability;
using NWheels.Microservices.Runtime;

namespace NWheels.Microservices.UnitTests.Runtime
{
    public class MicroserviceXmlReaderTests : TestBase.UnitTest
    {
        [Fact]
        public void EmptyBootConfig_ValidXml()
        {
            //-- arrange

            var xml = @"
                <microservice name='MyService'>
                    <framework-modules>
                        <module assembly='FX.M1' />
                        <module assembly='FX.M2'>
                            <feature name='FX-M2-F1' />
                            <feature name='FX-M2-F2' />
                        </module>
                    </framework-modules>
                    <application-modules>
                        <module assembly='App.M3'>
                            <feature name='app-m3-f3' />
                        </module>
                        <module assembly='App.M4' />
                    </application-modules>
                    <customization-modules>
                        <module assembly='App.Custom.M5' />
                        <module assembly='App.Custom.M6'>
                            <feature name='custom-m6-f4' />
                        </module>
                    </customization-modules>
                </microservice>";

            var parsedXml = XElement.Parse(xml);
            var bootConfig = new MutableBootConfiguration();

            //-- act

            MicroserviceXmlReader.PopulateBootConfiguration(parsedXml, bootConfig);

            //-- assert

            bootConfig.MicroserviceName.Should().Be("MyService");
            bootConfig.FrameworkModules.Count.Should().Be(2);

            bootConfig.FrameworkModules[0].AssemblyName.Should().Be("FX.M1");
            bootConfig.FrameworkModules[0].RuntimeAssembly.Should().BeNull();
            bootConfig.FrameworkModules[0].Features.Should().BeEmpty();

            bootConfig.FrameworkModules[1].AssemblyName.Should().Be("FX.M2");
            bootConfig.FrameworkModules[1].RuntimeAssembly.Should().BeNull();
            bootConfig.FrameworkModules[1].Features.Count.Should().Be(2);
            bootConfig.FrameworkModules[1].Features[0].FeatureName.Should().Be("FX-M2-F1");
            bootConfig.FrameworkModules[1].Features[1].FeatureName.Should().Be("FX-M2-F2");

            bootConfig.ApplicationModules.Count.Should().Be(2);
            
            bootConfig.ApplicationModules[0].AssemblyName.Should().Be("App.M3");
            bootConfig.ApplicationModules[0].RuntimeAssembly.Should().BeNull();
            bootConfig.ApplicationModules[0].Features.Count.Should().Be(1);
            bootConfig.ApplicationModules[0].Features[0].FeatureName.Should().Be("app-m3-f3");
            
            bootConfig.ApplicationModules[1].AssemblyName.Should().Be("App.M4");
            bootConfig.ApplicationModules[1].RuntimeAssembly.Should().BeNull();
            bootConfig.ApplicationModules[1].Features.Should().BeEmpty();
            
            bootConfig.CustomizationModules.Count.Should().Be(2);
            
            bootConfig.CustomizationModules[0].AssemblyName.Should().Be("App.Custom.M5");
            bootConfig.CustomizationModules[0].RuntimeAssembly.Should().BeNull();
            bootConfig.CustomizationModules[0].Features.Should().BeEmpty();

            bootConfig.CustomizationModules[1].AssemblyName.Should().Be("App.Custom.M6");
            bootConfig.CustomizationModules[1].RuntimeAssembly.Should().BeNull();
            bootConfig.CustomizationModules[1].Features.Count.Should().Be(1);
            bootConfig.CustomizationModules[1].Features[0].FeatureName.Should().Be("custom-m6-f4");
        }
    }
}