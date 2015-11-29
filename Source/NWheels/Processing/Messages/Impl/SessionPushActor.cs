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
            if ( message.ToSession.Endpoint != null && message.ToSession.Endpoint.IsPushSupprted )
            {
                message.ToSession.Endpoint.PushMessage(message.ToSession, message);
            }
        }

        #endregion
    }
}
