using NWheels.Domains.DevOps.Alerts.Entities;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.Alerts.UI.ScreenParts
{
    public class AlertsScreenPart : CrudScreenPart<IAlertEntity>
    {
        public AlertsScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

    }
}
