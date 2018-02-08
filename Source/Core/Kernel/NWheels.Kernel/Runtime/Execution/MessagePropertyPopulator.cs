using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Runtime.Execution
{
    public delegate bool MessagePropertyPopulator<TInput, TTarget>(TInput input, ref TTarget target);
}
