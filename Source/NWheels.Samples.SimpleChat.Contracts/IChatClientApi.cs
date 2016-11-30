using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints;
using NWheels.Logging;

namespace NWheels.Samples.SimpleChat.Contracts
{
    public interface IChatClientApi
    {
        void SomeoneSaidSomething(string who, string what);
    }
}
