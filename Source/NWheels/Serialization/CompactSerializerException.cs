using System;
using NWheels.Extensions;

namespace NWheels.Serialization
{
    public class CompactSerializerException : Exception
    {
        public CompactSerializerException(string message)
            : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializerException(string format, params object[] args)
            : base(format.FormatIf(args))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactSerializerException(Exception innerException, string format, params object[] args)
            : base(format.FormatIf(args), innerException)
        {
        }
    }
}
