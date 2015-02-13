using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using System.Xml.Linq;

namespace NWheels.Core.DataObjects
{
    public abstract class MetadataElement<TElement> : IMetadataElement
        where TElement : class, IMetadataElement
    {
        #region IMetadataElement Members

        public abstract void AcceptVisitor(IMetadataElementVisitor visitor);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual Type ElementType
        {
            get
            {
                return typeof(TElement);
            }
        }

        #endregion
    }
}
