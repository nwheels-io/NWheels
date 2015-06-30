using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects.Core;

namespace NWheels.DataObjects
{
    public static class ObjectContract
    {
        public static class Storage
        {
            [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
            public class RelationalMappingAttribute : Attribute, IObjectContractAttribute
            {
                public void ApplyTo(TypeMetadataBuilder type, TypeMetadataCache metadataCache)
                {
                    var mapping = type.SafeGetRelationalMapping();

                    if ( !string.IsNullOrWhiteSpace(this.PrimaryTable) )
                    {
                        mapping.PrimaryTableName = this.PrimaryTable;
                    }

                    if ( this.InheritanceKind != RelationalInheritanceKind.None )
                    {
                        mapping.InheritanceKind = this.InheritanceKind;
                    }
                }

                //-------------------------------------------------------------------------------------------------------------------------------------------------

                public string PrimaryTable { get; set; }
                public RelationalInheritanceKind InheritanceKind { get; set; }
            }
        }
    }
}
