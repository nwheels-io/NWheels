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
using TT = Hapil.TypeTemplate;

namespace LinqPadODataV4Driver
{
    public class ODataClientEntityFactory : ConventionObjectFactory
    {
        public ODataClientEntityFactory(DynamicModule module)
            : base(module, new ODataClientEntityConvention())
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public Type ImplementClientEntity(IEdmModel model, IEdmEntityType entityType)
        {
            return base.GetOrBuildType(new EdmEntityTypeKey(model, entityType)).DynamicType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly EdmCoreModel s_CoreModel;
        private static readonly Dictionary<IEdmPrimitiveType, Type> s_BuiltInTypesMapping;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        static ODataClientEntityFactory()
        {
            s_CoreModel = EdmCoreModel.Instance;
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

        public class ODataClientEntityConvention : ImplementationConvention
        {
            public ODataClientEntityConvention() 
                : base(Will.InspectDeclaration | Will.ImplementBaseClass)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                var entityKey = (EdmEntityTypeKey)context.TypeKey;
                context.ClassFullName = entityKey.EntityType.FullTypeName();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TT.TBase> writer)
            {
                var entityTypeKey = (EdmEntityTypeKey)base.Context.TypeKey;
                var entityType = entityTypeKey.EntityType;

                WriteEntityAttributes(writer, entityType);

                var initializers = new List<Action<ConstructorWriter>>();

                foreach (var property in entityType.Properties())
                {
                    WriteEntityProperty(writer, entityTypeKey.Model, property, initializers);
                }

                writer.Constructor(cw => {
                    initializers.ForEach(init => init(cw));
                });
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private static void WriteEntityAttributes(ImplementationClassWriter<TT.TBase> writer, IEdmEntityType entityType)
            {
                writer.Attribute<OriginalNameAttribute>(a => a.Arg(entityType.Name));

                if ( entityType.DeclaredKey != null )
                {
                    var commaSeparatedKeyPropertyNames = string.Join(",", entityType.DeclaredKey.Select(p => p.Name).ToArray());
                    writer.Attribute<KeyAttribute>(a => a.Arg(commaSeparatedKeyPropertyNames));
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void WriteEntityProperty(
                ImplementationClassWriter<TypeTemplate.TBase> writer, 
                IEdmModel model,
                IEdmProperty property, 
                List<Action<ConstructorWriter>> initializers)
            {
                var propertyClrType = TranslateEdmTypeToClrType(model, property.Type.Definition);

                using (TT.CreateScope<TT.TProperty>(propertyClrType))
                {
                    var backingField = writer.Field<TT.TProperty>("_" + property.Name);

                    if (property.Type.IsCollection())
                    {
                        Type elementClrType = propertyClrType.GetGenericArguments()[0];

                        initializers.Add(cw => {
                            using (TT.CreateScope<TT.TProperty, TT.TItem>(propertyClrType, elementClrType))
                            {
                                backingField.Assign(cw.New<TT.TProperty>(cw.Const<IEnumerable<TT.TItem>>(null), cw.Const(TrackingMode.None)));
                            }
                        });
                    }

                    writer.NewVirtualWritableProperty<TT.TProperty>(property.Name).ImplementAutomatic(backingField);
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private Type TranslateEdmTypeToClrType(IEdmModel model, IEdmType edmType)
            {
                var annotation = model.GetAnnotationValue<ClrTypeAnnotation>(edmType);

                if ( annotation != null )
                {
                    return annotation.ClrType;
                }

                var entityType = (edmType as IEdmEntityType);

                if ( entityType != null )
                {
                    return base.Context.Factory.FindDynamicType(new EdmEntityTypeKey(model, entityType));
                }

                var collectionType = (edmType as IEdmCollectionType);

                if ( collectionType != null )
                {
                    var elementClrType = TranslateEdmTypeToClrType(model, collectionType.ElementType.Definition);
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
