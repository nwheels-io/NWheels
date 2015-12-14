using System;
using Autofac;
using NWheels.DataObjects.Core;

namespace NWheels.Conventions.Core
{
    public interface IEntityObjectFactory
    {
        TEntityContract NewEntity<TEntityContract>() where TEntityContract : class;
        TEntityContract NewEntity<TEntityContract>(IComponentContext externalComponents) where TEntityContract : class;
        object NewEntity(Type entityContractType);
    }
}