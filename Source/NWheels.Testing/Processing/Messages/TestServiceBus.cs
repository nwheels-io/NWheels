using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Processing.Messages;

namespace NWheels.Testing.Processing.Messages
{
    public class TestServiceBus : IServiceBus
    {
        public TestServiceBus()
        {
            this.PublishedMessages = new List<IMessageObject>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IServiceBus

        public void EnqueueMessage(IMessageObject message)
        {
            this.PublishedMessages.Add(message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DispatchMessageOnCurrentThread(IMessageObject message)
        {
            if ( DispatchingOnCurrentThread != null )
            {
                DispatchingOnCurrentThread(message);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void SubscribeActor(object actorInstance)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Type> GetSubscribedMessageBodyTypes()
        {
            throw new NotImplementedException();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<IMessageObject> PublishedMessages { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Action<IMessageObject> DispatchingOnCurrentThread { get; set; }
    }
}
