using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using NWheels.Authorization;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class EntityMethodCommandMessage : AbstractCommandMessage
    {
        private readonly IEntityId _entityId;
        private readonly Type _domainContextContract;
        private readonly IMethodCallObject _call;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityMethodCommandMessage(
            IFramework framework, 
            ISession session, 
            IEntityId entityId, 
            Type domainContextContract,
            IMethodCallObject call, 
            bool synchronous)
            : base(framework, session, synchronous)
        {
            _entityId = entityId;
            _domainContextContract = domainContextContract;
            _call = call;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IReadOnlyCollection<IMessageHeader> OnGetHeaders()
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type DomainContextContract
        {
            get
            {
                return _domainContextContract;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type EntityContract
        {
            get
            {
                return _entityId.ContractType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityId EntityId
        {
            get
            {
                return _entityId;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodInfo EntityMethod
        {
            get
            {
                return _call.MethodInfo;
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
    }
}
