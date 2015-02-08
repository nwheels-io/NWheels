using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public interface IResourceLock
    {
        IDisposable AcquireReadAccess(string forWhat, int holdDurationMs);
        IDisposable AcquireReadAccess(string forWhat, int holdDurationMs, int waitTimeoutMs);
        IDisposable AcquireWriteAccess(string forWhat, int holdDurationMs);
        IDisposable AcquireWriteAccess(string forWhat, int holdDurationMs, int waitTimeoutMs);
        bool TryAcquireReadAccess(string forWhat, int holdDurationMs, out IDisposable access);
        bool TryAcquireReadAccess(string forWhat, int holdDurationMs, int waitTimeoutMs, out IDisposable access);
        bool TryAcquireWriteAccess(string forWhat, int holdDurationMs, out IDisposable access);
        bool TryAcquireWriteAccess(string forWhat, int holdDurationMs, int waitTimeoutMs, out IDisposable access);
        IResourceAccessHolderInfo[] GetCurrentAccessHolders();
        void ReportHoldDurationMs(out int max, out int average);
        void ReportWaitDurationMs(out int max, out int average);
        void ReportWaitQueueLength(out int current, out int max, out int average);
        string ResourceName { get; }
        ResourceLockMode Mode { get; }
    }
}
