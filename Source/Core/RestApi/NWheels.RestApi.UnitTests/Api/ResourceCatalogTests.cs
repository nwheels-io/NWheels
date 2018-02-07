using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NWheels.RestApi.Api;
using NWheels.Testability;
using Xunit;

namespace NWheels.RestApi.UnitTests.Api
{
    public class ResourceCatalogTests : TestBase.UnitTest
    {
        class Resource1 { }
        class Resource2 { }
        class Protocol1 { }
        class Protocol2 { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGetValuesConstructedWith()
        {
            //-- arrange

            var baseUriPath = "http://base.uri";
            var resources = new[] {typeof(Resource1), typeof(Resource2)};
            var protocols = new[] {typeof(Protocol1), typeof(Protocol2)};

            //-- act

            var catalog = new ResourceCatalog(baseUriPath, resources, protocols);
            
            //-- assert

            catalog.BaseUriPath.Should().Be(baseUriPath);

            catalog.Resources.Should().Equal(resources);
            catalog.Resources.Should().NotBeSameAs(resources);

            catalog.Protocols.Should().Equal(protocols);
            catalog.Protocols.Should().NotBeSameAs(protocols);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_BaseUriPathNull_Throw()
        {
            //-- arrange

            Action act = () => {
                var catalog = new ResourceCatalog(null, new Type[0], new Type[0]);
            };

            //-- act

            ArgumentNullException exception = act.ShouldThrow<ArgumentNullException>().Which;

            //-- assert

            exception.ParamName.Should().Be("baseUriPath");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_ResourcesNull_Throw()
        {
            //-- arrange

            Action act = () => {
                var catalog = new ResourceCatalog(string.Empty, resources: null, protocols: new Type[0]);
            };

            //-- act

            ArgumentNullException exception = act.ShouldThrow<ArgumentNullException>().Which;

            //-- assert

            exception.ParamName.Should().Be("resources");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_ProtocolsNull_Throw()
        {
            //-- arrange

            Action act = () => {
                var catalog = new ResourceCatalog(string.Empty, resources: new Type[0], protocols: null);
            };

            //-- act

            ArgumentNullException exception = act.ShouldThrow<ArgumentNullException>().Which;

            //-- assert

            exception.ParamName.Should().Be("protocols");
        }
    }
}
