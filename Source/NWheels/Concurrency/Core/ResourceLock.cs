using System;

namespace NWheels.Concurrency.Core
{
    internal class ResourceLock : IResourceLock
    {
        private readonly ResourceLockMode _mode;
        private readonly string _resourceName;
        //private int _waitQueueLength = 0;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ResourceLock(ResourceLockMode mode, string resourceName)
        {
            if (mode != ResourceLockMode.Exclusive)
            {
                throw new NotSupportedException("ResourceLockMode is not yet supported: " + mode);
            }

            _resourceName = resourceName;
            _mode = mode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireReadAccess(string forWhat, int holdDurationMs)
        {
            return AcquireReadAccess(forWhat, holdDurationMs, waitTimeoutMs: 10000);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireReadAccess(string forWhat, int holdDurationMs, int waitTimeoutMs)
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable AcquireWriteAccess(string forWhat, int holdDurationMs)
        {
            return AcquireWriteAccess(forWhat, holdDurationMs, waitTimeoutMs: 10000);
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
