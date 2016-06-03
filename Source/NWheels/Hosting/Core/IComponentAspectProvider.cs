using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Hosting.Factories;

namespace NWheels.Hosting.Factories
{
}

namespace NWheels.Hosting.Core
{
    public interface IComponentAspectProvider
    {
        IObjectFactoryConvention GetAspectConvention(ComponentAspectFactory.ConventionContext context);
    }
}
