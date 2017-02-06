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

            Crud.Form.Field(f => f.AlertActions, setup: alertActions => {
                var alertActionCrud = (Crud<IEntityPartAlertAction>)alertActions.NestedWidget;
                alertActionCrud.Grid.Column(x => x.SummaryText);
                /*alertActionCrud.FormTypeSelector.ForEachWidgetOfType<IUidlForm>(form => {
                    form.HideFieldsOf<IEntityPartAlertAction>(x => x.SummaryText);
                });*/

                var emailActionForm = alertActionCrud.FormTypeSelector.GetWidget<IEntityPartByEmailAlertAction, Form<IEntityPartByEmailAlertAction>>();
                emailActionForm.Field(x => x.Recipients, setup: f => {
                    var emailRecipientsCrud = (Crud<IEntityPartEmailRecipient>)f.NestedWidget;
                    emailRecipientsCrud.FormTypeSelector.ForEachWidgetOfType<IUidlForm>(form => {
                        form.HideFieldsOf<IEntityPartEmailRecipient>(x => x.SendToEmail, x => x.UserFullName);
                    });
                });

                alertActionCrud.FormTypeSelector.GetWidget<IEntityPartByEmailAlertAction, Form<IEntityPartByEmailAlertAction>>()
                    .Field(f => f.Recipients, setup: recipients => {
                        ((Crud<IEntityPartEmailRecipient>)recipients.NestedWidget).Grid
                            .Column(x => x.SendToEmail)
                            .Column(x => x.UserFullName);
                    });
            });
        }
    }
}
