using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;

namespace NWheels.UI.Uidl
{
    [DataContract(Namespace = DataContractNamespace)]
    [KnownType(typeof(UidlKeyMetaType))]
    [KnownType(typeof(UidlValueMetaType))]
    [KnownType(typeof(UidlObjectMetaType))]
    public class UidlDocument
    {
        public const string DataContractNamespace = "nwheels.uidl";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlDocument() 
        {
            this.Applications = new List<UidlApplication>();
            this.Locales = new Dictionary<string, UidlLocale>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public List<UidlApplication> Applications { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Dictionary<string, UidlLocale> Locales { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public Dictionary<string, UidlMetaType> MetaTypes { get; set; }
    }
}
