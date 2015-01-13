using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    [Serializable]
    public class CodeBehindErrorException : Exception
    {
        public CodeBehindErrorException(string message)
            : base(message)
        {
        }
    }
}
