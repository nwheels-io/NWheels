﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Factories;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public class BreezeMetadataBuilder
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly IEntityObjectFactory _entityObjectFactory;
        private readonly MetadataRoot _metadata;
        private readonly HashSet<Type> _includedEntities;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BreezeMetadataBuilder(ITypeMetadataCache metadataCache, IDomainObjectFactory domainObjectFactory, IEntityObjectFactory entityObjectFactory)
        {
            _metadataCache = metadataCache;
            _domainObjectFactory = domainObjectFactory;
            _entityObjectFactory = entityObjectFactory;
            _includedEntities = new HashSet<Type>();
            _metadata = new MetadataRoot {
                MetadataVersion = "1.0.5",
                LocalQueryComparisonOptions = "caseInsensitiveSQL",
                DataServices = new List<DataService>(),
                StructuralTypes = new List<StructuralType>(),
                ResourceEntityTypeMap = new Dictionary<string, string>()
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddDataService(string relativeUrlPath)
        {
            _metadata.DataServices.Add(new DataService {
                ServiceName = relativeUrlPath,
                HasServerMetadata = true,
                JsonResultsAdapter = "webApi_default",
                UseJsonp = false
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddEntity(Type contractType)
        {
            if ( !_includedEntities.Add(contractType) )
            {
                return;
            }

            var typeMetadata = _metadataCache.GetTypeMetadata(contractType);
            var implementationType = GetEntityImplementationType(typeMetadata);
            var isComplexType = ShouldTreatAsComplexType(typeMetadata);

            var structuralType = CreateStructuralType(implementationType, isComplexType);

            AddDataProperties(typeMetadata, structuralType);
            AddNavigationProperties(typeMetadata, isComplexType, structuralType);

            _metadata.StructuralTypes.Add(structuralType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetMetadataJsonString()
        {
            return JsonConvert.SerializeObject(
                _metadata, 
                Newtonsoft.Json.Formatting.Indented, 
                new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool ShouldTreatAsComplexType(ITypeMetadata metaType)
        {
            if ( metaType.IsEntityPart )
            {
                return true;
            }

            if ( metaType.Name != "AttributeValue" )
            {
                return false;
            }

            var incomingRelationProperties = _metadataCache.GetIncomingRelations(metaType).Where(p => p.DeclaringContract.IsEntity).ToArray();
            
            if ( incomingRelationProperties.Length == 0 )
            {
                return false;
            }

            return incomingRelationProperties.All(p => p.Relation.Kind == RelationKind.Composition);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private StructuralType CreateStructuralType(Type implementationType, bool isComplexType)
        {
            StructuralType structuralType = new StructuralType {
                ShortName = implementationType.Name,
                Namespace = implementationType.Namespace,
                IsComplexType = isComplexType,
                DataProperties = new List<DataProperty>()
            };

            if ( !isComplexType )
            {
                structuralType.AutoGeneratedKeyType = "KeyGenerator";
                structuralType.NavigationProperties = new List<NavigationProperty>();
            }

            return structuralType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddDataProperties(ITypeMetadata typeMetadata, StructuralType structuralType)
        {
            foreach ( var property in typeMetadata.Properties.Where(p => p.Kind == PropertyKind.Scalar) )
            {
                AddDataProperty(typeMetadata, structuralType, property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddNavigationProperties(ITypeMetadata typeMetadata, bool isComplexType, StructuralType structuralType)
        {
            foreach ( var property in typeMetadata.Properties.Where(p => p.Kind == PropertyKind.Relation) )
            {
                if ( ShouldTreatAsComplexType(property.Relation.RelatedPartyType) )
                {
                    AddDataProperty(typeMetadata, structuralType, property);
                }
                else if ( !isComplexType )
                {
                    AddNavigationProperty(typeMetadata, structuralType, property);
                }
                else
                {
                    throw new NotSupportedException("BreezeJS does not allow complext types to have navigation properties: " + property.ContractQualifiedName);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddDataProperty(ITypeMetadata typeMetadata, StructuralType structuralType, IPropertyMetadata property)
        {
            structuralType.DataProperties.Add(new DataProperty {
                Name = property.Name,
                DataType = GetBreezeDataTypeName(property.ClrType),
                IsScalar = !property.IsCollection,
                MaxLength = (PropertyHasMaxLength(property) ? property.Validation.MaxLength : null),
                IsNullable = IsNullableProperty(property),
                DefaultValue = property.DefaultValue,
                IsPartOfKey = (!structuralType.IsComplexType && property.Role == PropertyRole.Key),
                EnumType = (property.ClrType.IsEnum ? property.ClrType.FullName : null)
            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddNavigationProperty(ITypeMetadata typeMetadata, StructuralType structuralType, IPropertyMetadata property)
        {
            var navigationProperty = new NavigationProperty {
                Name = property.Name,
                AssociationName = GetAssociationName(property),
                EntityTypeName = GetQualifiedStructuralTypeName(property.Relation.RelatedPartyType),
                IsScalar = property.Relation.Multiplicity.IsIn(RelationMultiplicity.OneToOne, RelationMultiplicity.ManyToOne)
            };

            if ( !property.IsCollection )
            {
                var relatedPartyKey = (property.Relation.RelatedPartyKey ?? property.Relation.RelatedPartyType.PrimaryKey);

                var foreignKeyProperty = new DataProperty {
                    Name = JsonSerializationUtility.GetForeignKeyPropertyName(property.Name),
                    DataType = GetBreezeDataTypeName(relatedPartyKey.Properties[0].ClrType),
                    IsScalar = true,
                    IsNullable = IsNullableProperty(property)
                };

                navigationProperty.ForeignKeyNames = new List<string> { foreignKeyProperty.Name };
                structuralType.DataProperties.Add(foreignKeyProperty);
            }
            else if ( property.Relation.InverseProperty != null && !property.Relation.InverseProperty.IsCollection )
            {
                navigationProperty.InvForeignKeyNames = new List<string> {
                    JsonSerializationUtility.GetForeignKeyPropertyName(property.Relation.InverseProperty.Name)
                };
            }

            structuralType.NavigationProperties.Add(navigationProperty);
            AddEntity(property.Relation.RelatedPartyType.ContractType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetQualifiedStructuralTypeName(ITypeMetadata type)
        {
            var implementationType = GetEntityImplementationType(type);
            return GetQualifiedTypeString(implementationType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Type GetEntityImplementationType(ITypeMetadata type)
        {
            var domainObjectType = _domainObjectFactory.GetOrBuildDomainObjectType(type.ContractType, _entityObjectFactory.GetType());
            return domainObjectType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetAssociationName(IPropertyMetadata property)
        {
            if ( property.Relation.ThisPartyKind == RelationPartyKind.Dependent || property.Relation.InverseProperty == null )
            {
                return string.Format(
                    "{0}_{1}_{2}{3}",
                    property.DeclaringContract.Name,
                    property.Name,
                    property.Relation.RelatedPartyType.Name,
                    property.Relation.InverseProperty != null ? "_" + property.Relation.InverseProperty.Name : "");
            }
            else
            {
                return string.Format(
                    "{0}_{1}_{2}_{3}",
                    property.Relation.RelatedPartyType.Name,
                    property.Relation.InverseProperty.Name,
                    property.DeclaringContract.Name,
                    property.Name);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool PropertyHasMaxLength(IPropertyMetadata property)
        {
            return (
                property.ClrType == typeof(string) && 
                property.Validation != null && property.Validation.MaxLength.HasValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool IsNullableProperty(IPropertyMetadata property)
        {
            return !(
                property.Validation.IsRequired ||
                (property.ClrType.IsValueType && !property.ClrType.IsNullableValueType()));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string GetBreezeDataTypeName(Type type)
        {
            Type collectionElementType;

            if ( type.IsCollectionType(out collectionElementType) )
            {
                return GetBreezeDataTypeName(collectionElementType);
            }
            else if ( type.IsNullableValueType() )
            {
                return GetBreezeDataTypeName(type.GetGenericArguments()[0]);
            }
            else if ( type == typeof(TimeSpan) )
            {
                return "Time";
            }
            else
            {
                return type.Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetQualifiedTypeString(Type type)
        {
            return type.Name + ":#" + type.Namespace;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MetadataRoot
        {
            public string MetadataVersion { get; set; }
            public string NamingConvention { get; set; }
            public string LocalQueryComparisonOptions { get; set; }
            public List<DataService> DataServices { get; set; }
            public List<StructuralType> StructuralTypes { get; set; }
            public Dictionary<string, string> ResourceEntityTypeMap { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DataService
        {
            public string ServiceName { get; set; }
            public bool HasServerMetadata { get; set; }
            public string JsonResultsAdapter { get; set; }
            public bool UseJsonp { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StructuralType
        {
            public string ShortName { get; set; }
            public string Namespace { get; set; }
            public bool IsComplexType { get; set; }
            public List<DataProperty> DataProperties { get; set; }
            public string AutoGeneratedKeyType { get; set; }
            public string DefaultResourceName { get; set; }
            public List<NavigationProperty> NavigationProperties { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DataProperty
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public bool IsScalar { get; set; }
            public int? MaxLength { get; set; }
            public List<Validator> Validators { get; set; }
            public bool IsNullable { get; set; }
            public object DefaultValue { get; set; }
            public bool IsPartOfKey { get; set; }
            public string ComplexTypeName { get; set; }
            public string ConcurrencyMode { get; set; }
            public string RawTypeName { get; set; }
            public string EnumType { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NavigationProperty
        {
            public string Name { get; set; }
            public string EntityTypeName { get; set; }
            public bool IsScalar { get; set; }
            public string AssociationName { get; set; }
            public List<string> InvForeignKeyNames { get; set; }
            public List<string> ForeignKeyNames { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Validator
        {
            public int MaxLength { get; set; }
            public string Name { get; set; }
            public int? Min { get; set; }
            public long? Max { get; set; }
        }
    }
}
