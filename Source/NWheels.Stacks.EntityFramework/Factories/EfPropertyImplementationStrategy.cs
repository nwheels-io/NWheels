using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects.Core.Factories;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class EfPropertyImplementationStrategy : PropertyImplementationStrategyDecorator
    {
        public EfPropertyImplementationStrategy(IPropertyImplementationStrategy implementor, PropertyConfigurationStrategy configurator)
            : base(implementor)
        {
            this.Configurator = configurator;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return string.Format("{0} {{ {1}, {2} }}", this.GetType().Name, Target.GetType().Name, Configurator.GetType().Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyConfigurationStrategy Configurator { get; private set; }
    }
}
