using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions.Core;
using NWheels.UI.Factories;

namespace NWheels.UI
{
    public abstract class DomainApiBase
    {
        private readonly IViewModelObjectFactory _objectFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected DomainApiBase(IViewModelObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TContract NewModel<TContract>() where TContract : class
        {
            return _objectFactory.NewEntity<TContract>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected TContract NewModel<TContract>(params Action<TContract>[] initializers) where TContract : class
        {
            var model = _objectFactory.NewEntity<TContract>();

            foreach ( var initializer in initializers )
            {
                initializer(model);
            }

            return model;
        }
    }
}
