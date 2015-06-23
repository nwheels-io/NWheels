using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UI.Uidl
{
    [DataContract(Name = "Locale", Namespace = UidlDocument.DataContractNamespace)]
    public class UidlLocale
    {
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
