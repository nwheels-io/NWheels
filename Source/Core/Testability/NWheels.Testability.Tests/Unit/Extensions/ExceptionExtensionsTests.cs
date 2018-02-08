using System;
using NWheels.Testability.Extensions;
using Xunit;
using FluentAssertions;

namespace NWheels.Testability.Tests.Unit.Extensions
{
    public class ExceptionExtensionsTests : TestBase.UnitTest
    {
        [Fact]
        public void InnermostException_NoInnerException_ReturnsNull()
        {
            //-- arrange

            var e = new Exception("test");

            //-- act
            
            var result = e.InnermostException();

            //-- assert

            result.Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void InnermostException_OneInnerException_ReturnsInner()
        {
            //-- arrange

            var e2 = new Exception("test-inner");
            var e = new Exception("test-outer", innerException: e2);

            //-- act
            
            var result = e.InnermostException();

            //-- assert

            result.Should().BeSameAs(e2);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void InnermostException_MultipleLevelInnerExceptions_ReturnsInnermost()
        {
            //-- arrange

            var e3 = new Exception("test-innermost");
            var e2 = new Exception("test-inner", innerException: e3);
            var e = new Exception("test-outer", innerException: e2);

            //-- act
            
            var result = e.InnermostException();

            //-- assert

            result.Should().BeSameAs(e3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public void InnermostException_NullArgument_Throw()
        {
            //-- arrange

            Exception e = null;

            //-- act
            
            Action act = () => {
                e.InnermostException();
            };

            //-- assert

            act.ShouldThrow<ArgumentNullException>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void FlattenException_NonAggregate_ReturnAsIs()
        {
            //-- arrange

            var inner = new Exception("TEST-INNER-ERROR");
            var input = new Exception("TEST-ERROR", inner);

            //-- act

            var output = input.Flatten();

            //-- assert

            output.Should().BeSameAs(input);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void FlattenException_AggregatesOneInner_ReturnInner()
        {
            //-- arrange

            var inner = new Exception("TEST-ERROR");
            Exception input = new AggregateException(inner);

            //-- act

            var output = input.Flatten(); // ExceptionExtensions.Flatten

            //-- assert

            output.Should().BeSameAs(inner);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void FlattenException_AggregatesMany_ReturnFlattened()
        {
            //-- arrange

            var inner1 = new Exception("TEST-ERROR-1");
            var inner2 = new Exception("TEST-ERROR-2");
            var inner3 = new Exception("TEST-ERROR-3");
            var innerAggregate = new AggregateException(inner2, inner3);
            Exception input = new AggregateException(inner1, innerAggregate);

            //-- act

            var output = input.Flatten(); // ExceptionExtensions.Flatten

            //-- assert

            var outputAggregate = (AggregateException)output;
            outputAggregate.InnerExceptions.Should().Equal(inner1, inner2, inner3);
        }
    }
}