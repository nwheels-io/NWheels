using NWheels.UI.Uidl;

namespace NWheels.UI.Core
{
    public class UserAlertDescription : UINodeDescription
    {
        public UserAlertDescription(string idName, UIContentElementDescription parent)
            : base(idName, parent)
        {
            base.NodeType = UINodeType.UserAlert;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAlertType Type { get; set; }
        public string Text { get; set; }
        public UserAlertResult[] Results { get; set; }
        public string[] Parameters { get; set; }
    }
}
