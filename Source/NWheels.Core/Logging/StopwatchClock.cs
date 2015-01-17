using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.Logging
{
    internal class StopwatchClock : IClock
    {
        private readonly Stopwatch _watch = Stopwatch.StartNew();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long ElapsedMilliseconds
        {
            get { return _watch.ElapsedMilliseconds; }
        }
    }
}
