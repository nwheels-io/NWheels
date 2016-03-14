using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ServiceMethodCommandMessage : AbstractCommandMessage, IHaveMethodCall
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

        public override Dictionary<string, object> GetParameters()
        {
            var parameterInfos = ServiceMethod.GetParameters();
            var parametersPairs = new Dictionary<string, object>();

            for (int i = 0 ; i < parameterInfos.Length ; i++)
            {
                parametersPairs[parameterInfos[i].Name] = Call.GetParameterValue(i);
            }
            
            return parametersPairs;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of AbstractCommandMessage

        public override string AuditName
        {
            get
            {
                return ServiceContract.Name + "." + ServiceMethod.Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override string CommandString
        {
            get
            {
                return string.Format("InvokeServiceMethod({0}.{1})", ServiceContract.Name, ServiceMethod.Name);
            }
        }

        #endregion
    }
}
