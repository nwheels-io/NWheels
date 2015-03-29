using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Modules.Security.UI
{
    public interface IUserSignInReply
    {
        string UserName { get; set; }
        string FullName { get; set; }
        string Email { get; set; }
    }
}
