using System;
using Newtonsoft.Json;

namespace NWheels.DataObjects.Core
{
    public abstract class MetadataElement<TElement> : IMetadataElement
        where TElement : class, IMetadataElement
    {
        #region IMetadataElement Members

        public abstract void AcceptVisitor(ITypeMetadataVisitor visitor);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [JsonIgnore]
        public virtual Type ElementType
        {
            get
            {
                return typeof(TElement);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [JsonIgnore]
        public abstract string ReferenceName { get; }

        #endregion
    }
}
