using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;

namespace NWheels.Stacks.ODataBreeze
{
    public static class JsonSerializationUtility
    {
        public static DataRepositoryBase GetCurrentDomainContext()
        {
            var domainContext = RuntimeEntityModelHelpers.CurrentDomainContext;

            if ( domainContext == null )
            {
                throw new InvalidOperationException("No domain context on current thread.");
            }

            return domainContext;
        }
    }
}
