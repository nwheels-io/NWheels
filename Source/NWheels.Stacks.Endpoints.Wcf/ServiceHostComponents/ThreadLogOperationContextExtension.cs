using System.ServiceModel;
using NWheels.Logging;

namespace NWheels.Stacks.Endpoints.Wcf.ServiceHostComponents
{
    public class ThreadLogOperationContextExtension : IExtension<OperationContext>
    {
        public ThreadLogOperationContextExtension(ILogActivity activity)
        {
            Activity = activity;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------
            
        public void Attach(OperationContext owner)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Detach(OperationContext owner)
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ILogActivity Activity { get; private set; }
    }
}