using System.Collections.Generic;

namespace NWheels.Microservices
{
    public abstract class ModuleLoaderBase
    {
        public abstract List<IFeatureLoader> LoadAllFeatures();
    }
}
