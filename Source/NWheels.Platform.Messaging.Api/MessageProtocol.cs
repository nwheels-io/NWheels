using NWheels.Platform.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Messaging
{
    public abstract class MessageProtocol
    {
        protected MessageProtocol(Type protocolInterface, string protocolName)
        {
            this.ProtocolInterface = protocolInterface;
            this.ProtocolName = protocolName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract IMessageProtocolInterface GetAbstractInterface();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ProtocolInterface { get; }
        public string ProtocolName { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract bool IsConcreteProtocol { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class MessageProtocol<TInterface> : MessageProtocol
        where TInterface : class, IMessageProtocolInterface
    {
        protected MessageProtocol(string protocolName) 
            : base(typeof(TInterface), protocolName)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IMessageProtocolInterface GetAbstractInterface()
        {
            return GetConcreteInterface();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract TInterface GetConcreteInterface();
    }
}
