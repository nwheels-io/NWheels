using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public abstract class PropertyContractAttribute : Attribute, IMetadataElement
    {
        public virtual void AcceptVisitor(IMetadataElementVisitor visitor)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ElementType
        {
            get { return typeof(PropertyContractAttribute); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ReferenceName
        {
            get
            {
                return this.GetType().Name;
            }
        }
    }
}
