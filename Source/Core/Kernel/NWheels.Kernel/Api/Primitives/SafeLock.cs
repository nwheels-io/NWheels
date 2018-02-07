using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.Api.Primitives
{
    public class SafeLock
    {
        private readonly SyncRoot _syncRoot = new SyncRoot();
        private readonly string _resourceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SafeLock(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName), $"{nameof(resourceName)} must be a non-empty string.");
            }

            _resourceName = resourceName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDisposable Acquire(TimeSpan timeout, string purpose)
        {
            if (string.IsNullOrEmpty(purpose))
            {
                throw new ArgumentNullException(nameof(purpose), $"{nameof(purpose)} must be a non-empty string.");
            }

            if (!_syncRoot.TryLock(timeout))
            {
                throw SafeLockException.TimedOutWaitingForAccess(_resourceName, timeout);
            }

            return _syncRoot;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SyncRoot : IDisposable
        {
            public bool TryLock(TimeSpan timeout)
            {
                return Monitor.TryEnter(this, timeout);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                Monitor.Exit(this);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: add lock holder info
        //private class LockHolderInfo
        //{
        //    public LockHolder

        //    public void Dispose()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}
