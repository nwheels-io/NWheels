using System;

namespace NWheels.Injection
{
    public interface IComponentContainerBuilder
    {
        //TODO: refactor the following APIs to be more self-documenting
        //for example: 
        //  RegisterComponent<...>().ForServices<...>()

        void Register<TService, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService;

        void Register<TService1, TService2, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2;

        void Register<TService1, TService2, TService3, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2, TService3;

        void Register<TService>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        void Register<TService1, TService2>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        void Register<TService1, TService2, TService3>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);
    }
}
