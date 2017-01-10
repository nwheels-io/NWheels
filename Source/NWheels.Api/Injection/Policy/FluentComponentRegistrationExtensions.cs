using NWheels.Injection.Mechanism;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Policy
{
    public static class FluentComponentRegistrationExtensions
    {
        public static FluentComponentRegistration<T> InstancingOptionSingleton<T>(this FluentComponentRegistration<T> registration)
        {
            return registration.UseInstancingStrategy<ISingletonComponentInstancingStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> InstancingOptionTransient<T>(this FluentComponentRegistration<T> registration)
        {
            return registration.UseInstancingStrategy<ITransientComponentInstancingStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> InstancingOptionPerExecutionPath<T>(this FluentComponentRegistration<T> registration)
        {
            return registration.UseInstancingStrategy<IPerExecutionPathComponentInstancingStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PrecedenceOptionFallback<T>(this FluentComponentRegistration<T> registration)
        {
            return registration.UsePrecedenceStrategy<IFallbackComponentPrecedenceStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PrecedenceOptionPrimary<T>(this FluentComponentRegistration<T> registration)
        {
            return registration.UsePrecedenceStrategy<IPrimaryComponentPrecedenceStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PrecedenceOptionNormal<T>(this FluentComponentRegistration<T> registration)
        {
            return registration.UsePrecedenceStrategy<INormalComponentPrecedenceStrategy>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PipeOptionAfter<T>(this FluentComponentRegistration<T> registration)
        {
            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PipeOptionBefore<T>(this FluentComponentRegistration<T> registration)
        {
            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PipeOptionFirst<T>(this FluentComponentRegistration<T> registration)
        {
            return registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static FluentComponentRegistration<T> PipeOptionLast<T>(this FluentComponentRegistration<T> registration)
        {
            return registration;
        }
    }
}
