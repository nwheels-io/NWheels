using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;

namespace NWheels.Processing.Messages
{
    public abstract class AbstractSessionPushMessage : MessageObjectBase
    {
        protected AbstractSessionPushMessage(IFramework framework, ISession toSession)
            : base(framework)
        {
            this.ToSession = toSession;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract object TakeSerializableSnapshot();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ISession ToSession { get; private set; }
    }
}
