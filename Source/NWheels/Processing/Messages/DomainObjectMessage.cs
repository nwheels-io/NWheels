#if false

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

        protected override object OnGetBody()
        {
            return DomainObject;
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

        IDomainObject IDomainObjectMessage.DomainObject
        {
            get 
            { 
                return (IDomainObject)DomainObject; 
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override object OnGetBody()
        {
            return DomainObject;
        }
    }
}

#endif