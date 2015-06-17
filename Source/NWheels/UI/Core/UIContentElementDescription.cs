using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Core
{
    public abstract class UIContentElementDescription : UIElementDescription
    {
        protected UIContentElementDescription(string idName, UIContentElementDescription parent)
            : base(idName, parent)
        {
            this.Presenter = new PresenterDescription(this);
            this.Commands = new List<CommandDescription>();
            this.UserAlerts = new List<UserAlertDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresenterDescription Presenter { get; private set; }
        public List<CommandDescription> Commands { get; private set; }
        public List<UserAlertDescription> UserAlerts { get; private set; }
    }
}
