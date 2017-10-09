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
        [InlineData("TestReason", null, "Test reason")]
        [InlineData("TestReason", "", "Test reason")]
        [InlineData("TestReason", "K1,V1", "Test reason: K1=V1")]
        [InlineData("TestReason", "K1,V1,K2,V2", "Test reason: K1=V1, K2=V2")]
        public void Message_HumanReadable(string reason, string keyValues, string expectedMessage)
        {
            //-- arrange

            var exception = new UnderTest.TestException(reason, keyValues);

            //-- act

            var actualMessage = exception.Message;

            //-- assert

            actualMessage.Should().Be(expectedMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Message_Cached()
        {
            //-- arrange

            var exception = new UnderTest.TestException("TestReason", "K1,V1");

            //-- act

            var message1 = exception.Message;
            var message2 = exception.Message;

            //-- assert

            message2.Should().BeSameAs(message1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData("TestReason", null, "Reason=TestReason")]
        [InlineData("TestReason", "", "Reason=TestReason")]
        [InlineData("TestReason", "K1,V1", "Reason=TestReason&K1=V1")]
        [InlineData("TestReason", "K1,V1,K2,V2", "Reason=TestReason&K1=V1&K2=V2")]
        public void HelpLink_AbsoluteHelpUrl(string reason, string keyValues, string expectedExplanationQuery)
        {
            //-- arrange

            var exception = new UnderTest.TestException(reason, keyValues);
            var expectedHelpLink = 
                $"{ExplainableExceptionBase.DefaultHelpLinkBaseUri}{UnderTestNamespace}.{nameof(UnderTest.TestException)}?{expectedExplanationQuery}";

            //-- act

            var actualHelpLink = exception.HelpLink;

            //-- assert

            actualHelpLink.Should().Be(expectedHelpLink);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void HelpLink_Cached()
        {
            //-- arrange

            var exception = new UnderTest.TestException("TestReason", "K1,V1");

            //-- act

            var helpLink1 = exception.HelpLink;
            var helpLink2 = exception.HelpLink;

            //-- assert

            helpLink2.Should().BeSameAs(helpLink1);
        }

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

            var exception = new UnderTest.TestException(reason, keyValues);

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
        public void InnerException_Constructor()
        {
            //-- arrange

            var innerException = new DivideByZeroException();

            //-- act

            var exception = new UnderTest.WithInnerException("TestReason", innerException);

            //-- assert

            exception.InnerException.Should().BeSameAs(innerException);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData("TestReason", null, "Test reason {Object reference not set to an instance of an object.}")]
        [InlineData("TestReason", "K1,V1", "Test reason: K1=V1 {Object reference not set to an instance of an object.}")]
        [InlineData("TestReason", "K1,V1,K2,V2", "Test reason: K1=V1, K2=V2 {Object reference not set to an instance of an object.}")]
        public void InnerException_IncludedInMessage(string reason, string keyValues, string expectedMessage)
        {
            //-- arrange

            var innerException = new NullReferenceException();
            var exception = new UnderTest.WithInnerException(reason, keyValues, innerException);

            //-- act

            var message = exception.Message;

            //-- assert

            message.Should().Be(expectedMessage);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace UnderTest
    {
        public class TestException : ExplainableExceptionBase
        {
            private readonly KeyValuePair<string, string>[] _keyValuePairs;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestException(string reason, string commaSeparatedKeyValues)
                : this(reason, ParseCommaSeparatedKeyValues(commaSeparatedKeyValues))
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestException(string reason, string[] flatKeyValuesArray) 
                : base(reason)
            {
                _keyValuePairs = BuildKeyValuePairs(flatKeyValuesArray);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
            {
                return _keyValuePairs;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static string[] ParseCommaSeparatedKeyValues(string commaSeparatedKeyValues)
            {
                var keyValuesArray = commaSeparatedKeyValues?.Split(",", StringSplitOptions.RemoveEmptyEntries);

                if (commaSeparatedKeyValues == null)
                {
                    keyValuesArray.Should().BeNull();
                }

                if (commaSeparatedKeyValues == "")
                {
                    keyValuesArray.Should().BeEmpty();
                }

                return keyValuesArray;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            internal static KeyValuePair<string, string>[] BuildKeyValuePairs(string[] flatKeyValuesArray)
            {
                if (flatKeyValuesArray == null)
                {
                    return null;
                }

                var keyValuePairs = new KeyValuePair<string, string>[flatKeyValuesArray.Length / 2];

                for (int i = 0; i < keyValuePairs.Length; i++)
                {
                    keyValuePairs[i] = new KeyValuePair<string, string>(
                        key: flatKeyValuesArray[i * 2],
                        value: flatKeyValuesArray[i * 2 + 1]);
                }

                return keyValuePairs;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithInnerException : ExplainableExceptionBase
        {
            private readonly KeyValuePair<string, string>[] _keyValuePairs;

            public WithInnerException(string reason, Exception innerException)
                : base(reason, innerException)
            {
            }

            public WithInnerException(string reason, string commaSeparatedKeyValues, Exception innerException)
                : base(reason, innerException)
            {
                _keyValuePairs = TestException.BuildKeyValuePairs(TestException.ParseCommaSeparatedKeyValues(commaSeparatedKeyValues));
            }

            protected override IEnumerable<KeyValuePair<string, string>> BuildKeyValuePairs()
            {
                return _keyValuePairs;
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
