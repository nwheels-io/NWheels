using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class TaskExtensions
    {
        public static void WaitOrThrow(this Task task, TimeSpan timeout, CancellationToken? cancellation = null)
        {
            WaitOrThrow(task, (int)timeout.TotalMilliseconds, cancellation);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void WaitOrThrow(this Task task, int millisecondsTimeout, CancellationToken? cancellation = null)
        {
            if (!task.Wait(millisecondsTimeout, cancellation.GetValueOrDefault(CancellationToken.None)))
            {
                throw new TimeoutException("Timed out waiting for a Task to complete.");
            }
        }
    }
}
