using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Microsoft.OData.Edm;

namespace LinqPadODataV4Driver
{
    public class EdmEntityTypeKey : TypeKey
    {
        private readonly IEdmModel _model;
        private readonly IEdmEntityType _entityType;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public EdmEntityTypeKey(IEdmModel model, IEdmEntityType entityType)
        {
            _model = model;
            _entityType = entityType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(TypeKey other)
        {
            var otherEntityKey = (other as EdmEntityTypeKey);

            if ( otherEntityKey != null )
            {
                return (otherEntityKey.EntityType.FullTypeName() == this.EntityType.FullTypeName());
            }
            else
            {
                return base.Equals(other);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return _entityType.FullTypeName().GetHashCode();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IEdmEntityType EntityType
        {
            get { return _entityType; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IEdmModel Model
        {
            get { return _model; }
        }
    }
}
