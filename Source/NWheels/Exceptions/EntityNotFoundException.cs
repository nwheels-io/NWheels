using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message)
            : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityNotFoundException(Type contract, object id)
            : base(string.Format("Entity not found: {0}[{1}]", contract.FullName, id))
        {
        }
    }
}
