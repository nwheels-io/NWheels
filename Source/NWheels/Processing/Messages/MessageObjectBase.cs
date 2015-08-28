using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public abstract class MessageObjectBase : IMessageObject
    {
        private readonly DateTime _createdAtUtc;
        private readonly Guid _messageId;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MessageObjectBase()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MessageObjectBase(IFramework framework)
        {
            _createdAtUtc = framework.UtcNow;
            _messageId = framework.NewGuid();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IMessageObject

        public Guid MessageId
        {
            get
            {
                return _messageId;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        DateTime IMessageObject.CreatedAtUtc
        {
            get
            {
                return _createdAtUtc;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyCollection<IMessageHeader> IMessageObject.Headers
        {
            get
            {
                return OnGetHeaders();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IMessageObject.Body
        {
            get
            {
                return OnGetBody();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual IReadOnlyCollection<IMessageHeader> OnGetHeaders()
        {
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual object OnGetBody()
        {
            return this;
        }
    }
}
