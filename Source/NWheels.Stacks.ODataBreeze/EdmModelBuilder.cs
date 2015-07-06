using Microsoft.Data.Edm.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Spatial;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Hapil;
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
        private readonly Dictionary<Type, EdmType> _edmTypeByClrType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EdmModelBuilder(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            
            _model = new EdmModel();
            _entityContainer = new EdmEntityContainer("", "Default");
            _entityTypeByMetadata = new Dictionary<ITypeMetadata, EdmEntityType>();
            _edmTypeByClrType = new Dictionary<Type, EdmType>();
            
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
                        else if ( property.ClrType.IsCollectionType() )
                        {
                            AddScalarCollectionProperty(entityType, property);
                        }
                        else if ( property.ClrType != typeof(Type) )
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
            var underlyingClrType = (property.ClrType.IsNullableValueType() ? property.ClrType.GetGenericArguments()[0] : property.ClrType);
            var edmProperty = entityType.AddStructuralProperty(
                property.Name, 
                GetEdmPrimitiveTypeKind(underlyingClrType), 
                isNullable: !property.Validation.IsRequired);

            if ( property.Role == PropertyRole.Key )
            {
                entityType.AddKeys(edmProperty);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddScalarCollectionProperty(EdmEntityType entityType, IPropertyMetadata property)
        {
            var collectionType = GetOrAddCollectionType(property.ClrType);
            entityType.AddStructuralProperty(property.Name, new EdmCollectionTypeReference(collectionType, isNullable: !property.Validation.IsRequired));
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
                ContainsTarget = (
                    property.Relation.Kind == RelationKind.Composition && 
                    property.Relation.ThisPartyKind == RelationPartyKind.Principal),
                Target = GetOrAddEntityType(property.Relation.RelatedPartyType),
                TargetMultiplicity = GetRelationTargetMultiplicity(property)
            };

            if ( property.Relation.InverseProperty != null )
            {
                var relatedNavigationInfo = new EdmNavigationPropertyInfo() {
                    Name = property.Relation.RelatedPartyType.Name,
                    ContainsTarget = (
                        property.Relation.Kind == RelationKind.Composition && 
                        property.Relation.RelatedPartyKind == RelationPartyKind.Principal),
                    Target = _entityTypeByMetadata[property.DeclaringContract],
                    TargetMultiplicity = GetRelationSourceMultiplicity(property)
                };

                entityType.AddBidirectionalNavigation(thisNavigationInfo, relatedNavigationInfo);
            }
            else
            {
                entityType.AddUnidirectionalNavigation(thisNavigationInfo);
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
            EdmType existingEdmType;

            if ( _edmTypeByClrType.TryGetValue(clrType, out existingEdmType) )
            {
                return (EdmEnumType)existingEdmType;
            }

            var newEdmType = AddEnumType(clrType);
            _edmTypeByClrType.Add(clrType, newEdmType);
            _model.AddElement(newEdmType);

            return newEdmType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IEdmTypeReference GetEdmTypeReference(Type clrType, bool isNullable)
        {
            var primitiveTypeReference = TryGetEdmPrimitiveTypeReference(clrType, isNullable);

            if ( primitiveTypeReference != null )
            {
                return primitiveTypeReference;
            }

            if ( clrType.IsEnum )
            {
                return new EdmEnumTypeReference(GetOrAddEnumType(clrType), isNullable);
            }

            if ( clrType.IsCollectionType() )
            {
                return new EdmCollectionTypeReference(GetOrAddCollectionType(clrType), isNullable);
            }

            throw new NotSupportedException("Specified type is not supported");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EdmCollectionType GetOrAddCollectionType(Type clrType)
        {
            EdmType existingEdmType;

            if ( _edmTypeByClrType.TryGetValue(clrType, out existingEdmType) )
            {
                return (EdmCollectionType)existingEdmType;
            }

            Type elementClrType;
            clrType.IsCollectionType(out elementClrType);

            var newEdmType = new EdmCollectionType(GetEdmTypeReference(elementClrType, isNullable: false));
            _edmTypeByClrType.Add(clrType, newEdmType);

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

        private EdmMultiplicity GetRelationSourceMultiplicity(IPropertyMetadata property)
        {
            switch ( property.Relation.Multiplicity )
            {
                case RelationMultiplicity.ManyToOne:
                case RelationMultiplicity.ManyToMany:
                    return EdmMultiplicity.ZeroOrOne;
                case RelationMultiplicity.OneToOne:
                case RelationMultiplicity.OneToMany:
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
                { typeof(DateTime), EdmPrimitiveTypeKind.DateTime },
                { typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset },
                { typeof(TimeSpan), EdmPrimitiveTypeKind.Time }
                #endregion
            };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<Type, Func<bool, IEdmTypeReference>> _s_primitiveTypeReferenceByType =
            new Dictionary<Type, Func<bool, IEdmTypeReference>> { 
                #region Mappings
                { typeof(string), isNullable => EdmCoreModel.Instance.GetString(isNullable) }, 
                { typeof(bool), isNullable => EdmCoreModel.Instance.GetBoolean(isNullable) }, 
                { typeof(byte), isNullable => EdmCoreModel.Instance.GetByte(isNullable) }, 
                { typeof(decimal), isNullable => EdmCoreModel.Instance.GetDecimal(isNullable) }, 
                { typeof(double), isNullable => EdmCoreModel.Instance.GetDouble(isNullable) }, 
                { typeof(Guid), isNullable => EdmCoreModel.Instance.GetGuid(isNullable) }, 
                { typeof(short), isNullable => EdmCoreModel.Instance.GetInt16(isNullable) },
                { typeof(int), isNullable => EdmCoreModel.Instance.GetInt32(isNullable) }, 
                { typeof(long), isNullable => EdmCoreModel.Instance.GetInt64(isNullable) }, 
                { typeof(sbyte), isNullable => EdmCoreModel.Instance.GetSByte(isNullable) }, 
                { typeof(float), isNullable => EdmCoreModel.Instance.GetSingle(isNullable) }, 
                { typeof(byte[]), isNullable => EdmCoreModel.Instance.GetBinary(isNullable) }, 
                { typeof(Stream), isNullable => EdmCoreModel.Instance.GetStream(isNullable) }, 
                { typeof(DateTime), isNullable => EdmCoreModel.Instance.GetDateTime(isNullable) },
                { typeof(DateTimeOffset), isNullable => EdmCoreModel.Instance.GetDateTimeOffset(isNullable) },
                { typeof(TimeSpan), isNullable => EdmCoreModel.Instance.GetTime(isNullable) }
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

        public static IEdmTypeReference TryGetEdmPrimitiveTypeReference(Type type, bool isNullable)
        {
            Func<bool, IEdmTypeReference> typeReferenceFunc;

            if ( _s_primitiveTypeReferenceByType.TryGetValue(type, out typeReferenceFunc) )
            {
                return typeReferenceFunc(isNullable);
            }
            else
            {
                return null;
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
