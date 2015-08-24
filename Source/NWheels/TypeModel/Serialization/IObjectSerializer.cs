using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;

namespace NWheels.TypeModel.Serialization
{
    public interface IObjectSerializer
    {
        object Deserialize(IMessageObject message);
    }
}
