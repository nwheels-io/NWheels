using System;
using System.Security.Principal;
using NWheels.Authorization;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public abstract class AbstractCommandMessage : MessageObjectBase
    {
        protected AbstractCommandMessage()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AbstractCommandMessage(IFramework framework, ISession session)
            : base(framework)
        {
            Principal = session.UserPrincipal;
            Session = session;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IPrincipal Principal { get; private set; }
        public ISession Session { get; private set; }
    }
}
