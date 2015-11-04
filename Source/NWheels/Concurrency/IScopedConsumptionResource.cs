using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public interface IScopedConsumptionResource
    {
        void ActiveScopeChanged(bool currentScopeIsActive);
    }
}
