using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Messages
{
    public class MessageActionHeader : IMessageHeader
    {
        public MessageActionHeader(Type contract, MethodInfo method)
        {
            Contract = contract;
            Method = method;
        }

        public Type Contract { get; private set; }

        public MethodInfo Method { get; private set; }

        string IMessageHeader.Name
        {
            get
            {
                return "Action";
            }
        }

        string IMessageHeader.Values
        {
            get
            {
                return string.Format("{0}.{1}", Contract.Name, Method.Name);
            }
        }
    }
}
