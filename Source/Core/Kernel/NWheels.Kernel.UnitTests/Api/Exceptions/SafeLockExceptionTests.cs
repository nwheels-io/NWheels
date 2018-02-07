using System;
using System.Linq;
using System.Collections.Generic;
using NWheels.Testability;
using NWheels.Kernel.Api.Exceptions;
using Xunit;
using FluentAssertions;

namespace NWheels.Kernel.UnitTests.Api.Exceptions
{
    public class SafeLockExceptionTests : TestBase.UnitTest
    {
        [Fact]
        public void TestKeyValuePairs()
        {
            //-- arrange

            var exception = SafeLockException.TimedOutWaitingForAccess(resourceName: "TestResource", timeout: TimeSpan.FromSeconds(123));

            //-- act

            var keyValuePairs = new Dictionary<string, string>(exception.KeyValuePairs);

            //-- assert

            keyValuePairs[nameof(SafeLockException.ResourceName)].Should().Be("TestResource");
            keyValuePairs[nameof(SafeLockException.Timeout)].Should().Be("00:02:03"); // 123 seconds
        }
    }
}