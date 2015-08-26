using System;
using System.Linq;
using System.Reflection;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;
using TT2 = NWheels.Entities.Factories.DomainObjectFactory.TemplateTypes;

namespace NWheels.Entities.Factories
{
    public class PresentationObjectFactoryContext
    {
        public PresentationObjectFactoryContext(
            ObjectFactoryContext baseContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            PropertyImplementationStrategyMap propertyMap)
        {
            this.BaseContext = baseContext;
            this.MetadataCache = metadataCache;
            this.MetaType = metaType;
            this.PropertyMap = propertyMap;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectFactoryContext BaseContext { get; private set; }
        public ITypeMetadataCache MetadataCache { get; private set; }
        public ITypeMetadata MetaType { get; private set; }
        public PropertyImplementationStrategyMap PropertyMap { get; private set; }
        public Field<TT.TInterface> DomainObjectField { get; set; }
        public Field<IPresentationObjectFactory> PresentationFactoryField { get; set; }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsEntity
        {
            get
            {
                return MetaType.ContractType.IsEntityContract();
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsEntityPart
        {
            get
            {
                return MetaType.ContractType.IsEntityPartContract();
            }
        }
    }
}