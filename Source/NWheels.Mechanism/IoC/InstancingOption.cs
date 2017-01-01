using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Mechanism.IoC
{
    public enum InstancingOption
    {
        Transient,
        SingleInstance,
        InstancePerExecutionPath,
        InstanceFromPool
    }
}
