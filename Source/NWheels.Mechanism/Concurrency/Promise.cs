using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NWheels.Mechanism.Concurrency
{
    public struct Promise<T>
    {
        public PromiseAwaiter<T> GetAwaiter()
        {
            return new PromiseAwaiter<T>();
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------------

    public struct PromiseAwaiter<T> : INotifyCompletion
    {
        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public T GetResult()
        {
            return default(T);
        }

        public bool IsCompleted
        {
            get
            {
                return true;
            }
        }
    }
}
