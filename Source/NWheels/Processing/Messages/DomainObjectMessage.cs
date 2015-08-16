using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities.Core;

namespace NWheels.Processing.Messages
{
    public interface IDomainObjectMessage : IMessageObject
    {
        IDomainObject DomainObject { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainObjectMessage : MessageObjectBase, IDomainObjectMessage
    {
        public DomainObjectMessage(IDomainObject obj)
        {
            DomainObject = obj;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IDomainObject DomainObject { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object Body
        {
            get { return DomainObject; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class DomainObjectMessage<TContract> : MessageObjectBase, IDomainObjectMessage
    {
        public DomainObjectMessage(TContract obj)
        {
            DomainObject = obj;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContract DomainObject { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object Body
        {
            get { return DomainObject; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IDomainObject IDomainObjectMessage.DomainObject
        {
            get 
            { 
                return (IDomainObject)DomainObject; 
            }
        }
    }
}
