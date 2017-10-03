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
            typeof(UnderTest.TestException), 
            UnderTestNamespace + ".TestException")]
        [InlineData(
            typeof(UnderTest.ContainerClass.NestedException), 
            UnderTestNamespace + ".ContainerClass.NestedException")]
        [InlineData(
            typeof(UnderTest.GenericContainerClass<string>.NestedException<DayOfWeek>), 
            UnderTestNamespace + ".GenericContainerClass<String>.NestedException<DayOfWeek>")]
        public void ExplanationPath_FriendlyExceptionTypeName(Type exceptionType, string expectedExplanationPath)
        {
            //-- act

            var exception = (ExplainableExceptionBase)Activator.CreateInstance(exceptionType, "TestReason", new[] { "K1", "V1" });

            //-- assert

            exception.ExplanationPath.Should().Be(expectedExplanationPath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ExplanationPath_Cached()
        {
            //-- arrange

            var exception = new UnderTest.TestException("TestReason", new[] { "K1", "V1" });

            //-- act

            var explanationPath1 = exception.ExplanationPath;
            var explanationPath2 = exception.ExplanationPath;

            //-- assert

            explanationPath1.Should().NotBeNull();
            explanationPath2.Should().BeSameAs(explanationPath1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData("TestReason", null, "Reason=TestReason")]
        [InlineData("TestReason", "", "Reason=TestReason")]
        [InlineData("TestReason", "K1,V1", "Reason=TestReason&K1=V1")]
        [InlineData("TestReason", "K1,V1,K2,V2","Reason=TestReason&K1=V1&K2=V2")]
        [InlineData("Test & Reason", "K1,V&1,K2,V 2", "Reason=Test%20%26%20Reason&K1=V%261&K2=V%202")]
        [InlineData("Yet.Another-Reason", "K1,V1", "Reason=Yet.Another-Reason&K1=V1")]
        public void ExplanationQuery_CorrectlyEncoded(string reason, string keyValues, string expectedQuery)
        {
            //-- arrange

            var keyValuePairs = keyValues?.Split(",", StringSplitOptions.RemoveEmptyEntries);
            
            if (keyValues == null)
            {
                keyValuePairs.Should().BeNull();
            }
            if (keyValues == "")
            {
                keyValuePairs.Should().BeEmpty();
            }

            var exception = new UnderTest.TestException(reason, keyValuePairs);

            //-- act 

            var actualQuery = exception.ExplanationQuery;

            //-- assert

            actualQuery.Should().Be(expectedQuery);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ExplanationQuery_NullValues_EncodedAsEmpty()
        {
            //-- arrange

            var exception = new UnderTest.TestException("TestReason", new string[] { "K1", null, "K2", "V2" });

            //-- act 

            var actualQuery = exception.ExplanationQuery;

            //-- assert

            actualQuery.Should().Be("Reason=TestReason&K1=&K2=V2");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ExplanationQuery_Cached()
        {
            //-- arrange

            var exception = new UnderTest.TestException("TestReason", new string[] { "K1", "V1", "K2", "V2" });

            //-- act 

            var actualQuery1 = exception.ExplanationQuery;
            var actualQuery2 = exception.ExplanationQuery;

            //-- assert

            actualQuery1.Should().Be("Reason=TestReason&K1=V1&K2=V2");
            actualQuery2.Should().BeSameAs(actualQuery1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ConstructorWithInnerException()
        {
            //-- arrange

            var innerException = new DivideByZeroException();

            //-- act

            var exception = new UnderTest.WithInnerException("TestReason", innerException);

            //-- assert

            exception.InnerException.Should().BeSameAs(innerException);
        }
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

            protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
            {
                if (_keyValuePairs == null)
                {
                    return null;
                }

                var pairs = new List<KeyValuePair<string, string>>();

                for (int i = 0 ; i < _keyValuePairs.Length - 1 ; i += 2)
                {
                    pairs.Add(new KeyValuePair<string, string>(_keyValuePairs[i], _keyValuePairs[i + 1]));
                }

                return pairs;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithInnerException : ExplainableExceptionBase
        {
            public WithInnerException(string reason, Exception innerException)
                : base(reason, innerException)
            {
            }

            protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ContainerClass
        {
            public class NestedException : ExplainableExceptionBase
            {
                public NestedException(string reason, string[] keyValuePairs)
                    : base(reason)
                {
                }

                protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
                {
                    return null;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class GenericContainerClass<T>
        {
            public class NestedException<K> : ExplainableExceptionBase
            {
                public NestedException(string reason, string[] keyValuePairs)
                    : base(reason)
                {
                }

                protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
                {
                    return null;
                }
            }
        }
    }
}
