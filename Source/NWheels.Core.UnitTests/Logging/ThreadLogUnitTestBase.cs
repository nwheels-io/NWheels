using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NWheels.Core.Logging;
using NWheels.Testing;

namespace NWheels.Core.UnitTests.Logging
{
    public class ThreadLogUnitTestBase : UnitTestBase
    {
        [SetUp]
        public void ThreadLogSetUp()
        {
            Clock = new TestClock();
            Registry = new TestThreadRegistry();
            Anchor = new TestThreadLogAnchor();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal TestClock Clock { get; private set; }
        internal TestThreadRegistry Registry { get; private set; }
        internal TestThreadLogAnchor Anchor { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class TestClock : IClock
        {
            public long ElapsedMilliseconds { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class TestThreadLogAnchor : IThreadLogAnchor
        {
            public ThreadLog CurrentThreadLog { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal class TestThreadRegistry : IThreadRegistry
        {
            private readonly HashSet<ThreadLog> _runningThreads = new HashSet<ThreadLog>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ThreadStarted(ThreadLog threadLog)
            {
                _runningThreads.Add(threadLog);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void ThreadFinished(ThreadLog threadLog)
            {
                _runningThreads.Remove(threadLog);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ThreadLog[] GetRunningThreads()
            {
                return _runningThreads.ToArray();
            }
        }
    }
}