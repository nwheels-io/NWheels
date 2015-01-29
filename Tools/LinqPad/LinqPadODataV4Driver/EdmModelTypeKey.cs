
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Microsoft.OData.Edm;

namespace LinqPadODataV4Driver
{
    public class EdmModelTypeKey : TypeKey
    {
        private readonly IEdmModel _model;
        private readonly string _entityNamespace;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public EdmModelTypeKey(IEdmModel model)
            : base(baseType: typeof(ODataClientContextBase))
        {
            _model = model;
            _entityNamespace = model.GetEntityNamespace();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool Equals(TypeKey other)
        {
            var otherEntityKey = (other as EdmModelTypeKey);

            if ( otherEntityKey != null )
            {
                return (otherEntityKey.EntityNamespace == this.EntityNamespace);
            }
            else
            {
                return base.Equals(other);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int GetHashCode()
        {
            return _entityNamespace.GetHashCode();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IEdmModel Model
        {
            get { return _model; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string EntityNamespace
        {
            get { return _entityNamespace; }
        }
    }
}
