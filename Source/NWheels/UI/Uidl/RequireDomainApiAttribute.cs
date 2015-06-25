using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    /// <summary>
    /// Whan applied to UI application class, declares that specfieid domain API contract is required by the UI application.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireDomainApiAttribute : Attribute
    {
        public RequireDomainApiAttribute(Type contractType)
        {
            this.ContractType = contractType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ContractType { get; private set; }
    }
}
