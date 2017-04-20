using System;

namespace NWheels.Injection
{
    public interface IComponentContainerBuilder
    {
        //TODO: refactor the following APIs to be more self-documenting
        //for example: 
        //  RegisterComponent<...>().ForServices<...>()

        IComponentInstantiationBuilder RegisterComponent2<TComponent>();
        IComponentRegistrationBuilder RegisterInstance2<TComponent>(TComponent componentInstance)
            where TComponent : class;

        IComponentInstantiationBuilder Register<TService, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService;

        IComponentInstantiationBuilder Register<TService1, TService2, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2;

        IComponentInstantiationBuilder Register<TService1, TService2, TService3, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2, TService3;

        IComponentInstantiationBuilder Register<TService>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        IComponentInstantiationBuilder Register<TService1, TService2>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        IComponentInstantiationBuilder Register<TService1, TService2, TService3>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        void RegisterInstance<TService>(object componentInstance);

        void RegisterInstance<TService1, TService2>(object componentInstance);

        void RegisterInstance<TService1, TService2, TService3>(object componentInstance);

    }
}
