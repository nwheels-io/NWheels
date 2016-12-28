using NWheels.Domains.DevOps.Alerts.Entities;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.DevOps.Alerts.UI.ScreenParts
{
    public class AlertsScreenPart : CrudScreenPart<ISystemAlertConfigurationEntity>
    {
        public AlertsScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(PresenterBuilder<CrudScreenPart<ISystemAlertConfigurationEntity>, Empty.Data, IState> presenter)
        {
            base.DescribePresenter(presenter);

            Crud.Grid
                .Column(x => x.Id, size: FieldSize.Large)
                .Column(x => x.DescriptionText, title: "Description", size: FieldSize.ExtraLarge);

            Crud.Form.AutoRecalculateOnChange = true;
            Crud.Form
                .LookupSource(x => x.Id, x => x.AvailableAlertIds)
                .Field(x => x.DescriptionText, label: "Description", setup: f => f.Modifiers |= FormFieldModifiers.Memo)
                .Field(x => x.PossibleReasonText, label: "PossibleReason", setup: f => f.Modifiers |= FormFieldModifiers.Memo)
                .Field(x => x.SuggestedActionText, label: "SuggestedAction", setup: f => f.Modifiers |= FormFieldModifiers.Memo);
        }
    }
}
