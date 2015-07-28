using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public interface IMessageHeader
    {
        string Name { get; }
        string Values { get; }
    }
}
