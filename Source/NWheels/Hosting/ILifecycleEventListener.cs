using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Hosting
{
    public interface ILifecycleEventListener
    {
        void InjectDependencies(IComponentContext components);
        void NodeConfigured(List<ILifecycleEventListener> additionalComponentsToHost);
        void NodeLoading();
        void Load();
        void NodeLoaded();
        void NodeActivating();
        void Activate();
        void NodeActivated();
        void NodeDeactivating();
        void Deactivate();
        void NodeDeactivated();
        void NodeUnloading();
        void Unload();
        void NodeUnloaded();
        string ComponentName { get; }
    }
}
