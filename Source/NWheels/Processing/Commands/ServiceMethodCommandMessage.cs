using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using NWheels.Authorization;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class ServiceMethodCommandMessage : AbstractCommandMessage
    {
        private readonly IMethodCallObject _call;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServiceMethodCommandMessage(IFramework framework, ISession session, IMethodCallObject call, bool synchronous)
            : base(framework, session, synchronous)
        {
            _call = call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool CheckAuthorization(out bool authenticationRequired)
        {
            authenticationRequired = true;
            return Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMethodCallObject Call
        {
            get
            {
                return _call;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ServiceContract
        {
            get
            {
                return _call.MethodInfo.DeclaringType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodInfo ServiceMethod
        {
            get
            {
                return _call.MethodInfo;
            }
        }
    }
}
