using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;
using FluentAssertions;
using NWheels.Kernel.Api.Extensions;
using NWheels.Testability;
using NWheels.Kernel.Api.Execution;

namespace NWheels.Kernel.UnitTests.Api.Extensions
{
    public class ExecutionPathActivityExtensionsTests : TestBase.UnitTest
    {
        [Fact]
        public void RunActivityOrThrow_VoidNoArguments_Success()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            var action = new Action(() => {
                activity.Log.Add("ACTION");
            });

            //-- act

            activity.RunActivityOrThrow(action);

            //-- assert

            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_VoidNoArguments_Failure()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            var exception = new MockException();

            var action = new Action(() => {
                activity.Log.Add("ACTION");
                throw exception;
            });

            Action act = () => activity.RunActivityOrThrow(action);

            //-- act

            act.ShouldThrow<MockException>().Which.Should().BeSameAs(exception);

            //-- assert

            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "FAIL-ERROR", "DISPOSE" });
            activity.FailError.Should().BeSameAs(exception);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_VoidWithArgument_Success()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            IExecutionPathActivity activityArgument = null;

            Action<IExecutionPathActivity> action = (x) => {
                activityArgument = x;
                activity.Log.Add("ACTION");
            };

            //-- act

            activity.RunActivityOrThrow(action);

            //-- assert

            activityArgument.Should().BeSameAs(activity);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_VoidWithArgument_Failure()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            Exception exception = new MockException();

            Action<IExecutionPathActivity> action = (x) => {
                activity.Log.Add("ACTION");
                throw exception;
            };

            Action act = () => {
                activity.RunActivityOrThrow(action);
            };

            //-- act

            act.ShouldThrow<MockException>().Which.Should().BeSameAs(exception);

            //-- assert

            activity.FailError.Should().BeSameAs(exception);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "FAIL-ERROR", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_FuncNoArguments_Success()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            Func<int> func = () => {
                activity.Log.Add("ACTION");
                return 123;
            };

            //-- act

            var returnValue = activity.RunActivityOrThrow(func);

            //-- assert

            returnValue.Should().Be(123);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_FuncNoArguments_Failure()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            var exception = new MockException();

            Func<int> func = () => {
                activity.Log.Add("ACTION");
                throw exception;
            };

            Action act = () => {
                activity.RunActivityOrThrow(func);
            };

            //-- act

            act.ShouldThrow<MockException>().Which.Should().BeSameAs(exception);

            //-- assert

            activity.FailError.Should().BeSameAs(exception);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "FAIL-ERROR", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_FuncWithArgument_Success()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            IExecutionPathActivity activityArgument = null;

            Func<IExecutionPathActivity, int> func = (x) => {
                activityArgument = x;
                activity.Log.Add("ACTION");
                return 123;
            };

            //-- act

            var returnValue = activity.RunActivityOrThrow(func);

            //-- assert

            returnValue.Should().Be(123);
            activityArgument.Should().BeSameAs(activity);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrThrow_FuncWithArgument_Failure()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            var exception = new MockException();

            Func<IExecutionPathActivity, int> func = (x) => {
                activity.Log.Add("ACTION");
                throw exception;
            };

            Action act = () => {
                activity.RunActivityOrThrow(func);
            };

            //-- act

            act.ShouldThrow<MockException>().Which.Should().BeSameAs(exception);

            //-- assert

            activity.FailError.Should().BeSameAs(exception);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "FAIL-ERROR", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrCatch_NoArguments_Success_ReturnTrue()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            Action action = () => {
                activity.Log.Add("ACTION");
            };

            //-- act

            var success = activity.RunActivityOrCatch(action, out Exception error);

            //-- assert

            success.Should().BeTrue();
            error.Should().BeNull();
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrCatch_NoArguments_Failure_ReturnFalseAndException()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            var exception = new MockException();

            Action action = () => {
                activity.Log.Add("ACTION");
                throw exception;
            };

            //-- act

            var success = activity.RunActivityOrCatch(action, out Exception error);

            //-- assert

            success.Should().BeFalse();
            error.Should().BeSameAs(exception);
            activity.FailError.Should().BeSameAs(exception);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "FAIL-ERROR", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrCatch_WithArgument_Success_ReturnTrue()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            IExecutionPathActivity activityArgument = null;

            Action<IExecutionPathActivity> action = (x) => {
                activityArgument = x;
                activity.Log.Add("ACTION");
            };

            //-- act

            var success = activity.RunActivityOrCatch(action, out Exception error);

            //-- assert

            success.Should().BeTrue();
            error.Should().BeNull();
            activityArgument.Should().BeSameAs(activity);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunActivityOrCatch_WithArgument_Failure_ReturnFalseAndException()
        {
            //-- arrange

            var activity = new MockActivity("A1");
            var exception = new MockException();

            Action<IExecutionPathActivity> action = (x) => {
                activity.Log.Add("ACTION");
                throw exception;
            };

            //-- act

            var success = activity.RunActivityOrCatch(action, out Exception error);

            //-- assert

            success.Should().BeFalse();
            error.Should().BeSameAs(exception);
            activity.FailError.Should().BeSameAs(exception);
            activity.Log.Should().Equal(new[] { "CTOR", "ACTION", "FAIL-ERROR", "DISPOSE" });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MockActivity : IExecutionPathActivity
        {
            public MockActivity(string text)
            {
                this.Log = new List<string>();
                this.Text = text;

                Log.Add("CTOR");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                Log.Add("DISPOSE");
                IsDisposed = true;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Fail(Exception error)
            {
                Log.Add("FAIL-ERROR");
                FailError = error;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Fail(string reason)
            {
                Log.Add("FAIL-REASON");
                FailReason = reason;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<string> Log { get; }
            public string Text { get; }
            public bool IsDisposed { get; private set; }
            public Exception FailError { get; private set; }
            public string FailReason { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MockException : Exception
        {
            public MockException() : base("MOCK-ERROR")
            {
            }
        }
    }
}
