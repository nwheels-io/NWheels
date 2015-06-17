using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using NWheels.Extensions;

namespace NWheels.UI.Core
{
    public abstract class UIElementDescription : UINodeDescription
    {
        protected UIElementDescription(string idName, UIContentElementDescription parent)
            : base(idName, parent)
        {
            this.Enabled = true;
            this.Authorized = true;
            this.Text = idName.SplitPascalCase();
            this.ElementType = null;
            this.Notifications = new List<NotificationDescription>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ElementType { get; protected set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Text { get; set; }
        public string HelpText { get; set; }
        public string Icon { get; set; }
        public bool Enabled { get; set; }
        public bool Authorized { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<NotificationDescription> Notifications { get; private set; }
    }
}
