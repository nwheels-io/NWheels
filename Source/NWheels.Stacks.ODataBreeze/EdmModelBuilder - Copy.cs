#if false

using Microsoft.Data.Edm.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Spatial;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using NWheels.DataObjects;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public class EdmModelBuilder
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly EdmModel _model;
        private readonly EdmEntityContainer _entityContainer;
        private readonly Dictionary<ITypeMetadata, EdmEntityType> _entityTypeByMetadata;
        private readonly Dictionary<Type, EdmEnumType> _enumTypeByClrType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EdmModelBuilder(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            
            _model = new EdmModel();
            _entityContainer = new EdmEntityContainer("", "Default");
            _entityTypeByMetadata = new Dictionary<ITypeMetadata, EdmEntityType>();
            _enumTypeByClrType = new Dictionary<Type, EdmEnumType>();
            
            _model.AddElement(_entityContainer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddEntity(ITypeMetadata metadata)
        {
            if ( _entityTypeByMetadata.ContainsKey(metadata) )
            {
                return;
            }

            var entityType = new EdmEntityType(metadata.ContractType.Namespace, metadata.Name);
            var entitySet = _entityContainer.AddEntitySet(metadata.Name, entityType);

            _model.AddElement(entityType);
            _entityTypeByMetadata.Add(metadata, entityType);

            AddProperties(entityType, metadata);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EdmModel GetModel()
        {
            return _model;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetModelJsonString()
        {
            var output = new StringBuilder();
            IEnumerable<EdmError> errors;

            using ( var xmlWriter = XmlWriter.Create(output) )
            {
                if ( !EdmxWriter.TryWriteEdmx(_model, xmlWriter, EdmxTarget.OData, out errors) )
                {
                    var errorsArray = (errors != null ? errors.ToArray() : new EdmError[0]);
                    var errorsText = string.Join("; ", errorsArray.Select(e => e.ToString()));

                    throw new Exception(string.Format("Failed to write EDM metadata JSON. {0} errors. {1}", errorsArray.Length, errorsText));
                }

                xmlWriter.Flush();
            }

            return output.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void AddProperties(EdmEntityType entityType, ITypeMetadata metadata)
        {
            foreach ( var property in metadata.Properties )
            {
                switch ( property.Kind )
                {
                    case PropertyKind.Scalar:
                        if ( property.ClrType.IsEnum )
                        {
                            AddEnumProperty(entityType, property);
                        }
                        else
                        {
                            AddScalarProperty(entityType, property);
                        }
                        break;
                    case PropertyKind.Relation:
                        AddRelationProperty(entityType, property);
                        break;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddScalarProperty(EdmEntityType entityType, IPropertyMetadata property)
        {
            entityType.AddStructuralProperty(property.Name, GetEdmPrimitiveTypeKind(property.ClrType));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddEnumProperty(EdmEntityType entityType, IPropertyMetadata property)
        {
            var enumType = GetOrAddEnumType(property.ClrType);
            entityType.AddStructuralProperty(property.Name, new EdmEnumTypeReference(enumType, isNullable: !property.Validation.IsRequired));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void AddRelationProperty(EdmEntityType entityType, IPropertyMetadata property)
        {
            var thisNavigationInfo = new EdmNavigationPropertyInfo() {
                Name = property.Name,
                ContainsTarget = (property.Relation.Kind == RelationKind.Composition),
                Target = GetOrAddEntityType(property.Relation.RelatedPartyType),
                TargetMultiplicity = GetRelationTargetMultiplicity(property)
            };

            if ( property.Relation.InverseProperty != null )
            {
                entityType.AddBidirectionalNavigation()
            }
            else
            {
                entityType.AddUnidirectionalNavigation()
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EdmEntityType GetOrAddEntityType(ITypeMetadata metadata)
        {
            EdmEntityType existingEdmType;

            if ( _entityTypeByMetadata.TryGetValue(metadata, out existingEdmType) )
            {
                return existingEdmType;
            }

            AddEntity(metadata);
            return _entityTypeByMetadata[metadata];
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EdmEnumType GetOrAddEnumType(Type clrType)
        {
            EdmEnumType existingEdmType;

            if ( _enumTypeByClrType.TryGetValue(clrType, out existingEdmType) )
            {
                return existingEdmType;
            }

            var newEdmType = AddEnumType(clrType);
            _enumTypeByClrType.Add(clrType, newEdmType);

            return newEdmType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static EdmEnumType AddEnumType(Type clrType)
        {
            var edmType = new EdmEnumType(
                clrType.Namespace,
                clrType.Name,
                underlyingType: GetEdmPrimitiveTypeKind(clrType.UnderlyingSystemType),
                isFlags: clrType.HasAttribute<FlagsAttribute>());

            var memberNames = Enum.GetNames(clrType);
            var memberValues = Enum.GetValues(clrType);

            for ( int i = 0 ; i < memberNames.Length ; i++ )
            {
                edmType.AddMember(memberNames[i], GetEdmPrimitiveValue(memberValues.GetValue(i)));
            }

            return edmType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EdmMultiplicity GetRelationTargetMultiplicity(IPropertyMetadata property)
        {
            switch ( property.Relation.Multiplicity )
            {
                case RelationMultiplicity.OneToOne:
                case RelationMultiplicity.ManyToOne:
                    return (property.Validation.IsRequired ? EdmMultiplicity.One : EdmMultiplicity.ZeroOrOne);
                case RelationMultiplicity.OneToMany:
                case RelationMultiplicity.ManyToMany:
                    return EdmMultiplicity.Many;
                default:
                    return EdmMultiplicity.Unknown;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, EdmPrimitiveTypeKind> _s_primitiveTypeKindByType = 
            new Dictionary<Type, EdmPrimitiveTypeKind> { 
                #region Mappings
                { typeof(string), EdmPrimitiveTypeKind.String }, 
                { typeof(bool), EdmPrimitiveTypeKind.Boolean }, 
                { typeof(byte), EdmPrimitiveTypeKind.Byte }, 
                { typeof(decimal), EdmPrimitiveTypeKind.Decimal }, 
                { typeof(double), EdmPrimitiveTypeKind.Double }, 
                { typeof(Guid), EdmPrimitiveTypeKind.Guid }, 
                { typeof(short), EdmPrimitiveTypeKind.Int16 },
                { typeof(int), EdmPrimitiveTypeKind.Int32 }, 
                { typeof(long), EdmPrimitiveTypeKind.Int64 }, 
                { typeof(sbyte), EdmPrimitiveTypeKind.SByte }, 
                { typeof(float), EdmPrimitiveTypeKind.Single }, 
                { typeof(byte[]), EdmPrimitiveTypeKind.Binary }, 
                { typeof(Stream), EdmPrimitiveTypeKind.Stream }, 
                { typeof(Geography), EdmPrimitiveTypeKind.Geography },
                { typeof(GeographyPoint), EdmPrimitiveTypeKind.GeographyPoint }, 
                { typeof(GeographyLineString), EdmPrimitiveTypeKind.GeographyLineString }, 
                { typeof(GeographyPolygon), EdmPrimitiveTypeKind.GeographyPolygon }, 
                { typeof(GeographyCollection), EdmPrimitiveTypeKind.GeographyCollection },
                { typeof(GeographyMultiLineString), EdmPrimitiveTypeKind.GeographyMultiLineString },
                { typeof(GeographyMultiPoint), EdmPrimitiveTypeKind.GeographyMultiPoint }, 
                { typeof(GeographyMultiPolygon), EdmPrimitiveTypeKind.GeographyMultiPolygon }, 
                { typeof(Geometry), EdmPrimitiveTypeKind.Geometry }, 
                { typeof(GeometryPoint), EdmPrimitiveTypeKind.GeometryPoint }, 
                { typeof(GeometryLineString), EdmPrimitiveTypeKind.GeometryLineString }, 
                { typeof(GeometryPolygon), EdmPrimitiveTypeKind.GeometryPolygon }, 
                { typeof(GeometryCollection), EdmPrimitiveTypeKind.GeometryCollection }, 
                { typeof(GeometryMultiLineString), EdmPrimitiveTypeKind.GeometryMultiLineString }, 
                { typeof(GeometryMultiPoint), EdmPrimitiveTypeKind.GeometryMultiPoint }, 
                { typeof(GeometryMultiPolygon), EdmPrimitiveTypeKind.GeometryMultiPolygon }, 
                { typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset },
                { typeof(TimeSpan), EdmPrimitiveTypeKind.Time }
                #endregion
            };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, Func<object, IEdmPrimitiveValue>> _s_primitiveValueFactoryByType = 
            new Dictionary<Type, Func<object, IEdmPrimitiveValue>> { 
                #region Mappings
                { typeof(string), value => new EdmStringConstant((string)value) }, 
                { typeof(bool), value => new EdmBooleanConstant((bool)value) }, 
                { typeof(float), value => new EdmFloatingConstant((float)value) },
                { typeof(double), value => new EdmFloatingConstant((double)value) }, 
                { typeof(decimal), value => new EdmDecimalConstant((decimal)value) }, 
                { typeof(Guid), value => new EdmGuidConstant((Guid)value) }, 
                { typeof(byte), value => new EdmIntegerConstant((byte)value) }, 
                { typeof(sbyte), value => new EdmIntegerConstant((sbyte)value) },
                { typeof(short), value => new EdmIntegerConstant((short)value) },
                { typeof(int), value => new EdmIntegerConstant((int)value) },
                { typeof(long), value => new EdmIntegerConstant((long)value) },
                { typeof(byte[]), value => new EdmBinaryConstant((byte[])value) },
                { typeof(DateTime), value => new EdmDateTimeConstant((DateTime)value) },
                { typeof(DateTimeOffset), value => new EdmDateTimeOffsetConstant((DateTimeOffset)value) },
                { typeof(TimeSpan), value => new EdmTimeConstant((TimeSpan)value) },
                #endregion
            };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static EdmPrimitiveTypeKind GetEdmPrimitiveTypeKind(Type type)
        {
            EdmPrimitiveTypeKind primitiveKind;

            if ( _s_primitiveTypeKindByType.TryGetValue(type, out primitiveKind) )
            {
                return primitiveKind;
            }
            else
            {
                return EdmPrimitiveTypeKind.None;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEdmPrimitiveValue GetEdmPrimitiveValue(object value)
        {
            if ( value == null )
            {
                throw new ArgumentNullException("value");
            }

            Func<object, IEdmPrimitiveValue> valueFactory;

            if ( _s_primitiveValueFactoryByType.TryGetValue(value.GetType(), out valueFactory) )
            {
                return valueFactory(value);
            }
            else
            {
                throw new ArgumentOutOfRangeException(value.GetType().FullName + " does not correspond to EDM primitive type.", "value");
            }
        }
    }
}

#endif
