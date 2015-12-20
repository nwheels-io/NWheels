using System;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.DataObjects;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Stacks.MongoDb.Factories.PropertyStrategies
{
    public class ScalarPropertyStrategy : PropertyImplementationStrategy
    {
        public ScalarPropertyStrategy(
            PropertyImplementationStrategyMap ownerMap, 
            ObjectFactoryContext factoryContext,
            ITypeMetadataCache metadataCache,
            ITypeMetadata metaType,
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnImplementContractProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TypeTemplate.TInterface> writer)
        {
            AttributeWriter attributes = null;

            if ( MetaProperty == MetaType.EntityIdProperty )
            {
                attributes = Attributes.Set<BsonIdAttribute>();
            }

            if ( MetaProperty.ClrType.IsEnum )
            {
                attributes = (attributes != null
                    ? attributes.Set<BsonRepresentationAttribute>(args => args.Arg(BsonType.String))
                    : Attributes.Set<BsonRepresentationAttribute>(args => args.Arg(BsonType.String)));
            }

            if ( MetaProperty.ClrType == typeof(DateTime) )
            {
                attributes = (attributes != null
                    ? attributes.Set<BsonDateTimeOptionsAttribute>(args => args.Named(x => x.Kind, DateTimeKind.Utc)) 
                    : Attributes.Set<BsonDateTimeOptionsAttribute>(args => args.Named(x => x.Kind, DateTimeKind.Utc)));
            }

            if ( attributes != null )
            {
                writer.NewVirtualWritableProperty<TT.TProperty>(MetaProperty.Name).ImplementAutomatic(p => attributes);
            }
            else
            {
                writer.NewVirtualWritableProperty<TT.TProperty>(MetaProperty.Name).ImplementAutomatic();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingImportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            if ( MetaProperty.ClrType == typeof(DateTime) )
            {
                var valueLocal = writer.Local<DateTime>(initialValue: valueVector.ItemAt(MetaProperty.PropertyIndex).CastTo<DateTime>());
                HelpGetPropertyBackingFieldByName(writer).Assign(
                    writer.New<DateTime>(
                        valueLocal.Prop(x => x.Year),
                        valueLocal.Prop(x => x.Month),
                        valueLocal.Prop(x => x.Day),
                        valueLocal.Prop(x => x.Hour),
                        valueLocal.Prop(x => x.Minute),
                        valueLocal.Prop(x => x.Second),
                        valueLocal.Prop(x => x.Millisecond),
                        writer.Const(DateTimeKind.Utc))
                    .CastTo<TT.TProperty>()
                );
            }
            else
            {
                HelpGetPropertyBackingFieldByName(writer).Assign(valueVector.ItemAt(MetaProperty.PropertyIndex).CastTo<TT.TProperty>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingExportStorageValue(MethodWriterBase writer, Operand<IEntityRepository> entityRepo, Operand<object[]> valueVector)
        {
            valueVector.ItemAt(MetaProperty.PropertyIndex).Assign(HelpGetPropertyBackingFieldByName(writer).CastTo<object>());
        }

        #endregion
    }
}
