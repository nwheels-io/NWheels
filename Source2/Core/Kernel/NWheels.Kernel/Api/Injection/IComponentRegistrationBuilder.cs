using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Api.Injection
{
    public interface IComponentRegistrationBuilder
    {
        IComponentConditionBuilder ForService<TService>();
        IComponentConditionBuilder ForServices<TService1, TService2>();
        IComponentConditionBuilder ForServices<TService1, TService2, TService3>();
        IComponentConditionBuilder ForServices(params Type[] serviceTypes);

        IComponentConditionBuilder NamedForService<TService>(string name);
        IComponentConditionBuilder NamedForServices<TService1, TService2>(string name);
        IComponentConditionBuilder NamedForServices<TService1, TService2, TService3>(string name);
        IComponentConditionBuilder NamedForServices(string name, params Type[] serviceTypes);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IComponentInstantiationBuilder : IComponentRegistrationBuilder
    {
        IComponentRegistrationBuilder SingleInstance();
        IComponentRegistrationBuilder InstancePerDependency();
        IComponentInstantiationBuilder WithParameter<T>(T value);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IComponentConditionBuilder : IComponentRegistrationBuilder
    {
        void AsFallback();
    }
}
