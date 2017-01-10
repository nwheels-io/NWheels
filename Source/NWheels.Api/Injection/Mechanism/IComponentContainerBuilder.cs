using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public interface IComponentContainerBuilder
    {
        void AddRegistration(ComponentRegistration registration);
    }
}
