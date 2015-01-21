using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Conventions
{
    public interface IAutoObjectFactory
    {
        TService CreateService<TService>() where TService : class;
        Type ServiceAncestorMarkerType { get; }
    }
}
