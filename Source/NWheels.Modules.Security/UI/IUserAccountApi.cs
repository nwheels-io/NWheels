using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Security.UI
{
    public interface IUserAccountApi
    {
        IUserSignInReply SignIn(IUserSignInRequest request);
    }
}
