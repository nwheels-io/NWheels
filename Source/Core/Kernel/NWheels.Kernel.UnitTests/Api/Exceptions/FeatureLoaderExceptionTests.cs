using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Testability;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Kernel.UnitTests.Api.Exceptions
{
    public class FeatureLoaderExceptionTests : TestBase.UnitTest
    {
        [Fact]
        public void BuildKeyValuePairs_FeatureLoaderTypeIncluded()
        {
            //-- arrange
            
            var exception = FeatureLoaderException.FeatureNameMissingOrInvalid(typeof(TestFeature));

            //-- act

            var pathAndQuery = exception.ExplanationQuery;

            //-- assert

            var expectedFeatureLoaderType = "NWheels.Kernel.UnitTests.Api.Exceptions.FeatureLoaderExceptionTests.TestFeature";
            pathAndQuery.Should().Be($"Reason={nameof(FeatureLoaderException.FeatureNameMissingOrInvalid)}&featureLoaderType={expectedFeatureLoaderType}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestFeature : AdvancedFeature
        {
        }
    }
}
