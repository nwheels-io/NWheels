using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Exceptions
{
    public class ContractConventionException : Exception
    {
        public ContractConventionException(string message)
            : base(message)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContractConventionException(Type convention, Type contract, MemberInfo member, string message)
            : base(string.Format("{0}.{1} does not match {2}. {3}.", contract.FullName, member.Name, convention.GetType().Name, message))
        {
        }
    }
}
