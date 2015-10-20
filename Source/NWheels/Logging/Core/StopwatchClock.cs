using System.Diagnostics;
using NWheels.Logging.Impl;

namespace NWheels.Logging.Core
{
    internal class StopwatchClock : IClock
    {
        private readonly Stopwatch _watch = Stopwatch.StartNew();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long ElapsedMilliseconds
        {
            get
            {
                return _watch.ElapsedMilliseconds;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long ElapsedMicroseconds
        {
            get
            {
                return (long)(_watch.Elapsed.TotalMilliseconds * 1000.0d);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ulong ThreadCpuCycles
        {
            get
            {
                return ThreadCpuTimeUtility.GetThreadCycles();
            }
        }
    }
}
