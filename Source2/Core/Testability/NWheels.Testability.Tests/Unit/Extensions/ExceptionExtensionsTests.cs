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
    }
}