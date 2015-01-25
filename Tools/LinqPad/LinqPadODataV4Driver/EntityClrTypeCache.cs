using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Microsoft.OData.Edm;

namespace LinqPadODataV4Driver
{
    public class EntityClrTypeCache
    {
        private readonly Dictionary<IEdmEntityType, Type> _clrTypeByEdmType = new Dictionary<IEdmEntityType, Type>();
        private readonly DynamicModule _module;
        private readonly IEdmModel _model;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityClrTypeCache(DynamicModule module, IEdmModel model)
        {
            _module = module;
            _model = model;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetEntityClrType(IEdmEntityType edmType)
        {
            Type clrType;

            if ( !_clrTypeByEdmType.TryGetValue(edmType, out clrType) )
            {
                var factory = new EntityObjectFactory(this, _module, _model, edmType);
                clrType = factory.GetGeneratedEntityClassType();
                _clrTypeByEdmType[edmType] = clrType;
            }

            return clrType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update(IEdmEntityType edmType, Type clrType)
        {
            _clrTypeByEdmType[edmType] = clrType;
        }
    }
}
