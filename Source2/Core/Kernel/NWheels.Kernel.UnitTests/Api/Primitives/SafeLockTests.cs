using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NWheels.Kernel.Api.Exceptions;
using NWheels.Kernel.Api.Primitives;
using NWheels.Testability;
using Xunit;

namespace NWheels.Kernel.UnitTests.Api.Primitives
{
    public class SafeLockTests : TestBase.UnitTest
    {
        [Fact]
        public void Acquire_ExclusiveAccess()
        {
            //-- arrange

            var lockUnderTest = new SafeLock("TestResource");
            var clock = Stopwatch.StartNew();
            var canStartThread2 = new ManualResetEvent(false);

            TimeSpan thread1ReleaseTimestamp;
            TimeSpan thread2AcquireTimestamp;

            //-- act

            var thread1 = Task.Factory.StartNew(() => {
                using (lockUnderTest.Acquire(TimeSpan.FromMinutes(1), "Purpose1"))
                {
                    canStartThread2.Set();
                    Thread.Sleep(200);
                    thread1ReleaseTimestamp = clock.Elapsed;
                }
            });

            canStartThread2.WaitOne(10000).Should().BeTrue("timed out waiting for signal from thread 1");

            var thread2 = Task.Factory.StartNew(() => {
                using (lockUnderTest.Acquire(TimeSpan.FromMinutes(1), "Purpose2"))
                {
                    thread2AcquireTimestamp = clock.Elapsed;
                }
            });

            Task.WaitAll(new[] { thread1, thread2 }, 10000).Should().BeTrue("timed out waiting for threads to finish");
            clock.Stop();
            canStartThread2.Dispose();

            //-- assert

            thread1ReleaseTimestamp.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(200));
            thread2AcquireTimestamp.Should().BeGreaterOrEqualTo(thread1ReleaseTimestamp);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Acquire_TimedOut_Throws()
        {
            //-- arrange

            var clock = new Stopwatch();
            var lockUnderTest = new SafeLock("TestResource");

            lockUnderTest.Acquire(TimeSpan.FromSeconds(1), "Purpose1");

            Action doubleAcquireAttempt = () => {
                clock.Start();
                try
                {
                    lockUnderTest.Acquire(TimeSpan.FromMilliseconds(100), "Purpose2");
                }
                finally
                {
                    clock.Stop();
                }
            };

            SafeLockException exception = null;

            //-- act

            var thread1 = Task.Factory.StartNew(() => {
                exception = doubleAcquireAttempt
                    .ShouldThrow<SafeLockException>()
                    .Which;
            });

            thread1.Wait(10000).Should().BeTrue(because: "the thread must finish in timely manner");

            //-- assert

            thread1.Exception.Should().BeNull(because: "ShouldThrow<SafeLockException> must successfully validate");
            clock.Elapsed.Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(100), because: "Acquire() must wait for the specified timeout of 100ms");

            exception.Should().NotBeNull();
            exception.Reason.Should().Be(nameof(SafeLockException.TimedOutWaitingForAccess));
            exception.ResourceName.Should().Be("TestResource");
            exception.Timeout.Should().Be(TimeSpan.FromMilliseconds(100));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Constructor_ResourceNameNullOrEmpty_Throw(string resourceName)
        {
            //-- arrange

            Action act = () => {
                new SafeLock(resourceName);
            };

            //-- act & assert

            act.ShouldThrow<ArgumentNullException>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Acquire_PurposeNullOrEmpty_Throw(string purpose)
        {
            //-- arrange

            var lockUnderTest = new SafeLock("TestResource");

            Action act = () => {
                lockUnderTest.Acquire(TimeSpan.FromSeconds(1), purpose);
            };

            //-- act & assert

            act.ShouldThrow<ArgumentNullException>();
        }
    }
}
