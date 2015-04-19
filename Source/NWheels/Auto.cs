using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using NWheels.Conventions;
using NWheels.Extensions;

namespace NWheels
{
    public sealed class Auto<TService>
        where TService : class
    {
        private readonly TService _instance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Auto(IComponentContext container)
        {
            if ( container.ComponentRegistry.RegistrationsFor(new TypedService(typeof(TService))).Any() )
            {
                _instance = container.Resolve<TService>();
            }
            else
            {
                var autoFactories = container.Resolve<IEnumerable<IAutoObjectFactory>>();
                var factory = autoFactories.Single(f => f.ServiceAncestorMarkerType.IsAssignableFrom(typeof(TService)));
                _instance = factory.CreateService<TService>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal Auto(TService instance)
        {
            _instance = instance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService Instance
        {
            get { return _instance; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static implicit operator Auto<TService>(TService service)
        {
            return new Auto<TService>(service);
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class Auto
    {
        public static Auto<TService> Of<TService>(TService service) where TService : class
        {
            return new Auto<TService>(service);
        }
    }
}
