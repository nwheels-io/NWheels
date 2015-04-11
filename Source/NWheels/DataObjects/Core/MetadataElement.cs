using System;

namespace NWheels.DataObjects.Core
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract string ReferenceName { get; }

        #endregion
    }
}
