using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices
{
    public interface ILifecycleListenerComponent
    {
        void MicroserviceLoading();
        void Load();
        void MicroserviceLoaded();
    }
}
