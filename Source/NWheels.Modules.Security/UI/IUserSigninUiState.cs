using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Behaviors;

namespace NWheels.Modules.Security.UI
{
    public interface IUserSignInUiState
    {
        string ErrorMessage { get; set; }
    }
}
