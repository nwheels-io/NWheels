using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    public class SessionNotFoundException : Exception
    {
        public SessionNotFoundException(string message)
            : base(message)
        {
        }
    }
}
