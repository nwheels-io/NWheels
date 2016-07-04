using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Locale", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlLocale
    {
        public UidlLocale ForCurrentUser()
        {
            if (this.IdName == CultureInfo.CurrentCulture.Name)
            {
                // return full
                return this;
            }

            // return header only
            return new UidlLocale() { 
                IdName = this.IdName,
                FullName = this.FullName,
                IsRightToLeft = this.IsRightToLeft
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string IdName { get; set; }
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public bool IsRightToLeft { get; set; }
        [DataMember]
        public string ListSeparator { get; set; }
        [DataMember]
        public Dictionary<string, string> Translations { get; set; }
    }
}
