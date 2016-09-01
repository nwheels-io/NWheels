using System;
using System.Runtime.CompilerServices;

namespace NWheels.Concurrency.Advanced
{
    public struct PromiseAwaiter<T> : ICriticalNotifyCompletion
    {
        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public T GetResult()
        {
            throw new NotImplementedException();
        }

        public bool IsCompleted
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}