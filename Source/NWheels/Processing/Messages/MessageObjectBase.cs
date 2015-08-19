using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public abstract class MessageObjectBase : IMessageObject
    {
        #region Implementation of IMessageObject

        public virtual IReadOnlyCollection<IMessageHeader> Headers
        {
            get
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual object Body
        {
            get
            {
                return this;
            }
        }

        #endregion
    }
}
