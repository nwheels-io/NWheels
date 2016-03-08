using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.TypeModel.Serialization
{
    public class CompactObjectSerializerException : Exception
    {
        public CompactObjectSerializerException(string format, params object[] args)
            : base(format.FormatIf(args))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompactObjectSerializerException(Exception innerException, string format, params object[] args)
            : base(format.FormatIf(args), innerException)
        {
        }
    }
}
