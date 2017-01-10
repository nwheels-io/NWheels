using System;
using Autofac;
using Autofac.Core;

namespace NWheels.Stacks.IoC.Autofac
{
    public class Class1
    {
        void F(ContainerBuilder builder)
        {
            builder.RegisterType<Class1>().WithParameter(TypedParameter.From(1212));
        }
    }
}
