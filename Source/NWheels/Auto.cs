using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions;

namespace NWheels
{
    public sealed class Auto<TService> 
        where TService : class
    {
        private readonly TService _instance;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Auto(IEnumerable<IAutoObjectFactory> autoFactories)
        {
            var factory = autoFactories.Single(f => f.ServiceAncestorMarkerType.IsAssignableFrom(typeof(TService)));
            _instance = factory.CreateService<TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService Instance
        {
            get { return _instance; }
        }
    }
}
