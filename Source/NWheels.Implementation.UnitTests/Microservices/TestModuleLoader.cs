using NWheels.Microservices;
using System;
using System.Collections.Generic;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class TestModuleLoader : ModuleLoaderBase
    {
        public override List<IFeatureLoader> LoadModule()
        {
            throw new NotImplementedException();
        }
    }
}
