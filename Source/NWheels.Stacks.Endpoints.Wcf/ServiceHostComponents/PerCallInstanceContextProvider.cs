using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class PerCallInstanceContextProvider : IInstanceContextProvider
    {
        public InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            return null;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsIdle(InstanceContext instanceContext)
        {
            return true;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
        }
    }
}