using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyContractAttribute : Attribute
    {
        public PropertyContractAttribute()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsKey { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public object DefaultValue { get; set; }
        public int MinLength { get; set; }
        public int MaxLength { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public bool MinValueInclusive { get; set; }
        public bool MaxValueInclusive { get; set; }
        public string RegularExpression { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }
        public bool SortAscending { get; set; }
    }
}
