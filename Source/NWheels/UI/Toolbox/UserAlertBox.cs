using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Core;
using NWheels.UI.Uidl;

namespace NWheels.UI.Toolbox
{
    public class UserAlertBox : WidgetBase<UserAlertBox, Empty.Data, UserAlertBox.IState>
    {
        public UserAlertBox(string idName, ControlledUidlNode parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<UserAlertBox, Empty.Data, IState> presenter)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlNotification<IState> StateSetter { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        [ViewModelContract]
        public interface IState
        {
            UserAlertType Type { get; set; }
            string Heading { get; set; }
            string Details { get; set; }
            string Action { get; set; }
            string ActionUrl { get; set; }
        }
    }
}
