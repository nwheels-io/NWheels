using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Api.IoC
{
    public interface IComponentLifecycleListener
    {
        void Constructed();
        void Acquired();
        void MightBeReleased();
        void MightBeDestructing();
    }
}
