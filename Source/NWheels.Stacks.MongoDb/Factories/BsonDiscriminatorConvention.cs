using Hapil;
using Hapil.Members;
using Hapil.Writers;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.DataObjects;
using System;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class BsonDiscriminatorConvention : DecorationConvention
    {
        private readonly ObjectFactoryContext _context;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public BsonDiscriminatorConvention(ObjectFactoryContext context, ITypeMetadataCache metadataCache, ITypeMetadata metaType)
            : base(Will.DecorateClass)
        {
            _context = context;
            _metadataCache = metadataCache;
            _metaType = metaType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool ShouldApply(ObjectFactoryContext context)
        {
            return (_metaType.BaseType != null || _metaType.DerivedTypes.Count > 0);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnClass(ClassType classType, DecoratingClassWriter classWriter)
        {
            if ( _metaType.BaseType == null )
            {
                classWriter.Attribute<BsonDiscriminatorAttribute>(v => v.Arg<string>(_metaType.Name).Named<bool>(a => a.Required, true));
            }
            else
            {
                classWriter.Attribute<BsonDiscriminatorAttribute>(v => v.Arg<string>(_metaType.Name).Named<bool>(a => a.Required, true));
            }
        }
    }
}