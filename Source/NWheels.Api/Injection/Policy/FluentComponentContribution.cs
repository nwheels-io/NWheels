using NWheels.Injection.Mechanism;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Policy
{
    public class FluentComponentContribution
    {
        public FluentComponentContribution(IComponentContainerBuilder containerBuilder)
        {
            ContainerBuilder = containerBuilder;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContainerBuilder ContainerBuilder { get; }
    }
}
