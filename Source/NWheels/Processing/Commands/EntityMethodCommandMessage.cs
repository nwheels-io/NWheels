using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Processing.Commands.Impl;
using NWheels.Processing.Messages;

namespace NWheels.Processing.Commands
{
    public class EntityMethodCommandMessage : AbstractCommandMessage
    {
        private readonly IEntityId _entityId;
        private readonly IMethodCallObject _call;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityMethodCommandMessage(IEntityId entityId, IMethodCallObject call)
        {
            _entityId = entityId;
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
