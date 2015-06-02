using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    public interface IUIApplicationBuilder<TApp>
        where TApp : IUIApplication
    {
        void BuildApplication(TApp app);
    }
}
