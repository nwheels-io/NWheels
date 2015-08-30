using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public interface ISetMessageResult
    {
        void SetMessageResult(MessageResult result, Exception error, out Action continuation);
    }
}
