using NWheels.Injection.Mechanism;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NWheels.Injection.Policy
{
    public class FluentComponentRegistration<T>
    {
        private readonly ComponentRegistration _registration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration(ComponentRegistration registration)
        {
            _registration = registration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> AsSelf()
        {
            _registration.ServiceTypes.Add(typeof(T));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> As<S>()
        {
            _registration.ServiceTypes.Add(typeof(S));
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> As<S1, S2>()
        {
            _registration.ServiceTypes.Add(typeof(S1));
            _registration.ServiceTypes.Add(typeof(S2));

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> As<S1, S2, S3>()
        {
            _registration.ServiceTypes.Add(typeof(S1));
            _registration.ServiceTypes.Add(typeof(S2));
            _registration.ServiceTypes.Add(typeof(S3));

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> As(params Type[] serviceTypes)
        {
            _registration.ServiceTypes.UnionWith(serviceTypes);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> WithKey(string key)
        {
            _registration.Key = key;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> WithConstructor(System.Linq.Expressions.Expression<Func<T>> constructorCall)
        {
            throw new NotImplementedException();
            //return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> WithMetadata(string key, object value)
        {
            if (_registration.Metadata == null)
            {
                _registration.Metadata = new Dictionary<string, object>();
            }

            _registration.Metadata[key] = value;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> UseInstancingStrategy<TStrategy>() where TStrategy : IComponentInstancingStrategy
        {
            _registration.InstancingStrategyType = typeof(TStrategy);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> UsePrecedenceStrategy<TStrategy>() where TStrategy : IComponentPrecedenceStrategy
        {
            _registration.PrecedenceStrategyType = typeof(TStrategy);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public FluentComponentRegistration<T> UsePipeStrategy<TStrategy>() where TStrategy : IComponentPipeStrategy
        {
            _registration.PipeStrategyType = typeof(TStrategy);
            return this;
        }
    }
}
