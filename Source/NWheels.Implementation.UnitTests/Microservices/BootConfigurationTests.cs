using FluentAssertions;
using NWheels.Microservices;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class BootConfigurationTests
    {
        [Fact]
        public void DeserializeMicroserviceConfig()
        {
            //-- arrange

            var xml = @"
                <microservice name=""BackendApi"">
                    <injection-adapter assembly=""NWheels.Injection.Adapters.Autofac"" />
                    <framework-modules>
                        <module assembly=""NWheels.Platform.Database"" />
                        <module assembly=""NWheels.Platform.Messaging"">
                            <feature name=""MessageBus"" />
                            <feature name=""PubSub"" />
                        </module>
                    </framework-modules>
                    <application-modules>
                        <module assembly=""ExpenseTracker"">
                            <feature name=""Domain"" />
                        </module>
                        <module assembly=""ExpenseTracker.BackendApi"" />
                    </application-modules>
                </microservice>";

            //-- act

            var serializer = new XmlSerializer(typeof(MicroserviceConfig));
            var reader = new StringReader(xml);
            var result = (MicroserviceConfig)serializer.Deserialize(reader);

            //-- assert

            result.Should().NotBeNull();
            result.Name.Should().Be("BackendApi");
            result.InjectionAdapter.Assembly.Should().Be("NWheels.Injection.Adapters.Autofac");
            result.FrameworkModules.Count.Should().Be(2);
            result.FrameworkModules[0].Assembly.Should().Be("NWheels.Platform.Database");
            result.FrameworkModules[1].Features.Count.Should().Be(2);
            result.FrameworkModules[1].Features[1].Name.Should().Be("PubSub");
            result.ApplicationModules.Count.Should().Be(2);
        }

        [Fact]
        public void DeserializeEnvironmentConfig()
        {
            //-- arrange

            var xml = @"
                <environment name=""uk-main"">
                    <variable-list>
                        <variable 
                            name=""db-connection"" 
                            value=""server=uklw6735;schema=my_app_db;user=my_db_user;password={secret:db_password}"" />
                        <variable 
                            name=""input-queue-url""
                            value=""my-queue-address"" />
                    </variable-list>
                </environment>";

            //-- act

            var serializer = new XmlSerializer(typeof(EnvironmentConfig));
            var reader = new StringReader(xml);
            var result = (EnvironmentConfig)serializer.Deserialize(reader);

            //-- assert

            result.Should().NotBeNull();
            result.Name.Should().Be("uk-main");
            result.Variables.Length.Should().Be(2);
            result.Variables[1].Name.Should().Be("input-queue-url");
            result.Variables[1].Value.Should().Be("my-queue-address");
        }

        [Fact]
        public void SerializeMicroserviceConfig()
        {
            //-- arrange

            var config = new MicroserviceConfig()
            {
                Name = "test",
                InjectionAdapter = new MicroserviceConfig.InjectionAdapterWrapper()
                {
                    Assembly = "InjectionAdapter"
                },
                FrameworkModules = new List<MicroserviceConfig.ModuleConfig>()
                {
                    new MicroserviceConfig.ModuleConfig()
                    {
                        Assembly = "FrameworkAssembly"
                    }
                },
                ApplicationModules = new List<MicroserviceConfig.ModuleConfig>()
                {
                    new MicroserviceConfig.ModuleConfig()
                    {
                        Assembly="TestAssembly"
                    }
                }
            };

            //-- act

            var serializer = new XmlSerializer(typeof(MicroserviceConfig));
            var writer = new StringWriter();
            serializer.Serialize(writer, config);

            var xml = writer.ToString();
        }
    }
}
