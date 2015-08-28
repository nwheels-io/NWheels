using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages.Impl
{
    public class SessionPushActor : IMessageHandler<AbstractSessionPushMessage>
    {
        #region Implementation of IMessageHandler<AbstractSessionPushMessage>

        public void HandleMessage(AbstractSessionPushMessage message)
        {
            message.ToSession.Endpoint.PushMessage(message.ToSession, message);
        }

        #endregion
    }
}
