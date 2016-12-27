using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NWheels.Conventions.Core;
using NWheels.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.UI.Factories;

namespace NWheels.Serialization
{
    public class MetaTypedObjectExtension : CompactSerializerExtensionBase
    {
        private readonly ICoreFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IViewModelObjectFactory _viewModelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MetaTypedObjectExtension(
            ICoreFramework framework,
            ITypeMetadataCache metadataCache, 
            IViewModelObjectFactory viewModelFactory)
        {
            _framework = framework;
            _metadataCache = metadataCache;
            _viewModelFactory = viewModelFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type GetSerializationType(Type declaredType, object obj)
        {
            var metaTypedObject = (obj as IObject);

            if (metaTypedObject != null)
            {
                return metaTypedObject.ContractType;
            }

            return obj.GetType();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override Type GetMaterializationType(Type declaredType, Type serializedType)
        {
            ITypeMetadata metaType;

            if (serializedType.IsInterface && _metadataCache.TryGetTypeMetadata(serializedType, out metaType))
            {
                if (metaType.IsEntity || metaType.IsEntityPart)
                {
                    return metaType.GetImplementationBy(typeof(DomainObjectFactory));
                }
                else if (metaType.IsViewModel)
                {
                    return metaType.GetImplementationBy(typeof(ViewModelObjectFactory));
                }
                else
                {
                    return metaType.GetImplementationBy(typeof(ConfigurationObjectFactory));
                }
            }

            return serializedType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool CanMaterialize(Type declaredType, Type serializedType)
        {
            return (serializedType.IsInterface && _metadataCache.ContainsTypeMetadata(serializedType));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override object Materialize(Type declaredType, Type serializedType)
        {
            var metaType = _metadataCache.GetTypeMetadata(serializedType);

            if (metaType.IsEntity || metaType.IsEntityPart)
            {
                return _framework.NewDomainObject(serializedType);
            }
            else if (metaType.IsViewModel)
            {
                return _viewModelFactory.NewEntity(serializedType);
            }
            else
            {
                return _framework.Components.Resolve(serializedType);
            }
        }
    }
}
