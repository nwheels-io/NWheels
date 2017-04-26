using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection
{
    public interface IComponentRegistrationBuilder
    {
        IComponentConditionBuilder ForService<TService>();
        IComponentConditionBuilder ForServices<TService1, TService2>();
        IComponentConditionBuilder ForServices<TService1, TService2, TService3>();
        IComponentConditionBuilder ForServices(params Type[] serviceTypes);
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
