using System;
using NWheels.Composition.Model;
using NWheels.DevOps.Model;

namespace NWheels.DevOps.Adapters.Microservices.DotNet
{
    public static class DotNetMicroserviceTechnologyAdapter
    {
        public static DotNetMicroservice AsDotNetMicroservice<TService>(
            this TService microservice,
            params Func<TechnologyAdaptedComponent>[] children)
            where TService : IMicroservice
        {
            return new DotNetMicroservice();
        }
    }
}