using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class AlertAttribute : Attribute
    {
        public AlertType AlertType { get; protected set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class InfoAlertAttribute : AlertAttribute
    {
        public InfoAlertAttribute()
        {
            base.AlertType = AlertType.Info;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class WarningAlertAttribute : AlertAttribute
    {
        public WarningAlertAttribute()
        {
            base.AlertType = AlertType.Warning;
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ErrorAlertAttribute : AlertAttribute
    {
        public ErrorAlertAttribute()
        {
            base.AlertType = AlertType.Error;
        }
    }
}
