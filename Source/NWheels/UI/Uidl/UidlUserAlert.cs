using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NWheels.Exceptions;
using NWheels.UI.Uidl;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "UserAlert", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlUserAlert : AbstractUidlNode
    {
        public UidlUserAlert(string idName, UidlApplication parent)
            : base(UidlNodeType.UserAlert, idName, parent)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal UidlUserAlert(MethodInfo alertMethod, UidlApplication parent)
            : base(UidlNodeType.UserAlert, GetIdNameFromMethod(alertMethod), parent)
        {
            var attribute = alertMethod.GetCustomAttribute<UserAlertAttribute>();

            if ( attribute == null )
            {
                throw new ContractConventionException(
                    typeof(UidlBuilder), alertMethod.DeclaringType, alertMethod, "UserAlertType attribute is missing on alert method.");
            }

            this.Type = attribute.AlertType;
            this.Text = alertMethod.Name;
            this.ParameterNames = alertMethod.GetParameters().Select(p => p.Name).ToArray();
            this.ResultChoices = attribute.AlertResults;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UserAlertType Type { get; set; }
        public string Text { get; set; }
        public string[] ParameterNames { get; set; }
        public UserAlertResult[] ResultChoices { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static string GetIdNameFromMethod(MethodInfo alertMethod)
        {
            return alertMethod.DeclaringType.FullName + "." + alertMethod.Name;
        }
    }
}
