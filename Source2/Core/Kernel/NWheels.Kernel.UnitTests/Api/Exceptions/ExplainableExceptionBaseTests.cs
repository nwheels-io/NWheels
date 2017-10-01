using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Testability;
using Xunit;
using FluentAssertions;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.UnitTests.Api.Exceptions
{
    public class ExplainableExceptionBaseTests : TestBase.UnitTest
    {
        public const string UnderTestNamespace = "NWheels.Kernel.UnitTests.Api.Exceptions.UnderTest";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(
            typeof(UnderTest.TestException), "TestReason", "",
            UnderTestNamespace + ".TestException/TestReason")]
        [InlineData(
            typeof(UnderTest.TestException), "TestReason", "K1,V1",
            UnderTestNamespace + ".TestException/TestReason?K1=V1")]
        [InlineData(
            typeof(UnderTest.TestException), "TestReason", "K1,V1,K2,V2",
            UnderTestNamespace + ".TestException/TestReason?K1=V1&K2=V2")]
        [InlineData(
            typeof(UnderTest.TestException), "Test & Reason", "K1,V&1,K2,V 2",
            UnderTestNamespace + ".TestException/Test%20&%20Reason?K1=V%261&K2=V+2")]
        [InlineData(
            typeof(UnderTest.ContainerClass.NestedException), "AnotherReason", "K1,V1",
            UnderTestNamespace + ".ContainerClass.NestedException/AnotherReason?K1=V1")]
        [InlineData(
            typeof(UnderTest.GenericContainerClass<int>.NestedException<string>), "Yet.Another-Reason", "K1,V1",
            UnderTestNamespace + ".GenericContainerClass<Int32>.NestedException<String>/Yet.Another-Reason?K1=V1")]

        public void TestExplanationPathAndQuery(
            Type exceptionType,
            string reason,
            string keyValues, 
            string expectedPathAndQuery)
        {
            //-- arrange

            var keyValuePairs = keyValues.Split(",");
            var exception = (ExplainableExceptionBase)Activator.CreateInstance(exceptionType, reason, keyValuePairs);

            //-- act 

            var actualPathAndQuery = exception.ExplanationPathAndQuery;

            //-- assert

            actualPathAndQuery.Should().Be(expectedPathAndQuery);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------



    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace UnderTest
    {
        public class TestException : ExplainableExceptionBase
        {
            private readonly string[] _keyValuePairs;

            public TestException(string reason, string[] keyValuePairs) 
                : base(reason)
            {
                _keyValuePairs = keyValuePairs;
            }

            protected override string[] BuildKeyValuePairs()
            {
                return _keyValuePairs;
            }
        }

        public class ContainerClass
        {
            public class NestedException : ExplainableExceptionBase
            {
                private readonly string[] _keyValuePairs;

                public NestedException(string reason, string[] keyValuePairs)
                    : base(reason)
                {
                    _keyValuePairs = keyValuePairs;
                }

                protected override string[] BuildKeyValuePairs()
                {
                    return _keyValuePairs;
                }
            }
        }

        public class GenericContainerClass<T>
        {
            public class NestedException<K> : ExplainableExceptionBase
            {
                private readonly string[] _keyValuePairs;

                public NestedException(string reason, string[] keyValuePairs)
                    : base(reason)
                {
                    _keyValuePairs = keyValuePairs;
                }

                protected override string[] BuildKeyValuePairs()
                {
                    return _keyValuePairs;
                }
            }
        }
    }
}
