using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Cli
{
    public class BadArgumentsException : Exception
    {
        public BadArgumentsException(string message) : base(message)
        {
        }
    }
}
