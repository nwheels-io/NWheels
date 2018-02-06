using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NWheels.Testability.Extensions;
using Xunit;
using Xunit.Sdk;

namespace NWheels.Testability.Tests.Unit.Extensions
{
    public class DelegateExtensionsTests : TestBase.UnitTest
    {
        public class ATestException : Exception
        {
            public ATestException(string message)
                : base(message)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldThrowException_ThrownExpected_ExceptionReturned()
        {
            //-- arrange

            var thrownException = new ATestException("TEST-ERROR");
            Action action = () => throw thrownException;

            //-- act

            var returnedException = action.ShouldThrowException<ATestException>(because: "this is the test");

            //-- assert

            returnedException.Should().BeSameAs(thrownException);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldThrowException_NoExceptionThrown_AssertFailed()
        {
            //-- arrange

            Action action = () => { };

            Action underTest = () => {
                action.ShouldThrowException<ATestException>(because: "this is the test");
            };

            //-- act & assert

            underTest.ShouldThrow<XunitException>().Which.Message.Should().Contain(
                $"Expected {typeof(ATestException).FullName} because this is the test, but no exception was thrown.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldThrowException_UnexpectedExceptionThrown_AssertFailed()
        {
            //-- arrange

            Action action = () => throw new DivideByZeroException("TEST-ERROR");

            Action underTest = () => {
                action.ShouldThrowException<ATestException>(because: "this is the test");
            };

            //-- act & assert

            underTest.ShouldThrow<XunitException>().Which.Message.Should().Contain(
                $"Expected {typeof(ATestException).FullName} because this is the test, but caught System.DivideByZeroException : TEST-ERROR");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task ShouldThrowExceptionAsync_ThrownExpected_ExceptionReturned()
        {
            //-- arrange

            var thrownException = new ATestException("TEST-ERROR");

            Func<Task> action = async () => {
                await Task.Delay(1);
                throw thrownException;
            };

            //-- act

            var returnedException = await action.ShouldThrowExceptionAsync<ATestException>(because: "this is the test");

            //-- assert

            returnedException.Should().BeSameAs(thrownException);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldThrowExceptionAsync_NoExceptionThrown_AssertFailed()
        {
            //-- arrange

            Func<Task> action = async () => {
                await Task.CompletedTask;
            };

            Action underTest = () => {
                action.ShouldThrowExceptionAsync<ATestException>(because: "this is the test")
                    .Wait(10000).Should().BeTrue(because: "async action must complete");
            };

            //-- act & assert

            underTest.ShouldThrow<XunitException>().Which.Message.Should().Contain(
                $"Expected {typeof(ATestException).FullName} because this is the test, but no exception was thrown.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldThrowExceptionAsync_UnexpectedExceptionThrown_AssertFailed()
        {
            //-- arrange

            Func<Task> action = async () => {
                await Task.Delay(1);
                throw new DivideByZeroException("TEST-ERROR");
            };

            Action underTest = () => {
                action.ShouldThrowExceptionAsync<ATestException>(because: "this is the test")
                    .Wait(10000).Should().BeTrue(because: "async action must complete");
            };

            //-- act & assert

            underTest.ShouldThrow<XunitException>().Which.Message.Should().Contain(
                $"Expected {typeof(ATestException).FullName} because this is the test, but caught System.DivideByZeroException : TEST-ERROR");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldNotThrowExceptionAsync_NoExceptionThrown_AssertPassed()
        {
            //-- arrange

            Func<Task> action = async () => {
                await Task.CompletedTask;
            };

            Action underTest = () => {
                action.ShouldNotThrowExceptionAsync<ATestException>(because: "this is the test")
                    .Wait(10000).Should().BeTrue(because: "async action must complete");
            };

            //-- act & assert

            underTest.ShouldNotThrow();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldNotThrowExceptionAsync_ForbiddenExceptionThrown_AssertFailed()
        {
            //-- arrange

            Func<Task> action = async () => {
                await Task.CompletedTask;
                throw new ATestException("FORBIDDEN-ERROR");
            };

            Action underTest = () => {
                action.ShouldNotThrowExceptionAsync<ATestException>(because: "this is the test")
                    .Wait(10000).Should().BeTrue(because: "async action must complete");
            };

            //-- act & assert

            underTest.ShouldThrow<XunitException>().Which.Message.Should().Contain(
                $"Expected no {typeof(ATestException).FullName} because this is the test, but caught {typeof(ATestException).FullName} : FORBIDDEN-ERROR");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldNotThrowAnyExceptionAsync_NoExceptionThrown_AssertPassed()
        {
            //-- arrange

            Func<Task> action = async () => {
                await Task.CompletedTask;
            };

            Action underTest = () => {
                action.ShouldNotThrowAnyExceptionAsync(because: "this is the test")
                    .Wait(10000).Should().BeTrue(because: "async action must complete");
            };

            //-- act & assert

            underTest.ShouldNotThrow();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ShouldNotThrowAnyExceptionAsync_ExceptionThrown_AssertFailed()
        {
            //-- arrange

            Func<Task> action = async () => {
                await Task.CompletedTask;
                throw new DivideByZeroException("TEST-ERROR");
            };

            Action underTest = () => {
                action.ShouldNotThrowAnyExceptionAsync(because: "this is the test")
                    .Wait(10000).Should().BeTrue(because: "async action must complete");
            };

            //-- act & assert

            underTest.ShouldThrow<XunitException>().Which.Message.Should().Contain(
                $"Expected no exceptions because this is the test, but caught System.DivideByZeroException : TEST-ERROR");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
