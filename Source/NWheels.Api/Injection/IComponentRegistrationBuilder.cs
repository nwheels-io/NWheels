using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection
{
    public interface IComponentRegistrationBuilder
    {
        IComponentRegistrationBuilder ForService<TService>();
        IComponentRegistrationBuilder ForServices<TService1, TService2>();
        IComponentRegistrationBuilder ForServices<TService1, TService2, TService3>();
        IComponentRegistrationBuilder ForServices(params Type[] serviceTypes);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IComponentInstantiationBuilder : IComponentRegistrationBuilder
    {
        IComponentRegistrationBuilder SingleInstance();
        IComponentRegistrationBuilder InstancePerDependency();
        IComponentInstantiationBuilder WithParameter<T>(T value);
    }
}
