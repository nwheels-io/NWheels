using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection
{
    public interface IComponentRegistrationBuilder
    {
        IComponentRegistrationBuilder As<TService>();
        IComponentRegistrationBuilder As<TService1, TService2>();
        IComponentRegistrationBuilder As<TService1, TService2, TService3>();
        IComponentRegistrationBuilder As(params Type[] serviceTypes);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IComponentInstantiationBuilder : IComponentRegistrationBuilder
    {
        IComponentRegistrationBuilder SingleInstance();
        IComponentRegistrationBuilder InstancePerDependency();
        IComponentInstantiationBuilder WithParameter<T>(T value);
    }
}
