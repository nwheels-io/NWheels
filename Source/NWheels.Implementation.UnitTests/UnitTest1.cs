using NWheels.Configuration;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace NWheels.Implementation.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void DeserializeMicroserviceConfig()
        {
            var xml = @"
                <microservice name=""BackendApi"">
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

            var serializer = new XmlSerializer(typeof(MicroserviceConfig));
            var reader = new StringReader(xml);
            var result = (MicroserviceConfig)serializer.Deserialize(reader);

            Assert.NotNull(result);
            Assert.Equal(result.Name, "BackendApi");
            Assert.Equal(result.FrameworkModules.Length, 2);
            Assert.Equal(result.FrameworkModules[0].Assembly, "NWheels.Platform.Database");
            Assert.Equal(result.FrameworkModules[1].Features.Length, 2);
            Assert.Equal(result.FrameworkModules[1].Features[1].Name, "PubSub");
            Assert.Equal(result.ApplicationModules.Length, 2);
        }

        [Fact]
        public void DeserializeEnvironmentConfig()
        {
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

            var serializer = new XmlSerializer(typeof(EnvironmentConfig));
            var reader = new StringReader(xml);
            var result = (EnvironmentConfig)serializer.Deserialize(reader);

            Assert.NotNull(result);
            Assert.Equal(result.Name, "uk-main");
            Assert.Equal(result.Variables.Length, 2);
            Assert.Equal(result.Variables[1].Name, "input-queue-url");
            Assert.Equal(result.Variables[1].Value, "my-queue-address");
        }
    }
}
