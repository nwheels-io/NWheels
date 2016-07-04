using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
            this.MetaTypes = new Dictionary<string, UidlMetaType>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private UidlDocument(
            List<UidlApplication> applications, 
            Dictionary<string, UidlLocale> locales, 
            Dictionary<string, UidlMetaType> metaTypes,
            string localeIdName)
        {
            this.Applications = applications;
            this.Locales = locales;
            this.MetaTypes = metaTypes;
            this.CurrentLocaleIdName = localeIdName;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public UidlDocument ForCurrentUser()
        {
            return new UidlDocument(
                this.Applications, 
                this.Locales.Values.Select(locale => locale.ForCurrentUser()).ToDictionary(locale => locale.IdName),
                this.MetaTypes,
                CultureInfo.CurrentUICulture.Name);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataMember]
        public string CurrentLocaleIdName { get; set; }
    }
}
