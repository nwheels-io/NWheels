using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Execution
{
    public interface IInvocationScheduler
    {
        IInvocationChannel GetInvocationChannel(string[] invocationTraits, string[] processorTraits);
    }
}
