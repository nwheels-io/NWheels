using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices
{
    public interface IMicroserviceLifecycleListener
    {
        void Loading();
        void Load();
        void Loaded();
        void Activating();
        void Activate();
        void Activated();
        void MightBeDeactivating();
        void MightDeactivate();
        void MightBeDeactivated();
        void MightBeUnloading();
        void MightUnload();
        void MightBeUnloaded();
        string IdName { get; }
    }
}
