using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Sensors;
using NWheels.Microservices;

namespace ElectricityBilling.SensorService.HubIntegrationStub
{
    [DefaultFeatureLoader]
    public class StubSensorHubFeature : FeatureLoaderBase
    {
        public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            newComponents.RegisterComponentType<StubSensorHubService>().InstancePerDependency().ForService<ISensorHubService>();
        }
    }
}
