using NWheels.Concurrency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Core.Concurrency
{
    internal class ResourceLock : IResourceLock
    {
        private readonly ResourceLockMode _mode;
        private readonly string _resourceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ResourceLock(ResourceLockMode mode, string resourceName)
        {
            _resourceName = resourceName;
            _mode = mode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireReadAccess(string forWhat, int holdDurationMs)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireReadAccess(string forWhat, int holdDurationMs, int waitTimeoutMs)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireWriteAccess(string forWhat, int holdDurationMs)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireWriteAccess(string forWhat, int holdDurationMs, int waitTimeoutMs)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryAcquireReadAccess(string forWhat, int holdDurationMs, out IDisposable access)
        {
            access = null;
            return true;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryAcquireReadAccess(string forWhat, int holdDurationMs, int waitTimeoutMs, out IDisposable access)
        {
            access = null;
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryAcquireWriteAccess(string forWhat, int holdDurationMs, out IDisposable access)
        {
            access = null;
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryAcquireWriteAccess(string forWhat, int holdDurationMs, int waitTimeoutMs, out IDisposable access)
        {
            access = null;
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResourceAccessHolderInfo[] GetCurrentAccessHolders()
        {
            return new IResourceAccessHolderInfo[0];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReportHoldDurationMs(out int max, out int average)
        {
            max = 0;
            average = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReportWaitDurationMs(out int max, out int average)
        {
            max = 0;
            average = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ReportWaitQueueLength(out int current, out int max, out int average)
        {
            current = 0;
            max = 0;
            average = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ResourceName
        {
            get { return _resourceName; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ResourceLockMode Mode
        {
            get { return _mode; }
        }
    }
}
