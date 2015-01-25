using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.OData;
using System.Xml.Linq;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.Spatial;
using System.IO;

namespace LinqPadODataV4Driver
{
    public class EntityObjectFactory : ConventionObjectFactory
    {
        public EntityObjectFactory(EntityClrTypeCache typeCache, DynamicModule module, IEdmModel model, IEdmEntityType entityType)
            : base(module, new EntityObjectConvention(typeCache, model, entityType))
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetGeneratedEntityClassType()
        {
            return base.GetOrBuildType(new TypeKey()).DynamicType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly EdmCoreModel s_CoreModel;
        private static readonly Dictionary<IEdmPrimitiveType, Type> s_BuiltInTypesMapping;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        static EntityObjectFactory()
        {
            s_CoreModel = EdmCoreModel.Instance;
            //_defaultAssemblyResolver = new DefaultAssembliesResolver();
            s_BuiltInTypesMapping = new KeyValuePair<Type, IEdmPrimitiveType>[] { 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(string), GetPrimitiveType(EdmPrimitiveTypeKind.String)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(bool), GetPrimitiveType(EdmPrimitiveTypeKind.Boolean)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte), GetPrimitiveType(EdmPrimitiveTypeKind.Byte)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(decimal), GetPrimitiveType(EdmPrimitiveTypeKind.Decimal)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(double), GetPrimitiveType(EdmPrimitiveTypeKind.Double)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Guid), GetPrimitiveType(EdmPrimitiveTypeKind.Guid)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(short), GetPrimitiveType(EdmPrimitiveTypeKind.Int16)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(int), GetPrimitiveType(EdmPrimitiveTypeKind.Int32)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(long), GetPrimitiveType(EdmPrimitiveTypeKind.Int64)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(sbyte), GetPrimitiveType(EdmPrimitiveTypeKind.SByte)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(float), GetPrimitiveType(EdmPrimitiveTypeKind.Single)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(byte[]), GetPrimitiveType(EdmPrimitiveTypeKind.Binary)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Stream), GetPrimitiveType(EdmPrimitiveTypeKind.Stream)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geography), GetPrimitiveType(EdmPrimitiveTypeKind.Geography)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPoint)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyLineString)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyPolygon)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyCollection)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiLineString)),
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPoint)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeographyMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeographyMultiPolygon)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(Geometry), GetPrimitiveType(EdmPrimitiveTypeKind.Geometry)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPoint)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryLineString)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryPolygon)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryCollection), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryCollection)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiLineString), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiLineString)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPoint), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPoint)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(GeometryMultiPolygon), GetPrimitiveType(EdmPrimitiveTypeKind.GeometryMultiPolygon)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(DateTimeOffset), GetPrimitiveType(EdmPrimitiveTypeKind.DateTimeOffset)), 
                new KeyValuePair<Type, IEdmPrimitiveType>(typeof(TimeSpan), GetPrimitiveType(EdmPrimitiveTypeKind.Duration)), 
             }.ToDictionary<KeyValuePair<Type, IEdmPrimitiveType>, IEdmPrimitiveType, Type>(kvp => kvp.Value, kvp => kvp.Key);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IEdmPrimitiveType GetPrimitiveType(EdmPrimitiveTypeKind kind)
        {
            return s_CoreModel.GetPrimitiveType(kind);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityObjectConvention : ImplementationConvention
        {
            private readonly EntityClrTypeCache _typeCache;
            private readonly IEdmEntityType _entityType;
            private readonly IEdmModel _model;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityObjectConvention(EntityClrTypeCache typeCache, IEdmModel model, IEdmEntityType entityType) 
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
                _typeCache = typeCache;
                _model = model;
                _entityType = entityType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                context.ClassFullName = _entityType.FullTypeName();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                _typeCache.Update(_entityType, writer.OwnerClass.TypeBuilder);

                writer.Attribute<OriginalNameAttribute>(a => a.Arg(_entityType.Name));

                if ( _entityType.DeclaredKey != null )
                {
                    writer.Attribute<KeyAttribute>(a => a.Arg(string.Join(",", _entityType.DeclaredKey.Select(p => p.Name).ToArray())));
                }

                var collectionFields = new List<Field<TypeTemplate.TProperty>>();
                var collectionTypes = new List<Type>();

                foreach ( var property in _entityType.Properties() )
                {
                    var propertyClrType = TranslateEdmTypeToClrType(property.Type.Definition);

                    using ( TypeTemplate.CreateScope<TypeTemplate.TProperty>(propertyClrType) )
                    {
                        Field<TypeTemplate.TProperty> backingField;
                        writer.Field<TypeTemplate.TProperty>("_" + property.Name, out backingField);

                        if ( property.Type.IsCollection() )
                        {
                            collectionFields.Add(backingField);
                            collectionTypes.Add(propertyClrType);
                        }

                        writer.NewVirtualWritableProperty<TypeTemplate.TProperty>(property.Name).ImplementAutomatic(backingField);
                    }
                }

                writer.Constructor(w => {
                    for ( int i = 0 ; i < collectionFields.Count ; i++ )
                    {
                        using (TypeTemplate.CreateScope<TypeTemplate.TProperty>(collectionTypes[i]))
                        {
                            IOperand op = Static.Func<string, object>(
                                DynamicDataServiceContextBase.CreateObjectByTypeName,
                                w.Const(collectionTypes[i].FullName));

                            collectionFields[i].Assign(op.CastTo<TypeTemplate.TProperty>());
                        }
                    }
                });
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private Type TranslateEdmTypeToClrType(IEdmType edmType)
            {
                var annotation = _model.GetAnnotationValue<ClrTypeAnnotation>(edmType);

                if ( annotation != null )
                {
                    return annotation.ClrType;
                }

                var entityType = (edmType as IEdmEntityType);

                if ( entityType != null )
                {
                    return _typeCache.GetEntityClrType(entityType);
                }

                var collectionType = (edmType as IEdmCollectionType);

                if ( collectionType != null )
                {
                    var elementClrType = TranslateEdmTypeToClrType(collectionType.ElementType.Definition);
                    return typeof (DataServiceCollection<>).MakeGenericType(elementClrType);
                }

                var primitiveType = (edmType as IEdmPrimitiveType);
                
                if ( primitiveType != null )
                {
                    return s_BuiltInTypesMapping[primitiveType];
                }

                throw new Exception("Could not determine CLR type for EDM type: " + edmType.FullTypeName() + " {" + edmType.GetType().Name + "}");
            }
        }
    }
}
