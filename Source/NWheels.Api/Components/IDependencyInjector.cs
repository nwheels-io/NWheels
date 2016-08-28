using System;
using System.Collections.Generic;

namespace NWheels.Api.Components
{
    public interface IDependencyInjector
    {
        T Get<T>();
        T Get<T>(string name);
        IEnumerable<T> GetAll<T>();
        IPipeline<T> GetPipeline<T>();
    }
}