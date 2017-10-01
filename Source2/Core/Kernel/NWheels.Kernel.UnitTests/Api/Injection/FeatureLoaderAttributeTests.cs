using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Testability;
using Xunit;
using FluentAssertions;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.UnitTests.Api.Injection
{
    public class FeatureLoaderAttributeTests : TestBase.UnitTest
    {
        [Fact]
        public void GetFeatureNameOrThrow_NameValid_NameReturned()
        {
            //-- act

            var name = FeatureLoaderAttribute.GetFeatureNameOrThrow(typeof(GoodNamedFeature));

            //-- assert

            name.Should().Be("Good");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(typeof(EmptyNamedFeature))]
        [InlineData(typeof(NullNamedFeature))]
        [InlineData(typeof(UnnamedFeature))]
        public void GetFeatureNameOrThrow_NameMissingOrInvalid_Throw(Type featureLoaderType)
        {
            //-- act

            Action act = () => {
                var name = FeatureLoaderAttribute.GetFeatureNameOrThrow(featureLoaderType);
            };

            //-- assert

            act.ShouldThrow<FeatureLoaderException>().Where(exc => 
                exc.Reason == nameof(FeatureLoaderException.FeatureNameMissingOrInvalid) &&
                exc.FeatureLoaderType == featureLoaderType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "Good")]
        public class GoodNamedFeature
        {
        }
        [FeatureLoader(Name = "")]
        public class EmptyNamedFeature
        {
        }
        [FeatureLoader]
        public class NullNamedFeature
        {
        }
        public class UnnamedFeature
        {
        }
    }
}
