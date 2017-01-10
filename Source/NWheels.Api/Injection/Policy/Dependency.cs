using NWheels.Injection.Mechanism;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Policy
{
    public sealed class Dependency<T> 
        where T : class
    {
        private readonly IComponentContainer _container;
        private T _resolved;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Dependency(IComponentContainer container)
        {
            _container = container;
            _resolved = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Dependency(T resolved)
        {
            _container = null;
            _resolved = resolved;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public T Resolve()
        {
            if (_resolved == null)
            {
                _resolved = _container.Resolve<T>();
            }

            return _resolved;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator Dependency<T>(T resolved)
        {
            return new Dependency<T>(resolved);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator T(Dependency<T> dependency)
        {
            return dependency.Resolve();
        }
    }
}
