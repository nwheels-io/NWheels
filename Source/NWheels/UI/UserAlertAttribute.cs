using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.UI.Uidl;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class UserAlertAttribute : Attribute
    {
        protected UserAlertAttribute(UserAlertType alertType, UserAlertResult[] alertResults)
        {
            this.AlertType = alertType;
            this.AlertResults = alertResults;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAlertType AlertType { get; private set; }
        public UserAlertResult[] AlertResults { get; private set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class InfoAlertAttribute : UserAlertAttribute
    {
        public InfoAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Info, alertResults)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class SuccessAlertAttribute : UserAlertAttribute
    {
        public SuccessAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Success, alertResults)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WarningAlertAttribute : UserAlertAttribute
    {
        public WarningAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Warning, alertResults)
        {
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ErrorAlertAttribute : UserAlertAttribute
    {
        public ErrorAlertAttribute(params UserAlertResult[] alertResults)
            : base(UserAlertType.Error, alertResults)
        {
        }
    }
}
