using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using NWheels.DataObjects;

namespace NWheels.Core.DataObjects
{
    internal class TypeMetadataBuilderConstructor
    {
        private readonly DataObjectConventions _conventions;
        private Type _primaryContract;
        private Type[] _mixinContracts;
        private HashSet<PropertyInfo> _allProperties;
        private TypeMetadataBuilder _thisType;
        private TypeMetadataCache _cache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataBuilderConstructor(DataObjectConventions conventions)
        {
            _conventions = conventions;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ConstructMetadata(Type primaryContract, Type[] mixinContracts, TypeMetadataBuilder builder, TypeMetadataCache cache)
        {
            _mixinContracts = mixinContracts;
            _primaryContract = primaryContract;
            _thisType = builder;
            _cache = cache;
            
            _allProperties = UnionPropertiesInAllContracts(primaryContract, mixinContracts);

            builder.Name = primaryContract.Name.TrimPrefix("I");
            builder.ContractType = primaryContract;
            builder.MixinContractTypes.AddRange(mixinContracts);

            CreateProperties();
            FindPrimaryKey();
            FindRelations();

            builder.EndBuild();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CreateProperties()
        {
            foreach ( var propertyInfo in _allProperties )
            {
                _thisType.Properties.Add(CreatePropertyMetadata(propertyInfo));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FindPrimaryKey()
        {
            foreach ( var property in _thisType.Properties.Where(p => _conventions.IsKeyProperty(p)) )
            {
                if ( _thisType.PrimaryKey == null )
                {
                    _thisType.PrimaryKey = new KeyMetadataBuilder {
                        Kind = KeyKind.Primary,
                        Name = "PK_" + _thisType.Name
                    };

                    _thisType.AllKeys.Add(_thisType.PrimaryKey);
                }

                _thisType.PrimaryKey.Properties.Add(property);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void FindRelations()
        {
            foreach ( var property in _thisType.Properties )
            {
                if ( _conventions.IsToOneRelationProperty(property) )
                {
                    AddToOneRelation(property);
                }

                Type relatedContract;

                if ( _conventions.IsToManyRelationProperty(property, out relatedContract) )
                {
                    AddToManyRelation(property, relatedContract);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private void AddToOneRelation(PropertyMetadataBuilder property)
        {
            property.Kind = PropertyKind.Relation;

            var thisKey = FindOrAddForeignKey(_thisType, property);
            var relatedType = _cache.FindTypeMetadataAllowIncomplete(property.ClrType);

            property.Relation = new RelationMetadataBuilder {
                RelationKind = RelationKind.ManyToOne,
                ThisPartyKey = thisKey,
                ThisPartyKind = RelationPartyKind.Dependent,
                RelatedPartyKind = RelationPartyKind.Principal,
                RelatedPartyType = relatedType,
                RelatedPartyKey = relatedType.PrimaryKey
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddToManyRelation(PropertyMetadataBuilder property, Type relatedContract)
        {
            property.Kind = PropertyKind.Relation;

            var relatedType = _cache.FindTypeMetadataAllowIncomplete(relatedContract);
            var relatedProperty = relatedType.Properties.FirstOrDefault(p => p.ClrType == _thisType.ContractType);

            if ( relatedProperty != null )
            {
                var relatedKey = FindOrAddForeignKey(relatedType, relatedProperty);

                property.Relation = new RelationMetadataBuilder {
                    RelationKind = RelationKind.OneToMany,
                    ThisPartyKey = _thisType.PrimaryKey,
                    ThisPartyKind = RelationPartyKind.Principal,
                    RelatedPartyKind = RelationPartyKind.Dependent,
                    RelatedPartyType = relatedType,
                    RelatedPartyKey = relatedKey
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyMetadataBuilder CreatePropertyMetadata(PropertyInfo declaration)
        {
            var property = new PropertyMetadataBuilder {
                Name = declaration.Name,
                ClrType = declaration.PropertyType,
                ContractPropertyInfo = declaration
            };

            var defaultValueAttribute = declaration.GetCustomAttribute<DefaultValueAttribute>();
            property.DefaultValue = (defaultValueAttribute != null ? defaultValueAttribute.Value : null);

            return property;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static HashSet<PropertyInfo> UnionPropertiesInAllContracts(Type primaryContract, Type[] mixinContracts)
        {
            var result = new HashSet<PropertyInfo>();

            result.UnionWith(TypeMemberCache.Of(primaryContract).ImplementableProperties);

            foreach ( var contract in mixinContracts )
            {
                result.UnionWith(TypeMemberCache.Of(contract).ImplementableProperties);
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static KeyMetadataBuilder FindOrAddForeignKey(TypeMetadataBuilder type, PropertyMetadataBuilder relationProperty)
        {
            var existingKey = type.AllKeys.FirstOrDefault(k => k.Properties.SingleOrDefault() == relationProperty);

            if ( existingKey != null )
            {
                return existingKey;
            }

            var newKey = new KeyMetadataBuilder {
                Kind = KeyKind.Foreign,
                Name = "FK_" + relationProperty.Name
            };

            newKey.Properties.Add(relationProperty);
            type.AllKeys.Add(newKey);

            return newKey;
        }
    }
}
