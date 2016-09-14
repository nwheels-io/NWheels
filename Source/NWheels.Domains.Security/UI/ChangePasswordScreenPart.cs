using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.UI;
using NWheels.UI.Toolbox;
using NWheels.UI.Uidl;

namespace NWheels.Domains.Security.UI
{
    public class ChangePasswordScreenPart :
        TransactionScreenPart<Empty.Context, AuthenticatedChangePasswordTx.IInput, AuthenticatedChangePasswordTx, AuthenticatedChangePasswordTx.IOutput>
    {
        public ChangePasswordScreenPart(string idName, UidlApplication parent)
            : base(idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void DescribePresenter(
            PresenterBuilder<
                TransactionScreenPart<Empty.Context, AuthenticatedChangePasswordTx.IInput, AuthenticatedChangePasswordTx, AuthenticatedChangePasswordTx.IOutput>, 
                Empty.Data, 
                Empty.State
            > presenter)
        {
            base.DescribePresenter(presenter);
            
            Transaction.TemplateName = "TransactionFormFlatStyle";
            Transaction.InputForm.Field(x => x.NewPassword, modifiers: FormFieldModifiers.Password | FormFieldModifiers.Confirm);
            Transaction.Commands.Remove(Transaction.Reset);
            Transaction.Execute.Text = "ChangePassword";
            
            Transaction.UseOutputForm();
            Transaction.OutputForm.TemplateName = "FormAlertBig";
            Transaction.OutputForm.Field(x => x.Message, type: FormFieldType.Alert, setup: f => {
                f.Label = null; // use field value for alert contents
                f.AlertType = UserAlertType.Success;
            });
        }
    }
}
