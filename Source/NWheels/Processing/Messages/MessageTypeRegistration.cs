using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public class MessageTypeRegistration
    {
        public MessageTypeRegistration(Type bodyType)
        {
            BodyType = bodyType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type BodyType { get; private set; }
    }
}
