using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NWheels.Testability
{
    public class SynchronizationContextScope : IDisposable
    {
        public SynchronizationContextScope(SynchronizationContext newContext)
        {
            this.OriginalContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(newContext);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(this.OriginalContext);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SynchronizationContext OriginalContext { get; }
    }
}
