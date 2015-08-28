using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
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

        public ServiceMethodCommandMessage(IFramework framework, ISession session, IMethodCallObject call)
            : base(framework, session)
        {
            _call = call;
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
