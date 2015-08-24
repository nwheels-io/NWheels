using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;

namespace NWheels.TypeModel.Serialization
{
    public class BinarySerializer : IObjectSerializer
    {
        #region Implementation of IObjectSerializer

        public object Deserialize(IMessageObject message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
