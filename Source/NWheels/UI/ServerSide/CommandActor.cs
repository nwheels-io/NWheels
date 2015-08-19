using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;

namespace NWheels.UI.ServerSide
{
    public class CommandActor :
        IMessageHandler<EntityChangeSetMessage>,
        IMessageHandler<EntityCommandMessage>,
        IMessageHandler<TransactionScriptCommandMessage>
    {
        #region Implementation of IMessageHandler<EntityChangeSetMessage>

        public void HandleMessage(EntityChangeSetMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<EntityCommandMessage>

        public void HandleMessage(EntityCommandMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageHandler<TransactionScriptCommandMessage>

        public void HandleMessage(TransactionScriptCommandMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
