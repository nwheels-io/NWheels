using NWheels.Injection.Mechanism;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Policy
{
    public static class ComponentContainerBuilderExtensions
    {
        public static FluentComponentContribution Contribute(this IComponentContainerBuilder builder)
        {
            return new FluentComponentContribution(builder);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<object> RegisterType(this IComponentContainerBuilder builder, Type componentType)
        {
            var registration = new ComponentRegistration() {
                Type = componentType
            };

            builder.AddRegistration(registration);
            return new FluentComponentRegistration<object>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> RegisterType<T>(this IComponentContainerBuilder builder)
            where T : class
        {
            var registration = new ComponentRegistration() {
                Type = typeof(T)
            };

            builder.AddRegistration(registration);
            return new FluentComponentRegistration<T>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> RegisterInstance<T>(this IComponentContainerBuilder builder, T instance)
            where T : class
        {
            var registration = new ComponentRegistration() {
                Instance = instance
            };

            builder.AddRegistration(registration);
            return new FluentComponentRegistration<T>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> RegisterFactory<T>(this IComponentContainerBuilder builder, Func<IComponentContainer, T> factory)
            where T : class
        {
            var registration = new ComponentRegistration() {
                InstanceFactory = factory
            };

            builder.AddRegistration(registration);
            return new FluentComponentRegistration<T>(registration);
        }
    }
}
