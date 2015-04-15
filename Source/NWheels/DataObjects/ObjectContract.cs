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
        public interface ITypeMetadataModifier
        {
            void ApplyTo(TypeMetadataBuilder type);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class Storage
        {
            [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
            public class RelationalMappingAttribute : Attribute, ITypeMetadataModifier
            {
                public void ApplyTo(TypeMetadataBuilder type)
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
