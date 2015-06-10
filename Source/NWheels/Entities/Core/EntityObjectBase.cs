using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace NWheels.Entities.Core
{
    public abstract class EntityObjectBase
    {
        private IComponentContext _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected EntityObjectBase()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected EntityObjectBase(IComponentContext components)
        {
            _components = components;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContext InjectedComponents
        {
            set
            {
                _components = value;
            }
        }
    }
}
