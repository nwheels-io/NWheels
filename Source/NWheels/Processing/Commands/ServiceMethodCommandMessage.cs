using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class ServiceMethodCommandMessage : AbstractCommandMessage
    {
        private readonly IMethodCallObject _call;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServiceMethodCommandMessage(IMethodCallObject call)
        {
            _call = call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IReadOnlyCollection<IMessageHeader> Headers
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object Body
        {
            get
            {
                return _call;
            }
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
