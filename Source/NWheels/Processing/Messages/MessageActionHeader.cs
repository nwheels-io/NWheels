using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Processing.Messages
{
    public class MessageActionHeader : IMessageHeader
    {
        public MessageActionHeader(Type contract, MethodInfo method)
        {
            Contract = contract;
            Method = method;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type Contract { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodInfo Method { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IMessageHeader.Name
        {
            get
            {
                return "Action";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IMessageHeader.Values
        {
            get
            {
                return string.Format("{0}.{1}", Contract.Name, Method.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MessageActionHeader FromMethod<TContract, TRequest>(Expression<Func<TContract, Action<TRequest>>> methodSelector)
        {
            var methodInfo = methodSelector.GetMethodInfo();
            return new MessageActionHeader(typeof(TContract), methodInfo);
        }
    }
}
