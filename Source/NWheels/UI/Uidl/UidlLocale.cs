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
        public string CultureName { get; set; }
        [DataMember]
        public Dictionary<string, string> Translations { get; set; }
    }
}
