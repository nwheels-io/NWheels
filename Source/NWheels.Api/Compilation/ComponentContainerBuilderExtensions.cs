using NWheels.Compilation.Mechanism.Factories;
using NWheels.Injection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Compilation
{
    public static class ComponentContainerBuilderExtensions
    {
        public static void ContributeTypeFactory<TTypeFactory, TObjectFactory>(this IComponentContainerBuilder containerBuilder)
            where TTypeFactory : class, TObjectFactory, ITypeFactory
        {
            containerBuilder.Register<TObjectFactory, TTypeFactory>(LifeStyle.Singleton);
        }
    }
}
