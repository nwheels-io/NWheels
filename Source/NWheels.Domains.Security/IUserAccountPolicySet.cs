using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Domains.Security.Core;

namespace NWheels.Domains.Security
{
    public interface IUserAccountPolicySet
    {
        UserAccountPolicy GetPolicy(IUserAccountEntity userAccount);
    }
}
